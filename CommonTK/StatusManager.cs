using System;

namespace SAPTeam.CommonTK
{
    public abstract partial class StatusProvider
    {
        private static IStatusProvider provider = Empty;
        static object lockObj = new object();

        /// <summary>
        /// Gets or Sets the Global Status Provider.
        /// </summary>
        public static IStatusProvider Provider
        {
            get
            {
                return provider;
            }
            set
            {
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
        /// Otherwise it throws an <see cref="InvalidOperationException"/>.
        /// </param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void Write(string status, ProgressBarType progress = ProgressBarType.None)
        {
            if (progress == ProgressBarType.None)
            {
                Provider.Write(status);
            }
            else if (Provider is IProgressStatusProvider ps)
            {
                ps.Write(status, progress);
            }
            else
            {
                throw new InvalidOperationException("Operation is not supported in the current status provider.");
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
        /// if class implements <see cref="IMultiStatusProvider"/> you can provide <paramref name="message"/>, otherwise it will ignored.
        /// </summary>
        /// <param name="message">
        /// A specific message that will be removed.
        /// </param>
        public static void Clear(string message = null)
        {
            if (Provider is IMultiStatusProvider msp && message != null)
            {
                msp.Clear(message);
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