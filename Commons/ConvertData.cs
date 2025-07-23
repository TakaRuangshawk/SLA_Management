using SLA_Management.Models.CassetteStatus;
using System.Globalization;
using System.Text.Json;

namespace SLA_Management.Commons
{
    public class ConvertData
    {
        public static TerminalCassette ConvertToTerminalCassette(string data)
        {
            string[] item = data.Split('|');
            if (item.Length < 2) return null;
            return new TerminalCassette(item[0], ConvertToCassette(item[1]), new DateTime());

        }
        public static TerminalCassette ConvertToTerminalCassetteV2(string data)
        {
            var test = JsonSerializer.Deserialize<TerminalCassetteJsonData>(data);

            if (test == null) return null;
            return new TerminalCassette(test.TERM_ID, ConvertToCassette(test.BOX_STATUS_DETAIL), test.QUERY_DATE);

        }

        public static List<CassetteBox> ConvertToCassette(string data)
        {

            // deserialize JSON เป็น List ของ MyDataModel
            return JsonSerializer.Deserialize<List<CassetteBox>>(data);
        }

        /*public static TerminalCassetteData ConvertToCassetteJson(string data)
        {

            return JsonSerializer.Deserialize<TerminalCassetteData>(data);
        }*/







        public static DateTime ConvertDateTimeFormant(string dateTimeString)
        {

            string[] validformats = new string[] { "yyyy/MM/dd HH:mm:ss", "MM/dd/yyyy HH:mm:ss","yyyy-MM-dd",
                                         "yyyy-MM-dd HH:mm:ss" , "yyyy-MM-dd HH:mm:ss fff","yyyyMMdd HH:mm:ss","yyyy/MM/dd" ,"yyyyMMdd","yyyy-MM-dd"};
            DateTime dateTime = new DateTime();
            CultureInfo provider = new CultureInfo("en-US");
            if (dateTimeString != null && dateTimeString != "")
            {
                try
                {
                    dateTime = DateTime.ParseExact(dateTimeString, validformats, provider, DateTimeStyles.AllowWhiteSpaces);

                }
                catch (Exception ex)
                {



                    Environment.Exit(1);
                }
            }
            else
            {


                Environment.Exit(1);

            }
            return dateTime;

        }
    }
}
