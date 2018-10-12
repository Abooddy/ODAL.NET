using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace ODAL.Repositories
{
    public interface IBaseRepository
    {
        Task<List<IModel>> GetAsync(IQueryContext queryContext, CancellationToken cancellationToken);
        Task<bool> InsertAsync(IModel model, OracleTransaction oracleTransaction, CancellationToken cancellationToken);
        Task<bool> UpdateAsync(IModel model, OracleTransaction oracleTransaction, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(int modelID, OracleTransaction oracleTransaction, CancellationToken cancellationToken);
    }
}