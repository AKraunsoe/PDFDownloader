using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System.Collections.Concurrent;

class PdfDownloader
{

    private static readonly HttpClient httpClient = CreateHttpClient();

    private static HttpClient CreateHttpClient()
    {
        HttpClientHandler handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
        };

        HttpClient client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromMinutes(2)
        };

        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:157.0) Gecko/20100101 Firefox/157.0");
        client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml,application/pdf;q=0.9,*/*;q=0.8");
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");

        return client;

    }

    public readonly List<DownloadResult> downloadResults = new();
    private readonly ConcurrentDictionary<string, byte> retriedUrls = new(StringComparer.OrdinalIgnoreCase);
    private int totalCount = 0;
    private int processedCount = 0;
    private int downloadedCount = 0;
    private ProgressBar progressBar = new ProgressBar();
    private readonly object progressUpdateLock = new();
    private readonly SemaphoreSlim downloadLimit = new(40);

    public async Task FillDownLoadResults(List<ExcelRow> NewExcelFileObject, string PdfDirectoryPath)
    {
        downloadResults.Clear();
        retriedUrls.Clear();
        bool debug = Debugger.IsAttached;

        totalCount = NewExcelFileObject.Count(row => row.success != true);
        int loopCount = NewExcelFileObject.Count();

        if (totalCount == 0)
        {
            progressBar.Update(0, 0, 0);
            return;
        }

        progressBar.Update(0, 0, totalCount);
        progressBar.StartTimer();

        List<Task<DownloadResult>> downloadTasks = new List<Task<DownloadResult>>(loopCount);

        for (int i = 0; i < loopCount; i++)
        {
            ExcelRow row = NewExcelFileObject[i];
            if (row.success == true)
            {
                downloadTasks.Add(Task.FromResult(new DownloadResult(row.id, row.url, row.altUrl, row.success, row.filename, row.error)));
                continue;
            }

            downloadTasks.Add(DownloadRowAsync(row, PdfDirectoryPath));

        }
        DownloadResult[] results = await Task.WhenAll(downloadTasks);
        downloadResults.AddRange(results);
    }

    private async Task<DownloadResult> DownloadRowAsync(
        ExcelRow row,
        string pdfDirectoryPath)
    {

        await downloadLimit.WaitAsync();

        try
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

                    using HttpResponseMessage response = await GetResponseWithRetryAsync(uri);

                    if (!response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        int errorCode = (int)response.StatusCode;

                        if(errorCode == 403)
                        {
                            throw new HttpRequestException(
                            $"HTTP {errorCode} ({response.StatusCode}) for {uri}. This error can be a program only error, or it might be a legit forbidden error, please try to download this manually ");
                        }

                        throw new HttpRequestException(
                            $"HTTP {errorCode} ({response.StatusCode}) for {uri}. ");
                    }

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
                    updateProgressBar(true);
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
                }
            }

            string error = $"Primary URL error: {primaryError} Alternate URL error: {altError}";
            updateProgressBar(false);
            return new DownloadResult(row.id, row.url, row.altUrl, false, null, error);
        }
        finally
        {
            downloadLimit.Release();
        }

    }

    private void updateProgressBar(bool success)
    {
        if (success)
        {
            Interlocked.Increment(ref downloadedCount);
        }

        Interlocked.Increment(ref processedCount);

        lock (progressUpdateLock)
        {
            progressBar.Update(
                Volatile.Read(ref processedCount),
                Volatile.Read(ref downloadedCount),
                totalCount);
        }
    }

    private async Task<HttpResponseMessage> GetResponseWithRetryAsync(Uri uri)
    {
        while (true)
        {
            HttpResponseMessage response = await httpClient.GetAsync(uri);

            if (response.StatusCode != HttpStatusCode.TooManyRequests)
                return response;

            //string responseBody = await response.Content.ReadAsStringAsync();

            if (!TryGetRetryDelay(response, out TimeSpan retryDelay))
            {
                response.Dispose();
                throw new HttpRequestException(
                    $"HTTP 429 (Too Many Requests) for {uri}. " +
                    "Retry-After was missing or invalid, so the request was not retried.");
            }

            if (!retriedUrls.TryAdd(uri.AbsoluteUri, 0))
            {
                response.Dispose();
                throw new HttpRequestException(
                    $"HTTP 429 (Too Many Requests) for {uri}. " +
                    "Tried twice");
            }

            response.Dispose();

            await Task.Delay(retryDelay);
        }
    }

    private static bool TryGetRetryDelay(
        HttpResponseMessage response,
        out TimeSpan delay)
    {
        var retryAfter = response.Headers.RetryAfter;

        if (retryAfter?.Delta is TimeSpan delta)
        {
            delay = delta > TimeSpan.Zero ? delta : TimeSpan.Zero;
            return true;
        }

        if (retryAfter?.Date is DateTimeOffset retryDate)
        {
            delay = retryDate - DateTimeOffset.UtcNow;
            if (delay < TimeSpan.Zero)
                delay = TimeSpan.Zero;

            return true;
        }

        delay = TimeSpan.Zero;
        return false;
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
