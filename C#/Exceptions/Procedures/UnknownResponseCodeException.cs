using System;

namespace ODAL.Exceptions
{
    public class UnknownResponseCodeException : ApplicationException
    {
        public UnknownResponseCodeException(string message) : base(message)
        {

        }
    }
}
