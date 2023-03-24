using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace SLA_Management.Data.TermProbDB
{
    public class DBService
    {

        #region Property
        public string ErrorMessage { get; set; }

        public string ErrorMessDBIns
        {
            get { return _strErrDB; }
            set { _strErrDB = value; }
        }

        #endregion

        #region Local Variable

        private DataAccess _objDb = new DataAccess();
        private string _strErrDB = string.Empty;

        #endregion

        #region Public Functions
  
        public DataTable GetAllMasterProblem()
        {
            DataTable _dt = new DataTable();
            string _sql = string.Empty;
            try
            {
                _sql = "Select * From ejlog_problemmascode where status = '1'";
                _dt = _objDb.GetDtDataNoneParam(_sql);
                return _dt;
            }
            catch (Exception ex)
            { throw ex; }
        }
        #endregion

    }
}
