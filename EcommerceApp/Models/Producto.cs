namespace EcommerceApp.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public int Precio { get; set; }
        public string ImagenUrl { get; set; } = null!;
        public int CategoriaId { get; set; }
    }
}