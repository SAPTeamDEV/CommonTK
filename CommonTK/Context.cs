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
        /// Gets the name identifier of this context.
        /// </summary>
        public string Name => GetType().Name;

        /// <summary>
        /// Gets a value indicating whether this context is globally registered.
        /// </summary>
        public bool IsGlobal => contexts.ContainsValue(this);

        /// <summary>
        /// Gets a value indicating whether this context is running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Initializes a new context.
        /// This method must be called in the end of context constructor.
        /// </summary>
        /// <param name="global">
        /// Determines whether to register this context globally.
        /// </param>
        /// <exception cref="InvalidOperationException"></exception>
        protected void Initialize(bool global)
        {
            if (global)
            {
                if (Exists(Name))
                {
                    throw new InvalidOperationException("An instance of this context already exists");
                }

                lock (lockObj)
                {
                    contexts[Name] = this;
                }
            }

            if (!IsRunning)
            {
                IsRunning = true;
                CreateContext();
            }
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
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// Determines the dispose stage.
        /// </param>
        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (IsRunning)
                {
                    DisposeContext();
                    IsRunning = false;
                }

                if (IsGlobal)
                {
                    lock (lockObj)
                    {
                        contexts.Remove(Name);
                    }
                }

                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}