import os
import json
import shutil
from typing import Optional, Dict, Any, List
from . import download
from .globals import Globals

class Module:
    """
    Represents a Bite module, which is a directory containing a module.json file.
    Provides access to module metadata and validation.
    """
    def __init__(self, path: str, require_json: bool = True):
        """
        Initialize a Module by loading its module.json file.

        :param path: Path to the module directory.
        :param require_json: If True, raises if module.json is missing or invalid.
        """
        self.path = path
        self.id = os.path.basename(path)
        self.name = None
        self.version = None
        self.description = None
        self.author = None
        self.private = False
        self.update_url = None
        self.module_info = None
        try:
            self._load_json()
        except Exception:
            if require_json:
                raise

    def _load_json(self):
        """
        Internal: Loads and parses the module.json file, extracting all metadata fields.
        Raises FileNotFoundError if the file does not exist.
        """
        json_path = os.path.join(self.path, 'module.json')
        if not os.path.exists(json_path):
            raise FileNotFoundError(f"JSON file not found at {json_path}")
        with open(json_path, 'r') as f:
            data = json.load(f)
        self.id = data.get('id', self.id)
        self.name = data.get('name')
        self.version = data.get('version')
        self.description = data.get('description')
        self.author = data.get('author')
        self.private = data.get('private', False)
        self.update_url = data.get('update_url')
        self.module_info = data

    @property
    def files(self) -> Dict[str, str]:
        """
        Returns a dictionary mapping file names to their absolute paths for all files in the module directory.
        """
        return {f: os.path.join(self.path, f) for f in os.listdir(self.path) if os.path.isfile(os.path.join(self.path, f))}

    @property
    def valid(self) -> bool:
        """
        Returns True if the module has a valid structure (no subdirectories except __pycache__).
        """
        if self.module_info is None:
            return False
        folders = [f for f in os.listdir(self.path) if os.path.isdir(os.path.join(self.path, f)) and f != '__pycache__']
        return not folders

    @property
    def updatable(self):
        """
        Returns True if the module is valid, not private, and has an update_url.
        """
        return self.valid and self.update_url and not self.private

    @property
    def current(self):
        """
        Returns the module identifier, including version if available.
        """
        return f"{self.id}-{self.version}" if self.version else self.id

    def update_info(self):
        """
        Updates the module.json file with the current metadata fields.
        """
        json_path = os.path.join(self.path, 'module.json')
        data = {}
        if os.path.exists(json_path):
            with open(json_path, 'r') as f:
                data = json.load(f)
        data.update({
            'name': self.name,
            'version': self.version,
            'description': self.description,
            'author': self.author,
            'private': self.private,
            'update_url': self.update_url
        })
        with open(json_path, 'w') as f:
            json.dump(data, f, indent=4)

def install(url: str, modules_dir: str, upgrade: bool = False, override: bool = False) -> None:
    """
    Installs one or more modules from a URL or local path into the modules directory.
    Handles upgrades and overrides as specified.

    :param url: Source URL or path for the module(s).
    :param modules_dir: Destination directory for modules.
    :param upgrade: If True, upgrades existing modules if newer.
    :param override: If True, forcibly replaces existing modules.
    """
    temp_dir = download.create_temp_folder()
    download.download_folder(url, temp_dir)
    modules: List[Module] = []

    def find_modules(path: str):
        """
        Internal: Recursively search for valid modules (directories with module.json) in the given path.
        Appends found modules to the 'modules' list.
        """
        mj = os.path.join(path, 'module.json')
        if os.path.exists(mj):
            m = Module(path, require_json=False)
            if m.valid and not m.private:
                modules.append(m)
                if Globals.VERBOSE: print(f"Found module: {m.current}")
            elif m.private and Globals.VERBOSE:
                print(f"Skipping private module: {m.id}")
            elif Globals.VERBOSE:
                print(f"Skipping invalid module: {m.id}")
            return
        for entry in os.listdir(path):
            if entry == '__pycache__': continue
            entry_path = os.path.join(path, entry)
            if os.path.isdir(entry_path):
                find_modules(entry_path)

    find_modules(temp_dir)
    ids = [m.id for m in modules]
    dups = {i for i in ids if ids.count(i) > 1}
    if dups:
        raise ValueError(f"Duplicate module IDs found: {', '.join(dups)}")

    for m in modules:
        mod_path = os.path.join(modules_dir, m.id)
        if os.path.isdir(mod_path):
            if not override and not upgrade:
                print(f"Module {m.id} already exists.")
                continue
            old = Module(mod_path, require_json=False)
            if not override and not old.valid:
                print(f"Can't update module {m.id}, invalid structure.")
                continue
            if not override and old.version and m.version:
                def parse(v): return tuple(map(int, v.split('.')))
                try:
                    if parse(m.version) <= parse(old.version):
                        print(f"Latest version of module {old.current} is already installed. Skipping update.")
                        continue
                except Exception as e:
                    print(f"Error comparing versions: {e}. Skipping update.")
                    continue
            uninstall(old, force=override)
        print(f"Installing module {m.current}...")
        m.update_url = url
        m.update_info()
        os.makedirs(mod_path, exist_ok=True)
        for f, src in m.files.items():
            dest = os.path.join(mod_path, f)
            if Globals.VERBOSE:
                print(f"Copying {src} to {dest}")
            shutil.copy2(src, dest)
        print(f"Module {m.current} installed successfully.")
    if not modules:
        print(f"No valid modules found in {temp_dir}.")

def uninstall(module: Module, force: bool = False) -> None:
    """
    Uninstalls the specified module by deleting its directory.

    :param module: The Module instance to uninstall.
    :param force: If True, uninstall even if the module is invalid.
    :raises ValueError: If the module is invalid and force is False, or if the path does not exist.
    """
    print(f"Uninstalling module {module.current}...")
    if not force and not module.valid:
        raise ValueError(f"Module {module.current} is not valid.")
    if not os.path.exists(module.path):
        raise ValueError(f"Module path does not exist: {module.path}")
    shutil.rmtree(module.path)
    print(f"Module {module.current} uninstalled successfully.")
