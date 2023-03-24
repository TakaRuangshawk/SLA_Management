namespace SLA_Management.Models
{
    public class ComLogLost
    {
        private string term_id;
        private IList<string> listComLog;

        public ComLogLost(string term_id, IList<string> listComLog)
        {
            Term_id = term_id;
            ListComLog = listComLog;
            /* Term_id = term_id;
             ListComLog = listComLog;*/
        }

        public string Term_id { get => term_id; set => term_id = value; }
        public IList<string> ListComLog { get => listComLog; set => listComLog = value; }
    }
}
