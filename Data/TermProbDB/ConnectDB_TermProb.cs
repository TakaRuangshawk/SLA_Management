using SLA_Management.Models.TermProbModel;
using System.Data.Common;
using System.Data;
using MySql.Data.MySqlClient;

namespace SLA_Management.Data.TermProbDB
{
    public class ConnectDB_TermProb
    {
        public List<ej_trandeviceprob> GetErrorTermDeviceKWEJLog(ej_trandada_seek model, int startRowIndex, int maximumRows)
        {
            List<ej_trandeviceprob> recordset = null;

            recordset = GetErrorTermDeviceKWEJLog_Database(model, GetPageIndex(startRowIndex, maximumRows), maximumRows);

            return recordset;
        }

        private int GetPageIndex(int startRowIndex, int maximumRows)
        {
            if (maximumRows <= 0)
                return 0;
            else
                return (int)Math.Floor((double)startRowIndex / (double)maximumRows);
        }

        private List<ej_trandeviceprob> GetErrorTermDeviceEJLogCollectionFromReader(IDataReader reader)
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

        private ej_trandeviceprob GetErrorTermDeviceEJLogFromReader(IDataReader reader, int pSeqNo)
        {
            ej_trandeviceprob record = new ej_trandeviceprob();

            record.Seqno = pSeqNo;
            record.TerminalID = reader["terminalid"].ToString();
            record.BranchName = reader["branchname"].ToString();
            record.Location = reader["locationbranch"].ToString();
            record.ProbName = reader["probname"].ToString();
            record.Remark = reader["remark"].ToString();
            record.TransactionDate = Convert.ToDateTime(reader["trxdatetime"]);

            return record;
        }
        public List<ej_trandeviceprob> GetErrorTermDeviceKWEJLog_Database(ej_trandada_seek model, int pageIndex, int pageSize)
        {
            try
            {
                using (MySqlConnection cn = new MySqlConnection("server=10.98.14.12;Port=3308;User Id=root;database=gsb_logview;password=P@ssw0rd;CharSet=utf8;"))
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

        public List<ProblemMaster> GetMasterSysErrorWord()
        {
            List<ProblemMaster> _result = new List<ProblemMaster>();
            DataTable _dt = new DataTable();
            DBService _objDB = new DBService();
            try
            {

                _dt = _objDB.GetAllMasterProblem();
                foreach (DataRow _dr in _dt.Rows)
                {
                    ProblemMaster obj = new ProblemMaster();
                    obj.ProblemCode = _dr["probcode"].ToString();
                    obj.ProblemName = _dr["probname"].ToString();
                    _result.Add(obj);
                }
            }
            catch (Exception ex)
            {

            }
            return _result;
        }
    }
}
