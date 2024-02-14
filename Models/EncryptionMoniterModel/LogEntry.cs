using Serilog;

namespace SLA_Management.Models.EncryptionMoniterModel
{
    public class LogEntry
    {
        public LogEntry(int row, string log)
        {
            string[] data = log.Split(new char[] { ',' }, 10);
            if (data.Count() < 10)
            {
                Log.Error($"LogEntry Error : {log}");
            }
            Row = row;
            LogName = data[0];
            Timestamp = DateTime.ParseExact(data[1], "yyyy-MM-dd HH.mm.ss.fff", System.Globalization.CultureInfo.InvariantCulture);
            RowID = data[2];
            Version = data[3];
            Program = data[4];
            Num1 = data[5];
            Num2 = data[6];
            Num3 = data[7];
            Event = data[8];
            Message = data[9];
            LogData = log;
        }

        public int Row { get; set; }
        public string LogName { get; set; }
        public DateTime Timestamp { get; set; }
        public string RowID { get; set; }
        public string Version { get; set; }
        //public string LogLevel { get; set; }
        public string Program { get; set; }
        public string Num1 { get; set; }
        public string Num2 { get; set; }
        public string Num3 { get; set; }
        public string Event { get; set; }
        public string Message { get; set; }
        public string LogData { get; set; }

        public override string ToString() => $"{LogName},{Timestamp.ToString("yyyy-MM-dd HH.mm.ss.fff")},{RowID},{Version},{Program},{Num1},{Num2},{Num3},{Event},{Message}";

    }
}
