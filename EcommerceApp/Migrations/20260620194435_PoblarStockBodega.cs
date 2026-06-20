using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EcommerceApp.Migrations
{
    /// <inheritdoc />
    public partial class PoblarStockBodega : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "CategoriaId", "Descripcion", "ImagenUrl", "Nombre", "Precio" },
                values: new object[,]
                {
                    { 1, 1, "16 núcleos y 32 hilos a 5.7GHz. Potencia bruta para renderizado y compilación pesada.", "https://i.postimg.cc/m2tRPqgs/Ryzen-7950x.png", "AMD Ryzen 9 7950X", 589990 },
                    { 2, 1, "Arquitectura híbrida de 24 núcleos. Frecuencia térmica máxima de 6.0 GHz.", "https://i.postimg.cc/RCYyD04X/14900k.png", "Intel Core i9-14900K", 619990 },
                    { 3, 1, "El rey indiscutido del gaming gracias a su tecnología de caché vertical 3D V-Cache.", "https://i.postimg.cc/5jcPN6tW/7800.png", "AMD Ryzen 7 7800X3D", 429990 },
                    { 4, 1, "20 núcleos avanzados optimizados para streaming, diseño y multitarea pesada.", "https://i.postimg.cc/rFxTG4xf/14700k.png", "Intel Core i7-14700K", 449990 },
                    { 5, 1, "Excelente punto de entrada a la plataforma AM5 con soporte nativo PCIe 5.0.", "https://i.postimg.cc/rpn2yN2x/7600X.png", "AMD Ryzen 5 7600X", 239990 },
                    { 6, 2, "Fuerza bruta para Inteligencia Artificial, trazado de rayos completo y 4K extremo.", "https://i.postimg.cc/qMsf6Td4/4090RTX.png", "NVIDIA GeForce RTX 4090 24GB", 1949990 },
                    { 7, 2, "Rendimiento puro en rasterización para dominar cualquier juego en monitores ultra-wide.", "https://i.postimg.cc/JhdSC7jM/RX7900.png", "AMD Radeon RX 7900 XTX 24GB", 1049990 },
                    { 8, 2, "El equilibrio perfecto con arquitectura Ada Lovelace para 1440p a 144Hz estables.", "https://i.postimg.cc/xTSh24NX/RTX4070-SUPER.png", "NVIDIA GeForce RTX 4070 Ti Super 16GB", 869990 },
                    { 9, 2, "La opción con mejor relación precio-rendimiento del mercado para resoluciones Quad HD.", "https://i.postimg.cc/sggnfdjL/RX7800X.png", "AMD Radeon RX 7800 XT 16GB", 549990 },
                    { 10, 3, "Kit de alto rendimiento 2x32GB a 6600MHz con disipador térmico de aluminio.", "https://i.postimg.cc/26RFZPYf/Corsair-64GB-DDR5.png", "Corsair Dominator Titanium 64GB DDR5", 289990 },
                    { 11, 3, "Kit de latencia ultra baja 2x16GB a 6000MHz CL30 optimizado para AMD EXPO.", "https://i.postimg.cc/3N9gzNv7/G-Skill-32GB-DDR5.png", "G.Skill Trident Z5 Neo RGB 32GB DDR5", 145990 },
                    { 12, 3, "Memoria estable de perfil bajo a 5600MHz, ideal para disipadores de torre masivos.", "https://i.postimg.cc/h40bXy3N/Kingston-32GB-DDR5.png", "Kingston FURY Beast DDR5 32GB", 119990 },
                    { 13, 3, "Módulo de actualización seguro a 3200MHz con disipador negro integrado de fábrica.", "https://i.postimg.cc/6qBrHnTp/Crucial-16GB-DDR4.png", "Crucial Pro RAM 16GB DDR4", 42990 },
                    { 14, 4, "Unidad PCIe 4.0 con velocidades de lectura de hasta 7450 MB/s. Cero loading screens.", "https://i.postimg.cc/BbZF53wd/Samsung-990-PRO-2TB.png", "Samsung 990 PRO 2TB NVMe M.2", 185990 },
                    { 15, 4, "Arquitectura NVMe optimizada con modo de juego predictivo de texturas.", "https://i.postimg.cc/vTPV3KJY/WD-Black-1TB.png", "WD Black SN850X 1TB SSD NVMe", 105990 },
                    { 16, 4, "Excelente almacenamiento principal para arrancar Windows en 5 segundos.", "https://i.postimg.cc/jqvPxWL1/Crucial-P3-500GB.png", "Crucial P3 Plus 500GB PCIe 4.0", 54990 },
                    { 17, 4, "Placa base entusiasta con fases de poder 18+2, ranuras PCIe 5.0 y puertos duales USB4.", "https://i.postimg.cc/3JVyN2YH/ASUS-ROG-X670E.png", "ASUS ROG Crosshair X670E Hero", 649990 },
                    { 18, 4, "Estructura blindada con disipación Shield Frozr, soporte DDR5 y Wi-Fi 6E.", "https://i.postimg.cc/cJM6Fj3B/MSI-MAG-B650.png", "MSI MAG B650 Tomahawk WiFi", 229990 },
                    { 19, 4, "Fuente modular ATX 3.0 con certificación 80 Plus Gold y cable nativo PCIe 5.0.", "https://i.postimg.cc/XJjqm8cC/Seasonic-1000W.png", "Seasonic Vertex GX-1000 1000W", 215990 },
                    { 20, 4, "Pasta térmica premium de alta conductividad para un control térmico absoluto.", "https://i.postimg.cc/G2XtCyq5/Pasta-Kryonaut.png", "Thermal Grizzly Kryonaut Extreme", 19990 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 20);
        }
    }
}
