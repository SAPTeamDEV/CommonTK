/*
 namespace WindowsPro.Extensions.StatusProvider;

public class ConsoleStatusProvider : IStatusProvider
{
    private readonly int line;
    private (int left, int top) currentPosition = (-1, -1);

    public ConsoleStatusProvider(int line)
    {
        this.line = line;
    }

    private void Focus()
    {
        currentPosition = (Console.CursorLeft, Console.CursorTop);
        Console.SetCursorPosition(0, line);
    }

    private void UnFocus()
    {
        if (currentPosition.left == -1 || currentPosition.top == -1) throw new ApplicationException("Focus method must be called before calling UnFocus");
        Console.SetCursorPosition(currentPosition.left, currentPosition.top);
    }

    public void Clear()
    {
        Focus();
        ClearLine(true);
        UnFocus();
    }

    public void Write(string message)
    {
        Clear();
        Focus();
        Echo(message, false);
        UnFocus();
    }
}
*/