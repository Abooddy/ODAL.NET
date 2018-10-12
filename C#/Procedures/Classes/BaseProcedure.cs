/*////////////////////////////////////////////////////////////////////////////////////////////////////////

  BASE PROCEDURE
  ________________

  Copyrights © [2018] Arab Information Technology
  Authored by Abdullah Adel
  ________________________________________________

  Parameters datatype Mapping:
  
  .NET            ODP.NET                PLSQL Procedure
 _________       __________________     _____________________
  int?            Int32(18)              NUMBER
  Int64?          Int64(18)              NUMBER
  double?         Double(18)             NUMBER
  string          Varchar2(1024)         VARCHAR2 or CLOB
  DateTime?       Date(0)                DATE
  bool            Boolean(0)             BOOLEAN

  Mapping of datatypes and thier sizes is happenning in DatabaseHelper class. The sizes are dynamic and are stored in the applications
  config file. For example, Varchar2 maximum size is 1024 and numbers consists of 18 digits maximum.


////////////////////////////////////////////////////////////////////////////////////////////////////////*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ODAL.Exceptions;
using ODAL.Helpers;
using ODAL.Infrastructure;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ODAL.Procedures
{
    public class BaseProcedure : IBaseProcedure
    {
        private readonly IDatabaseContext databaseContext;
        private readonly DatabaseConfigHelper configuration;

        private JObject dataOut;

        public BaseProcedure(IUnitOfWork unitOfWork, IOptions<DatabaseConfigHelper> configuration)
        {
            if (unitOfWork == null)
                throw new ArgumentNullException();

            databaseContext = unitOfWork.GetDatabaseContext();
            this.configuration = configuration.Value;
        }

        #region Stored Procedure Execution
        public async Task<JObject> Execute(CancellationToken cancellationToken, OracleTransaction oracleTransaction = null)
        {
            using (var command = databaseContext.Connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = configuration.OracleDriverCommandTimeout;
                command.BindByName = true;
                command.Transaction = oracleTransaction;
                command.CommandText = GetProcedureName();

                // All public procedure properties will be converted to parameters, each have its name in DB convention, it's DB datatype, it's size and it's value. All these parameters are input parameters.
                foreach (var parameter in GetProcedureParameters())
                {
                    command.Parameters.Add(parameter);
                }

                // In normal case, these should be the ONLY output parameters.
                command.Parameters.Add(new OracleParameter("O_DATA_OUT", OracleDbType.Clob, null, ParameterDirection.Output));
                command.Parameters.Add(new OracleParameter("O_ERROR_CODE", OracleDbType.Int32, 3, null, ParameterDirection.Output));
                command.Parameters.Add(new OracleParameter("O_ERROR_DESC", OracleDbType.Varchar2, configuration.Varchar2MaxBufferSize, null, ParameterDirection.Output));

                await command.ExecuteNonQueryAsync(cancellationToken);

                Int32.TryParse(command.Parameters["O_ERROR_CODE"].Value.ToString(), out int responseCode);
                var responseDescription = command.Parameters["O_ERROR_DESC"].Value.ToString() == "null" ? string.Empty
                                        : command.Parameters["O_ERROR_DESC"].Value.ToString();

                if (!((OracleClob)(command.Parameters["O_DATA_OUT"].Value)).IsNull)
                {
                    dataOut = JObject.Parse(((OracleClob)(command.Parameters["O_DATA_OUT"].Value)).Value);
                }

                command.Dispose();

                // Method should return either the "OK" response description or an exception. Controllers should not be concerned with response codes.
                switch (responseCode)
                {
                    case (int)ErrorCode.Ok:
                        {
                            return dataOut;
                        }

                    case (int)ErrorCode.BadRequest:
                        {
                            throw new BadRequestException(responseDescription);
                        }

                    case (int)ErrorCode.Unauthorized:
                        {
                            throw new AccessDeniedException(responseDescription);
                        }

                    case (int)ErrorCode.NotFound:
                        {
                            throw new NotFoundException("Not Found - 404: " + responseDescription);
                        }

                    default:
                        {
                            throw new UnknownResponseCodeException("Invalid response code. " +
                                                                   "Please report this error to the application developers. " + responseDescription);
                        }
                }
            }
        }
        #endregion

        #region Virtual Members
        protected virtual IEnumerable<OracleParameter> GetProcedureParameters()
        {
            yield return null;
        }

        protected virtual string GetProcedureName()
        {
            return null;
        }
        #endregion
    }
}
