using System;

namespace SAPTeam.CommonTK.Status;

/// <summary>
/// Provides mechanisms for interacting with users and notifying them.
/// </summary>
public interface IStatusProvider : IDisposable
{
    /// <summary>
    /// Clears the text of Status Provider
    /// </summary>
    void Clear();

    /// <summary>
    /// Writes new status to the Status Provider.
    /// </summary>
    /// <param name="message">
    /// New status of Status Provider.
    /// </param>
    StatusIdentifier Write(string message);
}