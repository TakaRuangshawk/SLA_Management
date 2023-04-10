namespace SLA_Management.Models
{
    public class UploadFileFrom
    {
        public string targetFile { get; set; }
        public string pathUpload { get; set; }

        public UploadFileFrom(string targetFile, string pathUpload)
        {
            this.targetFile = targetFile;
            this.pathUpload = pathUpload;
        }
    }
}
