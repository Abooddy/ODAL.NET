namespace ODAL.Infrastructure
{
    public interface IClientSession
    {
        int UserID { get; set; }
        string IP { get; set; }
        string AccessToken { get; set; }
    }
}
