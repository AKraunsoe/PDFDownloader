using System.Diagnostics;

class Menu
{
    private string DirectoryPath;
    private string FilePath;
    private string PdfDirectoryPath;
    private string ExcelResultDirectoryPath;
    private List<ExcelRow> NewExcelFileObject;
    private PdfDownloader pdfDownloader = new PdfDownloader();
    private List<DownloadResult> downloadResults = new();

    private FileReader fr = new FileReader();
    List<ExcelRow> ExcelResultFileObject;

    public void StartMenu ()
    {
        Console.WriteLine("Enter the directory path of the folder containing your Excel file.");
        Console.WriteLine("The folder should contain exactly one .xlsx file.");
        Console.WriteLine("Example: C:\\Users\\YourName\\Documents\\ExcelFiles");
        Console.Write("Directory path: ");
        SetDirectoryPath();
        FillData();
        Options();
    }

    private void SetDirectoryPath()
    {
        DirectoryPath = Console.ReadLine() ?? "";
    }

    private void Options()
    {

        var options = new List<MenuOption>
        {
            new MenuOption("1. Reset: download all files", () => ResetDownload()),
            new MenuOption("2. Download only new files", () => DownloadNew())
        };
        var debug = Debugger.IsAttached;

        if (debug)
        {
            Console.WriteLine("0 = Reset, 1 = Download");
            string line = Console.ReadLine();
            int function = int.Parse(line);
            options[function].Selected.Invoke();
        }else{

            // Set the default index of the selected item to be the first
            int index = 0;

            // Write the menu out
            WriteMenu(options, options[index]);

            // Store key info in here
            ConsoleKeyInfo keyinfo;
            do
            {
                keyinfo = Console.ReadKey();

                // Handle each key input (down arrow will write the menu again with a different selected item)
                if (keyinfo.Key == ConsoleKey.DownArrow)
                {
                    if (index + 1 < options.Count)
                    {
                        index++;
                        WriteMenu(options, options[index]);
                    }
                }
                if (keyinfo.Key == ConsoleKey.UpArrow)
                {
                    if (index - 1 >= 0)
                    {
                        index--;
                        WriteMenu(options, options[index]);
                    }
                }
                // Handle different action for the option
                if (keyinfo.Key == ConsoleKey.Enter)
                {
                    options[index].Selected.Invoke();
                    index = 0;
                }
            }
            while (keyinfo.Key != ConsoleKey.X);

            Console.ReadKey();
        }
    }

    private static void WriteMenu(List<MenuOption> options, MenuOption selectedOption)
    {
        Console.Clear();
        Console.WriteLine("Pick what you want to do:");

        foreach (MenuOption option in options)
        {
            if (option == selectedOption)
            {
                Console.Write("> ");
            }
            else
            {
                Console.Write(" ");
            }

            Console.WriteLine(option.Name);
        }
    }

    private async void FillData()
    {
        
        FilePath = fr.FindFile(DirectoryPath);
        if (FilePath != string.Empty)
        {
            PdfDirectoryPath = System.IO.Path.Combine(DirectoryPath, "PDF Filer");
            ExcelResultDirectoryPath = System.IO.Path.Combine(DirectoryPath, "Excel Result");
            System.IO.Directory.CreateDirectory(PdfDirectoryPath);
            System.IO.Directory.CreateDirectory(ExcelResultDirectoryPath);

            Console.WriteLine(FilePath);
            NewExcelFileObject = fr.ExcelReader(FilePath);
            string ResultFilePath = fr.FindFile(ExcelResultDirectoryPath);
            if(ResultFilePath != string.Empty)
            {
                ExcelResultFileObject = fr.ExcelReader(ResultFilePath);
            }
            
        }
        else
        {
            Console.WriteLine("No Such Directory Found Please try again.");
            Console.WriteLine();
            Console.Clear();
            StartMenu();
        }
        
    }
    
    async private void ResetDownload()
    {
        await Download(NewExcelFileObject);
        CreateExcelResult();
    }

    async private void DownloadNew()
    {
        List<ExcelRow> UpdatedList = fr.ReturnUpdatedList(NewExcelFileObject, ExcelResultFileObject);
        await Download(UpdatedList);
        CreateExcelResult();
    }

    async private Task Download(List<ExcelRow> ExcelList)
    {
        await pdfDownloader.FillDownLoadResults(ExcelList, PdfDirectoryPath);
        downloadResults = pdfDownloader.downloadResults;
    }

    private void CreateExcelResult()
    {
        OutputFileWriter outputFileWriter = new OutputFileWriter();
        outputFileWriter.Write(ExcelResultDirectoryPath, downloadResults);
        End();
    }

    private void End()
    {
        Environment.Exit(0);
    }
}