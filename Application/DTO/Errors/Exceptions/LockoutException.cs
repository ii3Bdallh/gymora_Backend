using System;

namespace Application.DTO.Exceptions
{
    public class LockoutException : Exception
    {
        public LockoutException(string message) : base(message)
        {
        }
    }
}
