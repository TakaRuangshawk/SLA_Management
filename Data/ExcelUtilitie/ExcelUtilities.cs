
using OfficeOpenXml;
using SLA_Management.Models;
using SLA_Management.Models.OperationModel;
using SLA_Management.Models.ReportModel;
using SLA_Management.Models.TermProbModel;
using System.Globalization;
using static SLA_Management.Controllers.ManagementController;
using static SLA_Management.Controllers.OperationController;
using static SLA_Management.Controllers.ReportController;

namespace SLA_Management.Data.ExcelUtilitie
{

    using OfficeOpenXml;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    public class ExcelUtilities_CavityNoteMonitor
    {
        #region Local Variable

        cavity_seek param = null;
        private readonly CultureInfo _cultureEnInfo = new CultureInfo("en-US");

        #endregion

        #region Property

        public string PathDefaultTemplate { get; set; }

        public string FileSaveAsXlsxFormat { get; set; }

        #endregion

        #region Constructor

        public ExcelUtilities_CavityNoteMonitor(cavity_seek paramTemp)
        {
            param = paramTemp;
        }

        public ExcelUtilities_CavityNoteMonitor()
        {
            param = new cavity_seek();
        }

        #endregion

        #region Function

        /// <summary>
        /// สร้างไฟล์ Excel สำหรับรายงาน Cavity Note Monitor
        /// ใช้ template: CavityNoteMonitor.xlsx
        /// </summary>
        public void GenExcelFileCavityMonitor(List<CavityMonitorModel> objData)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                // สร้าง Excel ใหม่
                using (var package = new ExcelPackage())
                {
                    var ws = package.Workbook.Worksheets.Add("CavityNoteMonitor");

                    int row = 1;

                    // ===== HEADER REPORT =====
                    ws.Cells[row, 1].Value = "Cavity Note Monitor Report";
                    ws.Cells[row, 1, row, 9].Merge = true;
                    ws.Cells[row, 1].Style.Font.Bold = true;
                    ws.Cells[row, 1].Style.Font.Size = 16;
                    row++;

                    ws.Cells[row, 1].Value = "Generated:";
                    ws.Cells[row, 2].Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    row++;

                    row++;

                    // ===== TABLE HEADER =====
                    string[] headers = new string[]
                    {
                "No.",
                "Serial No.",
                "Terminal ID",
                "Terminal Name",
                "Terminal Type",
                "Cavity Note",
                "Has 4199",
                "NV Log Time",
                "NV Version",
                "Main Version"
                    };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        ws.Cells[row, i + 1].Value = headers[i];
                        ws.Cells[row, i + 1].Style.Font.Bold = true;
                        ws.Cells[row, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws.Cells[row, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        ws.Cells[row, i + 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }

                    row++;

                    // ===== WRITE DATA =====
                    foreach (var d in objData)
                    {
                        ws.Cells[row, 1].Value = d.no;
                        ws.Cells[row, 2].Value = d.term_seq;
                        ws.Cells[row, 3].Value = d.term_id;
                        ws.Cells[row, 4].Value = d.term_name;
                        ws.Cells[row, 5].Value = d.term_type;
                        ws.Cells[row, 6].Value = (d.cavity_note == -1 ? "-" : d.cavity_note.ToString());

                        ws.Cells[row, 7].Value = d.xdc_has_4199 ? "Yes" : "No";
                        ws.Cells[row, 8].Value = d.nv_log_time?.ToString("yyyy-MM-dd HH:mm:ss");
                        ws.Cells[row, 9].Value = d.nv_version;
                        ws.Cells[row, 10].Value = d.main_version;

                        row++;
                    }

                    // AutoFit columns
                    ws.Cells[1, 1, row, 9].AutoFitColumns();

                    // ===== SAVE FILE =====
                    string saveFolder = PathDefaultTemplate.Replace("InputTemplate", "tempfiles");
                    if (!Directory.Exists(saveFolder))
                        Directory.CreateDirectory(saveFolder);

                    string savePath = Path.Combine(saveFolder, "CavityNoteMonitor.xlsx");
                    FileSaveAsXlsxFormat = "CavityNoteMonitor.xlsx";

                    package.SaveAs(new FileInfo(savePath));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }

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
                        excelWorksheet.Cells[nStartRowData, 7].Value = data.ProbName;



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
    public class ExcelUtilities_AdminCardMonitor
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
        public void GatewayOutput(List<AdminCardModel> objData)
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

                FileInfo oTemplate = new FileInfo(Path.Combine(PathDefaultTemplate, "wwwroot\\RegulatorExcel\\InputTemplate\\AdminCardMonitor.xlsx"));
                using (var oPackage = new ExcelPackage(oTemplate))
                {
                    //ExcelWorksheet excelWorksheet = oPackage.Workbook.Worksheets.First<ExcelWorksheet>();

                    var oWorkbook = oPackage.Workbook;
                    //var excelWorksheet = oWorkbook.Worksheets[0];
                    var excelWorksheet = oWorkbook.Worksheets["Sheet1"];
                    //excelWorksheet.Name = "Regulator";

                    excelWorksheet.Cells[2, 1].Value = "Admin Card Report";
                    excelWorksheet.Cells[4, 7].Value = DateTime.Now.ToString("dd/MM/yyyy", _cultureEnInfo);
                    excelWorksheet.Cells[5, 7].Value = DateTime.Now.ToString("HH:mm:ss", _cultureEnInfo);

                    foreach (AdminCardModel data in objData)
                    {



                        excelWorksheet.Cells[nStartRowData, 1].Value = data.id;
                        excelWorksheet.Cells[nStartRowData, 2].Value = data.term_seq;
                        excelWorksheet.Cells[nStartRowData, 3].Value = data.term_id;
                        excelWorksheet.Cells[nStartRowData, 4].Value = data.term_name;
                        excelWorksheet.Cells[nStartRowData, 5].Value = data.admin_card_master;
                        excelWorksheet.Cells[nStartRowData, 6].Value = data.admin_password_digits;
                        excelWorksheet.Cells[nStartRowData, 7].Value = data.status;
                        excelWorksheet.Cells[nStartRowData, 8].Value = data.update_date;

                        nStartRowData++;
                        nSeq++;

                    }

                    oPackage.SaveAs(new FileInfo(Path.Combine(Path.Combine(PathDefaultTemplate.Replace("InputTemplate", "tempfiles"), "AdminCardMonitor.xlsx"))));
                    FileSaveAsXlsxFormat = "AdminCardMonitor.xlsx";
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
        #region Contractor

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
        public void GatewayOutput(List<HardwareReportWebModel> objData,int total,string date,string terminal)
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
        public void GatewayOutput(List<InventoryMaintenanceModel> objData, string fromdata, string todate)
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

}