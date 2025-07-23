namespace SLA_Management.Models.TermProbModel
{
    public class report_cassette
    {
        public string Id { get; set; }
        public string Cassette_Id { get; set; }

        public int Cassette_Status_Count { get; set; }

        public string Cassette_Status { get; set; }

        public string Cassette_Event_File_Id { get; set; }

    }
}
