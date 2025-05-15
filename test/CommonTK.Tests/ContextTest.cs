// ----------------------------------------------------------------------------
//  <copyright file="ContextTest.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

using SAPTeam.CommonTK.ExecutionPolicy;

using static SAPTeam.CommonTK.Context;

namespace SAPTeam.CommonTK.Tests;

public class ContextTest
{
    [Fact]
    public void RegisterContextTest()
    {
        using (DummyContext context = new(true))
        {
            Assert.True(Exists<DummyContext>());
            Assert.True(Exists(typeof(DummyContext)));
            Assert.True(Exists(typeof(DummyContext).Name));
            Assert.True(Exists("DummyContext"));
            Assert.False(Exists("DummyContext2"));
        }

        Assert.False(Exists<DummyContext>());
        Assert.False(Exists(typeof(DummyContext)));
        Assert.False(Exists(typeof(DummyContext).Name));
    }

    [Fact]
    public void GetContextTest()
    {
        using DummyContext context = new(true);
        Assert.Equal(context, GetContext<DummyContext>());
        Assert.Equal(context, GetContext(typeof(DummyContext)));
        Assert.Equal(context, GetContext(typeof(DummyContext).Name));
    }

    [Fact]
    public void ContextArgumentHandlingTest()
    {
        using DummyContext context = new(true);
        Assert.True(context.IsTest);
    }

    [Fact]
    public void ContextFunctionalityTest()
    {
        Assert.Equal(InteractInterface.UI, Application.Interface);

        using (DummyContext context = new(true))
        {
            Assert.Equal(InteractInterface.UI, context.PreStat);
            Assert.Equal(InteractInterface.None, Application.Interface);
        }

        Assert.Equal(InteractInterface.UI, Application.Interface);
    }

    [Fact]
    public void DuplicateContextTest()
    {
        using DummyContext context = new(true);
        Assert.Throws<InvalidOperationException>(() => new DummyContext(true));
    }

    [Fact]
    public void RegisterTypeContextTest()
    {
        using (DummyContext2 context2 = Register<DummyContext2>())
        {
            Assert.True(Exists<DummyContext2>());
            Assert.Equal(context2, GetContext<DummyContext2>());
            Assert.Throws<ActionGroupException>(() => QueryGroup("global.interface"));
        }

        Assert.False(Exists<DummyContext>());
        QueryGroup("global.interface");
    }

    [Fact]
    public void PrivateContextTest()
    {
        using (DummyContext context = new(true, false))
        {
            Assert.False(Exists<DummyContext>());
            Assert.False(context.IsGlobal);
        }

        Assert.False(Exists<DummyContext>());
    }

    [Fact]
    public void RunningContextTest()
    {
        DummyContext context = Register<DummyContext>();
        Assert.True(context.IsRunning);
        context.Dispose();
        Assert.False(context.IsRunning);
    }

    [Fact]
    public void GlobalContextTest()
    {
        DummyContext context = Register<DummyContext>();
        Assert.True(context.IsGlobal);
        context.Dispose();
        Assert.False(context.IsGlobal);
    }

    [Fact]
    public void MultiContextHandlingTest()
    {
        using (DummyContext context = new())
        {
            Assert.True(Exists<DummyContext>());
            Assert.False(Exists<DummyContext2>());

            using (DummyContext2 context2 = new(true))
            {
                Assert.True(Exists<DummyContext>());
                Assert.True(Exists<DummyContext2>());
            }

            Assert.True(Exists<DummyContext>());
            Assert.False(Exists<DummyContext2>());
        }

        Assert.False(Exists<DummyContext>());
        Assert.False(Exists<DummyContext2>());
    }

    [Fact]
    public void RegisterPrivateContextTest()
    {
        using (DummyContext2 context2 = new())
        {
            Assert.False(context2.IsGlobal);
        }

        using (DummyContext2 context2 = Register<DummyContext2>())
        {
            Assert.True(context2.IsGlobal);
        }
    }

    [Fact]
    public void IsGlobalReliabilityTest()
    {
        using DummyContext2 context2 = new();
        Assert.False(context2.IsGlobal);

        using DummyContext2 context2b = new();
        Assert.False(context2b.IsGlobal);

        using DummyContext2 context2c = new(true);
        Assert.False(context2.IsGlobal);
        Assert.False(context2b.IsGlobal);
        Assert.True(context2c.IsGlobal);
    }

    [Fact]
    public void ActionGroupTest()
    {
        using DummyContext context = new();
        Assert.Throws<ActionGroupException>(() => Application.Interface = InteractInterface.None);
        using (DummyContext2 context2 = new(true))
        {
            Assert.Throws<ActionGroupException>(() => Application.Interface = InteractInterface.None);
        }

        Assert.Throws<ActionGroupException>(() => Application.Interface = InteractInterface.None);
    }

    [Fact]
    public void GroupGeneratorTest()
    {
        Assert.Equal("application.test", ActionGroup(ActionScope.Application, "TeSt"));
        Assert.Equal("application.tes_t", ActionGroup(ActionScope.Application, "TeS t"));
        Assert.Equal("application.contexttest", ActionGroup(ActionScope.Application, GetType().Name));
        Assert.Equal("application.t.e.s.t", ActionGroup(ActionScope.Application, "T", "e", "S", "t"));
    }

    [Fact]
    public void ProtectedAPITest()
    {
        using (DummyContext3 context3 = new())
        {
            Assert.Throws<ActionGroupException>(() => QueryGroup("application.test"));
            QueryGroup("application.test2");

            using (DummyContext2 context2 = new(true, true))
            {
                Assert.Equal(ActionGroupState.Suppressed, QueryGroupState(ActionGroup(ActionScope.Application, "test")));
                Assert.Equal(ActionGroupState.Locked, QueryGroupState(ActionGroup(ActionScope.Application, "test2")));
                QueryGroup("application.test");
                Assert.Throws<ActionGroupException>(() => QueryGroup("application.test2"));
                Assert.Equal(ActionGroupState.Free, QueryGroupState(ActionGroup(ActionScope.Application, "test4")));

                using (ProtectedExposerContext exposer = new())
                {
                    Assert.Equal(ActionGroupState.Locked, QueryGroupState(ActionGroup(ActionScope.Application, "test4")));
                    Assert.Throws<ActionGroupException>(() => exposer.Suppress("application.test"));
                    Assert.Throws<ActionGroupException>(() => exposer.Suppress("application.test7"));
                    Assert.Throws<ActionGroupException>(() => exposer.Lock("application.test"));
                    Assert.Throws<ActionGroupException>(() => exposer.Lock("application.test3"));
                    Assert.Throws<ActionGroupException>(() => exposer.Lock("application.test4"));
                    exposer.Suppress("application.test4");
                    Assert.Equal(ActionGroupState.Suppressed, QueryGroupState(ActionGroup(ActionScope.Application, "test4")));
                    exposer.Lock("application.test4");
                    Assert.Equal(ActionGroupState.Locked, QueryGroupState(ActionGroup(ActionScope.Application, "test4")));
                    exposer.Suppress("application.test4");
                    exposer.Suppress("application.test5");
                    exposer.Suppress("application.test6");
                    exposer.Dispose();
                    Assert.Throws<ActionGroupException>(() => exposer.Lock("application.test"));
                    Assert.Throws<ActionGroupException>(() => exposer.Suppress("application.test"));
                }

                Assert.Equal(ActionGroupState.Free, QueryGroupState(ActionGroup(ActionScope.Application, "test4")));
                Assert.Equal(ActionGroupState.Locked, QueryGroupState(ActionGroup(ActionScope.Application, "test5")));
                Assert.Equal(ActionGroupState.Suppressed, QueryGroupState(ActionGroup(ActionScope.Application, "test")));
            }

            Assert.Equal(ActionGroupState.Free, QueryGroupState(ActionGroup(ActionScope.Application, "test2")));
        }

        Assert.Equal(ActionGroupState.Free, QueryGroupState(ActionGroup(ActionScope.Application, "test")));
        Assert.Equal(ActionGroupState.Free, QueryGroupState(ActionGroup(ActionScope.Application, "utest")));
    }

    [Fact]
    public void PrivateContextActionGroupTest()
    {
        using ProtectedExposerContext exposer = new(false);
        Assert.Throws<ActionGroupException>(() => exposer.Lock("application.test5"));
        Assert.Throws<ActionGroupException>(() => exposer.Suppress("application.test5"));
    }

    [Fact]
    public void InitializerCrashTest()
    {
        Assert.Throws<Exception>(() => new DummyContext2(throwOnInitializer: true));
        Assert.False(Exists<DummyContext2>());
        Assert.Throws<KeyNotFoundException>(GetContext<DummyContext2>);

        using (DummyContext2 context2 = new(earlyRegisterAction: true))
        {
            Assert.True(Exists<DummyContext2>());
        }

        using (DummyContext2 context2 = new(throwOnRegisterAction: true))
        {
            Assert.Throws<ActionGroupException>(() => new DummyContext3());
            Assert.False(Exists<DummyContext3>());
            Assert.Throws<KeyNotFoundException>(GetContext<DummyContext3>);

            Assert.True(Exists<DummyContext2>());
        }
    }

    [Fact]
    public void FinalizerCrashTest()
    {
        DummyContext2 context2 = new(throwOnFinalizer: true);
        Assert.True(context2.IsGlobal);
        Assert.True(context2.IsRunning);
        Assert.Throws<Exception>(context2.Dispose);
        context2.Dispose();
        Assert.False(context2.IsGlobal);
        Assert.False(context2.IsRunning);
        Assert.False(Exists<DummyContext2>());
        Assert.Throws<KeyNotFoundException>(GetContext<DummyContext2>);
    }

    [Fact]
    public void EnvironmentTest()
    {
        Assert.NotNull(Application.FullPath);
        Assert.Equal("testhost", Application.Name);
        Assert.NotNull(Application.BaseDirectory);
    }
}
