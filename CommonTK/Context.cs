using System;
using System.Runtime.InteropServices;

namespace SAPTeam.CommonTK
{
    /// <summary>
    /// Defines a disposable Context that can be used to set some properties or do particular actions in a specified <see cref="Context"/>.
    /// </summary>
    public abstract partial class Context : IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class.
        /// </summary>
        /// <param name="args">
        /// Arguments that be passed to <see cref="ArgsHandler(dynamic[])"/> method.
        /// </param>
        public Context(params dynamic[] args)
        {
            if (args.Length > 0)
            {
                ArgsHandler(args);
            }
            SetContext(this);
            CreateContext();
        }

        /// <summary>
        /// This method called after the constructor has successfully registers the Context.
        /// </summary>
        protected abstract void CreateContext();

        /// <summary>
        /// This method called when the context is disposing.
        /// </summary>
        protected abstract void DisposeContext();

        /// <summary>
        /// This method called only when that an argument or arguments passed to the constructor.
        /// </summary>
        /// <param name="args">
        /// An array of arguments that passed to the constructor.
        /// </param>
        protected abstract void ArgsHandler(dynamic[] args);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// Determines the dispose stage.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                DisposeContext(this);
                DisposeContext();

                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}