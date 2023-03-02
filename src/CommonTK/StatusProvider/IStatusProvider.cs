namespace WindowsPro.Extensions.StatusProvider;

public interface IStatusProvider
{
    public void Clear();
    public void Write(string message);
}
