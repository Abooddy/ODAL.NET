/*////////////////////////////////////////////////////////////////////////////////////////////////////////

  BASE REPOSITORY
  ________________

  Authored by Abdullah Adel
  ________________________________________________

  Known Issues:
  
  The value beyond the decimal point will be lost and rounded automatically if the decimal value were stored in the table with NUMBER
  datatype. An example will be 7.50 as NUMBER from database will be 8 as int in .NET. Normal double values will be stored correctly.


////////////////////////////////////////////////////////////////////////////////////////////////////////*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ODAL.Exceptions;
using ODAL.Helpers;
using ODAL.Infrastructure;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;

namespace ODAL.Repositories
{
    public class BaseRepository : IBaseRepository
    {
        private readonly IDatabaseContext databaseContext;
        private readonly DatabaseConfigHelper configuration;

        public BaseRepository(IUnitOfWork unitOfWork, IOptions<DatabaseConfigHelper> configuration)
        {
            if (unitOfWork == null)
                throw new ArgumentNullException();

            databaseContext = unitOfWork.GetDatabaseContext();
            this.configuration = configuration.Value;
        }

        #region Select Statement Execution
        public async Task<List<IModel>> GetAsync(IQueryContext queryContext, CancellationToken cancellationToken)
        {
            using (var command = databaseContext.Connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = configuration.OracleDriverCommandTimeout;
                command.CommandText = QueryBuilder.Build(queryContext, GetTableName());

                var reader = await command.ExecuteReaderAsync(cancellationToken);

                var result = new List<IModel>();

                if (reader.HasRows)
                {
                    var modelType = CreateModel().GetType();                                                                               // To set the properties values of the models using reflection services.

                    if ((reader.FieldCount != modelType.GetProperties().Count()) && queryContext.Columns == null)                                                            // Just in case that the number of columns does not match the number of properties.
                    {
                        reader.Close();

                        throw new InvalidModelOrTableException("Database table does not match the corresponding application model." +
                                                               " Number of columns is not equal to the number of properties.");
                    }

                    while (reader.Read())
                    {
                        var model = CreateModel();                                                                                         // Get a new model for each new record.

                        foreach (var property in modelType.GetProperties())                                                                // Read data from reader object and assign them to model properties using reflection services.
                        {
                            object columnValue = null;

                            // When all columns are selected.
                            if (queryContext.Columns is null)
                            {
                                columnValue = reader[DatabaseHelper.ToDbNamingConvention(property.Name)];
                            }

                            // When specific columns are selected.
                            else if (queryContext.Columns.Contains(DatabaseHelper.ToDbNamingConvention(property.Name)))
                            {
                                columnValue = reader[DatabaseHelper.ToDbNamingConvention(property.Name)];
                            }

                            else
                            {
                                continue;
                            }

                            if (columnValue is decimal)                                                                                    // ODP.NET returns non double numbers as decimals.
                            {
                                columnValue = Convert.ToInt32(columnValue);
                            }

                            if (columnValue is DBNull)
                            {
                                columnValue = null;
                            }

                            property.SetValue(model, columnValue);
                        }

                        result.Add(model);
                    }

                    reader.Close();

                    return result;
                }

                else
                {
                    reader.Close();

                    throw new EmptyResultSetException("Query yielded no results. No items were selected.");
                }
            }
        }
        #endregion

        #region Insert Statement Execution
        public async Task<bool> InsertAsync(IModel model, OracleTransaction oracleTransaction, CancellationToken cancellationToken)
        {
            using (var command = databaseContext.Connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = configuration.OracleDriverCommandTimeout;
                command.Transaction = oracleTransaction;
                command.CommandText = DMLBuilder.BuildInsertStatement(model, GetTableName(), configuration.DatabaseDateFormat);

                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                command.Dispose();

                if (rowsAffected == 1)                                                                                                     // Model inserted successfully.
                {
                    return true;
                }

                else if (rowsAffected == 0)
                {
                    throw new NoRowsAffectedException("No items were inserted. " +
                                                      "Please recheck the item you're trying to insert. If this error kept showing, kindly report it to developers.");
                }

                else
                {
                    throw new MultipleRowsAffectedException("More than one item was inserted. " +
                                                            "Please report this error to the application developers.");
                }
            }
        }
        #endregion

        #region Update Statement Execution
        public async Task<bool> UpdateAsync(IModel model, OracleTransaction oracleTransaction, CancellationToken cancellationToken)    // The method purposely lacks exception handling. Exceptions will be handeled in controllers.
        {
            using (var command = databaseContext.Connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = configuration.OracleDriverCommandTimeout;
                command.Transaction = oracleTransaction;
                command.CommandText = DMLBuilder.BuildUpdateStatement(model, GetTableName(), configuration.DatabaseDateFormat);

                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                command.Dispose();

                if (rowsAffected == 1)
                {
                    return true;
                }

                else if (rowsAffected == 0)
                {
                    throw new NoRowsAffectedException("No items were updated. " +
                                                      "Please recheck the item you're trying to update. If this error kept showing, kindly report it to developers.");
                }

                else
                {
                    throw new MultipleRowsAffectedException("More than one item was updated. " +
                                                            "Please report this error to the application developers.");
                }
            }
        }
        #endregion

        #region Delete Statement Execution
        public async Task<bool> DeleteAsync(int modelID, OracleTransaction oracleTransaction, CancellationToken cancellationToken)
        {
            using (var command = databaseContext.Connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = configuration.OracleDriverCommandTimeout;
                command.Transaction = oracleTransaction;
                command.CommandText = DMLBuilder.BuildDeleteStatement(modelID, GetTableName());

                var rowsAffected = await command.ExecuteNonQueryAsync();

                command.Dispose();

                if (rowsAffected == 1)
                {
                    return true;
                }

                else if (rowsAffected == 0)
                {
                    throw new NoRowsAffectedException("No items were deleted. " +
                                                      "Please recheck the item you're trying to delete. If this error kept showing, kindly report it to developers.");
                }

                else
                {
                    throw new MultipleRowsAffectedException("More than one item was deleted. " +
                                                            "Please report this error to the application developers.");
                }
            }
        }
        #endregion

        #region Virtual Members
        protected virtual string GetTableName()
        {
            return null;
        }

        protected virtual IModel CreateModel()
        {
            return null;
        }
        #endregion
    }
}
