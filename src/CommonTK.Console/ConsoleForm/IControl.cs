namespace SAPTeam.CommonTK.Console.ConsoleForm
{
    public interface IControl
    {
        int Line { get; }
        string Text { get; }

        Interface Parent { get; }

        bool Selectable { get; }

        void Write();
        void Clear();
        void Focus();
        void Update();
    }
}