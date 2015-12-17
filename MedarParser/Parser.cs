using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MedarParser.Model;

namespace MedarParser
{
    public static class Parser
    {
        #region Parser Methods

        public static async Task<IEnumerable<Letter>> ParseLetter(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            using (var reader = new StringReader(text))
            {
                return await ParseLetter(reader);
            }
        }

        public static async Task<IEnumerable<Letter>> ParseLetter(TextReader reader)
        {
            var letters = new List<Letter>();
            var lineNumber = 1;

            var line = await reader.ReadLineAsync();
            do
            {
                if (line != null)
                    letters.Add(await ParseLetterBlock(reader, () => lineNumber, (ln) => lineNumber = ln, line));

                lineNumber++;
            }
            while ((line = await reader.ReadLineAsync()) != null);

            return letters;
        }

        public static async Task<IEnumerable<Labo>> ParseLabo(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            using (var reader = new StringReader(text))
            {
                return await ParseLabo(reader);
            }
        }

        public static async Task<IEnumerable<Labo>> ParseLabo(TextReader reader)
        {
            var labos = new List<Labo>();
            var lineNumber = 1;

            var line = await reader.ReadLineAsync();
            var previousLineNumber = 0;
            string previousLaboCode = null;
            Labo labo = null;
            do
            {
                var currentLineNumber = LineNumber(line, previousLineNumber, labo.ParserErrors);
                if (currentLineNumber == 9999)
                    break;
                if (currentLineNumber != (previousLineNumber + 1))
                    labo?.ParserErrors.AddItem(lineNumber, $"Lines not in sequence: line {previousLineNumber} followed by line {currentLineNumber}: {line}");
                previousLineNumber = currentLineNumber;

                var currentLaboCode = LaboCode(line, lineNumber, labo.ParserErrors);
                if (currentLaboCode != previousLaboCode)
                {
                    labo = new Labo() { Code = currentLaboCode };
                    labos.Add(labo);
                    previousLaboCode = currentLaboCode;
                }

                var recordDescriptor = LaboRecordDescriptor(line, lineNumber, labo.ParserErrors);
                var recordParts = LaboRecordParts(line, lineNumber, labo.ParserErrors);
                ParseLaboRecord(recordDescriptor, labo, recordParts, lineNumber);

                lineNumber++;
            }
            while ((line = await reader.ReadLineAsync()) != null && !line.StartsWith("END"));

            return labos;
        }

        #endregion

        #region Labo Parser Methods

        static void ParseLaboRecord(string recordDescriptor, Labo labo, IList<string> recordParts, int lineNumber)
        {
            switch (recordDescriptor)
            {
                case "S1":
                    ParseLaboS1(recordParts, labo, lineNumber);
                    break;
                case "S2":
                    ParseLaboS2(recordParts, labo, lineNumber);
                    break;
                case "S3":
                    ParseLaboS3(recordParts, labo, lineNumber);
                    break;
                case "S4":
                    ParseLaboS4(recordParts, labo, lineNumber);
                    break;
                case "S5":
                    ParseLaboS5(recordParts, labo, lineNumber);
                    break;
                case "S6":
                    ParseLaboS6(recordParts, labo, lineNumber);
                    break;
                case "C1":
                    ParseLaboC1(recordParts, labo, lineNumber);
                    break;
                case "A1":
                    ParseLaboA1(recordParts, labo, lineNumber);
                    break;
                case "D1":
                    ParseLaboD1(recordParts, labo, lineNumber);
                    break;
                case "R1":
                    ParseLaboR1(recordParts, labo, lineNumber);
                    break;
                default:
                    labo.ParserErrors.AddItem(lineNumber, $"Unknown record descriptor '{recordDescriptor}'");
                    break;
            }
        }

        static void ParseLaboS1(IList<string> recordParts, Labo labo, int lineNumber)
        {
            if (recordParts.Count != 2)
                labo.ParserErrors.AddItem(lineNumber, $"Expected 2 parts in S1 but got {recordParts.Count} parts: '{string.Join("\\", recordParts)}'");

            labo.Code = recordParts.ElementAtOrDefault(0);
            if (labo.Patient == null)
                labo.Patient = new LaboPatient();
            labo.Patient.Key = recordParts.ElementAtOrDefault(1);
        }

        static void ParseLaboS2(IList<string> recordParts, Labo labo, int lineNumber)
        {
            if (recordParts.Count != 2)
                labo.ParserErrors.AddItem(lineNumber, $"Expected 2 parts in S2 but got {recordParts.Count} parts: '{string.Join("\\", recordParts)}'");

            if (labo.Patient == null)
                labo.Patient = new LaboPatient();
            labo.Patient.LastName = recordParts.ElementAtOrDefault(0);
            labo.Patient.FirstName = recordParts.ElementAtOrDefault(1);
        }

        static void ParseLaboS3(IList<string> recordParts, Labo labo, int lineNumber)
        {
            if (recordParts.Count != 2)
                labo.ParserErrors.AddItem(lineNumber, $"Expected 2 parts in S3 but got {recordParts.Count} parts: '{string.Join("\\", recordParts)}'");

            if (labo.Patient == null)
                labo.Patient = new LaboPatient();
            labo.Patient.Street = recordParts.ElementAtOrDefault(0);
            labo.Patient.PostalName = recordParts.ElementAtOrDefault(1);
        }

        static void ParseLaboS4(IList<string> recordParts, Labo labo, int lineNumber)
        {
            if (recordParts.Count != 2)
                labo.ParserErrors.AddItem(lineNumber, $"Expected 2 parts in S4 but got {recordParts.Count} parts: '{string.Join("\\", recordParts)}'");

            if (labo.Patient == null)
                labo.Patient = new LaboPatient();
            labo.Patient.BirthDate = recordParts.ElementAtOrDefault(0)?.ToNullableDatetime("dd/MM/yyyy");

            switch (recordParts.ElementAtOrDefault(1))
            {
                case "F":
                case "V":
                    labo.Patient.Sex = Sex.Female;
                    break;
                case "M":
                    labo.Patient.Sex = Sex.Male;
                    break;
                default:
                    labo.Patient.Sex = Sex.Unknown;
                    break;
            }
        }

        static void ParseLaboS5(IList<string> recordParts, Labo labo, int lineNumber)
        {
            if (recordParts.Count != 5)
                labo.ParserErrors.AddItem(lineNumber, $"Expected 5 parts in S5 but got {recordParts.Count} parts: '{string.Join("\\", recordParts)}'");

            if (labo.Mutuality == null)
                labo.Mutuality = new Mutuality();
            labo.Mutuality.Name = recordParts.ElementAtOrDefault(0);
            labo.Mutuality.MemberNumber = recordParts.ElementAtOrDefault(1);
            labo.Mutuality.Kg1Kg2 = recordParts.ElementAtOrDefault(2);
            labo.Mutuality.Relationship = recordParts.ElementAtOrDefault(3);
            labo.Mutuality.Holder = recordParts.ElementAtOrDefault(4);
        }

        static void ParseLaboS6(IList<string> recordParts, Labo labo, int lineNumber)
        {
            if (recordParts.Count != 2)
                labo.ParserErrors.AddItem(lineNumber, $"Expected 2 parts in S6 but got {recordParts.Count} parts: '{string.Join("\\", recordParts)}'");

            if (labo.Patient == null)
                labo.Patient = new LaboPatient();
            labo.Patient.UniquePatientNumber = recordParts.ElementAtOrDefault(0);
            labo.Patient.UniqueMedicalRecordNumber = recordParts.ElementAtOrDefault(1);
        }

        static void ParseLaboC1(IList<string> recordParts, Labo labo, int lineNumber)
        {
            if (recordParts.Count != 2)
                labo.ParserErrors.AddItem(lineNumber, $"Expected 4 parts in C1 but got {recordParts.Count} parts: '{string.Join("\\", recordParts)}'");

            if (labo.Contact == null)
                labo.Contact = new Contact();
            labo.Contact.Number = recordParts.ElementAtOrDefault(0);

            switch (recordParts.ElementAtOrDefault(1))
            {
                case "01":
                case "1":
                    labo.Contact.Type = ContactType.Consultation;
                    break;
                case "02":
                case "2":
                    labo.Contact.Type = ContactType.Hospitalization;
                    break;
                default:
                    labo.Contact.Type = ContactType.Unknown;
                    break;
            }
            labo.Contact.StartDate = recordParts.ElementAtOrDefault(2)?.ToNullableDatetime("dd/MM/yyyy");
            labo.Contact.EndDate = recordParts.ElementAtOrDefault(3)?.ToNullableDatetime("dd/MM/yyyy");
        }

        static void ParseLaboA1(IList<string> recordParts, Labo labo, int lineNumber)
        {
            if (recordParts.Count != 1)
                labo.ParserErrors.AddItem(lineNumber, $"Expected 1 parts in A1 but got {recordParts.Count} parts: '{string.Join("\\", recordParts)}'");

            labo.ArrivalDate = recordParts.ElementAtOrDefault(2)?.ToNullableDatetime("dd/MM/yyyy");
        }

        static void ParseLaboD1(IList<string> recordParts, Labo labo, int lineNumber)
        {
            if (recordParts.Count != 2)
                labo.ParserErrors.AddItem(lineNumber, $"Expected 2 parts in D1 but got {recordParts.Count} parts: '{string.Join("\\", recordParts)}'");

            if (labo.Doctor == null)
                labo.Doctor = new Doctor();
            labo.Doctor.LastName = recordParts.ElementAtOrDefault(0);
            labo.Doctor.FirstName = recordParts.ElementAtOrDefault(1);
        }

        static void ParseLaboR1(IList<string> recordParts, Labo labo, int lineNumber)
        {
            if (recordParts.Count != 5)
                labo.ParserErrors.AddItem(lineNumber, $"Expected 5 parts in R1 but got {recordParts.Count} parts: '{string.Join("\\", recordParts)}'");

            var code = recordParts.ElementAtOrDefault(0)?.Trim();
            var description = recordParts.ElementAtOrDefault(1)?.Trim();
            if (code.IsNullOrEmpty())
            {
                if (description.IsNullOrEmpty()) //TITLE
                {
                    var resultGroup = new LaboResultGroup();
                    resultGroup.Title = recordParts.ElementAtOrDefault(4);
                    labo.ResultGroups.Add(resultGroup);
                }
                else //COMMENT ON PREVIOUS ANALYSIS
                {
                    var lastResultGroup = labo.ResultGroups.LastOrDefault();
                    if (lastResultGroup == null)
                    {
                        labo.ParserErrors.AddItem(lineNumber, $"No result group found: '{string.Join("\\", recordParts)}'");
                        lastResultGroup = new LaboResultGroup();
                        labo.ResultGroups.Add(lastResultGroup);
                    }

                    var previousAnalysis = lastResultGroup.Results.LastOrDefault();
                    if (previousAnalysis == null)
                        labo.ParserErrors.AddItem(lineNumber, $"No previous comment found to add coment to: '{string.Join("\\", recordParts)}'");
                    else
                        string.Concat(previousAnalysis.Comment, previousAnalysis.Comment.IsNullOrEmpty() ? null : Environment.NewLine, recordParts.ElementAtOrDefault(4));
                }
            }
            else
            {
                if (description.IsNullOrEmpty()) //SPECIFIC COMMENT TO REFERENCED ANALYSIS
                {

                }
                else //NORMAL ANALYSIS
                {
                    var lastResultGroup = labo.ResultGroups.LastOrDefault();
                    if (lastResultGroup == null)
                    {
                        labo.ParserErrors.AddItem(lineNumber, $"No result group found: '{string.Join("\\", recordParts)}'");
                        lastResultGroup = new LaboResultGroup();
                        labo.ResultGroups.Add(lastResultGroup);
                    }

                    var result = new LaboResult();
                    lastResultGroup.Results.Add(result);
                    result.Code = code;
                    result.Description = description;
                    result.Boundaries = recordParts.ElementAtOrDefault(2);
                    result.Units = recordParts.ElementAtOrDefault(3);
                    result.Result = recordParts.ElementAtOrDefault(4);
                }
            }
        }

        #endregion

        #region Letter Parser Methods

        static async Task<Letter> ParseLetterBlock(TextReader reader, Func<int> lineNumberGetter, Action<int> lineNumberSetter, string line)
        {
            var letter = new Letter();
            var lineNumber = lineNumberGetter();

            letter.Header = await ParseHeaderBlock(reader, () => lineNumber, (ln) => lineNumber = ln, line, letter.ParserErrors);

            lineNumber++;
            line = await reader.ReadLineAsync();

            if (line == null)
                letter.ParserErrors.AddItem(lineNumber, "No body block defined");
            else
                letter.Body = await ParseBodyBlock(reader, () => lineNumber, (ln) => lineNumber = ln, line);

            lineNumberSetter(lineNumber);
            return letter;
        }

        static async Task<Header> ParseHeaderBlock(TextReader reader, Func<int> lineNumberGetter, Action<int> lineNumberSetter, string line, IDictionary<int, IList<string>> parserErrors)
        {
            var header = new Header();
            var lineNumber = lineNumberGetter();

            header.From = ParseFrom(line, lineNumber, parserErrors);

            lineNumber++;
            line = await reader.ReadLineAsync();
            header.To = ParseFrom(line, lineNumber, parserErrors);

            lineNumber++;
            line = await reader.ReadLineAsync();
            header.Subject = ParseSubject(line, lineNumber, parserErrors);

            lineNumber++;
            line = await reader.ReadLineAsync();
            header.Info = ParseInfo(line, lineNumber, parserErrors);

            lineNumberSetter(lineNumber);
            return header;
        }

        static Person ParseFrom(string line, int lineNumber, IDictionary<int, IList<string>> parserErrors)
        {
            var person = new Person();

            if (line == null || line.Length < 12)
                return null;

            var lineParts = line?.Substring(11).Split('|').ToList();
            if (lineParts.Count != 6)
                parserErrors.AddItem(lineNumber, $"Expected 6 parts in /FROM but got {lineParts.Count} parts: '{line}'");

            person.Name = lineParts.ElementAtOrDefault(0);
            person.Street = lineParts.ElementAtOrDefault(1);
            person.PostalCode = lineParts.ElementAtOrDefault(2);
            person.PostalName = lineParts.ElementAtOrDefault(3);
            person.Phone = lineParts.ElementAtOrDefault(4);
            person.RizivNr = lineParts.ElementAtOrDefault(5);

            for (int i = 6; i < lineParts.Count; i++)
                person.UnknownParts.Add(lineParts[i]);

            return person;
        }

        static Person ParseTo(string line, int lineNumber, IDictionary<int, IList<string>> parserErrors)
        {
            var person = new Person();

            if (line == null || line.Length < 12)
                return null;

            var lineParts = line?.Substring(11).Split('|').ToList();
            if (lineParts.Count != 6)
                parserErrors.AddItem(lineNumber, $"Expected 6 parts in /TO but got {lineParts.Count} parts: '{line}'");

            person.Name = lineParts.ElementAtOrDefault(0);
            person.Street = lineParts.ElementAtOrDefault(1);
            person.PostalCode = lineParts.ElementAtOrDefault(2);
            person.PostalName = lineParts.ElementAtOrDefault(3);
            person.Phone = lineParts.ElementAtOrDefault(4);
            person.RizivNr = lineParts.ElementAtOrDefault(5);

            for (int i = 6; i < lineParts.Count; i++)
                person.UnknownParts.Add(lineParts[i]);

            return person;
        }

        static LetterPatient ParseSubject(string line, int lineNumber, IDictionary<int, IList<string>> parserErrors)
        {
            var patient = new LetterPatient();

            if (line == null || line.Length < 12)
                return null;

            var lineParts = line?.Substring(11).Split('|').ToList();
            if (lineParts.Count != 7)
                parserErrors.AddItem(lineNumber, $"Expected 7 parts in /SUBJECT but got {lineParts.Count} parts: '{line}'");

            patient.FirstName = lineParts.ElementAtOrDefault(0);
            patient.LastName = lineParts.ElementAtOrDefault(1);
            patient.Street = lineParts.ElementAtOrDefault(2);
            patient.PostalCode = lineParts.ElementAtOrDefault(3);
            patient.PostalName = lineParts.ElementAtOrDefault(4);
            patient.BirthDate = lineParts.ElementAtOrDefault(5)?.ToNullableDatetime("dd/MM/yyyy");

            switch (lineParts.ElementAtOrDefault(6))
            {
                case "F":
                case "V":
                    patient.Sex = Sex.Female;
                    break;
                case "M":
                    patient.Sex = Sex.Male;
                    break;
                default:
                    patient.Sex = Sex.Unknown;
                    break;
            }

            for (int i = 7; i < lineParts.Count; i++)
                patient.UnknownParts.Add(lineParts[i]);

            return patient;
        }

        static Info ParseInfo(string line, int lineNumber, IDictionary<int, IList<string>> parserErrors)
        {
            var info = new Info();

            if (line == null || line.Length < 12)
                return null;

            var lineParts = line?.Substring(11).Split('|').ToList();
            if (lineParts.Count != 2)
                parserErrors.AddItem(lineNumber, $"Expected 2 parts in /INFO but got {lineParts.Count} parts: '{line}'");

            info.ReferenceNumber = lineParts.ElementAtOrDefault(0);
            info.Date = lineParts.ElementAtOrDefault(1);

            for (int i = 2; i < lineParts.Count; i++)
                info.UnknownParts.Add(lineParts[i]);

            return info;
        }

        static async Task<Body> ParseBodyBlock(TextReader reader, Func<int> lineNumberGetter, Action<int> lineNumberSetter, string line)
        {
            var body = new Body();
            var sbText = new StringBuilder();
            var lineNumber = lineNumberGetter();

            do
            {
                begin:
                if (line != null && line.StartsWith("/TITLE"))
                    body.Title = line.Substring("/TITLE".Length + 1);
                else if (line != null && line.StartsWith("/DATE"))
                    body.Date = line.Substring("/DATE".Length + 1).ToNullableDatetime("dd/M/yy", "dd/MM/yy", "dd/MM/yyyy");
                else if (line != null && line.StartsWith("/EXAM"))
                {
                    body.Exams.Add(await ParseExam(reader, () => lineNumber, (ln) => lineNumber = ln, () => line, (l) => line = l));
                    goto begin;
                }
                else if (line != null && line.StartsWith("/END"))
                    continue;
                else
                    sbText.AppendLine(line);

                lineNumber++;
            }
            while ((line = await reader.ReadLineAsync()) != null);

            body.Text = sbText.ToString();

            lineNumberSetter(lineNumber);
            return body;
        }

        static async Task<Exam> ParseExam(TextReader reader, Func<int> lineNumberGetter, Action<int> lineNumberSetter, Func<string> lineGetter, Action<string> lineSetter)
        {
            var exam = new Exam();
            var sbText = new StringBuilder();
            var lineNumber = lineNumberGetter();
            var line = lineGetter();

            exam.Name = line.Substring("/EXAM".Length + 1);

            lineNumber++;
            line = await reader.ReadLineAsync();
            do
            {
                begin:
                if (line != null && line.StartsWith("/DESCR"))
                {
                    exam.Description = await ParseTextBlock(reader, () => lineNumber, (ln) => lineNumber = ln, () => line, (l) => line = l);
                    goto begin;
                }
                else if (line != null && line.StartsWith("/CONCL"))
                {
                    exam.Conclusion = await ParseTextBlock(reader, () => lineNumber, (ln) => lineNumber = ln, () => line, (l) => line = l);
                    goto begin;
                }
                else if (line != null && line.StartsWith("/RESULT"))
                    exam.Result = line.Substring("/RESULT".Length + 1);
                else if (line != null && line.StartsWith("/SPECIF"))
                    exam.Specification = line.Substring("/SPECIF".Length + 1);
                else if (line != null && line.StartsWith("/END"))
                    continue;
                else
                    sbText.AppendLine(line);

                lineNumber++;
            }
            while ((line = await reader.ReadLineAsync()) != null && !line.StartsWith("/END") && !line.StartsWith("/EXAM"));

            exam.Text = sbText.ToString();

            lineSetter(line);
            lineNumberSetter(lineNumber);
            return exam;
        }

        static async Task<string> ParseTextBlock(TextReader reader, Func<int> lineNumberGetter, Action<int> lineNumberSetter, Func<string> lineGetter, Action<string> lineSetter)
        {
            var sbDescription = new StringBuilder();
            var lineNumber = lineNumberGetter();
            var line = lineGetter();

            lineNumber++;
            line = await reader.ReadLineAsync();
            do
            {
                sbDescription.AppendLine(line);

                lineNumber++;
            }
            while ((line = await reader.ReadLineAsync()) != null && !line.StartsWith("/"));

            lineSetter(line);
            lineNumberSetter(lineNumber);
            return sbDescription.ToString();
        }

        #endregion

        #region Methods

        static IList<string> LaboRecordParts(string line, int lineNumber, IDictionary<int, IList<string>> parserErrors)
        {
            if (line.Length < 17)
            {
                parserErrors.AddItem(lineNumber, $"Cannot parse record parst from posion 17- of line '{line}'");
                return null;
            }

            return line.Substring(16).Split('\\').ToList();
        }

        static string LaboRecordDescriptor(string line, int lineNumber, IDictionary<int, IList<string>> parserErrors)
        {
            if (line.Length < 16)
            {
                parserErrors.AddItem(lineNumber, $"Cannot parse record descriptor on posion 15-16 of line '{line}'");
                return null;
            }

            return line.Substring(14, 2);
        }

        static string LaboCode(string line, int lineNumber, IDictionary<int, IList<string>> parserErrors)
        {
            if (line.Length < 14)
            {
                parserErrors.AddItem(lineNumber, $"Cannot parse labocode on posion 5-14 of line '{line}'");
                return null;
            }

            return line.Substring(4, 10);
        }

        static int LineNumber(string line, int previouslineNumber, IDictionary<int, IList<string>> parserErrors)
        {
            if (line.Length < 4)
            {
                parserErrors.AddItem(previouslineNumber + 1, $"Cannot parse linenumber on posion 1-4 of line '{line}'");
                return -1;
            }
            int result = -1;
            if (!int.TryParse(line.Substring(0, 4), out result))
            {
                parserErrors.AddItem(previouslineNumber + 1, $"Invalid linenumber '{result}' (not a number) on posion 1-4 of line '{line}'");
                return -1;
            }

            return result;
        }

        #endregion
    }
}
