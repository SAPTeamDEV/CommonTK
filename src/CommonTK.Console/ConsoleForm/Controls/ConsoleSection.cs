namespace SAPTeam.CommonTK.Console.ConsoleForm.Controls
{
    public class ConsoleSection : Control, IControl
    {
        public ConsoleSection(Interface parent, int line, string text) : base(parent, line, text) { }

        public override bool Selectable => false;
    }
}