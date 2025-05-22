import os
import zipfile
import tempfile
import time
import shutil
from urllib.parse import urlparse
import uuid
from typing import Optional

CACHE_DURATION = 7200  # seconds
GITHUB_TOKEN = os.environ.get("GITHUB_TOKEN", None)
_temp_folders: list[str] = []

def _get_temp_base() -> str:
    base = os.path.join(tempfile.gettempdir(), "pybite")
    os.makedirs(base, exist_ok=True)
    return base

def _create_temp_folder() -> str:
    folder = os.path.join(_get_temp_base(), str(uuid.uuid4()))
    os.makedirs(folder, exist_ok=True)
    _temp_folders.append(folder)
    return folder

def _cleanup_temp_folders() -> None:
    for folder in _temp_folders:
        if os.path.isdir(folder):
            shutil.rmtree(folder, ignore_errors=True)
    _temp_folders.clear()
    base = _get_temp_base()
    now = time.time()
    for fname in os.listdir(base):
        fpath = os.path.join(base, fname)
        if fname.endswith(".zip") and os.path.isfile(fpath):
            if now - os.path.getmtime(fpath) > CACHE_DURATION:
                try:
                    os.remove(fpath)
                except Exception:
                    pass

def _parse_github_url(url: str) -> tuple[str, str, str, str]:
    parts = urlparse(url)
    if parts.netloc.lower() != "github.com":
        raise ValueError(f"URL is not a github.com URL: {url}")
    path = parts.path.strip("/").split('/')
    if len(path) < 2:
        raise ValueError(f"Invalid GitHub URL: {url}")
    owner, repo = path[0], path[1].removesuffix('.git')
    branch = 'main'
    sub_path = ''
    if len(path) >= 4 and path[2] == 'tree':
        branch = path[3]
        sub_path = '/'.join(path[4:]) if len(path) > 4 else ''
    return owner, repo, branch, sub_path

def _file_url_to_path(url: str) -> str:
    """
    Convert a file:// URL to a local filesystem path.
    """
    parts = urlparse(url)
    if parts.scheme.lower() != "file":
        raise ValueError(f"Not a file URL: {url}")
    # On Windows, parts.netloc may contain a drive letter or be empty
    if os.name == "nt":
        # Handle file:///C:/path or file://localhost/C:/path
        if parts.netloc and parts.netloc != "localhost":
            path = f"//{parts.netloc}{parts.path}"
        else:
            path = parts.path
        return os.path.abspath(path.lstrip("/"))
    else:
        # On Unix, just join netloc and path
        return os.path.abspath(os.path.join("/", parts.netloc, parts.path))

def _is_local_path(path: str) -> bool:
    parts = urlparse(path)
    # Support file:// URLs as local paths
    if parts.scheme.lower() == "file":
        _file_url_to_path(path)
        return True
    if parts.scheme or parts.netloc:
        return False
    return True

def is_dir_empty(path: str) -> bool:
    """
    Return True if the directory at 'path' is empty (no files or subdirectories).
    """
    return os.path.isdir(path) and not any(os.scandir(path))

def _copy_local_folder(src: str, dest: str) -> None:
    if not os.path.isdir(src):
        raise ValueError(f"Local path does not exist or is not a directory: {src}")
    # Allow if dest does not exist or exists and is empty
    if os.path.exists(dest) and not is_dir_empty(dest):
        raise ValueError(f"Destination folder '{dest}' already exists and is not empty.")
    shutil.copytree(src, dest, dirs_exist_ok=True)

def _print_progress(filename: str, downloaded: int, total: int) -> None:
    if total:
        percent = downloaded * 100 // total
        print(f"\rDownloading {filename}: {percent}%", end="", flush=True)

def _extract_folder_from_github_zip(zip_path: str, folder_path: str, dest_dir: str, repo: str, branch: str) -> None:
    prefix = f"{repo}-{branch}/{folder_path.rstrip('/')}/"
    with zipfile.ZipFile(zip_path) as z:
        for member in z.namelist():
            if not member.startswith(prefix):
                continue
            rel = member[len(prefix):]
            if not rel:
                continue
            target = os.path.join(dest_dir, rel)
            if member.endswith('/'):
                os.makedirs(target, exist_ok=True)
            else:
                os.makedirs(os.path.dirname(target), exist_ok=True)
                with z.open(member) as src, open(target, 'wb') as dst:
                    dst.write(src.read())

def _download_github_api(owner: str, repo: str, branch: str, folder_path: str, dest_dir: str) -> None:
    import requests
    headers = {'Accept': 'application/vnd.github.v3+json'}
    if GITHUB_TOKEN:
        headers['Authorization'] = f"token {GITHUB_TOKEN}"
    def _download_dir(path: str, dest: str) -> None:
        os.makedirs(dest, exist_ok=True)
        api_url = f"https://api.github.com/repos/{owner}/{repo}/contents/{path}?ref={branch}" if path else f"https://api.github.com/repos/{owner}/{repo}/contents?ref={branch}"
        r = requests.get(api_url, headers=headers)
        r.raise_for_status()
        items = r.json()
        for item in items:
            if item['type'] == 'file':
                file_dest = os.path.join(dest, item['name'])
                print(f"Downloading file {item['path']}")
                download_file(item['download_url'], file_dest)
            elif item['type'] == 'dir':
                _download_dir(item['path'], os.path.join(dest, item['name']))
    _download_dir(folder_path, dest_dir)

def _download_github_zip(owner: str, repo: str, branch: str, folder_path: str, dest_dir: str) -> None:
    zip_url = f"https://github.com/{owner}/{repo}/archive/{branch}.zip"
    base = _get_temp_base()
    cache_name = f"github_{owner}_{repo}_{branch}.zip"
    cache_path = os.path.join(base, cache_name)
    if os.path.exists(cache_path) and (time.time() - os.path.getmtime(cache_path)) < CACHE_DURATION:
        print(f"Using cached file at {cache_path}")
    else:
        print(f"Downloading file from {zip_url}")
        download_file(zip_url, cache_path)
    print(f"Extracting '{folder_path}' into '{dest_dir}'")
    _extract_folder_from_github_zip(cache_path, folder_path, dest_dir, repo, branch)

def _download_github_folder(url: str, dest_dir: str) -> None:
    if os.path.exists(dest_dir) and not is_dir_empty(dest_dir):
        raise ValueError(f"Destination folder '{dest_dir}' is not empty.")
    owner, repo, branch, folder_path = _parse_github_url(url)
    try:
        import requests  # noqa
        _download_github_api(owner, repo, branch, folder_path, dest_dir)
    except ImportError:
        print("requests library not found, downloading the zip file instead.")
        _download_github_zip(owner, repo, branch, folder_path, dest_dir)
    print("Download complete.")

def _extract_zip(zip_path: str, dest_dir: str) -> None:
    with zipfile.ZipFile(zip_path, 'r') as zip_ref:
        zip_ref.extractall(dest_dir)

def download_file(url: str, dest_path: str, show_progress: bool = True) -> None:
    """
    Download a file from a URL to a local path.

    Args:
        url: The URL to download from.
        dest_path: The local file path to write to.
        show_progress: If True, print download progress.
    Raises:
        Exception: If download fails.
    """
    try:
        import requests
        resp = requests.get(url, stream=True)
        resp.raise_for_status()
        total = int(resp.headers.get('content-length', 0))
        downloaded = 0
        with open(dest_path, 'wb') as f:
            for chunk in resp.iter_content(chunk_size=8192):
                if not chunk:
                    continue
                f.write(chunk)
                if show_progress and total:
                    downloaded += len(chunk)
                    _print_progress(os.path.basename(dest_path), downloaded, total)
        if show_progress and total:
            print()
    except ImportError:
        from urllib.request import Request, urlopen
        req = Request(url, headers={"User-Agent": "python-urllib"})
        with urlopen(req) as resp:
            total = resp.length or 0
            downloaded = 0
            with open(dest_path, 'wb') as f:
                while True:
                    chunk = resp.read(8192)
                    if not chunk:
                        break
                    f.write(chunk)
                    if show_progress and total:
                        downloaded += len(chunk)
                        _print_progress(os.path.basename(dest_path), downloaded, total)
            if show_progress and total:
                print()

def is_url(path: str) -> bool:
    """
    Return True if the given path is a URL (http, https, file, etc).
    """
    parts = urlparse(path)
    return bool(parts.scheme)

def download_folder(src: str, dest_dir: str) -> None:
    """
    Download or copy a folder from a GitHub URL or local directory to a destination directory.

    Args:
        src: Source GitHub URL or local directory path.
        dest_dir: Destination directory path.
    Raises:
        ValueError: If the destination exists and is not empty, or if the source is invalid.
    """
    if os.path.exists(dest_dir) and not is_dir_empty(dest_dir):
        raise ValueError(f"Destination folder '{dest_dir}' is not empty.")
    if _is_local_path(src):
        if is_url(src):
            src = _file_url_to_path(src)
        if os.path.isdir(src):
            _copy_local_folder(src, dest_dir)
        elif os.path.isfile(src):
            _extract_zip(src, dest_dir)
    else:
        _download_github_folder(src, dest_dir)
