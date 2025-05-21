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
    if url.startswith("github://") or url.startswith("gh://"):
        url = "https://github.com/" + url.split("://", 1)[1]
    parts = urlparse(url)
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

def _is_local_path(path: str) -> bool:
    parts = urlparse(path)
    if parts.scheme or parts.netloc:
        return False
    if os.path.isfile(path):
        raise ValueError(f"Local path is a file, not a directory: {path}")
    return True

def _copy_local_folder(src: str, dest: str) -> None:
    if not os.path.isdir(src):
        raise ValueError(f"Local path does not exist or is not a directory: {src}")
    if os.path.exists(dest) and os.listdir(dest):
        raise ValueError(f"Destination folder '{dest}' is not empty.")
    if not os.path.exists(dest):
        shutil.copytree(src, dest)
    else:
        for root, dirs, files in os.walk(src):
            rel = os.path.relpath(root, src)
            target_root = os.path.join(dest, rel) if rel != '.' else dest
            os.makedirs(target_root, exist_ok=True)
            for f in files:
                shutil.copy2(os.path.join(root, f), os.path.join(target_root, f))

def _print_progress(filename: str, downloaded: int, total: int) -> None:
    if total:
        percent = downloaded * 100 // total
        print(f"\rDownloading {filename}: {percent}%", end="", flush=True)

def _extract_folder_from_zip(zip_path: str, folder_path: str, dest_dir: str, repo: str, branch: str) -> None:
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
    _extract_folder_from_zip(cache_path, folder_path, dest_dir, repo, branch)

def _download_github_folder(url: str, dest_dir: str) -> None:
    if os.path.exists(dest_dir) and os.listdir(dest_dir):
        raise ValueError(f"Destination folder '{dest_dir}' is not empty.")
    owner, repo, branch, folder_path = _parse_github_url(url)
    try:
        import requests  # noqa
        _download_github_api(owner, repo, branch, folder_path, dest_dir)
    except ImportError:
        print("requests library not found, downloading the zip file instead.")
        _download_github_zip(owner, repo, branch, folder_path, dest_dir)
    print("Download complete.")

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

def download_folder(src: str, dest_dir: str) -> None:
    """
    Download or copy a folder from a GitHub URL or local directory to a destination directory.

    Args:
        src: Source GitHub URL or local directory path.
        dest_dir: Destination directory path.
    Raises:
        ValueError: If the destination exists and is not empty, or if the source is invalid.
    """
    if os.path.exists(dest_dir) and os.listdir(dest_dir):
        raise ValueError(f"Destination folder '{dest_dir}' is not empty.")
    if _is_local_path(src):
        _copy_local_folder(src, dest_dir)
    else:
        _download_github_folder(src, dest_dir)
