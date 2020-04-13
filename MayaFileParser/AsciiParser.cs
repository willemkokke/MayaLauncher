using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace MayaFileParser
{
    public class AsciiParser : IDisposable
    {
        private string path;
        private StreamReader stream;
        private Encoding encoding;

        private bool abort = false;
        private FileSummary summary = new FileSummary();
        
        private Regex regex = new Regex("\".*?\"+|-?\\w+", RegexOptions.Compiled);

        private static Encoding defaultEncoding;
        
        static AsciiParser()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            defaultEncoding = Encoding.GetEncoding(1252);
        }

        public AsciiParser(string filepath)
        {
            path = filepath;
            encoding = defaultEncoding;
            OpenStream();
        }

        public void Abort()
        {
            abort = true;
        }

        public FileSummary ParseFileInfo()
        {
            try
            {
                bool isValid = false;
                while (!stream.EndOfStream)
                {
                    if (abort)
                    {
                        return summary;
                    }

                    string line = stream.ReadLine();

                    if (!isValid)
                    {
                        if (!line.StartsWith("//Maya"))
                        {
                            throw new ParseException("Maya Ascii files need to start with //Maya");
                        }
                        isValid = true;
                    }

                    line = line.Trim();

                    if (line.StartsWith("//"))
                    {
                        bool restart = ParseComment(line);
                        if (restart)
                        {
                            summary = new FileSummary();
                            OpenStream();
                            return ParseFileInfo();
                        }
                    }
                    else
                    {
                        string[] command = ParseCommand(line);

                        switch(command[0])
                        {
                            case "file":
                                ParseReference(command);
                                break;
                            case "requires":
                                ParseRequires(command);
                                break;
                            case "currentUnit":
                                ParseUnits(command);
                                break;
                            case "fileInfo":
                                ParseFileInfo(command);
                                break;
                            case "createNode":
                                Abort();
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new ParseException("Failed to parse Maya Ascii file", e);
            }

            return summary;
        }

        private void OpenStream()
        {
            if (stream != null)
            {
                stream.Dispose();
            }

            try
            {
                stream = new StreamReader(File.OpenRead(path), encoding);
            }
            catch (Exception e)
            {
                throw new ParseException("Failed to open file", e);
            }
        }

        private bool ParseComment(string comment)
        {
            comment = comment.TrimStart('/');
            if (comment.Contains(':'))
            {
                string[] parts = comment.Split(':', 2);
                parts[1] = parts[1].Trim();
                switch(parts[0].ToLower())
                {
                    case "last modified":
                        summary.LastSaved = parts[1];
                        break;
                    case "codeset":
                        int codepage;
                        if (int.TryParse(parts[1], out codepage))
                        {
                            if (encoding.CodePage != codepage)
                            {
                                Encoding newEncoding = Encoding.GetEncoding(codepage);
                                if (newEncoding != null)
                                {
                                    encoding = newEncoding;
                                    return true;
                                }
                            }
                        }
                        break;
                }
            }
            return false;
        }

        string[] ParseCommand(string command)
        {
            var matches = regex.Matches(command);
            if (matches != null)
            {
                string[] result = new string[matches.Count];
                int index = 0;
                foreach (Match match in matches)
                {
                    result[index] = match.Value.Trim('"');
                    index++;
                }
                return result;
            }
            return null;
        }

        private void ParseReference(string[] command)
        {
            string reference = command[command.Length - 1];
            if (!summary.References.Contains(reference))
            {
                summary.References.Add(reference);
            }
        }

        private void ParseRequires(string[] command)
        {
            if (command[1] == "maya")
            {
                summary.MayaVersion = command[2];
            }
            else
            {
                string key = command[command.Length - 2];
                string value = command[command.Length - 1];
                summary.Plugins[key] = value;
            }
        }

        private void ParseUnits(string[] command)
        {
            summary.DistanceUnit = FindParameter(command, "-l");
            summary.AngleUnit = FindParameter(command, "-a");
            summary.TimeUnit = FindParameter(command, "-t");
        }

        private void ParseFileInfo(string[] command)
        {
            string key = command[command.Length - 2];
            string value = command[command.Length - 1];
            summary.FileInfo[key] = value;
        }

        private string FindParameter(string[] command, string parameter, int index=0)
        {
            try 
            {
                int pos = 0;
                foreach (string s in command)
                {
                    if (s == parameter)
                    {
                        break;
                    }
                    pos++;
                }
                return command[pos + 1 + index];
            }
            catch(Exception)
            {
            }

            return null;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    stream?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
