
Console.InputEncoding = new System.Text.UTF8Encoding(false);
Console.OutputEncoding = new System.Text.UTF8Encoding(false);

try
{
    Console.WriteLine("Enter the directory path of the folder containing your Excel file.");
    Console.WriteLine("The folder should contain exactly one .xlsx file.");
    Console.WriteLine("Example: C:\\Users\\YourName\\Documents\\ExcelFiles");
    Console.Write("Directory path: ");
    string DirectoryPath = Console.ReadLine() ?? string.Empty;

    //Console.WriteLine("Using directory: " + DirectoryPath);

    FileReader fr = new FileReader();
    String FilePath = fr.FindFile(DirectoryPath);

    //Console.WriteLine("Found File Path: " + FilePath);
    if(FilePath != "")
    {
        string PdfDirectoryPath = System.IO.Path.Combine(DirectoryPath, "PDF Filer");
        string ExcelResultDirectoryPath = System.IO.Path.Combine(DirectoryPath, "Excel Result");
        System.IO.Directory.CreateDirectory(PdfDirectoryPath);
        System.IO.Directory.CreateDirectory(ExcelResultDirectoryPath);
        Console.WriteLine(FilePath);
        List<Dictionary<string, object?>> NewExcelFileObject = fr.ExcelReader(FilePath);
        PdfDownloader pdfDownloader = new PdfDownloader();
        List<DownloadResult> downloadResults = new();
        for (int i = 0; i < 10 && i < NewExcelFileObject.Count; i++)
        {
            downloadResults.Add(await pdfDownloader.DownloadRowAsync(NewExcelFileObject[i], PdfDirectoryPath, i + 1));
        }
        OutputFileWriter outputFileWriter = new OutputFileWriter();
        outputFileWriter.Write(ExcelResultDirectoryPath, downloadResults);
        /*
        foreach (Dictionary<string, object?> dict in NewExcelFileObject)
        {
            foreach (KeyValuePair<string, object?> KeyValue in dict)
            {
                Console.WriteLine(KeyValue);
            }   
        } */
    }

}
catch(Exception e)
{
    Console.WriteLine("Exception: " + e.Message);
}
finally
{
    Console.WriteLine("Executing finally block.");
}