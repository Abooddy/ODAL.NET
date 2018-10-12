using System;
using ODAL.Helpers;

namespace ODAL.Repositories
{
    internal class DMLBuilder
    {
        #region Build Insert Statement
        public static string BuildInsertStatement(IModel model, string tableName, string dbDateFormat)
        {
            string columnNames = null;
            string columnValues = null;
            
            foreach (var property in model.GetType().GetProperties()) // Build statement using reflection services.
            {
                columnNames += DatabaseHelper.ToDbNamingConvention(property.Name) + ", ";

                if (property.Name == "Id") // Replace ID value with S_TABLE_NAME.NEXTVAL.
                {
                    columnValues += $"S_" + tableName.Replace("T_", null) + ".NEXTVAL, "; // S_MC_CARD.NEXTVAL
                }

                else
                {
                    if (property.GetValue(model) != null)
                    {
                        if (property.PropertyType == typeof(string)) // Add quotations to string values.
                        {
                            columnValues += "'" + property.GetValue(model) + "', "; // 'StringValue'
                        }

                        else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?)) // Process DateTime values.
                        {
                            columnValues += "TO_DATE('" + DatabaseHelper.ToDbDateConvention(((DateTime)property.GetValue(model))) + $"', '{dbDateFormat}'), "; // TO_DATE('03/08/1992 08:20:00', 'DD/MM/YYYY HH24:MI:SS')
                        }

                        else
                        {
                            columnValues += property.GetValue(model) + ", "; // As is.
                        }
                    }

                    else
                    {
                        columnValues += "NULL, "; // Replace null values with NULL.
                    }
                }
            }

            columnNames = columnNames.Substring(0, columnNames.Length - 2); // Remove last comma.
            columnValues = columnValues.Substring(0, columnValues.Length - 2); // Remove last comma.

            return $"INSERT INTO {tableName} ({columnNames}) VALUES ({columnValues})";
        }
        #endregion

        #region Build Update Statement
        public static string BuildUpdateStatement(IModel model, string tableName, string dbDateFormat)
        {
            int id = 0;
            string setClause = null;

            foreach (var property in model.GetType().GetProperties())
            {
                if (property.Name == "Id")
                {
                    id = Convert.ToInt32(property.GetValue(model));
                }

                else
                {
                    if (property.GetValue(model) != null)
                    {
                        if (property.PropertyType == typeof(string))
                        {
                            setClause += DatabaseHelper.ToDbNamingConvention(property.Name) + " = '" + property.GetValue(model) + "', ";
                        }

                        else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                        {
                            setClause += DatabaseHelper.ToDbNamingConvention(property.Name) + " = TO_DATE('" + DatabaseHelper.ToDbDateConvention(((DateTime)property.GetValue(model))) + $"', '{dbDateFormat}'), ";
                        }

                        else
                        {
                            setClause += DatabaseHelper.ToDbNamingConvention(property.Name) + " = " + property.GetValue(model) + ", ";
                        }
                    }

                    else
                    {
                        setClause += DatabaseHelper.ToDbNamingConvention(property.Name) + " = NULL, ";
                    }
                }
            }

            setClause = setClause.Substring(0, setClause.Length - 2);

            return $"UPDATE {tableName} SET {setClause} WHERE ID = {id}";
        }
        #endregion

        #region Build Delete Statement
        public static string BuildDeleteStatement(int modelID, string tableName)
        {
            return $"DELETE FROM {tableName} WHERE ID = {modelID}"; // DELETE FROM MC_CARD WHERE ID = 7
        }
        #endregion
    }
}