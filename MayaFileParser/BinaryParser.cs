using System;
using System.Diagnostics;

namespace MayaFileParser
{
    public class BinaryParser : IFFParser
    {
        private FileSummary summary = new FileSummary();

        // General
        private static readonly Int32 MAYA = Chunk.ChunkIdFromString("Maya");

        // File referencing
        private static readonly Int32 FREF = Chunk.ChunkIdFromString("FREF");
        private static readonly Int32 FRDI = Chunk.ChunkIdFromString("FRDI");

        // Header fields
        private static readonly Int32 HEAD = Chunk.ChunkIdFromString("HEAD");
        private static readonly Int32 VERS = Chunk.ChunkIdFromString("VERS");
        private static readonly Int32 CHNG = Chunk.ChunkIdFromString("CHNG");
        private static readonly Int32 LUNI = Chunk.ChunkIdFromString("LUNI");
        private static readonly Int32 TUNI = Chunk.ChunkIdFromString("TUNI");
        private static readonly Int32 AUNI = Chunk.ChunkIdFromString("AUNI");
        private static readonly Int32 FINF = Chunk.ChunkIdFromString("FINF");
        private static readonly Int32 PLUG = Chunk.ChunkIdFromString("PLUG");

        // Node creation
        private static readonly Int32 XFRM = Chunk.ChunkIdFromString("XFRM");
        //private static readonly Int32 CREA = Chunk.ChunkIdFromString("CREA");
        //private static readonly Int32 SLCT = Chunk.ChunkIdFromString("SLCT");
        //private static readonly Int32 ATTR = Chunk.ChunkIdFromString("ATTR");

        //private static readonly Int32 CONS = Chunk.ChunkIdFromString("CONS");
        //private static readonly Int32 CONN = Chunk.ChunkIdFromString("CONN");

        //// Data types
        //private static readonly Int32 FLGS = Chunk.ChunkIdFromString("FLGS");
        //private static readonly Int32 DBLE = Chunk.ChunkIdFromString("DBLE");
        //private static readonly Int32 DBL3 = Chunk.ChunkIdFromString("DBL3");
        //private static readonly Int32 STR_ = Chunk.ChunkIdFromString("STR ");
        //private static readonly Int32 FLT2 = Chunk.ChunkIdFromString("FLT2");
        //private static readonly Int32 CMPD = Chunk.ChunkIdFromString("CMPD");
        //private static readonly Int32 MESH = Chunk.ChunkIdFromString("MESH");

        public BinaryParser(string file) : base(file)
        {

        }

        public FileSummary ParseFileInfo()
        {
            RegisterGroupHandler(HEAD, ParseHeaderGroup);
            RegisterGroupHandler(FREF, ParseFileReference);
            RegisterGroupHandler(FRDI, ParseFileReferenceDepth);
            RegisterGroupHandler(XFRM, ParseCreateNode);

            Parse(MAYA);
            return summary;
        }

        private void ParseHeaderGroup(GroupChunk group)
        {
            foreach (Chunk chunk in Stream(group))
            {
                if (chunk.ChunkId == VERS)
                {
                    summary.MayaVersion = chunk.ReadAsString(stream);
                }
                else if (chunk.ChunkId == CHNG)
                {
                    summary.LastSaved = chunk.ReadAsString(stream);
                }
                else if (chunk.ChunkId == LUNI)
                {
                    summary.DistanceUnit = chunk.ReadAsString(stream);
                }
                else if (chunk.ChunkId == TUNI)
                {
                    summary.TimeUnit = chunk.ReadAsString(stream);
                }
                else if (chunk.ChunkId == AUNI)
                {
                    summary.AngleUnit = chunk.ReadAsString(stream);
                }
                else if (chunk.ChunkId == FINF)
                {
                    string key = chunk.ReadString(stream);
                    string value = chunk.ReadString(stream);
                    summary.FileInfo[key] = value;
                }
                else if (chunk.ChunkId == PLUG)
                {
                    string key = chunk.ReadString(stream);
                    string value = chunk.ReadString(stream);
                    summary.Plugins[key] = value;
                }
            }
        }

        private void ParseFileReference(GroupChunk group)
        {
            foreach (Chunk chunk in Stream(group))
            {
                string reference = chunk.ReadString(stream);
                if (!summary.References.Contains(reference))
                {
                    summary.References.Add(reference);
                }
            }
        }

        private void ParseFileReferenceDepth(GroupChunk group)
        {
            foreach (Chunk chunk in Stream(group))
            {
                Int32 depth = stream.ReadInt32BE();
                string reference = chunk.ReadString(stream);
                if (!summary.References.Contains(reference))
                {
                    summary.References.Add(reference);
                }
            }
        }

        private void ParseCreateNode(GroupChunk group)
        {
            // Skip parsing the rest of the file once the header and file references have been parsed
            Abort();
        }
    }
}
