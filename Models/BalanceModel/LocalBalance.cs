namespace SLA_Management.Models.BalanceModel
{
    public class LocalBalance
    {
        public string BALANCE_ID { get; set; }
        public string TERM_ID { get; set; }
        public string SERIAL_NUMBER { get; set; }
        public DateTime BALANCING_DATE { get; set; }

        public int INITIAL_TYPE1000AQTY { get; set; }
        public int INITIAL_TYPE1000BQTY { get; set; }
        public int INITIAL_TYPE1000CQTY { get; set; }
        public int INITIAL_TYPE1000DQTY { get; set; }
        public int INITIAL_TYPE1000QTY { get; set; }
        public decimal INITIAL_TYPE1000AMOUNT { get; set; }

        public int INITIAL_TYPE500AQTY { get; set; }
        public int INITIAL_TYPE500BQTY { get; set; }
        public int INITIAL_TYPE500CQTY { get; set; }
        public int INITIAL_TYPE500DQTY { get; set; }
        public int INITIAL_TYPE500QTY { get; set; }
        public decimal INITIAL_TYPE500AMOUNT { get; set; }

        public int INITIAL_TYPE100AQTY { get; set; }
        public int INITIAL_TYPE100BQTY { get; set; }
        public int INITIAL_TYPE100CQTY { get; set; }
        public int INITIAL_TYPE100DQTY { get; set; }
        public int INITIAL_TYPE100QTY { get; set; }
        public decimal INITIAL_TYPE100AMOUNT { get; set; }

        public int DEPOSIT_TYPE1000QTY { get; set; }
        public decimal DEPOSIT_TYPE1000AMOUNT { get; set; }
        public int DEPOSIT_TYPE500QTY { get; set; }
        public decimal DEPOSIT_TYPE500AMOUNT { get; set; }
        public int DEPOSIT_TYPE100QTY { get; set; }
        public decimal DEPOSIT_TYPE100AMOUNT { get; set; }

        public int WITHDRAW_TYPE1000QTY { get; set; }
        public decimal WITHDRAW_TYPE1000AMOUNT { get; set; }
        public int WITHDRAW_TYPE500QTY { get; set; }
        public decimal WITHDRAW_TYPE500AMOUNT { get; set; }
        public int WITHDRAW_TYPE100QTY { get; set; }
        public decimal WITHDRAW_TYPE100AMOUNT { get; set; }

        public int BALANCE_TYPE1000QTY { get; set; }
        public decimal BALANCE_TYPE1000AMOUNT { get; set; }
        public int BALANCE_TYPE500QTY { get; set; }
        public decimal BALANCE_TYPE500AMOUNT { get; set; }
        public int BALANCE_TYPE100QTY { get; set; }
        public decimal BALANCE_TYPE100AMOUNT { get; set; }

        public int RETRACT_TYPE1000QTY { get; set; }
        public decimal RETRACT_TYPE1000AMOUNT { get; set; }
        public int RETRACT_TYPE500QTY { get; set; }
        public decimal RETRACT_TYPE500AMOUNT { get; set; }
        public int RETRACT_TYPE100QTY { get; set; }
        public decimal RETRACT_TYPE100AMOUNT { get; set; }
        public decimal RETRACT_TYPE_UNKNOWN_AMOUNT { get; set; }

        public int REJECT_TYPE1000QTY { get; set; }
        public decimal REJECT_TYPE1000AMOUNT { get; set; }
        public int REJECT_TYPE500QTY { get; set; }
        public decimal REJECT_TYPE500AMOUNT { get; set; }
        public int REJECT_TYPE100QTY { get; set; }
        public decimal REJECT_TYPE100AMOUNT { get; set; }
    }

}
