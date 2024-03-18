using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;

namespace SAPTeam.CommonTK
{
    public struct StatusIdentifier : IDisposable
    {
        public static StatusIdentifier Empty = default;

        public int Identifier {  get; set; }

        IStatusProvider parent;
        static Random random = new Random();

        public StatusIdentifier(IStatusProvider parent, int identifier)
        {
            this.parent = parent;
            Identifier = identifier;
        }

        public static StatusIdentifier Generate(IStatusProvider parent) => new StatusIdentifier(parent, random.Next(10000));

        /// <inheritdoc/>
        public void Dispose()
        {
            if (parent == null)
            {
                throw new InvalidOperationException("This identifier does not associated with any status providers");
            }

            if (parent is IMultiStatusProvider msp)
            {
                msp.Clear(this);
            }
            else
            {
                parent.Clear();
            }

            parent = null;
        }

        public bool IsValid()
        {
            return parent != null;
        }

        public static implicit operator int(StatusIdentifier x)
        {
            return x.Identifier;
        }
    }
}
