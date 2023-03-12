using SAPTeam.CommonTK.Console.ConsoleForm.Controls;
using SAPTeam.CommonTK.Contexts;

namespace WindowsPro.ConsoleForm;

public partial class Form
{
    public void CreateObjects()
    {
        using RedirectConsole cv = new();
        Title();

        if (Items.Count > 0)
        {
            foreach (var section in Items.OrderBy(key => key.Key))
            {
                if (Items[section.Key].Count == 0) continue;
                ConsoleSection secClass = AddSection(section.Key);
                foreach (var option in section.Value)
                {
                    AddOption(option, secClass);
                }
            }
        }
        else if (ExecutableItems.Count > 0)
        {
            foreach (var section in ExecutableItems.OrderBy(key => key.Key))
            {
                if (ExecutableItems[section.Key].Count == 0) continue;
                ConsoleSection secClass = AddSection(section.Key);
                foreach (var data in section.Value)
                {
                    AddOption(data.Value, data.Key, secClass);
                }
            }
        }
    }

    private ConsoleSection AddSection(string section)
    {
        ConsoleSection secClass = new(Platform, GetLine(), section);
        Container[GetLine()] = secClass;
        secClass.Write();
        Echo();
        return secClass;
    }

    private ConsoleOption AddOption(string option, ConsoleSection? secClass)
    {
        if (First == 0) First = Current = GetLine();
        Last = GetLine();
        ConsoleOption opClass = new(Platform, GetLine(), option, secClass);
        Container[GetLine()] = opClass;
        opClass.Write();
        Echo();
        return opClass;
    }

    private ConsoleOption AddOption(string option)
    {
        return AddOption(option, null);
    }

    private ConsoleOption AddOption(string option, string identifier, ConsoleSection secClass)
    {
        ConsoleOption opClass = AddOption(option, secClass);
        opClass.Identifier = identifier;
        return opClass;
    }

}
