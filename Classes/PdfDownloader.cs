using System.Net.Http;
using System.Text;

class PdfDownloader
{
    private static readonly HttpClient httpClient = new HttpClient();

    public async Task<DownloadResult> DownloadRowAsync(
        ExcelRow row,
        string pdfDirectoryPath,
        int rowNumber)
    {

        string? primaryError = null;
        string? altError = null;

        foreach ((string sourceName, string? downloadUrl) in new[]
        {
            ("url", row.url),
            ("alt url", row.altUrl)
        })
        {
            if (string.IsNullOrWhiteSpace(downloadUrl))
            {
                if (sourceName == "url")
                {
                    primaryError = "The URL value is missing.";
                }
                else
                {
                    altError = "The alternate URL value is missing.";
                }
                continue;
            }

            try
            {
                if (!Uri.TryCreate(downloadUrl, UriKind.Absolute, out Uri? uri)
                    || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                {
                    throw new InvalidOperationException("The value is not a valid HTTP or HTTPS URL.");
                }

                using HttpResponseMessage response = await httpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                byte[] fileContents = await response.Content.ReadAsByteArrayAsync();
                if (!HasPdfSignature(fileContents))
                {
                    string? detectedFormat = response.Content.Headers.ContentType?.MediaType;
                    if (string.IsNullOrWhiteSpace(detectedFormat)
                        || detectedFormat.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
                    {
                        string extension = Path.GetExtension(uri.LocalPath);
                        detectedFormat = string.IsNullOrWhiteSpace(extension) ? "unknown format" : $"{extension} file";
                    }
                    throw new InvalidDataException($"Downloaded file is not a PDF. Detected format: {detectedFormat}.");
                }

                string fileName = Path.GetFileName(uri.LocalPath);
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = $"document_{row.id}.pdf";
                }
                else if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return new DownloadResult(row.id, row.url, row.altUrl, false, "", "File found is not a pdf file it is a "+fileName.Substring(fileName.LastIndexOf('.'), fileName.Length));
                }

                string destinationPath = Path.Combine(pdfDirectoryPath, $"{row.id}_{fileName}");
                await File.WriteAllBytesAsync(destinationPath, fileContents);
                string savedFileName = Path.GetFileName(destinationPath);
                Console.WriteLine($"Successfully downloaded row {rowNumber} using {sourceName}: {destinationPath}");
                return new DownloadResult(row.id, row.url, row.altUrl, true, savedFileName, null);
            }
            catch (Exception e)
            {
                if (sourceName == "url")
                {
                    primaryError = e.Message;
                }
                else
                {
                    altError = e.Message;
                }

                Console.WriteLine($"Download using {sourceName} failed for row {rowNumber}: {e.Message}");
            }
        }

        string error = $"Primary URL error: {primaryError} Alternate URL error: {altError}";
        Console.WriteLine($"Could not download row {rowNumber}. {error}");
        return new DownloadResult(row.id, row.url, row.altUrl, false, null, error);
    }

    private static bool HasPdfSignature(byte[] fileContents)
    {
        byte[] signature = Encoding.ASCII.GetBytes("%PDF-");
        int searchLimit = Math.Min(fileContents.Length - signature.Length, 1024 - signature.Length);

        for (int index = 0; index <= searchLimit; index++)
        {
            if (fileContents.AsSpan(index, signature.Length).SequenceEqual(signature))
            {
                return true;
            }
        }

        return false;
    }

}
