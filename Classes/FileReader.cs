
using System.Runtime.CompilerServices;
using ExcelDataReader;
using System.Text;


class FileReader()
{
    public List<Dictionary<string, object?>> ExcelReader (String filePath)
    {
        
        List<Dictionary<string, object?>> rows = new List<Dictionary<string, object?>>();
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
                        string[] headers = new string[reader.FieldCount];

                        while (reader.Read())
                        {
                            if(headers[headers.Count()-1] == null){
                                for (int i = 0; i < headers.Length; i++)
                                {
                                    headers[i] = reader.GetValue(i)?.ToString() ?? $"Column{i + 1}";
                                }
                                continue;
                            }

                            Dictionary<string, object?> row = new Dictionary<string, object?>();

                            for (int i = 0; i < headers.Length; i++)
                            {
                                if(i == 0 && reader.GetValue(i) == null)
                                {
                                    return rows;
                                }
                                row[headers[i]] = reader.GetValue(i);
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