using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLA_Management.Data.TermProbDB
{
    class MySQLDBHelp
    {
        #region Local Variable
        private string strConnection;
        private string _strErrDB = string.Empty;
        private MySqlConnection m_dbConnection = null;
        private bool isConnect = false;
        #endregion

        #region Constructor
        public MySQLDBHelp(string connectString)
        {
            strConnection = connectString;
            ConnectionDB = strConnection;
        }
        #endregion

        #region Property
        public string ConnectionDB
        {
            get { return strConnection; }
            set { strConnection = value; }
        }

        public string ErrorMessDB
        {
            get { return _strErrDB; }
            set { _strErrDB = value; }
        }

        public bool IsConnect
        {
            get
            {
                if (DbConnection != null && isConnect)
                    return true;
                else
                    return false;
            }
        }

        public MySqlConnection DbConnection
        {
            get
            {
                if (m_dbConnection != null)
                    return m_dbConnection;
                else
                {
                    if (OpenDb())
                        return m_dbConnection;
                    else
                        return null;
                }
            }
        }

        #endregion



        #region Public Functions
        public bool OpenDb()
        {
            try
            {
                m_dbConnection = new MySqlConnection(strConnection);
                m_dbConnection.Open();
                isConnect = true;
                return true;
            }
            catch (Exception ex)
            {

                isConnect = false;
                return false;
            }
        }

        public bool CloseDb()
        {
            try
            {
                if (m_dbConnection != null)
                {
                    m_dbConnection.Close();
                    m_dbConnection.Dispose();
                    isConnect = false;
                }
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        public object GetValue(string sql)
        {
            try
            {
                if (!IsConnect)
                    OpenDb();
                MySqlCommand cmd = new MySqlCommand(sql, DbConnection);
                Object result = cmd.ExecuteScalar();
                cmd.Dispose();
                CloseDb();
                return result;
            }
            catch (Exception ex)
            {

                CloseDb();
                return null;
            }
        }

        public MySqlDataReader ExecuteReader(string sql)
        {
            try
            {
                if (!IsConnect)
                    OpenDb();
                MySqlCommand cmd = new MySqlCommand(sql, DbConnection);
                MySqlDataReader reader = cmd.ExecuteReader();
                CloseDb();
                return reader;

            }
            catch (Exception ex)
            {

                CloseDb();
                return null;
            }
        }

        public DataTable GetDatatable(string sql, params MySqlParameter[] parameters)
        {
            if (!IsConnect)
                OpenDb();
            MySqlCommand mysqlcom = new MySqlCommand(sql, DbConnection);
            mysqlcom.Parameters.AddRange(parameters);
            MySqlDataAdapter mda = new MySqlDataAdapter(mysqlcom);
            DataTable dt = new DataTable();
            mda.Fill(dt);
            CloseDb();
            return dt;

        }

        public DataTable GetDatatableNotParam(string sql)
        {
            DataTable dt = new DataTable();
            try
            {
                if (!IsConnect)
                    OpenDb();
                MySqlCommand mysqlcom = new MySqlCommand(sql, DbConnection);
                MySqlDataAdapter mda = new MySqlDataAdapter(mysqlcom);
                mda.Fill(dt);
                CloseDb();
            }
            catch (Exception ex)
            {

            }
            CloseDb();
            return dt;

        }



        public bool InsertDataStoreWithParam(string strStoreName, string param)
        {
            bool _return = false;
            string _sql = string.Empty;
            try
            {
                if (!IsConnect)
                    OpenDb();
                MySqlCommand command = new MySqlCommand();

                _sql = "Insert into ejlog_balance_period " + param;
                command.Connection = DbConnection;
                command.CommandType = CommandType.Text;
                command.CommandText = _sql.Replace("/", "-");

                if (command.ExecuteNonQuery() == 1)
                    _return = true;

                // DbConnection.Close();
            }
            catch (Exception ex)
            {

                ErrorMessDB = ex.Message;
            }
            CloseDb();
            return _return;
        }

        public bool ExecuteQueryNoneParam(string sql)
        {
            bool _return = false;
            try
            {
                if (!IsConnect)
                    OpenDb();

                MySqlCommand command = new MySqlCommand();

                command.Connection = DbConnection;
                command.CommandType = CommandType.Text;
                command.CommandText = sql;

                if (command.ExecuteNonQuery() == 1)
                    _return = true;

                // DbConnection.Close();
            }
            catch (Exception ex)
            {

                ErrorMessDB = ex.Message;
            }
            CloseDb();
            return _return;
        }

        public DataTable GetDataStoreProcedure(string pStoreName, List<MySqlParameter> pParam)
        {
            DataTable _dt = new DataTable();
            try
            {
                if (!IsConnect)
                    OpenDb();

                MySqlCommand command = new MySqlCommand();
                command.Connection = DbConnection;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = pStoreName;

                foreach (MySqlParameter obj in pParam)
                {
                    MySqlParameter _param = new MySqlParameter();
                    _param = obj;
                    command.Parameters.Add(_param);
                }

                MySqlDataAdapter adap = new MySqlDataAdapter(command);
                adap.Fill(_dt);
                CloseDb();
                // DbConnection.Close();
            }
            catch (Exception ex)
            {

                ErrorMessDB = ex.Message;
            }
            CloseDb();
            return _dt;

        }

        #endregion

    }
}
