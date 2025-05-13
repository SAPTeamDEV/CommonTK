using System.Text;

namespace SAPTeam.CommonTK.Tests;

public class DummyStatusProvider : IStatusProvider
{
    public StringBuilder Input { get; }

    public DummyStatusProvider() => Input = new StringBuilder();

    public void Clear() => Input.Clear();

    public void Dispose() => Clear();

    public StatusIdentifier Write(string message)
    {
        Input.Append(message);
        return new StatusIdentifier(this, 0);
    }
}
