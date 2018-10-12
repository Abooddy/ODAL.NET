using System;
using System.Collections.Generic;
using System.Linq;

namespace ODAL.Repositories
{
    public class WhereClause
    {
        public List<Condition> Conditions { get; set; }

        public WhereClause()
        {
            Conditions = new List<Condition>();
        }

        public string GetConditions()
        {
            string temp = null;

            foreach(var condition in Conditions)
            {
                if (Condition.ParameterLessOperators.Contains(Condition.GetOperator(condition.Operator)))
                {
                    temp = temp + condition.ColumnName + " " + condition.Operator + " AND ";
                }

                else if (Condition.SingleValueOperators.Contains(Condition.GetOperator(condition.Operator)))
                {
                    if (condition.Operator == Condition.GetOperatorString(WhereOperator.Like)|| condition.Operator == Condition.GetOperatorString(WhereOperator.NotLike))
                    {
                        string likeValue = ((string)condition.Values[0]);
                        likeValue = likeValue.Insert(1, "%");
                        likeValue = likeValue.Insert(likeValue.Length - 1, "%");

                        temp = temp + condition.ColumnName + " " + condition.Operator + " " + likeValue + " AND ";
                    }

                    else
                    {
                        temp = temp + condition.ColumnName + " " + condition.Operator + " " + condition.Values[0] + " AND ";
                    }

                }

                else if (Condition.TwoValueOperators.Contains(Condition.GetOperator(condition.Operator)))
                {
                    temp = temp + condition.ColumnName + " " + condition.Operator + " " + condition.Values[0] + " AND " + condition.Values[1] + " AND ";
                }

                else // IN & NOT IN
                {
                    string valuesString = null;

                    foreach(var value in condition.Values) // Construct values.
                    {
                        valuesString = valuesString + value.ToString() + ", ";
                    }

                    valuesString = valuesString.Substring(0, valuesString.Length - 2); // Remove last character ",".

                    valuesString = "(" + valuesString + ")"; // Append parenthesis.

                    temp = temp + condition.ColumnName + " " + condition.Operator + " " + valuesString + " AND ";
                }
            }

            temp = temp?.Substring(0, temp.Length - 5); // Remove last "AND".

            return temp;
        }
    }

    public class Condition
    {
        public string ColumnName { get; }
        public string Operator { get; }
        public object[] Values { get; }

        public static readonly IEnumerable<WhereOperator> ParameterLessOperators = new[] 
        {
            WhereOperator.IsNotNull, WhereOperator.IsNull
        };

        public static readonly IEnumerable<WhereOperator> SingleValueOperators = new[]
        {
            WhereOperator.Equal, WhereOperator.GreaterOrEqual, WhereOperator.GreaterThan,
            WhereOperator.LessOrEqual, WhereOperator.LessThan, WhereOperator.NotEqual,
            WhereOperator.Like, WhereOperator.NotLike
        };

        public static readonly IEnumerable<WhereOperator> TwoValueOperators = new[]
        {
            WhereOperator.Between
        };


        public Condition(string columnName, object value)
        {
            ColumnName = columnName;
            Operator = GetOperatorString(WhereOperator.Equal);
            Values = new[]
            {
                GetValueByType(value)
            };
        }

        public Condition(string columnName, WhereOperator @operator, params object[] values)
        {
            if ((@operator == WhereOperator.In) || (@operator == WhereOperator.NotIn))
            {
                // Can be any number of values, assign them all.
                Values = values;
            }

            else
            {
                if (ParameterLessOperators.Contains(@operator))
                {
                    if (values.Length > 0)
                        throw new ArgumentException($"Operator {@operator} is a parameterless operator.", nameof(values));
                }

                else
                {
                    if (values.Length == 0)
                        throw new ArgumentException($"Operator {@operator} need at least one parameter.", nameof(values));
                }

                if (TwoValueOperators.Contains(@operator) && (values.Length != 2))
                    throw new ArgumentException($"Operator {@operator} need two parameters.", nameof(values));

                if (SingleValueOperators.Contains(@operator) && (values.Length != 1))
                    throw new ArgumentException($"Operator {@operator} need one parameter.", nameof(values));

                if (values.Length == 1)
                {
                    values = new[]
                    {
                        GetValueByType(values[0])
                    };
                }

                else if (values.Length == 2)
                {
                    values = new[]
                    {
                        GetValueByType(values[0]),
                        GetValueByType(values[1])
                    };
                }

                // More than 2 values.
                Values = values;
            }

            ColumnName = columnName;
            Operator = GetOperatorString(@operator);
        }

        private static object GetValueByType(object value)
        {
            return value is string ? $"'{value}'" : value is DateTime ? $"'{value}'" : value;
        }

        public static string GetOperatorString(WhereOperator op)
        {
            string opString;
            switch (op)
            {
                case WhereOperator.Equal:
                    opString = "=";
                    break;
                case WhereOperator.NotEqual:
                    opString = "!=";
                    break;
                case WhereOperator.LessThan:
                    opString = "<";
                    break;
                case WhereOperator.LessOrEqual:
                    opString = "<=";
                    break;
                case WhereOperator.GreaterThan:
                    opString = ">";
                    break;
                case WhereOperator.GreaterOrEqual:
                    opString = ">=";
                    break;
                case WhereOperator.In:
                    opString = "IN";
                    break;
                case WhereOperator.NotIn:
                    opString = "NOT IN";
                    break;
                case WhereOperator.Between:
                    opString = "BETWEEN";
                    break;
                case WhereOperator.Like:
                    opString = "LIKE";
                    break;
                case WhereOperator.NotLike:
                    opString = "NOT LIKE";
                    break;
                case WhereOperator.IsNull:
                    opString = "IS NULL";
                    break;
                case WhereOperator.IsNotNull:
                    opString = "IS NOT NULL";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, "Unkown SQL operator.");
            }

            return opString;
        }

        public static WhereOperator GetOperator(string op)
        {
            WhereOperator @operator;
            switch (op)
            {
                case "=":
                    @operator = WhereOperator.Equal;
                    break;
                case "!=":
                    @operator = WhereOperator.NotEqual;
                    break;
                case "<":
                    @operator = WhereOperator.LessThan;
                    break;
                case "<=":
                    @operator = WhereOperator.LessOrEqual;
                    break;
                case ">":
                    @operator = WhereOperator.GreaterThan;
                    break;
                case ">=":
                    @operator = WhereOperator.GreaterOrEqual;
                    break;
                case "IN":
                    @operator = WhereOperator.In;
                    break;
                case "NOT IN":
                    @operator = WhereOperator.NotIn;
                    break;
                case "BETWEEN":
                    @operator = WhereOperator.Between;
                    break;
                case "LIKE":
                    @operator = WhereOperator.Like;
                    break;
                case "NOT LIKE":
                    @operator = WhereOperator.NotLike;
                    break;
                case "IS NULL":
                    @operator = WhereOperator.IsNull;
                    break;
                case "IS NOT NULL":
                    @operator = WhereOperator.IsNotNull;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, "Unkown SQL operator.");
            }

            return @operator;
        }
    }

    public enum WhereOperator
    {
        Equal,
        NotEqual,
        LessThan,
        LessOrEqual,
        GreaterThan,
        GreaterOrEqual,
        In,
        NotIn,
        Between,
        Like,
        NotLike,
        IsNull,
        IsNotNull
    }
}