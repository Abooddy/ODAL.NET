using System;

namespace ODAL.Exceptions
{
    public class MultipleRowsAffectedException : ApplicationException
    {
        public MultipleRowsAffectedException(string message) : base(message)
        {

        }
    }
}
