using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace SLA_Management.Data.TermProbDB
{
    public class DBService
    {

        #region Property
        public string ErrorMessage { get; set; }

        public string ErrorMessDBIns
        {
            get { return _strErrDB; }
            set { _strErrDB = value; }
        }

        #endregion

        #region Local Variable

        private DataAccess _objDb = new DataAccess();
        private string _strErrDB = string.Empty;

        #endregion

        #region Public Functions
        public DataTable GetClientData()
        {
            DataTable _dt = new DataTable();
            string _sql = string.Empty;
            try
            {
                //_sql = "Select * From ejlog_terminal order by terminalid";
                _sql = "Select TERM_ID as terminalid From fv_device_info order by TERM_ID";
                _dt = _objDb.GetDtDataNoneParam(_sql);
                return _dt;
            }
            catch (Exception ex)
            { throw ex; }
        }

        public DataTable GetAllMasterProblem()
        {
            DataTable _dt = new DataTable();
            string _sql = string.Empty;
            try
            {
                _sql = "Select * From ejlog_problemmascode where status = '1'";
                _dt = _objDb.GetDtDataNoneParam(_sql);
                return _dt;
            }
            catch (Exception ex)
            { throw ex; }
        }

        public bool InsertTerminalDeviceErrorData(string pTerminalid, string pProbCode, string pTrxdatetime, string pStatus, string ErrDesc, string dtEndTimeErrCode13,
            long dtTimeErrCode13)
        {
            bool _result = false;
            string sqlins = string.Empty;
            int nPosStar = 0;
            string strTemRem = string.Empty;
            try
            {
                nPosStar = ErrDesc.IndexOf(" ");
                strTemRem = ErrDesc.Substring(nPosStar, ErrDesc.Length - nPosStar);
                strTemRem = strTemRem.Trim();
                sqlins = "Insert into ejlog_devicetermprob_bak (terminalid,probcode,trxdatetime,status,createdate,remark) ";
                sqlins += "Values ('" + pTerminalid + "','" + pProbCode + "','" + pTrxdatetime + "','" + pStatus + "',now(),'" + strTemRem + "')";

                _result = _objDb.ExecuteQueryNoneParam(sqlins);
                ErrorMessDBIns = _objDb.ErrorMessDB;
            }
            catch (Exception ex)
            {
               
                ErrorMessDBIns = _objDb.ErrorMessDB;
            }
            return _result;
        }

        //public bool InsertTerminalDeviceErrorDataMutiValue_sla(List<ejlog_devicetermprob> dataList)
        //{
        //    bool _result = false;
        //    string sqlins = string.Empty;
        //    int nPosStar = 0;
        //    string strTemRem = string.Empty;


        //    try
        //    {
        //        var lastData = dataList.Last();
        //        sqlins = "Insert into ejlog_devicetermprob_sla_bak (terminalid,probcode,trxdatetime,status,createdate,remark) Values";

        //        foreach (var item in dataList)
        //        {
        //            nPosStar = item.remark.IndexOf(" ");
        //            strTemRem = item.remark.Substring(nPosStar, item.remark.Length - nPosStar);
        //            strTemRem = strTemRem.Trim();

        //            if (item == lastData)
        //            {
        //                //Console.WriteLine(item);
        //                // Console.WriteLine(lastData);
        //                sqlins += " ('" + item.terminalid + "','" + item.probcode + "','" + item.trxdatetime.ToString("yyyy-MM-dd HH:mm:ss") + "','" + item.status + "',now(),'" + strTemRem + "')";

        //            }
        //            else
        //            {

        //                sqlins += " ('" + item.terminalid + "','" + item.probcode + "','" + item.trxdatetime.ToString("yyyy-MM-dd HH:mm:ss") + "','" + item.status + "',now(),'" + strTemRem + "'),";

        //            }


        //        }


        //        if (dataList.Count > 0)
        //        {
        //            _result = _objDb.ExecuteQueryNoneParam(sqlins);
        //            ErrorMessDBIns = _objDb.ErrorMessDB;
        //            _result = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
                
        //        ErrorMessDBIns = _objDb.ErrorMessDB;
        //    }


        //    return _result;
        //}

        //public bool InsertTerminalDeviceErrorDataMutiValue(List<ejlog_devicetermprob> dataList)
        //{
        //    bool _result = false;
        //    string sqlins = string.Empty;
        //    int nPosStar = 0;
        //    string strTemRem = string.Empty;


        //    try
        //    {
        //        var lastData = dataList.Last();
        //        sqlins = "Insert into ejlog_devicetermprob_bak (terminalid,probcode,trxdatetime,status,createdate,remark) Values";

        //        foreach (var item in dataList)
        //        {
        //            nPosStar = item.remark.IndexOf(" ");
        //            strTemRem = item.remark.Substring(nPosStar, item.remark.Length - nPosStar);
        //            strTemRem = strTemRem.Trim();

        //            if (item == lastData)
        //            {
        //                //Console.WriteLine(item);
        //                // Console.WriteLine(lastData);
        //                sqlins += " ('" + item.terminalid + "','" + item.probcode + "','" + item.trxdatetime.ToString("yyyy-MM-dd HH:mm:ss") + "','" + item.status + "',now(),'" + strTemRem + "')";

        //            }
        //            else
        //            {

        //                sqlins += " ('" + item.terminalid + "','" + item.probcode + "','" + item.trxdatetime.ToString("yyyy-MM-dd HH:mm:ss") + "','" + item.status + "',now(),'" + strTemRem + "'),";

        //            }


        //        }


        //        if (dataList.Count > 0)
        //        {
        //            _result = _objDb.ExecuteQueryNoneParam(sqlins);
        //            ErrorMessDBIns = _objDb.ErrorMessDB;
        //            _result = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
               
        //        ErrorMessDBIns = _objDb.ErrorMessDB;
        //    }


        //    return _result;
        //}

        public bool InsertTerminalDeviceErrorData_sla(string pTerminalid, string pProbCode, string pTrxdatetime, string pStatus, string ErrDesc, string dtEndTimeErrCode13,
           long dtTimeErrCode13)
        {
            bool _result = false;
            string sqlins = string.Empty;
            int nPosStar = 0;
            string strTemRem = string.Empty;
            try
            {
                nPosStar = ErrDesc.IndexOf(" ");
                strTemRem = ErrDesc.Substring(nPosStar, ErrDesc.Length - nPosStar);
                strTemRem = strTemRem.Trim();
                sqlins = "Insert into ejlog_devicetermprob_sla_bak (terminalid,probcode,trxdatetime,status,createdate,remark) ";
                sqlins += "Values ('" + pTerminalid + "','" + pProbCode + "','" + pTrxdatetime + "','" + pStatus + "',now(),'" + strTemRem + "')";

                _result = _objDb.ExecuteQueryNoneParam(sqlins);
                ErrorMessDBIns = _objDb.ErrorMessDB;
            }
            catch (Exception ex)
            {
               
                ErrorMessDBIns = _objDb.ErrorMessDB;
            }
            return _result;
        }

        public bool GetCheckEJSizeFileDataWithPK(string Terminalid, string ProbCode, string trxDateTime)
        {
            bool _result = false;
            DataTable _dt = new DataTable();
            string _sql = string.Empty;
            try
            {
                _sql = "Select * From ejlog_devicetermprob_bak where terminalid = '" + Terminalid + "' and probcode = '" + ProbCode + "' and  trxdatetime = '" + trxDateTime + "'";
                _dt = _objDb.GetDtDataNoneParam(_sql);
                if (_dt.Rows.Count > 0)
                    _result = true;
                return _result;
            }
            catch (Exception ex)
            { throw ex; }
        }

        //public List<ejlog_devicetermprob> GetEJHaveDataIinDBWithPK(string Terminalid, string dateStart, string dateEnd)
        //{

        //    DataTable _dt = new DataTable();
        //    string _sql = string.Empty;
        //    try
        //    {
        //        _sql = "Select * From ejlog_devicetermprob_bak where terminalid = '" + Terminalid + "' and  trxdatetime between '" + dateStart + "' and '" + dateEnd + "'";
        //        _dt = _objDb.GetDtDataNoneParam(_sql);
        //        List<ejlog_devicetermprob> data = ejlog_devicetermprob.mapToList(_dt).ToList();
        //        return data;
        //    }
        //    catch (Exception ex)
        //    { throw ex; }
        //}

        //public List<ejlog_devicetermprob> GetEJHaveDataIinDBWithPK_sla(string Terminalid, string dateStart, string dateEnd)
        //{

        //    DataTable _dt = new DataTable();
        //    string _sql = string.Empty;
        //    try
        //    {
        //        _sql = "Select * From ejlog_devicetermprob_sla_bak where terminalid = '" + Terminalid + "' and  trxdatetime between '" + dateStart + "' and '" + dateEnd + "'";
        //        _dt = _objDb.GetDtDataNoneParam(_sql);
        //        List<ejlog_devicetermprob> data = ejlog_devicetermprob.mapToList(_dt).ToList();
        //        return data;
        //    }
        //    catch (Exception ex)
        //    { throw ex; }
        //}

        public bool GetCheckEJHaveDataIinDBWithPK_sla(string Terminalid, string ProbCode, string trxDateTime)
        {
            bool _result = false;
            DataTable _dt = new DataTable();
            string _sql = string.Empty;
            try
            {
                _sql = "Select * From ejlog_devicetermprob_sla_bak where terminalid = '" + Terminalid + "' and probcode = '" + ProbCode + "' and  trxdatetime = '" + trxDateTime + "'";
                _dt = _objDb.GetDtDataNoneParam(_sql);
                if (_dt.Rows.Count > 0)
                    _result = true;
                return _result;
            }
            catch (Exception ex)
            { throw ex; }
        }

        #endregion

    }
}
