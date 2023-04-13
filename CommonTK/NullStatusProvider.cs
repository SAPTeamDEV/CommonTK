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
        public void Clear(string message)
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
        public void Write(string message)
        {
            
        }

        /// <inheritdoc/>
        public void Write(string message, ProgressBarType type)
        {
            Type = type;
        }
    }
}
