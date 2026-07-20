using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/PedidosCompraLin_New.asp (detalle imprimible de un pedido de compra,
/// variante idéntica a PedidosCompraLin salvo que la cabecera del proveedor no muestra la línea
/// "Prov (Gama)").
/// Procedures: asp.PROC_PedidoCompraLin_Select (líneas), asp.PROC_PedidoCompraCab_Select (cabecera).
/// Parámetros de URL P2 (fecha) y P3 (pedido) se conservan. La acción "Marcar como Recepcionado"
/// enlaza a /Accion?Proc=4 (antes "Ejecuta procedure.asp").
/// </summary>
public class PedidosCompraLinNewModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P3 { get; set; }

    public List<dynamic> Lineas { get; private set; } = [];
    public dynamic? Cab { get; private set; }

    private string? Fecha => string.IsNullOrEmpty(P2) ? null : P2;
    private long? Pedido => long.TryParse(P3, out var p) ? p : null;

    /// <summary>Equivale al IF Estado &gt; "0" del original (pedido ya enviado/recepcionable).</summary>
    public bool EstadoMayorCero
    {
        get
        {
            if (Cab is null) return false;
            var estado = $"{((IDictionary<string, object>)Cab!)["Estado"]}";
            return string.Compare(estado, "0", StringComparison.Ordinal) > 0;
        }
    }

    public async Task OnGetAsync()
    {
        Lineas = await Db.QueryProcAsync("asp.PROC_PedidoCompraLin_Select",
            new { Empresa, Fecha, Pedido });

        Cab = await Db.QueryFirstOrDefaultProcAsync("asp.PROC_PedidoCompraCab_Select",
            new { Empresa, Pedido });
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel, sin icono en esta página).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.PROC_PedidoCompraLin_Select",
            new { Empresa, Fecha, Pedido });
        return Excel(rows, $"PedidoCompra_{P3}");
    }
}
