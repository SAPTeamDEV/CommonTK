using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SAPTeam.CommonTK
{
    internal struct ActionGroupContainer : IEnumerable<Context>
    {
        public Context this[int index] { get => Contexts[index]; set => Contexts[index] = value; }

        public string Name { get; }

        public ActionGroupState State => IsSuppressed ? ActionGroupState.Suppressed : Contexts.Count > 0 ? ActionGroupState.Locked : ActionGroupState.Free;

        public List<Context> Contexts { get; }

        private bool suppress;

        public bool IsSuppressed
        {
            get => suppress;
            set
            {
                if (suppress && value == true)
                {
                    throw new ActionGroupException("The action group is already supressed.");
                }
                else
                {
                    suppress = value;
                }
            }
        }

        public int Count => Contexts.Count;

        public ActionGroupContainer(string name)
        {
            Name = name;
            Contexts = new List<Context>();
            suppress = false;
        }

        public void Add(Context context)
        {
            if (Contexts.Contains(context))
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
