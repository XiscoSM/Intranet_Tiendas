using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/InformeManualCab.asp.
/// Procedures: asp.PROC_InformeManualCab_Select (@Empresa, @Alm, @FechaTop, @InformeTop).
/// Parámetros de URL P2 (fecha top) y P3 (informe top) se conservan para la paginación.
/// </summary>
public class InformeManualCabModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P3 { get; set; }

    public List<dynamic> Informes { get; private set; } = [];

    /// <summary>P2/P3 de la última fila para el enlace "Siguiente Pagina".</summary>
    public string SigP2 { get; private set; } = "";
    public string SigP3 { get; private set; } = "";

    private string? FechaTop => string.IsNullOrEmpty(P2) ? null : P2;
    private long? InformeTop => long.TryParse(P3, out var i) ? i : null;

    public async Task OnGetAsync()
    {
        Informes = await Db.QueryProcAsync("asp.PROC_InformeManualCab_Select",
            new { Empresa, Alm, FechaTop, InformeTop });

        if (Informes.Count > 0)
        {
            var d = (IDictionary<string, object>)Informes[^1];
            SigP2 = $"{d["Fecha"]}";
            SigP3 = $"{d["Informe"]}";
        }
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.PROC_InformeManualCab_Select",
            new { Empresa, Alm, FechaTop, InformeTop });
        return Excel(rows, "InformeManualCab");
    }
}
