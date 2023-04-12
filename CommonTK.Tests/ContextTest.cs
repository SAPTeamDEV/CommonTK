using SAPTeam.CommonTK;

using static SAPTeam.CommonTK.Context;

namespace SAPTeam.CommonTK.Tests
{
    public class ContextTest
    {
        [Fact]
        public void RegisterContextTest()
        {
            using (var context = new DummyContext(true))
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
            using (var context = new DummyContext(true))
            {
                Assert.Equal(context, GetContext<DummyContext>());
                Assert.Equal(context, GetContext(typeof(DummyContext)));
                Assert.Equal(context, GetContext(typeof(DummyContext).Name));
            }
        }

        [Fact]
        public void ContextArgumentHandlingTest()
        {
            using (var context = new DummyContext(true))
            {
                Assert.True(context.IsTest);
            }
        }

        [Fact]
        public void ContextFunctionalityTest()
        {
            Assert.Equal(InteractInterface.UI, Interface);

            using (var context = new DummyContext(true))
            {
                Assert.Equal(InteractInterface.UI, context.PreStat);
                Assert.Equal(InteractInterface.None, Interface);
            }

            Assert.Equal(InteractInterface.UI, Interface);
        }

        [Fact]
        public void DuplicateContextTest()
        {
            using (var context = new DummyContext(true))
            {
                Assert.Throws<InvalidOperationException>(() => new DummyContext(true));
            }
        }

        [Fact]
        public void RegisterTypeContextTest()
        {
            using (var context2 = Register<DummyContext2>())
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
            using (var context = new DummyContext(true, false))
            {
                Assert.False(Exists<DummyContext>());
                Assert.False(context.IsGlobal);
            }

            Assert.False(Exists<DummyContext>());
        }

        [Fact]
        public void RunningContextTest()
        {
            var context = Register<DummyContext>();
            Assert.True(context.IsRunning);
            context.Dispose();
            Assert.False(context.IsRunning);
        }

        [Fact]
        public void GlobalCotextTest()
        {
            var context = Register<DummyContext>();
            Assert.True(context.IsGlobal);
            context.Dispose();
            Assert.False(context.IsGlobal);
        }

        [Fact]
        public void MultiContextHandlingTest()
        {
            using (var context = new DummyContext())
            {
                Assert.True(Exists<DummyContext>());
                Assert.False(Exists<DummyContext2>());

                using (var context2 = new DummyContext2(true))
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
            using (var context2 = new DummyContext2())
            {
                Assert.False(context2.IsGlobal);
            }

            using (var context2 = Register<DummyContext2>())
            {
                Assert.True(context2.IsGlobal);
            }
        }

        [Fact]
        public void IsGlobalReliablityTest()
        {
            using (var context2 = new DummyContext2())
            {
                Assert.False(context2.IsGlobal);

                using (var context2b = new DummyContext2())
                {
                    Assert.False(context2b.IsGlobal);

                    using (var context2c = new DummyContext2(true))
                    {
                        Assert.False(context2.IsGlobal);
                        Assert.False(context2b.IsGlobal);
                        Assert.True(context2c.IsGlobal);
                    }
                }
            }
        }

        [Fact]
        public void ActionGroupTest()
        {
            using (var context = new DummyContext())
            {
                Assert.Throws<ActionGroupException>(() => Interface = InteractInterface.None);
                using (var context2 = new DummyContext2(true))
                {
                    Assert.Throws<ActionGroupException>(() => Interface = InteractInterface.None);
                }
                Assert.Throws<ActionGroupException>(() => Interface = InteractInterface.None);
            }
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
            using (var context3 = new DummyContext3())
            {
                Assert.Throws<ActionGroupException>(() => QueryGroup("application.test"));
                QueryGroup("application.test2");

                using (var context2 = new DummyContext2(true, true))
                {
                    Assert.Equal(ActionGroupState.Suppressed, QueryGroupState(ActionGroup(ActionScope.Application, "test")));
                    Assert.Equal(ActionGroupState.Locked, QueryGroupState(ActionGroup(ActionScope.Application, "test2")));
                    QueryGroup("application.test");
                    Assert.Throws<ActionGroupException>(() => QueryGroup("application.test2"));
                    Assert.Equal(ActionGroupState.Free, QueryGroupState(ActionGroup(ActionScope.Application, "test4")));

                    using (var exposer = new ProtectedExposerContext())
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
            using (var exposer = new ProtectedExposerContext(false))
            {
                Assert.Throws<ActionGroupException>(() => exposer.Lock("application.test5"));
                Assert.Throws<ActionGroupException>(() => exposer.Suppress("application.test5"));
            }
        }

        [Fact]
        public void InitializerCrashTest()
        {
            Assert.Throws<Exception>(() => new DummyContext2(throwOnInitializer: true));
            Assert.False(Exists<DummyContext2>());
            Assert.Throws<KeyNotFoundException>(() => GetContext<DummyContext2>());

            using (var context2 = new DummyContext2(earlyRegisterAction: true))
            {
                Assert.True(Exists<DummyContext2>());
            }

            using (var context2 = new DummyContext2(throwOnRegisterAction: true))
            {
                Assert.Throws<ActionGroupException>(() => new DummyContext3());
                Assert.False(Exists<DummyContext3>());
                Assert.Throws<KeyNotFoundException>(() => GetContext<DummyContext3>());

                Assert.True(Exists<DummyContext2>());
            }
        }

        [Fact]
        public void FinalizerCrashTest()
        {
            var context2 = new DummyContext2(throwOnFinalizer: true);
            Assert.True(context2.IsGlobal);
            Assert.True(context2.IsRunning);
            Assert.Throws<Exception>(() => context2.Dispose());
            context2.Dispose();
            Assert.False(context2.IsGlobal);
            Assert.False(context2.IsRunning);
            Assert.False(Exists<DummyContext2>());
            Assert.Throws<KeyNotFoundException>(() => GetContext<DummyContext2>());
        }
    }
}