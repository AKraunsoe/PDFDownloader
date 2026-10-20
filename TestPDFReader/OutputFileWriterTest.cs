namespace TestPDFReader;

public class OutputFileWriterTest
{
    [Fact]
    public void Write_CreatesResultFile()
    {
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"OutputFileWriterTest-{Guid.NewGuid():N}");
        Directory.CreateDirectory(outputDirectory);

        try
        {
            var writer = new OutputFileWriter();
            var results = new[]
            {
                new DownloadResult(1, "https://example.com/file.pdf", null, true, "1_file.pdf", null)
            };

            writer.Write(outputDirectory, results);

            Assert.True(File.Exists(Path.Combine(outputDirectory, "Result.xlsx")));
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }
}