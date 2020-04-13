using System;
using System.Collections.Generic;
using System.Text;

namespace MayaFileParser
{
    public class FileSummary
    {
        public string MayaVersion;
        public string DistanceUnit;
        public string AngleUnit;
        public string TimeUnit;
        public string LastSaved;
        public Dictionary<string, string> FileInfo = new Dictionary<string, string>();
        public Dictionary<string, string> Plugins = new Dictionary<string, string>();
        public List<string> References = new List<string>();
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrEmpty(MayaVersion))
            {
                builder.Append("Version: ").AppendLine(MayaVersion);
            }
            if (!string.IsNullOrEmpty(LastSaved))
            {
                builder.Append("Last Saved: ").AppendLine(LastSaved);
            }
            if (!string.IsNullOrEmpty(DistanceUnit))
            {
                builder.Append("Distance Unit: ").AppendLine(DistanceUnit);
            }
            if (!string.IsNullOrEmpty(TimeUnit))
            {
                builder.Append("Time Unit: ").AppendLine(TimeUnit);
            }
            if (!string.IsNullOrEmpty(AngleUnit))
            {
                builder.Append("Angle Unit: ").AppendLine(AngleUnit);
            }
            if (FileInfo.Count > 0)
            {
                builder.AppendLine("File Info:");
                foreach (var Item in FileInfo)
                {
                    builder.Append("\t").Append(Item.Key).Append(": ").AppendLine(Item.Value);
                }
            }
            if (Plugins.Count > 0)
            {
                builder.AppendLine("Required Plugins:");
                foreach (var Item in Plugins)
                {
                    builder.Append("\t").Append(Item.Key).Append(": ").AppendLine(Item.Value);
                }
            }
            if (References.Count > 0)
            {
                builder.AppendLine("Required References:");
                foreach (var Item in References)
                {
                    builder.Append("\t").AppendLine(Item);
                }
            }

            return builder.ToString();
        }
    }
}
