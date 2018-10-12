using ODAL.Helpers;
using ODAL.Infrastructure;
using ODAL.Models;
using Microsoft.Extensions.Options;

namespace ODAL.Repositories
{
    public class TUsersRepository : BaseRepository, ITUsersRepository
    {
        public TUsersRepository(IUnitOfWork unitOfWork, IOptions<DatabaseConfigHelper> configuration) : base(unitOfWork, configuration)
        {

        }

        protected override string GetTableName()
        {
            return DatabaseHelper.ToDbNamingConvention(typeof(TUsers).Name);
        }

        protected override IModel CreateModel()
        {
            return new TUsers();
        }
    }
}