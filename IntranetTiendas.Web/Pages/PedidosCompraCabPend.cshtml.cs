using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/PedidosCompraCab_Pend.asp (pedidos a proveedor pendientes de recepcionar).
/// Procedure: asp.PROC_PedidoCompraCab_Pend_Select_Alm.
/// Parámetros de URL P2 (fecha tope) y P3 (pedido tope) se conservan para la paginación
/// "Siguiente Pagina" (aquí P2 avanza con la FechaPrevEnvio de la última fila, como en el original).
/// </summary>
public class PedidosCompraCabPendModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P3 { get; set; }

    public List<dynamic> Pedidos { get; private set; } = [];
    public string NextP2 { get; private set; } = "";
    public string NextP3 { get; private set; } = "";

    private string? FechaTop => string.IsNullOrEmpty(P2) ? null : P2;
    private long? PedidoTop => long.TryParse(P3, out var p) ? p : null;

    public async Task OnGetAsync()
    {
        Pedidos = await Db.QueryProcAsync("asp.PROC_PedidoCompraCab_Pend_Select_Alm",
            new { Empresa, Alm, FechaTop, PedidoTop });

        if (Pedidos.Count > 0)
        {
            var ult = (IDictionary<string, object>)Pedidos[^1];
            NextP2 = $"{ult["FechaPrevEnvio"]}";
            NextP3 = $"{ult["Pedido"]}";
        }
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.PROC_PedidoCompraCab_Pend_Select_Alm",
            new { Empresa, Alm, FechaTop, PedidoTop });
        return Excel(rows, "PedidosCompraPend");
    }
}
