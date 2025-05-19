import argparse
import glob
import importlib.util
import os
import platform
import subprocess

from .global_json import GlobalJson

class Host:
    BASE_DIR = os.getcwd()
    MODULES_DIR = os.path.join(BASE_DIR, 'build', 'modules')
    DOTNET_DIR = os.path.join(BASE_DIR, '.dotnet')
    
    SOLUTION_PATH = None
    
    DEFAULT_ARGS = ['--nologo']

    ENVIRONMENT_VARIABLES = {
        'DOTNET_CLI_TELEMETRY_OPTOUT': '1',
        'DOTNET_SKIP_FIRST_TIME_EXPERIENCE': '1',
        'DOTNET_CLI_UI_LANGUAGE': 'en',
        'MSBUILDTERMINALLOGGER': 'off'
    }
    
    def __init__(self, app):
        self.name = app
        
        try:
            self.global_json = GlobalJson(os.path.join(self.BASE_DIR, 'global.json'))
        except FileNotFoundError:
            self.global_json = None
        
        self.ENVIRONMENT_VARIABLES['PATH'] = self.DOTNET_DIR + os.pathsep + os.environ.get('PATH', '')
        self._set_environment_variables()
        
        self.requested_sdk = self._resolve_requested_sdk()
        self.solution = self.SOLUTION_PATH or self.detect_solution()
        self.argparser = None

    def get_argparser(self):
        if self.argparser is not None:
            return self.argparser
        
        self.argparser = argparse.ArgumentParser(prog=self.name)
        self.argparser.add_argument('action', choices=['restore','clean','build','bite','help'])
        self.argparser.add_argument('extras', nargs=argparse.REMAINDER)
        
        return self.argparser

    def _set_environment_variables(self):
        for k, v in self.ENVIRONMENT_VARIABLES.items():
            os.environ[k] = v

    def install_sdk(self):
        if not self.requested_sdk:
            return

        os.makedirs(self.DOTNET_DIR, exist_ok=True)

        system = platform.system().lower()
        if system == 'windows':
            # Use PowerShell installer
            installer = os.path.join(self.DOTNET_DIR, 'dotnet-install.ps1')
            url = 'https://dot.net/v1/dotnet-install.ps1'

            # Download the PowerShell script
            subprocess.check_call([
                'powershell', '-NoProfile', '-ExecutionPolicy', 'Bypass',
                '-Command', f"Invoke-WebRequest '{url}' -OutFile '{installer}'"
            ])

            # Run the installer script
            subprocess.check_call([
                'powershell', '-NoProfile', '-ExecutionPolicy', 'Bypass',
                installer,
                '-Version', self.requested_sdk,
                '-InstallDir', self.DOTNET_DIR
            ])
        else:
            # Use Bash installer
            installer = os.path.join(self.DOTNET_DIR, 'dotnet-install.sh')
            url = 'https://dot.net/v1/dotnet-install.sh'

            # Download the Bash script
            subprocess.check_call([
                'curl', '-sSL', url,
                '-o', installer
            ])

            # Make the script executable
            os.chmod(installer, 0o755)

            # Run the installer script
            subprocess.check_call([
                'bash', installer,
                '--version', self.requested_sdk,
                '--install-dir', self.DOTNET_DIR
            ])
    
    def detect_solution(self):
        """
        Finds the single .sln file in BASE_DIR and returns its full path.
        Raises RuntimeError if zero or multiple solutions are found.
        """
        slns = [f for f in os.listdir(self.BASE_DIR) if f.endswith('.sln')]
        if len(slns) == 1:
            return os.path.join(self.BASE_DIR, slns[0])
        if not slns:
            raise RuntimeError('No .sln file found in ' + self.BASE_DIR)
        raise RuntimeError('Multiple .sln files found: ' + ', '.join(slns))

    def _resolve_requested_sdk(self):
        required = self.global_json.version if self.global_json and self.global_json.version else 'latest'
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

    def run(self, command, *args):
        cmd = ['dotnet', command] + self.DEFAULT_ARGS + [self.solution] + list(args)
        subprocess.check_call(cmd)

    def msbuild(self, target, *args):
        cmd = ['dotnet', 'msbuild'] + self.DEFAULT_ARGS + [f'-t:{target}', 'bite.proj'] + list(args)
        subprocess.check_call(cmd)
        
    def load_modules(self):
        plugins = {}
        for path in glob.glob(os.path.join(self.MODULES_DIR, '**', '*.bite.py')):
            name = os.path.splitext(os.path.basename(path))[0]
            spec = importlib.util.spec_from_file_location(name, path)
            mod = importlib.util.module_from_spec(spec)
            spec.loader.exec_module(mod)
            if hasattr(mod, 'load'):
                plugins[name] = mod.load(self)
        return plugins
