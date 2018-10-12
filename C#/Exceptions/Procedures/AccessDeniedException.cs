using System;

namespace ODAL.Exceptions
{
    public class AccessDeniedException : ApplicationException
    {
        public AccessDeniedException(string message) : base(message)
        {

        }
    }
}
