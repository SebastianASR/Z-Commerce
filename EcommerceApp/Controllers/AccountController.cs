using EcommerceApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // --- REGISTRO ---
        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            return View(new RegisterViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,

                Nombre = model.Nombre,
                Apellido = model.Apellido,
                PhoneNumber = model.Telefono,
                TelefonoContacto = model.Telefono,

                Region = model.Region,
                Comuna = model.Comuna,
                Calle = model.Calle,
                Numero = model.Numero,
                DeptoBlockOficina = model.DeptoBlockOficina,

                FechaRegistro = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Cliente");

                await _signInManager.SignInAsync(user, isPersistent: false);

                TempData["SuccessMessage"] = $"¡Bienvenido/a, {user.Nombre}! Tu cuenta fue creada correctamente.";

                if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return LocalRedirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // --- LOGIN ---
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                isPersistent: model.RememberMe,
                lockoutOnFailure: true
            );

            if (result.Succeeded)
            {
                var usuario = await _userManager.FindByEmailAsync(model.Email);
                var nombreUsuario = usuario?.Nombre;

                TempData["SuccessMessage"] = !string.IsNullOrWhiteSpace(nombreUsuario)
                    ? $"¡Hola, {nombreUsuario}! Has iniciado sesión correctamente."
                    : "Has iniciado sesión correctamente.";

                if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return LocalRedirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(
                    "",
                    "Tu cuenta fue bloqueada temporalmente por demasiados intentos fallidos. Intenta nuevamente en 15 minutos."
                );

                return View(model);
            }

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(
                    "",
                    "No tienes permiso para iniciar sesión. Verifica tu cuenta."
                );

                return View(model);
            }

            ModelState.AddModelError(
                "",
                "Credenciales inválidas. Verifica tu correo o contraseña."
            );

            return View(model);
        }

        // --- CERRAR SESIÓN ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        // --- ACCESO DENEGADO ---
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}