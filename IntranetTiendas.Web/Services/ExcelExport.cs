using ClosedXML.Excel;

namespace IntranetTiendas.Web.Services;

/// <summary>
/// Exportación a Excel real (.xlsx). Sustituye el truco de ContentType=vnd.ms-excel del ASP clásico.
/// Recibe directamente las filas dynamic de Dapper.
/// </summary>
public static class ExcelExport
{
    public static byte[] ToXlsx(IEnumerable<dynamic> rows, string sheetName = "Informe")
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add(Sanitize(sheetName));

        var list = rows.Cast<IDictionary<string, object?>>().ToList();
        if (list.Count > 0)
        {
            var cols = list[0].Keys.ToList();
            for (var c = 0; c < cols.Count; c++)
                ws.Cell(1, c + 1).Value = cols[c];
            ws.Row(1).Style.Font.Bold = true;

            for (var r = 0; r < list.Count; r++)
                for (var c = 0; c < cols.Count; c++)
                {
                    var v = list[r][cols[c]];
                    ws.Cell(r + 2, c + 1).Value = v switch
                    {
                        null => Blank.Value,
                        byte[] => "[binario]",
                        bool b => b,
                        DateTime d => d,
                        string s => s,
                        _ => XLCellValue.FromObject(v)
                    };
                }
            ws.Columns().AdjustToContents(1, Math.Min(list.Count + 1, 200));
        }

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static string Sanitize(string name)
    {
        foreach (var ch in new[] { ':', '\\', '/', '?', '*', '[', ']' })
            name = name.Replace(ch, ' ');
        return name.Length > 31 ? name[..31] : name;
    }
}
