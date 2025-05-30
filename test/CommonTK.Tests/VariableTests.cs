// ----------------------------------------------------------------------------
//  <copyright file="VariableTests.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

namespace SAPTeam.CommonTK.Tests;

public class VariableTests
{
    [Fact]
    public void GeneratorTest() => Assert.Equal("global.variable.process.test", Variable.VariableActionGroup("test", EnvironmentVariableTarget.Process));

    [Fact]
    public void ProcessVariableTest()
    {
        Assert.Null(Variable.GetVariable("test"));
        Assert.Throws<KeyNotFoundException>(() => new Variable("test", false));
        Variable test = new("test");
        Assert.Null(Variable.GetVariable("test"));
        test.Value = "test";
        Assert.Equal("test", Variable.GetVariable("test"));
        Assert.Equal("test", test.Value);
    }
}
