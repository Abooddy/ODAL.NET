using System;
using ODAL.Exceptions;
using Oracle.ManagedDataAccess.Client;

namespace ODAL.Helpers
{
    public static class DatabaseHelper
    {
        #region ToDbNamingConvention
        public static string ToDbNamingConvention(string inApplicationConvention)
        {
            string result = null;

            foreach (var character in inApplicationConvention) // If character is uppercase, attach an underscore behind it. Else, leave it alone.
            {
                if (char.IsUpper(character))
                {
                    result = result + "_" + character;
                }

                else
                {
                    result = result + character;
                }
            }

            return result.Substring(1, result.Length - 1).ToUpper(); // Remove the first underscore. The final result is AbdullahAdel > ABDULLAH_ADEL.
        }
        #endregion

        #region ToDbDateConvention
        public static string ToDbDateConvention(DateTime dateProperty)
        {
            return dateProperty.ToString("dd/MM/yyyy HH:mm:ss");
        }
        #endregion

        #region ToOracleDbType
        public static OracleDbType ToOraclDbType(Type parameterType)
        {
            OracleDbType oracleDbType;

            if (parameterType == typeof(string))
            {
                oracleDbType = OracleDbType.Varchar2;
            }

            else if (parameterType == typeof(int) || parameterType == typeof(int?))
            {
                oracleDbType = OracleDbType.Int32;
            }

            else if (parameterType == typeof(Int64) || parameterType == typeof(Int64?))
            {
                oracleDbType = OracleDbType.Int64;
            }

            else if (parameterType == typeof(double) || parameterType == typeof(double?))
            {
                oracleDbType = OracleDbType.Double;
            }

            else if (parameterType == typeof(DateTime) || parameterType == typeof(DateTime?))
            {
                oracleDbType = OracleDbType.Date;
            }

            else if (parameterType == typeof(bool) || parameterType == typeof(bool?))
            {
                oracleDbType = OracleDbType.Boolean;
            }

            else
            {
                throw new InvalidDataTypeException($"Invalid parameter data type: {parameterType.Name}, {parameterType.GetType().FullName}.");
            }

            return oracleDbType;
        }
        #endregion

        #region GetParameterSize
        public static int GetParameterSize(Type parameterType, DatabaseConfigHelper configuration)
        {
            if (parameterType == typeof(string))
            {
                return configuration.Varchar2MaxBufferSize;
            }

            else if (parameterType == typeof(int) || parameterType == typeof(int?) ||       // Basically, all allowed numbers datatypes in the framework. Giving a fixed percision with no scale to double values will not affect the decimal point.
                     parameterType == typeof(Int64) || parameterType == typeof(Int64?) ||
                     parameterType == typeof(double) || parameterType == typeof(double?))
            {
                return configuration.NumberMaxBufferSize;
            }

            else if (parameterType == typeof(DateTime) || parameterType == typeof(DateTime?))
            {
                return 0; // This works fine.
            }

            else if (parameterType == typeof(bool) || parameterType == typeof(bool?))
            {
                return 0; // This works fine.
            }

            else
            {
                throw new InvalidDataTypeException($"Invalid parameter datatype: {parameterType.Name}, {parameterType.GetType().FullName}.");
            }
        }
        #endregion
    }
}