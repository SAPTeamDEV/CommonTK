using System;

namespace SAPTeam.CommonTK
{
    /// <summary>
    /// Provides mechanisms for interacting with users and notifying them.
    /// </summary>
    public abstract partial class StatusProvider : IStatusProvider
    {
        /// <summary>
        /// Represents the empty <see cref="StatusProvider"/>.
        /// </summary>
        public static IStatusProvider Empty { get; }

        /// <inheritdoc/>
        public void Clear() { }

        /// <inheritdoc/>
        public void Dispose() { }

        /// <inheritdoc/>
        public void Write(string message) { }
    }
}