// ----------------------------------------------------------------------------
//  <copyright file="ProtectedExposerContext.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

namespace SAPTeam.CommonTK.Tests;

internal class ProtectedExposerContext : Context
{
    public override string[] Groups { get; } = new string[] { "application.test4" };

    public override string[] NeutralGroups { get; } = new string[] { "application.test", "application.test5", "application.test6" };

    public ProtectedExposerContext(bool global = true)
    {
        Initialize(global);
    }

    protected override void CreateContext()
    {

    }

    protected override void DisposeContext()
    {

    }

    public void Suppress(string group) => SuppressLock(group);
    public void Lock(string group) => LockGroup(group);
}
