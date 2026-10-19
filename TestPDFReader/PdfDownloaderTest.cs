using System.Text;

namespace TestPDFReader;

public class PdfDownloaderTest
{

    [Fact]
    public async Task FillDownloadResults_ValidPdfUrl_SavesPdf()
    {
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"PdfDownloaderTest-{Guid.NewGuid():N}");

        Directory.CreateDirectory(outputDirectory);

        try
        {
            var downloader = new PdfDownloader();
            var rows = new List<ExcelRow>
            {
                new()
                {
                    id = 1,
                    url = "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf"
                }
            };

            await downloader.FillDownLoadResults(rows, outputDirectory);

            var result = Assert.Single(downloader.downloadResults);
            Assert.True(result.Success, result.Error);
            Assert.Equal("1_dummy.pdf", result.Filename);

            var filePath = Path.Combine(outputDirectory, result.Filename!);
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
    public async Task FillDownloadResults_PrimaryFails_DownloadsFromAltUrl()
    {
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"PdfDownloaderTest-{Guid.NewGuid():N}");

        Directory.CreateDirectory(outputDirectory);

        try
        {
            var downloader = new PdfDownloader();
            var rows = new List<ExcelRow>
            {
                new()
                {
                    id = 7,
                    url = "not-a-valid-url",
                    altUrl = "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf"
                }
            };

            await downloader.FillDownLoadResults(rows, outputDirectory);

            var result = Assert.Single(downloader.downloadResults);
            Assert.True(result.Success, result.Error);
            Assert.Equal("7_dummy.pdf", result.Filename);
            Assert.True(File.Exists(Path.Combine(outputDirectory, result.Filename!)));
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

}
