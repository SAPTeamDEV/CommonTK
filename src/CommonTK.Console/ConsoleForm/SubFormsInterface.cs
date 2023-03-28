using System.Collections.Generic;
using System.Linq;

namespace SAPTeam.CommonTK.Console.ConsoleForm
{
    public partial class Interface
    {
        private readonly List<Form> subForms = new List<Form>();

        public void AddSubForm(Form subForm)
        {
            subForms.Add(subForm);
            System.Console.Clear();
            subForm.Platform = this;
            subForm.CreateObjects();
            SetActiveForm(subForm);
        }

        public void SetActiveForm(Form form)
        {
            form.SetReceivers();
            activeForm = form;
            Refresh();
        }

        public void CloseSubForm()
        {
            if (activeForm == subForms.Last())
            {
                RaiseClose();
                subForms.RemoveAt(subForms.Count - 1);
                SetActiveForm(subForms.Count > 0 ? subForms.Last() : rootForm);
            }
        }
    }
}