using System.Linq;

using SAPTeam.CommonTK.Console.ConsoleForm.Controls;
using SAPTeam.CommonTK.Contexts;

namespace SAPTeam.CommonTK.Console.ConsoleForm
{
    public partial class Form
    {
        public void CreateObjects()
        {
            using (RedirectConsole cv = new RedirectConsole())
            {
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
        }

        private ConsoleSection AddSection(string section)
        {
            ConsoleSection secClass = new ConsoleSection(Platform, Utils.GetLine(), section);
            Container[Utils.GetLine()] = secClass;
            secClass.Write();
            Utils.Echo();
            return secClass;
        }

        private ConsoleOption AddOption(string option, ConsoleSection secClass)
        {
            if (First == 0) First = Current = Utils.GetLine();
            Last = Utils.GetLine();
            ConsoleOption opClass = new ConsoleOption(Platform, Utils.GetLine(), option, secClass);
            Container[Utils.GetLine()] = opClass;
            opClass.Write();
            Utils.Echo();
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
}