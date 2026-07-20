using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/InformeManualLin7.asp (detalle de informe manual, tipo 7: solo cantidades, sin total).
/// Procedures: asp.PROC_InformeManualLin_Select (@Empresa, @Informe).
/// Parámetros de URL P2 (fecha, solo display), P3 (nº informe) y Text (título) se conservan.
/// </summary>
public class InformeManualLin7Model(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P3 { get; set; }
    [BindProperty(SupportsGet = true)] public string? Text { get; set; }

    public List<dynamic> Lineas { get; private set; } = [];

    private long? Informe => long.TryParse(P3, out var i) ? i : null;

    public async Task OnGetAsync()
    {
        Lineas = await Db.QueryProcAsync("asp.PROC_InformeManualLin_Select", new { Empresa, Informe });
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.PROC_InformeManualLin_Select", new { Empresa, Informe });
        return Excel(rows, "InformeManualLin7");
    }
}
