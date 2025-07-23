namespace SLA_Management.Models.CassetteStatus
{
    public class ReportTerminalCassette
    {
        public string termId { get; set; }
        public string cassetteId { get; set; }
        public string cassetteStatus { get; set; }
        public int cassetteRemain { get; set; }
        public string fileData { get; set; }


        public string CassetteDescription()
        {
            string result = cassetteStatus;

            switch (cassetteStatus)
            {
                case "0":
                    result = "Cassette Normal";
                    break;
                case "1":
                    result = "Cash Low";
                    break;
                case "2":
                    result = "Cash Out";
                    break;
                case "9":
                    result = "Cassette Fault";
                    break;
                default:
                    result = cassetteStatus;
                    break;
            }
            return result;
        }





    }
}
