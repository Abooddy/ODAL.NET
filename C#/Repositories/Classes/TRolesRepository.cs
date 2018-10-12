using ODAL.Helpers;
using ODAL.Infrastructure;
using ODAL.Models;
using Microsoft.Extensions.Options;

namespace ODAL.Repositories
{
    public class TRolesRepository : BaseRepository, ITRolesRepository
    {
        public TRolesRepository(IUnitOfWork unitOfWork, IOptions<DatabaseConfigHelper> configuration) : base(unitOfWork, configuration)
        {

        }

        protected override string GetTableName()
        {
            return DatabaseHelper.ToDbNamingConvention(typeof(TRoles).Name);
        }

        protected override IModel CreateModel()
        {
            return new TRoles();
        }
    }
}