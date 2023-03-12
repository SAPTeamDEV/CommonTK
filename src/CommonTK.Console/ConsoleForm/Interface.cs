using SAPTeam.CommonTK.Console;
using SAPTeam.CommonTK.Console.ConsoleForm;
using SAPTeam.CommonTK.Console.ConsoleForm.Controls;
using SAPTeam.CommonTK.Contexts;

namespace WindowsPro.ConsoleForm;

/// <summary>
/// Generic Class for creating Selectable elements through Console Interface.
/// </summary>
public partial class Interface
{
    private readonly int titleLine;
    private int bufferWidth;
    private DisposableWriter disposableWriter;
    private readonly Form rootForm;
    private Form activeForm;

    private int Index
    {
        get => activeForm.Current;
        set
        {
            int newIndex = Math.Max(Math.Min(value, activeForm.Last), activeForm.First);
            if (newIndex == activeForm.Current) return;
            bool isSum = newIndex > activeForm.Current;
            activeForm.Container[activeForm.Current].Update();
            activeForm.Current = newIndex;
            if (activeForm.Container[newIndex] is ISelectableControl opClass)
            {
                opClass.Select();
            }
            else
            {
                if (isSum) Index++;
                else Index--;
            }
        }
    }

    public IControl this[int index] => activeForm.Container[index];

    public int Spacing => rootForm.First - titleLine;

    public ColorSet ColorSchema { get; set; } = new(ConsoleColor.White, ConsoleColor.Black);

    private bool hasExited;

    public Interface(Form form)
    {
        rootForm = activeForm = form;
        disposableWriter = rootForm.UseDisposableWriter ? new(true) : null;
        bufferWidth = Console.BufferWidth;
        titleLine = GetLine();
    }

    public void Refresh()
    {
        if (disposableWriter != null)
        {
            disposableWriter.Clear();
        }
        else
        {
            Console.Clear();
        }
        RaiseTitle();
        foreach (var item in activeForm.Container.Values)
        {
            item.Write();
            Echo();
        }
        if (activeForm.Container.Count > 0 && activeForm.Container[Index] is ConsoleOption opClass) opClass.Select();
    }

    public void Start()
    {
        Refresh();
        if (activeForm.FocusToTop && disposableWriter == null && OperatingSystem.IsWindows())
        {
            Console.WindowTop = titleLine;
        }
        Console.CursorVisible = false;
        RaiseStart();
        while (!hasExited)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (bufferWidth != Console.BufferWidth || keyInfo.Key == ConsoleKey.F5)
            {
                Refresh();
                bufferWidth = Console.BufferWidth;
            }
            if (keyInfo.Key == ConsoleKey.Escape)
            {
                if (!activeForm.IsClosable)
                {
                    ScreenMessage("You can't leave this page.");
                    continue;
                }
                if (activeForm == rootForm)
                {
                    Close();
                    break;
                }
                else
                {
                    CloseSubForm();
                    continue;
                }
            }
            if (rootForm.Container.Count > 0)
            {
                if (keyInfo.Key == ConsoleKey.Enter) RaiseEnter((ConsoleOption)rootForm.Container[Index]);
                else if (keyInfo.Key == ConsoleKey.UpArrow) Index--;
                else if (keyInfo.Key == ConsoleKey.DownArrow) Index++;
            }
            RaiseKeyPressed(keyInfo);
        }
    }

    public void Close()
    {
        RaiseClose();
        if (disposableWriter != null)
        {
            disposableWriter.Dispose();
        }
        hasExited = true;
        Console.CursorVisible = true;
    }
}
