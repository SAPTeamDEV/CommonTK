namespace SAPTeam.CommonTK
{
    public interface IStatusProvider
    {
        void Clear();
        void Write(string message);
    }
}