using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Models
{
    // Cambiamos DbContext por IdentityDbContext para activar el blindaje de usuarios
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Producto> Productos { get; set; } = null!;
        public DbSet<SolicitudVip> SolicitudesVip { get; set; } = null!;
        public DbSet<Pedido> Pedidos { get; set; } = null!;
        public DbSet<PedidoDetalle> PedidosDetalle { get; set; } = null!;

        // --- INYECCIÓN AUTOMÁTICA DE STOCK A LA NUBE (DATA SEEDING) ---
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ⚠️ OJO AQUÍ: Esta línea es sagrada. Le dice a Microsoft que construya sus 7 tablas de seguridad antes de hacer las tuyas.
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Producto>().HasData(
                // CATEGORÍA 1: PROCESADORES (CPU)
                new Producto { Id = 1, Nombre = "AMD Ryzen 9 7950X", Descripcion = "16 núcleos y 32 hilos a 5.7GHz. Potencia bruta para renderizado y compilación pesada.", Precio = 589990, ImagenUrl = "https://i.postimg.cc/m2tRPqgs/Ryzen-7950x.png", CategoriaId = 1 },
                new Producto { Id = 2, Nombre = "Intel Core i9-14900K", Descripcion = "Arquitectura híbrida de 24 núcleos. Frecuencia térmica máxima de 6.0 GHz.", Precio = 619990, ImagenUrl = "https://i.postimg.cc/RCYyD04X/14900k.png", CategoriaId = 1 },
                new Producto { Id = 3, Nombre = "AMD Ryzen 7 7800X3D", Descripcion = "El rey indiscutido del gaming gracias a su tecnología de caché vertical 3D V-Cache.", Precio = 429990, ImagenUrl = "https://i.postimg.cc/5jcPN6tW/7800.png", CategoriaId = 1 },
                new Producto { Id = 4, Nombre = "Intel Core i7-14700K", Descripcion = "20 núcleos avanzados optimizados para streaming, diseño y multitarea pesada.", Precio = 449990, ImagenUrl = "https://i.postimg.cc/rFxTG4xf/14700k.png", CategoriaId = 1 },
                new Producto { Id = 5, Nombre = "AMD Ryzen 5 7600X", Descripcion = "Excelente punto de entrada a la plataforma AM5 con soporte nativo PCIe 5.0.", Precio = 239990, ImagenUrl = "https://i.postimg.cc/rpn2yN2x/7600X.png", CategoriaId = 1 },

                // CATEGORÍA 2: TARJETAS GRÁFICAS (GPU)
                new Producto { Id = 6, Nombre = "NVIDIA GeForce RTX 4090 24GB", Descripcion = "Fuerza bruta para Inteligencia Artificial, trazado de rayos completo y 4K extremo.", Precio = 1949990, ImagenUrl = "https://i.postimg.cc/qMsf6Td4/4090RTX.png", CategoriaId = 2 },
                new Producto { Id = 7, Nombre = "AMD Radeon RX 7900 XTX 24GB", Descripcion = "Rendimiento puro en rasterización para dominar cualquier juego en monitores ultra-wide.", Precio = 1049990, ImagenUrl = "https://i.postimg.cc/JhdSC7jM/RX7900.png", CategoriaId = 2 },
                new Producto { Id = 8, Nombre = "NVIDIA GeForce RTX 4070 Ti Super 16GB", Descripcion = "El equilibrio perfecto con arquitectura Ada Lovelace para 1440p a 144Hz estables.", Precio = 869990, ImagenUrl = "https://i.postimg.cc/xTSh24NX/RTX4070-SUPER.png", CategoriaId = 2 },
                new Producto { Id = 9, Nombre = "AMD Radeon RX 7800 XT 16GB", Descripcion = "La opción con mejor relación precio-rendimiento del mercado para resoluciones Quad HD.", Precio = 549990, ImagenUrl = "https://i.postimg.cc/sggnfdjL/RX7800X.png", CategoriaId = 2 },

                // CATEGORÍA 3: MEMORIAS RAM
                new Producto { Id = 10, Nombre = "Corsair Dominator Titanium 64GB DDR5", Descripcion = "Kit de alto rendimiento 2x32GB a 6600MHz con disipador térmico de aluminio.", Precio = 289990, ImagenUrl = "https://i.postimg.cc/26RFZPYf/Corsair-64GB-DDR5.png", CategoriaId = 3 },
                new Producto { Id = 11, Nombre = "G.Skill Trident Z5 Neo RGB 32GB DDR5", Descripcion = "Kit de latencia ultra baja 2x16GB a 6000MHz CL30 optimizado para AMD EXPO.", Precio = 145990, ImagenUrl = "https://i.postimg.cc/3N9gzNv7/G-Skill-32GB-DDR5.png", CategoriaId = 3 },
                new Producto { Id = 12, Nombre = "Kingston FURY Beast DDR5 32GB", Descripcion = "Memoria estable de perfil bajo a 5600MHz, ideal para disipadores de torre masivos.", Precio = 119990, ImagenUrl = "https://i.postimg.cc/h40bXy3N/Kingston-32GB-DDR5.png", CategoriaId = 3 },
                new Producto { Id = 13, Nombre = "Crucial Pro RAM 16GB DDR4", Descripcion = "Módulo de actualización seguro a 3200MHz con disipador negro integrado de fábrica.", Precio = 42990, ImagenUrl = "https://i.postimg.cc/6qBrHnTp/Crucial-16GB-DDR4.png", CategoriaId = 3 },

                // CATEGORÍA 4: ALMACENAMIENTO, PLACAS, FUENTES & REFRIGERACIÓN
                new Producto { Id = 14, Nombre = "Samsung 990 PRO 2TB NVMe M.2", Descripcion = "Unidad PCIe 4.0 con velocidades de lectura de hasta 7450 MB/s. Cero loading screens.", Precio = 185990, ImagenUrl = "https://i.postimg.cc/BbZF53wd/Samsung-990-PRO-2TB.png", CategoriaId = 4 },
                new Producto { Id = 15, Nombre = "WD Black SN850X 1TB SSD NVMe", Descripcion = "Arquitectura NVMe optimizada con modo de juego predictivo de texturas.", Precio = 105990, ImagenUrl = "https://i.postimg.cc/vTPV3KJY/WD-Black-1TB.png", CategoriaId = 4 },
                new Producto { Id = 16, Nombre = "Crucial P3 Plus 500GB PCIe 4.0", Descripcion = "Excelente almacenamiento principal para arrancar Windows en 5 segundos.", Precio = 54990, ImagenUrl = "https://i.postimg.cc/jqvPxWL1/Crucial-P3-500GB.png", CategoriaId = 4 },
                new Producto { Id = 17, Nombre = "ASUS ROG Crosshair X670E Hero", Descripcion = "Placa base entusiasta con fases de poder 18+2, ranuras PCIe 5.0 y puertos duales USB4.", Precio = 649990, ImagenUrl = "https://i.postimg.cc/3JVyN2YH/ASUS-ROG-X670E.png", CategoriaId = 4 },
                new Producto { Id = 18, Nombre = "MSI MAG B650 Tomahawk WiFi", Descripcion = "Estructura blindada con disipación Shield Frozr, soporte DDR5 y Wi-Fi 6E.", Precio = 229990, ImagenUrl = "https://i.postimg.cc/cJM6Fj3B/MSI-MAG-B650.png", CategoriaId = 4 },
                new Producto { Id = 19, Nombre = "Seasonic Vertex GX-1000 1000W", Descripcion = "Fuente modular ATX 3.0 con certificación 80 Plus Gold y cable nativo PCIe 5.0.", Precio = 215990, ImagenUrl = "https://i.postimg.cc/XJjqm8cC/Seasonic-1000W.png", CategoriaId = 4 },
                new Producto { Id = 20, Nombre = "Thermal Grizzly Kryonaut Extreme", Descripcion = "Pasta térmica premium de alta conductividad para un control térmico absoluto.", Precio = 19990, ImagenUrl = "https://i.postimg.cc/G2XtCyq5/Pasta-Kryonaut.png", CategoriaId = 4 }
            );
        }
    }
}