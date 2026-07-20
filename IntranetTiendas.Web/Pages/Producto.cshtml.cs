using System.Globalization;
using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/Producto.asp.
/// Procedures: asp.Info_Producto_EanAlm, asp.Info_Producto_Stock, asp.Info_Producto_Ean.
/// Parámetros de URL P1 (producto) y P2 (EAN) se conservan para mantener los enlaces existentes.
/// </summary>
public class ProductoModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P1 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }

    public dynamic? Ficha { get; private set; }
    public List<dynamic> StockAlmacenes { get; private set; } = [];
    public List<dynamic> Eans { get; private set; } = [];
    public string DescProdLink { get; private set; } = "";

    private long Prod => long.TryParse(P1, out var p) ? p : 0;
    private long Ean => long.TryParse(P2, out var e) ? e : 0;

    public string Moneda(object? v) =>
        Convert.ToDecimal(v ?? 0).ToString("C2", CultureInfo.GetCultureInfo("es-ES"));

    public async Task OnGetAsync()
    {
        Ficha = await Db.QueryFirstOrDefaultProcAsync("asp.Info_Producto_EanAlm",
            new { Empresa, Alm, Prod, Ean });

        if (Ficha is not null)
        {
            var dict = (IDictionary<string, object>)Ficha!;
            P1 = dict["Prod"].ToString(); // el proc resuelve el producto a partir del EAN
            DescProdLink = ($"{dict["DescProd"]}").Replace("\"", "''");
        }

        StockAlmacenes = await Db.QueryProcAsync("asp.Info_Producto_Stock", new { Empresa, Prod, Ean });
        Eans = await Db.QueryProcAsync("asp.Info_Producto_Ean", new { Empresa, Prod, Ean });
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.Info_Producto_Stock", new { Empresa, Prod, Ean });
        return Excel(rows, $"Producto_{Prod}");
    }
}
