
using OfficeOpenXml;
using SLA_Management.Controllers;
using SLA_Management.Models.TermProbModel;
using System.Globalization;


namespace SLA_Management.Data.ExcelUtilitie
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



        #endregion

        #region Function 
        public void GatewayOutput(List<GatewayModel> objData,string terminal,string fromdate,string todate)
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

                FileInfo oTemplate = new FileInfo(Path.Combine(PathDefaultTemplate, "wwwroot\\GatewayExcel\\InputTemplate\\GatewayTransaction.xlsx"));
                using (var oPackage = new ExcelPackage(oTemplate))
                {
                    // ExcelWorksheet excelWorksheet = oPackage.Workbook.Worksheets.First<ExcelWorksheet>();

                    var oWorkbook = oPackage.Workbook;
                    var excelWorksheet = oWorkbook.Worksheets["Sheet1"];

                    //excelWorksheet.Name = "GatewayTransaction";

                    if (GatewayController.tmp_term == null || GatewayController.tmp_term == "")
                    {
                        terminal = "All";
                    }
                    else
                    {
                        terminal = GatewayController.tmp_term;
                    }

                    excelWorksheet.Cells[3, 1].Value = "Gateway Report";
                    //excelWorksheet.Cells[3, 1].Value = "AS AT " + param.FRDATE.Substring(0, 10) + "-" + param.TODATE.Substring(0, 10);
                    excelWorksheet.Cells[4, 2].Value = GatewayController.tmp_fromdate;
                    excelWorksheet.Cells[4, 4].Value = GatewayController.tmp_todate;
                    excelWorksheet.Cells[4, 7].Value = DateTime.Now.ToString("dd/MM/yyyy", _cultureEnInfo);
                    excelWorksheet.Cells[5, 2].Value = terminal;
                    excelWorksheet.Cells[5, 7].Value = DateTime.Now.ToString("HH:mm:ss", _cultureEnInfo);

                    foreach (GatewayModel data in objData)
                    {



                        excelWorksheet.Cells[nStartRowData, 1].Value = data.Id;
                        excelWorksheet.Cells[nStartRowData, 2].Value = data.SeqNo;
                        excelWorksheet.Cells[nStartRowData, 3].Value = data.ThaiID;
                        excelWorksheet.Cells[nStartRowData, 4].Value = data.PhoneOTP;
                        excelWorksheet.Cells[nStartRowData, 5].Value = data.AcctNoTo;
                        excelWorksheet.Cells[nStartRowData, 6].Value = data.FromBank;
                        excelWorksheet.Cells[nStartRowData, 7].Value = data.TransType;
                        excelWorksheet.Cells[nStartRowData, 8].Value = Convert.ToDateTime(data.TransDateTime).ToString("yyyy-MM-dd HH:mm:ss", _cultureEnInfo);
                        excelWorksheet.Cells[nStartRowData, 9].Value = data.TerminalNo;
                        excelWorksheet.Cells[nStartRowData, 10].Value = data.Amount;
                        excelWorksheet.Cells[nStartRowData, 11].Value = data.UpdateStatus;
                        excelWorksheet.Cells[nStartRowData, 12].Value = data.ErrorCode;


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
