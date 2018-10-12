namespace ODAL.Infrastructure
{
    public class ConnectionString
    {
        public string Protocol { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public string GlobalDbName { get; set; }
        public string ServiceName { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public bool EnablePooling { get; set; } = true;
        public int MinPoolSize { get; set; }
        public int MaxPoolSize { get; set; }
        public int ConnectionLifetime { get; set; }

        public override string ToString()
        {
            string connectionString = "Data Source=(DESCRIPTION = " +
            $"(ADDRESS = (PROTOCOL = {Protocol})(HOST = {Host})(PORT = {Port}))" +
            "(CONNECT_DATA =(SERVER = DEDICATED)" + $"(GLOBAL_DBNAME = {GlobalDbName})" + $"(SERVICE_NAME = {ServiceName})));" +
            $"Persist Security Info=True;User ID={UserId};Password={Password};" +
            $"Pooling={EnablePooling};Connection Lifetime={ConnectionLifetime};" +
            $"Min Pool Size={MinPoolSize};Max Pool Size={MaxPoolSize};";

            return connectionString;
        }
    }
}
