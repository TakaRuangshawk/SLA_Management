using SLA_Management.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

public class CavitySnapshotRepository
{
    private readonly string _csvPath;
    private static List<CavityMonitorModel> _cache;
    private static readonly object _lock = new object();

    public CavitySnapshotRepository(string csvPath)
    {
        _csvPath = csvPath;
    }

    public List<CavityMonitorModel> GetAll()
    {
        if (_cache != null)
            return _cache;

        lock (_lock)
        {
            if (_cache != null)
                return _cache;

            var list = new List<CavityMonitorModel>();

            if (!File.Exists(_csvPath))
                return list;

            using (var reader = new StreamReader(_csvPath, Encoding.UTF8))
            {
                string line;
                bool isHeader = true;

                while ((line = reader.ReadLine()) != null)
                {
                    if (isHeader)
                    {
                        isHeader = false;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = SplitCsvLine(line);
                    if (parts.Count < 6)
                        continue;

                    int no;
                    int cavity;

                    int.TryParse(parts[0], out no);
                    int.TryParse(parts[5], out cavity);

                    // *** FIELD ใหม่ ***
                    bool xdc_has_4199 = false;
                    DateTime? nvTime = null;
                    string nvVersion = null;
                    string mainVersion = null;

                    // xdc_has_4199 (index 6)
                    if (parts.Count > 6)
                    {
                        string flag = parts[6]?.Trim();
                        xdc_has_4199 =
                            flag == "1" ||
                            flag.Equals("true", StringComparison.OrdinalIgnoreCase);
                    }

                    // nv_log_time (index 7)
                    if (parts.Count > 7)
                    {
                        string nvTimeStr = parts[7]?.Trim();
                        if (!string.IsNullOrEmpty(nvTimeStr))
                        {
                            DateTime parsed;
                            if (DateTime.TryParseExact(
                                nvTimeStr,
                                "yyyy-MM-dd HH:mm:ss",
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out parsed))
                            {
                                nvTime = parsed;
                            }
                        }
                    }

                    // nv_version (index 8)
                    if (parts.Count > 8)
                    {
                        nvVersion = parts[8]?.Trim();
                    }

                    if (parts.Count > 9)
                    {
                        mainVersion = parts[9]?.Trim();
                    }

                    list.Add(new CavityMonitorModel
                    {
                        no = no,
                        term_seq = parts[1]?.Trim(),
                        term_id = parts[2]?.Trim(),
                        term_name = parts[3]?.Trim(),
                        term_type = parts[4]?.Trim(),
                        cavity_note = cavity,
                        xdc_has_4199 = xdc_has_4199,     // *** ใช้ชื่อใหม่ที่ถูกต้อง ***
                        nv_log_time = nvTime,
                        nv_version = nvVersion,
                        main_version = mainVersion
                    });
                }
            }

            _cache = list;
            return _cache;
        }
    }


    // ================= CSV Parser รองรับ comma + quote ===================
    private static List<string> SplitCsvLine(string line)
    {
        var result = new List<string>();
        if (line == null) return result;

        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '\"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                {
                    sb.Append('\"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }

        result.Add(sb.ToString());
        return result;
    }
}

