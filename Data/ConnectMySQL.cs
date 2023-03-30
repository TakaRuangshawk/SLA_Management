using MySql.Data.MySqlClient;
using static Org.BouncyCastle.Math.EC.ECCurve;
using System.Data;

namespace SLA_Management.Data
{
    public class ConnectMySQL
    {
        private MySqlConnection conn;

        public ConnectMySQL(string mySqlConnection)
        {
            //string stringConect = ConfigurationManager.AppSettings["oracleserver"].ToString();
            this.conn = new MySqlConnection(mySqlConnection);
        }
        public MySqlConnection Conn
        {
            get
            {
                return conn;
            }
        }
        public void OpenDB()
        {
            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                OpenDB();
            }

        }
        public bool GetstateDB()
        {

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
            catch (Exception ex)
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
            catch (Exception ex)
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }

        }
    }
}
