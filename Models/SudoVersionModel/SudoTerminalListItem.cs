namespace SLA_Management.Models.OperationModel
{
    public class SudoTerminalSelectionRequest
    {
        public List<string> selectedTerminalIds { get; set; }
    }

    public class SudoTerminalListItem
    {
        public string term_seq { get; set; }
        public string term_id { get; set; }
        public string term_name { get; set; }
        public string version_mb { get; set; }
        public int z_flag { get; set; }
    }
}
