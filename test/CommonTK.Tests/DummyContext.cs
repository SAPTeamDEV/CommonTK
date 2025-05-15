// ----------------------------------------------------------------------------
//  <copyright file="DummyContext.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

namespace SAPTeam.CommonTK.Tests;

internal class DummyContext : Context
{
    public bool IsTest { get; set; }
    public InteractInterface PreStat { get; set; }

    public override string[] Groups { get; } = new string[] { "global.interface" };

    public DummyContext() : this(false)
    {

    }
    public DummyContext(bool isTest = false, bool isGlobal = true)
    {
        IsTest = isTest;
        Initialize(isGlobal);
    }

    protected override void CreateContext()
    {
        PreStat = Application.Interface;
        Application.Interface = InteractInterface.None;
    }

    protected override void DisposeContext() => Application.Interface = PreStat;
}
