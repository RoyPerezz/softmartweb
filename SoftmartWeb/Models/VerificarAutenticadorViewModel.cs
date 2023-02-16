using Microsoft.Build.Graph;
using System.ComponentModel.DataAnnotations;

namespace SoftmartWeb.Models
{
    public class VerificarAutenticadorViewModel
    {

        [Required]
        [Display(Name ="Código del autenticador")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Recordar datos")]
        public bool RecordarDatos { get; set; }

    }
}
