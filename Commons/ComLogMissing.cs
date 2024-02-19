using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using SLA_Management.Data;
using SLA_Management.Models.COMLogModel;
using SLA_Management.Data;
using System.Data;
using System.Data.SqlClient;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace SLA_Management.Commons
{
    public class ComLogMissing
    {
        private string sqlConnection;
        private string mySqlConnectionFv;
        

        public ComLogMissing(string sqlConnection)
        {
            this.sqlConnection = sqlConnection;
        }

        public void InsertComLogMissing(string comLogMissing, string term_id)
        {

            string start = comLogMissing.Substring(3, 8);
            string end = comLogMissing.Substring(3, 8);
            int countInseart = 0;
            try
            {
                DateTime date_Start = new DateTime(Convert.ToInt32(start.Substring(0, 4)), Convert.ToInt32(start.Substring(4, 2)), Convert.ToInt32(start.Substring(6, 2)), 0, 0, 0);
                DateTime date_End = new DateTime(Convert.ToInt32(end.Substring(0, 4)), Convert.ToInt32(end.Substring(4, 2)), Convert.ToInt32(end.Substring(6, 2)), 23, 59, 59);
                IList<Base24_master_errordesc> base24_Master_Errordescs = GetFeelView_base24_master_errordesc();
                LinkedList<string> ListFV = new LinkedList<string>();
                foreach (Base24_master_errordesc item in base24_Master_Errordescs)
                {
                    if (!ListFV.Contains(item.Feelview_eventNo))
                    {
                        ListFV.AddFirst(item.Feelview_eventNo);
                    }
                }
                if (date_End > date_Start)
                {
                    int DateStartBetweenDateEnd = ((date_End - date_Start).Days) + 1;
                    LinkedList<ComLogLost> data =GetComlog_recordNotInsert(date_Start, DateStartBetweenDateEnd, term_id);

                    foreach (ComLogLost noComLog in data)
                    {
                        foreach (string comLog in noComLog.ListComLog)
                        {
                            DateTime dateTimeS = new DateTime(Int32.Parse(comLog.Substring(3, 4)), Int32.Parse(comLog.Substring(7, 2)), Int32.Parse(comLog.Substring(9, 2)), 0, 0, 0);
                            DateTime dateTimeE = new DateTime(Int32.Parse(comLog.Substring(3, 4)), Int32.Parse(comLog.Substring(7, 2)), Int32.Parse(comLog.Substring(9, 2)), 23, 59, 59);
                            IList<Mt_caseflow_record> test = GetEventList_Mt_caseflow_record_his_For_Date(dateTimeS, dateTimeE, noComLog.Term_id);



                            foreach (string FVEvent in ListFV)
                            {
                                foreach (Mt_caseflow_record item in test)
                                {
                                    Sla_emslog startEvent = new Sla_emslog();
                                    Sla_emslog entEvent = new Sla_emslog();
                                    startEvent.Term_id = item.Term_id;
                                    startEvent.Start_datetime = item.Start_time;
                                    Base24_master_errordesc EventNoFvStart = base24_Master_Errordescs.SingleOrDefault(i => (i.Feelview_eventNo == FVEvent && i.Feelview_event_type == "S"));
                                    startEvent.Event_no = EventNoFvStart.Event_no;
                                    startEvent.Msg_content = FVEvent;


                                    InsertEventNo(startEvent, item.End_time.Year + "" + item.End_time.Month.ToString("00"));
                                    entEvent.Term_id = item.Term_id;
                                    Base24_master_errordesc EventNoFvEnd = base24_Master_Errordescs.SingleOrDefault(i => i.Feelview_eventNo == FVEvent && i.Feelview_event_type == "E");
                                    entEvent.Event_no = EventNoFvEnd.Event_no;
                                    entEvent.Start_datetime = item.End_time;
                                    entEvent.Msg_content = FVEvent;
                                    InsertEventNo(entEvent, item.End_time.Year + "" + item.End_time.Month.ToString("00"));
                                    countInseart += 2;
                                }
                            }
                        }


                    }
                    countInseart += InsertComLogMissingEjlog(date_Start, date_End, term_id);
                }

            }
            catch (Exception ex)
            {


            }

            if (countInseart != 0)
            {
                ConnectSQL_Server db = new ConnectSQL_Server(sqlConnection);
                string sqlRecord = "INSERT INTO [dbo].[comlog_record] ( TERM_ID, MSG_SOURCE, TOTAL_RECORD, UPDATE_DATE, UPDATE_BY, REMARK) VALUES (@TERM_ID, @MSG_SOURCE, @TOTAL_RECORD, @UPDATE_DATE, @UPDATE_BY, @REMARK);";
                
                SqlCommand com = new SqlCommand();
                com.CommandText = sqlRecord;
                com.Parameters.AddWithValue("@TERM_ID", term_id);
                com.Parameters.AddWithValue("@MSG_SOURCE", term_id + "_" + comLogMissing);
                com.Parameters.AddWithValue("@TOTAL_RECORD", countInseart);
                com.Parameters.AddWithValue("@UPDATE_DATE", DateTime.Now);
                com.Parameters.AddWithValue("@UPDATE_BY", "repairInsert");
                com.Parameters.AddWithValue("@REMARK", "");
                db.insertData(com);
                //db.insertDataComlog_recordString(sqlRecord, comlog_record);
            }


        }
        public int InsertComLogMissingEjlog(DateTime start, DateTime end, string terminalid)
        {
            int count = 0;
            try
            {
                ConnectSQL_Server db = new ConnectSQL_Server(sqlConnection);
                string sqlRecord = "SELECT * FROM ejlog_message_" + start.ToString("yyyyMM") + " where terminalid = '" + terminalid + "' and  trxdatetime between '" + start.ToString("yyyy-MM-DD") + "' and '" + end.ToString("yyyy-MM-DD") + "'";
                SqlCommand com = new SqlCommand();
                com.CommandText= sqlRecord;
                DataTable datas = db.GetDatatable(com);



                foreach (DataRow row in datas.Rows)
                {
                    Sla_emslog sla = new Sla_emslog();

                    sla.Term_id = (string)row["terminalid"];
                    sla.Start_datetime = (DateTime)row["trxdatetime"];
                    sla.Event_no = (string)row["probcode"];
                    sla.Msg_content = "ejlog_message_" + start.ToString("yyyyMM");
                    InsertEventNo(sla, start.ToString("yyyyMM"));
                    count++;

                }
            }
            catch (Exception e)
            {
                /*Log.Error("insertComLogMissingEjlog Error :" + count + "{" + e + "}");*/
            }
            return count;
        }
        public void InsertEventNo(Sla_emslog data, string YM)
        {
            ConnectSQL_Server db = new ConnectSQL_Server(sqlConnection);
            SqlCommand com = new SqlCommand();
            com.CommandText = string.Format(SqlCommandScript.inseartSla_emslog, YM);
            com.Parameters.AddWithValue("@TERM_ID", data.Term_id);
            com.Parameters.AddWithValue("@START_DATETIME", data.Start_datetime);
            com.Parameters.AddWithValue("@EVENT_NO", data.Event_no);
            com.Parameters.AddWithValue("@ID", data.Id);
            com.Parameters.AddWithValue("@MSG_CONTENT", data.Msg_content);
            com.Parameters.AddWithValue("@UPDATE_BY", data.Update_by);
            bool status = db.insertData(com);
        }
        public IList<Device_info_his> GetComlog_recordNotInsert()
        {

            ConnectSQL_Server db = new ConnectSQL_Server(sqlConnection);
            SqlCommand com = new SqlCommand(SqlCommandScript.selectdevice_info_his_statusUse);

            IList<Device_info_his> list = Device_info_his.mapToList(db.GetDatatable(com));
            return list;
        }
        public  LinkedList<ComLogLost> GetComlog_recordNotInsert(DateTime start, int dateCount, string trem_id)
        {
            ConnectSQL_Server db = new ConnectSQL_Server(sqlConnection);
            LinkedList<string> ComLogList = new LinkedList<string>();
            for (int i = 0; i < dateCount; i++)
            {
                DateTime date = start.AddDays(i);
                ComLogList.AddLast("COM" + date.Year + date.Month.ToString("00") + date.Day.ToString("00"));
            }
            

            LinkedList<ComLogLost> list = new LinkedList<ComLogLost>();
            /* foreach (device_info_his device_Info in list_term_id)
             {*/

            string listComLogCheck = "";
            foreach (string item in ComLogList)
            {
                listComLogCheck += listComLogCheck != "" ? string.Format(",({0})", "'" + trem_id + "_" + item + "'") : string.Format("({0})", "'" + trem_id + "_" + item + "'");
            }
            string sqlScript = string.Format(SqlCommandScript.selectSla_emslog_notInsectDB, listComLogCheck);
            SqlCommand com = new SqlCommand(sqlScript);

            com.Parameters.AddWithValue("@TERM_ID", trem_id);



            var dataComLogLost = db.GetDatatable(com);

            if (dataComLogLost.Rows.Count != 0)
            {
                IList<string> listComLog = new List<string>();
                foreach (DataRow dataList in dataComLogLost.Rows)
                {
                    listComLog.Add(dataList.ItemArray[0].ToString().Split(Convert.ToChar("_"))[1]);
                }
                ComLogLost data = new ComLogLost(trem_id, listComLog);
                list.AddLast(data);
            }
            /*}*/
            return list;
        }
        public IList<Mt_caseflow_record> GetEventList_Mt_caseflow_record(DateTime start, DateTime end, string eventNo)
        {
            ConnectMySQL dbFvMysql = new ConnectMySQL(mySqlConnectionFv);

            MySqlCommand com = new MySqlCommand(SqlCommandScript.selectMt_caseflow_record);
            com.Parameters.AddWithValue("@EVENT_ID", eventNo);
            com.Parameters.AddWithValue("@STARTDATE", start);
            com.Parameters.AddWithValue("@ENDDATE", end);
            IList<Mt_caseflow_record> dt = Mt_caseflow_record.mapToList(dbFvMysql.GetDatatable(com));

            return dt;
        }
        public  IList<Mt_caseflow_record> GetEventList_Mt_caseflow_record_his(DateTime start, DateTime end, string eventNo)
        {
            ConnectMySQL dbFvMysql = new ConnectMySQL(mySqlConnectionFv);

            MySqlCommand com = new MySqlCommand(SqlCommandScript.selectMt_caseflow_record_his);
            com.Parameters.AddWithValue("@EVENT_ID", eventNo);
            com.Parameters.AddWithValue("@STARTDATE", start);
            com.Parameters.AddWithValue("@ENDDATE", end);
            IList<Mt_caseflow_record> list = Mt_caseflow_record.mapToList(dbFvMysql.GetDatatable(com));
            return list;
        }
        public  IList<Mt_caseflow_record> GetEventList_Mt_caseflow_record_his_For_Date(DateTime start, DateTime end, string trem_id)
        {
            ConnectMySQL dbFvMysql = new ConnectMySQL(mySqlConnectionFv);

            MySqlCommand com = new MySqlCommand(SqlCommandScript.selectMt_caseflow_record_his_For_date);
            com.Parameters.AddWithValue("@TERM_ID", trem_id);
            com.Parameters.AddWithValue("@STARTDATE", start);
            com.Parameters.AddWithValue("@ENDDATE", end);
            IList<Mt_caseflow_record> list = Mt_caseflow_record.mapToList(dbFvMysql.GetDatatable(com));
            return list;
        }
        public  IList<Base24_master_errordesc> GetFeelView_base24_master_errordesc()
        {
            ConnectSQL_Server db = new ConnectSQL_Server(sqlConnection);
            SqlCommand com = new SqlCommand(SqlCommandScript.selectBase24_master_errordesc);
            IList<Base24_master_errordesc> list = Base24_master_errordesc.mapToList(db.GetDatatable(com));
            return list;
        }


        class SqlCommandScript
        {

            public static string selectMt_caseflow_record = "SELECT * FROM mt_caseflow_record where EVENT_ID = @EVENT_ID and ((START_TIME between @STARTDATE and @ENDDATE) or (END_TIME between @STARTDATE and @ENDDATE));";
            public static string selectMt_caseflow_record_his_For_date = "SELECT * FROM mt_caseflow_record where TERM_ID = @TERM_ID and ((START_TIME between @STARTDATE and @ENDDATE) or (END_TIME between @STARTDATE and @ENDDATE));";
            public static string selectMt_caseflow_record_his = "SELECT * FROM mt_caseflow_record_his where EVENT_ID = @EVENT_ID and ((START_TIME between @STARTDATE and @ENDDATE) or (END_TIME between @STARTDATE and @ENDDATE));";
            public static string inseartSla_emslog = "INSERT INTO sla_emslog_{0} ( TERM_ID, START_DATETIME, EVENT_NO, ID, MSG_CONTENT, UPDATE_BY) VALUES ( @TERM_ID, @START_DATETIME, @EVENT_NO, @ID, @MSG_CONTENT, @UPDATE_BY)";
            public static string selectBase24_master_errordesc = "SELECT * FROM base24_master_errordesc where FEELVIEW_EVENTNO is not null;";
            public static string selectdevice_info_his_statusUse = "SELECT * FROM device_info_his where STATUS = 'use';";
            public static string selectSla_emslog_notInsectDB = "declare @data table (comLog varchar(53));insert into @data values {0};SELECT comLog FROM @data where comLog not in (SELECT MSG_SOURCE from [dbo].[comlog_record] where TERM_ID = @TERM_ID);";


        }
       
      
       
       

    }
    
}
