using System.Globalization;
using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/InformeManualLin3.asp (detalle de informe manual, tipo 3: stock/compra/pvp con pvp nuevo).
/// Procedures: asp.PROC_InformeManualLin_Select (@Empresa, @Informe).
/// Parámetros de URL P2 (fecha, solo display), P3 (nº informe) y Text (título) se conservan.
/// </summary>
public class InformeManualLin3Model(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P3 { get; set; }
    [BindProperty(SupportsGet = true)] public string? Text { get; set; }

    public List<dynamic> Lineas { get; private set; } = [];
    public decimal Total { get; private set; }

    private long? Informe => long.TryParse(P3, out var i) ? i : null;

    public string Moneda(object? v) =>
        Convert.ToDecimal(v ?? 0).ToString("C2", CultureInfo.GetCultureInfo("es-ES"));

    /// <summary>Antes FormatNumber(x, 0).</summary>
    public string Numero(object? v) =>
        Convert.ToDecimal(v ?? 0).ToString("N0", CultureInfo.GetCultureInfo("es-ES"));

    public async Task OnGetAsync()
    {
        Lineas = await Db.QueryProcAsync("asp.PROC_InformeManualLin_Select", new { Empresa, Informe });
        foreach (var r in Lineas)
        {
            var d = (IDictionary<string, object>)r;
            Total += Convert.ToDecimal(d["PrecioA"] ?? 0) * Convert.ToDecimal(d["Cant"] ?? 0);
        }
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.PROC_InformeManualLin_Select", new { Empresa, Informe });
        return Excel(rows, "InformeManualLin3");
    }
}
