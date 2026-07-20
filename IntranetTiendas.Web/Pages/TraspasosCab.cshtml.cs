using System.Globalization;
using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/TraspasosCab.asp.
/// Procedures: asp.PROC_MovTraspasoCab_Select_Alm.
/// Parámetros de URL P2 (fecha tope) y P3 (traspaso tope) se conservan para la
/// paginación "Siguiente Página" (en origen iban como NULL cuando faltaban).
/// </summary>
public class TraspasosCabModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P3 { get; set; }

    public List<dynamic> Traspasos { get; private set; } = [];

    /// <summary>Valores de la última fila, para el enlace "Siguiente Página" (igual que el ASP).</summary>
    public string NextP2 { get; private set; } = "";
    public string NextP3 { get; private set; } = "";

    private string? FechaTop => string.IsNullOrEmpty(P2) ? null : P2;
    private long? TraspasoTop => long.TryParse(P3, out var t) ? t : null;

    public string Moneda(object? v) =>
        Convert.ToDecimal(v ?? 0).ToString("C2", CultureInfo.GetCultureInfo("es-ES"));

    public async Task OnGetAsync()
    {
        Traspasos = await Db.QueryProcAsync("asp.PROC_MovTraspasoCab_Select_Alm",
            new { Empresa, Alm, FechaTop, TraspasoTop });

        if (Traspasos.Count > 0)
        {
            var last = (IDictionary<string, object>)Traspasos[^1];
            NextP2 = $"{last["Fecha"]}";
            NextP3 = $"{last["Traspaso"]}";
        }
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.PROC_MovTraspasoCab_Select_Alm",
            new { Empresa, Alm, FechaTop, TraspasoTop });
        return Excel(rows, "Traspasos");
    }
}
