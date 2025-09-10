using ClosedXML.Excel;
using donacionesWeb.Models;
using donacionesWeb.Models.ViewModels;
using donacionesWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Linq;
using System.Security.Claims;

namespace donacionesWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AsignacionesController : Controller
    {
        private readonly AsignacionService _asignacionService;
        private readonly CampaniaService _campaniaService;
        private readonly DetallesAsignacionService _detallesAsignacionService;
        private readonly DonacionAsignacionService _donacionAsignacionService;
        private readonly SaldosDonacionService _saldosDonacionService;
        private readonly DonacionService _donacionService;
        private readonly SupabaseStorageService _supabaseStorageService;

        public AsignacionesController(
            AsignacionService asignacionService,
            CampaniaService campaniaService,
            DetallesAsignacionService detallesAsignacionService,
            DonacionAsignacionService donacionAsignacionService,
            SaldosDonacionService saldosDonacionService,
            DonacionService donacionService,
            SupabaseStorageService supabaseStorageService)
        {
            _asignacionService = asignacionService;
            _campaniaService = campaniaService;
            _detallesAsignacionService = detallesAsignacionService;
            _donacionAsignacionService = donacionAsignacionService;
            _saldosDonacionService = saldosDonacionService;
            _donacionService = donacionService;
            _supabaseStorageService = supabaseStorageService;
        }

        [HttpGet]
        public async Task<IActionResult> CrearPaso1()
        {
            var campanias = await _campaniaService.GetAllAsync();
            ViewBag.Campanias = campanias.Select(c => new SelectListItem
            {
                Value = c.CampaniaId.ToString(),
                Text = c.Titulo
            }).ToList();

            return View("AsignarPaso1", new Asignacione());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearPaso1(Asignacione model, IFormFile imagen)
        {
            model.UsuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            model.FechaAsignacion = DateTime.Now;
            model.Monto = 0;

            if (imagen != null && imagen.Length > 0)
            {
                model.ImagenUrl = await _supabaseStorageService.SubirImagenAsync(imagen, "asignaciones");
            }
            else
            {
                model.ImagenUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/ac/No_image_available.svg/480px-No_image_available.svg.png";
            }

            await _asignacionService.CreateAsync(model);

            var nueva = (await _asignacionService.GetAllAsync()).OrderByDescending(a => a.AsignacionId).FirstOrDefault();

            return RedirectToAction("AgregarDetalles", new { id = nueva?.AsignacionId });
        }

        [HttpGet]
        public async Task<IActionResult> AgregarDetalles(int id)
        {
            var asignacion = await _asignacionService.GetByIdAsync(id);
            var detalles = (await _detallesAsignacionService.GetDetallesAsync())
                .Where(d => d.AsignacionId == id).ToList();
            var donacionesAsignadas = (await _donacionAsignacionService.GetAllAsync())
                .Where(d => d.AsignacionId == id).ToList();

            var vm = new DetallesAsignacionViewModel
            {
                Asignacion = asignacion,
                Detalles = detalles,
                DonacionesAsignadas = donacionesAsignadas,
                NuevoDetalle = new DetallesAsignacion { AsignacionId = id }
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarDetalles(DetallesAsignacionViewModel model, IFormFile imagen)
        {
            if (imagen != null && imagen.Length > 0)
            {
                model.NuevoDetalle.ImagenUrl = await _supabaseStorageService.SubirImagenAsync(imagen, "detallesAsignaciones");
            }
            else
            {
                model.NuevoDetalle.ImagenUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/ac/No_image_available.svg/480px-No_image_available.svg.png";
            }

            await _detallesAsignacionService.CreateDetalleAsync(model.NuevoDetalle);

            var detallesAsignacion = await _detallesAsignacionService.GetByAsignacionAsync(model.NuevoDetalle.AsignacionId);
            decimal total = detallesAsignacion.Sum(d => d.Cantidad * d.PrecioUnitario);

            var asignacion = await _asignacionService.GetByIdAsync(model.NuevoDetalle.AsignacionId);
            asignacion.Monto = total;
            await _asignacionService.UpdateAsync(asignacion.AsignacionId, asignacion);

            TempData["SuccessMessage"] = "Detalle agregado y monto actualizado correctamente.";
            return RedirectToAction("AgregarDetalles", new { id = asignacion.AsignacionId });
        }



        [HttpGet]
        public async Task<IActionResult> AsignarDonacion(int id)
        {
            var asignacion = await _asignacionService.GetByIdAsync(id);
            var yaAsignado = (await _donacionAsignacionService.GetAllAsync())
                .Where(d => d.AsignacionId == id)
                .Sum(d => d.MontoAsignado);

            var faltante = asignacion.Monto - yaAsignado;

            var todasLasDonaciones = await _donacionService.GetAllAsync();
            foreach (var don in todasLasDonaciones)
            {
                var saldo = await _saldosDonacionService.GetByDonacionIdAsync(don.DonacionId);
                if (saldo == null)
                {
                    await _saldosDonacionService.CreateAsync(new SaldosDonacione
                    {
                        DonacionId = don.DonacionId,
                        MontoOriginal = don.Monto,
                        MontoUtilizado = 0,
                        SaldoDisponible = don.Monto,
                        UltimaActualizacion = DateTime.Now
                    });
                }
            }

            var saldos = await _saldosDonacionService.GetAllAsync();
            var disponibles = saldos
                .Where(s => s.SaldoDisponible > 0)
                .Join(todasLasDonaciones, s => s.DonacionId, d => d.DonacionId, (s, d) => new { Saldo = s, Donacion = d })
                .Where(x => x.Donacion.CampaniaId == asignacion.CampaniaId)
                .GroupBy(x => x.Saldo.DonacionId)
                .Select(g => g.OrderByDescending(x => x.Saldo.SaldoId).First().Saldo)
                .ToList();

            ViewBag.AsignacionId = id;
            ViewBag.MontoAsignacion = asignacion.Monto;
            ViewBag.YaAsignado = yaAsignado;
            ViewBag.Faltante = faltante;
            ViewBag.Donaciones = disponibles.Select(s => new SelectListItem
            {
                Value = s.DonacionId.ToString(),
                Text = $"Donación #{s.DonacionId} - Disponible: Bs{s.SaldoDisponible:F2}"
            }).ToList();

            return View(new DonacionesAsignacione { AsignacionId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarDonacion(DonacionesAsignacione model)
        {
            var saldo = await _saldosDonacionService.GetByDonacionIdAsync(model.DonacionId);
            if (saldo == null)
            {
                ModelState.AddModelError("", "No se encontró el saldo de la donación.");
                return await AsignarDonacion(model.AsignacionId);
            }

            var asignacion = await _asignacionService.GetByIdAsync(model.AsignacionId);
            if (asignacion == null)
            {
                ModelState.AddModelError("", "Asignación no encontrada.");
                return await AsignarDonacion(model.AsignacionId);
            }

            var yaAsignado = (await _donacionAsignacionService.GetAllAsync())
                .Where(d => d.AsignacionId == model.AsignacionId)
                .Sum(d => d.MontoAsignado);

            var disponibleParaAsignar = asignacion.Monto - yaAsignado;

            if (disponibleParaAsignar <= 0)
            {
                ModelState.AddModelError("", "Ya se ha asignado el total requerido. No se puede asignar más.");
                return await AsignarDonacion(model.AsignacionId);
            }

            if (model.MontoAsignado <= 0 || model.MontoAsignado > disponibleParaAsignar || model.MontoAsignado > saldo.SaldoDisponible)
            {
                ModelState.AddModelError("", $"Monto inválido. Puede asignar hasta Bs {Math.Min(disponibleParaAsignar, saldo.SaldoDisponible):F2}.");
                return await AsignarDonacion(model.AsignacionId);
            }

            model.FechaAsignacion = DateTime.Now;
            await _donacionAsignacionService.CreateAsync(model);

            saldo.SaldoDisponible -= model.MontoAsignado;
            saldo.MontoUtilizado += model.MontoAsignado;
            saldo.UltimaActualizacion = DateTime.Now;
            await _saldosDonacionService.UpdateAsync(saldo.SaldoId, saldo);

            var donacion = await _donacionService.GetByIdAsync(model.DonacionId);
            donacion.EstadoId = saldo.SaldoDisponible <= 0 ? 4 : 3;
            await _donacionService.UpdateAsync(donacion.DonacionId, donacion);

            TempData["SuccessMessage"] = "Donación asignada correctamente.";
            return RedirectToAction("Index", "Asignaciones");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var asignaciones = await _asignacionService.GetAllAsync();
            var campanias = await _campaniaService.GetAllAsync();
            var donacionesAsignadas = await _donacionAsignacionService.GetAllAsync();

            var modelos = asignaciones.Select(a =>
            {
                var campania = campanias.FirstOrDefault(c => c.CampaniaId == a.CampaniaId);
                var totalAsignado = donacionesAsignadas
                    .Where(d => d.AsignacionId == a.AsignacionId)
                    .Sum(d => d.MontoAsignado);

                return new AsignacionIndexViewModel
                {
                    Asignacion = a,
                    CampaniaTitulo = campania?.Titulo ?? "(Sin campaña)",
                    TotalAsignado = totalAsignado
                };
            }).ToList();

            return View(modelos);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                await _asignacionService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Asignación eliminada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al eliminar: {ex.Message}";
            }

            return RedirectToAction("Index");
        }



        [HttpGet]
        public async Task<IActionResult> DescargarReporteExcel()
        {
            var asignaciones = await _asignacionService.GetAllAsync();
            var detalles = await _detallesAsignacionService.GetDetallesAsync();
            var campanias = await _campaniaService.GetAllAsync();
            var asignados = await _donacionAsignacionService.GetAllAsync();

            var reporte = asignaciones.Select(a => new AsignacionReporteViewModel
            {
                AsignacionId = a.AsignacionId,
                Descripcion = a.Descripcion,
                Campania = campanias.FirstOrDefault(c => c.CampaniaId == a.CampaniaId)?.Titulo ?? "Desconocida",
                Fecha = a.FechaAsignacion ?? DateTime.MinValue,
                MontoTotal = a.Monto,
                MontoAsignado = asignados.Where(x => x.AsignacionId == a.AsignacionId).Sum(x => x.MontoAsignado),
                Detalles = detalles.Where(d => d.AsignacionId == a.AsignacionId).Select(d =>
                    $"{d.Concepto} ({d.Cantidad} x Bs {d.PrecioUnitario:F2})").ToList()
            }).ToList();

            // Aquí usarías una librería como ClosedXML para crear el archivo Excel
            using var wb = new ClosedXML.Excel.XLWorkbook();
            var ws = wb.Worksheets.Add("Asignaciones");
            ws.Cell(1, 1).Value = "ID";
            ws.Cell(1, 2).Value = "Descripción";
            ws.Cell(1, 3).Value = "Campaña";
            ws.Cell(1, 4).Value = "Fecha";
            ws.Cell(1, 5).Value = "Monto Total";
            ws.Cell(1, 6).Value = "Monto Asignado";
            ws.Cell(1, 7).Value = "Ítems";

            int row = 2;
            foreach (var a in reporte)
            {
                ws.Cell(row, 1).Value = a.AsignacionId;
                ws.Cell(row, 2).Value = a.Descripcion;
                ws.Cell(row, 3).Value = a.Campania;
                ws.Cell(row, 4).Value = a.Fecha.ToShortDateString();
                ws.Cell(row, 5).Value = a.MontoTotal;
                ws.Cell(row, 6).Value = a.MontoAsignado;
                ws.Cell(row, 7).Value = string.Join("; ", a.Detalles);
                row++;
            }

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Reporte_Asignaciones_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> DescargarReportePdf()
        {
            var asignaciones = await _asignacionService.GetAllAsync();
            var detalles = await _detallesAsignacionService.GetDetallesAsync();
            var campanias = await _campaniaService.GetAllAsync();
            var asignados = await _donacionAsignacionService.GetAllAsync();

            var reporte = asignaciones.Select(a => new AsignacionReporteViewModel
            {
                AsignacionId = a.AsignacionId,
                Descripcion = a.Descripcion,
                Campania = campanias.FirstOrDefault(c => c.CampaniaId == a.CampaniaId)?.Titulo ?? "Desconocida",
                Fecha = a.FechaAsignacion ?? DateTime.MinValue,
                MontoTotal = a.Monto,
                MontoAsignado = asignados.Where(x => x.AsignacionId == a.AsignacionId).Sum(x => x.MontoAsignado),
                Detalles = detalles
                    .Where(d => d.AsignacionId == a.AsignacionId)
                    .Select(d => $"{d.Concepto} ({d.Cantidad} x Bs {d.PrecioUnitario:F2})")
                    .ToList()
            }).ToList();

            // Usar QuestPDF
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.Header().Text("Reporte de Asignaciones").FontSize(20).Bold();
                    page.Content().Column(col =>
                    {
                        foreach (var a in reporte)
                        {
                            col.Item().PaddingBottom(10).Column(inner =>
                            {
                                inner.Item().Text($"ID: {a.AsignacionId}").Bold();
                                inner.Item().Text($"Descripción: {a.Descripcion}");
                                inner.Item().Text($"Campaña: {a.Campania}");
                                inner.Item().Text($"Fecha: {a.Fecha:dd/MM/yyyy}");
                                inner.Item().Text($"Monto Total: Bs {a.MontoTotal}");
                                inner.Item().Text($"Monto Asignado: Bs {a.MontoAsignado}");
                                inner.Item().Text($"Ítems: {string.Join("; ", a.Detalles)}");
                                inner.Item().LineHorizontal(1).LineColor("#DDD");
                            });
                        }
                    });
                });
            });

            var pdfStream = new MemoryStream();
            document.GeneratePdf(pdfStream);
            pdfStream.Position = 0;

            return File(pdfStream.ToArray(), "application/pdf", $"Reporte_Asignaciones_{DateTime.Now:yyyyMMdd}.pdf");
        }





    }
}
