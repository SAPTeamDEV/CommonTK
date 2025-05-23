import argparse
from typing import List

import pybite

def load(host: pybite.Host) -> None:
    """
    Load the PyBite module.
    """
    host.add_command(
        'nbgv',
        handler=handle_nbgv,
        help='Run NerdBank.GitVersioning CLI tool',
        add_help=False,
    )

def handle_nbgv(host: pybite.Host, args: argparse.Namespace, extras: List[str]) -> None:
    """
    Handle the nbgv command.
    """
    host.run('nbgv', *extras)
