using MySql.Data.MySqlClient;
using Org.BouncyCastle.Bcpg.OpenPgp;
using SLA_Management.Models.TermProbModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using static SLA_Management.Controllers.EJAddTranProbTermController;

namespace SLA_Management.Data
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

        public class checkuserfeelview
        {
            public string check { get; set; }
        }

        #endregion

        #region Local Variable
        private static IConfiguration ConnectString_MySQL;

        private string _strErrDB = string.Empty;

        private static ConnectMySQL _objDb;

        private static string FullNameConnection = string.Empty;
        #endregion

        #region Constructor
        public DBService(IConfiguration myConfiguration)
        {
            ConnectString_MySQL = myConfiguration;

            FullNameConnection = ConnectString_MySQL.GetValue<string>("ConnectString_MySQL:FullNameConnection");

            _objDb = new ConnectMySQL(FullNameConnection);

        }
        #endregion

        #region Public Functions     

        public static bool CheckDatabase()
        {
            bool result = false;
           
            if (_objDb.GetstateDB())
            {
                result = true;
            }
            return result;
        }
        public DataTable GetAllMasterProblem()
        {
            DataTable _dt = new DataTable();
            string _sql = string.Empty;         

            try
            {
                _sql = "Select * From ejlog_problemmascode where status = '1'";
                _dt = _objDb.GetDatatableNotParam(_sql);
                return _dt;
            }
            catch (Exception ex)
            { throw ex; }
        }

        public DataTable GetClientData()
        {
            DataTable _dt = new DataTable();
            string _sql = string.Empty;
            
            try
            {
                //_sql = "Select * From ejlog_terminal order by terminalid";
                _sql = "Select DISTINCT TERM_ID as terminalid , TERM_SEQ  From fv_device_info order by TERM_ID";
                _dt = _objDb.GetDatatableNotParam(_sql);
                return _dt;
            }
            catch (Exception ex)
            { throw ex; }
        }

        public bool InsertDataToProbMaster(string probCode, string probName, string probType, string probTerm, string memo, string username)
        {
            bool result = false;
            
            string _sql = string.Empty;

            try
            {
                _sql = "INSERT INTO `gsb_logview`.`ejlog_problemmascode` (`probcode`,`probname`,`probtype`,`probterm`,`status`,`displayflag`,`memo`,`createdate`,`updatedate`,`updateby`)VALUE ( '" + probCode + "','" + probName + "','" + probType + "','" + probTerm + "','" + "1" + "','" + "1" + "','" + memo + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + username + "') ;";


                result = _objDb.ExecuteQueryNoneParam(_sql);

                if (result == false)
                {
                    if (_objDb.ErrorMessDB != null)
                        ErrorMessage = _objDb.ErrorMessDB;
                }


                return result;
            }
            catch (Exception ex)
            { throw ex; }

        }
        public List<ProblemMaster> GetMasterSysErrorWord()
        {
            List<ProblemMaster> _result = new List<ProblemMaster>();
            DataTable _dt = new DataTable();
          
            try
            {

                _dt = GetAllMasterProblem();
                foreach (DataRow _dr in _dt.Rows)
                {
                    ProblemMaster obj = new ProblemMaster();
                    obj.ProblemCode = _dr["probcode"].ToString();
                    obj.ProblemName = _dr["probname"].ToString();
                    obj.Memo = _dr["memo"].ToString();
                    _result.Add(obj);
                }
            }
            catch (Exception ex)
            {

            }
            return _result;
        }

        public DataTable GetClientFromDB()
        {

            DataTable _result = null;
            try
            {
                _result = GetClientData();
            }
            catch (Exception ex)
            { }
            return _result;
        }

        public List<ej_trandeviceprob> GetErrorTermDeviceEJLogCollectionFromReader(IDataReader reader)
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



        public List<ej_trandeviceprob> GetErrorTermDeviceEJLog_Database(ej_trandada_seek model)
        {
            try
            {
                using (MySqlConnection cn = new MySqlConnection(FullNameConnection))
                {
                    MySqlCommand cmd = new MySqlCommand("GenDeviceProblemError", cn);

                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new MySqlParameter("?pStratDate", model.FRDATE));
                    cmd.Parameters.Add(new MySqlParameter("?pEndDate", model.TODATE));
                    cmd.Parameters.Add(new MySqlParameter("?pTerminalID", model.TERMID));
                    cmd.Parameters.Add(new MySqlParameter("?pProbMaster", model.PROBNAME));

                    cn.Open();
                    return GetErrorTermDeviceEJLogCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                string err = ex.Message;
                return null;
            }
        }


        public List<ej_trandeviceprob> GetErrorTermDeviceEJLog_Database_sla(ej_trandada_seek model)
        {
            try
            {
                using (MySqlConnection cn = new MySqlConnection(FullNameConnection))
                {
                    MySqlCommand cmd = new MySqlCommand("GenDeviceProblemError_sla", cn);

                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new MySqlParameter("?pStratDate", model.FRDATE));
                    cmd.Parameters.Add(new MySqlParameter("?pEndDate", model.TODATE));
                    cmd.Parameters.Add(new MySqlParameter("?pTerminalID", model.TERMID));
                    cmd.Parameters.Add(new MySqlParameter("?pProbMaster", model.PROBNAME));

                    cn.Open();
                    return GetErrorTermDeviceEJLogCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                string err = ex.Message;
                return null;
            }
        }

        public List<ej_trandeviceprob> GetErrorTermDeviceEJLog_Database_separate(ej_trandada_seek model)
        {
            try
            {
                using (MySqlConnection cn = new MySqlConnection(FullNameConnection))
                {
                    MySqlCommand cmd = new MySqlCommand("GenDeviceProblemError_separate", cn);

                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new MySqlParameter("?pStratDate", model.FRDATE));
                    cmd.Parameters.Add(new MySqlParameter("?pEndDate", model.TODATE));
                    cmd.Parameters.Add(new MySqlParameter("?pTerminalID", model.TERMID));
                    cmd.Parameters.Add(new MySqlParameter("?pProbMaster", model.PROBNAME));

                    cn.Open();
                    return GetErrorTermDeviceEJLogCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                string err = ex.Message;
                return null;
            }
        }

        public ej_trandeviceprob GetErrorTermDeviceEJLogFromReader(IDataReader reader, int pSeqNo)
        {
            ej_trandeviceprob record = new ej_trandeviceprob();

            record.Seqno = reader["serialnumber"].ToString();
            record.TerminalID = reader["terminalid"].ToString();
            record.BranchName = reader["branchname"].ToString();
            record.Location = reader["locationbranch"].ToString();
            record.ProbName = reader["probname"].ToString();
            record.Remark = reader["remark"].ToString();
            record.TransactionDate = Convert.ToDateTime(reader["trxdatetime"]);

            return record;
        }
        public List<ej_trandeviceprob> GetErrorTermDeviceKWEJLog_Database(ej_trandada_seek model)
        {
            try
            {
                using (MySqlConnection cn = new MySqlConnection(FullNameConnection))
                {
                    MySqlCommand cmd = new MySqlCommand("GenDeviceProblemErrorKW", cn);

                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new MySqlParameter("?pStratDate", model.FRDATE));
                    cmd.Parameters.Add(new MySqlParameter("?pEndDate", model.TODATE));
                    cmd.Parameters.Add(new MySqlParameter("?pTerminalID", model.TERMID));
                    cmd.Parameters.Add(new MySqlParameter("?pProbKeyWord", model.PROBKEYWORD));

                    cn.Open();
                    return GetErrorTermDeviceEJLogCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                string err = ex.Message;
                return null;
            }
        }

        public List<checkuserfeelview> GetCheckUserFeelview(string user, string email)
        {
            string _sql = string.Empty;

            try
            {
                using (MySqlConnection cn = new MySqlConnection(FullNameConnection))
                {

                    _sql = "SELECT CASE WHEN COUNT(*) > 0 THEN 'yes' ELSE 'no' END AS _check FROM fv_system_users WHERE   ";
                    _sql += " ACCOUNT = '" + user + "' AND EMAIL = '" + email + "'; ";
                    cn.Open();

                    MySqlCommand cmd = new MySqlCommand(_sql, cn);
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();

                    return GetCheckUserFeelviewCollectionFromReader(ExecuteReader(cmd));
                }
            }
            catch (MySqlException ex)
            {
                return null;
            }
        }


        #endregion

        #region Private/Protected Function

        private IDataReader ExecuteReader(DbCommand cmd)
        {
            return ExecuteReader(cmd, CommandBehavior.Default);
        }

        private IDataReader ExecuteReader(DbCommand cmd, CommandBehavior behavior)
        {
            try
            {
                return cmd.ExecuteReader(behavior);
            }
            catch (MySqlException ex)
            {
                string err = "";
                err = "Inner message : " + ex.InnerException.Message;
                err += Environment.NewLine + "Message : " + ex.Message;
                return null;
            }
        }

        protected virtual List<checkuserfeelview> GetCheckUserFeelviewCollectionFromReader(IDataReader reader)
        {
            List<checkuserfeelview> recordlst = new List<checkuserfeelview>();
            while (reader.Read())
            {
                recordlst.Add(GetCheckUserFeelviewFromReader(reader));
            }
            return recordlst;
        }
        protected virtual checkuserfeelview GetCheckUserFeelviewFromReader(IDataReader reader)
        {
            checkuserfeelview record = new checkuserfeelview();

            record.check = reader["_check"].ToString();
            return record;
        }

        #endregion

    }
}
