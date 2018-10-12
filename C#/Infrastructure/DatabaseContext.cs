using System;
using System.Data;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;

namespace ODAL.Infrastructure
{
    public class DatabaseContext : IDatabaseContext
    {
        private readonly string _connectionString;
        private OracleConnection _connection;

        public DatabaseContext(IOptions<ConnectionString> connectionStringSettings)         // Bind to Configuration object in Startup.cs file: services.Configure<ConnectionString>(Configuration.GetSection("ConnectionString"));
        {
            _connectionString = connectionStringSettings.Value.ToString();
        }
        
        public OracleConnection Connection // Get accessor code will be automatically executed when a DatabaseContext object is created. So a an OracleConnection is created whenever a DatabaseContext object is created.
        {
            get
            {
                if (_connection == null)
                {
                    _connection = new OracleConnection(_connectionString);
                }
                if (_connection.State != ConnectionState.Open)
                {
                    try
                    {
                        _connection.Open();

                        if (_connection.State != ConnectionState.Open)
                        {
                            ForceOpenConnection();
                        }
                    }

                    catch (Exception)
                    {
                        ForceOpenConnection();
                    }
                }
                return _connection;
            }
        }

        public void Dispose()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
                _connection.Close();
        }

        public void ForceOpenConnection()
        {
            _connection.Dispose();
            _connection = new OracleConnection(_connectionString);
            _connection.Open();
        }
    }
}
