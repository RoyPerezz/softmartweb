using System.ComponentModel.DataAnnotations;

namespace SoftmartWeb.Models
{
    public class ConfirmacionAccesoExternoViewModel
    {
        [Required]
        [EmailAddress]

        public string Email { get; set; }

        [Required]
      

        public string Name { get; set; }
    }
}
