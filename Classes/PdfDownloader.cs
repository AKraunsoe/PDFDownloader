using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using DocumentFormat.OpenXml.Office2010.ExcelAc;

class PdfDownloader
{
    private static readonly HttpClientHandler handler = new HttpClientHandler {
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
    };
    private static readonly HttpClient httpClient = new HttpClient(handler);

    public readonly List<DownloadResult> downloadResults = new();

    public async Task FillDownLoadResults(List<ExcelRow> NewExcelFileObject, string PdfDirectoryPath)
    {
        downloadResults.Clear();
        var debug = Debugger.IsAttached;

        int totalCount = NewExcelFileObject.Count(row => row.Success != true);
        int loopCount = NewExcelFileObject.Count();
        int processedCount = 0;
        int downloadedCount = 0;

        if (debug)
        {
            loopCount = 10;
            totalCount = 10 - NewExcelFileObject.Count(row => row.Success);
        }
        
        //int totalCount = NewExcelFileObject.Count(row => row.Success != true);
        using var progressBar = new ProgressBar();

        if (totalCount == 0)
        {
            progressBar.Update(0, 0, 0);
            return;
        }

        progressBar.Update(0, 0, totalCount);
        progressBar.StartTimer();

        for (int i = 0; i < loopCount; i++)
        {
            ExcelRow row = NewExcelFileObject[i];
            if (row.Success == true)
            {
                downloadResults.Add(new DownloadResult(row.id, row.url, row.altUrl, row.Success, row.Filename, row.Error));
                continue;
            }

            DownloadResult result = await DownloadRowAsync(row, PdfDirectoryPath, i + 1);
            downloadResults.Add(result);
            processedCount++;

            if (result.Success)
            {
                downloadedCount++;
            }

            progressBar.Update(processedCount, downloadedCount, totalCount);
        }
    }

    private async Task<DownloadResult> DownloadRowAsync(
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
            if (string.IsNullOrWhiteSpace(downloadUrl)){
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
                    || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)){
                    throw new InvalidOperationException("The value is not a valid HTTP or HTTPS URL.");
                }

                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:157.0) Gecko/20100101 Firefox/157.0");
                httpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
                httpClient.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");

                using HttpResponseMessage response = await httpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                byte[] fileContents = await response.Content.ReadAsByteArrayAsync();
                if (!HasPdfSignature(fileContents)){
                    string? detectedFormat = response.Content.Headers.ContentType?.MediaType;
                    if (string.IsNullOrWhiteSpace(detectedFormat)
                        || detectedFormat.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase)){
                        string extension = Path.GetExtension(uri.LocalPath);
                        detectedFormat = string.IsNullOrWhiteSpace(extension) ? "unknown format" : $"{extension} file";
                    }
                    throw new InvalidDataException($"Downloaded file is not a PDF. Detected format: {detectedFormat}.");
                }

                string fileName = Path.GetFileName(uri.LocalPath);
                if (string.IsNullOrWhiteSpace(fileName)){
                    fileName = $"document_{row.id}.pdf";
                }else if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)){
                    fileName = Path.ChangeExtension(fileName, ".pdf");
                }

                string destinationPath = Path.Combine(pdfDirectoryPath, $"{row.id}_{fileName}");
                await File.WriteAllBytesAsync(destinationPath, fileContents);
                string savedFileName = Path.GetFileName(destinationPath);
                //Console.WriteLine($"Successfully downloaded row {rowNumber} using {sourceName}: {destinationPath}");
                return new DownloadResult(row.id, row.url, row.altUrl, true, savedFileName, null);
            }
            catch (Exception e)
            {
                if (sourceName == "url"){
                    primaryError = e.Message;
                }
                else{
                    altError = e.Message;
                }

                //Console.WriteLine($"Download using {sourceName} failed for row {rowNumber}: {e.Message}");
            }
        }


        string error = $"Primary URL error: {primaryError} Alternate URL error: {altError}";
        //Console.WriteLine($"Could not download row {rowNumber}. {error}");
        return new DownloadResult(row.id, row.url, row.altUrl, false, null, error);
        
    }

    private static bool HasPdfSignature(byte[] fileContents)
    {
        byte[] signature = Encoding.ASCII.GetBytes("%PDF-");
        int searchLimit = Math.Min(fileContents.Length - signature.Length, 1024 - signature.Length);

        for (int index = 0; index <= searchLimit; index++)
        {
            if (fileContents.AsSpan(index, signature.Length).SequenceEqual(signature)){
                return true;
            }
        }

        return false;
    }

}
