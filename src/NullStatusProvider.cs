using System;
using System.Collections.Generic;
using System.Text;

namespace SAPTeam.CommonTK
{
    /// <summary>
    /// Represents a null status provider that accepts any requests.
    /// </summary>
    public class NullStatusProvider : IProgressStatusProvider, IMultiStatusProvider
    {
        /// <inheritdoc/>
        public ProgressBarType Type { get; set; }

        /// <inheritdoc/>
        public void Clear(StatusIdentifier identifier)
        {
            
        }

        /// <inheritdoc/>
        public void Clear()
        {
            
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            
        }

        /// <inheritdoc/>
        public void Increment(int value)
        {
            
        }

        /// <inheritdoc/>
        public StatusIdentifier Write(string message)
        {
            return StatusIdentifier.Empty;
        }

        /// <inheritdoc/>
        public StatusIdentifier Write(string message, ProgressBarType type)
        {
            Type = type;
            return StatusIdentifier.Empty;
        }
    }
}
