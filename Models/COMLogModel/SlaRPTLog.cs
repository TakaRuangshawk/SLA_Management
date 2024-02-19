using System.Globalization;

namespace SLA_Management.Models.COMLogModel
{
    public class SlaRPTLog
    {
        public SlaRPTLog(string tERM_ID, string eVENT_NO, string date, string time)
        {
            TERM_ID = tERM_ID;
            START_DATETIME = ConvertDateTimeFormant(string.Format("{0} {1}", date, time));
            EVENT_NO = eVENT_NO;
            UPDATE_BY = "GSB_EMSLog";
        }

        public int EMSID { get; set; }
        public string TERM_ID { get; set; }
        public DateTime START_DATETIME { get; set; }
        public string EVENT_NO { get; set; }
        public string ID { get; set; }
        public string MSG_CONTENT { get; set; }
        public string UPDATE_BY { get; set; }
        public string REMARK { get; set; }

        private DateTime ConvertDateTimeFormant(string dateTimeString)
        {
            string[] validformats = new string[] { "MM/dd/yyyy HH:mm:ss", "yyyy/MM/dd HH:mm:ss", "MM/dd/yyyy HH:mm:ss",
                                        "MM/dd/yyyy HH:mm:ss", "yyyy-MM-dd HH:mm:ss","yyyyMMdd HH:mm:ss" };

            CultureInfo provider = new CultureInfo("en-US");
            try
            {
                DateTime dateTime = DateTime.ParseExact(dateTimeString, validformats, provider, DateTimeStyles.AllowWhiteSpaces);
                return dateTime;
            }
            catch (Exception ex)
            {

                throw ex;
               

            }






        }
    }
}
