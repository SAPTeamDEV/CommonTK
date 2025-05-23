import os
import re
import zipfile
import tempfile
import time
import shutil
import uuid
import multiprocessing
from urllib.parse import urlparse
from typing import Optional
from .globals import Globals

CACHE_DURATION = 7200
GITHUB_TOKEN = os.environ.get("GITHUB_TOKEN", None)
GH_URL_REGEX = re.compile(r"^gh:([^/]+)/([^/]+)(?:/([^/]+))?(?:/(.*))?$")
_temp_folders: list[str] = []
_has_requests_lib_result: Optional[bool] = None

def _is_url(path: str) -> bool:
    """
    Internal: Returns True if the given path is a URL (http, https, file, etc).
    """
    if os.name == "nt":
        if re.match(r"^[a-zA-Z]:[\\/]", path) or path.startswith("\\\\"):
            return False
    if os.name != "nt" and path.startswith("/"):
        return False
    return bool(urlparse(path).scheme)

def _is_local_path(path: str) -> bool:
    """
    Internal: Returns True if the path is a local filesystem path (absolute or relative).
    """
    if path.startswith("file://"):
        return True
    if os.name == "nt":
        if path.startswith("\\\\") or re.match(r"^[a-zA-Z]:[\\/]", path):
            return True
    if os.name != "nt" and path.startswith("/"):
        return True
    parts = urlparse(path)
    return not parts.scheme and not parts.netloc

def _file_url_to_path(url: str) -> str:
    """
    Internal: Converts a file:// URL to a local filesystem path.
    Raises ValueError if the URL is not a file URL.
    """
    parts = urlparse(url)
    if parts.scheme.lower() != "file":
        raise ValueError(f"Not a file URL: {url}")
    if os.name == "nt":
        if parts.netloc and parts.netloc != "localhost":
            path = f"//{parts.netloc}{parts.path}"
        else:
            path = parts.path
        if not re.match(r"^/[a-zA-Z]:/", path):
            raise ValueError(f"Not a valid file URL: {url}")
        return os.path.abspath(path.lstrip("/"))
    else:
        return os.path.abspath(os.path.join("/", parts.netloc, parts.path))

def _is_dir_empty(path: str) -> bool:
    """
    Internal: Returns True if the directory at 'path' is empty.
    """
    return os.path.isdir(path) and not any(os.scandir(path))

def _copy_local_folder(src: str, dest: str) -> None:
    """
    Internal: Copies a local folder from src to dest, raising if dest is not empty.
    """
    print(f"Copying local folder from '{src}' to '{dest}'")
    if not os.path.isdir(src):
        raise ValueError(f"Local path does not exist or is not a directory: {src}")
    if os.path.exists(dest) and not _is_dir_empty(dest):
        raise ValueError(f"Destination folder '{dest}' already exists and is not empty.")
    shutil.copytree(src, dest, dirs_exist_ok=True)

def _extract_zip(zip_path: str, dest_dir: str) -> None:
    """
    Internal: Extracts a zip file to the destination directory.
    """
    print(f"Extracting '{zip_path}' into '{dest_dir}'")
    with zipfile.ZipFile(zip_path, 'r') as zip_ref:
        zip_ref.extractall(dest_dir)

def _print_progress(filename: str, downloaded: int, total: int) -> None:
    """
    Internal: Prints download progress for a file.
    """
    if total:
        percent = min(100, downloaded * 100 // total)
        print(f"\rDownloading {filename}: {percent}%", end="", flush=True)

def get_temp_base() -> str:
    """
    Returns the base directory for temporary folders used by pybite.
    """
    base = os.path.join(tempfile.gettempdir(), "pybite")
    os.makedirs(base, exist_ok=True)
    return base

def create_temp_folder() -> str:
    """
    Creates and returns a new temporary folder for downloads.
    """
    folder = os.path.join(get_temp_base(), str(uuid.uuid4()))
    os.makedirs(folder, exist_ok=True)
    _temp_folders.append(folder)
    return folder

def cleanup_temp_folders() -> None:
    """
    Deletes all tracked temporary folders and cleans up old cached zip files.
    """
    for folder in _temp_folders:
        if os.path.isdir(folder):
            shutil.rmtree(folder, ignore_errors=True)
    _temp_folders.clear()
    base = get_temp_base()
    now = time.time()
    for fname in os.listdir(base):
        fpath = os.path.join(base, fname)
        if fname.endswith(".zip") and os.path.isfile(fpath):
            if now - os.path.getmtime(fpath) > CACHE_DURATION:
                try: os.remove(fpath)
                except Exception: pass

def _parse_github_url(url: str):
    """
    Internal: Parses a GitHub URL or gh: shortcut and returns (owner, repo, branch, sub_path).
    Raises ValueError if the URL is invalid.
    """
    if url.startswith("gh:"):
        m = GH_URL_REGEX.match(url)
        if not m:
            raise ValueError(f"Invalid gh: URL: {url}")
        owner, repo, branch, sub_path = m.group(1), m.group(2), m.group(3) or "main", m.group(4) or ""
        return owner, repo, branch, sub_path
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

def _has_requests_lib() -> bool:
    """
    Internal: Returns True if the 'requests' library is available, False otherwise. Caches the result.
    """
    global _has_requests_lib_result
    if _has_requests_lib_result is None:
        try:
            import requests
            _has_requests_lib_result = True
        except ImportError:
            _has_requests_lib_result = False
    return _has_requests_lib_result

def _build_github_auth_header(token: str) -> str:
    """
    Internal: Returns the correct Authorization header value for a GitHub token.
    """
    token = token.strip()
    if re.fullmatch(r"[0-9a-f]{40}", token):
        return f"token {token}"
    if re.fullmatch(r"[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+", token):
        return f"Bearer {token}"
    if token.startswith("v1.") or "." in token:
        return f"Bearer {token}"
    return f"token {token}"

def _github_api_request(url: str, headers: Optional[dict] = None, token: Optional[str] = None) -> dict:
    """
    Internal: Performs a GitHub API GET request and returns the parsed JSON response.
    Raises ImportError if requests is not available.
    """
    if not _has_requests_lib():
        raise ImportError("The 'requests' library is required for GitHub API requests.")
    import requests
    final_headers = {'Accept': 'application/vnd.github.v3+json'}
    github_api = "https://api.github.com"
    if url.startswith("/"):
        url = github_api + url
    if token is None:
        token = GITHUB_TOKEN
    if token:
        final_headers['Authorization'] = _build_github_auth_header(token)
    if headers:
        final_headers.update(headers)
    resp = requests.get(url, headers=final_headers)
    resp.raise_for_status()
    return resp.json()

def _get_github_api_limit() -> int:
    """
    Internal: Returns the current GitHub API rate limit (remaining requests).
    Returns 0 if requests is not available.
    """
    if not _has_requests_lib():
        return 0
    result = _github_api_request("/rate_limit")
    if 'rate' in result and 'remaining' in result['rate']:
        return result['rate']['remaining']
    return 0

def _extract_folder_from_github_zip(zip_path: str, folder_path: str, dest_dir: str, repo: str, branch: str) -> None:
    """
    Internal: Extracts a specific folder from a GitHub repository zip archive into dest_dir.
    """
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
                if Globals.VERBOSE:
                    print(f"Extracting {member} to {target}")
                os.makedirs(os.path.dirname(target), exist_ok=True)
                with z.open(member) as src, open(target, 'wb') as dst:
                    dst.write(src.read())

def _download_github_api(owner: str, repo: str, branch: str, folder_path: str, dest_dir: str) -> None:
    """
    Internal: Downloads a GitHub folder using the GitHub API and saves it to dest_dir.
    Uses a thread pool for parallel downloads.
    """
    max_workers = multiprocessing.cpu_count()
    import concurrent.futures
    threadpool = concurrent.futures.ThreadPoolExecutor(max_workers=max_workers)
    downloaded = []
    requested = 0

    def _download_dir(path: str, dest: str) -> None:
        """
        Internal: Recursively downloads all files and subdirectories from a GitHub API path.
        """
        os.makedirs(dest, exist_ok=True)
        api_url = f"/repos/{owner}/{repo}/contents/{path}?ref={branch}" if path else f"/repos/{owner}/{repo}/contents?ref={branch}"
        if Globals.VERBOSE:
            print(f"Getting informations from: {api_url}")
        items = _github_api_request(api_url)
        for item in items:
            if item['type'] == 'file':
                nonlocal requested
                threadpool.submit(_download_file_from_github, dest, item)
                requested += 1
            elif item['type'] == 'dir':
                _download_dir(item['path'], os.path.join(dest, item['name']))

    def _download_file_from_github(dest, item):
        """
        Internal: Downloads a single file from GitHub and saves it to dest.
        """
        file_dest = os.path.join(dest, item['name'])
        download_file(item['download_url'], file_dest, show_progress=False)
        if Globals.VERBOSE:
            print(f"Downloaded {item['path']}")
        downloaded.append(file_dest)

    _download_dir(folder_path, dest_dir)
    threadpool.shutdown(wait=True)
    if Globals.VERBOSE:
        print(f"Downloaded {len(downloaded)} of {requested} files.")
    if requested > 0 and len(downloaded) != requested:
        raise Exception("Partial download")

def _download_github_zip(owner: str, repo: str, branch: str, folder_path: str, dest_dir: str) -> None:
    """
    Internal: Downloads a GitHub repository as a zip file and extracts the specified folder to dest_dir.
    Uses a cache for repeated downloads.
    """
    zip_url = f"https://github.com/{owner}/{repo}/archive/{branch}.zip"
    base = get_temp_base()
    cache_name = f"github_{owner}_{repo}_{branch}.zip"
    cache_path = os.path.join(base, cache_name)
    if os.path.exists(cache_path) and (time.time() - os.path.getmtime(cache_path)) < CACHE_DURATION:
        if Globals.VERBOSE:
            print(f"Using cached file at {cache_path}")
    else:
        if Globals.VERBOSE:
            print(f"Downloading file from {zip_url}")
        download_file(zip_url, cache_path)
    if Globals.VERBOSE:
        print(f"Extracting '{folder_path}' into '{dest_dir}'")
    _extract_folder_from_github_zip(cache_path, folder_path, dest_dir, repo, branch)

def _download_github_folder(url: str, dest_dir: str) -> None:
    """
    Internal: Downloads a folder from GitHub (API or zip fallback) and saves it to dest_dir.
    """
    if not Globals.VERBOSE:
        print(f"Downloading from GitHub URL: {url}")
    if os.path.exists(dest_dir) and not _is_dir_empty(dest_dir):
        raise ValueError(f"Destination folder '{dest_dir}' is not empty.")
    owner, repo, branch, folder_path = _parse_github_url(url)
    if _has_requests_lib and (rate := _get_github_api_limit()) > 10:
        if Globals.VERBOSE:
            print(f"Downloading using GitHub API (remaining rate limit: {rate})...")
        _download_github_api(owner, repo, branch, folder_path, dest_dir)
    else:
        if Globals.VERBOSE:
            print("Downloading repository zip file...")
        _download_github_zip(owner, repo, branch, folder_path, dest_dir)
    print("Download completed successfully")

def download_file(url: str, dest_path: str, show_progress: bool = True) -> None:
    """
    Downloads a file from a URL to a local path.
    Uses requests if available, otherwise falls back to urllib.
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
    Downloads or copies a folder from a GitHub URL, local directory, or zip file to a destination directory.
    Raises ValueError if the destination is not empty.
    """
    if os.path.exists(dest_dir) and not _is_dir_empty(dest_dir):
        raise ValueError(f"Destination folder '{dest_dir}' is not empty.")
    if _is_local_path(src):
        if _is_url(src):
            src = _file_url_to_path(src)
        if os.path.isdir(src):
            _copy_local_folder(src, dest_dir)
        elif os.path.isfile(src):
            _extract_zip(src, dest_dir)
        else:
            raise ValueError(f"File or directory does not exist: {src}")
    else:
        _download_github_folder(src, dest_dir)
