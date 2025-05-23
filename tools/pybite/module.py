# write a class named module that accepts a path to a directory and then opens a json file and stores parsed object and extracts name and description
import os
import json
import shutil
from typing import Optional, Dict, Any
from urllib.parse import urlparse

from . import download

class Module:
    """
    A class representing a module in the Bite build engine.
    It loads a JSON file from the specified directory and extracts its information.
    """
    def __init__(self, path: str, require_json: bool = True) -> None:
        self.path = path
        self.id: str = os.path.basename(path)
        self.name: Optional[str] = None
        self.version: Optional[str] = None
        self.description: Optional[str] = None
        self.author: Optional[str] = None
        self.private: bool = False
        self.update_url: Optional[str] = None
        self.module_info: Optional[Dict[str, Any]] = None
        
        try:
            self._load_json()
        except:
            if require_json:
                raise

    def _load_json(self) -> None:
        """
        Load the JSON file from the specified path and extract name and description.
        """
        json_file_path = os.path.join(self.path, 'module.json')
        if not os.path.exists(json_file_path):
            raise FileNotFoundError(f"JSON file not found at {json_file_path}")

        with open(json_file_path, 'r') as json_file:
            data: Dict[str, Any] = json.load(json_file)
            if data.get('id', None) is not None:
                self.id = data['id']
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
        Get a dictionary of files in the module directory.
        The keys are the file names and the values are their paths.
        """
        return {f: os.path.join(self.path, f) for f in os.listdir(self.path) if os.path.isfile(os.path.join(self.path, f))}

    @property
    def valid(self) -> bool:
        """
        Check if the module structure is valid.
        A valid module should contain a 'module.json' file and no subdirectories.
        """
        if self.module_info is None:
            return False
        folders = {f: os.path.join(self.path, f) for f in os.listdir(self.path) if os.path.isdir(os.path.join(self.path, f)) and f != '__pycache__'}
        return len(folders) == 0
    
    @property
    def updatable(self):
        return self.valid and self.update_url is not None and not self.private
    
    @property
    def current(self):
        """Get the current module identifier, including version if available."""
        res = self.id
        if self.version:
            res += f"-{self.version}"
        return res
    
    def update_info(self):
        json_file_path = os.path.join(self.path, 'module.json')
        
        if not os.path.exists(json_file_path):
            data: Dict[str, Any] = {}
        else:
            with open(json_file_path, 'r') as json_file:
                data: Dict[str, Any] = json.load(json_file)
        
        data['name'] = self.name
        data['version'] = self.version
        data['description'] = self.description
        data['author'] = self.author
        data['private'] = self.private
        data['update_url'] = self.update_url

        with open(json_file_path, 'w') as json_file:
            json.dump(data, json_file, indent=4)

def install(url: str, modules_dir: str, upgrade: bool = False, override: bool = False) -> None:
    """
    Install module from a URL to the specified modules directory.
    The URL can be a GitHub repository or a local path.
    """
    temp_dir = download.create_temp_folder()
    
    download.download_folder(url, temp_dir)
    
    # detect directory structure
    modules: list[Module] = []

    def _get_modules(path: str) -> None:
        # Recursively crawl directories to find modules
        mj = os.path.join(path, 'module.json')
        if os.path.exists(mj):
            module = Module(path, require_json=False)
            if module.valid and not module.private:
                modules.append(module)
                print(f"Found module: {module.current}")
            elif module.private:
                print(f"Skipping private module: {module.id}")
            else:
                print(f"Skipping invalid module: {module.id}")
            return  # Stop recursion if module.json found at this level

        for entry in os.listdir(path):
            if entry == '__pycache__':
                continue
            entry_path = os.path.join(path, entry)
            if os.path.isdir(entry_path):
                _get_modules(entry_path)

    print(f"Searching for modules in {temp_dir}...")
    _get_modules(temp_dir)
    
    # detect modules with the same id
    module_ids = [module.id for module in modules]
    duplicate_ids = {id for id in module_ids if module_ids.count(id) > 1}
    if duplicate_ids:
        print(f"Found duplicate module IDs: {duplicate_ids}")

    for module in modules:
        module_path = os.path.join(modules_dir, module.id)
        if os.path.isdir(module_path):
            if not override and not upgrade:
                print(f"Module {module.id} already exists.")
                continue
            old_module = Module(module_path, require_json=False)
            if not override and not old_module.valid:
                print(f"Can't update module {module.id}, invalid structure.")
                continue
            # Compare versions and skip update if not newer
            if not override and old_module.version:
                if not module.version:
                    print(f"Module {module.id} has no version specified. Skipping update.")
                    continue
                
                def parse_version(v):
                    return tuple(int(x) for x in v.split('.'))
                try:
                    if parse_version(module.version) <= parse_version(old_module.version):
                        print(f"Latest version of module {old_module.current} is already installed. Skipping update.")
                        continue
                except Exception as e:
                    print(f"Error comparing versions: {e}. Skipping update.")
                    continue
            uninstall(old_module, force=override)
        print(f"Installing module {module.current}...")
        module.update_url = url
        module.update_info()
        
        if not os.path.exists(module_path):
            os.makedirs(module_path, exist_ok=True)
        for f, src in module.files.items():
            dest = os.path.join(module_path, f)
            shutil.copy2(src, dest)
        print(f"Module {module.current} installed successfully.")

    if not modules:
        print(f"No valid modules found in {temp_dir}.")
            
def uninstall(module: Module, force: bool = False) -> None:
    """
    Uninstall a module.
    """
    print(f"Uninstalling module {module.current}...")
    if not force and not module.valid:
        raise ValueError(f"Module {module.current} is not valid.")
    
    if not os.path.exists(module.path):
        raise ValueError(f"Module path does not exist: {module.path}")
    
    shutil.rmtree(module.path)
    print(f"Module {module.current} uninstalled successfully.")
