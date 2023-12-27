using OfficeOpenXml;
using SLA_Management.Controllers;
using System.Globalization;


namespace SLA_Management.Data.ExcelUtilitie
{
    public class ExcelUtilitiesgateway
    {
        #region  Local Variable

        
        readonly CultureInfo _cultureEnInfo = new CultureInfo("en-US");

        readonly Loger log = new Loger();

        #endregion

        #region Property
        public string? PathDefaultTemplate { get; set; }

        public string? FileSaveAsXlsxFormat { get; set; }


        #endregion

        #region Contractor
        public ExcelUtilitiesgateway()
        {
           
        }



        #endregion

        #region Function 
        public void GatewayOutput(List<GatewayModel> objData,string terminal,string fromdate,string todate,string tmp_term , string tmp_fromdate,string tmp_todate)
        {
            int nStartRowData = 0;
          
            int nSeq = 1;


            try
            {
                nStartRowData = 8;

                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                FileInfo oTemplate = new FileInfo(Path.Combine(PathDefaultTemplate ?? "", "wwwroot\\GatewayExcel\\InputTemplate\\GatewayTransaction.xlsx"));
                using (var oPackage = new ExcelPackage(oTemplate))
                {
                    

                    var oWorkbook = oPackage.Workbook;
                    var excelWorksheet = oWorkbook.Worksheets["Sheet1"];

                   

                    if (tmp_term == null || tmp_term == "")
                    {
                        terminal = "All";
                    }
                    else
                    {
                        terminal = tmp_term;
                    }

                    excelWorksheet.Cells[3, 1].Value = "Gateway Report";
                  
                    excelWorksheet.Cells[4, 2].Value = tmp_fromdate;
                    excelWorksheet.Cells[4, 4].Value = tmp_todate;
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

                    oPackage.SaveAs(new FileInfo(Path.Combine(Path.Combine(PathDefaultTemplate ?? "".Replace("InputTemplate", "tempfiles"), "GatewayTransaction.xlsx"))));
                    FileSaveAsXlsxFormat = "GatewayTransaction.xlsx";
                }

            }
            catch (Exception ex)
            {
                log.WriteErrLog("GatewayOutput error : " + ex);
                throw;
            }
        }

        #endregion


    }


}
