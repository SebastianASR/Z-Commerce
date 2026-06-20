using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EcommerceApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CheckoutController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --- 1. RENDERIZAR EL FORMULARIO DE COMPRA ---
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Validamos si la bolsa del carrito de la sesión está vacía
            string? carritoBolsa = HttpContext.Session.GetString("MiBolsa");
            if (string.IsNullOrEmpty(carritoBolsa)) return RedirectToAction("Catalogo", "Home");

            var listaIds = JsonSerializer.Deserialize<List<int>>(carritoBolsa)!;
            if (!listaIds.Any()) return RedirectToAction("Catalogo", "Home");

            // Agrupamos y calculamos el total actual de la bolsa
            var resumenBolsa = ObtenerBolsaAgrupada(listaIds);
            int totalBolsa = resumenBolsa.Sum(item => item.Producto.Precio * item.Cantidad);

            // Auto-llenar datos si el partner ya inició sesión
            var modeloPedido = new Pedido { Total = totalBolsa };
            if (User.Identity?.IsAuthenticated == true)
            {
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario != null)
                {
                    modeloPedido.UsuarioId = usuario.Id;
                    modeloPedido.Correo = usuario.Email ?? "";
                }
            }

            ViewBag.ResumenBolsa = resumenBolsa; // Pasamos los productos a la vista lateral
            return View(modeloPedido);
        }

        // --- 2. PROCESAR LA ORDEN DE COMPRA (POST) ---
        [HttpPost]
        public async Task<IActionResult> Index(Pedido modelo)
        {
            string? carritoBolsa = HttpContext.Session.GetString("MiBolsa");
            if (string.IsNullOrEmpty(carritoBolsa)) return RedirectToAction("Catalogo", "Home");

            var listaIds = JsonSerializer.Deserialize<List<int>>(carritoBolsa)!;
            var resumenBolsa = ObtenerBolsaAgrupada(listaIds);

            if (ModelState.IsValid)
            {
                modelo.FechaPedido = DateTime.UtcNow;
                modelo.Total = resumenBolsa.Sum(item => item.Producto.Precio * item.Cantidad);

                // Mapeamos cada producto de la sesión al detalle físico de la base de datos
                foreach (var item in resumenBolsa)
                {
                    modelo.Detalles.Add(new PedidoDetalle
                    {
                        ProductoId = item.Producto.Id,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.Producto.Precio
                    });
                }

                // Guardamos la orden en Neon
                _context.Pedidos.Add(modelo);
                await _context.SaveChangesAsync();

                // ¡OPERACIÓN EXITOSA! Limpiamos la memoria RAM del carrito de este usuario
                HttpContext.Session.Remove("MiBolsa");

                return RedirectToAction("Confirmacion", new { id = modelo.Id });
            }

            // Si falló una validación, recargamos la vista con el resumen de la bolsa intacto
            ViewBag.ResumenBolsa = resumenBolsa;
            return View(modelo);
        }

        [HttpGet]
        public IActionResult Confirmacion(int id)
        {
            var pedido = _context.Pedidos
                .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefault(p => p.Id == id);

            if (pedido == null) return NotFound();
            return View(pedido);
        }

        // Helper reutilizable de agrupación LINQ
        private List<CarritoItem> ObtenerBolsaAgrupada(List<int> listaIds)
        {
            if (!listaIds.Any()) return new List<CarritoItem>();
            var idsDistintos = listaIds.Distinct().ToList();
            var productosDb = _context.Productos.Where(p => idsDistintos.Contains(p.Id)).ToList();

            return listaIds
                .GroupBy(id => id)
                .Select(g => {
                    var prod = productosDb.FirstOrDefault(p => p.Id == g.Key);
                    return prod != null ? new CarritoItem { Producto = prod, Cantidad = g.Count() } : null;
                })
                .Where(x => x != null).Select(x => x!).ToList();
        }
    }
}
