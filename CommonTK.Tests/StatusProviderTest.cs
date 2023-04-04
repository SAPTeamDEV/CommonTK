using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.CommonTK.Tests
{
    public class StatusProviderTest
    {
        [Fact]
        public void GlobalStatusTest()
        {
            var status = new DummyStatusProvider();
            StatusProvider.Provider = status;
            Assert.Equal(status, StatusProvider.Provider);
            StatusProvider.Provider = StatusProvider.Empty;
            Assert.NotEqual(status, StatusProvider.Provider);
        }

        [Fact]
        public void NullRefrenceTest()
        {
            Assert.ThrowsAny<NullReferenceException>(() => StatusProvider.Write("new"));
            Assert.ThrowsAny<NullReferenceException>(() => StatusProvider.Clear());
        }

        [Fact]
        public void FunctionalityTest()
        {
            var status = new DummyStatusProvider();
            StatusProvider.Provider = status;
            Assert.Throws<InvalidOperationException>(() => StatusProvider.Write("test", ProgressBarType.Block));
            StatusProvider.Write("test");
            Assert.Equal("test", status.Input.ToString());
            StatusProvider.Clear();
            Assert.Equal("", status.Input.ToString());
            StatusProvider.Write("test");
            StatusProvider.Provider = StatusProvider.Empty;
            Assert.Equal("", status.Input.ToString());
        }

        [Fact]
        public void SwithcingStatusesTest()
        {
            var status1 = new DummyStatusProvider();
            var status2 = new DummyProgressStatusProvider();
            StatusProvider.Provider = status1;
            Assert.Equal(status1, StatusProvider.Provider);
            StatusProvider.Provider = status2;
            Assert.Equal(status2, StatusProvider.Provider);
            StatusProvider.Provider = StatusProvider.Empty;
        }

        [Fact]
        public void WriteProgressTest()
        {
            var status = new DummyProgressStatusProvider();
            StatusProvider.Provider = status;
            StatusProvider.Write("test", ProgressBarType.None);
            Assert.Equal("test", status.Input.ToString());
            status.Clear();
            StatusProvider.Write("test", ProgressBarType.Wait);
            Assert.Equal("test: Wait", status.Input.ToString());
            status.Clear();
            StatusProvider.Write("test", ProgressBarType.Block);
            Assert.Equal("test: Block", status.Input.ToString());
            StatusProvider.Reset();
        }

        [Fact]
        public void IncrementProgressTest()
        {
            Assert.Throws<InvalidOperationException>(() => StatusProvider.Increment());
            var status = new DummyProgressStatusProvider();
            StatusProvider.Provider = status;
            Assert.Throws<NotImplementedException>(() => StatusProvider.Increment());
            StatusProvider.Write("test", ProgressBarType.Wait);
            Assert.Throws<NotImplementedException>(() => StatusProvider.Increment());
            status.Clear();
            StatusProvider.Write("test", ProgressBarType.Block);
            StatusProvider.Increment();
            Assert.Equal("test: Block++++++++++", status.Input.ToString());
            StatusProvider.Reset();
        }

        [Fact]
        public void MultiStatusTest()
        {
            var status = new DummyMultiStatusProvider();
            StatusProvider.Provider = status;
            Assert.Throws<InvalidOperationException>(() => StatusProvider.Write("test", ProgressBarType.Block));
            StatusProvider.Write("test1");
            Assert.Contains("test1", status.Input);
            StatusProvider.Clear();
            Assert.Empty(status.Input);
            StatusProvider.Write("test2");
            Assert.Contains("test2", status.Input);
            StatusProvider.Write("test3");
            Assert.Contains("test2", status.Input);
            Assert.Contains("test3", status.Input);
            StatusProvider.Clear("test2");
            Assert.DoesNotContain("test2", status.Input);
            Assert.Contains("test3", status.Input);
            StatusProvider.Provider = StatusProvider.Empty;
            Assert.Empty(status.Input);
        }
    }
}
