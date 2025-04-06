using donacionesWeb.Models;
using donacionesWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace donacionesWeb.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly UsuarioService _usuarioService;

        public UsuarioController(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var usuarios = await _usuarioService.GetUsuariosAsync();
                return View(usuarios);
            }
            catch (HttpRequestException ex)
            {
                ViewBag.ErrorMessage = "Error al cargar usuarios: " + ex.Message;
                return View(new List<Usuario>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _usuarioService.CreateUsuarioAsync(usuario);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al crear usuario: {ex.Message}");
                }
            }
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _usuarioService.DeleteUsuarioAsync(id);
                if (!success)
                {
                    TempData["ErrorMessage"] = "No se pudo eliminar el usuario";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al eliminar usuario: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
