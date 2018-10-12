using Oracle.ManagedDataAccess.Client;

namespace ODAL.Infrastructure
{
    public interface IUnitOfWork
    {
        IDatabaseContext GetDatabaseContext();
        OracleTransaction BeginTransaction();
        void Commit();
        void Rollback();
    }
}
