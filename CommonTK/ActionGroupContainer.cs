using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SAPTeam.CommonTK
{
    internal class ActionGroupContainer : IEnumerable<Context>
    {
        public Context this[int index] { get => Contexts[index]; set => Contexts[index] = value; }

        public string Name { get; }

        public ActionGroupState State => IsSuppressed ? ActionGroupState.Suppressed : Contexts.Count > 0 ? ActionGroupState.Locked : ActionGroupState.Free;

        List<Context> Contexts { get; }

        Context suppressor;

        public bool IsSuppressed { get; private set; }

        public int Count => Contexts.Count;

        public ActionGroupContainer(string name)
        {
            Name = name;
            Contexts = new List<Context>();
            suppressor = default;
        }

        public void Suppress(Context suppressor)
        {
            if (IsSuppressed)
            {
                throw new ActionGroupException("The lock of action group already has suppressed.");
            }

            this.suppressor = suppressor;
            IsSuppressed = true;
        }

        public void Relock(Context suppressor)
        {
            if (!IsSuppressed)
            {
                throw new ActionGroupException("The lock of action group does not suppressed.");
            }
            else if (!IsSuppressor(suppressor))
            {
                throw new ActionGroupException("Only the suppressor context can relock the action group.");
            }
            else
            {
                this.suppressor = default;
                IsSuppressed = false;
            }
        }

        public bool HasRegistered(Context context)
        {
            return Contexts.Contains(context) ? true : IsSuppressor(context);
        }

        public bool IsSuppressor(Context context)
        {
            return context == suppressor;
        }

        public void Add(Context context)
        {
            if (IsSuppressed && !IsSuppressor(context))
            {
                throw new ActionGroupException($"The action group \"{Name}\" is suppressed by {suppressor.Name}.");
            }
            else if (Contexts.Contains(context))
            {
                throw new ActionGroupException("The action group is already locked by this context.");
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

        public IEnumerator<Context> GetEnumerator()
        {
            return Contexts.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Contexts.GetEnumerator();
        }
    }
}
