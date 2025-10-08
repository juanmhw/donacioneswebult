using donacionesWeb.Models;
using donacionesWeb.Models.ViewModels;
using donacionesWeb.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;

// Alias para BCrypt (evita CS0103)
using BCryptNet = BCrypt.Net.BCrypt;

namespace donacionesWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly UsuarioService _usuarioService;
        private readonly UsuarioRolService _usuarioRolService;
        private readonly RolService _rolService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UsuarioService usuarioService,
            UsuarioRolService usuarioRolService,
            RolService rolService,
            ILogger<AuthController> logger)
        {
            _usuarioService = usuarioService;
            _usuarioRolService = usuarioRolService;
            _rolService = rolService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // bool? -> compara con true para evitar CS0266
            if (User?.Identity?.IsAuthenticated == true)
                return RedirectToAction("Dashboard", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var email = NormalizeEmail(model.Email);
                _logger.LogInformation("Intento de login para {Email}", email);

                var usuario = await _usuarioService.GetUsuarioByEmailAsync(email);

                // Mensaje genérico; nunca reveles si el email existe
                if (usuario is null || !VerifyPassword(model.Contrasena, usuario.Contrasena))
                {
                    _logger.LogWarning("Login fallido para {Email} desde {IP}",
                        email, HttpContext.Connection.RemoteIpAddress?.ToString());
                    ModelState.AddModelError(string.Empty, "Credenciales inválidas");
                    return View(model);
                }

                // Activo podría ser bool? en tu modelo -> compara con true
                if (usuario.Activo != true)
                {
                    _logger.LogWarning("Usuario inactivo {Email}", email);
                    ModelState.AddModelError(string.Empty, "Credenciales inválidas");
                    return View(model);
                }

                var usuarioRoles = await _usuarioRolService.GetUsuariosRolesByUsuarioIdAsync(usuario.UsuarioId);
                await SignInUser(usuario, usuarioRoles ?? new List<UsuarioRol>());

                _logger.LogInformation("Usuario {Email} inició sesión", usuario.Email);
                return RedirectToLocal(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar sesión");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al iniciar sesión");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromForm] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var email = NormalizeEmail(model.Email);
                var existingUser = await _usuarioService.GetUsuarioByEmailAsync(email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(nameof(model.Email), "Este correo electrónico ya está registrado");
                    return View(model);
                }

                var passwordHash = HashPassword(model.Contrasena);

                var usuario = new Usuario
                {
                    Email = email,
                    Contrasena = passwordHash,
                    Nombre = SafeTrim(model.Nombre),
                    Apellido = SafeTrim(model.Apellido),
                    Telefono = SafeTrim(model.Telefono),
                    Activo = true,
                    FechaRegistro = DateTime.UtcNow,
                    ImagenUrl = "https://example.com/user_default.jpg" // placeholder
                };

                var nuevoUsuario = await _usuarioService.CreateUsuarioAsync(usuario);

                // Rol por defecto: "Usuario" (principio de menor privilegio)
                var roles = await _rolService.GetRolesAsync() ?? new List<Rol>();
                var rolUsuario = roles.FirstOrDefault(r => r.Nombre == "Usuario")
                    ?? await _rolService.CreateRolAsync(new Rol
                    {
                        Nombre = "Usuario",
                        Descripcion = "Rol básico de la plataforma",
                        Activo = true
                    });

                await _usuarioRolService.CreateUsuarioRolAsync(new UsuarioRol
                {
                    UsuarioId = nuevoUsuario.UsuarioId,
                    RolId = rolUsuario.RolId,
                    FechaAsignacion = DateTime.UtcNow
                });

                await SignInUser(nuevoUsuario, new List<UsuarioRol>
                {
                    new UsuarioRol { UsuarioId = nuevoUsuario.UsuarioId, RolId = rolUsuario.RolId }
                });

                TempData["SuccessMessage"] = "Usuario registrado correctamente";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar nuevo usuario");
                ModelState.AddModelError("", "Ocurrió un error al registrar el usuario");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("Usuario cerró sesión");
            return RedirectToAction("Login", "Auth");
        }

        // ===================== Helpers de seguridad =====================

        private static string NormalizeEmail(string? email) =>
            (email ?? string.Empty).Trim().ToLowerInvariant();

        private static string SafeTrim(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            // Lista blanca simple (ajusta si necesitas más caracteres)
            var cleaned = Regex.Replace(s, @"[^a-zA-Z0-9 áéíóúÁÉÍÓÚñÑ\.\-_'@]", string.Empty);
            return cleaned.Trim();
        }

        private static string HashPassword(string plain) =>
            BCryptNet.HashPassword(plain, workFactor: 12);

        private static bool VerifyPassword(string plain, string hashed) =>
            !string.IsNullOrWhiteSpace(hashed) && BCryptNet.Verify(plain, hashed);

        private async Task SignInUser(Usuario usuario, List<UsuarioRol> usuarioRoles)
        {
            // Evita CS8602 (usa ?? "")
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, $"{usuario.Nombre ?? ""} {usuario.Apellido ?? ""}".Trim())
            };

            foreach (var ur in usuarioRoles)
            {
                try
                {
                    var rol = await _rolService.GetRolByIdAsync(ur.RolId);
                    if (rol != null && rol.Activo == true) // bool? -> == true
                    {
                        claims.Add(new Claim(ClaimTypes.Role, rol.Nombre ?? "Usuario"));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error obteniendo el rol {RolId}", ur.RolId);
                }
            }

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

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
    }
}
