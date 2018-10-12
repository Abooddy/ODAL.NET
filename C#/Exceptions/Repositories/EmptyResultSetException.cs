using System;

namespace ODAL.Exceptions
{
    public class EmptyResultSetException : Exception
    {
        public EmptyResultSetException(string message) : base(message)
        {

        }
    }
}
