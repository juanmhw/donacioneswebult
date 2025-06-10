using ClosedXML.Excel;
using donacionesWeb.Models.ViewModels;
using donacionesWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

[Authorize(Roles = "Admin")]
public class RendicionGeneralController : Controller
{
    private readonly CampaniaService _campaniaService;
    private readonly DonacionService _donacionService;
    private readonly AsignacionService _asignacionService;
    private readonly DetallesAsignacionService _detallesService;
    private readonly DonacionAsignacionService _donacionAsignacionService;
    private readonly UsuarioService _usuarioService;

    public RendicionGeneralController(
        CampaniaService campaniaService,
        DonacionService donacionService,
        AsignacionService asignacionService,
        DetallesAsignacionService detallesService,
        DonacionAsignacionService donacionAsignacionService,
        UsuarioService usuarioService)
    {
        _campaniaService = campaniaService;
        _donacionService = donacionService;
        _asignacionService = asignacionService;
        _detallesService = detallesService;
        _donacionAsignacionService = donacionAsignacionService;
        _usuarioService = usuarioService;
    }

    public async Task<IActionResult> Index(string? filtroCampania, DateTime? desde, DateTime? hasta, string? periodo)
    {
        if (!desde.HasValue && !hasta.HasValue && !string.IsNullOrEmpty(periodo))
        {
            var hoy = DateTime.Today;
            if (periodo == "semana")
                desde = hoy.AddDays(-7);
            else if (periodo == "mes")
                desde = new DateTime(hoy.Year, hoy.Month, 1);
            else if (periodo == "trimestre")
            {
                int currentQuarter = (hoy.Month - 1) / 3;
                desde = new DateTime(hoy.Year, currentQuarter * 3 + 1, 1);
            }
            else if (periodo == "anio")
                desde = new DateTime(hoy.Year, 1, 1);
            hasta = hoy;
        }

        var datos = await ObtenerDatosCierreAsync();
        var filtrado = FiltrarDatos(datos, filtroCampania, desde, hasta).ToList();

        ViewBag.CampaniasDisponibles = datos.Select(c => c.Campania).Distinct().ToList();
        ViewBag.FiltroCampania = filtroCampania;
        ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
        ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");
        ViewBag.Periodo = periodo;

        return View("Index", filtrado);
    }

    private IEnumerable<CierreCajaViewModel> AplicarFiltros(IEnumerable<CierreCajaViewModel> datos, string? filtroCampania, DateTime? desde, DateTime? hasta)
    {
        if (!string.IsNullOrEmpty(filtroCampania))
            datos = datos.Where(x => x.Campania.Contains(filtroCampania, StringComparison.OrdinalIgnoreCase));

        if (desde.HasValue)
            datos = datos.Where(x => x.Donaciones.Any() && x.Donaciones.Min(d => d.Fecha) >= desde.Value);

        if (hasta.HasValue)
            datos = datos.Where(x => x.Donaciones.Any() && x.Donaciones.Max(d => d.Fecha) <= hasta.Value);

        return datos;
    }

    private async Task<List<CierreCajaViewModel>> ObtenerDatosCierreAsync()
    {
        var campanias = await _campaniaService.GetAllAsync();
        var donaciones = await _donacionService.GetAllAsync();
        var asignaciones = await _asignacionService.GetAllAsync();
        var detalles = await _detallesService.GetDetallesAsync();
        var donacionesAsignadas = await _donacionAsignacionService.GetAllAsync();
        var usuarios = await _usuarioService.GetUsuariosAsync();

        var modelos = new List<CierreCajaViewModel>();

        foreach (var camp in campanias)
        {
            var donacionesCamp = donaciones.Where(d => d.CampaniaId == camp.CampaniaId).ToList();
            var asignacionesCamp = asignaciones.Where(a => a.CampaniaId == camp.CampaniaId).ToList();
            var detallesAsignados = detalles.Where(d => asignacionesCamp.Any(a => a.AsignacionId == d.AsignacionId)).ToList();
            var asignacionesIds = asignacionesCamp.Select(a => a.AsignacionId).ToList();

            var vm = new CierreCajaViewModel
            {
                Campania = camp.Titulo,
                MontoRecaudado = camp.MontoRecaudado ?? 0,
                MontoDonado = donacionesCamp.Sum(d => d.Monto),
                MontoAsignado = donacionesAsignadas
                    .Where(x => asignacionesIds.Contains(x.AsignacionId))
                    .Sum(x => x.MontoAsignado),

                Donantes = donacionesCamp
                    .Select(d =>
                    {
                        var usuario = usuarios.FirstOrDefault(u => u.UsuarioId == d.UsuarioId);
                        return d.EsAnonima == true || usuario == null
                            ? "Anónimo"
                            : $"{usuario.Nombre} {usuario.Apellido}";
                    })
                    .Distinct()
                    .ToList(),

                DetallesAsignaciones = detallesAsignados.Select(d =>
                {
                    var fechaAsignacion = asignacionesCamp
                        .FirstOrDefault(a => a.AsignacionId == d.AsignacionId)?.FechaAsignacion ?? DateTime.Now;

                    return new ItemAsignadoViewModel
                    {
                        Descripcion = d.Concepto,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Fecha = fechaAsignacion
                    };
                }).ToList(),

                Donaciones = donacionesCamp.Select(d =>
                {
                    var usuario = d.UsuarioId.HasValue
                        ? usuarios.FirstOrDefault(u => u.UsuarioId == d.UsuarioId)
                        : null;

                    var nombre = d.EsAnonima == true || usuario == null
                        ? "Anónimo"
                        : $"{usuario.Nombre} {usuario.Apellido}";

                    var montoAsignado = donacionesAsignadas
                        .Where(da => da.DonacionId == d.DonacionId)
                        .Sum(da => da.MontoAsignado);

                    var detallesAsociados = detalles
                        .Where(det => donacionesAsignadas.Any(da =>
                            da.DonacionId == d.DonacionId && da.AsignacionId == det.AsignacionId))
                        .Select(det => new ItemAsignadoViewModel
                        {
                            Descripcion = det.Concepto,
                            Cantidad = det.Cantidad,
                            PrecioUnitario = det.PrecioUnitario,
                            Fecha = asignaciones.FirstOrDefault(a => a.AsignacionId == det.AsignacionId)?.FechaAsignacion ?? DateTime.Now
                        }).ToList();

                    return new DonacionDetalleViewModel
                    {
                        Nombre = nombre,
                        Fecha = d.FechaDonacion ?? DateTime.Now,
                        Monto = d.Monto,
                        Asignado = montoAsignado,
                        Asignaciones = detallesAsociados
                    };
                }).ToList()
            };

            modelos.Add(vm);
        }

        return modelos;
    }


    private IEnumerable<CierreCajaViewModel> FiltrarDatos(IEnumerable<CierreCajaViewModel> datos, string? filtroCampania, DateTime? desde, DateTime? hasta)
    {
        var resultado = new List<CierreCajaViewModel>();
        bool hayFiltroFecha = desde.HasValue || hasta.HasValue;

        foreach (var item in datos)
        {
            // Filtro por nombre de campaña
            if (!string.IsNullOrEmpty(filtroCampania) &&
                !item.Campania.Contains(filtroCampania, StringComparison.OrdinalIgnoreCase))
                continue;

            // Filtrar donaciones por fecha si se ha definido un rango
            var donacionesFiltradas = item.Donaciones
                .Where(d =>
                    (!desde.HasValue || d.Fecha >= desde.Value) &&
                    (!hasta.HasValue || d.Fecha <= hasta.Value))
                .ToList();

            // Si hay filtros por fecha y no hay donaciones, continuar
            if (hayFiltroFecha && !donacionesFiltradas.Any())
            {
                continue;
            }

            // Filtrar asignaciones que correspondan a las donaciones filtradas
            var detallesAsignacionesFiltradas = item.DetallesAsignaciones
                .Where(a => donacionesFiltradas
                    .Any(d => d.Asignaciones.Any(da => da.Fecha == a.Fecha && da.Descripcion == a.Descripcion)))
                .ToList();

            var nuevo = new CierreCajaViewModel
            {
                Campania = item.Campania,
                MontoRecaudado = item.MontoRecaudado,
                MontoDonado = donacionesFiltradas.Sum(d => d.Monto),
                MontoAsignado = donacionesFiltradas.Sum(d => d.Asignado),
                Donaciones = donacionesFiltradas,
                DetallesAsignaciones = detallesAsignacionesFiltradas,
                Donantes = donacionesFiltradas.Select(d => d.Nombre).Distinct().ToList()
            };

            resultado.Add(nuevo);
        }

        return resultado;
    }





    public async Task<IActionResult> ExportarExcel(string? filtroCampania, DateTime? desde, DateTime? hasta)
    {
        var datos = await ObtenerDatosCierreAsync();
        var filtrado = FiltrarDatos(datos, filtroCampania, desde, hasta);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Informe General");

        // Configurar estilos
        var headerStyle = wb.Style;
        headerStyle.Font.Bold = true;
        headerStyle.Font.FontSize = 14;
        headerStyle.Fill.BackgroundColor = XLColor.FromHtml("#FF6B35"); // Naranja principal
        headerStyle.Font.FontColor = XLColor.White;
        headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerStyle.Border.OutsideBorder = XLBorderStyleValues.Thin;

        var campaignHeaderStyle = wb.Style;
        campaignHeaderStyle.Font.Bold = true;
        campaignHeaderStyle.Font.FontSize = 12;
        campaignHeaderStyle.Fill.BackgroundColor = XLColor.FromHtml("#FFB499"); // Naranja claro
        campaignHeaderStyle.Font.FontColor = XLColor.FromHtml("#8B4513");
        campaignHeaderStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        campaignHeaderStyle.Border.OutsideBorder = XLBorderStyleValues.Thin;

        var summaryStyle = wb.Style;
        summaryStyle.Font.FontSize = 10;
        summaryStyle.Fill.BackgroundColor = XLColor.FromHtml("#FFF2E6"); // Naranja muy claro
        summaryStyle.Border.OutsideBorder = XLBorderStyleValues.Thin;

        var dataStyle = wb.Style;
        dataStyle.Font.FontSize = 10;
        dataStyle.Border.OutsideBorder = XLBorderStyleValues.Thin;
        dataStyle.Alignment.Vertical = XLAlignmentVerticalValues.Top;

        // Configurar columnas
        ws.Column("A").Width = 25; // Concepto
        ws.Column("B").Width = 20; // Valor/Fecha
        ws.Column("C").Width = 15; // Monto
        ws.Column("D").Width = 15; // Asignado
        ws.Column("E").Width = 15; // Saldo
        ws.Column("F").Width = 30; // Descripción

        int row = 1;

        // Título principal
        ws.Cell(row, 1).Value = "INFORME GENERAL DE RENDICIÓN DE CUENTAS";
        ws.Range(row, 1, row, 6).Merge();
        ws.Cell(row, 1).Style = headerStyle;
        ws.Row(row).Height = 25;
        row += 2;

        // Información de filtros aplicados
        ws.Cell(row, 1).Value = "Filtros Aplicados:";
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;

        if (!string.IsNullOrEmpty(filtroCampania))
        {
            ws.Cell(row, 1).Value = $"Campaña: {filtroCampania}";
            row++;
        }

        if (desde.HasValue)
        {
            ws.Cell(row, 1).Value = $"Desde: {desde.Value:dd/MM/yyyy}";
            row++;
        }

        if (hasta.HasValue)
        {
            ws.Cell(row, 1).Value = $"Hasta: {hasta.Value:dd/MM/yyyy}";
            row++;
        }

        ws.Cell(row, 1).Value = $"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}";
        row += 2;

        // Resumen general
        var totalDonado = filtrado.Sum(x => x.MontoDonado);
        var totalAsignado = filtrado.Sum(x => x.MontoAsignado);
        var totalSaldo = filtrado.Sum(x => x.SaldoRestante);

        ws.Cell(row, 1).Value = "RESUMEN GENERAL";
        ws.Range(row, 1, row, 6).Merge();
        ws.Cell(row, 1).Style = campaignHeaderStyle;
        row++;

        // Headers de resumen
        ws.Cell(row, 1).Value = "Concepto";
        ws.Cell(row, 2).Value = "Total Donado (Bs)";
        ws.Cell(row, 3).Value = "Total Asignado (Bs)";
        ws.Cell(row, 4).Value = "Saldo Restante (Bs)";
        ws.Range(row, 1, row, 4).Style = summaryStyle;
        row++;

        ws.Cell(row, 1).Value = "TOTALES";
        ws.Cell(row, 2).Value = totalDonado;
        ws.Cell(row, 3).Value = totalAsignado;
        ws.Cell(row, 4).Value = totalSaldo;
        ws.Range(row, 1, row, 4).Style = dataStyle;
        ws.Range(row, 2, row, 4).Style.NumberFormat.Format = "#,##0.00";
        row += 2;

        // Detalle por campaña
        foreach (var camp in filtrado)
        {
            // Header de campaña
            ws.Cell(row, 1).Value = $"CAMPAÑA: {camp.Campania}";
            ws.Range(row, 1, row, 6).Merge();
            ws.Cell(row, 1).Style = campaignHeaderStyle;
            row++;

            // Resumen de campaña
            ws.Cell(row, 1).Value = "Resumen";
            ws.Cell(row, 2).Value = "Donado";
            ws.Cell(row, 3).Value = "Asignado";
            ws.Cell(row, 4).Value = "Saldo";
            ws.Range(row, 1, row, 4).Style = summaryStyle;
            row++;

            ws.Cell(row, 1).Value = camp.Campania;
            ws.Cell(row, 2).Value = camp.MontoDonado;
            ws.Cell(row, 3).Value = camp.MontoAsignado;
            ws.Cell(row, 4).Value = camp.SaldoRestante;
            ws.Range(row, 1, row, 4).Style = dataStyle;
            ws.Range(row, 2, row, 4).Style.NumberFormat.Format = "#,##0.00";
            row += 2;

            // Headers de donaciones detalladas
            ws.Cell(row, 1).Value = "Donante";
            ws.Cell(row, 2).Value = "Fecha";
            ws.Cell(row, 3).Value = "Monto (Bs)";
            ws.Cell(row, 4).Value = "Asignado (Bs)";
            ws.Cell(row, 5).Value = "Saldo (Bs)";
            ws.Cell(row, 6).Value = "Observaciones";
            ws.Range(row, 1, row, 6).Style = summaryStyle;
            row++;

            // Detalle de donaciones
            foreach (var don in camp.Donaciones)
            {
                ws.Cell(row, 1).Value = don.Nombre;
                ws.Cell(row, 2).Value = don.Fecha.ToString("dd/MM/yyyy");
                ws.Cell(row, 3).Value = don.Monto;
                ws.Cell(row, 4).Value = don.Asignado;
                ws.Cell(row, 5).Value = don.Saldo;
                ws.Range(row, 1, row, 6).Style = dataStyle;
                ws.Range(row, 3, row, 5).Style.NumberFormat.Format = "#,##0.00";
                row++;

                // Asignaciones detalladas
                if (don.Asignaciones != null && don.Asignaciones.Any())
                {
                    // Subheader para asignaciones
                    ws.Cell(row, 1).Value = "ASIGNACIONES:";
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    ws.Cell(row, 1).Style.Font.Italic = true;
                    row++;

                    foreach (var asig in don.Asignaciones)
                    {
                        ws.Cell(row, 1).Value = $"  • {asig.Descripcion}";
                        ws.Cell(row, 2).Value = asig.Fecha.ToString("dd/MM/yyyy");
                        ws.Cell(row, 3).Value = $"Cant: {asig.Cantidad}";
                        ws.Cell(row, 4).Value = asig.PrecioUnitario;
                        ws.Cell(row, 5).Value = asig.Total;
                        ws.Range(row, 1, row, 6).Style = dataStyle;
                        ws.Range(row, 4, row, 5).Style.NumberFormat.Format = "#,##0.00";
                        row++;
                    }
                }
            }
            row += 2; // Espacio entre campañas
        }

        // Ajustar filas
        ws.Rows().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        stream.Position = 0;

        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"InformeGeneral_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    public async Task<IActionResult> ExportarPdf(string? filtroCampania, DateTime? desde, DateTime? hasta)
    {
        var datos = await ObtenerDatosCierreAsync();
        var filtrado = FiltrarDatos(datos, filtroCampania, desde, hasta).ToList();

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);

                // ✅ HEADER corregido
                page.Header().Height(80).Background(Colors.Orange.Medium).Padding(15).Column(col =>
                {
                    col.Item().Text("DONACIONES BENI").FontSize(18).Bold().FontColor(Colors.White);
                    col.Item().Text("Informe General de Rendición de Cuentas").FontSize(12).FontColor(Colors.White);
                });

                // ✅ FOOTER sin cambios
                // Footer corregido con más espacio
                page.Footer().Height(35).Background(Colors.Orange.Lighten2).PaddingVertical(5).PaddingHorizontal(8)
                    .AlignCenter().Text(text =>
                    {
                        text.Span("Página ").FontSize(9);
                        text.CurrentPageNumber().FontSize(9);
                        text.Span(" de ").FontSize(9);
                        text.TotalPages().FontSize(9);
                    });


                // ✅ CONTENIDO
                page.Content().Padding(20).Column(mainCol =>
                {
                    // Información de filtros y fecha
                    mainCol.Item().Padding(10).Column(infoCol =>
                    {
                        infoCol.Item().Text($"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10).Bold();

                        if (!string.IsNullOrEmpty(filtroCampania) || desde.HasValue || hasta.HasValue)
                        {
                            infoCol.Item().Text("FILTROS APLICADOS:").FontSize(11).Bold();

                            if (!string.IsNullOrEmpty(filtroCampania))
                                infoCol.Item().Text($"• Campaña: {filtroCampania}").FontSize(10);

                            if (desde.HasValue)
                                infoCol.Item().Text($"• Desde: {desde.Value:dd/MM/yyyy}").FontSize(10);

                            if (hasta.HasValue)
                                infoCol.Item().Text($"• Hasta: {hasta.Value:dd/MM/yyyy}").FontSize(10);
                        }
                    });

                    mainCol.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Orange.Medium);

                    // RESUMEN GENERAL
                    var totalDonado = filtrado.Sum(x => x.MontoDonado);
                    var totalAsignado = filtrado.Sum(x => x.MontoAsignado);
                    var totalSaldo = filtrado.Sum(x => x.SaldoRestante);

                    mainCol.Item().Background(Colors.Orange.Lighten4).Padding(15).Column(summaryCol =>
                    {
                        summaryCol.Item().Text("RESUMEN GENERAL").FontSize(14).Bold();

                        summaryCol.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Orange.Medium).Padding(8)
                                    .Text("Concepto").FontSize(10).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Orange.Medium).Padding(8)
                                    .Text("Donado (Bs)").FontSize(10).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Orange.Medium).Padding(8)
                                    .Text("Asignado (Bs)").FontSize(10).Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Orange.Medium).Padding(8)
                                    .Text("Saldo (Bs)").FontSize(10).Bold().FontColor(Colors.White);
                            });

                            table.Cell().Padding(8).Text("TOTALES").FontSize(10).Bold();
                            table.Cell().Padding(8).AlignRight().Text($"{totalDonado:N2}").FontSize(10);
                            table.Cell().Padding(8).AlignRight().Text($"{totalAsignado:N2}").FontSize(10);
                            table.Cell().Padding(8).AlignRight().Text($"{totalSaldo:N2}").FontSize(10);
                        });
                    });

                    mainCol.Item().PaddingVertical(15).LineHorizontal(1).LineColor(Colors.Orange.Medium);

                    // DETALLE POR CAMPAÑA
                    bool isFirst = true;
                    foreach (var camp in filtrado)
                    {
                        if (!isFirst)
                            mainCol.Item().PageBreak();
                        isFirst = false;

                        mainCol.Item().Column(campCol =>
                        {
                            campCol.Item().Background(Colors.Orange.Lighten2).Padding(12)
                                .Text($"CAMPAÑA: {camp.Campania}").FontSize(12).Bold();

                            // RESUMEN POR CAMPAÑA
                            campCol.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Border(0.5f).Padding(6).Background(Colors.Orange.Lighten3)
                                        .Text("Donado (Bs)").FontSize(9).Bold();
                                    header.Cell().Border(0.5f).Padding(6).Background(Colors.Orange.Lighten3)
                                        .Text("Asignado (Bs)").FontSize(9).Bold();
                                    header.Cell().Border(0.5f).Padding(6).Background(Colors.Orange.Lighten3)
                                        .Text("Saldo (Bs)").FontSize(9).Bold();
                                });

                                table.Cell().Border(0.5f).Padding(6).AlignRight().Text($"{camp.MontoDonado:N2}").FontSize(9);
                                table.Cell().Border(0.5f).Padding(6).AlignRight().Text($"{camp.MontoAsignado:N2}").FontSize(9);
                                table.Cell().Border(0.5f).Padding(6).AlignRight().Text($"{camp.SaldoRestante:N2}").FontSize(9);
                            });

                            // DETALLE DE DONACIONES
                            campCol.Item().PaddingTop(15).Text("DETALLE DE DONACIONES").FontSize(11).Bold();

                            foreach (var don in camp.Donaciones)
                            {
                                campCol.Item().PaddingTop(10).Border(0.5f).Padding(10).Column(donCol =>
                                {
                                    donCol.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text($"Donante: {don.Nombre}").FontSize(10).Bold();
                                        row.ConstantItem(80).AlignRight().Text($"{don.Fecha:dd/MM/yyyy}").FontSize(9);
                                    });

                                    donCol.Item().PaddingTop(5).Row(row =>
                                    {
                                        row.RelativeItem().Text($"Monto: Bs {don.Monto:N2}").FontSize(9);
                                        row.RelativeItem().Text($"Asignado: Bs {don.Asignado:N2}").FontSize(9);
                                        row.RelativeItem().Text($"Saldo: Bs {don.Saldo:N2}").FontSize(9);
                                    });

                                    if (don.Asignaciones != null && don.Asignaciones.Any())
                                    {
                                        donCol.Item().PaddingTop(8).Text("ASIGNACIONES:").FontSize(9).Bold();

                                        foreach (var asig in don.Asignaciones)
                                        {
                                            donCol.Item().PaddingLeft(15).PaddingTop(3).Text(
                                                $"• {asig.Descripcion} | Cant: {asig.Cantidad} | Unit: Bs {asig.PrecioUnitario:N2} | Total: Bs {asig.Total:N2} | {asig.Fecha:dd/MM/yyyy}"
                                            ).FontSize(8);
                                        }
                                    }
                                });
                            }
                        });
                    }
                });
            });
        });

        var stream = new MemoryStream();
        pdf.GeneratePdf(stream);
        stream.Position = 0;

        return File(stream.ToArray(), "application/pdf", $"InformeGeneral_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }


}
