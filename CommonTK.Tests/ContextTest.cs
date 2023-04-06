using SAPTeam.CommonTK;

namespace SAPTeam.CommonTK.Tests
{
    public class ContextTest
    {
        [Fact]
        public void RegisterContextTest()
        {
            using (var context = new DummyContext(true))
            {
                Assert.True(Context.Exists<DummyContext>());
                Assert.True(Context.Exists(typeof(DummyContext)));
                Assert.True(Context.Exists(typeof(DummyContext).Name));
                Assert.True(Context.Exists("DummyContext"));
                Assert.False(Context.Exists("DummyContext2"));
            }

            Assert.False(Context.Exists<DummyContext>());
            Assert.False(Context.Exists(typeof(DummyContext)));
            Assert.False(Context.Exists(typeof(DummyContext).Name));
        }

        [Fact]
        public void GetContextTest()
        {
            using (var context = new DummyContext(true))
            {
                Assert.Equal(context, Context.GetContext<DummyContext>());
                Assert.Equal(context, Context.GetContext(typeof(DummyContext)));
                Assert.Equal(context, Context.GetContext(typeof(DummyContext).Name));
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
            Assert.Equal(InteractInterface.UI, Context.Interface);

            using (var context = new DummyContext(true))
            {
                Assert.Equal(InteractInterface.UI, context.PreStat);
                Assert.Equal(InteractInterface.None, Context.Interface);
            }

            Assert.Equal(InteractInterface.UI, Context.Interface);
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
            using (var context = Context.Register<DummyContext>())
            {
                Assert.True(Context.Exists<DummyContext>());
                Assert.Equal(context, Context.GetContext<DummyContext>());
            }

            Assert.False(Context.Exists<DummyContext>());
        }

        [Fact]
        public void PrivateContextTest()
        {
            using (var context = new DummyContext(true, false))
            {
                Assert.False(Context.Exists<DummyContext>());
                Assert.False(context.IsGlobal);
            }

            Assert.False(Context.Exists<DummyContext>());
        }

        [Fact]
        public void RunningContextTest()
        {
            var context = Context.Register<DummyContext>();
            Assert.True(context.IsRunning);
            context.Dispose();
            Assert.False(context.IsRunning);
        }

        [Fact]
        public void GlobalCotextTest()
        {
            var context = Context.Register<DummyContext>();
            Assert.True(context.IsGlobal);
            context.Dispose();
            Assert.False(context.IsGlobal);
        }

        [Fact]
        public void MultiContextHandlingTest()
        {
            using (var context = new DummyContext())
            {
                Assert.True(Context.Exists<DummyContext>());
                Assert.False(Context.Exists<DummyContext2>());

                using (var context2 = new DummyContext2(true))
                {
                    Assert.True(Context.Exists<DummyContext>());
                    Assert.True(Context.Exists<DummyContext2>());
                }

                Assert.True(Context.Exists<DummyContext>());
                Assert.False(Context.Exists<DummyContext2>());
            }

            Assert.False(Context.Exists<DummyContext>());
            Assert.False(Context.Exists<DummyContext2>());
        }

        [Fact]
        public void RegisterPrivateContextTest()
        {
            using (var context2 = new DummyContext2())
            {
                Assert.False(context2.IsGlobal);
            }

            using (var context2 = Context.Register<DummyContext2>())
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
                Assert.Throws<ActionGroupException>(() => Context.Interface = InteractInterface.None);
                using (var context2 = new DummyContext2(true))
                {
                    Assert.Throws<ActionGroupException>(() => Context.Interface = InteractInterface.None);
                }
                Assert.Throws<ActionGroupException>(() => Context.Interface = InteractInterface.None);
            }
        }

        [Fact]
        public void GroupGeneratorTest()
        {
            Assert.Equal("application.test", Context.ActionGroup(ActionScope.Application, "TeSt"));
            Assert.Equal("application.tes_t", Context.ActionGroup(ActionScope.Application, "TeS t"));
            Assert.Equal("application.contexttest", Context.ActionGroup(ActionScope.Application, GetType().Name));
            Assert.Equal("application.t.e.s.t", Context.ActionGroup(ActionScope.Application, "T", "e", "S", "t"));
        }
    }
}