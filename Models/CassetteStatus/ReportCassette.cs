namespace SLA_Management.Models.CassetteStatus
{
    public class ReportCassette
    {
        public string cassetteId { get; set; }
        public int cassetteStatusCount { get; set; }
        public string fileData { get; set; }
        public string cassetteStatus { get; set; }

        public ReportCassette(string cassetteId, int cassetteStatusCount, string fileData, string cassetteStatus)
        {
            this.cassetteId = cassetteId;
            this.cassetteStatusCount = cassetteStatusCount;
            this.fileData = fileData;
            this.cassetteStatus = cassetteStatus;
        }
    }
}
