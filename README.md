# PDFReader

PDFReader reads document URLs from an Excel workbook, downloads PDF files, and writes a workbook with the download results.

## Requirements

- Windows
- The .NET 10 SDK installed. In Windows Terminal or PowerShell, install it with `winget install Microsoft.DotNet.SDK.10` (or use the [.NET 10 download page](https://dotnet.microsoft.com/download/dotnet/10.0)).
- An internet connection, so the program can access the URLs in your spreadsheet

## Install and run

1. Install the .NET 10 SDK from the [.NET download page](https://dotnet.microsoft.com/download/dotnet/10.0).
2. Download or clone this repository to your computer.
3. Double-click `PDFReader.bat` in the repository folder. The batch file builds and starts the program; the first run may take longer while dependencies are restored.
4. When prompted, enter the path to the directory containing your Excel workbook, for example `C:\Users\My_User\Documents\PDFsToDownload`.

The directory you provide must contain **exactly one `.xlsx` file**. Keep the workbook in its own directory; do not put other files in that directory.

## Excel workbook

Put the column headers in the first row. The program recognizes these names (case-insensitively):

| Header | Purpose |
| --- | --- |
| `id` | Identifier used in the downloaded filename |
| `url` | Primary URL to try |
| `alt url` | Optional alternate URL to try if the primary URL fails |

For a new input workbook, provide at least `id` and `url`; include `alt url` if you have a backup URL. The `Success`, `Filename`, and `Error` headers are not required in the input workbook. The program provides those columns in `Excel Result/Result.xlsx`.

## Download options

- **Reset: download all files** — processes the workbook from scratch and replaces the previous results workbook.
- **Download only new files** — compares the input workbook with the existing results and processes new items, retaining prior results.

The program creates a `PDF Filer` directory for downloaded PDFs and an `Excel Result` directory containing `Result.xlsx`, both alongside your input workbook.

## Recommendation

Some hosts may time out or temporarily fail during downloads. It is recommended to run the program twice: first choose **Reset**, then run it again and choose **Download only new files** to retry incomplete downloads while keeping the results.

Open the Result.xlsx and look for 403 errors. These errors may happens due to a cloudflare issue that prevents httpRequests or they may be actually permission denied errors. If you want these files try to download them manually.
