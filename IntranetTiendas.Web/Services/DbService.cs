using System.Data;
using System.Globalization;
using Dapper;
using Microsoft.Data.SqlClient;

namespace IntranetTiendas.Web.Services;

/// <summary>
/// Acceso a datos vía Dapper contra los procedimientos existentes del esquema asp.*
/// Sustituye a los ADODB.Recordset + DSN ODBC del ASP clásico.
/// </summary>
public class DbService(IConfiguration cfg)
{
    private readonly string _cs = cfg.GetConnectionString("NavR2")
        ?? throw new InvalidOperationException("Falta ConnectionStrings:NavR2");

    private readonly string _csMadisa = cfg.GetConnectionString("MadisaNet")
        ?? throw new InvalidOperationException("Falta ConnectionStrings:MadisaNet");

    /// <summary>Empresa fija de configuración (antes cookie EMPRESA).</summary>
    public string Empresa { get; } = cfg["Empresa"] ?? "HM1";

    public SqlConnection Open() => new(_cs);

    /// <summary>BD Madisa_Net (informes rep.* de Balanzas y Cierre).</summary>
    public SqlConnection OpenMadisa() => new(_csMadisa);

    /// <summary>Ejecuta un procedure en Madisa_Net y devuelve todas las filas.</summary>
    public async Task<List<dynamic>> QueryProcMadisaAsync(string proc, object? param = null)
    {
        await using var cn = OpenMadisa();
        var rows = await cn.QueryAsync(proc, param, commandType: CommandType.StoredProcedure);
        return NormalizarFechas(rows.ToList());
    }

    /// <summary>Ejecuta un procedure y devuelve todas las filas (dynamic).</summary>
    public async Task<List<dynamic>> QueryProcAsync(string proc, object? param = null)
    {
        await using var cn = Open();
        var rows = await cn.QueryAsync(proc, param, commandType: CommandType.StoredProcedure);
        return NormalizarFechas(rows.ToList());
    }

    /// <summary>Ejecuta un procedure y devuelve la primera fila o null.</summary>
    public async Task<dynamic?> QueryFirstOrDefaultProcAsync(string proc, object? param = null)
    {
        await using var cn = Open();
        var row = await cn.QueryFirstOrDefaultAsync(proc, param, commandType: CommandType.StoredProcedure);
        // Cast explícito a object?: 'row' es dynamic y, si es null, la resolución de sobrecarga en
        // tiempo de ejecución elegiría NormalizarFechasFila(List<dynamic>) y reventaría el foreach.
        NormalizarFechasFila((object?)row);
        return row;
    }

    /// <summary>
    /// Normaliza las fechas de cada fila para presentación, mutando las filas dynamic (DapperRow):
    ///  - 01/01/1753 (centinela de SQL) → null (se muestra en blanco).
    ///  - Fecha sin hora (medianoche) → string "dd/MM/yy" (formato pedido; evita el "0:00:00").
    ///  - Fecha con hora real (firmas, cierres) → se conserva como DateTime para no perder la hora.
    /// Como las páginas leen los valores por clave (IDictionary) y los procedures aceptan dd/MM/yy
    /// (sesión SQL en español, dmy), el formato corto vale también para los parámetros de enlace.
    /// </summary>
    private static List<dynamic> NormalizarFechas(List<dynamic> rows)
    {
        foreach (var row in rows) NormalizarFechasFila((object?)row);
        return rows;
    }

    private static void NormalizarFechasFila(object? row)
    {
        if (row is not IDictionary<string, object> d) return;
        List<KeyValuePair<string, object?>>? cambios = null;
        foreach (var kv in d)
        {
            if (kv.Value is not DateTime dt) continue;
            if (dt.Year <= 1753)
                (cambios ??= []).Add(new(kv.Key, null));
            else if (dt.TimeOfDay == TimeSpan.Zero)
                (cambios ??= []).Add(new(kv.Key, dt.ToString("dd/MM/yy", CultureInfo.InvariantCulture)));
        }
        if (cambios is null) return;
        foreach (var c in cambios) d[c.Key] = c.Value!;
    }

    /// <summary>Ejecuta un procedure sin resultado (acciones).</summary>
    public async Task<int> ExecuteProcAsync(string proc, object? param = null)
    {
        await using var cn = Open();
        return await cn.ExecuteAsync(proc, param, commandType: CommandType.StoredProcedure);
    }

    /// <summary>Descripción de un almacén (asp.Almacen). "" si no se encuentra.</summary>
    public async Task<string> DescAlmacenAsync(int alm)
    {
        var row = await QueryFirstOrDefaultProcAsync("asp.Almacen", new { Empresa, Alm = alm });
        return row is null ? "" : ((IDictionary<string, object>)row)["DescAlm"]?.ToString() ?? "";
    }

    /// <summary>Lista de almacenes de la empresa (asp.Almacen @Alm=-1) para el selector de tienda.</summary>
    public Task<List<dynamic>> AlmacenesAsync() =>
        QueryProcAsync("asp.Almacen", new { Empresa, Alm = -1 });
}
