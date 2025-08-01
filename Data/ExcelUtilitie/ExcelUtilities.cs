﻿
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SLA_Management.Models.LogMonitorModel;
using SLA_Management.Models.OperationModel;
using SLA_Management.Models.ReportModel;
using SLA_Management.Models.TermProbModel;
using System.Drawing;
using System.Globalization;
using static SLA_Management.Controllers.MaintenanceController;
using static SLA_Management.Controllers.MonitorController;
using static SLA_Management.Controllers.ReportController;

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
        public void GatewayOutput(List<TicketManagement> objData)
        {
            int nStartRowData = 0;
            string strTermID = string.Empty;
            string strBranchName = string.Empty;
            string strLocation = string.Empty;
            string strProbName = string.Empty;
            int nSeq = 1;


            try
            {
                nStartRowData = 2;

                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                FileInfo oTemplate = new FileInfo(Path.Combine(PathDefaultTemplate, "wwwroot\\RegulatorExcel\\InputTemplate\\Ticket.xlsx"));
                using (var oPackage = new ExcelPackage(oTemplate))
                {
                    var oWorkbook = oPackage.Workbook;
                    var excelWorksheet = oWorkbook.Worksheets["Sheet1"];
                    foreach (TicketManagement data in objData)
                    {



                        excelWorksheet.Cells[nStartRowData, 1].Value = data.Open_Date;
                        excelWorksheet.Cells[nStartRowData, 2].Value = data.Appointment_Date;
                        excelWorksheet.Cells[nStartRowData, 3].Value = data.Closed_Repair_Date;
                        excelWorksheet.Cells[nStartRowData, 4].Value = data.Down_Time;
                        excelWorksheet.Cells[nStartRowData, 5].Value = data.Actual_Open_Date;
                        excelWorksheet.Cells[nStartRowData, 6].Value = data.Actual_Appointment_Date;
                        excelWorksheet.Cells[nStartRowData, 7].Value = data.Actual_Closed_Repair_Date;
                        excelWorksheet.Cells[nStartRowData, 8].Value = data.Actual_Down_Time;
                        excelWorksheet.Cells[nStartRowData, 9].Value = data.Status;
                        excelWorksheet.Cells[nStartRowData, 10].Value = data.TERM_ID;
                        excelWorksheet.Cells[nStartRowData, 11].Value = data.TERM_SEQ;
                        excelWorksheet.Cells[nStartRowData, 12].Value = data.TERM_NAME;
                        excelWorksheet.Cells[nStartRowData, 13].Value = data.Problem_Detail;
                        excelWorksheet.Cells[nStartRowData, 14].Value = data.Solving_Program;
                        excelWorksheet.Cells[nStartRowData, 15].Value = data.Service_Team;
                        excelWorksheet.Cells[nStartRowData, 16].Value = data.Contact_Name_Branch_CIT;
                        excelWorksheet.Cells[nStartRowData, 17].Value = data.Open_By;
                        excelWorksheet.Cells[nStartRowData, 18].Value = data.Remark;
                        excelWorksheet.Cells[nStartRowData, 19].Value = data.Job_No;
                        excelWorksheet.Cells[nStartRowData, 20].Value = data.Aservice_Status;
                        excelWorksheet.Cells[nStartRowData, 21].Value = data.Service_Type;
                        excelWorksheet.Cells[nStartRowData, 22].Value = data.Open_Name;
                        excelWorksheet.Cells[nStartRowData, 23].Value = data.Assign_By;
                        excelWorksheet.Cells[nStartRowData, 24].Value = data.Zone_Area;
                        excelWorksheet.Cells[nStartRowData, 25].Value = data.Main_Problem;
                        excelWorksheet.Cells[nStartRowData, 26].Value = data.Sub_Problem;
                        excelWorksheet.Cells[nStartRowData, 27].Value = data.Main_Solution;
                        excelWorksheet.Cells[nStartRowData, 28].Value = data.Sub_Solution;
                        excelWorksheet.Cells[nStartRowData, 29].Value = data.Part_of_use;
                        excelWorksheet.Cells[nStartRowData, 30].Value = data.TechSupport;
                        excelWorksheet.Cells[nStartRowData, 31].Value = data.CIT_Request;
                        excelWorksheet.Cells[nStartRowData, 32].Value = data.Terminal_Status;
                        nStartRowData++;
                        nSeq++;

                    }

                    oPackage.SaveAs(new FileInfo(Path.Combine(Path.Combine(PathDefaultTemplate.Replace("InputTemplate", "tempfiles"), "Ticket.xlsx"))));
                    FileSaveAsXlsxFormat = "Ticket.xlsx";
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

                    excelWorksheet.Cells[2, 1].Value = "Problem Monitor Report";
                    excelWorksheet.Cells[3, 1].Value = "AS AT " + param.FRDATE.Substring(0, 10) + "-" + param.TODATE.Substring(0, 10);
                    excelWorksheet.Cells[4, 2].Value = param.FRDATE.Substring(0, 10);
                    excelWorksheet.Cells[4, 4].Value = param.TODATE.Substring(0, 10);
                    excelWorksheet.Cells[4, 7].Value = DateTime.Now.ToString("dd/MM/yyyy", _cultureEnInfo);
                    excelWorksheet.Cells[5, 2].Value = param.TERMID;
                    excelWorksheet.Cells[5, 7].Value = DateTime.Now.ToString("HH:mm:ss", _cultureEnInfo);

                    foreach (ej_trandeviceprob data in objData)
                    {




                        excelWorksheet.Cells[nStartRowData, 1].Value = nSeq;
                        excelWorksheet.Cells[nStartRowData, 2].Value = data.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss", _cultureEnInfo);
                        excelWorksheet.Cells[nStartRowData, 3].Value = data.Seqno;
                        excelWorksheet.Cells[nStartRowData, 4].Value = data.TerminalID;
                        excelWorksheet.Cells[nStartRowData, 5].Value = data.BranchName;
                        excelWorksheet.Cells[nStartRowData, 6].Value = data.Location;
                        excelWorksheet.Cells[nStartRowData, 7].Value = data.Memo;
                        excelWorksheet.Cells[nStartRowData, 8].Value = data.Remark;




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

    public class ExcelUtilities_AuditReport
    {
        public string PathDefaultTemplate { get; set; }

        public string FileSaveAsXlsxFormat { get; set; }

        public void GenExcelFileUserDetailReport(IList<fv_system_users> objData)
        {
            int nStartRowData = 0;

            try
            {
              

                nStartRowData = 3;

                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                FileInfo oTemplate = new FileInfo(Path.Combine(PathDefaultTemplate, "ReportUserFV_SecOne.xlsx"));
                using (var oPackage = new ExcelPackage(oTemplate))
                {

                    //If error , check the path of excel carefully !
                    //ExcelWorksheet excelWorksheet = oPackage.Workbook.Worksheets.First<ExcelWorksheet>();
                    //excelWorksheet.Name = "sheet1";
                    var oWorkbook = oPackage.Workbook;
                    var excelWorksheet = oWorkbook.Worksheets[0];

                    excelWorksheet.Name = "ReportUser";
                    //excelWorksheet.Cells[1, 1].Value = objData[0].System;

                    foreach (fv_system_users data in objData)
                    {

                        excelWorksheet.Cells[nStartRowData, 1].Value = data.AccountName;
                        excelWorksheet.Cells[nStartRowData, 2].Value = data.UserName;
                        excelWorksheet.Cells[nStartRowData, 3].Value = data.LastLoginDateTime.ToString("dd-MM-yyyy HH:mm:ss");



                        nStartRowData++;


                    }

                    //if (String.IsNullOrEmpty(date.ToString()))
                    //{
                    //    date = DateTime.Now.AddMonths(-1);
                    //}


                    //string excelName = "ReportUserFV_" + date.ToString("yyyy_MM") + ".xlsx";



                    oPackage.SaveAs(new FileInfo(Path.Combine(Path.Combine(PathDefaultTemplate.Replace("InputTemplate", "tempfiles"), "ReportUserFV_SecOne.xlsx"))));
                    FileSaveAsXlsxFormat = "ReportUserFV_SecOne.xlsx";

                    

                }

               

            }
            catch (Exception ex)
            {
                throw ex;


            }

        }

        //public void GenExcelFileSecOneUserDetailReport(IList<fv_system_users> objData, DateTime date)
        //{
        //    int nStartRowData = 0;

        //    try
        //    {

        //        nStartRowData = 3;

        //        ExcelPackage.LicenseContext = LicenseContext.Commercial;
        //        FileInfo oTemplate = new FileInfo(Path.Combine(PathDefaultTemplate, "ReportUserFV_SecOne.xlsx"));
        //        using (var oPackage = new ExcelPackage(oTemplate))
        //        {

        //            //If error , check the path of excel carefully !
        //            //ExcelWorksheet excelWorksheet = oPackage.Workbook.Worksheets.First<ExcelWorksheet>();
        //            //excelWorksheet.Name = "sheet1";

        //            var oWorkbook = oPackage.Workbook;
        //            var excelWorksheet = oWorkbook.Worksheets[0];

        //            excelWorksheet.Name = "DeviceTermProb";
        //            //ExcelWorksheet worksheet = oPackage.Workbook.Worksheets[0];

        //            //string str = "ID, ACCOUNT, NAME, PASSWORD, DEPT_ID, TEL, MOBILE, EMAIL, QQNO, FLAG, UPDATE_DATE, LOGIN_FAIL_COUNT, LOGIN_LOCKED_DATE, LAST_LOGINIP, LAST_LOGINTIME, LOGIN_TOTAL, PASSWORD_EXPIRED_DATE";
        //            //string[] strSpilt = str.Split(',');

        //            //for (int i = 0; i < strSpilt.Length; i++)
        //            //{
        //            //    excelWorksheet.Cells[nStartRowData - 1, i +1].Value = strSpilt[i];
        //            //}

        //            foreach (fv_system_users data in objData)
        //            {


        //                excelWorksheet.Cells[nStartRowData, 1].Value = data.AccountName;
        //                excelWorksheet.Cells[nStartRowData, 2].Value = data.UserName;
        //                excelWorksheet.Cells[nStartRowData, 3].Value = data.LastLoginDateTime.ToString("dd-MM-yyyy HH:mm:ss");



        //                nStartRowData++;


        //            }

        //            //if (String.IsNullOrEmpty(date.ToString()))
        //            //{
        //            //    date = DateTime.Now.AddMonths(-1);
        //            //}


        //            //string excelName = "ReportUserSecOne_" + date.ToString("yyyy_MM") + ".xlsx";

        //            //path_Excel = PathDefaultTemplate + excelName;

        //            oPackage.SaveAs(new FileInfo(Path.Combine(Path.Combine(PathDefaultTemplate.Replace("InputTemplate", "tempfiles"), "ReportUserFV_SecOne.xlsx"))));
        //            FileSaveAsXlsxFormat = "ReportUserFV_SecOne.xlsx";

        //        }

              

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //}


    }

    public class ExcelUtilities_SLALogMonitor
    {
        #region Local Variable

        CultureInfo _cultureEnInfo = new CultureInfo("en-US");

        #endregion

        #region Property

        public string PathDefaultTemplate { get; set; }

        public string FileSaveAsXlsxFormat { get; set; }

        #endregion

        #region Constructor

        public ExcelUtilities_SLALogMonitor()
        {
        }

        #endregion

        #region Function

        public void ExportToExcel(List<Dictionary<string, object>> objData, LatestStatusDto latestDto, string outputFilePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;

            FileInfo templateFile = new FileInfo(Path.Combine(PathDefaultTemplate, "SLALogMonitorTemplate.xlsx"));
            using (var package = new ExcelPackage(templateFile))
            {
                // --------- Main Sheet ---------
                var mainSheet = package.Workbook.Worksheets["Sheet1"];
                mainSheet.Name = "Main";

                int col = 1;
                foreach (var header in objData[0].Keys)
                {
                    var cell = mainSheet.Cells[1, col];
                    cell.Value = header;
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.Orange);
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    col++;
                }

                int row = 2;
                foreach (var rowData in objData)
                {
                    col = 1;
                    foreach (var value in rowData.Values)
                    {
                        mainSheet.Cells[row, col].Value = value;
                        col++;
                    }
                    row++;
                }

                for (int i = 1; i <= objData[0].Count; i++)
                {
                    mainSheet.Column(i).AutoFit();
                }

                // --------- Latest Summary Sheet ---------
                var summarySheet = package.Workbook.Worksheets.Add("Latest Summary");

                // Header labels
                string[] summaryHeaders = {
    "COUNT_ALL_TERMINAL",
    "COUNT_TERMINAL",
    "COUNT_TASK_TERMINAL",
    "COUNT_TASK_UPLOAD_SUCCESSFUL",
    "COUNT_UPLOAD_COMLOG_SUCCESSFUL",
    "COUNT_INSERT_COMLOG_SUCCESSFUL",
    "COUNT_TASK_UPLOAD_UNSUCCESSFUL",
    "COUNT_UPLOAD_COMLOG_UNSUCCESSFUL",
    "COUNT_INSERT_COMLOG_UNSUCCESSFUL"
};

                // ใส่ Header
                for (int i = 0; i < summaryHeaders.Length; i++)
                {
                    var cell = summarySheet.Cells[1, i + 1];
                    cell.Value = summaryHeaders[i];

                    // Style header
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(191, 191, 191));
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // ใส่ข้อมูล
                summarySheet.Cells[2, 1].Value = latestDto.COUNT_ALL_TERMINAL;
                summarySheet.Cells[2, 2].Value = latestDto.COUNT_TERMINAL;
                summarySheet.Cells[2, 3].Value = latestDto.COUNT_TASK_TERMINAL;
                summarySheet.Cells[2, 4].Value = latestDto.COUNT_TASK_UPLOAD_SUCCESSFUL;
                summarySheet.Cells[2, 5].Value = latestDto.COUNT_UPLOAD_COMLOG_SUCCESSFUL;
                summarySheet.Cells[2, 6].Value = latestDto.COUNT_INSERT_COMLOG_SUCCESSFUL;
                summarySheet.Cells[2, 7].Value = latestDto.COUNT_TASK_UPLOAD_UNSUCCESSFUL;
                summarySheet.Cells[2, 8].Value = latestDto.COUNT_UPLOAD_COMLOG_UNSUCCESSFUL;
                summarySheet.Cells[2, 9].Value = latestDto.COUNT_INSERT_COMLOG_UNSUCCESSFUL;

                // ตกแต่ง cell ของข้อมูล
                for (int i = 1; i <= summaryHeaders.Length; i++)
                {
                    var valueCell = summarySheet.Cells[2, i];
                    valueCell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    valueCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    summarySheet.Column(i).AutoFit();
                    summarySheet.Column(i).Width += 5; // เพิ่ม spacing
                }



                package.SaveAs(new FileInfo(outputFilePath));
                FileSaveAsXlsxFormat = Path.GetFileName(outputFilePath);
            }
        }

        #endregion
    }


    public class ExcelUtilities_Inventory
    {
        #region  Local Variable

        CultureInfo _cultureEnInfo = new CultureInfo("en-US");

        #endregion

        #region Property
        public string PathDefaultTemplate { get; set; }

        public string FileSaveAsXlsxFormat { get; set; }

        #endregion

        #region Contractor
     

        public ExcelUtilities_Inventory()
        {
     
        }

        #endregion

        #region Function 
        public void GatewayOutput(List<InventoryMaintenanceModel> objData,string fromdata,string todate)
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

                FileInfo oTemplate = new FileInfo(Path.Combine(PathDefaultTemplate, "wwwroot\\RegulatorExcel\\InputTemplate\\Inventory.xlsx"));
                using (var oPackage = new ExcelPackage(oTemplate))
                {
                    //ExcelWorksheet excelWorksheet = oPackage.Workbook.Worksheets.First<ExcelWorksheet>();

                    var oWorkbook = oPackage.Workbook;
                    var excelWorksheet = oWorkbook.Worksheets["Sheet1"];

                    foreach (InventoryMaintenanceModel data in objData)
                    {



                        excelWorksheet.Cells[nStartRowData, 1].Value = data.DEVICE_ID;
                        excelWorksheet.Cells[nStartRowData, 2].Value = data.TERM_SEQ;
                        excelWorksheet.Cells[nStartRowData, 3].Value = data.TYPE_ID;
                        excelWorksheet.Cells[nStartRowData, 4].Value = data.TERM_ID;
                        excelWorksheet.Cells[nStartRowData, 5].Value = data.TERM_NAME;
                        excelWorksheet.Cells[nStartRowData, 6].Value = data.Connected;
                        excelWorksheet.Cells[nStartRowData, 7].Value = data.Status;
                        excelWorksheet.Cells[nStartRowData, 8].Value = data.COUNTER_CODE;
                        excelWorksheet.Cells[nStartRowData, 9].Value = data.ServiceType;
                        excelWorksheet.Cells[nStartRowData, 10].Value = data.TERM_LOCATION;
                        excelWorksheet.Cells[nStartRowData, 11].Value = data.LATITUDE;
                        excelWorksheet.Cells[nStartRowData, 12].Value = data.LONGITUDE;
                        excelWorksheet.Cells[nStartRowData, 13].Value = data.CONTROL_BY;
                        excelWorksheet.Cells[nStartRowData, 14].Value = data.PROVINCE;
                        excelWorksheet.Cells[nStartRowData, 15].Value = data.SERVICE_BEGINDATE;
                        excelWorksheet.Cells[nStartRowData, 16].Value = data.SERVICE_ENDDATE;
                        excelWorksheet.Cells[nStartRowData, 17].Value = data.VERSION_MASTER;
                        excelWorksheet.Cells[nStartRowData, 18].Value = data.VERSION;
                        excelWorksheet.Cells[nStartRowData, 19].Value = data.VERSION_AGENT;
                        nStartRowData++;
                        nSeq++;

                    }

                    oPackage.SaveAs(new FileInfo(Path.Combine(Path.Combine(PathDefaultTemplate.Replace("InputTemplate", "tempfiles"), "Inventory.xlsx"))));
                    FileSaveAsXlsxFormat = "Inventory.xlsx";
                }

            }
            catch (Exception ex)
            { throw ex; }
        }
        public void GatewayOutput(List<TicketManagement> objData)
        {
            int nStartRowData = 0;
            string strTermID = string.Empty;
            string strBranchName = string.Empty;
            string strLocation = string.Empty;
            string strProbName = string.Empty;
            int nSeq = 1;


            try
            {
                nStartRowData = 2;

                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                FileInfo oTemplate = new FileInfo(Path.Combine(PathDefaultTemplate, "wwwroot\\RegulatorExcel\\InputTemplate\\Ticket.xlsx"));
                using (var oPackage = new ExcelPackage(oTemplate))
                {
                    var oWorkbook = oPackage.Workbook;
                    var excelWorksheet = oWorkbook.Worksheets["Sheet1"];
                    foreach (TicketManagement data in objData)
                    {



                        excelWorksheet.Cells[nStartRowData, 1].Value = data.Open_Date;
                        excelWorksheet.Cells[nStartRowData, 2].Value = data.Appointment_Date;
                        excelWorksheet.Cells[nStartRowData, 3].Value = data.Closed_Repair_Date;
                        excelWorksheet.Cells[nStartRowData, 4].Value = data.Down_Time;
                        excelWorksheet.Cells[nStartRowData, 5].Value = data.Actual_Open_Date;
                        excelWorksheet.Cells[nStartRowData, 6].Value = data.Actual_Appointment_Date;
                        excelWorksheet.Cells[nStartRowData, 7].Value = data.Actual_Closed_Repair_Date;
                        excelWorksheet.Cells[nStartRowData, 8].Value = data.Actual_Down_Time;
                        excelWorksheet.Cells[nStartRowData, 9].Value = data.Status;
                        excelWorksheet.Cells[nStartRowData, 10].Value = data.TERM_ID;
                        excelWorksheet.Cells[nStartRowData, 11].Value = data.TERM_SEQ;
                        excelWorksheet.Cells[nStartRowData, 12].Value = data.TERM_NAME;
                        excelWorksheet.Cells[nStartRowData, 13].Value = data.Problem_Detail;
                        excelWorksheet.Cells[nStartRowData, 14].Value = data.Solving_Program;
                        excelWorksheet.Cells[nStartRowData, 15].Value = data.Service_Team;
                        excelWorksheet.Cells[nStartRowData, 16].Value = data.Contact_Name_Branch_CIT;
                        excelWorksheet.Cells[nStartRowData, 17].Value = data.Open_By;
                        excelWorksheet.Cells[nStartRowData, 18].Value = data.Remark;
                        excelWorksheet.Cells[nStartRowData, 19].Value = data.Job_No;
                        excelWorksheet.Cells[nStartRowData, 20].Value = data.Aservice_Status;
                        excelWorksheet.Cells[nStartRowData, 21].Value = data.Service_Type;
                        excelWorksheet.Cells[nStartRowData, 22].Value = data.Open_Name;
                        excelWorksheet.Cells[nStartRowData, 23].Value = data.Assign_By;
                        excelWorksheet.Cells[nStartRowData, 24].Value = data.Zone_Area;
                        excelWorksheet.Cells[nStartRowData, 25].Value = data.Main_Problem;
                        excelWorksheet.Cells[nStartRowData, 26].Value = data.Sub_Problem;
                        excelWorksheet.Cells[nStartRowData, 27].Value = data.Main_Solution;
                        excelWorksheet.Cells[nStartRowData, 28].Value = data.Sub_Solution;
                        excelWorksheet.Cells[nStartRowData, 29].Value = data.Part_of_use;
                        excelWorksheet.Cells[nStartRowData, 30].Value = data.TechSupport;
                        excelWorksheet.Cells[nStartRowData, 31].Value = data.CIT_Request;
                        excelWorksheet.Cells[nStartRowData, 32].Value = data.Terminal_Status;
                        nStartRowData++;
                        nSeq++;

                    }

                    oPackage.SaveAs(new FileInfo(Path.Combine(Path.Combine(PathDefaultTemplate.Replace("InputTemplate", "tempfiles"), "Ticket.xlsx"))));
                    FileSaveAsXlsxFormat = "Ticket.xlsx";
                }

            }
            catch (Exception ex)
            { throw ex; }
        }
        #endregion
    }
    public class ExcelUtilities_WhitelistReport
    {
        #region  Local Variable

        CultureInfo _cultureEnInfo = new CultureInfo("en-US");

        #endregion

        #region Property
        public string PathDefaultTemplate { get; set; }

        public string FileSaveAsXlsxFormat { get; set; }

        #endregion

        #region Contractor


        public ExcelUtilities_WhitelistReport()
        {
            
        }

        #endregion

        #region Function 
        public void GatewayOutput(List<WhitelistReportModel> objData,List<WhitelistPivotModel> pivotData)
        {
            int nStartRowData = 0;
            string strTermID = string.Empty;
            string strBranchName = string.Empty;
            string strLocation = string.Empty;
            string strProbName = string.Empty;
            int nStartPivotData = 0;

            try
            {
                nStartRowData = 2;
                nStartPivotData = 4;
                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                FileInfo oTemplate = new FileInfo(Path.Combine(PathDefaultTemplate, "wwwroot\\RegulatorExcel\\InputTemplate\\WhitelistReport.xlsx"));
                using (var oPackage = new ExcelPackage(oTemplate))
                {
                    //ExcelWorksheet excelWorksheet = oPackage.Workbook.Worksheets.First<ExcelWorksheet>();

                    var oWorkbook = oPackage.Workbook;
                    //var excelWorksheet = oWorkbook.Worksheets[0];
                    var excelWorksheet = oWorkbook.Worksheets["Summary_rawdata"];
                    var excelWorksheet_Pivot = oWorkbook.Worksheets["Pivot"];

                    foreach (WhitelistReportModel data in objData)
                    {
                        excelWorksheet.Cells[nStartRowData, 1].Value = data.eventdate;
                        excelWorksheet.Cells[nStartRowData, 2].Value = data.warn_type;
                        excelWorksheet.Cells[nStartRowData, 3].Value = data.severity_level;
                        excelWorksheet.Cells[nStartRowData, 4].Value = data.total;
                        nStartRowData++;

                    }
                    foreach (WhitelistPivotModel data in pivotData)
                    {
                        excelWorksheet_Pivot.Cells[nStartPivotData, 2].Value = data.warn_type;
                        excelWorksheet_Pivot.Cells[nStartPivotData, 3].Value = data.critical;
                        excelWorksheet_Pivot.Cells[nStartPivotData, 4].Value = data.high;
                        excelWorksheet_Pivot.Cells[nStartPivotData, 5].Value = data.low;
                        nStartPivotData++;

                    }
                    oPackage.SaveAs(new FileInfo(Path.Combine(Path.Combine(PathDefaultTemplate.Replace("InputTemplate", "tempfiles"), "WhitelistReport.xlsx"))));
                    FileSaveAsXlsxFormat = "WhitelistReport.xlsx";
                }

            }
            catch (Exception ex)
            { throw ex; }
        }

        #endregion
    }
    public class ExcelUtilities_TransactionSummary
    {
        #region  Local Variable

        CultureInfo _cultureEnInfo = new CultureInfo("en-US");

        #endregion

        #region Property
        public string PathDefaultTemplate { get; set; }

        public string FileSaveAsXlsxFormat { get; set; }

        #endregion

        #region Contractor


        public ExcelUtilities_TransactionSummary()
        {

        }

        #endregion

        #region Function 
        public void GatewayOutput(List<Dictionary<string, object>> objData)
        {
            int nStartRowData = 0;
            string strTermID = string.Empty;
            string strBranchName = string.Empty;
            string strLocation = string.Empty;
            string strProbName = string.Empty;
            int nStartPivotData = 0;

            try
            {
                nStartRowData = 2;
                nStartPivotData = 4;
                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                FileInfo oTemplate = new FileInfo(Path.Combine(PathDefaultTemplate, "wwwroot\\RegulatorExcel\\InputTemplate\\TransactionSummary.xlsx"));
                using (var oPackage = new ExcelPackage(oTemplate))
                {
                    //ExcelWorksheet excelWorksheet = oPackage.Workbook.Worksheets.First<ExcelWorksheet>();

                    var oWorkbook = oPackage.Workbook;
                    //var excelWorksheet = oWorkbook.Worksheets[0];
                    var excelWorksheet = oWorkbook.Worksheets["Sheet1"];

                    // Add headers
                    int column = 1;
                    foreach (var header in objData[0].Keys)
                    {
                        ExcelRange headerCell = excelWorksheet.Cells[1, column];
                        headerCell.Value = header.Replace("_", "");

                        // Apply styling to header cells
                        headerCell.Style.Font.Bold = true; // Make text bold
                        headerCell.Style.Font.Size = 13;
                        headerCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid; // Set fill pattern
                        headerCell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Orange); // Set background color
                        headerCell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin); // Add border around cell
                        column++;
                    }

                    // Add data
                    int row = 2;
                    foreach (var data in objData)
                    {
                        column = 1;
                        foreach (var value in data.Values)
                        {
                            excelWorksheet.Cells[row, column].Value = value;
                            column++;
                        }
                        row++;
                    }
                    for (int i = 1; i <= objData[0].Count; i++)
                    {
                        excelWorksheet.Column(i).AutoFit();

                    }
                    oPackage.SaveAs(new FileInfo(Path.Combine(Path.Combine(PathDefaultTemplate.Replace("InputTemplate", "tempfiles"), "TransactionSummary.xlsx"))));
                    FileSaveAsXlsxFormat = "TransactionSummary.xlsx";
                }

            }
            catch (Exception ex)
            { throw ex; }
        }

        #endregion
    }

    public class ExcelUtilities_LastTransaction
    {
        #region  Local Variable
        CultureInfo _cultureEnInfo = new CultureInfo("en-US");

        #endregion

        #region Property
        public string PathDefaultTemplate { get; set; }

        public string FileSaveAsXlsxFormat { get; set; }

        #endregion
        #region Contractor

        #endregion
        #region Function 
        public void GatewayOutput(List<LastTransactionModel> objData)
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

                FileInfo oTemplate = new FileInfo(Path.Combine(PathDefaultTemplate, "wwwroot\\RegulatorExcel\\InputTemplate\\LastTransaction.xlsx"));
                using (var oPackage = new ExcelPackage(oTemplate))
                {
                    //ExcelWorksheet excelWorksheet = oPackage.Workbook.Worksheets.First<ExcelWorksheet>();

                    var oWorkbook = oPackage.Workbook;
                    //var excelWorksheet = oWorkbook.Worksheets[0];
                    var excelWorksheet = oWorkbook.Worksheets["Sheet1"];
                    //excelWorksheet.Name = "Regulator";

                    excelWorksheet.Cells[2, 1].Value = "Last Transaction Report";
                    excelWorksheet.Cells[4, 7].Value = DateTime.Now.ToString("dd/MM/yyyy", _cultureEnInfo);
                    excelWorksheet.Cells[5, 7].Value = DateTime.Now.ToString("HH:mm:ss", _cultureEnInfo);

                    foreach (LastTransactionModel data in objData)
                    {



                        excelWorksheet.Cells[nStartRowData, 1].Value = data.no;
                        excelWorksheet.Cells[nStartRowData, 2].Value = data.term_seq;
                        excelWorksheet.Cells[nStartRowData, 3].Value = data.term_id;
                        excelWorksheet.Cells[nStartRowData, 4].Value = data.term_name;
                        excelWorksheet.Cells[nStartRowData, 5].Value = data.last_transaction;
                        excelWorksheet.Cells[nStartRowData, 6].Value = data.last_transaction_success;


                        nStartRowData++;
                        nSeq++;

                    }

                    oPackage.SaveAs(new FileInfo(Path.Combine(Path.Combine(PathDefaultTemplate.Replace("InputTemplate", "tempfiles"), "LastTransaction.xlsx"))));
                    FileSaveAsXlsxFormat = "LastTransaction.xlsx";
                }

            }
            catch (Exception ex)
            { throw ex; }
        }
        #endregion
    }
    public class ExcelUtilities_CardRetain
    {
        #region  Local Variable
        CultureInfo _cultureEnInfo = new CultureInfo("en-US");

        #endregion

        #region Property
        public string PathDefaultTemplate { get; set; }

        public string FileSaveAsXlsxFormat { get; set; }

        #endregion

        #region Function 
        public void GatewayOutput(List<CardRetainModel> objData)
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

                FileInfo oTemplate = new FileInfo(Path.Combine(PathDefaultTemplate, "wwwroot\\RegulatorExcel\\InputTemplate\\CardRetain.xlsx"));
                using (var oPackage = new ExcelPackage(oTemplate))
                {
                    //ExcelWorksheet excelWorksheet = oPackage.Workbook.Worksheets.First<ExcelWorksheet>();

                    var oWorkbook = oPackage.Workbook;
                    //var excelWorksheet = oWorkbook.Worksheets[0];
                    var excelWorksheet = oWorkbook.Worksheets["Sheet1"];
                    //excelWorksheet.Name = "Regulator";

                    excelWorksheet.Cells[2, 1].Value = "Card Retain Report";
                    excelWorksheet.Cells[4, 7].Value = DateTime.Now.ToString("dd/MM/yyyy", _cultureEnInfo);
                    excelWorksheet.Cells[5, 7].Value = DateTime.Now.ToString("HH:mm:ss", _cultureEnInfo);

                    foreach (CardRetainModel data in objData)
                    {



                        excelWorksheet.Cells[nStartRowData, 1].Value = data.no;
                        excelWorksheet.Cells[nStartRowData, 2].Value = data.term_seq;
                        excelWorksheet.Cells[nStartRowData, 3].Value = data.term_id;
                        excelWorksheet.Cells[nStartRowData, 4].Value = data.term_name;
                        excelWorksheet.Cells[nStartRowData, 5].Value = data.card_number;
                        excelWorksheet.Cells[nStartRowData, 6].Value = data.trxdatetime;


                        nStartRowData++;
                        nSeq++;

                    }

                    oPackage.SaveAs(new FileInfo(Path.Combine(Path.Combine(PathDefaultTemplate.Replace("InputTemplate", "tempfiles"), "CardRetain.xlsx"))));
                    FileSaveAsXlsxFormat = "CardRetain.xlsx";
                }

            }
            catch (Exception ex)
            { throw ex; }
        }
        #endregion
    }
    public class ExcelUtilities_EJReportMonitor
    {
        #region Local Variable
        CultureInfo _cultureEnInfo = new CultureInfo("en-US");

        #endregion

        #region Property
        public string PathDefaultTemplate { get; set; }

        public string FileSaveAsXlsxFormat { get; set; }

        #endregion

        #region Function
        public void GatewayOutput(List<EJReportMonitorModel> objData)
        {
            int nStartRowData = 0;
            int nSeq = 1;

            try
            {
                nStartRowData = 3; 

                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                // Load the template
                FileInfo oTemplate = new FileInfo(Path.Combine(PathDefaultTemplate, "wwwroot\\RegulatorExcel\\InputTemplate\\EJLogMonitor.xlsx"));
                using (var oPackage = new ExcelPackage(oTemplate))
                {
                    var oWorkbook = oPackage.Workbook;
                    var excelWorksheet = oWorkbook.Worksheets["Sheet1"]; // Assuming the sheet is named "Sheet1"

                    // Add report header details
                    //excelWorksheet.Cells[2, 1].Value = "EJ Log Monitoring Report";
                    //excelWorksheet.Cells[4, 7].Value = DateTime.Now.ToString("dd/MM/yyyy", _cultureEnInfo); // Current date
                    //excelWorksheet.Cells[5, 7].Value = DateTime.Now.ToString("HH:mm:ss", _cultureEnInfo); // Current time

                    // Populate the data
                    foreach (EJReportMonitorModel data in objData)
                    {
                        excelWorksheet.Cells[nStartRowData, 1].Value = nSeq++;
                        excelWorksheet.Cells[nStartRowData, 2].Value = data.TerminalId;
                        excelWorksheet.Cells[nStartRowData, 3].Value = data.TermName;
                        excelWorksheet.Cells[nStartRowData, 4].Value = data.TermSeq;                                              
                        excelWorksheet.Cells[nStartRowData, 5].Value = DateTime.TryParse(data.Date, out DateTime parsedDate)
                        ? parsedDate.ToString("yyyy-MM-dd")
                        : data.Date;
                        excelWorksheet.Cells[nStartRowData, 6].Value = data.TransactionHistory; 
                        excelWorksheet.Cells[nStartRowData, 7].Value = data.TransactionMonitoring;
                        excelWorksheet.Cells[nStartRowData, 8].Value = data.Diff;
                        excelWorksheet.Cells[nStartRowData, 9].Value = data.Status;
                        for (int col = 2; col <= 10; col++)
                        {
                            excelWorksheet.Cells[nStartRowData, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            excelWorksheet.Cells[nStartRowData, col].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        }
                        nStartRowData++;
                    }
                    for (int col = 2; col <= 10; col++)
                    {
                        excelWorksheet.Column(col).AutoFit();
                    }
                    // Save the updated Excel file
                    string savePath = Path.Combine(PathDefaultTemplate.Replace("InputTemplate", "tempfiles"), "EJLogMonitor.xlsx");
                    oPackage.SaveAs(new FileInfo(savePath));
                    FileSaveAsXlsxFormat = "EJLogMonitor.xlsx";
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating Excel report: " + ex.Message, ex);
            }
        }
        #endregion
    }

    public class ExcelUtilities_Transaction
    {
        #region  Local Variable
        CultureInfo _cultureEnInfo = new CultureInfo("en-US");

        #endregion

        #region Property
        public string PathDefaultTemplate { get; set; }

        public string FileSaveAsXlsxFormat { get; set; }

        #endregion
        #region Contractor

        #endregion
        #region Function 
        public void GatewayOutput(List<TransactionModel> objData)
        {
            int nStartRowData = 0;
            string strTermID = string.Empty;
            string strBranchName = string.Empty;
            string strLocation = string.Empty;
            string strProbName = string.Empty;
            int nSeq = 1;


            try
            {
                nStartRowData = 7;

                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                FileInfo oTemplate = new FileInfo(Path.Combine(PathDefaultTemplate, "wwwroot\\RegulatorExcel\\InputTemplate\\Transaction.xlsx"));
                using (var oPackage = new ExcelPackage(oTemplate))
                {
                    //ExcelWorksheet excelWorksheet = oPackage.Workbook.Worksheets.First<ExcelWorksheet>();

                    var oWorkbook = oPackage.Workbook;
                    //var excelWorksheet = oWorkbook.Worksheets[0];
                    var excelWorksheet = oWorkbook.Worksheets["Sheet1"];
                    //excelWorksheet.Name = "Regulator";



                    foreach (TransactionModel data in objData)
                    {
                        excelWorksheet.Cells[nStartRowData, 1].Value = data.no;
                        excelWorksheet.Cells[nStartRowData, 2].Value = data.seq;
                        excelWorksheet.Cells[nStartRowData, 3].Value = data.trx_datetime;
                        excelWorksheet.Cells[nStartRowData, 4].Value = data.trx_type;
                        excelWorksheet.Cells[nStartRowData, 5].Value = data.bankcode;
                        excelWorksheet.Cells[nStartRowData, 6].Value = data.s_other;
                        excelWorksheet.Cells[nStartRowData, 7].Value = data.pan_no;
                        excelWorksheet.Cells[nStartRowData, 8].Value = data.fr_accno;
                        excelWorksheet.Cells[nStartRowData, 9].Value = data.to_accno;
                        excelWorksheet.Cells[nStartRowData, 10].Value = data.trx_status;
                        excelWorksheet.Cells[nStartRowData, 11].Value = data.amt1;
                        excelWorksheet.Cells[nStartRowData, 12].Value = data.fee_amt1;
                        excelWorksheet.Cells[nStartRowData, 13].Value = data.retract_amt1;
                        excelWorksheet.Cells[nStartRowData, 14].Value = data.billcounter;
                        excelWorksheet.Cells[nStartRowData, 15].Value = data.rc;
                        nStartRowData++;
                        nSeq++;

                    }
                    excelWorksheet.Cells[2, 8].Value = bankname_ej;
                    excelWorksheet.Cells[3, 3].Value = fromtodate_ej;
                    excelWorksheet.Cells[3, 8].Value = sortby_ej;
                    excelWorksheet.Cells[3, 11].Value = orderby_ej;
                    excelWorksheet.Cells[4, 3].Value = term_ej;
                    excelWorksheet.Cells[4, 8].Value = branchname_ej;
                    excelWorksheet.Cells[4, 11].Value = status_ej;
                    excelWorksheet.Cells[5, 3].Value = totaltransaction_ej;
                    excelWorksheet.Cells[5, 8].Value = trxtype_ej;
                    excelWorksheet.Cells[5, 11].Value = rc_ej;
                    oPackage.SaveAs(new FileInfo(Path.Combine(Path.Combine(PathDefaultTemplate.Replace("InputTemplate", "tempfiles"), "Transaction.xlsx"))));
                    FileSaveAsXlsxFormat = "Transaction.xlsx";
                }

            }
            catch (Exception ex)
            { throw ex; }
        }
        #endregion
    }
    public class ExcelUtilities_BalancingReport
    {
        #region  Local Variable
        CultureInfo _cultureEnInfo = new CultureInfo("en-US");

        #endregion

        #region Property
        public string PathDefaultTemplate { get; set; }

        public string FileSaveAsXlsxFormat { get; set; }

        #endregion
        #region Contractor

        #endregion
        #region Function 
        public void GatewayOutput(List<BalancingReportModel> objData)
        {
            int nStartRowData = 0;
            string strTermID = string.Empty;
            string strBranchName = string.Empty;
            string strLocation = string.Empty;
            string strProbName = string.Empty;
            int nSeq = 1;


            try
            {
                nStartRowData = 7;

                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                FileInfo oTemplate = new FileInfo(Path.Combine(PathDefaultTemplate, "wwwroot\\RegulatorExcel\\InputTemplate\\BalancingReport.xlsx"));
                using (var oPackage = new ExcelPackage(oTemplate))
                {
                    //ExcelWorksheet excelWorksheet = oPackage.Workbook.Worksheets.First<ExcelWorksheet>();

                    var oWorkbook = oPackage.Workbook;
                    //var excelWorksheet = oWorkbook.Worksheets[0];
                    var excelWorksheet = oWorkbook.Worksheets["Sheet1"];
                    //excelWorksheet.Name = "Regulator";


                    foreach (BalancingReportModel data in objData)
                    {



                        excelWorksheet.Cells[nStartRowData, 1].Value = nSeq;
                        excelWorksheet.Cells[nStartRowData, 2].Value = data.term_id;
                        excelWorksheet.Cells[nStartRowData, 3].Value = data.term_name;
                        excelWorksheet.Cells[nStartRowData, 4].Value = data.term_seq;
                        excelWorksheet.Cells[nStartRowData, 5].Value = data.transationdate;
                        excelWorksheet.Cells[nStartRowData, 6].Value = data.c1_inc;
                        excelWorksheet.Cells[nStartRowData, 7].Value = data.c2_inc;
                        excelWorksheet.Cells[nStartRowData, 8].Value = data.c3_inc;
                        excelWorksheet.Cells[nStartRowData, 9].Value = data.c1_dep;
                        excelWorksheet.Cells[nStartRowData, 10].Value = data.c2_dep;
                        excelWorksheet.Cells[nStartRowData, 11].Value = data.c3_dep;
                        excelWorksheet.Cells[nStartRowData, 12].Value = data.c1_out;
                        excelWorksheet.Cells[nStartRowData, 13].Value = data.c2_out;
                        excelWorksheet.Cells[nStartRowData, 14].Value = data.c3_out;
                        excelWorksheet.Cells[nStartRowData, 15].Value = data.c1_end;
                        excelWorksheet.Cells[nStartRowData, 16].Value = data.c2_end;
                        excelWorksheet.Cells[nStartRowData, 17].Value = data.c3_end;


                        nStartRowData++;
                        nSeq++;

                    }

                    oPackage.SaveAs(new FileInfo(Path.Combine(Path.Combine(PathDefaultTemplate.Replace("InputTemplate", "tempfiles"), "BalancingReport.xlsx"))));
                    FileSaveAsXlsxFormat = "BalancingReport.xlsx";
                }

            }
            catch (Exception ex)
            { throw ex; }
        }
        #endregion
    }
    public class ExcelUtilities_HardwareReport
    {
        #region  Local Variable
        CultureInfo _cultureEnInfo = new CultureInfo("en-US");

        #endregion

        #region Property
        public string PathDefaultTemplate { get; set; }

        public string FileSaveAsXlsxFormat { get; set; }

        #endregion
        #region Contractor

        #endregion
        #region Function 
        public void GatewayOutput(List<HardwareReportWebModel> objData, int total, string date, string terminal)
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

                FileInfo oTemplate = new FileInfo(Path.Combine(PathDefaultTemplate, "wwwroot\\RegulatorExcel\\InputTemplate\\HardwareReport.xlsx"));
                using (var oPackage = new ExcelPackage(oTemplate))
                {
                    var oWorkbook = oPackage.Workbook;
                    var excelWorksheet = oWorkbook.Worksheets["Sheet1"];
                    excelWorksheet.Cells[5, 3].Value = date;
                    excelWorksheet.Cells[6, 3].Value = terminal;
                    excelWorksheet.Cells[6, 4].Value = "จำนวนทั้งหมด : " + total;
                    foreach (HardwareReportWebModel data in objData)
                    {

                        excelWorksheet.Cells[nStartRowData, 2].Value = nSeq;
                        excelWorksheet.Cells[nStartRowData, 3].Value = data.problem_name;
                        excelWorksheet.Cells[nStartRowData, 4].Value = data.problem_count;

                        nStartRowData++;
                        nSeq++;

                    }

                    oPackage.SaveAs(new FileInfo(Path.Combine(Path.Combine(PathDefaultTemplate.Replace("InputTemplate", "tempfiles"), "HardwareReport.xlsx"))));
                    FileSaveAsXlsxFormat = "HardwareReport.xlsx";
                }

            }
            catch (Exception ex)
            { throw ex; }
        }
        #endregion
    }

}
