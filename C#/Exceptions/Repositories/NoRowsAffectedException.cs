using System;

namespace ODAL.Exceptions
{
    public class NoRowsAffectedException : ApplicationException
    {
        public NoRowsAffectedException(string message) : base(message)
        {

        }
    }
}
