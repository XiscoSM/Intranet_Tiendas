using System.Globalization;
using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/AjustesLin.asp.
/// Procedures: asp.PROC_MovAjusteLin_Select.
/// Parámetros de URL P2 (fecha), P3 (ajuste) y Text (descripción tipo-programa,
/// solo presentación) se conservan para mantener los enlaces existentes.
/// </summary>
public class AjustesLinModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P3 { get; set; }
    [BindProperty(SupportsGet = true)] public string? Text { get; set; }

    public List<dynamic> Lineas { get; private set; } = [];

    /// <summary>Suma de ImporteCoste de todas las líneas (variable Total del ASP).</summary>
    public decimal Total { get; private set; }

    private string? Fecha => string.IsNullOrEmpty(P2) ? null : P2;
    private long? Ajuste => long.TryParse(P3, out var t) ? t : null;

    public string Moneda(object? v) =>
        Convert.ToDecimal(v ?? 0).ToString("C2", CultureInfo.GetCultureInfo("es-ES"));

    public async Task OnGetAsync()
    {
        Lineas = await Db.QueryProcAsync("asp.PROC_MovAjusteLin_Select",
            new { Empresa, Fecha, Ajuste });

        Total = Lineas
            .Cast<IDictionary<string, object>>()
            .Sum(d => Convert.ToDecimal(d["ImporteCoste"] ?? 0));
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.PROC_MovAjusteLin_Select",
            new { Empresa, Fecha, Ajuste });
        return Excel(rows, $"Ajuste_{Ajuste}");
    }
}
