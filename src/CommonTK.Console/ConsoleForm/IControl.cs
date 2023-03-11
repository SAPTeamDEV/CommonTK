using WindowsPro.ConsoleForm;

namespace SAPTeam.CommonTK.Console.ConsoleForm;

public interface IControl
{
    public int Line { get; init; }
    public string Text { get; init; }

    public Interface Parent { get; }

    public bool Selectable { get; }

    public void Write();
    public void Clear();
    public void Focus();
    public void Update();
}
