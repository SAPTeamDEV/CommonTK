import os
import sys

import config

if __name__ == '__main__':
    app_name = os.environ.get('PYBITE_APP_NAME')
    if not app_name:
        # Try to detect the script file name
        app_name = os.path.basename(sys.argv[0]) if sys.argv and sys.argv[0] else 'build.py'

    host = config.pybite.Host(
        app=app_name,
        description='Bite build engine command line interface',
    )

    host.load_modules()
    args, unknown = host.get_argparser().parse_known_args()

    if host.requested_sdk is not None:
        print(f'Installing .NET SDK {host.requested_sdk}')
        host._install_sdk()

    host.dispatch(args, unknown)
    
    config.pybite.download.cleanup_temp_folders()
