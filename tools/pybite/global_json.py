import json, os

class GlobalJson:
    """
    Represents the contents of a global.json file.
    """
    def __init__(self, path=None):
        self.path = path
        self._load()

    def _load(self):
        if not os.path.isfile(self.path):
            raise FileNotFoundError(self.path)
        with open(self.path, 'r', encoding='utf-8') as f:
            data = json.load(f)
        self.sdk = data.get('sdk', {})

    @property
    def version(self):
        return self.sdk.get('version')

    @property
    def roll_forward(self):
        return self.sdk.get('rollForward', 'patch')

    @property
    def allow_prerelease(self):
        return self.sdk.get('allowPrerelease', False)

    def is_compatible(self, given_version: str) -> bool:
        parts = given_version.split('-')[0].split('.')
        nums = [int(p) if p.isdigit() else 0 for p in parts]
        req = tuple(int(p) for p in self.version.split('.')) if self.version else (0, 0, 0)
        
        if req[0] == 0:
            return True
        
        if not self.allow_prerelease and '-' in given_version:
            return False
        
        rf = self.roll_forward.lower()
        if rf == 'patch':
            return nums[:2] == req[:2] and nums[2] >= req[2]
        elif rf in ('feature', 'latestfeature', 'minor', 'latestminor'):
            return nums[0] == req[0] and (nums[1] > req[1] or (nums[1] == req[1] and nums[2] >= req[2]))
        elif rf in ('major', 'latestmajor'):
            return (nums[0] > req[0] or (nums[0] == req[0] and (nums[1] > req[1] or (nums[1] == req[1] and nums[2] >= req[2]))))
        return nums == req
