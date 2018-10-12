using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System.Threading;
using System.Threading.Tasks;

namespace ODAL.Procedures
{
    public interface IBaseProcedure
    {
        Task<JObject> Execute(CancellationToken cancellationToken, OracleTransaction oracleTransaction = null);
    }
}
