namespace SLA_Management.Models.CassetteStatus
{
    public class ReportTerminalCassetteDB
    {
        public string Id { get; set; }
        public string TermId { get; set; }
        public string Cassette_Id { get; set; }
        public string Cassette_Status { get; set; }
        public int Cassette_Remain { get; set; }
        public string Cassette_Event_File_Id { get; set; }



    }
}
