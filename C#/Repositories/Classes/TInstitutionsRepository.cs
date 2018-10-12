using ODAL.Helpers;
using ODAL.Infrastructure;
using ODAL.Models;
using Microsoft.Extensions.Options;

namespace ODAL.Repositories
{
    public class TInstitutionsRepository : BaseRepository, ITInstitutionsRepository
    {
        public TInstitutionsRepository(IUnitOfWork unitOfWork, IOptions<DatabaseConfigHelper> configuration) : base(unitOfWork, configuration)
        {

        }

        protected override string GetTableName()
        {
            return DatabaseHelper.ToDbNamingConvention(typeof(TInstitutions).Name);
        }

        protected override IModel CreateModel()
        {
            return new TInstitutions();
        }
    }
}