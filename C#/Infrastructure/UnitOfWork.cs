using System;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;

namespace ODAL.Infrastructure
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private IDatabaseContext _dbContext;
        private readonly IDatabaseContextFactory _factory;
        private IOptions<ConnectionString> _connectionStringSettings;
        public OracleTransaction Tranasction { get; private set; }

        public UnitOfWork(IDatabaseContextFactory factory, IOptions<ConnectionString> connectionStringSettings) // Will be called in the DI chain.
        {
            _factory = factory;
            _connectionStringSettings = connectionStringSettings;
        }

        public IDatabaseContext DatabaseContext => _dbContext ?? (_dbContext = _factory.Init(_connectionStringSettings)); // Get accessor code will be automatically executed when a UnitOfWork object is created.

        public IDatabaseContext GetDatabaseContext()
        {
            return _dbContext ?? (_dbContext = _factory.Init(_connectionStringSettings));
        }

        public OracleTransaction BeginTransaction()
        {
            if (Tranasction != null)
            {
                throw new NullReferenceException("Not finished previous transaction.");
            }

            Tranasction = _dbContext.Connection.BeginTransaction();
            return Tranasction;
        }

        public void Commit()
        {
            if (Tranasction != null)
            {
                try
                {
                    Tranasction.Commit();
                }

                catch (Exception)
                {
                    Tranasction.Rollback();
                }

                Tranasction.Dispose();
                Tranasction = null;
            }

            else
            {
                throw new NullReferenceException("Tried to commit an unfinished transaction.");
            }
        }

        public void Rollback()
        {
            Tranasction?.Rollback();
            Tranasction?.Dispose();
            Tranasction = null;
        }

        public void Dispose()
        {
            Tranasction?.Dispose();
            _dbContext?.Dispose();
        }
    }
}
