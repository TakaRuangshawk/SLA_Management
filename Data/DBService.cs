using MySql.Data.MySqlClient;
using Org.BouncyCastle.Bcpg.OpenPgp;
using SLA_Management.Models.LogMonitorModel;
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


        #endregion

        #region Local Variable
        protected static IConfiguration ConnectString_MySQL;

        protected string _strErrDB = string.Empty;

        protected static ConnectMySQL _objDb;

        protected static string FullNameConnection = string.Empty;
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


        protected static IDataReader ExecuteReader(DbCommand cmd)
        {
            return ExecuteReader(cmd, CommandBehavior.Default);
        }

        protected static IDataReader ExecuteReader(DbCommand cmd, CommandBehavior behavior)
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

       


        #endregion

    }
}
