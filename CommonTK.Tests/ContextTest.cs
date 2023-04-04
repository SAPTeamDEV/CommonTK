using SAPTeam.CommonTK;

namespace CommonTK.Tests
{
    public class ContextTest
    {
        [Fact]
        public void RegisterContextTest()
        {
            using (var context = new DummyContext(true))
            {
                Assert.True(Context.HasContext<DummyContext>());
                Assert.True(Context.HasContext(typeof(DummyContext)));
                Assert.True(Context.HasContext(typeof(DummyContext).Name));
                Assert.True(Context.HasContext("DummyContext"));
                Assert.False(Context.HasContext("DummyContext2"));
            }

            Assert.False(Context.HasContext<DummyContext>());
            Assert.False(Context.HasContext(typeof(DummyContext)));
            Assert.False(Context.HasContext(typeof(DummyContext).Name));
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
            using (var context = Context.SetContext<DummyContext>())
            {
                Assert.True(Context.HasContext<DummyContext>());
                Assert.Equal(context, Context.GetContext<DummyContext>());
            }

            Assert.False(Context.HasContext<DummyContext>());
        }
    }
}