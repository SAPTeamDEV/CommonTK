import argparse
import glob
import importlib.util
import os
import platform
import subprocess
import urllib.request
from typing import Optional, Dict, Any, List, Callable

from .global_json import GlobalJson


class Host:
    """
    Host class for managing .NET SDK, environment, and project modules.
    Handles SDK installation, environment setup, solution detection, plugin loading, and extensible CLI commands.
    """

    BASE_DIR: str = os.getcwd()
    """Absolute path to the project root."""

    MODULES_DIR: str = os.path.join('build', 'modules')
    """Relative or absolute path to the bite modules directory."""

    DOTNET_DIR: str = '.dotnet'
    """Relative or absolute path to the .NET SDK directory."""

    SOLUTION_PATH: Optional[str] = None
    """Optional override for the solution (.sln) file."""

    BITE_PROJ_PATH: str = 'bite.proj'
    """Relative or absolute path to the bite.proj file."""

    DEFAULT_ARGS: List[str] = ['--nologo']
    """Default arguments to pass to dotnet CLI commands."""

    ENVIRONMENT_VARIABLES: Dict[str, str] = {
        'DOTNET_CLI_TELEMETRY_OPTOUT': '1',
        'DOTNET_SKIP_FIRST_TIME_EXPERIENCE': '1',
        'DOTNET_CLI_UI_LANGUAGE': 'en',
        'MSBUILDTERMINALLOGGER': 'off'
    }
    """Environment variables to set for all dotnet CLI invocations."""

    DOTNET_COMMANDS: List[Dict[str, str]] = [
        {'name': 'restore', 'help': 'Restore NuGet packages for the solution'},
        {'name': 'build', 'help': 'Build the solution (default)'},
        {'name': 'pack', 'help': 'Pack the solution'},
        {'name': 'clean', 'help': 'Clean the solution'},
        {'name': 'test', 'help': 'Test the solution'},
    ]
    """List of default dotnet commands and their help descriptions."""

    def __init__(
        self,
        app: str,
        description: Optional[str] = None,
    ) -> None:
        """
        Initialize the Host instance and prepare CLI argument parser configuration.

        Args:
            app: The name of the application.
            description: Description of the application.
        """
        self.name: str = app
        self.description = description
        self.argparser_usage = f'{self.name} command [options]'
        self.argparser_epilog = "Any unrecognized options will be passed to the command handler."

        # Normalize paths: only join with BASE_DIR if relative
        if not os.path.isabs(self.MODULES_DIR):
            self.MODULES_DIR = os.path.join(self.BASE_DIR, self.MODULES_DIR)
        if not os.path.isabs(self.DOTNET_DIR):
            self.DOTNET_DIR = os.path.join(self.BASE_DIR, self.DOTNET_DIR)
        if self.SOLUTION_PATH and not os.path.isabs(self.SOLUTION_PATH):
            self.SOLUTION_PATH = os.path.join(self.BASE_DIR, self.SOLUTION_PATH)
        if not os.path.isabs(self.BITE_PROJ_PATH):
            self.BITE_PROJ_PATH = os.path.join(self.BASE_DIR, self.BITE_PROJ_PATH)

        self.DEFAULT_ARGS.append(f'/p:BiteModulesPath={self.msbuild_path(self.MODULES_DIR)}')

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
        self.handlers: Dict[str, Callable[[argparse.Namespace, List[str]], None]] = {}
        self._subparsers_action: Optional[argparse._SubParsersAction] = None

    # --- CLI and Command Registration ---

    def get_argparser(self) -> argparse.ArgumentParser:
        """
        Get or create the argument parser for the CLI, using subparsers for each command.

        Returns:
            argparse.ArgumentParser: The argument parser instance.
        """
        if self.argparser is not None:
            return self.argparser

        parser = argparse.ArgumentParser(
            prog=self.name,
            description=self.description,
            epilog=self.argparser_epilog,
            usage=self.argparser_usage,
            add_help=True
        )
        subparsers = parser.add_subparsers(
            dest='command',
            required=False,
            metavar='',
        )
        subparsers.required = False
        parser.set_defaults(command='build', func=self._handle_dotnet_command)
        self._subparsers_action = subparsers

        # Register built-in dotnet commands
        for cmd in self.DOTNET_COMMANDS:
            sub = subparsers.add_parser(
                cmd['name'],
                help=cmd['help'],
                epilog=self.argparser_epilog + ' (Dotnet CLI)',
                usage=self.argparser_usage.replace('command', cmd['name']),
            )
            sub.set_defaults(func=self._handle_dotnet_command)
            self.handlers[cmd['name']] = self._handle_dotnet_command

        # Register bite command only if bite.proj exists
        if os.path.isfile(self.BITE_PROJ_PATH):
            bite_parser = subparsers.add_parser(
                'bite',
                help='Run a bite.core target',
                epilog=self.argparser_epilog + ' (MSBuild)',
                usage=self.argparser_usage.replace('command', 'bite') + ' [target]',
            )
            bite_parser.add_argument('target', nargs='?', default='help', help='bite.core target to run, default is "help"')
            bite_parser.set_defaults(func=self._handle_bite)
            self.handlers['bite'] = self._handle_bite

        self.argparser = parser
        return self.argparser

    def add_command(
        self,
        name: str,
        handler: Callable[[argparse.Namespace, List[str]], None],
        description: Optional[str] = None,
        help: str = "",
        arguments: Optional[List[Dict[str, Any]]] = None,
    ) -> None:
        """
        Register a new subcommand for extensibility (for modules/plugins).

        Args:
            name: The command name (e.g., 'deploy').
            handler: The function to handle the command (called with parsed args and unknown args).
            description: Optional description for the command.
            help: Help string for the command.
            arguments: List of dicts with keys for add_argument (optional).
        """
        self.get_argparser()
        subparsers = self._subparsers_action
        if subparsers is None:
            raise RuntimeError("Subparsers not found in parser")
        sub = subparsers.add_parser(name, help=help, description=description or help, epilog=self.argparser_epilog)
        if arguments:
            for arg in arguments:
                sub.add_argument(*arg.get('args', ()), **arg.get('kwargs', {}))
        sub.set_defaults(func=handler)
        self.handlers[name] = handler

    def register_handler(self, command: str, handler: Callable[[argparse.Namespace, List[str]], None]) -> None:
        """
        Register a custom handler for a command.

        Args:
            command: The command name.
            handler: The function to handle the command.
        """
        self.handlers[command] = handler

    def dispatch(self, args: argparse.Namespace, unknown_args: Optional[List[str]] = None) -> None:
        """
        Dispatch the parsed arguments to the appropriate handler.

        Args:
            args: The parsed argparse.Namespace.
            unknown_args: List of unknown arguments, if any.
        """
        extras = unknown_args or []
        if hasattr(args, 'func'):
            args.func(args, extras)
        else:
            self.get_argparser().print_help()

    # --- Dotnet/MSBuild Command Handlers ---

    def _handle_dotnet_command(self, args: argparse.Namespace, extras: List[str]) -> None:
        """
        Handle standard dotnet commands: restore, clean, build, test, pack.
        Passes any extra arguments to the dotnet CLI.
        """
        self.run(args.command, *extras)

    def _handle_bite(self, args: argparse.Namespace, extras: List[str]) -> None:
        """
        Handle the 'bite' command, running a custom msbuild target.
        Passes any extra arguments to msbuild.
        """
        target = getattr(args, 'target', 'help')
        self.run_bite(target, *extras)

    # --- Dotnet/MSBuild Execution ---

    def run(self, command: str, *args: str) -> None:
        """
        Run a dotnet command with the solution file as an argument.

        Args:
            command: The dotnet CLI command to run (e.g., 'build', 'restore').
            *args: Additional arguments to pass to the command.
        """
        cmd = ['dotnet', command] + [self.solution] + self.DEFAULT_ARGS + list(args)
        try:
            subprocess.check_call(cmd)
        except subprocess.CalledProcessError as e:
            print(f"Error: dotnet {command} failed with exit code {e.returncode}")
            raise

    def run_bite(self, target: str, *args: str) -> None:
        """
        Run bite.core with the specified target.

        Args:
            target: The bite.core target to run.
            *args: Additional arguments to pass to msbuild.
        """
        cmd = ['dotnet', 'msbuild'] + self.DEFAULT_ARGS + [f'-t:{target}', self.BITE_PROJ_PATH] + list(args)
        try:
            subprocess.check_call(cmd)
        except subprocess.CalledProcessError as e:
            print(f"Error: msbuild target '{target}' failed with exit code {e.returncode}")
            raise

    # --- SDK Installation ---

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
        if self.requested_sdk is None:
            raise RuntimeError("No .NET SDK install required")

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
        if self.requested_sdk is None:
            raise RuntimeError("No .NET SDK install required")

        installer = os.path.join(self.DOTNET_DIR, 'dotnet-install.ps1')
        url = 'https://dot.net/v1/dotnet-install.ps1'

        with urllib.request.urlopen(url) as response, open(installer, 'wb') as out_file:
            out_file.write(response.read())

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
        if self.requested_sdk is None:
            raise RuntimeError("No .NET SDK install required")

        installer = os.path.join(self.DOTNET_DIR, 'dotnet-install.sh')
        url = 'https://dot.net/v1/dotnet-install.sh'

        with urllib.request.urlopen(url) as response, open(installer, 'wb') as out_file:
            out_file.write(response.read())

        os.chmod(installer, 0o755)

        subprocess.check_call([
            'bash', installer,
            '--version', self.requested_sdk,
            '--install-dir', self.DOTNET_DIR
        ])

    # --- Solution/SDK Detection ---

    def detect_solution(self) -> str:
        """
        Find the single .sln file in BASE_DIR and return its full path.

        Returns:
            str: The full path to the solution file.

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
            Optional[str]: The required version if installation is needed, otherwise None.
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

    # --- Utility ---

    @staticmethod
    def msbuild_path(path: str) -> str:
        """
        Convert a Python path string to an MSBuild-acceptable path for directory properties.
        Ensures absolute path, uses backslashes, and ends with a backslash.
        """
        abs_path = os.path.abspath(path)
        msbuild_path = abs_path
        if not msbuild_path.endswith('\\'):
            msbuild_path += '\\'
        if ' ' in msbuild_path:
            msbuild_path = f'"{msbuild_path}"'
        return msbuild_path

    def load_modules(self) -> Dict[str, Any]:
        """
        Load all .bite.py modules from the modules directory.

        Returns:
            Dict[str, Any]: Mapping of plugin names to loaded module objects.
        """
        mods: Dict[str, Any] = {}
        pattern = os.path.join(self.MODULES_DIR, '**', '*.bite.py')
        for path in glob.glob(pattern, recursive=True):
            name = os.path.splitext(os.path.basename(path))[0]
            spec = importlib.util.spec_from_file_location(name, path)
            if spec is None or spec.loader is None:
                continue
            mod = importlib.util.module_from_spec(spec)
            try:
                spec.loader.exec_module(mod)
            except Exception as e:
                print(f"Failed to load module {name} from {path}: {e}")
                continue
            if hasattr(mod, 'load'):
                try:
                    mods[name] = mod.load(self)
                except Exception as e:
                    print(f"Module '{name}' failed to initialize: {e}")
        return mods
