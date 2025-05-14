namespace SAPTeam.CommonTK.Tests;

public class StatusProviderTest
{
    [Fact]
    public void GlobalStatusTest()
    {
        DummyStatusProvider status = new DummyStatusProvider();
        StatusProvider.Provider = status;
        Assert.Equal(status, StatusProvider.Provider);
        StatusProvider.Reset();
        Assert.NotEqual(status, StatusProvider.Provider);
    }

    [Fact]
    public void EnsureNotNullTest()
    {
        Assert.NotNull(StatusProvider.Provider);
        var id = StatusProvider.Write("new");
        Assert.Equal(id, StatusIdentifier.Empty);
        StatusProvider.Clear();
    }

    [Fact]
    public void FunctionalityTest()
    {
        DummyStatusProvider status = new DummyStatusProvider();
        StatusProvider.Provider = status;
        StatusIdentifier id = StatusProvider.Write("test");
        Assert.Equal("test", status.Input.ToString());
        StatusProvider.Clear(id);
        StatusIdentifier id2 = StatusProvider.Write("test2");
        Assert.Equal("test2", status.Input.ToString());
        id.Dispose();
        Assert.Equal("", status.Input.ToString());
        StatusProvider.Write("test");
        StatusProvider.Reset();
        Assert.Equal("", status.Input.ToString());
    }

    [Fact]
    public void SwitchingStatusesTest()
    {
        DummyStatusProvider status1 = new DummyStatusProvider();
        DummyProgressStatusProvider status2 = new DummyProgressStatusProvider();
        StatusProvider.Provider = status1;
        Assert.Equal(status1, StatusProvider.Provider);
        StatusProvider.Provider = status2;
        Assert.Equal(status2, StatusProvider.Provider);
        StatusProvider.Reset();
    }

    [Fact]
    public void WriteProgressTest()
    {
        DummyProgressStatusProvider status = new DummyProgressStatusProvider();
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
        DummyProgressStatusProvider status = new DummyProgressStatusProvider();
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
        DummyMultiStatusProvider status = new DummyMultiStatusProvider();
        StatusProvider.Provider = status;
        StatusProvider.Write("test1");
        Assert.Contains("test1", status.Input.Values);
        StatusProvider.Clear();
        Assert.Empty(status.Input);
        StatusIdentifier id2 = StatusProvider.Write("test2");
        Assert.Contains("test2", status.Input.Values);
        StatusIdentifier id3 = StatusProvider.Write("test3");
        StatusProvider.Write("test4");
        Assert.Contains("test2", status.Input.Values);
        Assert.Contains("test3", status.Input.Values);
        Assert.Contains("test4", status.Input.Values);
        StatusProvider.Clear(id2);
        Assert.DoesNotContain("test2", status.Input.Values);
        Assert.Contains("test3", status.Input.Values);
        Assert.Contains("test4", status.Input.Values);
        StatusProvider.Clear(id3);
        Assert.DoesNotContain("test3", status.Input.Values);
        Assert.Contains("test4", status.Input.Values);
        StatusProvider.Reset();
        Assert.Empty(status.Input);
    }

    [Fact]
    public void StatusIdentifierTest()
    {
        DummyMultiStatusProvider status = new DummyMultiStatusProvider();
        StatusProvider.Provider = status;
        StatusIdentifier id1 = StatusProvider.Write("test1");
        StatusIdentifier id2 = StatusProvider.Write("test2");
        Assert.Contains("test1", status.Input.Values);
        Assert.Contains("test2", status.Input.Values);
        StatusProvider.Clear(id1);
        Assert.DoesNotContain("test1", status.Input.Values);
        using (StatusIdentifier id3 = StatusProvider.Write("test3"))
        {
            Assert.Contains("test2", status.Input.Values);
            Assert.Contains("test3", status.Input.Values);

            using (StatusIdentifier id4 = StatusProvider.Write("test4"))
            {
                Assert.Contains("test2", status.Input.Values);
                Assert.Contains("test3", status.Input.Values);
                Assert.Contains("test4", status.Input.Values);

                using (StatusIdentifier id5 = StatusProvider.Write("test5"))
                {
                    Assert.Contains("test2", status.Input.Values);
                    Assert.Contains("test3", status.Input.Values);
                    Assert.Contains("test4", status.Input.Values);
                    Assert.Contains("test5", status.Input.Values);
                    id5.Dispose();
                    Assert.Contains("test2", status.Input.Values);
                    Assert.Contains("test3", status.Input.Values);
                    Assert.Contains("test4", status.Input.Values);
                    Assert.DoesNotContain("test5", status.Input.Values);
                }
            }

            Assert.DoesNotContain("test4", status.Input.Values);
        }

        Assert.DoesNotContain("test3", status.Input.Values);
        Assert.Contains("test2", status.Input.Values);
        StatusProvider.Reset();
        Assert.Empty(status.Input);
    }

    [Fact]
    public void NullStatusProviderTest()
    {
        Assert.Equal(StatusProvider.Empty, StatusProvider.Provider);
        StatusProvider.Provider = new NullStatusProvider();
        Assert.NotEqual(StatusProvider.Empty, StatusProvider.Provider);
        StatusProvider.Provider.Write("test");
        StatusProvider.Write("test");
        StatusProvider.Write("test", ProgressBarType.Block);
        StatusProvider.Increment();
        StatusProvider.Increment(10);
        StatusIdentifier id = StatusProvider.Write("test", ProgressBarType.Wait);
        StatusProvider.Clear();
        StatusProvider.Clear(id);
        ((IMultiStatusProvider)StatusProvider.Provider).Clear(id);
        StatusProvider.Reset();
        Assert.Equal(StatusProvider.Empty, StatusProvider.Provider);
    }
}
