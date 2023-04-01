using System;

namespace SAPTeam.CommonTK
{
    /// <summary>
    /// Provides mechanisms for interacting with users and notifying them.
    /// </summary>
    public abstract class StatusProvider : IStatusProvider
    {
        /// <summary>
        /// Represents the empty <see cref="StatusProvider"/>.
        /// </summary>
        public static IStatusProvider Empty { get; }

        /// <summary>
        /// Gets or Sets the Global Status Provider that can be controlled via <see cref="Interact"/> <see langword="static"/>methods.
        /// </summary>
        public static IStatusProvider Current { get; set; } = Empty;

        /// <inheritdoc/>
        public void Clear() { }

        /// <inheritdoc/>
        public void Write(string message) { }
    }
}