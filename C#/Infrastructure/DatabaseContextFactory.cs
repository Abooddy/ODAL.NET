using Microsoft.Extensions.Options;

namespace ODAL.Infrastructure
{
    public class DatabaseContextFactory : Disposable, IDatabaseContextFactory
    {
        private IDatabaseContext _dbContext;

        public IDatabaseContext Init(IOptions<ConnectionString> connectionStringSettings)
        {
            return _dbContext ?? (_dbContext = new DatabaseContext(connectionStringSettings));
        }

        protected override void DisposeCore()
        {
            _dbContext.Dispose();
        }
    }
}
