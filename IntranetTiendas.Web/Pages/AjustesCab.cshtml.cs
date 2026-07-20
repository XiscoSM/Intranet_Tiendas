using System.Globalization;
using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/AjustesCab.asp.
/// Procedures: asp.PROC_MovAjusteCab_Select_Alm.
/// Parámetros de URL P2 (fecha tope) y P3 (ajuste tope) se conservan para la
/// paginación "Siguiente Página" (en origen iban como NULL cuando faltaban).
/// </summary>
public class AjustesCabModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P3 { get; set; }

    public List<dynamic> Ajustes { get; private set; } = [];

    /// <summary>Valores de la última fila, para el enlace "Siguiente Página" (igual que el ASP).</summary>
    public string NextP2 { get; private set; } = "";
    public string NextP3 { get; private set; } = "";

    private string? FechaTop => string.IsNullOrEmpty(P2) ? null : P2;
    private long? AjusteTop => long.TryParse(P3, out var t) ? t : null;

    public string Moneda(object? v) =>
        Convert.ToDecimal(v ?? 0).ToString("C2", CultureInfo.GetCultureInfo("es-ES"));

    public async Task OnGetAsync()
    {
        Ajustes = await Db.QueryProcAsync("asp.PROC_MovAjusteCab_Select_Alm",
            new { Empresa, Alm, FechaTop, AjusteTop });

        if (Ajustes.Count > 0)
        {
            var last = (IDictionary<string, object>)Ajustes[^1];
            NextP2 = $"{last["Fecha"]}";
            NextP3 = $"{last["Ajuste"]}";
        }
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.PROC_MovAjusteCab_Select_Alm",
            new { Empresa, Alm, FechaTop, AjusteTop });
        return Excel(rows, "Ajustes");
    }
}
