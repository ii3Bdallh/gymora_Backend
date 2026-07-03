using Application.DTO.Base;
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


namespace Application.DTO.Base.Auditable
{
    public record BaseAuditableRDTO : BaseRDTO
    {

        public DateTime CreatedOn { get; set; }
        public int CreatedById { get; set; }
        public DateTime? ModifiedOn { get; set; }



    }


}
