namespace ODAL.Repositories
{
    internal class QueryBuilder
    {
        public static string Build(IQueryContext queryContext, string tableName)
        {
            string columns = GetColumns(queryContext);

            // Omit WHERE clause.
            if (queryContext.WhereClause == null)
            {
                // Omit OFFSET clause.
                if (queryContext.OffsetClause == null)
                {
                    // Omit ORDER BY clause.
                    if (queryContext.OrderByClause == null)
                    {
                        return $"SELECT {columns} FROM {tableName}";
                    }

                    // Include ORDER BY clause.
                    else
                    {
                        return $"SELECT {columns} FROM {tableName} " +
                               $"ORDER BY {queryContext.OrderByClause.ColumnName} {queryContext.OrderByClause.Operator.ToString()}";
                    }
                }

                // Include OFFSET clause.
                else
                {
                    // Omit ORDER BY clause.
                    if (queryContext.OrderByClause == null)
                    {
                        return $"SELECT {columns} FROM {tableName} " +
                               $"OFFSET {queryContext.OffsetClause.PageSize} * {queryContext.OffsetClause.PageIndex} ROWS FETCH NEXT {queryContext.OffsetClause.PageSize} ROWS ONLY";
                    }

                    // Include ORDER BY clause.
                    else
                    {
                        return $"SELECT {columns} FROM {tableName} " +
                               $"ORDER BY {queryContext.OrderByClause.ColumnName} {queryContext.OrderByClause.Operator.ToString()} " +
                               $"OFFSET {queryContext.OffsetClause.PageSize} * {queryContext.OffsetClause.PageIndex} ROWS FETCH NEXT {queryContext.OffsetClause.PageSize} ROWS ONLY";
                    }
                }
            }

            // Include WHERE clause.
            else
            {
                // Omit OFFSET clause.
                if (queryContext.OffsetClause == null)
                {
                    // Omit ORDER BY clause.
                    if (queryContext.OrderByClause == null)
                    {
                        return $"SELECT {columns} FROM {tableName} " +
                               $"WHERE {queryContext.WhereClause.GetConditions()}";
                    }

                    // Include ORDER BY clause.
                    else
                    {
                        return $"SELECT {columns} FROM {tableName} " +
                               $"WHERE {queryContext.WhereClause.GetConditions()} " +
                               $"ORDER BY {queryContext.OrderByClause.ColumnName} {queryContext.OrderByClause.Operator.ToString()}";
                    }
                }

                // Include OFFSET clause.
                else
                {
                    // Omit ORDER BY clause.
                    if (queryContext.OrderByClause == null)
                    {
                        return $"SELECT {columns} FROM {tableName} " +
                               $"WHERE {queryContext.WhereClause.GetConditions()} " +
                               $"OFFSET {queryContext.OffsetClause.PageSize} * {queryContext.OffsetClause.PageIndex} ROWS FETCH NEXT {queryContext.OffsetClause.PageSize} ROWS ONLY";
                    }

                    // Include ORDER BY clause.
                    else
                    {
                        return $"SELECT {columns} FROM {tableName} " +
                               $"WHERE {queryContext.WhereClause.GetConditions()} " +
                               $"ORDER BY {queryContext.OrderByClause.ColumnName} {queryContext.OrderByClause.Operator.ToString()} " +
                               $"OFFSET {queryContext.OffsetClause.PageSize} * {queryContext.OffsetClause.PageIndex} ROWS FETCH NEXT {queryContext.OffsetClause.PageSize} ROWS ONLY";
                    }
                }
            }
        }

        private static string GetColumns(IQueryContext queryContext)
        {
            string columns = string.Empty;

            if (queryContext.Columns is null)
            {
                return "*";
            }

            else
            {
                foreach (var column in queryContext.Columns)
                {
                    columns += column + ", ";
                }

                columns = columns.Substring(0, columns.Length - 2);

                return columns;
            }
        }
    }
}
