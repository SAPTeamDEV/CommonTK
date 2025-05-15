// ----------------------------------------------------------------------------
//  <copyright file="NullStatusProvider.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

using SAPTeam.CommonTK.Status;

namespace SAPTeam.CommonTK.Tests;

/// <summary>
/// Represents a null status provider that accepts any requests.
/// </summary>
public class NullStatusProvider : IProgressStatusProvider, IMultiStatusProvider
{
    /// <inheritdoc/>
    public ProgressBarType Type { get; set; }

    /// <inheritdoc/>
    public void Clear(StatusIdentifier identifier)
    {

    }

    /// <inheritdoc/>
    public void Clear()
    {

    }

    /// <inheritdoc/>
    public void Dispose()
    {

    }

    /// <inheritdoc/>
    public void Increment(int value)
    {

    }

    /// <inheritdoc/>
    public StatusIdentifier Write(string message) => StatusIdentifier.Empty;

    /// <inheritdoc/>
    public StatusIdentifier Write(string message, ProgressBarType type)
    {
        Type = type;
        return StatusIdentifier.Empty;
    }
}
