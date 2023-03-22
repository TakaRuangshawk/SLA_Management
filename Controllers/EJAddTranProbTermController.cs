using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using PagedList;
using SLA_Management.Data.TermProbDB;
using SLA_Management.Models.TermProbModel;
using System.Data;
using System.Data.Common;
using System.Globalization;


namespace SLA_Management.Controllers
{
    public class EJAddTranProbTermController : Controller
    {

        #region Local Variable
        CultureInfo _cultureEnInfo = new CultureInfo("en-US");
        SqlCommand com = new SqlCommand();      
        List<ej_trandeviceprob> ejLog_dataList = new List<ej_trandeviceprob>();
        ConnectDB_TermProb connectDB_TermProb = new ConnectDB_TermProb();

        #endregion

        #region Initialize Variable



        //public EJAddTranProbTermController(IConfiguration myConfiguration)
        //{
        //    _myConfiguration = myConfiguration;
        //    con = new ConnectSQL_Server(_myConfiguration["ConnectionStrings:DefaultConnection"]);

        //    gatewayTable = _myConfiguration["Database:GatewayTable"];

        //    secondDatabase = _myConfiguration["Database:SecondDatabase"];

        //    secondTable = _myConfiguration["Database:SecondTable"];

        //    symmetricKey = _myConfiguration["Database:SymmetricKey"];

        //    certification = _myConfiguration["Database:Certification"];

        //    startQuery = "OPEN SYMMETRIC KEY " + symmetricKey + " DECRYPTION BY CERTIFICATE " + certification + "  SELECT ID,SeqNo,CONVERT(nvarchar, DecryptByKey(Citizen_Id)) AS 'Citizen_Id_Decrypt',CONVERT (nvarchar , DecryptByKey(Customer_title)) AS 'Customer_title' ,CONVERT(nvarchar, DecryptByKey(Customer_first_name)) AS 'Customer_first_name',CONVERT(nvarchar, DecryptByKey(Customer_middle_name)) AS 'Customer_middle_name',CONVERT(nvarchar, DecryptByKey(Customer_last_name)) AS 'Customer_last_name',CONVERT(nvarchar, DecryptByKey(English_customer_title)) AS 'English_customer_title',CONVERT(nvarchar, DecryptByKey(English_first_name)) AS 'English_first_name',CONVERT(nvarchar, DecryptByKey(English_middle_name)) AS 'English_middle_name',CONVERT(nvarchar, DecryptByKey(English_last_name)) AS 'English_last_name',CONVERT(datetime, CONVERT(nvarchar, DecryptByKey(Date_of_birth))) AS 'Date_of_birth',CONVERT(nvarchar, DecryptByKey(AcctNoFrom)) AS 'AcctNoFrom',CONVERT(nvarchar, DecryptByKey(AcctNoTo)) AS 'AcctNoTo',CONVERT(nvarchar, DecryptByKey(AcctName)) AS 'AcctName', TransType,TransDateTime,TerminalNo,Amount,UpdateStatus,ErrorDescription,device_info.TERM_NAME FROM " + gatewayTable + " left join " + secondDatabase + ".dbo." + secondTable + " On TERM_ID = TerminalNo ";

        //    loginSub = new LoginController(myConfiguration);
        //}

        #endregion


        public ActionResult EJAddTranProbTermAction(string cmdButton, string TermID, string FrDate, string ToDate, string FrTime, string ToTime
            , string currTID, string currFr, string currTo, string currFrTime, string currToTime, string lstPageSize
            , string ddlProbMaster, string currProbMaster, string MessErrKeyWord, string currMessErrKeyWord
            , string currPageSize, int? page)
        {

            List<ej_trandeviceprob> recordset = new List<ej_trandeviceprob>();
            List<ProblemMaster> ProdMasData = new List<ProblemMaster>();
            ej_trandada_seek param = new ej_trandada_seek();

            ViewBag.maxRows = "5";

            //TermID = "T091B030B119G262";
            //FrDate = "2023-03-20";
            //ToDate = "2023-03-20";
            //MessErrKeyWord = "";

            int pageNum = 1;
            try
            {

                ProdMasData = connectDB_TermProb.GetMasterSysErrorWord();
                if (ProdMasData.Count > 0)
                {
                    ViewBag.ProbMaster = "";
                    foreach (ProblemMaster obj in ProdMasData)
                    {
                        if (ViewBag.ProbMaster == "")
                            ViewBag.ProbMaster = obj.ProblemCode + "," + obj.ProblemName;
                        else
                            ViewBag.ProbMaster += "|" + obj.ProblemCode + "," + obj.ProblemName;
                    }
                }

                if (cmdButton == "Clear")
                    return RedirectToAction("EJAddTranProbTermAction");

                

                if (null == TermID && null == FrDate && null == ToDate && null == page)
                {
                    //FrDate = DateTime.Now.ToString("yyyy-MM-dd", _cultureEnInfo);
                    FrDate = DateTime.Now.ToString("yyyy-MM-dd", _cultureEnInfo);
                    ToDate = DateTime.Now.ToString("yyyy-MM-dd", _cultureEnInfo);
                    page = 1;
                }
                else
                {
                    // Return temp value back to it own variabl
                    FrDate = (FrDate ?? currFr);
                    ToDate = (ToDate ?? currTo);
                    FrTime = (FrTime ?? currFrTime);
                    ToTime = (ToTime ?? currToTime);
                    TermID = (TermID ?? currTID);
                    ddlProbMaster = (ddlProbMaster ?? currProbMaster);
                    MessErrKeyWord = (MessErrKeyWord ?? currMessErrKeyWord);
                }

                ViewBag.CurrentTID = (TermID ?? currTID);
                ViewBag.CurrentFr = (FrDate ?? currFr);
                ViewBag.CurrentTo = (ToDate ?? currTo);
                ViewBag.CurrentPageSize = (lstPageSize ?? currPageSize);
                ViewBag.CurrentProbMaster = ddlProbMaster == null ? currProbMaster : ddlProbMaster;
                ViewBag.CurrentMessErrKeyWord = MessErrKeyWord == null ? currMessErrKeyWord : MessErrKeyWord;

                long recCnt = 0;

                if (null == TermID)
                    param.TERMID = currTID == null ? "" : currTID;
                else
                    param.TERMID = TermID == null ? "" : TermID;

                if ((FrDate == null && currFr == null) && (FrDate == "" && currFr == ""))
                {
                    param.FRDATE = DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00";
                }
                else
                {
                    if ((FrTime == "" && currFrTime == "") || (FrTime == null && currFrTime == null) ||
                        (FrTime == null && currFrTime == "") || (FrTime == "" && currFrTime == null))
                        param.FRDATE = FrDate + " 00:00:00";
                    else
                        param.FRDATE = FrDate + " " + FrTime;
                }

                if ((ToDate == null && currTo == null) && (ToDate == "" && currTo == ""))
                {
                    param.TODATE = DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59";
                }
                else
                {
                    if ((ToTime == "" && currToTime == "") || (ToTime == null && currToTime == null) ||
                        (ToTime == null && currToTime == "") || (ToTime == "" && currToTime == null))
                        param.TODATE = ToDate + " 23:59:59";
                    else
                        param.TODATE = ToDate + " " + ToTime;
                }

                if (ddlProbMaster == null && currProbMaster == null)
                    param.PROBNAME = "All";
                else
                    param.PROBNAME = ddlProbMaster == null ? currProbMaster : ddlProbMaster;

                if (MessErrKeyWord == null && currMessErrKeyWord == null)
                    param.PROBKEYWORD = "";
                else
                    param.PROBKEYWORD = MessErrKeyWord == null ? currMessErrKeyWord : MessErrKeyWord;

                if (null != lstPageSize || null != currPageSize)
                {
                    param.PAGESIZE = String.IsNullOrEmpty(lstPageSize) == true ?
                        int.Parse(currPageSize) : int.Parse(lstPageSize);
                }
                else
                    param.PAGESIZE = 20;

                param.MONTHPERIOD = "";
                param.YEARPERIOD = "";
                param.TRXTYPE = "";



                if (MessErrKeyWord != null && MessErrKeyWord != "")
                {
                    recordset = connectDB_TermProb.GetErrorTermDeviceKWEJLog_Database(param, 1, 0);
                }
                //else
                //{ recordset = logicLogSeek.GetErrorTermDeviceEJLog(param, 1, 0); }

                if (null == recordset || recordset.Count <= 0)
                    ViewBag.NoData = "true";
                else
                    recCnt = recordset.Count;

                if (recCnt > 0)
                {
                    ViewBag.Records = String.Format("{0:#,##0}", recCnt.ToString());
                }
                else
                    ViewBag.Records = "0";

                pageNum = (page ?? 1);

            }
            catch (Exception ex)
            { }
            return View(recordset.ToPagedList(pageNum, (int)param.PAGESIZE));
        }

        public List<ej_trandeviceprob> GetErrorTermDeviceKWEJLog(ej_trandada_seek model, int startRowIndex, int maximumRows)
        {
            List<ej_trandeviceprob> recordset = null;

            recordset = connectDB_TermProb.GetErrorTermDeviceKWEJLog_Database(model, GetPageIndex(startRowIndex, maximumRows), maximumRows);

            return recordset;
        }

        private int GetPageIndex(int startRowIndex, int maximumRows)
        {
            if (maximumRows <= 0)
                return 0;
            else
                return (int)Math.Floor((double)startRowIndex / (double)maximumRows);
        }

        private List<ej_trandeviceprob> GetErrorTermDeviceEJLogCollectionFromReader(IDataReader reader)
        {
            int _seqNo = 1;
            List<ej_trandeviceprob> recordlst = new List<ej_trandeviceprob>();
            while (reader.Read())
            {
                recordlst.Add(GetErrorTermDeviceEJLogFromReader(reader, _seqNo));
                _seqNo++;
            }

            return recordlst;
        }

        private ej_trandeviceprob GetErrorTermDeviceEJLogFromReader(IDataReader reader, int pSeqNo)
        {
            ej_trandeviceprob record = new ej_trandeviceprob();

            record.Seqno = pSeqNo;
            record.TerminalID = reader["terminalid"].ToString();
            record.BranchName = reader["branchname"].ToString();
            record.Location = reader["locationbranch"].ToString();
            record.ProbName = reader["probname"].ToString();
            record.Remark = reader["remark"].ToString();
            record.TransactionDate = Convert.ToDateTime(reader["trxdatetime"]);

            return record;
        }
       









    }

}
