using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;
using SLA_Management.Models.TermProbModel;

namespace SLA_Management.Data.TermProbDB
{
    public class DataAccess
    {

        #region  Local Variable
        private string _connStr = String.Empty;       
        private string _strDBServer;
        private string _strDBName;
        private string _strPort;
        private string _strUserName;
        private string _strPwd;
        private string _strTimeOut;
        private string _strPool;
        private string _strPathXml;
        private MySQLDBHelp obj;
        private string _strErrDB = string.Empty;
        private string _strConnection = string.Empty;

        #endregion

        #region Property
        public string ConnectionString
        {
            get { return _connStr; }
            set { _connStr = value; }
        }
        public string ErrorMessDB
        {
            get { return _strErrDB; }
            set { _strErrDB = value; }
        }

        public string ConnectionDB
        {
            get { return _strConnection; }
            set { _strConnection = value; }
        }

        #endregion

        #region  Contractor
        public DataAccess()
        {
            var config = new ConfigurationBuilder()
                 .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                 .AddJsonFile("appsettings.json").Build();


            var section = config.GetSection("ConnectString_MySQL");
            var ConnectString_MySQL = section.Get<Connect_MySqlModel>();

            _strDBServer = ConnectString_MySQL.IP;
            _strPort = ConnectString_MySQL.Port;
            _strUserName = ConnectString_MySQL.Username;
            _strPwd = ConnectString_MySQL.Password;
            _strDBName = ConnectString_MySQL.DBName;
            _strTimeOut = ConnectString_MySQL.TimeOut;
            _strPool = ConnectString_MySQL.Pool;
            obj = new MySQLDBHelp(_strDBServer, _strPort, _strUserName, _strPwd, _strDBName, _strTimeOut, _strPool);
            ConnectionDB = obj.ConnectionDB;
        }


        #endregion

        #region Function    
        public DataTable GetDtDataNoneParam(string sql)
        {
            DataTable _dt = new DataTable();
            try
            {
                _dt = obj.GetDatatableNotParam(sql);
            }
            catch (Exception ex)
            {

                ErrorMessDB = obj.ErrorMessDB;
            }
            return _dt;
        }

        public bool ExecuteQueryNoneParam(string sql)
        {
            bool _result = false;
            try
            {
                _result = obj.ExecuteQueryNoneParam(sql);
                ErrorMessDB = obj.ErrorMessDB;
            }
            catch (Exception ex)
            {

                ErrorMessDB = obj.ErrorMessDB;
            }
            return _result;
        }

        #endregion


    }
}