namespace ODAL.Repositories
{
    public class OrderByClause
    {
        public string ColumnName { get; set; }
        public OrderByOperator Operator { get; set; }
    }

    public enum OrderByOperator
    {
        ASC,
        DESC
    }
}
