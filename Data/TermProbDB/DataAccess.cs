using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace SLA_Management.Data.TermProbDB
{
    public class DataAccess
    {
        private string _connStr = String.Empty;
        public string ConnectionString
        {
            get { return _connStr; }
            set { _connStr = value; }
        }
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


        public DataAccess()
        {
            _strDBServer = "10.98.14.12";
            _strPort = "3308";
            _strUserName = "root";
            _strPwd = "P@ssw0rd";
            _strDBName = "gsb_logview";
            _strTimeOut = "30";
            _strPool = "";
            obj = new MySQLDBHelp(_strDBServer, _strPort, _strUserName, _strPwd, _strDBName, _strTimeOut, _strPool);
            ConnectionDB = obj.ConnectionDB;
        }



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

        public DataTable GetDtDataParam(string sql, params MySqlParameter[] parameters)
        {
            DataTable _dt = new DataTable();
            try
            {
                _dt = obj.GetDatatable(sql, parameters);
            }
            catch (Exception ex)
            {
                
            }
            return _dt;
        }

        public int GetIntValue(string sql)
        {
            int _result = 0;
            try
            {
                _result = Convert.ToInt32(obj.GetValue(sql));
            }
            catch (Exception ex)
            {
                
            }
            return _result;
        }

        public string GetStrValue(string sql)
        {
            string _result = "";
            try
            {
                _result = Convert.ToString(obj.GetValue(sql));
            }
            catch (Exception ex)
            {
               
            }
            return _result;
        }

        public decimal GetDecValue(string sql)
        {
            decimal _result = 0;
            try
            {
                _result = Convert.ToDecimal(obj.GetValue(sql));
            }
            catch (Exception ex)
            {
               
            }
            return _result;
        }

        public bool InsertDataStoreWithParam(string strStoreName, string param)
        {
            bool _return = false;
            try
            {
                _return = obj.InsertDataStoreWithParam(strStoreName, param);
                ErrorMessDB = obj.ErrorMessDB;
            }
            catch (Exception ex)
            {
               
                ErrorMessDB = obj.ErrorMessDB;
            }
            return _return;
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



        public DataTable GetDtDataStoreProcedure(string pStoreName, List<MySqlParameter> pParams)
        {
            DataTable _dt = new DataTable();
            try
            {
                _dt = obj.GetDataStoreProcedure(pStoreName, pParams);
            }
            catch (Exception ex)
            {
               
                ErrorMessDB = obj.ErrorMessDB;
            }
            return _dt;
        }


        public int ExecuteNonQuery(DbCommand cmd)
        {
            foreach (DbParameter param in cmd.Parameters)
            {
                if (param.Direction == ParameterDirection.Output ||
                   param.Direction == ParameterDirection.ReturnValue)
                {
                    switch (param.DbType)
                    {
                        case DbType.AnsiString:
                        case DbType.AnsiStringFixedLength:
                        case DbType.String:
                        case DbType.StringFixedLength:
                        case DbType.Xml:
                            param.Value = "";
                            break;
                        case DbType.Boolean:
                            param.Value = false;
                            break;
                        case DbType.Byte:
                            param.Value = byte.MinValue;
                            break;
                        case DbType.Date:
                        case DbType.DateTime:
                            param.Value = DateTime.MinValue;
                            break;
                        case DbType.Currency:
                        case DbType.Decimal:
                            param.Value = decimal.MinValue;
                            break;
                        case DbType.Guid:
                            param.Value = Guid.Empty;
                            break;
                        case DbType.Double:
                        case DbType.Int16:
                        case DbType.Int32:
                        case DbType.Int64:
                            param.Value = 0;
                            break;
                        default:
                            param.Value = null;
                            break;
                    }
                }
            }
            return cmd.ExecuteNonQuery();
        }

        public IDataReader ExecuteReader(DbCommand cmd)
        {
            return ExecuteReader(cmd, CommandBehavior.Default);
        }

        public IDataReader ExecuteReader(DbCommand cmd, CommandBehavior behavior)
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

        public object ExecuteScalar(DbCommand cmd)
        {
            return cmd.ExecuteScalar();
        }
    }
}