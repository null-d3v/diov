using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Diov.Web
{
    public class SetupModel
    {
        [Required]
        public IFormFile ImportFile { get; set; }
    }
}