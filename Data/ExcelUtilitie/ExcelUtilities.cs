
using OfficeOpenXml;
using SLA_Management.Models;
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

    public class ExcelUtilities_Regulator
    {
        #region  Local Variable

        regulator_seek param = null;
        CultureInfo _cultureEnInfo = new CultureInfo("en-US");

        #endregion

        #region Property
        public string PathDefaultTemplate { get; set; }

        public string FileSaveAsXlsxFormat { get; set; }

        #endregion

        #region Contractor
        public ExcelUtilities_Regulator(regulator_seek paramTemp)
        {
            param = paramTemp;
        }

        public ExcelUtilities_Regulator()
        {
            param = new regulator_seek();
        }

        #endregion

        #region Function 
        public void GatewayOutput(List<Regulator> objData)
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

                FileInfo oTemplate = new FileInfo(Path.Combine(PathDefaultTemplate, "wwwroot\\RegulatorExcel\\InputTemplate\\Regulator.xlsx"));
                using (var oPackage = new ExcelPackage(oTemplate))
                {
                    //ExcelWorksheet excelWorksheet = oPackage.Workbook.Worksheets.First<ExcelWorksheet>();

                    var oWorkbook = oPackage.Workbook;
                    //var excelWorksheet = oWorkbook.Worksheets[0];
                    var excelWorksheet = oWorkbook.Worksheets["Sheet1"];
                    //excelWorksheet.Name = "Regulator";

                    if (param.TerminalNo == null || param.TerminalNo == "")
                    {
                        param.TerminalNo = "All";
                    }

                    excelWorksheet.Cells[2, 1].Value = "Regulator Report";
                    //excelWorksheet.Cells[3, 1].Value = "AS AT " + param.FRDATE.Substring(0, 10) + "-" + param.TODATE.Substring(0, 10);
                    excelWorksheet.Cells[4, 2].Value = param.FRDATE.Substring(0, 10);
                    excelWorksheet.Cells[4, 4].Value = param.TODATE.Substring(0, 10);
                    excelWorksheet.Cells[4, 7].Value = DateTime.Now.ToString("dd/MM/yyyy", _cultureEnInfo);
                    excelWorksheet.Cells[5, 2].Value = param.TerminalNo;
                    excelWorksheet.Cells[5, 7].Value = DateTime.Now.ToString("HH:mm:ss", _cultureEnInfo);

                    foreach (Regulator data in objData)
                    {



                        excelWorksheet.Cells[nStartRowData, 1].Value = data.TERMID;
                        excelWorksheet.Cells[nStartRowData, 2].Value = data.DEP100;
                        excelWorksheet.Cells[nStartRowData, 3].Value = data.DEP500;
                        excelWorksheet.Cells[nStartRowData, 4].Value = data.DEP1000;
                        excelWorksheet.Cells[nStartRowData, 5].Value = data.WDL100;
                        excelWorksheet.Cells[nStartRowData, 6].Value = data.WDL500;
                        excelWorksheet.Cells[nStartRowData, 7].Value = data.WDL1000;
                        excelWorksheet.Cells[nStartRowData, 8].Value = data.DIFF100;
                        excelWorksheet.Cells[nStartRowData, 9].Value = data.DIFF500;
                        excelWorksheet.Cells[nStartRowData, 10].Value = data.DIFF1000;


                        nStartRowData++;
                        nSeq++;

                    }

                    oPackage.SaveAs(new FileInfo(Path.Combine(Path.Combine(PathDefaultTemplate.Replace("InputTemplate", "tempfiles"), "Regulator.xlsx"))));
                    FileSaveAsXlsxFormat = "Regulator.xlsx";
                }

            }
            catch (Exception ex)
            { throw ex; }
        }

        #endregion
    }
    public class ExcelUtilities_CheckLastUpdate
    {
        #region  Local Variable

        ejchecksize_seek param = null;
        CultureInfo _cultureEnInfo = new CultureInfo("en-US");

        #endregion

        #region Property
        public string PathDefaultTemplate { get; set; }

        public string FileSaveAsXlsxFormat { get; set; }

        #endregion

        #region Contractor
        public ExcelUtilities_CheckLastUpdate(ejchecksize_seek paramTemp)
        {
            param = paramTemp;
        }

        public ExcelUtilities_CheckLastUpdate()
        {
            param = new ejchecksize_seek();
        }

        #endregion

        #region Function 
        public void GatewayOutput(List<ejloglastupdate> objData)
        {
            int nStartRowData = 0;
            string strTermID = string.Empty;
            string strBranchName = string.Empty;
            string strLocation = string.Empty;
            string strProbName = string.Empty;
            int nSeq = 1;


            try
            {
                nStartRowData = 6;

                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                FileInfo oTemplate = new FileInfo(Path.Combine(PathDefaultTemplate, "wwwroot\\RegulatorExcel\\InputTemplate\\CheckLastUpdate.xlsx"));
                using (var oPackage = new ExcelPackage(oTemplate))
                {
                    //ExcelWorksheet excelWorksheet = oPackage.Workbook.Worksheets.First<ExcelWorksheet>();

                    var oWorkbook = oPackage.Workbook;
                    //var excelWorksheet = oWorkbook.Worksheets[0];
                    var excelWorksheet = oWorkbook.Worksheets["Sheet1"];
                    //excelWorksheet.Name = "Regulator";

                    if (param.TerminalNo == null || param.TerminalNo == "")
                    {
                        param.TerminalNo = "All";
                    }

                    excelWorksheet.Cells[1, 1].Value = "Check EJ Last Update Report";
                    //excelWorksheet.Cells[3, 1].Value = "AS AT " + param.FRDATE.Substring(0, 10) + "-" + param.TODATE.Substring(0, 10);
                    //excelWorksheet.Cells[4, 2].Value = param.FRDATE.Substring(0, 10);
                    //excelWorksheet.Cells[4, 4].Value = param.TODATE.Substring(0, 10);
                    excelWorksheet.Cells[3, 6].Value = DateTime.Now.ToString("dd/MM/yyyy", _cultureEnInfo);
                    //excelWorksheet.Cells[5, 2].Value = param.TerminalNo;
                    excelWorksheet.Cells[4, 6].Value = DateTime.Now.ToString("HH:mm:ss", _cultureEnInfo);

                    foreach (ejloglastupdate data in objData)
                    {



                        excelWorksheet.Cells[nStartRowData, 1].Value = data.term_seq;
                        excelWorksheet.Cells[nStartRowData, 2].Value = data.term_id;
                        excelWorksheet.Cells[nStartRowData, 3].Value = data.term_name;
                        excelWorksheet.Cells[nStartRowData, 4].Value = data.update_date;
                        excelWorksheet.Cells[nStartRowData, 5].Value = data.lastran_date;
                        excelWorksheet.Cells[nStartRowData, 6].Value = data.status;
                        nStartRowData++;
                        nSeq++;

                    }

                    oPackage.SaveAs(new FileInfo(Path.Combine(Path.Combine(PathDefaultTemplate.Replace("InputTemplate", "tempfiles"), "CheckLastUpdate.xlsx"))));
                    FileSaveAsXlsxFormat = "CheckLastUpdate.xlsx";
                }

            }
            catch (Exception ex)
            { throw ex; }
        }

        #endregion
    }

    public class ExcelUtilities_EJTermProb
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
        public ExcelUtilities_EJTermProb(ej_trandada_seek paramTemp)
        {
            param = paramTemp;
        }

        public ExcelUtilities_EJTermProb()
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
                    var excelWorksheet = oWorkbook.Worksheets[0];

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
