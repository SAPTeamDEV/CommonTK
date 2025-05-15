// ----------------------------------------------------------------------------
//  <copyright file="DummyContext3.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

namespace SAPTeam.CommonTK.Tests;

internal class DummyContext3 : Context
{
    public override string[] Groups { get; } = new string[]
    {
        "application.test",
        "application.test5"
    };

    public DummyContext3()
    {
        Initialize(true);
    }

    protected override void CreateContext()
    {

    }

    protected override void DisposeContext()
    {

    }
}
