using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class ResendEmailConfirmationViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingresa un correo válido")]
        public string Email { get; set; } = "";
    }
}