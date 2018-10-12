using System.Collections.Generic;
using System.Data;
using ODAL.Helpers;
using ODAL.Infrastructure;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;

namespace ODAL.Procedures
{
    public class LoginProcedure : BaseProcedure, ILoginProcedure
    {
        public string PUsername { get; set; }
        public string PPassword { get; set; }
        public string PInst { get; set; }

        private readonly DatabaseConfigHelper configuration;

        public LoginProcedure(IUnitOfWork unitOfWork, IOptions<DatabaseConfigHelper> configuration) : base(unitOfWork, configuration)
        {
            this.configuration = configuration.Value;
        }

        protected override string GetProcedureName()
        {
            return "PKG_ACCOUNT.LOGIN";
        }

        protected override IEnumerable<OracleParameter> GetProcedureParameters()
        {
            List<OracleParameter> parameterCollection = new List<OracleParameter>();

            foreach (var parameter in this.GetType().GetProperties())
            {
                parameterCollection.Add(new OracleParameter(
                 DatabaseHelper.ToDbNamingConvention(parameter.Name),
                 DatabaseHelper.ToOraclDbType(parameter.PropertyType),
                 DatabaseHelper.GetParameterSize(parameter.PropertyType, configuration),
                 parameter.GetValue(this),
                 ParameterDirection.Input));
            }

            // ADD ADDITIONAL CUSTOM PARAMETERS HERE! STILL NOT RECOMMENDED.

            return parameterCollection;
        }
    }
}