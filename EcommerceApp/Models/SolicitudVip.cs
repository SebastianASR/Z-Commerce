using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class SolicitudVip
    {
        public int Id { get; set; }

        [Required]
        public string NombreEmpresa { get; set; } = null!;

        [Required, EmailAddress]
        public string Correo { get; set; } = null!;

        [Required]
        public string TipoProyecto { get; set; } = null!;

        [Required]
        public string Detalles { get; set; } = null!;

        // C# va a guardar la fecha y hora exacta en que el cliente apretó el botón
        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;
    }
}