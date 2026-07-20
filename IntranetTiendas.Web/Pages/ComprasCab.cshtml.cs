using System.Globalization;
using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/ComprasCab.asp.
/// Procedure: asp.PROC_MovCompraCab_Select_Alm.
/// Parámetros de URL P2 (fecha tope) y P3 (albarán tope) se conservan para la paginación
/// "Siguiente Pagina" (la página se reenlaza a sí misma con la última fila mostrada).
/// </summary>
public class ComprasCabModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P3 { get; set; }

    public List<dynamic> Compras { get; private set; } = [];
    public string NextP2 { get; private set; } = "";
    public string NextP3 { get; private set; } = "";

    private string? FechaTop => string.IsNullOrEmpty(P2) ? null : P2;
    private long? AlbaranTop => long.TryParse(P3, out var a) ? a : null;

    public string Moneda(object? v) =>
        Convert.ToDecimal(v ?? 0).ToString("C2", CultureInfo.GetCultureInfo("es-ES"));

    public async Task OnGetAsync()
    {
        Compras = await Db.QueryProcAsync("asp.PROC_MovCompraCab_Select_Alm",
            new { Empresa, Alm, FechaTop, AlbaranTop });

        if (Compras.Count > 0)
        {
            var ult = (IDictionary<string, object>)Compras[^1];
            NextP2 = $"{ult["Fecha"]}";
            NextP3 = $"{ult["Albaran"]}";
        }
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.PROC_MovCompraCab_Select_Alm",
            new { Empresa, Alm, FechaTop, AlbaranTop });
        return Excel(rows, "Compras");
    }
}
