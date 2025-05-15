// ----------------------------------------------------------------------------
//  <copyright file="DummyStatusProvider.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

using System.Text;

using SAPTeam.CommonTK.Status;

namespace SAPTeam.CommonTK.Tests;

public sealed class DummyStatusProvider : IStatusProvider
{
    public StringBuilder Input { get; }

    public DummyStatusProvider() => Input = new StringBuilder();

    public void Clear() => Input.Clear();

    public void Dispose() => Clear();

    public StatusIdentifier Write(string message)
    {
        Input.Append(message);
        return new StatusIdentifier(this, 0);
    }
}
