using Oracle.ManagedDataAccess.Client;

namespace ODAL.Infrastructure
{
    public interface IDatabaseContext
    {
        OracleConnection Connection { get; }
        void Dispose();
    }
}
