using System.Diagnostics;
using System.IO;
using MayaFileParser;

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
                var parser = new AsciiParser(file);
                var result = new MayaFileVersion();
                FileSummary summary = parser.ParseFileInfo();
                Debug.WriteLine(summary);

                return result;
            }
            catch (System.Exception e)
            {
                Debug.WriteLine($"Failed to open file: {file} ({e.Message})");
                return null;
            }
        }

        private static void PrintChunk(IFFParser.Chunk chunk, int depth=0)
        {
            Debug.Write("".PadLeft(depth*2));
            Debug.WriteLine(chunk.ToString());
            if (chunk is IFFParser.GroupChunk gc)
            {
                foreach (IFFParser.Chunk c in gc.Children)
                {
                    PrintChunk(c, depth+1);
                }
            }
        }

        private static MayaFileVersion ParseMayaBinaryFile(string file)
        {
            try
            {
                var result = new MayaFileVersion();
                BinaryParser parser = new BinaryParser(file);
                FileSummary summary = parser.ParseFileInfo();
                PrintChunk(parser.Root);
                Debug.WriteLine(summary);

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
