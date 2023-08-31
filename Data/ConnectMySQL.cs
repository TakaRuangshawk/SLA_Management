using MySql.Data.MySqlClient;
using static Org.BouncyCastle.Math.EC.ECCurve;
using System.Data;
using System.Data.Common;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SLA_Management.Data
{
    public class ConnectMySQL
    {

        #region Local Variable
        private string strConnection;
        private string _strErrDB = string.Empty;
        private  MySqlConnection conn = null;
        private bool isConnect = false;
        #endregion

        #region Constructor
        public ConnectMySQL(string mySqlConnection)
        {
            //string stringConect = ConfigurationManager.AppSettings["oracleserver"].ToString();
            strConnection = mySqlConnection;
            ConnectionDB = strConnection;
            conn = new MySqlConnection(mySqlConnection);
        }
        public MySqlConnection Conn
        {
            get
            {
                return conn;
            }
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
                if (Conn != null && isConnect)
                    return true;
                else
                    return false;
            }
        }

        //public MySqlConnection DbConnection
        //{
        //    get
        //    {
        //        if (conn != null)
        //            return conn;
        //        else
        //        {
        //            if (OpenDb())
        //                return conn;
        //            else
        //                return null;
        //        }
        //    }
        //}

        #endregion

        #region Public Function

        public bool OpenDb()
        {
            try
            {
                conn = new MySqlConnection(strConnection);
                conn.Open();
                isConnect = true;
                return true;
            }
            catch (MySqlException)
            {

                isConnect = false;
                return false;
            }
        }

        public DataTable GetDatatableNotParam(string sql)
        {
            DataTable dt = new DataTable();
            try
            {
                if (!IsConnect)
                    OpenDb();
                MySqlCommand mysqlcom = new MySqlCommand(sql, Conn);
                MySqlDataAdapter mda = new MySqlDataAdapter(mysqlcom);
                mda.Fill(dt);
                CloseDb();
            }
            catch (MySqlException)
            {

            }
            CloseDb();
            return dt;

        }
        public bool ExecuteQueryNoneParam(string sql)
        {
            bool _return = false;
            try
            {
                if (!IsConnect)
                    OpenDb();

                MySqlCommand command = new MySqlCommand();

                command.Connection = Conn;
                command.CommandType = CommandType.Text;
                command.CommandText = sql;

                if (command.ExecuteNonQuery() == 1)
                    _return = true;

                // DbConnection.Close();
            }
            catch (MySqlException ex)
            {

                ErrorMessDB = ex.Message;
            }
            CloseDb();
            return _return;
        }

        public bool CloseDb()
        {
            try
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                    isConnect = false;
                }
                return true;
            }
            catch (MySqlException)
            {

                return false;
            }
        }

        public void OpenDB()
        {
            try
            {
                conn.Open();
            }
            catch (MySqlException)
            {
                OpenDB();
            }

        }
        public bool GetstateDB()
        {
            try
            {
                OpenDB();
            }
            catch (MySqlException)
            {

            }
            

            if (conn.State == ConnectionState.Open)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool insertData(string sql, MySqlCommand com)
        {
            try
            {

                if (GetstateDB() == false)
                {
                    OpenDB();
                }
                com.Connection = conn;
                com.CommandText = sql;
                com.ExecuteNonQuery();
                conn.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                //Log.Information("sql error:" + ex.Message);
                Console.WriteLine("sql error:" + ex.Message);
                conn.Close();
                return false;
            }
        }

        public DataTable GetDatatable(MySqlCommand com)
        {
            DataTable dt = new DataTable();
            try
            {
                if (GetstateDB() == false)
                {
                    OpenDB();
                }
                com.Connection = conn;
                com.CommandTimeout = 300;
                MySqlDataAdapter mda = new MySqlDataAdapter(com);
                mda.Fill(dt);
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return dt;
        }

        public void createTable(string sql)
        {

            try
            {
                if (GetstateDB() == false)
                {
                    OpenDB();
                }

                MySqlCommand com = new MySqlCommand(sql, conn);
                com.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }

        }

        #endregion

    }
}
