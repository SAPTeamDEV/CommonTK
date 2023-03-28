namespace SAPTeam.CommonTK.Console.ConsoleForm.Controls
{
    public class ConsoleOption : Control, ISelectableControl
    {
        public override bool Selectable => true;
        public string Identifier { get; set; }

        public ConsoleSection Section;

        public ConsoleOption(Interface parent, int line, string text, ConsoleSection section) : base(parent, line, text)
        {
            Section = section;
        }

        public void Select()
        {
            Focus();
            Clear();
            Parent.ColorSchema.ChangeColor();
            Update();
            Utils.ResetColor();
        }

        public override void Write()
        {
            string text = Text;
            if (Section != null) text = "   " + text;
            Write(text);
        }
    }
}