using SAPTeam.CommonTK.Contexts;

namespace SAPTeam.CommonTK.Console.Contexts;

public class ConsoleWindow : Context
{
    protected override void ArgsHandler(dynamic[] args)
    {
        throw new NotImplementedException();
    }

    protected override void CreateContext()
    {
        Interface = InteractInterface.Console;
        ShowConsole();
    }

    protected override void DisposeContext()
    {
        HideConsole();
        Interface = InteractInterface.UI;
    }
}
