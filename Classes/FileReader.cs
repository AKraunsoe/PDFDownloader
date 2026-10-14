using System.Runtime.CompilerServices;
using ExcelDataReader;
using System.Text;

class FileReader()
{
    public List<ExcelRow> ExcelReader (String filePath)
    {

        List<ExcelRow> rows = new List<ExcelRow>();
        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - Legacy Excel XML Spreadsheet files (2002-2003 format; *.xml)
                //  - OpenXml Excel files (2007 format; *.xlsx, *.xlsb)
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    do
                {

                    Dictionary<string, int> headers = new Dictionary<string, int>(
                        StringComparer.OrdinalIgnoreCase);

                    Dictionary<string, string> columnMap = new Dictionary<string, string>
                    {
                        ["id"] = nameof(ExcelRow.id),
                        ["url"] = nameof(ExcelRow.url),
                        ["alt url"] = nameof(ExcelRow.altUrl),
                        ["Success"] = nameof(ExcelRow.Success),
                        ["Filename"] = nameof(ExcelRow.Filename),
                        ["Error"] = nameof(ExcelRow.Error)
                    };

                        if(reader.Read()){
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                headers[reader.GetValue(i)?.ToString()?.Trim() ?? ""] = i;
                            }
                        }

                        while (reader.Read())
                        {
                            var properties = typeof(ExcelRow).GetProperties()
                                .ToDictionary(p => p.Name);
                            ExcelRow row = new ExcelRow();

                            foreach (var (header, propertyName) in columnMap)
                            {
                                if (!headers.TryGetValue(header, out int columnIndex))
                                    continue;

                                var value = reader.GetValue(columnIndex);

                                if (value == null || value == DBNull.Value)
                                    continue;

                                var property = properties[propertyName];

                                var targetType =
                                    Nullable.GetUnderlyingType(property.PropertyType)
                                    ?? property.PropertyType;

                                property.SetValue(
                                    row,
                                    Convert.ChangeType(value, targetType));
                            }

                            rows.Add(row);
                        }
                    } while (reader.NextResult());
                }
            }
        return rows;
    }

    public String FindFile(String DirectoryPath)
    {
        if (!Directory.Exists(DirectoryPath))
        {
            return "";
        }

        var Files = Directory.EnumerateFiles(DirectoryPath);

        if (Files.Count() > 1)
        {
            return "";
        }

        var FoundFile = Files.First();

        if (Path.GetExtension(FoundFile).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return FoundFile;
        }
        return "";
    }
}
