import json, os

class GlobalJson:
    """
    Loads and represents the .NET global.json file, providing access to SDK versioning and roll-forward policy.
    """
    def __init__(self, path=None):
        """
        Initialize GlobalJson by loading the specified global.json file.

        :param path: Path to the global.json file.
        """
        self.path = path
        self.sdk = {}
        self._load()

    def _load(self):
        """
        Internal: Loads the global.json file and parses the 'sdk' section.
        Raises FileNotFoundError if the file does not exist.
        """
        if not os.path.isfile(self.path):
            raise FileNotFoundError(self.path)
        with open(self.path, 'r', encoding='utf-8') as f:
            data = json.load(f)
        self.sdk = data.get('sdk', {})

    @property
    def version(self):
        """
        Returns the required .NET SDK version as specified in global.json, or None if not set.
        """
        return self.sdk.get('version')

    @property
    def roll_forward(self):
        """
        Returns the rollForward policy for .NET SDK selection, defaulting to 'patch'.
        """
        return self.sdk.get('rollForward', 'patch')

    @property
    def allow_prerelease(self):
        """
        Returns True if prerelease SDKs are allowed, otherwise False.
        """
        return self.sdk.get('allowPrerelease', False)

    def is_compatible(self, given_version: str) -> bool:
        """
        Determines if the given SDK version string is compatible with the requirements in global.json.

        :param given_version: The SDK version string to check (e.g., '7.0.100').
        :return: True if compatible, False otherwise.
        """
        if not self.version:
            return True
        req = tuple(map(int, self.version.split('.')))
        parts = given_version.split('-')[0].split('.')
        nums = tuple(int(p) if p.isdigit() else 0 for p in parts)
        if not self.allow_prerelease and '-' in given_version:
            return False
        rf = self.roll_forward.lower()
        if rf == 'patch':
            return nums[:2] == req[:2] and nums[2] >= req[2]
        if rf in ('feature', 'latestfeature', 'minor', 'latestminor'):
            return nums[0] == req[0] and (nums[1] > req[1] or (nums[1] == req[1] and nums[2] >= req[2]))
        if rf in ('major', 'latestmajor'):
            return nums[0] > req[0] or (nums[0] == req[0] and (nums[1] > req[1] or (nums[1] == req[1] and nums[2] >= req[2])))
        return nums == req
