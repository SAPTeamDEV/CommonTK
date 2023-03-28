using SAPTeam.CommonTK.Console;

namespace SAPTeam.CommonTK.Contexts
{
    public class ConsoleWindow : Context
    {
        ConsoleLaunchMode mode;
        bool canClose;
        bool release;

        public ConsoleWindow(ConsoleLaunchMode mode = ConsoleLaunchMode.Allocation, bool canClose = false, bool release = true) : base(mode, canClose, release)
        {

        }

        protected override void ArgsHandler(dynamic[] args)
        {
            mode = args[0];
            canClose = args[1];
            release = args[2];
        }

        protected override void CreateContext()
        {
            Interface = InteractInterface.Console;
            ConsoleManager.ShowConsole(mode, canClose);
        }

        protected override void DisposeContext()
        {
            ConsoleManager.HideConsole(release);
            Interface = InteractInterface.UI;
        }
    }
}