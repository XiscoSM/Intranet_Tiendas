using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/PedidosCentralDirecto.asp.
/// Procedure: asp.PROC_PedidoCentralCab_Select_Alm (@Empresa, @Alm, @FechaTop, @PedidoTop).
/// P2 (fecha tope) y P3 (pedido tope) son la paginación de "Siguiente Página"; se conservan los nombres.
/// </summary>
public class PedidosCentralDirectoModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P3 { get; set; }

    public List<dynamic> Pedidos { get; private set; } = [];

    private string? FechaTop => string.IsNullOrEmpty(P2) ? null : P2;
    private long? PedidoTop => long.TryParse(P3, out var p) ? p : null;

    public async Task OnGetAsync()
    {
        Pedidos = await Db.QueryProcAsync("asp.PROC_PedidoCentralCab_Select_Alm",
            new { Empresa, Alm, FechaTop, PedidoTop });
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.PROC_PedidoCentralCab_Select_Alm",
            new { Empresa, Alm, FechaTop, PedidoTop });
        return Excel(rows, "PedidosCentralDirecto");
    }
}
