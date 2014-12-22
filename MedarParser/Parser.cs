using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MedarParser.Model;

namespace MedarParser
{
    public class Parser
    {
        #region Fields

        int lineNumber;

        #endregion

        #region Constructor

        public Parser()
        {
            ParserErrors = new Dictionary<int, IList<string>>();
        }

        #endregion

        #region Properties

        public IDictionary<int, IList<string>> ParserErrors { get; set; }

        #endregion

        #region Parser Methods

        public IEnumerable<Letter> ParseLetter(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            using (var reader = new StringReader(text))
            {
                return ParseLetter(reader);
            }
        }

        public IEnumerable<Letter> ParseLetter(TextReader reader)
        {
            var letters = new List<Letter>();
            lineNumber = -1;
            ParserErrors.Clear();

            var line = ReadLine(reader);
            do
            {
                if (line != null)
                    letters.Add(ParseLetterBlock(reader, line));
            }
            while ((line = ReadLine(reader)) != null);

            return letters;
        }

        public IEnumerable<Labo> ParseLabo(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            using (var reader = new StringReader(text))
            {
                return ParseLabo(reader);
            }
        }

        public IEnumerable<Labo> ParseLabo(TextReader reader)
        {
            var labos = new List<Labo>();
            lineNumber = -1;
            ParserErrors.Clear();

            var line = ReadLine(reader);
            var previousLineNumber = -1;
            string previousLaboCode = null;
            Labo labo = null;
            do
            {
                var currentLineNumber = LineNumber(line);
                if (currentLineNumber != (previousLineNumber + 1))
                    ParserErrors.AddItem(lineNumber, string.Format("Lines not in sequence: line {0} followed by line {1}: {2}", previousLineNumber, currentLineNumber));
                previousLineNumber = currentLineNumber;

                var currentLaboCode = LaboCode(line);
                if (currentLaboCode != previousLaboCode)
                {
                    labo = new Labo() { Code = currentLaboCode };
                    labos.Add(labo);
                    previousLaboCode = currentLaboCode;
                }

                var recordDescriptor = LaboRecordDescriptor(line);
                ParseLaboRecord(recordDescriptor, labo, LaboRecordParts(line));
            }
            while ((line = ReadLine(reader)) != null && !line.StartsWith("END"));

            return labos;
        }

        #endregion

        #region Private Parser Methods

        void ParseLaboRecord(string recordDescriptor, Labo labo, IList<string> recordParts)
        {
            switch (recordDescriptor)
            {
                case "S1":
                    ParseLaboS1(recordParts, labo);
                    break;
                case "S2":
                    ParseLaboS2(recordParts, labo);
                    break;
                case "S3":
                    ParseLaboS3(recordParts, labo);
                    break;
                case "S4":
                    ParseLaboS4(recordParts, labo);
                    break;
                case "S5":
                    ParseLaboS5(recordParts, labo);
                    break;
                case "S6":
                    ParseLaboS6(recordParts, labo);
                    break;
                case "C1":
                    ParseLaboC1(recordParts, labo);
                    break;
                case "A1":
                    ParseLaboA1(recordParts, labo);
                    break;
                case "D1":
                    ParseLaboD1(recordParts, labo);
                    break;
                case "R1":
                    ParseLaboR1(recordParts, labo);
                    break;
                default:
                    ParserErrors.AddItem(lineNumber, string.Format("Unknown record descriptor '{0}'", recordDescriptor));
                    break;
            }
        }

        void ParseLaboS1(IList<string> recordParts, Labo labo)
        {
            if (recordParts.Count != 2)
                ParserErrors.AddItem(lineNumber, string.Format("Expected 2 parts in S1 but got {0} parts: '{1}'", recordParts.Count, string.Join("\\", recordParts)));

            labo.Code = recordParts.ElementAtOrDefault(0);
            if (labo.Patient == null)
                labo.Patient = new LaboPatient();
            labo.Patient.Key = recordParts.ElementAtOrDefault(1);
        }

        void ParseLaboS2(IList<string> recordParts, Labo labo)
        {
            if (recordParts.Count != 2)
                ParserErrors.AddItem(lineNumber, string.Format("Expected 2 parts in S2 but got {0} parts: '{1}'", recordParts.Count, string.Join("\\", recordParts)));

            if (labo.Patient == null)
                labo.Patient = new LaboPatient();
            labo.Patient.LastName = recordParts.ElementAtOrDefault(0);
            labo.Patient.FirstName = recordParts.ElementAtOrDefault(1);
        }

        void ParseLaboS3(IList<string> recordParts, Labo labo)
        {
            if (recordParts.Count != 2)
                ParserErrors.AddItem(lineNumber, string.Format("Expected 2 parts in S3 but got {0} parts: '{1}'", recordParts.Count, string.Join("\\", recordParts)));

            if (labo.Patient == null)
                labo.Patient = new LaboPatient();
            labo.Patient.Street = recordParts.ElementAtOrDefault(0);
            labo.Patient.PostalName = recordParts.ElementAtOrDefault(1);
        }

        void ParseLaboS4(IList<string> recordParts, Labo labo)
        {
            if (recordParts.Count != 2)
                ParserErrors.AddItem(lineNumber, string.Format("Expected 2 parts in S4 but got {0} parts: '{1}'", recordParts.Count, string.Join("\\", recordParts)));

            if (labo.Patient == null)
                labo.Patient = new LaboPatient();
            labo.Patient.BirthDate = recordParts.ElementAtOrDefault(0).Maybe(s => s.ToNullableDatetime("dd/MM/yyyy"));
            labo.Patient.Sex = recordParts.ElementAtOrDefault(1).Maybe(s =>
            {
                switch (s)
                {
                    case "F":
                    case "V":
                        return Sex.Female;
                    case "M":
                        return Sex.Male;
                    default:
                        return Sex.Unknown;
                }
            });
        }

        void ParseLaboS5(IList<string> recordParts, Labo labo)
        {
            if (recordParts.Count != 5)
                ParserErrors.AddItem(lineNumber, string.Format("Expected 5 parts in S5 but got {0} parts: '{1}'", recordParts.Count, string.Join("\\", recordParts)));

            if (labo.Mutuality == null)
                labo.Mutuality = new Mutuality();
            labo.Mutuality.Name = recordParts.ElementAtOrDefault(0);
            labo.Mutuality.MemberNumber = recordParts.ElementAtOrDefault(1);
            labo.Mutuality.Kg1Kg2 = recordParts.ElementAtOrDefault(2);
            labo.Mutuality.Relationship = recordParts.ElementAtOrDefault(3);
            labo.Mutuality.Holder = recordParts.ElementAtOrDefault(4);
        }

        void ParseLaboS6(IList<string> recordParts, Labo labo)
        {
            if (recordParts.Count != 2)
                ParserErrors.AddItem(lineNumber, string.Format("Expected 2 parts in S6 but got {0} parts: '{1}'", recordParts.Count, string.Join("\\", recordParts)));

            if (labo.Patient == null)
                labo.Patient = new LaboPatient();
            labo.Patient.UniquePatientNumber = recordParts.ElementAtOrDefault(0);
            labo.Patient.UniqueMedicalRecordNumber = recordParts.ElementAtOrDefault(1);
        }

        void ParseLaboC1(IList<string> recordParts, Labo labo)
        {
            if (recordParts.Count != 2)
                ParserErrors.AddItem(lineNumber, string.Format("Expected 4 parts in C1 but got {0} parts: '{1}'", recordParts.Count, string.Join("\\", recordParts)));

            if (labo.Contact == null)
                labo.Contact = new Contact();
            labo.Contact.Number = recordParts.ElementAtOrDefault(0);
            labo.Contact.Type = recordParts.ElementAtOrDefault(1).Maybe(s =>
            {
                switch (s)
                {
                    case "01":
                    case "1":
                        return ContactType.Consultation;
                    case "02":
                    case "2":
                        return ContactType.Hospitalization;
                    default:
                        return ContactType.Unknown;
                }
            });
            labo.Contact.StartDate = recordParts.ElementAtOrDefault(2).Maybe(s => s.ToNullableDatetime("dd/MM/yyyy"));
            labo.Contact.EndDate = recordParts.ElementAtOrDefault(3).Maybe(s => s.ToNullableDatetime("dd/MM/yyyy"));
        }

        void ParseLaboA1(IList<string> recordParts, Labo labo)
        {
            if (recordParts.Count != 1)
                ParserErrors.AddItem(lineNumber, string.Format("Expected 1 parts in A1 but got {0} parts: '{1}'", recordParts.Count, string.Join("\\", recordParts)));

            labo.ArrivalDate = recordParts.ElementAtOrDefault(2).Maybe(s => s.ToNullableDatetime("dd/MM/yyyy"));
        }

        void ParseLaboD1(IList<string> recordParts, Labo labo)
        {
            if (recordParts.Count != 2)
                ParserErrors.AddItem(lineNumber, string.Format("Expected 2 parts in D1 but got {0} parts: '{1}'", recordParts.Count, string.Join("\\", recordParts)));

            if (labo.Doctor == null)
                labo.Doctor = new Doctor();
            labo.Doctor.LastName = recordParts.ElementAtOrDefault(0);
            labo.Doctor.FirstName = recordParts.ElementAtOrDefault(1);
        }

        void ParseLaboR1(IList<string> recordParts, Labo labo)
        {
            if (recordParts.Count != 5)
                ParserErrors.AddItem(lineNumber, string.Format("Expected 5 parts in R1 but got {0} parts: '{1}'", recordParts.Count, string.Join("\\", recordParts)));

            var code = recordParts.ElementAtOrDefault(0);
            var description = recordParts.ElementAtOrDefault(1);
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
                    var lastResultGroup = labo.ResultGroups.Last();
                    if (lastResultGroup == null)
                    {
                        ParserErrors.AddItem(lineNumber, string.Format("No result group found: '{0}'", string.Join("\\", recordParts)));
                        lastResultGroup = new LaboResultGroup();
                        labo.ResultGroups.Add(lastResultGroup);
                    }

                    var previousAnalysis = lastResultGroup.Results.Last();
                    if (previousAnalysis == null)
                        ParserErrors.AddItem(lineNumber, string.Format("No previous comment found to add coment to: '{0}'", string.Join("\\", recordParts)));
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
                    var lastResultGroup = labo.ResultGroups.Last();
                    if (lastResultGroup == null)
                    {
                        ParserErrors.AddItem(lineNumber, string.Format("No result group found: '{0}'", string.Join("\\", recordParts)));
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

        Letter ParseLetterBlock(TextReader reader, string line)
        {
            var letter = new Letter();

            letter.Header = ParseHeaderBlock(reader, line);

            line = ReadLine(reader);

            if (line == null)
                ParserErrors.AddItem(lineNumber, string.Format("No body block defined"));
            else
                letter.Body = ParseBodyBlock(reader, line);

            return letter;
        }

        Header ParseHeaderBlock(TextReader reader, string line)
        {
            var header = new Header();

            header.From = ParseFrom(line);

            line = ReadLine(reader);
            header.To = ParseFrom(line);

            line = ReadLine(reader);
            header.Subject = ParseSubject(line);

            line = ReadLine(reader);
            header.Info = ParseInfo(line);

            return header;
        }

        Person ParseFrom(string line)
        {
            var person = new Person();

            var lineParts = line.Maybe(s => s.Substring(11).Split('|').ToList());
            if (lineParts.Count != 6)
                ParserErrors.AddItem(lineNumber, string.Format("Expected 6 parts in /FROM but got {0} parts: '{1}'", lineParts.Count, line));

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

        Person ParseTo(string line)
        {
            var person = new Person();

            var lineParts = line.Maybe(s => s.Substring(11).Split('|').ToList());
            if (lineParts.Count != 6)
                ParserErrors.AddItem(lineNumber, string.Format("Expected 6 parts in /TO but got {0} parts: '{1}'", lineParts.Count, line));

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

        LetterPatient ParseSubject(string line)
        {
            var patient = new LetterPatient();

            var lineParts = line.Maybe(s => s.Substring(11).Split('|').ToList());
            if (lineParts.Count != 7)
                ParserErrors.AddItem(lineNumber, string.Format("Expected 7 parts in /SUBJECT but got {0} parts: '{1}'", lineParts.Count, line));

            patient.FirstName = lineParts.ElementAtOrDefault(0);
            patient.LastName = lineParts.ElementAtOrDefault(1);
            patient.Street = lineParts.ElementAtOrDefault(2);
            patient.PostalCode = lineParts.ElementAtOrDefault(3);
            patient.PostalName = lineParts.ElementAtOrDefault(4);
            patient.BirthDate = lineParts.ElementAtOrDefault(5).Maybe(s => s.ToNullableDatetime("dd/MM/yyyy"));
            patient.Sex = lineParts.ElementAtOrDefault(6).Maybe(s =>
            {
                switch (s)
                {
                    case "F":
                    case "V":
                        return Sex.Female;
                    case "M":
                        return Sex.Male;
                    default:
                        return Sex.Unknown;
                }
            });

            for (int i = 7; i < lineParts.Count; i++)
                patient.UnknownParts.Add(lineParts[i]);

            return patient;
        }

        Info ParseInfo(string line)
        {
            var info = new Info();

            var lineParts = line.Maybe(s => s.Substring(11).Split('|').ToList());
            if (lineParts.Count != 2)
                ParserErrors.AddItem(lineNumber, string.Format("Expected 2 parts in /INFO but got {0} parts: '{1}'", lineParts.Count, line));

            info.ReferenceNumber = lineParts.ElementAtOrDefault(0);
            info.Date = lineParts.ElementAtOrDefault(1);

            for (int i = 2; i < lineParts.Count; i++)
                info.UnknownParts.Add(lineParts[i]);

            return info;
        }

        Body ParseBodyBlock(TextReader reader, string line)
        {
            var body = new Body();
            var sbText = new StringBuilder();

            do
            {
            begin:
                if (line != null && line.StartsWith("/TITLE"))
                    body.Title = line.Substring("/TITLE".Length + 1);
                else if (line != null && line.StartsWith("/DATE"))
                    body.Date = line.Substring("/DATE".Length + 1).ToNullableDatetime("dd/M/yy", "dd/MM/yy", "dd/MM/yyyy");
                else if (line != null && line.StartsWith("/EXAM"))
                {
                    body.Exams.Add(ParseExam(reader, ref line));
                    goto begin;
                }
                else if (line != null && line.StartsWith("/END"))
                    continue;
                else
                    sbText.AppendLine(line);
            }
            while ((line = ReadLine(reader)) != null);

            body.Text = sbText.ToString();

            return body;
        }

        Exam ParseExam(TextReader reader, ref string line)
        {
            var exam = new Exam();
            var sbText = new StringBuilder();
            exam.Name = line.Substring("/EXAM".Length + 1);
            line = ReadLine(reader);
            do
            {
            begin:
                if (line != null && line.StartsWith("/DESCR"))
                {
                    exam.Description = ParseTextBlock(reader, ref line);
                    goto begin;
                }
                else if (line != null && line.StartsWith("/CONCL"))
                {
                    exam.Conclusion = ParseTextBlock(reader, ref line);
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

            }
            while ((line = ReadLine(reader)) != null && !line.StartsWith("/END") && !line.StartsWith("/EXAM"));

            exam.Text = sbText.ToString();

            return exam;
        }

        string ParseTextBlock(TextReader reader, ref string line)
        {
            var sbDescription = new StringBuilder();

            line = ReadLine(reader);
            do
            {
                sbDescription.AppendLine(line);
            }
            while ((line = ReadLine(reader)) != null && !line.StartsWith("/"));

            return sbDescription.ToString();
        }

        #endregion

        #region Methods

        IList<string> LaboRecordParts(string line)
        {
            if (line.Length < 17)
            {
                ParserErrors.AddItem(lineNumber, string.Format("Cannot parse record parst from posion 17- of line '{0}'", line));
                return null;
            }

            return line.Substring(16).Split('\\').ToList();
        }

        string LaboRecordDescriptor(string line)
        {
            if (line.Length < 16)
            {
                ParserErrors.AddItem(lineNumber, string.Format("Cannot parse record descriptor on posion 15-16 of line '{0}'", line));
                return null;
            }

            return line.Substring(14, 2);
        }

        string LaboCode(string line)
        {
            if (line.Length < 14)
            {
                ParserErrors.AddItem(lineNumber, string.Format("Cannot parse labocode on posion 5-14 of line '{0}'", line));
                return null;
            }

            return line.Substring(4, 10);
        }

        int LineNumber(string line)
        {
            if (line.Length < 4)
            {
                ParserErrors.AddItem(lineNumber, string.Format("Cannot parse linenumber on posion 1-4 of line '{0}'", line));
                return -1;
            }
            int result = -1;
            if (!int.TryParse(line.Substring(0, 4), out result))
            {
                ParserErrors.AddItem(lineNumber, string.Format("Invalid linenumber '{0}' (not a number) on posion 1-4 of line '{1}'", result, line));
                return -1;
            }

            return result;
        }

        string ReadLine(TextReader reader)
        {
            lineNumber++;
            return reader.ReadLine();
        }

        string TrimToMaxSize(string input, int max)
        {
            if ((input != null) && (input.Length > max))
                ParserErrors.AddItem(lineNumber, string.Format("Line exeeded max length of {0} characters: '{1}'", max, input));

            return input;
        }

        #endregion
    }
}
