using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/RevisionStockProds.asp (Revisión Stock Productos).
/// Procedure: asp.RevisionStockProds (@Empresa, @Alm, @TipoProg).
/// Parámetro de URL conservado: P1 (TipoProg: 0 Perecederos, 1 Ean; default 1).
/// </summary>
public class RevisionStockProdsModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P1 { get; set; }

    public List<dynamic> Productos { get; private set; } = [];

    private long TipoProg => long.TryParse(P1, out var p) ? p : 1;

    public async Task OnGetAsync()
    {
        P1 = string.IsNullOrEmpty(P1) ? "1" : P1;
        Productos = await Db.QueryProcAsync("asp.RevisionStockProds", new { Empresa, Alm, TipoProg });
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.RevisionStockProds", new { Empresa, Alm, TipoProg });
        return Excel(rows, "RevisionStockProds");
    }
}
