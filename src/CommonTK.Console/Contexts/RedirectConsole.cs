using System;
using System.IO;

namespace SAPTeam.CommonTK.Contexts
{
    public class RedirectConsole : Context
    {
        private TextWriter consoleOut;
        private StringWriter consoleOutVirtual;

        public int Line { get; set; }

        protected override void ArgsHandler(dynamic[] args)
        {
            throw new NotImplementedException();
        }

        protected override void CreateContext()
        {
            consoleOut = System.Console.Out;
            consoleOutVirtual = new StringWriter();
            System.Console.SetOut(consoleOutVirtual);
        }

        protected override void DisposeContext()
        {
            System.Console.SetOut(consoleOut);
            consoleOut = null;
            consoleOutVirtual.Dispose();
        }
    }
}