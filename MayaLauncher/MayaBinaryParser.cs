using System;
using System.Diagnostics;

namespace MayaLauncher
{
    class MayaBinaryParser : MayaIFFParser
    {
        // General
        private static readonly UInt32 MAYA = ChunkIdFromString("Maya");

        // File referencing
        private static readonly UInt32 FREF = ChunkIdFromString("FREF");
        private static readonly UInt32 FRDI = ChunkIdFromString("FRDI");

        // Header fields
        private static readonly UInt32 HEAD = ChunkIdFromString("HEAD");
        private static readonly UInt32 VERS = ChunkIdFromString("VERS");
        private static readonly UInt32 PLUG = ChunkIdFromString("PLUG");
        private static readonly UInt32 FINF = ChunkIdFromString("FINF");
        private static readonly UInt32 AUNI = ChunkIdFromString("AUNI");
        private static readonly UInt32 LUNI = ChunkIdFromString("LUNI");
        private static readonly UInt32 TUNI = ChunkIdFromString("TUNI");

        // Node creation
        private static readonly UInt32 CREA = ChunkIdFromString("CREA");
        private static readonly UInt32 SLCT = ChunkIdFromString("SLCT");
        private static readonly UInt32 ATTR = ChunkIdFromString("ATTR");

        private static readonly UInt32 CONS = ChunkIdFromString("CONS");
        private static readonly UInt32 CONN = ChunkIdFromString("CONN");

        // Data types
        private static readonly UInt32 FLGS = ChunkIdFromString("FLGS");
        private static readonly UInt32 DBLE = ChunkIdFromString("DBLE");
        private static readonly UInt32 DBL3 = ChunkIdFromString("DBL3");
        private static readonly UInt32 STR_ = ChunkIdFromString("STR ");
        private static readonly UInt32 FLT2 = ChunkIdFromString("FLT2");
        private static readonly UInt32 CMPD = ChunkIdFromString("CMPD");
        private static readonly UInt32 MESH = ChunkIdFromString("MESH");

        public MayaBinaryParser(string file) : base(file)
        {
            RegisterGroupHandler(HEAD, ParseHeaderGroup);
        }

        private void ParseHeaderGroup(GroupChunk group)
        {
            foreach (Chunk chunk in StreamChunk(group, 8))
            {
                Debug.WriteLine("Parse Header");
            }
            Debug.WriteLine("Parse Header");
        }
    }
}
