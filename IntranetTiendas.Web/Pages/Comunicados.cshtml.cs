using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de IntranetTiendas/Comunicados.asp.
/// Procedure: asp.PROC_Comunicados_AlmSelect (@Empresa, @Alm).
/// El enlace "Marcar Leido" llamaba a "Ejecuta procedure.asp" con Proc=3 → ahora /Accion?Proc=3.
/// </summary>
public class ComunicadosModel(DbService db) : BasePageModel(db)
{
    public List<dynamic> Comunicados { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Comunicados = await Db.QueryProcAsync("asp.PROC_Comunicados_AlmSelect", new { Empresa, Alm });
    }

    /// <summary>Exportación a Excel (antes truco ContentType con Formato=Excel).</summary>
    public async Task<IActionResult> OnGetExcelAsync()
    {
        var rows = await Db.QueryProcAsync("asp.PROC_Comunicados_AlmSelect", new { Empresa, Alm });
        return Excel(rows, "Comunicados");
    }
}
