
using OfficeOpenXml;
using SLA_Management.Models.TermProbModel;
using System.Globalization;


namespace SLA_Management.Data.TermProbDB.ExcelUtilitie
{
    public class ExcelUtilities
    {
        #region  Local Variable

        ej_trandada_seek param = null;

        CultureInfo _cultureEnInfo = new CultureInfo("en-US");

        #endregion

        #region Property
        public string PathDefaultTemplate { get; set; }

        public string FileSaveAsXlsxFormat { get; set; }

        #endregion

        #region Contractor
        public ExcelUtilities(ej_trandada_seek paramTemp)
        {
            param = paramTemp;
        }

        public ExcelUtilities()
        {
            param = new ej_trandada_seek();
        }

        #endregion

        #region Function 
        public void GenExcelFileDeviceTermPorb(List<ej_trandeviceprob> objData)
        {
            int nStartRowData = 0;
            string strTermID = string.Empty;
            string strBranchName = string.Empty;
            string strLocation = string.Empty;
            string strProbName = string.Empty;
            int nSeq = 1;


            try
            {
                nStartRowData = 8;

                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                FileInfo oTemplate = new FileInfo(Path.Combine(PathDefaultTemplate, "CheckProblemDevice.xlsx"));
                using (var oPackage = new ExcelPackage(oTemplate))
                {
                    // ExcelWorksheet excelWorksheet = oPackage.Workbook.Worksheets.First<ExcelWorksheet>();

                    var oWorkbook = oPackage.Workbook;
                    var excelWorksheet = oWorkbook.Worksheets.First();

                    excelWorksheet.Name = "DeviceTermProb";

                    if (param.TERMID == null || param.TERMID == "") param.TERMID = "All";

                    excelWorksheet.Cells[2, 1].Value = "Terminal Device Error Report";
                    excelWorksheet.Cells[3, 1].Value = "AS AT " + param.FRDATE.Substring(0, 10) + "-" + param.TODATE.Substring(0, 10);
                    excelWorksheet.Cells[4, 2].Value = param.FRDATE.Substring(0, 10);
                    excelWorksheet.Cells[4, 4].Value = param.TODATE.Substring(0, 10);
                    excelWorksheet.Cells[4, 7].Value = DateTime.Now.ToString("dd/MM/yyyy", _cultureEnInfo);
                    excelWorksheet.Cells[5, 2].Value = param.TERMID;
                    excelWorksheet.Cells[5, 7].Value = DateTime.Now.ToString("HH:mm:ss", _cultureEnInfo);

                    foreach (ej_trandeviceprob data in objData)
                    {



                        excelWorksheet.Cells[nStartRowData, 1].Value = nSeq;
                        excelWorksheet.Cells[nStartRowData, 2].Value = data.BranchName;
                        excelWorksheet.Cells[nStartRowData, 3].Value = data.TerminalID;
                        excelWorksheet.Cells[nStartRowData, 4].Value = data.Location;
                        excelWorksheet.Cells[nStartRowData, 5].Value = data.ProbName;
                        excelWorksheet.Cells[nStartRowData, 6].Value = data.Remark;
                        excelWorksheet.Cells[nStartRowData, 7].Value = data.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss", _cultureEnInfo);


                        nStartRowData++;
                        nSeq++;

                    }

                    oPackage.SaveAs(new FileInfo(Path.Combine(Path.Combine(PathDefaultTemplate.Replace("InputTemplate", "tempfiles"), "CheckProblemDeviceTemp.xlsx"))));
                    FileSaveAsXlsxFormat = "CheckProblemDeviceTemp.xlsx";
                }

            }
            catch (Exception ex)
            { throw ex; }
        }

        #endregion


    }
}
