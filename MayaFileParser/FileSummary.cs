using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace MayaFileParser
{
    public enum FileType
    {
        MayaAscii,
        MayaBinary32,
        MayaBinary64,
    }

    public class FileSummary
    {
        public FileType Type { get; set; }
        public string Name { get; set; }
        public string Folder { get; set; }
        public string Path { get; set; }
        public string Size { get; set; }
        public string MayaVersion { get; set; }
        public string DistanceUnit { get; set; }
        public string AngleUnit { get; set; }
        public string TimeUnit { get; set; }
        public string LastSaved { get; set; }

        public Dictionary<string, string> FileInfo { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Plugins { get; set; } = new Dictionary<string, string>();
        public List<string> References { get; set; } = new List<string>();

        private Regex regex = new Regex(@"\d\d\d\d", RegexOptions.Compiled);

        // Was this file made with a beta version of Maya.
        public bool IsBeta 
        {
            get
            {
                return FileInfo.ContainsKey("version") && FileInfo["version"].ToLower().Contains("preview"); 
            }
        }

        // This should return a string used to identify the correct maya launchable.
        public string InstallVersion
        {
            get 
            {
                try
                {
                    if (IsBeta && FileInfo.ContainsKey("product"))
                    {
                        var match = regex.Match(FileInfo["product"]);
                        if (match != null)
                        {
                            return match.Value;
                        }
                    }

                    if (FileInfo.ContainsKey("version"))
                    {
                        return FileInfo["version"];
                    }
 
                    return MayaVersion.Substring(0, 4);
                }
                catch
                {
                    return "Unknown";
                }
            }
        }

        public string DisplayName
        {
            get
            {
                return Name + $" ({Size})";
            }
        }

        public string DisplayType { get
            {
                switch(Type)
                {
                    case FileType.MayaAscii:
                        return "Maya Ascii File";
                    case FileType.MayaBinary32:
                        return "Maya Binary File (32 bit)";
                    case FileType.MayaBinary64:
                        return "Maya Binary File (64 bit)";
                    default:
                        return "Unknown";
                }
            }
        }

        public object DisplayHeader
        {
            get
            {
                return new dynamic[] {
                    new { Key  = "Name: ", Value = DisplayName },
                    new { Key  = "Type: ", Value = DisplayType },
                    new { Key  = "Saved With: ", Value = Product },
                    new { Key  = "Last Saved: ", Value = LastSaved },
                };
            }
        }

        static Dictionary<string, int> timeUnitMap = new Dictionary<string, int>
        {
            { "game", 15 },
            { "film", 24 },
            { "pal", 25 },
            { "ntsc", 25 },
            { "show", 48 },
            { "palf", 50 },
            { "ntscf", 60 },
        };

        public object DisplayTimeUnit
        {
            get
            {
                if (timeUnitMap.ContainsKey(TimeUnit))
                {
                    return $"{timeUnitMap[TimeUnit]}fps ({TimeUnit})";
                }
                return TimeUnit;
            }
        }

        static Dictionary<string, string> distanceUnitMap = new Dictionary<string, string>
        {
            { "mm", "millimeter" },
            { "cm", "centimeter" },
            { "m", "meter" },
            { "km", "kilometer" },
            { "in", "inch" },
            { "ft", "foot" },
            { "yd", "yard" },
            { "mi", "mile" },
        };

        public object DisplayDistanceUnit
        {
            get
            {
                if (distanceUnitMap.ContainsKey(DistanceUnit))
                {
                    return distanceUnitMap[DistanceUnit];
                }
                return DistanceUnit;
            }
        }

        static Dictionary<string, string> angleUnitMap = new Dictionary<string, string>
        {
            { "deg", "degree" },
            { "rad", "radian" },
        };

        public object DisplayAngleUnit
        {
            get
            {
                if (angleUnitMap.ContainsKey(AngleUnit))
                {
                    return angleUnitMap[AngleUnit];
                }
                return AngleUnit;
            }
        }

        public string Product
        {
            get
            {
                if (FileInfo.ContainsKey("product"))
                {
                    return FileInfo["product"] + (IsBeta ? " (Beta)" : "");
                }
                return "Unknown";
            }
        }

        public object DisplayUnits
        {
            get
            {
                return new dynamic[] {
                    new { Key  = "Distance: ", Value = DisplayDistanceUnit },
                    new { Key  = "Angle: ", Value = DisplayAngleUnit },
                    new { Key  = "Time: ", Value = DisplayTimeUnit },
                };
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrEmpty(Name))
            {
                builder.Append("Name: ").AppendLine(Name);
            }
            if (!string.IsNullOrEmpty(Folder))
            {
                builder.Append("Folder: ").AppendLine(Folder);
            }
            if (!string.IsNullOrEmpty(Size))
            {
                builder.Append("Size: ").AppendLine(Size);
            }
            
            builder.Append("Type: ").AppendLine(Type.ToString());

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

        public static FileSummary FromFile(string mayaFile)
        {
            try
            {
                if (File.Exists(mayaFile))
                {
                    FileSummary summary = null;

                    var extension = System.IO.Path.GetExtension(mayaFile).ToLower();

                    switch (extension)
                    {
                        case ".ma":
                            var asciiParser = new AsciiParser(mayaFile);
                            summary = asciiParser.ParseFileInfo();
                            break;
                        case ".mb":
                            var binaryParser = new BinaryParser(mayaFile);
                            summary = binaryParser.ParseFileInfo();
                            break;
                    }

                    if (summary != null)
                    {
                        GetFileInfo(mayaFile, summary);
                    }

                    return summary;
                }
            }
            catch (System.Exception e)
            {
                Debug.WriteLine($"Failed to open file: {mayaFile} ({e.Message})");
            }

            return null;
        }

        private static void GetFileInfo(string mayaFile, FileSummary summary)
        {
            FileInfo info = new FileInfo(mayaFile);
            summary.Name = info.Name;
            summary.Folder = info.DirectoryName;
            summary.Size = BytesToString(info.Length);
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern long StrFormatKBSize(long qdw, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder pszBuf, int cchBuf);

        public static string BytesToString(long byteCount)
        {
            var sb = new StringBuilder(32);
            StrFormatKBSize(byteCount, sb, sb.Capacity);
            return sb.ToString();
        }
    }
}
