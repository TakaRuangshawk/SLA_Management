
using OfficeOpenXml;
using SLA_Management.Models.TermProbModel;
using System.Globalization;


namespace SLA_Management.Data.TermProbDB.ExcelUtilitie
{
    public class ExcelUtilities_gateway
    {
        #region  Local Variable

        gateway_seek param = null;
        CultureInfo _cultureEnInfo = new CultureInfo("en-US");

        #endregion

        #region Property
        public string PathDefaultTemplate { get; set; }

        public string FileSaveAsXlsxFormat { get; set; }

        #endregion

        #region Contractor
        public ExcelUtilities_gateway(gateway_seek paramTemp)
        {
            param = paramTemp;
        }

        public ExcelUtilities_gateway()
        {
            param = new gateway_seek();
        }

        #endregion

        #region Function 
        public void GatewayOutput(List<GatewayTransaction> objData)
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

                FileInfo oTemplate = new FileInfo(Path.Combine(PathDefaultTemplate, "GatewayTransaction.xlsx"));
                using (var oPackage = new ExcelPackage(oTemplate))
                {
                    // ExcelWorksheet excelWorksheet = oPackage.Workbook.Worksheets.First<ExcelWorksheet>();

                    var oWorkbook = oPackage.Workbook;
                    var excelWorksheet = oWorkbook.Worksheets[0];

                    excelWorksheet.Name = "GatewayTransaction";

                    if (param.TerminalNo == null || param.TerminalNo == "")
                    {
                        param.TerminalNo = "All";
                    }

                    excelWorksheet.Cells[2, 1].Value = "Gateway Report";
                    excelWorksheet.Cells[3, 1].Value = "AS AT " + param.FRDATE.Substring(0, 10) + "-" + param.TODATE.Substring(0, 10);
                    excelWorksheet.Cells[4, 2].Value = param.FRDATE.Substring(0, 10);
                    excelWorksheet.Cells[4, 4].Value = param.TODATE.Substring(0, 10);
                    excelWorksheet.Cells[4, 7].Value = DateTime.Now.ToString("dd/MM/yyyy", _cultureEnInfo);
                    excelWorksheet.Cells[5, 2].Value = param.TerminalNo;
                    excelWorksheet.Cells[5, 7].Value = DateTime.Now.ToString("HH:mm:ss", _cultureEnInfo);

                    foreach (GatewayTransaction data in objData)
                    {



                        excelWorksheet.Cells[nStartRowData, 1].Value = data.seqno;
                        excelWorksheet.Cells[nStartRowData, 2].Value = data.phoneotp;
                        excelWorksheet.Cells[nStartRowData, 3].Value = data.acctnoto;
                        excelWorksheet.Cells[nStartRowData, 4].Value = data.frombank;
                        excelWorksheet.Cells[nStartRowData, 5].Value = data.transtype;
                        excelWorksheet.Cells[nStartRowData, 6].Value = Convert.ToDateTime(data.transdatetime).ToString("yyyy-MM-dd HH:mm:ss", _cultureEnInfo);
                        excelWorksheet.Cells[nStartRowData, 7].Value = data.terminalno;
                        excelWorksheet.Cells[nStartRowData, 8].Value = data.amount;
                        excelWorksheet.Cells[nStartRowData, 9].Value = data.updatestatus;


                        nStartRowData++;
                        nSeq++;

                    }

                    oPackage.SaveAs(new FileInfo(Path.Combine(Path.Combine(PathDefaultTemplate.Replace("InputTemplate", "tempfiles"), "GatewayTransaction.xlsx"))));
                    FileSaveAsXlsxFormat = "GatewayTransaction.xlsx";
                }

            }
            catch (Exception ex)
            { throw ex; }
        }

        #endregion


    }
}
