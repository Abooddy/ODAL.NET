using System;

namespace ODAL.Exceptions
{
    public class InvalidModelOrTableException : ApplicationException
    {
        public InvalidModelOrTableException(string message) : base(message)
        {

        }
    }
}
