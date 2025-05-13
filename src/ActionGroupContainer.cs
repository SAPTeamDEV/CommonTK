using System.Collections;
using System.Collections.Generic;

namespace SAPTeam.CommonTK;

internal class ActionGroupContainer : IEnumerable<Context>
{
    public Context this[int index] => Contexts[index];

    public string Name { get; }

    public ActionGroupState State => IsSuppressed ? ActionGroupState.Suppressed : Contexts.Count > 0 ? ActionGroupState.Locked : ActionGroupState.Free;

    private List<Context> Contexts { get; }

    private Context suppressor;

    public bool IsSuppressed { get; private set; }
    public bool IsLocked => State == ActionGroupState.Locked;

    public int Count => Contexts.Count;

    public ActionGroupContainer(string name)
    {
        Name = name;
        Contexts = [];
        suppressor = default;
    }

    public void Suppress(Context suppressor)
    {
        if (IsSuppressed)
        {
            throw new ActionGroupException(ActionGroupError.AlreadySuppressed);
        }

        this.suppressor = suppressor;
        IsSuppressed = true;
    }

    public void Relock(Context suppressor)
    {
        if (!IsSuppressed)
        {
            throw new ActionGroupException(ActionGroupError.NotSuppressed);
        }
        else if (!IsSuppressor(suppressor))
        {
            throw new ActionGroupException(ActionGroupError.SuppressorRequired);
        }
        else
        {
            this.suppressor = default;
            IsSuppressed = false;
        }
    }

    public bool HasRegistered(Context context) => Contexts.Contains(context) || IsSuppressor(context);

    public bool IsSuppressor(Context context) => context == suppressor;

    public void Add(Context context)
    {
        if (IsSuppressed && !IsSuppressor(context))
        {
            throw new ActionGroupException($"The action group \"{Name}\" is suppressed by {suppressor.Name}.", ActionGroupError.Suppressed);
        }
        else if (Contexts.Contains(context))
        {
            throw new ActionGroupException(ActionGroupError.AlreadyLocked);
        }
        else
        {
            Contexts.Add(context);
        }
    }

    public void Remove(Context context)
    {
        if (IsSuppressor(context))
        {
            Relock(context);
        }

        Contexts.Remove(context);
    }

    public IEnumerator<Context> GetEnumerator() => Contexts.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Contexts.GetEnumerator();
}
