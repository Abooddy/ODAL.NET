using System.Collections.Generic;

namespace ODAL.Repositories
{
    public interface IQueryContext
    {
        List<string> Columns { get; set; }
        WhereClause WhereClause { get; set; }
        OrderByClause OrderByClause { get; set; }
        OffsetClause OffsetClause { get; set; }

        QueryContext GetById(int? id);
    }
}
