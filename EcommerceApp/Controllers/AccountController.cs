using EcommerceApp.Models;
using EcommerceApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EcommerceApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _logger = logger;
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
                EmailConfirmed = false,

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

                try
                {
                    await EnviarCorreoConfirmacionAsync(user);

                    TempData["SuccessMessage"] =
                        "Tu cuenta fue creada correctamente. Revisa tu correo para confirmar tu cuenta antes de iniciar sesión.";

                    return RedirectToAction(nameof(RegisterConfirmation), new { email = user.Email });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enviando correo de confirmación para {Email}", user.Email);

                    TempData["ErrorMessage"] =
                        "Tu cuenta fue creada, pero no pudimos enviar el correo de confirmación. Puedes solicitar un nuevo enlace.";

                    return RedirectToAction(nameof(ResendEmailConfirmation));
                }
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // --- CONFIRMACIÓN POST REGISTRO ---
        [HttpGet]
        public IActionResult RegisterConfirmation(string? email)
        {
            ViewBag.Email = email;
            return View();
        }

        // --- CONFIRMAR CORREO ---
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string? userId, string? token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                TempData["ErrorMessage"] = "El enlace de confirmación no es válido.";
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "No se encontró el usuario asociado a este enlace.";
                return RedirectToAction("Login");
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                TempData["SuccessMessage"] = "Tu correo ya estaba confirmado. Puedes iniciar sesión.";
                return RedirectToAction("Login");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Correo confirmado correctamente. Ahora puedes iniciar sesión.";
                return View();
            }

            TempData["ErrorMessage"] = "No se pudo confirmar el correo. El enlace puede estar vencido o ser inválido.";
            return View();
        }

        // --- REENVIAR CONFIRMACIÓN DE CORREO ---
        [HttpGet]
        public IActionResult ResendEmailConfirmation()
        {
            return View(new ResendEmailConfirmationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                TempData["SuccessMessage"] =
                    "Si el correo existe en nuestro sistema, enviaremos un nuevo enlace de confirmación.";

                return RedirectToAction(nameof(ResendEmailConfirmation));
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                TempData["SuccessMessage"] =
                    "Este correo ya está confirmado. Puedes iniciar sesión normalmente.";

                return RedirectToAction("Login");
            }

            try
            {
                await EnviarCorreoConfirmacionAsync(user);

                TempData["SuccessMessage"] =
                    "Te enviamos un nuevo enlace de confirmación. Revisa tu bandeja de entrada o spam.";

                return RedirectToAction(nameof(ResendEmailConfirmation));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reenviando confirmación para {Email}", model.Email);

                ModelState.AddModelError(
                    "",
                    "No pudimos enviar el correo en este momento. Intenta nuevamente más tarde."
                );

                return View(model);
            }
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

            var usuario = await _userManager.FindByEmailAsync(model.Email);

            if (usuario != null && !await _userManager.IsEmailConfirmedAsync(usuario))
            {
                ModelState.AddModelError(
                    "",
                    "Debes confirmar tu correo antes de iniciar sesión. Revisa tu bandeja de entrada o solicita un nuevo enlace."
                );

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

        // --- OLVIDÉ MI CONTRASEÑA ---
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            // Respuesta genérica para no revelar si el correo existe o no.
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            try
            {
                await EnviarCorreoRecuperacionPasswordAsync(user);

                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando recuperación de contraseña para {Email}", model.Email);

                ModelState.AddModelError(
                    "",
                    "No pudimos enviar el correo de recuperación en este momento. Intenta nuevamente más tarde."
                );

                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // --- RESTABLECER CONTRASEÑA ---
        [HttpGet]
        public IActionResult ResetPassword(string? email, string? token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                TempData["ErrorMessage"] = "El enlace de recuperación no es válido.";
                return RedirectToAction("Login");
            }

            return View(new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            // Respuesta genérica para no revelar si el correo existe o no.
            if (user == null)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(
                user,
                model.Token,
                model.Password
            );

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
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
        // --- HELPERS PRIVADOS ---
        private async Task EnviarCorreoConfirmacionAsync(ApplicationUser user)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new InvalidOperationException("El usuario no tiene correo electrónico.");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var callbackUrl = Url.Action(
                action: nameof(ConfirmEmail),
                controller: "Account",
                values: new
                {
                    userId = user.Id,
                    token
                },
                protocol: Request.Scheme
            );

            if (string.IsNullOrWhiteSpace(callbackUrl))
            {
                throw new InvalidOperationException("No se pudo generar el enlace de confirmación.");
            }

            var safeUrl = WebUtility.HtmlEncode(callbackUrl);
            var safeName = WebUtility.HtmlEncode(user.Nombre ?? "cliente");

            var html = $@"
                <div style='font-family:Arial,sans-serif;max-width:620px;margin:auto;background:#f8fafc;padding:24px;border-radius:18px;border:1px solid #e2e8f0;'>
                    <div style='background:#0f172a;color:#ffffff;padding:22px;border-radius:16px;text-align:center;'>
                        <h1 style='margin:0;font-size:26px;'>Z-Commerce</h1>
                        <p style='margin:8px 0 0;color:#93c5fd;'>Confirmación de cuenta</p>
                    </div>

                    <div style='padding:24px 4px;color:#0f172a;'>
                        <h2 style='margin-top:0;'>Hola {safeName},</h2>

                        <p style='font-size:15px;line-height:1.6;'>
                            Gracias por registrarte en <strong>Z-Commerce</strong>.
                            Para activar tu cuenta y poder iniciar sesión, confirma tu correo electrónico.
                        </p>

                        <div style='text-align:center;margin:30px 0;'>
                            <a href='{safeUrl}'
                               style='background:#0d6efd;color:#ffffff;text-decoration:none;padding:14px 24px;border-radius:12px;font-weight:bold;display:inline-block;'>
                                Confirmar mi cuenta
                            </a>
                        </div>

                        <p style='font-size:13px;color:#64748b;line-height:1.6;'>
                            Si el botón no funciona, copia y pega este enlace en tu navegador:
                        </p>

                        <p style='font-size:12px;word-break:break-all;color:#2563eb;'>
                            {safeUrl}
                        </p>

                        <hr style='border:none;border-top:1px solid #e2e8f0;margin:24px 0;' />

                        <p style='font-size:12px;color:#94a3b8;margin:0;'>
                            Si no creaste esta cuenta, puedes ignorar este mensaje.
                        </p>
                    </div>
                </div>";

            await _emailService.SendEmailAsync(
                user.Email,
                "Confirma tu cuenta en Z-Commerce",
                html
            );
        }

        private async Task EnviarCorreoRecuperacionPasswordAsync(ApplicationUser user)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new InvalidOperationException("El usuario no tiene correo electrónico.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var callbackUrl = Url.Action(
                action: nameof(ResetPassword),
                controller: "Account",
                values: new
                {
                    email = user.Email,
                    token
                },
                protocol: Request.Scheme
            );

            if (string.IsNullOrWhiteSpace(callbackUrl))
            {
                throw new InvalidOperationException("No se pudo generar el enlace de recuperación.");
            }

            var safeUrl = WebUtility.HtmlEncode(callbackUrl);
            var safeName = WebUtility.HtmlEncode(user.Nombre ?? "cliente");

            var html = $@"
                <div style='font-family:Arial,sans-serif;max-width:620px;margin:auto;background:#f8fafc;padding:24px;border-radius:18px;border:1px solid #e2e8f0;'>
                    <div style='background:#0f172a;color:#ffffff;padding:22px;border-radius:16px;text-align:center;'>
                        <h1 style='margin:0;font-size:26px;'>Z-Commerce</h1>
                        <p style='margin:8px 0 0;color:#93c5fd;'>Recuperación de contraseña</p>
                    </div>

                    <div style='padding:24px 4px;color:#0f172a;'>
                        <h2 style='margin-top:0;'>Hola {safeName},</h2>

                        <p style='font-size:15px;line-height:1.6;'>
                            Recibimos una solicitud para restablecer la contraseña de tu cuenta.
                            Si fuiste tú, usa el siguiente botón.
                        </p>

                        <div style='text-align:center;margin:30px 0;'>
                            <a href='{safeUrl}'
                               style='background:#0d6efd;color:#ffffff;text-decoration:none;padding:14px 24px;border-radius:12px;font-weight:bold;display:inline-block;'>
                                Restablecer contraseña
                            </a>
                        </div>

                        <p style='font-size:13px;color:#64748b;line-height:1.6;'>
                            Si el botón no funciona, copia y pega este enlace en tu navegador:
                        </p>

                        <p style='font-size:12px;word-break:break-all;color:#2563eb;'>
                            {safeUrl}
                        </p>

                        <hr style='border:none;border-top:1px solid #e2e8f0;margin:24px 0;' />

                        <p style='font-size:12px;color:#94a3b8;margin:0;'>
                            Si no solicitaste este cambio, puedes ignorar este mensaje.
                        </p>
                    </div>
                </div>";

            await _emailService.SendEmailAsync(
                user.Email,
                "Recupera tu contraseña en Z-Commerce",
                html
            );
        }
    }
}