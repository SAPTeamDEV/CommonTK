import sys

import config

if __name__ == '__main__':
    host = config.pybite.Host('build.py')
    host.load_modules()
    
    if (host.requested_sdk is not None):
        print(f'Installing .NET SDK {host.requested_sdk}')
        host.install_sdk()
    
    p = host.get_argparser()
    args = p.parse_args()
    
    if args.action == 'help':
        p.print_help()
        sys.exit()
    elif args.action == 'bite':
        if args.extras and not args.extras[0].startswith('-'):
            target, extras = args.extras[0], args.extras[1:]
            host.msbuild(target, *extras)
        else:
            host.msbuild('help', *args.extras)
    else:
        host.run(args.action, *args.extras)
