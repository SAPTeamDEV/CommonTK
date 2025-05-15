// ----------------------------------------------------------------------------
//  <copyright file="EmptyStatusProvider.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

namespace SAPTeam.CommonTK.Status;

internal class EmptyStatusProvider : IStatusProvider
{
    public static EmptyStatusProvider Instance = new();

    public void Clear()
    {

    }

    public void Dispose()
    {

    }

    public StatusIdentifier Write(string message) => StatusIdentifier.Empty;
}
