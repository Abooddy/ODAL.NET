using System;

namespace ODAL.Exceptions
{
    public class InvalidDataTypeException : ApplicationException
    {
        public InvalidDataTypeException(string message) : base(message)
        {

        }
    }
}
