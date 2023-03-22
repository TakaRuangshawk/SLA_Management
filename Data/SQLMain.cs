using Microsoft.Data.SqlClient;
using System.Data;

namespace SLAManagement.Data
{
    public class SQLMain
    {
        public SQLMain(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private readonly IConfiguration Configuration;

        private SqlConnection conn;

        public SQLMain()
        {
            //string stringConect = ConfigurationManager.AppSettings["oracleserver"].ToString();
            this.conn = new SqlConnection("Data Source=10.98.14.13;Initial Catalog=ATMGATEWAYDB;Persist Security Info=True;User ID=sa;Password=P@ssw0rd;");
        }
        public SqlConnection Conn
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
        public bool insertData(SqlCommand com)
        {
            try
            {
                if (GetstateDB() == false)
                {
                    OpenDB();
                }
                com.Connection = conn;
                com.ExecuteNonQuery();
                conn.Close();
                return true;
            }
            catch (Exception ex)
            {

                Console.WriteLine("sql error:" + ex.Message);
                conn.Close();
                return false;
            }
        }
        public DataTable GetDatatable(string sql)
        {
            DataTable dt = new DataTable();
            try
            {
                if (GetstateDB() == false)
                {
                    OpenDB();
                }

                SqlCommand com = new SqlCommand(sql, conn);
                com.CommandTimeout = 300;
                //mysqlcom.Parameters.AddRange(parameters);
                SqlDataAdapter mda = new SqlDataAdapter(com);
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
        public DataTable GetDatatable(SqlCommand com)
        {
            DataTable dt = new DataTable();
            try
            {
                if (GetstateDB() == false)
                {
                    OpenDB();
                }
                com.Connection = conn;
                /* SqlCommand com = new SqlCommand(sql, conn);*/

                SqlDataAdapter mda = new SqlDataAdapter(com);
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

                SqlCommand com = new SqlCommand(sql, conn);
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
