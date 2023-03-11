namespace WindowsPro.ConsoleForm;

public partial class Interface
{
    private readonly List<Form> subForms = new();

    public void AddSubForm(Form subForm)
    {
        subForms.Add(subForm);
        Console.Clear();
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
        if (activeForm == subForms[^1])
        {
            RaiseClose();
            subForms.RemoveAt(subForms.Count - 1);
            SetActiveForm(subForms.Count > 0 ? subForms[^1] : rootForm);
        }
    }
}
