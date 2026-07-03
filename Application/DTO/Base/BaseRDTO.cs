using Application.DTO.Errors;
using Domain.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace Application.DTO.Base
{
    public record BaseRDTO
    {
        public int Id { get; set; }

    }
}
