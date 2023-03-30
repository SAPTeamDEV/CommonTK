﻿using System;

namespace SAPTeam.CommonTK
{
    /// <summary>
    /// Implements an <see cref="IStatusProvider"/> with multi status processing support.
    /// <para>
    /// Classes that implements this interface can set the <see cref="IStatusProvider.Clear()"/> to throw <see cref="NotImplementedException"/>.
    /// Also these classes must implement a way to store statuses.
    /// </para>
    /// </summary>
    public interface IMultiStatusProvider : IStatusProvider
    { 
        /// <summary>
        /// Clears the <paramref name="message"/> from Status Provider.
        /// </summary>
        /// <param name="message">
        /// An existing status text.
        /// </param>
        void Clear(string message);
    }
}
