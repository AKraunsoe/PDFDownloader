using System.Text;

namespace TestPDFReader;

public class PdfDownloaderTest
{

    [Fact]
    public async Task FillDownloadResults_ValidPdfUrl()
    {
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"PdfDownloaderTest-{Guid.NewGuid():N}");

        Directory.CreateDirectory(outputDirectory);

        try
        {
            PdfDownloader downloader = new PdfDownloader();
            List<ExcelRow> rows = new List<ExcelRow>
            {
                new()
                {
                    id = 1,
                    url = "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf"
                }
            };

            await downloader.FillDownLoadResults(rows, outputDirectory);

            var result = Assert.Single(downloader.downloadResults);
            Assert.True(result.success, result.error);
            Assert.Equal("1_dummy.pdf", result.filename);

            var filePath = Path.Combine(outputDirectory, result.filename!);
            Assert.True(File.Exists(filePath));

            var bytes = await File.ReadAllBytesAsync(filePath);
            Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(bytes));
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task FillDownloadResults_PrimaryFails()
    {
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"PdfDownloaderTest-{Guid.NewGuid():N}");

        Directory.CreateDirectory(outputDirectory);

        try
        {
            PdfDownloader downloader = new PdfDownloader();
            List<ExcelRow> rows = new List<ExcelRow>
            {
                new()
                {
                    id = 1,
                    url = "not-a-valid-url",
                    altUrl = "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf"
                }
            };

            await downloader.FillDownLoadResults(rows, outputDirectory);

            var result = Assert.Single(downloader.downloadResults);
            Assert.True(result.success, result.error);
            Assert.Equal("1_dummy.pdf", result.filename);
            Assert.True(File.Exists(Path.Combine(outputDirectory, result.filename!)));
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task FillDownloadResults_NoValidUrl()
    {
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"PdfDownloaderTest-{Guid.NewGuid():N}");

        Directory.CreateDirectory(outputDirectory);

        try
        {
            PdfDownloader downloader = new PdfDownloader();
            List<ExcelRow> rows = new List<ExcelRow>
            {
                new()
                {
                    id = 1,
                    url = "not-a-valid-url",
                    altUrl = "not-a-valid-url"
                }
            };

            await downloader.FillDownLoadResults(rows, outputDirectory);

            var result = Assert.Single(downloader.downloadResults);
            Assert.False(result.success);
            Assert.False(string.IsNullOrWhiteSpace(result.error));
            Assert.Empty(Directory.GetFiles(outputDirectory));
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task FillDownloadResults_NonPdfUrl()
    {
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"PdfDownloaderTest-{Guid.NewGuid():N}");

        Directory.CreateDirectory(outputDirectory);

        try
        {
            PdfDownloader downloader = new PdfDownloader();
            List<ExcelRow> rows = new List<ExcelRow>
            {
                new()
                {
                    id = 1,
                    url = "https://example.com/"
                }
            };

            await downloader.FillDownLoadResults(rows, outputDirectory);

            var result = Assert.Single(downloader.downloadResults);
            Assert.False(result.success);
            Assert.Contains("not a PDF", result.error);
            Assert.Null(result.filename);
            Assert.Empty(Directory.GetFiles(outputDirectory));
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

}
