using donacionesWeb.Models;
using donacionesWeb.Models.ViewModels;
using donacionesWeb.Services;
using donacionesWeb.Services.Firebase;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace donacionesWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly UsuarioService _usuarioService;
        private readonly UsuarioRolService _usuarioRolService;
        private readonly RolService _rolService;
        private readonly FirebaseStorageService _firebaseStorageService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UsuarioService usuarioService,
            UsuarioRolService usuarioRolService,
            RolService rolService,
            ILogger<AuthController> logger,
            FirebaseStorageService firebaseStorageService)
        {
            _usuarioService = usuarioService;
            _usuarioRolService = usuarioRolService;
            _rolService = rolService;
            _logger = logger;
            _firebaseStorageService = firebaseStorageService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
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
                    _logger.LogInformation("Intentando autenticar al usuario: {Email}", model.Email);

                    var usuario = await _usuarioService.GetUsuarioByEmailAsync(model.Email);

                    if (usuario == null)
                    {
                        _logger.LogWarning("No se encontró el usuario con email: {Email}", model.Email);
                        ModelState.AddModelError(string.Empty, "Credenciales inválidas");
                        return View(model);
                    }

                    _logger.LogInformation("Usuario encontrado: {UsuarioId}, contraseña en BD: {Contrasena}", usuario.UsuarioId, usuario.Contrasena);

                    if (usuario.Contrasena == model.Contrasena)
                    {
                        _logger.LogInformation("Contraseña verificada correctamente");

                        try
                        {
                            var usuarioRoles = await _usuarioRolService.GetUsuariosRolesByUsuarioIdAsync(usuario.UsuarioId);
                            _logger.LogInformation("Roles obtenidos: {RolesCount}", usuarioRoles.Count);

                            await SignInUser(usuario, usuarioRoles);
                            _logger.LogInformation("Usuario {Email} ha iniciado sesión", usuario.Email);
                            return RedirectToLocal(returnUrl);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Error al obtener roles o realizar el inicio de sesión: {Message}", ex.Message);
                            ModelState.AddModelError(string.Empty, "Error al obtener los roles del usuario");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Contraseña incorrecta para el usuario: {Email}", model.Email);
                        ModelState.AddModelError(string.Empty, "Credenciales inválidas");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al intentar iniciar sesión: {Message}", ex.Message);
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
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var existingUser = await _usuarioService.GetUsuarioByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Este correo electrónico ya está registrado");
                    return View(model);
                }

                var usuario = new Usuario
                {
                    Email = model.Email,
                    Contrasena = model.Contrasena,
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    Telefono = model.Telefono,
                    Activo = true,
                    FechaRegistro = DateTime.Now
                };

                if (model.Imagen != null && model.Imagen.Length > 0)
                {
                    usuario.ImagenUrl = await _firebaseStorageService.SubirImagenAsync(model.Imagen, "usuarios");
                }
                else
                {
                    usuario.ImagenUrl = "https://firebasestorage.googleapis.com/v0/b/transparenciadonaciones.appspot.com/o/user_default.jpg?alt=media";
                }

                var nuevoUsuario = await _usuarioService.CreateUsuarioAsync(usuario);

                var roles = await _rolService.GetRolesAsync();
                var rolAdmin = roles.FirstOrDefault(r => r.Nombre == "Admin")
                               ?? await _rolService.CreateRolAsync(new Rol
                               {
                                   Nombre = "Admin",
                                   Descripcion = "Administrador del sistema",
                                   Activo = true
                               });

                await _usuarioRolService.CreateUsuarioRolAsync(new UsuarioRol
                {
                    UsuarioId = nuevoUsuario.UsuarioId,
                    RolId = rolAdmin.RolId,
                    FechaAsignacion = DateTime.Now
                });

                await SignInUser(nuevoUsuario, new List<UsuarioRol>
                {
                    new UsuarioRol { UsuarioId = nuevoUsuario.UsuarioId, RolId = rolAdmin.RolId }
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
            _logger.LogInformation("Usuario ha cerrado sesión");
            return RedirectToAction("Login", "Auth");
        }

        private async Task SignInUser(Usuario usuario, List<UsuarioRol> usuarioRoles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}"),
            };

            foreach (var usuarioRol in usuarioRoles)
            {
                try
                {
                    var rol = await _rolService.GetRolByIdAsync(usuarioRol.RolId);
                    if (rol != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, rol.Nombre));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al obtener el rol con ID {RolId}", usuarioRol.RolId);
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
