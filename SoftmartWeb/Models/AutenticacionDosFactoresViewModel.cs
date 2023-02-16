using System.ComponentModel.DataAnnotations;

namespace SoftmartWeb.Models
{
    public class AutenticacionDosFactoresViewModel
    {
        //Para el acceso (login)
        [Required]
        [Display(Name ="Código del autenticador")]
        public string Code { get; set; }


        //para el registro
        public string Token { get; set; }

        //para codigo qr
        public string UrlCodigoQR { get; set; }

    }
}
