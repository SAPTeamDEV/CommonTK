using System;

namespace SAPTeam.CommonTK;

/// <summary>
/// Implements an <see cref="IStatusProvider"/> with multi status processing support.
/// <para>
/// Classes that implements this interface can set the <see cref="IStatusProvider.Clear()"/> to throw <see cref="NotImplementedException"/>.
/// But implementing the <see cref="IDisposable.Dispose()"/> is mandatory for handling the status changing events.
/// Also these classes must implement a way to store statuses.
/// </para>
/// </summary>
public interface IMultiStatusProvider : IStatusProvider
{
    /// <summary>
    /// Clears the status associated with <paramref name="identifier"/> from Status Provider.
    /// </summary>
    /// <param name="identifier">
    /// The identifier of the status.
    /// </param>
    void Clear(StatusIdentifier identifier);
}
