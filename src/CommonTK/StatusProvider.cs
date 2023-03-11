namespace SAPTeam.CommonTK;

public abstract class StatusProvider : IStatusProvider
{
    public static IStatusProvider Empty { get; }

    internal static IStatusProvider Current { get; set; } = Empty;

    public void Clear() { }
    public void Write(string message) { }
}
