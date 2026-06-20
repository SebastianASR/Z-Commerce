using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class Pedido
    {
        public int Id { get; set; }

        // Amarra el pedido al usuario logueado de Identity (opcional por si compra como invitado)
        public string? UsuarioId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string NombreCompleto { get; set; } = null!;

        [Required(ErrorMessage = "El correo es obligatorio"), EmailAddress]
        public string Correo { get; set; } = null!;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        public string Telefono { get; set; } = null!;

        // --- DATOS DE DIRECCIÓN ---
        [Required(ErrorMessage = "La región es obligatoria")]
        public string Region { get; set; } = null!;

        [Required(ErrorMessage = "La comuna es obligatoria")]
        public string Comuna { get; set; } = null!;

        [Required(ErrorMessage = "La calle es obligatoria")]
        public string Calle { get; set; } = null!;

        [Required(ErrorMessage = "El número es obligatorio")]
        public string Numero { get; set; } = null!;

        public string? DeptoBlockOficina { get; set; } // Opcional

        public int Total { get; set; }
        public DateTime FechaPedido { get; set; } = DateTime.UtcNow;

        // Relación histórica con los productos comprados
        public List<PedidoDetalle> Detalles { get; set; } = new();
    }
}