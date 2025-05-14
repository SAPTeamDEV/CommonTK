namespace SAPTeam.CommonTK.Status;

/// <summary>
/// Provides mechanisms for interacting with users and notifying them.
/// </summary>
public abstract partial class StatusProvider : IStatusProvider
{
    /// <summary>
    /// Represents the empty <see cref="IStatusProvider"/>.
    /// </summary>
    public static IStatusProvider Empty => EmptyStatusProvider.Instance;

    /// <inheritdoc/>
    public abstract void Clear();

    /// <inheritdoc/>
    public abstract void Dispose();

    /// <inheritdoc/>
    public abstract StatusIdentifier Write(string message);
}