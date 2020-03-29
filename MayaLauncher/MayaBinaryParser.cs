using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace MayaLauncher
{
    public class MayaBinaryParser : IDisposable
    {
        public class Chunk
        {
            public readonly UInt32 ChunkId;
            public readonly UInt64 DataStart;
            public readonly UInt64 DataLength;
            public readonly UInt64 ChunkEnd;
            private List<Chunk> children;

            public List<Chunk> Children { get => children; }

            public Chunk(UInt32 chunkId, UInt64 dataStart, UInt64 dataLength)
            {
                ChunkId = chunkId;
                DataStart = dataStart;
                DataLength = dataLength;
                ChunkEnd = dataStart + Align(dataLength, 8);
                children = MayaBinaryParser.IsGroup(chunkId) ? new List<Chunk>() : null;
            }

            private static UInt64 Align(UInt64 value, UInt32 alignment)
            {
                UInt64 mask = alignment - 1;
                UInt64 inverse_mask = ~mask;
                bool isAligned = (value & mask) == 0;
                if (!isAligned)
                {
                    return (value & inverse_mask) + alignment;
                }
                return value;
            }

            public bool IsGroup()
            {
                return children != null;
            }

            public override string ToString()
            {
                return $"Chunk: {StringFromChunkId(ChunkId)}, Data Start: {DataStart}, Data Length: {DataLength} End: {ChunkEnd}";
            }
        }

        public class GroupChunk :  Chunk
        {
            public readonly UInt32 GroupId;

            public GroupChunk(UInt32 chunkId, UInt32 groupId, UInt64 dataStart, UInt64 dataLength) : base(chunkId, dataStart, dataLength)
            {
                GroupId = groupId;
            }
            public override string ToString()
            {
                return $"GroupChunk: {StringFromChunkId(ChunkId)}, Type: {StringFromChunkId(GroupId)} Data Start: {DataStart}, Data Length: {DataLength} End: {ChunkEnd}";
            }
        }


    BinaryReader _stream;

        private readonly UInt32 alignment = 8;

        // IFF chunk type IDs
        private static readonly UInt32 FOR4 = ChunkIdFromString("FOR4");
        private static readonly UInt32 LIS4 = ChunkIdFromString("LIS4");
        private static readonly UInt32 FOR8 = ChunkIdFromString("FOR8");
        private static readonly UInt32 LIS8 = ChunkIdFromString("LIS8");

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


        public MayaBinaryParser(string file) : this(new BinaryReader(File.OpenRead(file)))
        {

        }

        public MayaBinaryParser(BinaryReader stream)
        {
            _stream = stream;
        }

        public void Parse()
        {
            HandleChunk(ParseChunk());
        }

        private Chunk ParseChunk()
        {
            UInt64 chunkStart = (UInt64) _stream.BaseStream.Position;
            UInt32 type = _stream.ReadUInt32BE();
            _stream.ReadUInt32BE();
            UInt64 dataLength = _stream.ReadUInt64BE();
            UInt64 dataStart = (UInt64) _stream.BaseStream.Position;

            if (IsGroup(type))
            {
                UInt32 groupType = _stream.ReadUInt32BE();
                return new GroupChunk(type, groupType, dataStart, dataLength);
            }
            else
            {
                return new Chunk(type, dataStart, dataLength);
            }
        }

        private void HandleChunk(Chunk chunk)
        {
            Debug.WriteLine(chunk);

            Int64 chunkEnd = (Int64)(chunk.DataStart + chunk.DataLength);

            switch(chunk)
            {
                case GroupChunk c:
                    while (_stream.BaseStream.Position < chunkEnd)
                    {
                        Chunk child = ParseChunk();
                        c.Children.Add(child);
                        HandleChunk(child);
                    }
                    break;
                default:
                    if ((UInt64)_stream.BaseStream.Position != chunk.ChunkEnd)
                    {
                        Debug.WriteLine($"skipped data to: {chunk.ChunkEnd} ({chunk.ChunkEnd:X})");
                    }

                    _stream.BaseStream.Position = (Int64)chunk.ChunkEnd;
                    break;
            }
        }

        private static bool IsGroup(UInt32 chunkId)
        {
            if (chunkId == FOR8) return true;
            if (chunkId == LIS8) return true;
            if (chunkId == FOR4) return true;
            if (chunkId == LIS4) return true;
            return false;
        }

        private static UInt32 ChunkIdFromString(string id)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(id);

            if (bytes.Length != 4)
            {
                throw new ArgumentException("ChunkId's must be four ascii characters");
            }

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
                return BitConverter.ToUInt32(bytes, 0);
            }
            else
            {
                return BitConverter.ToUInt32(bytes, 0);
            }
        }

        private static string StringFromChunkId(UInt32 chunkId)
        {
            byte[] bytes = BitConverter.GetBytes(chunkId);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return Encoding.ASCII.GetString(bytes);
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _stream?.Dispose();
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
