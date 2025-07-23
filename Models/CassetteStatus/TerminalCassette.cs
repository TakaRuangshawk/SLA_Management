namespace SLA_Management.Models.CassetteStatus
{
    public class TerminalCassette
    {
        public TerminalCassette(string terminalId, List<CassetteBox> cassetteBoxStatuses, DateTime queryDate)
        {
            this.terminalId = terminalId;
            this.cassetteBoxStatuses = cassetteBoxStatuses;
            this.queryDate = queryDate;
        }

        public string terminalId { get; set; }
        public List<CassetteBox> cassetteBoxStatuses { get; set; }
        public DateTime queryDate { get; set; }

    }
}
