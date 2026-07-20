using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/InfoEtiquetas.asp (Estimación aproximada de etiquetas por fecha).
/// Procedure: asp.InfoEtiquetas (@Alm) — en origen no recibía @Empresa.
/// El V2 de almacén del origen se elimina (va por sesión).
/// </summary>
public class InfoEtiquetasModel(DbService db) : BasePageModel(db)
{
    public List<dynamic> Etiquetas { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Etiquetas = await Db.QueryProcAsync("asp.InfoEtiquetas", new { Alm });
    }

    /// <summary>Exportación a Excel (el origen aceptaba Formato=Excel aunque no tenía icono).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.InfoEtiquetas", new { Alm });
        return Excel(rows, "InfoEtiquetas");
    }
}
