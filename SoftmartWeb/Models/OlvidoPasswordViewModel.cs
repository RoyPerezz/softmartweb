using System.ComponentModel.DataAnnotations;

namespace SoftmartWeb.Models
{
    public class OlvidoPasswordViewModel
    {
        [Required(ErrorMessage ="El Email es obligatorio")]
        [EmailAddress]
        public string Email { get; set; }
       
        

    }
}
