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

        public EJLogAnalystController(IConfiguration configuration)
        {
            _myConfiguration = configuration;
            sqlReport = configuration.GetValue<string>("ConnectString_MySQL:FullNameConnection");
            if (deviceEncryptions == null)
            {
                deviceEncryptions = new List<DeviceEncryption>();
            }
            
            connectMySQL = new ConnectMySQL(sqlReport);

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
        
        private void ProcessCassetteSegments(string[] lines, string fileName)
        {
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

                            var tx = ParseTransaction(transactionBuffer.ToString(), fileName);
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

                            var tx = ParseTransaction(transactionBuffer.ToString(), fileName);
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
            return CriticalPatterns.Any(p => line.Contains(p, StringComparison.OrdinalIgnoreCase));
        }
        private static readonly string[] CriticalPatterns = {
                 // General    
                "DISPENSE NOTE FAILED",
                "DISPENSE FAIL",
                "SEND REVERSAL",
                "REVERSAL MESSAGE",
                "NOTE JAM",
                "RETRACT NOTES FAIL",
                "ERROR",
                "KERNEL VERSION:",

                // Cash Acceptor (CIM / Cash-In)
                "CIM ERROR",
                "[CashInEndAsyn] Failed",
                "CRM9250 ERROR",
                "CIM ERR",
                "Cash-In-End Failed",
                "Cash-In-Start Failed",
                "CASHIN START FAILED",
                "CASH ACCEPTOR GETLASTCASHIN FAIL",
                "Have Money In CIM BEFORE Start TXN",
                "No FIT Matched",
                "CashJammed",
                "ERROR",
                "Cash-In Failure",
                "WARN:ROOLBACK SUCCESS, BUT TS(CASH ACCEPTOR) NOT EMPTY",
                "SOME NOTES RETRACTED IN CASH-IN START",

                //Cash Handler / Dispenser (CDM / Cash-Out)
                "CDM ERROR",
                "CDM Init ERROR",
                "CDM8240 ERROR",
                "DISPENSER DELIVER FATAL ERROR",
                "Failed TO CALL DispenseStart",
                "DISPENSE FAILURE AND SEND REVERSAL MESSAGE",
                "ERROR: DISPENSE RESULT ALLOCATION FAILED",
                "ERROR: ALLOT FAILURE AND CAN'T DISPENSE NOTES",
                "REPEAT DISPENSE NOTES",
                "DISPENSE FAIL AND SEND REVERSAL MESSAGE",
                "PRESENT FAIL AND TRY TO RETRACT NOTES",

                //Shutter & Transport

                "Open Shutter Failed",
                "Close Shutter Failed",
                "TAKE NOTES TIMEOUT",
                "RETRACT NOTES TO TRANSPORT",
                "PRESENT NOTES ONE MORE TIME",
                "RETRACT NOTES/MAYBE NOTES BE TAKEN AWAY",
                "RETRACT NOTES FAIL",
                "PRESENT FAIL",
                "NOTES RETRACTED FAILURE",
                "Retract Cash Abnormal",
                "Error: Retract Note",
                "Device Error: Rollback Note Failed",
                "ERROR: CASH ACCEPTOR OUTSHUTTERCONTROL FAIL",
                "DEV ERR:CLOSE SHUTTER FAIL",
                "Cash Shutter is Empty",
                "Error: Get Envelope Device Status Fail",
                "Error Code:Rollback And Reset Failed",
                "Refund To Customer But Timeout Retract",

                //Card Reader & PINPAD (EPP) 
                "CARDREADER ERROR",
                "CARDREADER IS JAMMED",
                "CARDREADER IS POWEROFF",
                "CARDREADER SENSOR/IC ERR",
                "EJECT CARD FAIL",
                "CARD EJECTED FAILED",
                "CARD RETAIN Fail",
                "CARD RETAINED FAILED",
                "CHIPPOWER COLDRESET",
                "EMV Card decline the transaction",
                "TVR",
                "Pinpad Failed",
                "ANTI-SKIMMING WARNING",
                "RKL Expired",
                "CARDREADER IS ERROR",

                //Receipt Printer
                "DEVICE ERROR",
                "RECEIPT PRINTER ERROR",
                "Paper Status Unknown",
                "RECEIPT PAPER OUT",
                "Receipt Paper Jammed",
                "Emergency Receipt Fail",
                "PRINT RECEIPT FORM ERROR",
                "Receipt Printer Failed",
                "Print Receipt Error",
                "RECEIPT PRINTER DEVICE ERROR",

                //Security, UPS & Environment
                "City Power Is Off",
                "Operator Switch Opened",
                "Upper Door Opened",
                "Safe Door Opened",
                "Failed To Parse Expired",
                "Key Dev Is Error",
                "SHUTDOWN",
                "REBOOT MACHINE",

                //Network & Communication
                "NETWORK LOST",
                "ERR: TRANSACTION REPLY MESSAGE INCORRECT",
                "SolicitedStatus  Send : Failed",
                "Interactive Res. Send : Failed",
                "Transaction Req. Send : Failed",
                "*Error",
                "Keypad Timeout",
                "NETWORK DISCONNECTED",
                "TRNSACTION REPLY MESSAGE INCORRECT",

                //Network & Communication
                "BARCODE HWERROR",
                "BarCode Error",
                "BARCODESCANNER NOT CONFIGURED",
                "Camera Error"
            };



        
        private EJTransaction ParseTransaction(string fullText, string fileName)
        {	
            var tx = new EJTransaction
            {
                TransactionType = "Withdrawal",      // default
                TransactionStatus = "FAIL",           // default
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

            while ((line = reader.ReadLine()) != null)
            {
                // ---------------- TERMINAL ID ----------------
                if (string.IsNullOrEmpty(tx.TerminalId))
                {
                    if (line.Contains("TERMINAL ID:", StringComparison.OrdinalIgnoreCase) ||
                        line.Contains("ATM NUMBER", StringComparison.OrdinalIgnoreCase))
                    {
                        var match = Regex.Match(line, @"(?:Terminal\s*Id|ATM\s*NUMBER)\s*[:=]?\s*(\S+)", RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            tx.TerminalId = match.Groups[1].Value.Trim();
                        }
                    }
                    else if (line.Contains("EJREPORT:", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = line.Split(';');
                        if (parts.Length > 14)
                        {
                            string termId = parts[14].Trim();
                            if (!string.IsNullOrEmpty(termId))
                            {
                                tx.TerminalId = termId;
                            }
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
                // ---------------- SEQUENCE NO ----------------                
                // 1. If EJREPORT exists, use it as priority
                if (line.Contains("EJREPORT:", StringComparison.OrdinalIgnoreCase))
                {
                    var m = Regex.Match(line, @"EJREPORT:\s*(\d+)");
                    if (m.Success)
                    {
                        tx.SequenceNo = m.Groups[1].Value;
                    }
                }
                else
                {                   
                    var journalMatch = Regex.Match(line, @"\.\d{3}\s+(?<seq>\d{4,6})\s+\d{2}:\d{2}:\d{2}");

                    if (journalMatch.Success)
                    {
                        tx.SequenceNo = journalMatch.Groups["seq"].Value;
                    }
                }

                // ---------------- AMOUNT ----------------
                if (line.Contains("EJREPORT:", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = line.Split(';');

                    if (parts.Length > 9)
                    {
                        string seqFromLog = parts[0].Split(':').LastOrDefault();

                        if (seqFromLog == tx.SequenceNo)
                        {
                            if (decimal.TryParse(parts[9], out var reportAmt))
                            {
                                tx.Amount = reportAmt;
                            }
                        }
                    }
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

            return tx;
        }

        private static readonly KeyValuePair<string, string>[] TransactionTypeMap =
        {
            // ===== Deposit =====
            new("DEP_AMLO", "Deposit"),
            new("DEP_DCC", "Deposit"),
            new("DEP_P00", "Deposit"),
            new("DEP_P01", "Deposit"),
            new("DEP_DCA", "Deposit"),
            new("DEP", "Deposit"),
            new("RFT_P00", "Deposit"),
            new("RFT_P01", "Deposit"),
            new("RFT_DCA", "Deposit"),

            // ===== Withdrawal =====
            new("CL_WDL", "Withdrawal"),
            new("WDL_CL", "Withdrawal"),
            new("CB_WDL", "Withdrawal"),
            new("MCASH", "Withdrawal"),
            new("FAS", "Withdrawal"),
            new("WDL", "Withdrawal"),

            // ===== Inquiry =====
            new("TRD INQ", "Inquiry"),
            new("ORFT INQ", "Inquiry"),
            new("PROM_INQ", "Inquiry"),
            new("INQ", "Inquiry"),

            // ===== Other =====
            new("PAY_P00", "Other"),
            new("PAY_P01", "Other"),
            new("BAR_P00", "Other"),
            new("QRC CONF", "Other"),
            new("PSC_PCC", "Other"),
            new("PP_BILL", "Other"),
            new("CHGPIN", "Other"),
            new("ANN_FEE", "Other"),
            new("AN-FEE", "Other"),
            new("IPSC", "Other"),
            new("PPCR", "Other"),
            new("ORFT TRF", "Other"),
            new("TRD", "Other"),
            new("TRF", "Other"),
            new("ORFT", "Other"),
            new("PROM_", "Other"),
            new("PAY_", "Other"),
            new("BAR_", "Other")
        };


        private void SetTransactionType(string line, EJTransaction tx)
        {
            foreach (var kv in TransactionTypeMap)
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
            List<string> matches = new List<string>();       
            foreach (var pattern in CriticalPatterns)
            {
                if (line.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    matches.Add(pattern);
                }
            }
            return matches;
        }
      
     
       
    }
}
