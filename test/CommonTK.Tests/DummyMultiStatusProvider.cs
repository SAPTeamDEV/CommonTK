// ----------------------------------------------------------------------------
//  <copyright file="DummyMultiStatusProvider.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

using SAPTeam.CommonTK.Status;

namespace SAPTeam.CommonTK.Tests;

public sealed class DummyMultiStatusProvider : IMultiStatusProvider
{
    public Dictionary<int, string> Input { get; }

    public DummyMultiStatusProvider() => Input = [];

    public void Clear() => Input.Clear();

    public void Dispose() => Clear();

    public StatusIdentifier Write(string message)
    {
        StatusIdentifier id = StatusIdentifier.Generate(this);
        Input[id] = message;
        return id;
    }

    public void Clear(StatusIdentifier identifier) => Input.Remove(identifier);
}
