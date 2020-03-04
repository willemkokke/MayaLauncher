using System.Diagnostics;
using System.IO;

namespace MayaLauncher
{
    public class MayaFileVersion
    {
        public string Requires { get; internal set; }
        public string Product { get; internal set; }
        public string Version { get; internal set; }
        public string Cut { get; internal set; }

        public static MayaFileVersion FromFile(string mayaFile)
        {
            if (File.Exists(mayaFile))
            {
                var extension = Path.GetExtension(mayaFile).ToLower();

                switch (extension)
                {
                    case ".ma":
                        return ParseMayaAsciiFile(mayaFile);
                    case ".mb":
                        return ParseMayaBinaryFile(mayaFile);
                }
            }
            
            return null;
        }

        private static MayaFileVersion ParseMayaAsciiFile(string file)
        {
            try 
            {
                var result = new MayaFileVersion();

                using (var stream = new StreamReader(file))
                {
                    string requires = "requires maya ";
                    string product = "fileInfo \"product\" ";
                    string version = "fileInfo \"version\" ";
                    string cut = "fileInfo \"cutIdentifier\" ";
                    string createNode = "createNode ";

                    string line = "";
                    while ((line = stream.ReadLine()) != null)
                    {
                        if (line.StartsWith(requires))
                        {
                            result.Requires = line.Replace(requires, "").Trim(';').Trim('"');
                        }
                        else if (line.StartsWith(product))
                        {
                            result.Product = line.Replace(product, "").Trim(';').Trim('"');
                        }
                        else if (line.StartsWith(version))
                        {
                            result.Version = line.Replace(version, "").Trim(';').Trim('"');
                        }
                        else if (line.StartsWith(cut))
                        {
                            result.Cut = line.Replace(cut, "").Trim(';').Trim('"');
                        }
                        else if (line.StartsWith(createNode))
                        {
                            break;
                        }
                    }
                }

                return result;
            }
            catch (System.Exception e)
            {
                Debug.WriteLine($"Failed to open file: {file} ({e.Message})");
                return null;
            }
        }

        private static MayaFileVersion ParseMayaBinaryFile(string file)
        {
            try
            {
                var result = new MayaFileVersion();

                using (BinaryReader writer = new BinaryReader(File.Open(file, FileMode.Open)))
                {
                }
                return result;
            }
            catch (System.Exception e)
            {
                Debug.WriteLine($"Failed to open file: {file} ({e.Message})");
                return null;
            }
        }
    }
}
