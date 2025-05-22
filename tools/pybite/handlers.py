import argparse
from typing import List

from .msbuild import MSBuildTarget
from .host import Host

def handle_dotnet_cli(host: Host, args: argparse.Namespace, extras: List[str]) -> None:
    """
    Handle custom dotnet commands.
    Passes any extra arguments to the dotnet CLI.
    """
    host.run('', *extras)

def handle_dotnet_builtin(host: Host, args: argparse.Namespace, extras: List[str]) -> None:
    """
    Handle built-in dotnet commands.
    Passes any extra arguments to the dotnet CLI.
    """
    host.run_builtin(args.command, *extras)

def handle_bite_run(host: Host, args: argparse.Namespace, extras: List[str]) -> None:
    """
    Handle the 'run' command, running a custom msbuild target.
    Passes any extra arguments to msbuild.
    """
    list_targets = getattr(args, 'list', False)
    if list_targets:
        if getattr(args, 'target', 'help') != 'help' or extras:
            host.get_argparser().error("The 'list' option cannot be used with other arguments.")
        
        dependant_targets: List[MSBuildTarget] = []
        targets = host.get_bite_core_targets()
        print("Available independent targets:")
        for target in targets:
            if getattr(target, 'AfterTargets', None) is None and getattr(target, 'BeforeTargets', None) is None:
                print(f"  {target.Name}")
            else:
                dependant_targets.append(target)
        if dependant_targets:
            print("\nAvailable automated targets:")
            for target in dependant_targets:
                print(f"  {target.Name}", end=' ')
                if getattr(target, 'AfterTargets', None):
                    print(f"(after '{target.AfterTargets}')", end=' ')
                if getattr(target, 'BeforeTargets', None):
                    print(f"(before '{target.BeforeTargets}')", end=' ')
                print()
        return
    target = getattr(args, 'target', 'help')
    host.run_bite(target, *extras)

def handle_bite_list(host: Host, args: argparse.Namespace, extras: List[str]) -> None:
    """
    Handle the 'list' command, listing available modules.
    """
    if extras:
        host.get_argparser().error("The 'list' option does not accept any extra arguments.")
    
    verbose = getattr(args, 'verbose', False)
    modules = host.get_modules()
    print("Available modules:")
    for module in modules.values():
        if not verbose:
            print(f"  {module.id}"
                  f"{f' ({module.name})' if module.name else ''}"
                  f"{f' - {module.description}' if module.description else ''}")
        else:
            print(f"  {module.id}")
            print(f"    Name: {module.name}")
            print(f"    Version: {module.version}")
            print(f"    Description: {module.description}")
            print(f"    Author: {module.author}")
            print(f"    Updateable: {module.updatable}")
            if module.updatable:
                print(f"    Update URL: {module.update_url}")
            print(f"    Path: {module.path}")
            
            if False:
                files = module.files
                print(f"    Files: {len(files)}")
                for file in files:
                    print(f"      - {file}")
            print()

def handle_bite_install(host: Host, args: argparse.Namespace, extras: List[str]) -> None:
    """
    Handle the 'install' command, installing module.
    """
    if extras:
        host.get_argparser().error(f"Invalid arguments: {extras}")
    
    from . import module
    
    source = args.source
    upgrade = args.upgrade
    
    module.install(source, host.MODULES_DIR, upgrade=upgrade)

def handle_bite_update(host: Host, args: argparse.Namespace, extras: List[str]) -> None:
    """
    Handle the 'update' command, updating module.
    """
    
    if extras:
        host.get_argparser().error(f"Invalid arguments: {extras}")
    
    from . import module
    
    modules = getattr(args, 'modules', [])
    all_modules = args.all
    
    sources: List[str] = []
    hmods = host.get_modules()
    if all_modules:
        for mod in hmods.values():
            if mod.updatable:
                sources.append(mod.update_url)
    elif modules:
        for id in modules:
            mod = hmods.get(id)
            if mod is None:
                host.get_argparser().error(f"Module '{id}' not found.")
            if mod.updatable:
                sources.append(mod.update_url)
            else:
                host.get_argparser().error(f"Module '{id}' is not updatable.")
    else:
        host.get_argparser().error("No modules specified for update.")

    for source in sources:
        module.install(source, host.MODULES_DIR, upgrade=True)
    
    if not sources:
        print("No modules to update.")
