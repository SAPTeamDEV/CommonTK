namespace SAPTeam.CommonTK
{
    public abstract class Context : IContext, IDisposable
    {
        private bool disposedValue;

        public static ContextContainer Current { get; } = new();
        public static InteractInterface Interface { get; set; }

        public Context(params dynamic[] args)
        {
            if (args.Length > 0)
            {
                ArgsHandler(args);
            }
            Current.SetContect(this);
            CreateContext();
        }

        protected abstract void CreateContext();
        protected abstract void DisposeContext();
        protected abstract void ArgsHandler(dynamic[] args);

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

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~Context()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}