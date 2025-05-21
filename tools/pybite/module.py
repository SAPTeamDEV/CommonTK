# write a class named module that accepts a path to a directory and then opens a json file and stores parsed object and extracts name and description
import os
import json
from typing import Optional, Dict, Any

class Module:
    """
    A class representing a module in the Bite build engine.
    It loads a JSON file from the specified directory and extracts its information.
    """
    def __init__(self, path: str, require_json: bool = True) -> None:
        self.path = path
        self.id: str = os.path.basename(path)
        self.name: Optional[str] = None
        self.description: Optional[str] = None
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
            self.name = data.get('name')
            self.description = data.get('description')
            self.module_info = data
