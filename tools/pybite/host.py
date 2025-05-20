import argparse
import glob
import importlib.util
import os
import platform
import subprocess
from typing import Optional, Dict, Any, List
import urllib.request

from .global_json import GlobalJson

class Host:
    """
    Host class for managing .NET SDK, environment, and project modules.
    Handles SDK installation, environment setup, solution detection, and plugin loading.
    """
    BASE_DIR: str = os.getcwd()
    """Base directory for the project (current working directory)."""

    MODULES_DIR: str = os.path.join(BASE_DIR, 'build', 'modules')
    """Directory where .bite.py modules are located."""

    DOTNET_DIR: str = os.path.join(BASE_DIR, '.dotnet')
    """Directory where the .NET SDK is or will be installed."""

    SOLUTION_PATH: Optional[str] = None
    """Optional override for the solution path."""

    DEFAULT_ARGS: List[str] = ['--nologo']
    """Default arguments to pass to dotnet CLI commands."""

    ENVIRONMENT_VARIABLES: Dict[str, str] = {
        'DOTNET_CLI_TELEMETRY_OPTOUT': '1',
        'DOTNET_SKIP_FIRST_TIME_EXPERIENCE': '1',
        'DOTNET_CLI_UI_LANGUAGE': 'en',
        'MSBUILDTERMINALLOGGER': 'off'
    }
    """Environment variables to set for all dotnet CLI invocations."""

    def __init__(self, app: str) -> None:
        """
        Initialize the Host instance.

        Args:
            app: The name of the application (used for argument parsing).
        """
        self.name: str = app

        try:
            self.global_json: Optional[GlobalJson] = GlobalJson(os.path.join(self.BASE_DIR, 'global.json'))
        except FileNotFoundError:
            self.global_json = None

        self.ENVIRONMENT_VARIABLES['PATH'] = (
            self.DOTNET_DIR + os.pathsep + os.environ.get('PATH', '')
        )
        self._set_environment_variables()

        self.requested_sdk: Optional[str] = self._resolve_requested_sdk()
        self.solution: str = self.SOLUTION_PATH or self.detect_solution()
        self.argparser: Optional[argparse.ArgumentParser] = None

    def get_argparser(self) -> argparse.ArgumentParser:
        """
        Get or create the argument parser for the CLI.

        Returns:
            An argparse.ArgumentParser instance for parsing command-line arguments.
        """
        if self.argparser is not None:
            return self.argparser

        parser = argparse.ArgumentParser(prog=self.name)
        parser.add_argument('action', choices=['restore', 'clean', 'build', 'bite', 'help'])
        parser.add_argument('extras', nargs=argparse.REMAINDER)
        self.argparser = parser
        return self.argparser

    def _set_environment_variables(self) -> None:
        """
        Set environment variables required for dotnet CLI commands.
        """
        for k, v in self.ENVIRONMENT_VARIABLES.items():
            os.environ[k] = v

    def _install_sdk(self) -> None:
        """
        Install the required .NET SDK if not already present.
        Determines the platform and calls the appropriate installation method.
        """
        if not self.requested_sdk:
            return

        os.makedirs(self.DOTNET_DIR, exist_ok=True)
        system = platform.system().lower()

        if system == 'windows':
            self._install_sdk_windows()
        else:
            self._install_sdk_unix()

    def _install_sdk_windows(self) -> None:
        """
        Download and install the .NET SDK on Windows using the official PowerShell script.
        """
        installer = os.path.join(self.DOTNET_DIR, 'dotnet-install.ps1')
        url = 'https://dot.net/v1/dotnet-install.ps1'

        # Download the PowerShell script using urllib
        with urllib.request.urlopen(url) as response, open(installer, 'wb') as out_file:
            out_file.write(response.read())

        # Run the installer script
        subprocess.check_call([
            'powershell', '-NoProfile', '-ExecutionPolicy', 'Bypass',
            installer,
            '-Version', self.requested_sdk,
            '-InstallDir', self.DOTNET_DIR
        ])

    def _install_sdk_unix(self) -> None:
        """
        Download and install the .NET SDK on Unix-like systems using the official Bash script.
        """
        installer = os.path.join(self.DOTNET_DIR, 'dotnet-install.sh')
        url = 'https://dot.net/v1/dotnet-install.sh'

        # Download the Bash script using urllib
        with urllib.request.urlopen(url) as response, open(installer, 'wb') as out_file:
            out_file.write(response.read())

        # Make the script executable
        os.chmod(installer, 0o755)

        # Run the installer script
        subprocess.check_call([
            'bash', installer,
            '--version', self.requested_sdk,
            '--install-dir', self.DOTNET_DIR
        ])

    def detect_solution(self) -> str:
        """
        Find the single .sln file in BASE_DIR and return its full path.

        Returns:
            The full path to the solution file.

        Raises:
            RuntimeError: If zero or multiple .sln files are found.
        """
        slns = [f for f in os.listdir(self.BASE_DIR) if f.endswith('.sln')]
        if len(slns) == 1:
            return os.path.join(self.BASE_DIR, slns[0])
        if not slns:
            raise RuntimeError(f'No .sln file found in {self.BASE_DIR}')
        raise RuntimeError('Multiple .sln files found: ' + ', '.join(slns))

    def _resolve_requested_sdk(self) -> Optional[str]:
        """
        Determine if the required SDK is installed.

        Returns:
            The required version if installation is needed, otherwise None.
        """
        required = (
            self.global_json.version
            if self.global_json and self.global_json.version
            else 'latest'
        )
        try:
            sdks = subprocess.check_output(['dotnet', '--list-sdks'], stderr=subprocess.DEVNULL)
            installed = [line.split()[0] for line in sdks.decode().splitlines()]
            if required != 'latest' and self.global_json:
                if any(self.global_json.is_compatible(s) for s in installed):
                    return None
                return required
            return None
        except (subprocess.CalledProcessError, FileNotFoundError):
            return required

    def run(self, command: str, *args: str) -> None:
        """
        Run a dotnet command with the solution file as an argument.

        Args:
            command: The dotnet CLI command to run (e.g., 'build', 'restore').
            *args: Additional arguments to pass to the command.
        """
        cmd = ['dotnet', command] + self.DEFAULT_ARGS + [self.solution] + list(args)
        subprocess.check_call(cmd)

    def msbuild(self, target: str, *args: str) -> None:
        """
        Run msbuild with a specific target using dotnet CLI.

        Args:
            target: The msbuild target to run.
            *args: Additional arguments to pass to msbuild.
        """
        cmd = ['dotnet', 'msbuild'] + self.DEFAULT_ARGS + [f'-t:{target}', 'bite.proj'] + list(args)
        subprocess.check_call(cmd)

    def load_modules(self) -> Dict[str, Any]:
        """
        Load all .bite.py modules from the modules directory.

        Returns:
            A dictionary mapping plugin names to loaded module objects.
        """
        mods: Dict[str, Any] = {}
        pattern = os.path.join(self.MODULES_DIR, '**', '*.bite.py')
        for path in glob.glob(pattern, recursive=True):
            name = os.path.splitext(os.path.basename(path))[0]
            spec = importlib.util.spec_from_file_location(name, path)
            if spec is None or spec.loader is None:
                continue
            mod = importlib.util.module_from_spec(spec)
            spec.loader.exec_module(mod)
            if hasattr(mod, 'load'):
                mods[name] = mod.load(self)
        return mods
