using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/TraspasosLinReducido.asp (versión reducida de TraspasosLin).
/// Procedures: asp.PROC_MovTraspasoLin_Select (líneas), rep.TraspasoCab (cabecera del traspaso).
/// Parámetros de URL P2 (fecha) y P3 (traspaso) se conservan para mantener los enlaces existentes.
/// El formulario oculto de filtro "No Preparado" (P4) del origen apuntaba a una página
/// inexistente (TraspasoLin.asp) y no se usaba: eliminado.
/// </summary>
public class TraspasosLinReducidoModel(DbService db) : BasePageModel(db)
{
    [BindProperty(SupportsGet = true)] public string? P2 { get; set; }
    [BindProperty(SupportsGet = true)] public string? P3 { get; set; }

    public List<dynamic> Lineas { get; private set; } = [];
    public dynamic? Cab { get; private set; }

    private string? Fecha => string.IsNullOrEmpty(P2) ? null : P2;
    private long? Traspaso => long.TryParse(P3, out var t) ? t : null;

    public async Task OnGetAsync()
    {
        Lineas = await Db.QueryProcAsync("asp.PROC_MovTraspasoLin_Select",
            new { Empresa, Fecha, Traspaso });
        Cab = await Db.QueryFirstOrDefaultProcAsync("rep.TraspasoCab",
            new { Empresa, Fecha, Traspaso });
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.PROC_MovTraspasoLin_Select",
            new { Empresa, Fecha, Traspaso });
        return Excel(rows, $"Traspaso_{Traspaso}");
    }
}
