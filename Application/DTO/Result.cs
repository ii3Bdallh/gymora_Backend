using Application.DTO.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;


namespace Application.DTO
{

    public class Result<T>
    {
        public bool IsSuccess { get; init; }
        public Error? Error { get; init; }

        public T? Data { get; init; }

        public Result(bool isSuccess, Error? error = null, T? data = default)
        {
            IsSuccess = isSuccess;
            Error = error;
            Data = data;
        }


        // ✅ Factory method for success
        public static Result<T> Success(T data) => new Result<T>(true, data: data);

        // ✅ Factory method for failure
        public static Result<T> Failure(string code, string message)
            => new Result<T>(false, new Error(code, message));

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true, // ✅ يخلي الـ JSON formatted حلو
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
        }
    }


}
