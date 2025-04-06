using donacionesWeb.Models;
using donacionesWeb.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using donacionesWeb.Models.ViewModels;

namespace donacionesWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly UsuarioService _usuarioService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(UsuarioService usuarioService, ILogger<AuthController> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                try
                {
                    var usuarios = await _usuarioService.GetUsuariosAsync();
                    var usuario = usuarios.FirstOrDefault(u =>
                        u.Email == model.Email && u.Contrasena == model.Contrasena);

                    if (usuario != null)
                    {
                        await SignInUser(usuario);
                        _logger.LogInformation("Usuario {Email} ha iniciado sesión", usuario.Email);
                        return RedirectToLocal(returnUrl);
                    }

                    ModelState.AddModelError(string.Empty, "Credenciales inválidas");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al intentar iniciar sesión");
                    ModelState.AddModelError(string.Empty, "Ocurrió un error al iniciar sesión");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var usuario = new Usuario
                    {
                        Email = model.Email,
                        Contrasena = model.Contrasena,
                        TipoUsuario = "Donante", // Rol por defecto
                        Nombre = model.Nombre,
                        Apellido = model.Apellido,
                        Telefono = model.Telefono,
                        Activo = true,
                        FechaRegistro = DateTime.Now
                    };

                    await _usuarioService.CreateUsuarioAsync(usuario);

                    _logger.LogInformation("Nuevo usuario registrado: {Email}", usuario.Email);

                    await SignInUser(usuario);
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al registrar nuevo usuario");
                    ModelState.AddModelError(string.Empty, "Ocurrió un error al registrar el usuario");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("Usuario ha cerrado sesión");
            return RedirectToAction("Login", "Auth");
        }

        private async Task SignInUser(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}"),
                new Claim(ClaimTypes.Role, usuario.TipoUsuario)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
