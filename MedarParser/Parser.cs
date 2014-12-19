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

        #endregion

        #region Private Parser Methods

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

        Patient ParseSubject(string line)
        {
            var patient = new Patient();

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
                        return Sex.female;
                    case "M":
                        return Sex.male;
                    default:
                        return Sex.unknown;
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
