﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;

namespace SAPTeam.CommonTK
{
    /// <summary>
    /// Stores an integer value as identifier for an status message.
    /// </summary>
    public struct StatusIdentifier : IDisposable
    {
        /// <summary>
        /// Returns an invalid empty identifier
        /// </summary>
        public static StatusIdentifier Empty = default;

        /// <summary>
        /// Gets the numerical identifier of this instance.
        /// </summary>
        public int Identifier {  get; }

        IStatusProvider parent;
        static Random random = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusIdentifier"/> class.
        /// </summary>
        /// <param name="parent">The parent Status Provider that this instance will associate to it.</param>
        /// <param name="identifier">The identifier of the status message.</param>
        public StatusIdentifier(IStatusProvider parent, int identifier)
        {
            this.parent = parent;
            Identifier = identifier;
        }

        /// <summary>
        /// Generates a new <see cref="StatusIdentifier"/> with a random identifier.
        /// </summary>
        /// <param name="parent">The identifier of the status message.</param>
        /// <returns></returns>
        public static StatusIdentifier Generate(IStatusProvider parent) => new StatusIdentifier(parent, random.Next(10000));

        /// <summary>
        /// Asks the parent status provider to remove the status message.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
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

        /// <summary>
        /// Checks whether this identifier object is valid or not.
        /// </summary>
        /// <returns><see langword="true"/> if the <see cref="StatusIdentifier"/> has parent, otherwise it will return <see langword="false"/>.</returns>
        public bool IsValid()
        {
            return parent != null;
        }

        /// <summary>
        /// Converts an instance of <see cref="StatusIdentifier"/> to <see cref="int"/>.
        /// </summary>
        /// <param name="x">The source object.</param>
        public static implicit operator int(StatusIdentifier x)
        {
            return x.Identifier;
        }
    }
}
