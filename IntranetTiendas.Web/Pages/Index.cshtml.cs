using IntranetTiendas.Web.Services;

namespace IntranetTiendas.Web.Pages;

/// <summary>Migración de IntranetTiendas/inicio.asp (menú principal + aviso de comunicados).</summary>
public class IndexModel(DbService db) : BasePageModel(db)
{
    public int ComunicadosPendientes { get; private set; }

    public async Task OnGetAsync()
    {
        var row = await Db.QueryFirstOrDefaultProcAsync("asp.PROC_Comunicados_AlmSelect_Pendientes",
            new { Empresa, Alm });
        if (row is not null)
        {
            var dict = (IDictionary<string, object>)row;
            ComunicadosPendientes = Convert.ToInt32(dict["PendLeer"]);
        }
    }
}
