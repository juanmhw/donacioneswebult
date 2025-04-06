using donacionesWeb.Models;
using donacionesWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace donacionesWeb.Controllers
{
    public class RespuestaMensajeController : Controller
    {
        private readonly RespuestaMensajeService _service;

        public RespuestaMensajeController(RespuestaMensajeService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var respuestas = await _service.GetAllAsync();  // Cambiado para usar el nuevo método
            return View(respuestas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RespuestasMensaje respuesta)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    respuesta.FechaRespuesta = DateTime.Now;
                    await _service.CreateAsync(respuesta);

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true });
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = ex.Message });
                    }
                    TempData["ErrorMessage"] = $"Error al crear respuesta: {ex.Message}";
                }
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = false,
                    message = "Datos inválidos",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _service.DeleteAsync(id);
                if (!success)
                {
                    TempData["ErrorMessage"] = "No se pudo eliminar la respuesta";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al eliminar respuesta: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
