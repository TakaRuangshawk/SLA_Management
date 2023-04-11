//using Microsoft.Data.SqlClient;
using System.Data.SqlClient;
using System.Data;
using System.Reflection;

namespace SLAManagement.Data
{
    public class ConnectSQL_Server
    {
        private SqlConnection conn;

        

        public ConnectSQL_Server(string sqlConnection)
        {
            //string stringConect = ConfigurationManager.AppSettings["oracleserver"].ToString();
            this.conn = new SqlConnection(sqlConnection);
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
                throw new Exception(ex + "");
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
                //Log.Information("sql error:" + ex.Message);
                Console.WriteLine("sql error:" + ex.Message);
                conn.Close();
                return false;
            }
        }
        public bool updateData(SqlCommand com)
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
                //Log.Information("sql error:" + ex.Message);
                Console.WriteLine("sql error:" + ex.Message);
                conn.Close();
                return false;
            }
        }


        public int insertDataAndReturnId(SqlCommand com)
        {
            try
            {
                if (GetstateDB() == false)
                {
                    OpenDB();
                }
                com.Connection = conn;
                int PK = (int)com.ExecuteScalar();
                conn.Close();
                return PK;
            }
            catch (Exception ex)
            {
                //Log.Information("sql error:" + ex.Message);
                Console.WriteLine("sql error:" + ex.Message);
                conn.Close();
                return 0;
            }
        }
        public bool deleteData(SqlCommand com)
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
                //Log.Information("sql error:" + ex.Message);
                Console.WriteLine("sql error:" + ex.Message);
                conn.Close();
                return false;
            }
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

        public List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        public T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }

       

        public int GetCountTable(string CommandText)
        {
            Int32 count = 0;
            try
            {
                if (GetstateDB() == false)
                {
                    OpenDB();
                }

                SqlCommand com = new SqlCommand(CommandText, conn);
                count = (Int32)com.ExecuteScalar();
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
           
            return count;
        }

    }
}
