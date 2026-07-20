using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/ProvGamaProd.asp (Productos Proveedor por Gama, paginado de 50 en 50).
/// Procedure: asp.Info_ProvGamaProd_Alm (@Empresa, @Alm, @Gama, @Prod, @FueraColeccion).
/// Parámetros de URL conservados: P2 (gama), P3 (último producto de la página anterior, para paginar),
/// P4 (1 = incluir productos fuera de colección; default 0).
/// </summary>
public class ProvGamaProdModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P3 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P4 { get; set; }

    public List<dynamic> Productos { get; private set; } = [];

    /// <summary>Nº de filas devueltas; si llega a 50 se muestra "Siguiente Pagina".</summary>
    public int Cont => Productos.Count;

    /// <summary>Último producto listado (P3 del enlace de paginación).</summary>
    public string UltimoProd { get; private set; } = "0";

    public bool FueraColeccionActivo => FueraColeccion == 0;

    private long Gama => long.TryParse(P2, out var g) ? g : 0;
    private long Prod => long.TryParse(P3, out var p) ? p : 0;
    private long FueraColeccion => long.TryParse(P4, out var f) ? f : 0;

    public async Task OnGetAsync()
    {
        Productos = await Db.QueryProcAsync("asp.Info_ProvGamaProd_Alm",
            new { Empresa, Alm, Gama, Prod, FueraColeccion });

        if (Productos.Count > 0)
        {
            var last = (IDictionary<string, object>)Productos[^1];
            UltimoProd = $"{last["Prod"]}";
        }
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.Info_ProvGamaProd_Alm",
            new { Empresa, Alm, Gama, Prod, FueraColeccion });
        return Excel(rows, $"ProvGamaProd_{P2}");
    }
}
