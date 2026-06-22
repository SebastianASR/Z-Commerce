using EcommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Controllers
{
    [Authorize]
    public class PedidosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PedidosController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Historial de compras del cliente autenticado
        [HttpGet]
        public async Task<IActionResult> MisCompras()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var correoUsuario = usuario.Email ?? "";

            var pedidos = await _context.Pedidos
                .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
                .Where(p => p.UsuarioId == usuario.Id || p.Correo == correoUsuario)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();

            return View(pedidos);
        }

        // Panel de compras para Admin y DemoAdmin
        [Authorize(Roles = "Admin,DemoAdmin")]
        [HttpGet]
        public async Task<IActionResult> AdminCompras()
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();

            ViewBag.EsDemoAdmin = User.IsInRole("DemoAdmin") && !User.IsInRole("Admin");

            ViewBag.TotalPedidos = pedidos.Count;
            ViewBag.TotalVendido = pedidos
                .Where(p => p.EstadoPago == "Autorizado")
                .Sum(p => p.Total);

            ViewBag.TotalClientes = pedidos
                .Where(p => !string.IsNullOrWhiteSpace(p.Correo))
                .Select(p => p.Correo)
                .Distinct()
                .Count();

            ViewBag.PedidosPendientes = pedidos
                .Count(p => p.EstadoPedido == "Pendiente" || p.EstadoPedido == "Pagado");

            return View(pedidos);
        }

        // Detalle de pedido
        // Cliente: solo puede ver sus propios pedidos
        // Admin/DemoAdmin: pueden ver cualquier pedido
        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
            {
                return NotFound();
            }

            bool esAdminODemo = User.IsInRole("Admin") || User.IsInRole("DemoAdmin");

            if (!esAdminODemo)
            {
                var usuario = await _userManager.GetUserAsync(User);

                if (usuario == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                bool pedidoEsDelUsuario =
                    pedido.UsuarioId == usuario.Id ||
                    pedido.Correo == usuario.Email;

                if (!pedidoEsDelUsuario)
                {
                    return Forbid();
                }
            }

            ViewBag.EsDemoAdmin = User.IsInRole("DemoAdmin") && !User.IsInRole("Admin");

            return View(pedido);
        }

        // Solo Admin real puede cambiar estado del pedido
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, string estadoPedido)
        {
            var estadosPermitidos = new List<string>
            {
                "Pagado",
                "Preparando",
                "Enviado",
                "Entregado",
                "Cancelado"
            };

            if (!estadosPermitidos.Contains(estadoPedido))
            {
                TempData["ErrorMessage"] = "El estado seleccionado no es válido.";
                return RedirectToAction(nameof(AdminCompras));
            }

            var pedido = await _context.Pedidos.FindAsync(id);

            if (pedido == null)
            {
                return NotFound();
            }

            pedido.EstadoPedido = estadoPedido;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"El pedido #{pedido.Id} fue actualizado a estado: {estadoPedido}.";

            return RedirectToAction(nameof(AdminCompras));
        }
    }
}