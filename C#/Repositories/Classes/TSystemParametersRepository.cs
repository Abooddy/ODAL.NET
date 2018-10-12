using ODAL.Helpers;
using ODAL.Infrastructure;
using ODAL.Models;
using Microsoft.Extensions.Options;

namespace ODAL.Repositories
{
    public class TSystemParametersRepository : BaseRepository, ITSystemParametersRepository
    {
        public TSystemParametersRepository(IUnitOfWork unitOfWork, IOptions<DatabaseConfigHelper> configuration) : base(unitOfWork, configuration)
        {

        }

        protected override string GetTableName()
        {
            return DatabaseHelper.ToDbNamingConvention(typeof(TSystemParameters).Name);
        }

        protected override IModel CreateModel()
        {
            return new TSystemParameters();
        }
    }
}