using SAPTeam.CommonTK;

namespace CommonTK.Tests
{
    public class ContextTest
    {
        [Fact]
        public void RegisterContextTest()
        {
            using (var context = new DummyContext())
            {
                Assert.True(Context.Current.HasContext<DummyContext>());
                Assert.True(Context.Current.HasContext(typeof(DummyContext)));
                Assert.True(Context.Current.HasContext(typeof(DummyContext).Name));
                Assert.True(Context.Current.HasContext("DummyContext"));
                Assert.False(Context.Current.HasContext("DummyContext2"));
            }

            Assert.False(Context.Current.HasContext<DummyContext>());
            Assert.False(Context.Current.HasContext(typeof(DummyContext)));
            Assert.False(Context.Current.HasContext(typeof(DummyContext).Name));
        }

        [Fact]
        public void GetContextTest()
        {
            using (var context = new DummyContext())
            {
                Assert.Equal(Context.Current.GetContext<DummyContext>(), context);
                Assert.Equal(Context.Current.GetContext(typeof(DummyContext)), context);
                Assert.Equal(Context.Current.GetContext(typeof(DummyContext).Name), context);
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
    }
}