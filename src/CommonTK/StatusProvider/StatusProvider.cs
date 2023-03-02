namespace WindowsPro.Extensions.StatusProvider;

public class StatusProvider : IStatusProvider
{
    public static IStatusProvider Empty => new StatusProvider();

    public void Clear() { }
    public void Write(string message) { }
}
