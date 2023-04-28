using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SAPTeam.CommonTK
{
    internal class ActionGroupContainer : IEnumerable<Context>
    {
        public Context this[int index] { get => Contexts[index]; }

        public string Name { get; }

        public ActionGroupState State => IsSuppressed ? ActionGroupState.Suppressed : Contexts.Count > 0 ? ActionGroupState.Locked : ActionGroupState.Free;

        List<Context> Contexts { get; }

        Context suppressor;

        public bool IsSuppressed { get; private set; }
        public bool IsLocked => State == ActionGroupState.Locked;

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
