﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace SLA_Management.Models.OperationModel
{
    [Table("sla_tracking")]
    public class TrackingDetail
    {

        public int ID { get; set; }

        public string APPNAME { get; set; }

        public string UPDATE_DATE { get; set; }

        public string STATUS { get; set; }

        public string REMARK { get; set; }

        public string USER_IP { get; set; }

        public static IList<TrackingDetail> mapToList(DataTable list)
        {
            IList<TrackingDetail> items = list.AsEnumerable().Select(row => new TrackingDetail
            {
                //AcctNoTo = row.Field<string>("AcctNoTo") == null ? null : hide(row.Field<string>("AcctNoTo")),
                ID = row.Field<int>("ID") == null ? 0 : row.Field<int>("ID"),
                APPNAME = row.Field<string>("APPNAME") == null ? null : row.Field<string>("APPNAME"),
                UPDATE_DATE = row.Field<DateTime?>("UPDATE_DATE") == null ? null : chng_trandate(row.Field<DateTime>("UPDATE_DATE")),
                STATUS = row.Field<string>("STATUS") == null ? null : row.Field<string>("STATUS"),
                REMARK = row.Field<string>("REMARK") == null ? null : row.Field<string>("REMARK"),
                USER_IP = row.Field<string>("USER_IP") == null ? null : row.Field<string>("USER_IP")


                //String.Format("{0:n}", Convert.ToInt64(Math.Floor(Convert.ToDouble(row.Field<double>("Amount")))))
            }).ToList();
            return items;
        }

        private static string chng_trandate(DateTime dt)
        {
            string trandate = dt.ToString("yyyy-MM-dd HH:mm:ss");
            return trandate;
        }







    }
}
