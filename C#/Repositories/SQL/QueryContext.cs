using System.Collections.Generic;

namespace ODAL.Repositories
{
    public class QueryContext : IQueryContext
    {
        public List<string> Columns { get; set; }
        public WhereClause WhereClause { get; set; }
        public OrderByClause OrderByClause { get; set; }
        public OffsetClause OffsetClause { get; set; }

        public QueryContext()
        {
            Columns = new List<string>();
            WhereClause = new WhereClause();
            OrderByClause = new OrderByClause();
            OffsetClause = new OffsetClause();
        }

        public QueryContext GetById(int? id)
        {
            QueryContext sqlContext = new QueryContext
            {
                Columns = null,
                OffsetClause = null,
                OrderByClause = null,
                WhereClause = new WhereClause
                {
                    Conditions = new List<Condition>
                    {
                        new Condition("ID", id)
                    }
                }
            };

            return sqlContext;
        }
    }
}