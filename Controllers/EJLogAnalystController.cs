using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using OfficeOpenXml;
using Serilog;
using SLA_Management.Commons;
using SLA_Management.Data;
using SLA_Management.Models.EncryptionMoniterModel;
using SLA_Management.Models.LogMonitorModel;
using SLA_Management.Models.OperationModel;
using System;
using System.Data;
using System.Reflection.Metadata;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using static SLA_Management.Data.DBService_EJLoganalyst;
using W = DocumentFormat.OpenXml.Wordprocessing;



namespace SLA_Management.Controllers
{
    public class EJLogAnalystController : Controller
    {
        private IConfiguration _myConfiguration { get; set; }
        private static string sqlReport { get; set; }
        public static List<DeviceEncryption> deviceEncryptions { get; set; }

        public ConnectMySQL connectMySQL { get; set; }
        private List<string> _transactionErrorKeywords;
        private readonly IEJPatternService _patternService;


        public EJLogAnalystController(IConfiguration configuration, IEJPatternService patternService)
        {
            _myConfiguration = configuration;
            sqlReport = configuration.GetValue<string>("ConnectString_MySQL:FullNameConnection");
            if (deviceEncryptions == null)
            {
                deviceEncryptions = new List<DeviceEncryption>();
            }
            
            connectMySQL = new ConnectMySQL(sqlReport);
            _patternService = patternService; 
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> UploadEJFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            string fileName = Path.GetFileName(file.FileName);
            var ejFormatRegex = new Regex(@"^EJ\d{8}\.txt$", RegexOptions.IgnoreCase);

            if (!ejFormatRegex.IsMatch(fileName))
            {
                return BadRequest("Invalid filename. Format must be EJYYYYMMDD.txt");
            }
            string content;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                content = await reader.ReadToEndAsync();
            }
            var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            ProcessCassetteSegments(lines, fileName);
            return Ok("File " + fileName + " processed successfully!");
        }
        private string ExtractGlobalTerminalId(string[] lines)
        {
            foreach (var line in lines)
            {
                // "ATM NUMBER:T045B030BE66G262"  (in SYSTEM STARTUP block)
                var m = Regex.Match(line,
                    @"ATM\s*NUMBER\s*[:=]\s*(\S+)",
                    RegexOptions.IgnoreCase);
                if (m.Success)
                    return m.Groups[1].Value.Trim();

                // "TERMINAL ID: T045B030BE66G262"  (in each transaction)
                var m2 = Regex.Match(line,
                    @"TERMINAL\s*ID\s*[:=]\s*(\S+)",
                    RegexOptions.IgnoreCase);
                if (m2.Success)
                    return m2.Groups[1].Value.Trim();
            }
            return null;
        }
        private void ProcessCassetteSegments(string[] lines, string fileName)
        {
            // pre-scan the whole file for a global Terminal ID 
            string globalTerminalId = ExtractGlobalTerminalId(lines);
            StringBuilder cassetteBuffer = null;
            StringBuilder transactionBuffer = null;
            string lastCassetteBlock = string.Empty;

            bool insideCassette = false;
            bool insideTransaction = false;
            bool waitingForPostTransactionCassette = false;
            bool errorOccurred = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                // START CASSETTE 
                if (line.Contains("STARTCASSETTE INFO", StringComparison.OrdinalIgnoreCase) || line.Contains("================CASSETTE INFO================", StringComparison.OrdinalIgnoreCase))
                {
                    if (insideCassette && cassetteBuffer != null)
                    {
                        lastCassetteBlock = cassetteBuffer.ToString();

                        if (waitingForPostTransactionCassette && transactionBuffer != null)
                        {
                            transactionBuffer.AppendLine(lastCassetteBlock);

                           // var tx = ParseTransaction(transactionBuffer.ToString(), fileName);
                            var tx = ParseTransaction(transactionBuffer.ToString(), fileName, globalTerminalId);

                            if (tx != null)
                            {
                                SaveToDatabase(tx, fileName);
                            }

                            transactionBuffer = null;
                            waitingForPostTransactionCassette = false;
                            insideTransaction = false;
                            errorOccurred = false;
                        }
                    }

                    insideCassette = true;
                    cassetteBuffer = new StringBuilder();
                }

                if (insideCassette)
                {
                    cassetteBuffer.AppendLine(line);

                    if (line.Contains("ENDCASSETTE INFO", StringComparison.OrdinalIgnoreCase) ||
                        (line.Contains("================",StringComparison.OrdinalIgnoreCase) && cassetteBuffer.Length > 100))
                    {
                        insideCassette = false;
                        lastCassetteBlock = cassetteBuffer.ToString();

                        if (waitingForPostTransactionCassette && transactionBuffer != null)
                        {
                            transactionBuffer.AppendLine(lastCassetteBlock);

                            var tx = ParseTransaction(transactionBuffer.ToString(), fileName, globalTerminalId);
                            if (tx != null)
                            {
                                SaveToDatabase(tx, fileName);
                            }

                            transactionBuffer = null;
                            waitingForPostTransactionCassette = false;
                            insideTransaction = false;
                            errorOccurred = false;
                        }
                    }

                    continue;
                }

                // TRANSACTION START
                if (line.Contains("INSERT CARD", StringComparison.OrdinalIgnoreCase) ||
                    line.Contains("PRESS NOCARD KEY", StringComparison.OrdinalIgnoreCase))
                {
                    insideTransaction = true;
                    transactionBuffer = new StringBuilder();

                    if (!string.IsNullOrEmpty(lastCassetteBlock))
                    {
                        transactionBuffer.AppendLine(lastCassetteBlock);
                    }

                    errorOccurred = false;
                }

                if (insideTransaction)
                {
                    transactionBuffer.AppendLine(line);

                    if (IsTransactionError(line))
                    {
                        errorOccurred = true;
                        waitingForPostTransactionCassette = true;
                    }
                }

                if (insideTransaction &&
                (line.Contains("TRANSACTION END", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("<======== TRANSACTION END", StringComparison.OrdinalIgnoreCase)))
                {
                    if (!errorOccurred)
                    {
                        // successful transaction → ignore
                        insideTransaction = false;
                        transactionBuffer = null;
                        waitingForPostTransactionCassette = false;
                        errorOccurred = false;
                        continue;
                    }
                }

                // MACHINE RESTART
                if (insideTransaction && (line.Contains("SYSTEM STARTUP", StringComparison.OrdinalIgnoreCase) ||
                    line.Contains("POWERUP MODE", StringComparison.OrdinalIgnoreCase)))
                {
                    if (insideTransaction && transactionBuffer != null)
                    {
                        errorOccurred = true;
                        waitingForPostTransactionCassette = true;
                    }
                }
            }
        }
        private bool IsTransactionError(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            return _patternService.GetPatterns()
                .Any(p => line.Contains(p, StringComparison.OrdinalIgnoreCase));
        }
    
        private EJTransaction ParseTransaction(string fullText, string fileName, string globalTerminalId = null)
        {	
            var tx = new EJTransaction
            {
                TransactionType = "Withdrawal",      
                TransactionStatus = "FAIL",          
                FullTransaction = fullText,
                TransactionDateTime = GetDateFromEJFileName(fileName)
            };            
            using var reader = new StringReader(fullText);
            // READ TIME FROM FIRST LINE
            string firstLine = reader.ReadLine();

            if (!string.IsNullOrWhiteSpace(firstLine))
            {
                var m = Regex.Match(
                    firstLine,
                    @"^(?<dt>\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}\.\d{3})"
                );

                if (m.Success &&
                    DateTime.TryParseExact(
                        m.Groups["dt"].Value,
                        "yyyy-MM-dd HH:mm:ss.fff",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out var logTime))
                {
                    tx.TransactionTime = TimeOnly.FromDateTime(logTime);
                }
            }
            string line;
            string lastTsn = null;
            string amountEntryField = null;

            while ((line = reader.ReadLine()) != null)
            {
                // ── TERMINAL ID ──────────────────────────────────────────────
                if (string.IsNullOrEmpty(tx.TerminalId))
                {
                    if (line.Contains("TERMINAL ID:", StringComparison.OrdinalIgnoreCase) ||
                        line.Contains("ATM NUMBER", StringComparison.OrdinalIgnoreCase))
                    {
                        var match = Regex.Match(line,
                            @"(?:TERMINAL\s*ID|ATM\s*NUMBER)\s*[:=]?\s*(\S+)",
                            RegexOptions.IgnoreCase);
                        if (match.Success)
                            tx.TerminalId = match.Groups[1].Value.Trim();
                    }
                    else if (line.Contains("EJREPORT:", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = line.Split(';');
                        if (parts.Length > 14)
                        {
                            string termId = parts[14].Trim();
                            if (!string.IsNullOrEmpty(termId))
                                tx.TerminalId = termId;
                        }
                    }
                }

                // ---------------- CARD NUMBER ----------------
                if (string.IsNullOrEmpty(tx.CardNumber) &&
                    line.Contains("Card Number", StringComparison.OrdinalIgnoreCase))
                {
                    var m = Regex.Match(line, @"Card Number\s*:\s*(.+)", RegexOptions.IgnoreCase);

                    if (m.Success)
                    {
                        string extractedCard = m.Groups[1].Value.Trim();
                        if (!string.IsNullOrEmpty(extractedCard))
                        {
                            tx.CardNumber = extractedCard;
                        }
                    }
                }
                // ── SEQUENCE NO ───────────────────────────────────────────────
                // Priority 1: EJREPORT (most reliable)
                if (line.Contains("EJREPORT:", StringComparison.OrdinalIgnoreCase))
                {
                    var m = Regex.Match(line, @"EJREPORT:\s*(\d+)");
                    if (m.Success)
                        tx.SequenceNo = m.Groups[1].Value;
                }
                else
                {
                    // Priority 2: Journal block 
                    var journalMatch = Regex.Match(line,
                        @"^\s+(?<seq>\d{4,6})\s+\d{1,2}:\d{2}:\d{2}\s+\d{2}/\d{2}/\d{2}");
                    if (journalMatch.Success && string.IsNullOrEmpty(tx.SequenceNo))
                        tx.SequenceNo = journalMatch.Groups["seq"].Value;
                }

                // ── NEW: capture [LAST TSN] as fallback (LastTSN + 1 = this seq) ──
                if (string.IsNullOrEmpty(tx.SequenceNo))
                {
                    var tsnMatch = Regex.Match(line,
                        @"\[LAST\s+TSN\]\s*:\s*\[(\d+)\]",
                        RegexOptions.IgnoreCase);
                    if (tsnMatch.Success)
                        lastTsn = tsnMatch.Groups[1].Value;
                }

                // ---------------- AMOUNT ----------------
                if (line.Contains("EJREPORT:", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = line.Split(';');

                    if (parts.Length > 9)
                    {
                        string seqFromLog = parts[0].Split(':').LastOrDefault()?.Trim();

                        if (seqFromLog == tx.SequenceNo)
                        {
                            if (decimal.TryParse(parts[9], out var reportAmt))
                            {
                                tx.Amount = reportAmt;
                            }
                        }
                    }
                }
                // ── AMOUNT ENTRY FIELD (fallback if no EJREPORT & no journal amt) ──
                if (!tx.Amount.HasValue &&
                    line.Contains("[AMOUNT ENTRY FIELD]", StringComparison.OrdinalIgnoreCase))
                {
                    var m = Regex.Match(line, @"\[AMOUNT ENTRY FIELD\]\s*:\s*\[(\d+)\]");
                    if (m.Success && long.TryParse(m.Groups[1].Value, out long rawAmt) && rawAmt > 0)
                        amountEntryField = (rawAmt / 100m).ToString("F2"); // store, apply as last resort
                }


                // ---------------- TRANSACTION TYPE ----------------
                SetTransactionType(line, tx);

                // ---------------- TRANSACTION STATUS ----------------
                if (line.Contains("ERROR", StringComparison.OrdinalIgnoreCase))
                {
                    tx.TransactionStatus = "ERROR";
                }
                else if (line.Contains("SYSTEM STARTUP", StringComparison.OrdinalIgnoreCase) || line.Contains("POWERUP MODE", StringComparison.OrdinalIgnoreCase))
                {
                    tx.TransactionStatus = "UNSUCCESS";
                }

            }
            // Terminal ID fallback: use global (from ATM NUMBER in startup)
            if (string.IsNullOrEmpty(tx.TerminalId) && !string.IsNullOrEmpty(globalTerminalId))
                tx.TerminalId = globalTerminalId;

            // Sequence No fallback: LastTSN + 1
            if (string.IsNullOrEmpty(tx.SequenceNo) && !string.IsNullOrEmpty(lastTsn))
            {
                if (int.TryParse(lastTsn, out int tsn))
                    tx.SequenceNo = (tsn).ToString();
            }
            if (!tx.Amount.HasValue && amountEntryField != null)
                if (decimal.TryParse(amountEntryField,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal entryAmt))
                    tx.Amount = entryAmt;

            return tx;
        }
        private void SetTransactionType(string line, EJTransaction tx)
        {
            foreach (var kv in _patternService.GetTransactionTypes())
            {
                if (line.Contains(kv.Key, StringComparison.OrdinalIgnoreCase))
                {
                    tx.TransactionType = kv.Value;
                    return;
                }
            }
        }

        private DateTime? GetDateFromEJFileName(string fileName)
        {
            var m = Regex.Match(fileName, @"EJ(\d{4})(\d{2})(\d{2})");

            if (!m.Success)
                return null;

            if (int.TryParse(m.Groups[1].Value, out var year) &&
                int.TryParse(m.Groups[2].Value, out var month) &&
                int.TryParse(m.Groups[3].Value, out var day))
            {
                return new DateTime(year, month, day);
            }

            return null;
        }

        
        private void SaveToDatabase(EJTransaction tx, string fileName)
        {
            var connStr = _myConfiguration.GetValue<string>(
                "ConnectString_MySQL:FullNameConnection");
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            using var checkCmd = new MySqlCommand(@"SELECT COUNT(1) FROM EJLogAnalyst 
                        WHERE Terminal_Id = @terminal 
                        AND Transaction_Date = @trnsDate
                        AND Transaction_Time = @trnsTime
                        AND ((Sequence_No = @sequence AND @sequence <> '') OR 
                            ((Sequence_No IS NULL OR Sequence_No = '') AND (@sequence IS NULL OR @sequence = '')))", conn);

            checkCmd.Parameters.AddWithValue("@terminal", tx.TerminalId ?? "");
            checkCmd.Parameters.AddWithValue("@sequence", tx.SequenceNo ?? "");
            checkCmd.Parameters.AddWithValue("@trnsDate", tx.TransactionDateTime ?? (object)DBNull.Value);
            checkCmd.Parameters.Add("@trnsTime", MySqlDbType.Time).Value = tx.TransactionTime?.ToTimeSpan() ?? (object)DBNull.Value;

            long existingCount = Convert.ToInt64(checkCmd.ExecuteScalar());

            // 2. SKIP: If the count is greater than 0, return immediately
            if (existingCount > 0)
            {
                return; // Data already exists, skip insertion
            }            

            using var cmd = new MySqlCommand(@"
            INSERT INTO EJLogAnalyst
            (
                Sequence_No,
                Terminal_Id,
                Transaction_Date,
                Transaction_Type,
                Transaction_Status,
                FullTransaction,
                Amount,
                Card_Number,
                EJ_File_Name,
                Create_Date,
                Create_By,
                Update_Date,
                Update_By,
                Remark,
                Transaction_Time
            )
            VALUES
            (
                @sequence,
                @terminal,
                @trnsDate,
                @type,
                @status,
                @full,
                @amount,
                @card,
                @filename,
                @createdate,
                @createby,
                @updatedate,
                @updateby,
                @remark,
                @trnsTime
            )", conn);

            cmd.Parameters.AddWithValue("@sequence", tx.SequenceNo ?? "");
            cmd.Parameters.AddWithValue("@terminal", tx.TerminalId ?? "");
            cmd.Parameters.AddWithValue("@trnsDate",
                tx.TransactionDateTime ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@type", tx.TransactionType ?? "Unknown");
            cmd.Parameters.AddWithValue("@status", tx.TransactionStatus ?? "");
            cmd.Parameters.AddWithValue("@full", tx.FullTransaction ?? "");
            cmd.Parameters.AddWithValue("@amount",
                tx.Amount.HasValue ? tx.Amount : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@card", tx.CardNumber ?? "");
            cmd.Parameters.AddWithValue("@filename", fileName);
            cmd.Parameters.AddWithValue("@createdate", DateTime.Now);
            cmd.Parameters.AddWithValue("@createby",
                HttpContext.Session.GetString("Username"));
            cmd.Parameters.AddWithValue("@updatedate", DateTime.Now);
            cmd.Parameters.AddWithValue("@updateby",
                HttpContext.Session.GetString("Username"));
            cmd.Parameters.AddWithValue("@remark", "");
            cmd.Parameters.Add("@trnsTime", MySqlDbType.Time).Value =
                tx.TransactionTime.HasValue ? tx.TransactionTime.Value.ToTimeSpan() : (object)DBNull.Value;

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public IActionResult ExportEJ(long Id)
        {
            string terminalId = "";
            DateTime txnDate = DateTime.MinValue;
            string fullTransaction = "";
            string sequenceNo = "";

            using (var conn = new MySqlConnection(sqlReport))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(
                    @"SELECT terminal_id, Transaction_Date, FullTransaction, Sequence_No 
                      FROM EJLogAnalyst 
                       WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", Id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) return NotFound();

                        terminalId = reader["terminal_id"].ToString();
                        txnDate = Convert.ToDateTime(reader["Transaction_Date"]);

                        sequenceNo = reader["Sequence_No"] == DBNull.Value || string.IsNullOrEmpty(reader["Sequence_No"].ToString())
                                     ? "N/A"
                                     : reader["Sequence_No"].ToString();

                        fullTransaction = SanitizeXmlString(reader["FullTransaction"].ToString());
                    }
                }
            }
            var logLines = fullTransaction.Replace("\r\n", "\n").Split('\n');
            List<string> rawMatches = new List<string>();

            foreach (var line in logLines)
            {
                var patterns = GetAllErrorPatternsInLine(line);
                foreach (var p in patterns)
                {
                    if (!rawMatches.Any(m => m.Equals(p, StringComparison.OrdinalIgnoreCase)))
                    {
                        rawMatches.Add(p);
                    }
                }
            }
            var filteredErrors = rawMatches.Where(e =>
                !((e.Equals("ERROR", StringComparison.OrdinalIgnoreCase) ||
                   e.Equals("DEVICE ERROR", StringComparison.OrdinalIgnoreCase))
                   && rawMatches.Count > 1)
            ).ToList();

            string resultHeader = "";
            if (filteredErrors.Count == 0)
            {
                resultHeader = "SUCCESS";
            }
            else
            {
                resultHeader = "FAIL (" + string.Join(", ", filteredErrors.Select((err, index) => $"{index + 1}. {err}")) + ")";
            }

            using var ms = new MemoryStream();

            using (var doc = WordprocessingDocument.Create(
                ms, WordprocessingDocumentType.Document, true))
            {
                var mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new W.Document(new W.Body());
                var body = mainPart.Document.Body;

                W.RunProperties CreateRunProps(string fontSize = "14", bool bold = false, bool underline = false)
                {
                    var props = new W.RunProperties(
                        new W.RunFonts
                        {
                            Ascii = "Cambria",
                            HighAnsi = "Cambria"
                        },
                        new W.FontSize { Val = fontSize }
                    );

                    if (bold)
                        props.Append(new W.Bold());

                    if (underline)
                        props.Append(new W.Underline { Val = W.UnderlineValues.Single });

                    return props;
                }


                void AddLine(string text, bool bold = false, bool underline = false, string lineSpacing = "180")
                {
                    var run = new W.Run(CreateRunProps("14", bold, underline));
                    run.Append(new W.Text(text)
                    {
                        Space = SpaceProcessingModeValues.Preserve
                    });

                    var para = new W.Paragraph(
                        run,
                        new W.ParagraphProperties(
                            new W.SpacingBetweenLines
                            {
                                Before = "0",
                                After = "0",
                                Line = lineSpacing,
                                LineRule = W.LineSpacingRuleValues.Exact
                            }
                        )
                    );

                    body.Append(para);
                }


                AddLine($"Terminal ID : {terminalId}", true);
                AddLine($"Transaction Date : {txnDate:yyyy-MM-dd}", true);
                AddLine($"Sequence No : {sequenceNo}", true);
                AddLine($"Result : {resultHeader}", true);
                AddLine("");
                AddLine("Log Analyst:", bold: true, underline: true);
                AddLine("");

                // ===== FULL TRANSACTION CONTENT =====
                var lines = fullTransaction
                    .Replace("\r\n", "\n")
                    .Split('\n');

                foreach (var line in lines)
                {
                    var run = new W.Run(CreateRunProps("14"));
                    run.Append(new W.Text(line)
                    {
                        Space = SpaceProcessingModeValues.Preserve
                    });

                    var para = new W.Paragraph(
                        run,
                        new W.ParagraphProperties(
                            new W.NoProof(),
                            new W.SpacingBetweenLines { After = "0", Before = "0", Line = "180", LineRule = W.LineSpacingRuleValues.Exact }
                        )
                    );

                    body.Append(para);
                }

                mainPart.Document.Save();
            }
            string finalSeq;

            if (!string.IsNullOrEmpty(sequenceNo) && sequenceNo != "N/A")
            {
              
                finalSeq = sequenceNo;
            }
            else
            {
                finalSeq = "XXXX";
            }

            // 2. Return the file using the finalSeq
            return File(
                ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                $"EJ_{terminalId}_{txnDate:yyyyMMdd}_Seq{finalSeq}.docx"
            );
        } 
        private string SanitizeXmlString(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;           
            return new string(input.Where(ch =>
                (ch == 0x9 || ch == 0xA || ch == 0xD) ||
                (ch >= 0x20 && ch <= 0xD7FF) ||
                (ch >= 0xE000 && ch <= 0xFFFD)
            ).ToArray());
        }
        private List<string> GetAllErrorPatternsInLine(string line)
        {
            var matches = new List<string>();
            foreach (var pattern in _patternService.GetPatterns())
                if (line.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    matches.Add(pattern);
            return matches;
        }

    }
}
