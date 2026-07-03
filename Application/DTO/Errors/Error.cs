using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Errors
{
    public class Error
    {
        public string? Code { get; }
        public string? Message { get; }

        public Error(string? code, string? message)
        {
            Code = code;
            Message = message;
        }


    }
}