using System.Net;

namespace SLA_Management.Models.TimeSync
{
    public class TimeSyncLogEntry
    {
        public IPAddress Ip { get; init; }
        public DateTime Timestamp { get; init; }
        public string Method { get; init; }
        public string Path { get; init; }
        public string Protocol { get; init; }
        public int Status { get; init; }
        public long? Bytes { get; init; }
    }
}
