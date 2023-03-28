namespace SAPTeam.CommonTK.Console.ConsoleForm
{
    public abstract class Control : IControl
    {
        public Interface Parent { get; }

        public int Line { get; }
        public string Text { get; }
        public abstract bool Selectable { get; }

        public Control(Interface parent, int line, string text)
        {
            Line = line;
            Text = text;
            Parent = parent;
        }

        public delegate void Method();

        public void Update()
        {
            Focus(Clear, Write);
        }

        public void Clear()
        {
            Focus(ClearInLine);
        }

        public void Focus()
        {
            SetCursor(0, Line, Parent.Spacing);
        }

        public void Focus(params Method[] methods)
        {
            Focus();
            foreach (Method method in methods)
            {
                method();
            }
        }

        protected static void Write(string text, bool centerize = false)
        {
            if (centerize)
            {
                text = new string(' ', GetCenterPosition(text.Length)) + text;
            }
            Echo(text, false);
        }

        public virtual void Write()
        {
            Write(Text);
        }
    }
}