using System;

namespace SAPTeam.CommonTK
{
    /// <summary>
    /// Base class of Context classes. this abstract class initializes and registers the <see cref="Context"/> instances into <see cref="Context.Current"/>.
    /// <para>
    /// Don't replace the class constructor, either override <see langword="abstract"/> methods.
    /// </para>
    /// </summary>
    public abstract class Context : IContext
    {
        private bool disposedValue;

        /// <summary>
        /// Gets the Global Context Container.
        /// </summary>
        public static ContextContainer Current { get; } = new ContextContainer();

        /// <summary>
        /// Gets or Sets the process interaction Interface.
        /// <para>
        /// This value must be set by application for the first time.
        /// </para>
        /// </summary>
        public static InteractInterface Interface { get; set; }

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
            Current.SetContect(this);
            CreateContext();
        }

        /// <summary>
        /// This method called after the constructor has successfully registers the Context into <see cref="Current"/>.
        /// </summary>
        protected abstract void CreateContext();

        /// <summary>
        /// This method called after that the Context has unloaded in <see cref="Current"/>.
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
                Current.DisposeContext(this);
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