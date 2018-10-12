namespace ODAL.Infrastructure
{
    public class ClientSession : IClientSession
    {
        public int UserID { get; set; }
        public string IP { get; set; }
        public string AccessToken { get; set; }
    }
}
