using System;

namespace Application.DTO.Exceptions
{
    public class UnprocessableEntityException : Exception
    {
        public string Code { get; }
        public UnprocessableEntityException(string code, string message) : base(message)
        {
            Code = code;
        }
    }
}
