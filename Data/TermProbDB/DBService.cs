using MySql.Data.MySqlClient;
using Org.BouncyCastle.Bcpg.OpenPgp;
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
        private static IConfiguration ConnectString_MySQL;


        private string _strErrDB = string.Empty;

        static MySQLDBHelp _objDb;
        #endregion
        public DBService(IConfiguration myConfiguration)
        {
            ConnectString_MySQL = myConfiguration;
        }



        #region Public Functions     

        public static bool CheckDatabase()
        {
            bool result = false;
            _objDb = new MySQLDBHelp(ConnectString_MySQL.GetValue<string>("ConnectString_MySQL:FullNameConnection"));
            if (_objDb.IsConnect)
            {
                result = true;
            }
            return result;
        }
        public DataTable GetAllMasterProblem()
        {
            DataTable _dt = new DataTable();
            string _sql = string.Empty;
            _objDb = new MySQLDBHelp(ConnectString_MySQL.GetValue<string>("ConnectString_MySQL:FullNameConnection"));

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
            _objDb = new MySQLDBHelp(ConnectString_MySQL.GetValue<string>("ConnectString_MySQL:FullNameConnection"));
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

        public bool InsertDataToProbMaster(string probCode, string probName, string probType, string probTerm)
        {
            bool result = false;
            _objDb = new MySQLDBHelp(ConnectString_MySQL.GetValue<string>("ConnectString_MySQL:FullNameConnection"));
            string _sql = string.Empty;

            try
            {
                _sql = "INSERT INTO `gsb_logview`.`ejlog_problemmascode` (`probcode`,`probname`,`probtype`,`probterm`,`status`,`displayflag`,`createdate`,`updatedate`)VALUE ( '" + probCode + "','" + probName + "','" + probType + "','" + probTerm + "','" + "1" + "','" + "1" + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "') ;";


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






        #endregion

    }
}
