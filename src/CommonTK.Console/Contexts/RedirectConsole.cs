using SAPTeam.CommonTK.Contexts;

namespace SAPTeam.CommonTK.Console.Contexts;

public class RedirectConsole : Context
{
    private TextWriter? consoleOut;
    private StringWriter? consoleOutVirtual;

    public int Line { get; set; }

    protected override void ArgsHandler(dynamic[] args)
    {
        throw new NotImplementedException();
    }

    protected override void CreateContext()
    {
        consoleOut = System.Console.Out;
        consoleOutVirtual = new();
        System.Console.SetOut(consoleOutVirtual);
    }

    protected override void DisposeContext()
    {
        System.Console.SetOut(consoleOut);
        consoleOut = null;
        consoleOutVirtual.Dispose();
    }
}
