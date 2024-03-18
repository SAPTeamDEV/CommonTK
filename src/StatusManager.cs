using System;

namespace SAPTeam.CommonTK
{
    public abstract partial class StatusProvider
    {
        private static IStatusProvider provider = Empty;
        static object lockObj = new object();

        /// <summary>
        /// Gets or Sets the Global Status Provider.
        /// <para>
        /// Property setter Action Group: global.status
        /// </para>
        /// </summary>
        public static IStatusProvider Provider
        {
            get
            {
                return provider;
            }
            set
            {
                Context.QueryGroup(Context.ActionGroup(ActionScope.Global, "status"));
                lock (lockObj)
                {
                    if (provider != Empty)
                    {
                        provider.Dispose();
                    }

                    provider = value;
                }
            }
        }

        /// <summary>
        /// Writes new status to the global status provider.
        /// </summary>
        /// <param name="status">
        /// The new status text.
        /// </param>
        /// <param name="progress">
        /// The type of progress bar.
        /// This argument applied only when the global status provider is an instance of the <see cref="IProgressStatusProvider"/> interface.
        /// Otherwise it will be ignored.
        /// </param>
        /// <exception cref="InvalidOperationException"></exception>
        public static StatusIdentifier Write(string status, ProgressBarType progress = ProgressBarType.None)
        {
            if (Provider is IProgressStatusProvider ps)
            {
                return ps.Write(status, progress);
            }
            else
            {
                return Provider.Write(status);
            }
        }

        /// <summary>
        /// Advances progress of status.
        /// This method is usable only when the global status provider is an instance of the <see cref="IProgressStatusProvider"/> interface and type of progress bar is block.
        /// Otherwise it throws an <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="value">
        /// New value of progress bar. if value is -1, uses default step value of progress bar.
        /// </param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public static void Increment(int value = -1)
        {
            if (Provider is IProgressStatusProvider ps)
            {
                if (ps.Type == ProgressBarType.Block)
                {
                    ps.Increment(value);
                }
                else
                {
                    throw new NotImplementedException("The increment operation is only supported in the block progress bar.");
                }
            }
            else
            {
                throw new InvalidOperationException("Operation is not supported in the current status provider.");
            }
        }

        /// <summary>
        /// Clears the text of the global status provider.
        /// if class implements <see cref="IMultiStatusProvider"/> you can provide <paramref name="identifier"/>, otherwise it will be ignored.
        /// </summary>
        /// <param name="identifier">
        /// The identifier of the status that will be removed.
        /// </param>
        public static void Clear(StatusIdentifier identifier = default)
        {
            if (Provider is IMultiStatusProvider msp && identifier.IsValid())
            {
                msp.Clear(identifier);
            }
            else
            {
                Provider.Clear();
            }
        }

        /// <summary>
        /// Resets the global status provider.
        /// </summary>
        public static void Reset()
        {
            Provider = Empty;
        }
    }
}