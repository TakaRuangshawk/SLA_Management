using System.Globalization;

namespace SLA_Management
{
    public class Loger
    {

        private readonly static CultureInfo _cultureEnInfo = new CultureInfo("en-US");

        private readonly string strFileNameLog = DateTime.Now.ToString("yyyyMMdd", _cultureEnInfo);   

        private readonly string _FolderProLog = Directory.GetCurrentDirectory() + "\\";

        #region private function
        private static void CheckExitAndCreateDirectoryFolder(string folder_nameTemp)
        {

            if (!Directory.Exists(folder_nameTemp))
            {
                Directory.CreateDirectory(folder_nameTemp);
            }

        }

        private static void WriteLogFiles(string file_nameTemp, string messageTemp)
        {
            StreamWriter? sw = null;
            FileStream? fs = null;
            try
            {
                fs = new FileStream(file_nameTemp, FileMode.Append, FileAccess.Write, FileShare.Write);
                using (sw = new StreamWriter(fs))
                {
                    sw.WriteLine(DateTime.Now.ToString("dd/MM/yyyy", _cultureEnInfo) + " " + DateTime.Now.ToString("HH:mm:ss.fff") + " : " + messageTemp);
                    sw.Flush();
                    sw.Close();
                }
                fs.Close();
            }         
            finally
            {
                if (sw != null) sw.Close();
                if (fs != null) fs.Close();
            }
        }

        #endregion

        public void WriteLogFile(string message)
        {
            string strDefPath = _FolderProLog;
            string folder_name = Path.Combine(strDefPath, "Log\\LogFile");
            string file_name = Path.Combine(folder_name, "LogFile_" + strFileNameLog + ".txt");

            CheckExitAndCreateDirectoryFolder(folder_name);
            WriteLogFiles(file_name, message);


        }

        public  void WriteErrLog(string message)
        {
            string strDefPath = _FolderProLog ;
            string folder_name = Path.Combine(strDefPath, "Log\\LogError");
            string file_name = Path.Combine(folder_name, "LogErr_" + strFileNameLog + ".txt");
            CheckExitAndCreateDirectoryFolder(folder_name);
            WriteLogFiles(file_name, message);
        }


    }
}
