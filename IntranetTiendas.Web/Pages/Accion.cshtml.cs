using IntranetTiendas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntranetTiendas.Web.Pages;

/// <summary>
/// Migración de "Ejecuta procedure.asp": las acciones que disparaban las páginas vía popup.
/// Antes se construía el SQL concatenando la querystring (inyección SQL directa); ahora todo va
/// parametrizado. Las acciones 1 y 2 del original (DELETE sobre [EMPRESA$Pedidos a tienda NET] /
/// [EMPRESA$Offline NET]) se eliminan: verificado contra NAVHM que esas tablas ya no existen y
/// ninguna página las invocaba. Se conservan los nombres de parámetros Proc/V1/V2.
/// </summary>
public class AccionModel(DbService db, ILogger<AccionModel> log) : BasePageModel(db)
{
    public bool Ok { get; private set; }
    public string Detalle { get; private set; } = "";

    [BindProperty(SupportsGet = true)] public string? Proc { get; set; }
    [BindProperty(SupportsGet = true)] public string? V1 { get; set; }
    [BindProperty(SupportsGet = true)] public string? V2 { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            switch (Proc)
            {
                case "3": // asp.PROC_Comunicados_Leido @Doc, @Alm
                    await Db.ExecuteProcAsync("asp.PROC_Comunicados_Leido",
                        new { Doc = long.Parse(V1 ?? "0"), Alm = int.Parse(V2 ?? Alm.ToString()) });
                    break;
                case "4": // asp.PROC_PedidoCompra_UpdateRecepcionado @Pedido, @Emp
                    await Db.ExecuteProcAsync("asp.PROC_PedidoCompra_UpdateRecepcionado",
                        new { Pedido = long.Parse(V1 ?? "0"), Emp = Empresa });
                    break;
                default:
                    Detalle = "Acción desconocida";
                    return;
            }
            Ok = true;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error en acción {Proc} V1={V1} V2={V2}", Proc, V1, V2);
            Detalle = $"Proc={Proc} V1={V1} V2={V2}: {ex.Message}";
        }
    }
}
