namespace EcommerceApp.Models
{
    public class CarritoItem
    {
        public Producto Producto { get; set; } = null!;
        public int Cantidad { get; set; }
        public int Subtotal => Producto.Precio * Cantidad;
    }
}