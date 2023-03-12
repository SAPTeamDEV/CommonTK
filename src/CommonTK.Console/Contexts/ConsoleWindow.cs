using SAPTeam.CommonTK.Console;

namespace SAPTeam.CommonTK.Contexts;

public class ConsoleWindow : Context
{
    ConsoleLaunchMode mode;

    public ConsoleWindow(ConsoleLaunchMode mode = ConsoleLaunchMode.Allocation) : base(mode)
    {
        
    }

    protected override void ArgsHandler(dynamic[] args)
    {
        mode = args[0];
    }

    protected override void CreateContext()
    {
        Interface = InteractInterface.Console;
        ShowConsole(mode);
    }

    protected override void DisposeContext()
    {
        HideConsole();
        Interface = InteractInterface.UI;
    }
}
