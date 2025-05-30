// ----------------------------------------------------------------------------
//  <copyright file="IProgressStatusProvider.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

namespace SAPTeam.CommonTK.Status;

/// <summary>
/// Implements an <see cref="IStatusProvider"/> with progress bar support.
/// <para>
/// Classes that implements this interface can set the <see cref="IStatusProvider.Write(string)"/> to throw <see cref="NotImplementedException"/>.
/// </para>
/// </summary>
public interface IProgressStatusProvider : IStatusProvider
{
    /// <summary>
    /// Gets or Sets a value indicating the type of status progress bar.
    /// </summary>
    ProgressBarType Type { get; set; }

    /// <summary>
    /// Writes the new status text with specified <paramref name="type"/> to the Status Provider.
    /// </summary>
    /// <param name="message">
    /// New status text of Status Provider.
    /// </param>
    /// <param name="type">
    /// The type of progress bar.
    /// </param>
    StatusIdentifier Write(string message, ProgressBarType type);

    /// <summary>
    /// Increases the progress value of <see cref="ProgressBarType.Block"/> Progress bar by <paramref name="value"/>.
    /// </summary>
    /// <param name="value">
    /// Amount of percentage that be added to Progress bar progress value.
    /// </param>
    void Increment(int value);
}
