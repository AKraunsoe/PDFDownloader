using ClosedXML.Excel;

public sealed record DownloadResult(
    object? Id,
    object? Url,
    object? AltUrl,
    bool Success,
    string? Filename,
    string? Error);

class OutputFileWriter
{
    private static readonly string[] headers =
    {
        "id",
        "url",
        "alt url",
        "Success",
        "Filename",
        "Error"
    };

    public void Write(string excelResultDirectoryPath, IEnumerable<DownloadResult> results)
    {

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Result");

        for (int column = 0; column < headers.Length; column++)
        {
            worksheet.Cell(1, column + 1).Value = headers[column];
        }

        int row = 2;
        foreach (DownloadResult result in results)
        {
            worksheet.Cell(row, 1).Value = XLCellValue.FromObject(result.Id);
            worksheet.Cell(row, 2).Value = XLCellValue.FromObject(result.Url);
            worksheet.Cell(row, 3).Value = XLCellValue.FromObject(result.AltUrl);
            worksheet.Cell(row, 4).Value = result.Success;
            worksheet.Cell(row, 5).Value = result.Filename ?? string.Empty;
            worksheet.Cell(row, 6).Value = result.Error ?? string.Empty;
            row++;
        }

        worksheet.Columns();
        workbook.SaveAs(Path.Combine(excelResultDirectoryPath, "Result.xlsx"));
    }
}