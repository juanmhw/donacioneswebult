using donacionesWeb.Models;
using donacionesWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace donacionesWeb.Controllers
{
    public class SaldosDonacionController : Controller
    {
        private readonly SaldosDonacionService _service;

        public SaldosDonacionController(SaldosDonacionService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var saldos = await _service.GetAllAsync();
            return View(saldos);
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
                    TempData["ErrorMessage"] = "No se pudo eliminar el saldo";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al eliminar saldo: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
