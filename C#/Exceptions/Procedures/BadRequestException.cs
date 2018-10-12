using System;

namespace ODAL.Exceptions
{
    public class BadRequestException : ApplicationException
    {
        public BadRequestException(string message) : base(message)
        {

        }
    }
}
