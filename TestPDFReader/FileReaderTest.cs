namespace TestPDFReader;
public class FileReaderTest
{

    [Fact]
    public void FindFile_Success()
    {
        var fileReader = new FileReader();
        var testDataDirectory = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "testData", "CorrectData"));

        var foundFile = fileReader.FindFile(testDataDirectory);

        Assert.Equal(Path.Combine(testDataDirectory, "TestData.xlsx"), foundFile);
    }

    [Fact]
    public void FindFile_Fail()
    {
        var fileReader = new FileReader();
        var testDataDirectory = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "testData", "blank"));

        var foundFile = fileReader.FindFile(testDataDirectory);

        Assert.NotEqual(Path.Combine(testDataDirectory, "TestData.xlsx"), foundFile);
    }

    [Fact]
    public void FindFile_Empty()
    {
        var fileReader = new FileReader();
        var testDataDirectory = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "testData", "blank"));

        var foundFile = fileReader.FindFile(testDataDirectory);

        Assert.Equal("", foundFile);
    }

    [Fact]
    public void FindFile_WrongFileType()
    {
        var fileReader = new FileReader();
        var testDataDirectory = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "testData", "NoneExcelData"));

        var foundFile = fileReader.FindFile(testDataDirectory);

        Assert.Equal("", foundFile);
    }

    [Fact]
    public void FindFile_NoDirectory()
    {
        var fileReader = new FileReader();

        var foundFile = fileReader.FindFile("asdf");

        Assert.Equal("", foundFile);
    }

    [Fact]
    public void ExcelReader_Success()
    {
        var fileReader = new FileReader();
        var testDataDirectory = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "testData", "CorrectData"));

        var foundFile = fileReader.FindFile(testDataDirectory);

        var list =  fileReader.ExcelReader(foundFile);

        var testList = new List<ExcelRow>
        {
            new() { id = 1, url = "https://www.novonordisk.com/content/dam/nncorp/global/en/investors/irmaterial/annual_report/2025/novo-nordisk-annual-report-2024.pdfst", altUrl = "https://www.novonordisk.com/content/dam/nncorp/global/en/investors/irmaterial/annual_report/2025/novo-nordisk-annual-report-2024.pdf"},
            new() { id = 2, url = "https://danskebank.com/-/media/danske-bank-com/file-cloud/2025/2/danske-bank---annual-report-2024.pdf?rev=430be65be4cd43d18fc8adeec2139eb5", altUrl = null},
            new() { id = 3, url = "https://www.cbs.dk/files/cbs.dk/call_to_action/cbs-annual-report-2024.pdf", altUrl = null},
            new() { id = 4, url = "https://www.nordea.com/en/doc/annual-report-nordea-bank-abp-2024-0.pdf", altUrl = null},
            new() { id = 10, url = "https://zerodha.com/variety/chapter/read-annual-report-company/", altUrl = "https://zerodha.com/varsity/chapter/read-annual-report-company/"},
            new() { id = 11, url = "https://report.adidas-group.com/2024/en/_assets/downloads/annual-report-adidas-ar24.pdf", altUrl = null},
            new() { id = 29, url = "https://www.bp.com/content/dam/bp/business-sites/en/global/corporate/pdfs/investors/bp-annual-report-and-form-20f-2024.pdf", altUrl = null},
            new() { id = 30, url = "https://www.boeing.com/content/dam/boeing/boeingdotcom/company/annual-report/2024/2024-annual-report.pdf", altUrl = null},
            new() { id = 31, url = "https://tdcnet.com/media/goyh0bhh/tdc-net-annual-report-2023.pdf", altUrl = null},
            new() { id = 38, url = "https://www.nationmedia.com/wp-content/uploads/2016/11/MVP2015AnnualReport.pdf", altUrl = "https://www.nationmedia.com/wp-content/uploads/2016/11/NMG2015AnnualReport.pdf"},
            new() { id = 62, url = "https://ir.mi.com/system/files-encrypted/nasdaq_kms/assets/2025/04/24/5-27-15/%E8%8B%B1%E6%96%87.pdf", altUrl = null},
            new() { id = 63, url = "https://investors.vodafone.com/~/media/files/v/vodafone-ir/documents/performance/financial-results/2023/vodafone-fy23-annual-report.pdf", altUrl = null},
            new() { id = 64, url = "https://www.jpmorganchase.com/content/damn/jpmc/jpmorgan-chase-and-co/investor-relations/documents/annualreport-2024.pdf", altUrl = "https://www.jpmorganchase.com/content/dam/jpmc/jpmorgan-chase-and-co/investor-relations/documents/annualreport-2024.pdf"},
            new() { id = 86, url = null, altUrl = "https://www.bat.com/content/dam/batcom/global/main-nav/investors-and-reporting/reporting/combined-annual-and-sustainability-report/BAT_Annual_Report_on_Form_20-F_2024.pdf"},
            new() { id = 91, url = "https://www.telekom.com/resource/blob/1085970/9e25d438580a5e3f39521fd94ed5e48c/dt-24-annual-report-data.pdf", altUrl = null},
            new() { id = 111, url = "https://www.adobe.com/cc-shared/assets/investor-relations/pdfs/adbe-2024-annual-report.pdf", altUrl = null},
            new() { id = 112, url = "https://www.parliament.uk/globalassets/documents/commons-expenditure/admin-annual-accounts/administration_annual_report_and_accounts_2022_23-accessible.pdf", altUrl = null},
            new() { id = 116, url = "https://www.statestreet.com/web/about/our-story/annual-report/documents/ssc-annual-report-2024.pdf", altUrl = null},
            new() { id = 131, url = "https://www.telefonica.com/en/wp-content/uploads/sites/5/2024/02/Consolidated-Annual-Accounts-2027.pdf", altUrl = "https://www.telefonica.com/en/wp-content/uploads/sites/5/2024/02/Consolidated-Annual-Accounts-2023.pdf"},
            new() { id = 132, url = "https://www.economie.gouv.fr/files/files/directions_services/agence-participations-etat/Documents/Rapports-de-l-Etat-actionnaire/2023/REA2023_Ang-web.pdf?v=1705416463", altUrl = null},
            new() { id = 154, url = "https://media.defense.gov/2024/Dec/18/2003615520/-1/-1/0/MILITARY-AND-SECURITY-DEVELOPMENTS-INVOLVING-THE-PEOPLES-REPUBLIC-OF-CHINA-PLEASE-HELP-CAPS-LOCK-STUCK-2024.PDF", altUrl = "https://media.defense.gov/2024/Dec/18/2003615520/-1/-1/0/MILITARY-AND-SECURITY-DEVELOPMENTS-INVOLVING-THE-PEOPLES-REPUBLIC-OF-CHINA-2024.PDF"},
            new() { id = 160, url = "https://www.relax.com/~/media/Files/R/RELX-Group/documents/reports/annual-reports/2020-annual-report.pdf", altUrl = "https://www.relx.com/~/media/Files/R/RELX-Group/documents/reports/annual-reports/2020-annual-report.pdf"},
            new() { id = 161, url = "https://www.bp.com/content/dam/bp/business-sites/en/global/corporate/pdfs/investors/bp-annual-report-and-form-20f-2019.pdf", altUrl = null},
            new() { id = 162, url = "https://www.nestle.com/sites/default/files/2025-02/2024-financial-statements-en.pdf", altUrl = null},
            new() { id = 163, url = "https://assets.publishing.service.gov.uk/media/67347985b613efc3f1822fe8/dwp_annual_report_and_accounts_2023_2024.pdf", altUrl = null},
            new() { id = 164, url = "https://www.athena.com/media/inqidi52/atea-annual-report-2021.pdf", altUrl = "https://www.atea.com/media/inqidi52/atea-annual-report-2021.pdf"},
            new() { id = 247, url = "https://ncert.nic.in/textbook/pdf/leac204.pdf", altUrl = null}
        };

        for (int i = 0; i < testList.Count(); i++)
        {
            Assert.Equal(testList[i].id,  list[i].id);
            Assert.Equal(testList[i].url,  list[i].url);
            Assert.Equal(testList[i].altUrl,  list[i].altUrl);
        }

    }

    [Fact]
    public void ReturnUpdatedList_WhenOldListIsNull()
    {
        var fileReader = new FileReader();
        var newList = new List<ExcelRow>
        {
            new() { id = 1, url = "https://example.com/report.pdf" }
        };

        List<ExcelRow> result = fileReader.ReturnUpdatedList(newList, null!);

        Assert.Equal(newList.Count(), result.Count());

        for (int i = 0; i < newList.Count(); i++)
        {
            Assert.Equal(newList[i].id,  result[i].id);
            Assert.Equal(newList[i].url,  result[i].url);
            Assert.Equal(newList[i].altUrl,  result[i].altUrl);
        }

    }

    [Fact]
    public void ReturnUpdatedList_WhenOldListIsNotNull()
    {
        var fileReader = new FileReader();
        List<ExcelRow> newList = new List<ExcelRow>
        {
            new() { id = 1, url = "https://example.com/report.pdf" }
        };
        var testDataDirectory = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "testData", "CorrectData"));

        var foundFile = fileReader.FindFile(testDataDirectory);
        List<ExcelRow> oldList = fileReader.ExcelReader(foundFile);

        List<ExcelRow> result = fileReader.ReturnUpdatedList(newList, oldList);
        List<ExcelRow> combined = [.. oldList, .. newList];
        combined = combined.OrderBy(x => x.id).ToList();

        Assert.Equal(combined.Count(), result.Count());

        for (int i = 0; i < combined.Count(); i++)
        {
            Assert.Equal(combined[i].id,  result[i].id);
            Assert.Equal(combined[i].url,  result[i].url);
            Assert.Equal(combined[i].altUrl,  result[i].altUrl);
        }
    }

    [Fact]
    public void ReturnUpdatedList_WhenDuplicateValue()
    {
        var fileReader = new FileReader();
        List<ExcelRow> newList = new List<ExcelRow>
        {
            new() { id = 1, url = "https://example.com/report.pdf" }
        };

        List<ExcelRow> oldList = new List<ExcelRow>
        {
            new() { id = 1, url = "https://example.com/report.pdf", altUrl=null, Success= true, Filename="example.pdf", Error="" }
        };

        List<ExcelRow> result = fileReader.ReturnUpdatedList(newList, oldList);


        Assert.Equal(oldList.Count(), result.Count());

        for (int i = 0; i < oldList.Count(); i++)
        {
            Assert.Equal(oldList[i].id,  result[i].id);
            Assert.Equal(oldList[i].url,  result[i].url);
            Assert.Equal(oldList[i].altUrl,  result[i].altUrl);
            Assert.Equal(oldList[i].Success,  result[i].Success);
            Assert.Equal(oldList[i].Filename,  result[i].Filename);
            Assert.Equal(oldList[i].Error,  result[i].Error);
        }

    }

    [Fact]
    public void ReturnUpdatedList_NewId()
    {
        var fileReader = new FileReader();
        List<ExcelRow> newList = new List<ExcelRow>
        {
            new() { id = 2, url = "https://example.com/report.pdf", altUrl=null }
        };

        List<ExcelRow> oldList = new List<ExcelRow>
        {
            new() { id = 1, url = "https://example.com/report.pdf", altUrl=null, Success= true, Filename="example.pdf", Error="" }
        };

        List<ExcelRow> result = fileReader.ReturnUpdatedList(newList, oldList);


        Assert.Equal(newList.Count(), result.Count());

        for (int i = 0; i < oldList.Count(); i++)
        {
            Assert.Equal(newList[i].id, result[i].id);
            Assert.Equal(newList[i].url, result[i].url);
            Assert.Equal(newList[i].altUrl, result[i].altUrl);
            Assert.Equal(newList[i].Success, result[i].Success);
            Assert.Equal(newList[i].Filename, result[i].Filename);
            Assert.Equal(newList[i].Error, result[i].Error);
        }

    }

    
}