using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Request
{

    public class GoogleLoginRequest
    {
        [Required(ErrorMessage = "IdToken is required.")]
        public string IdToken { get; set; } = string.Empty;
    }

}
