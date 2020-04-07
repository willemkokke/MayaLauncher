using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace MayaLauncher
{
    public class MayaIFFParser : IDisposable
    {
        public class Chunk
        {
            public readonly UInt32 ChunkId;
            public readonly UInt64 DataStart;
            public readonly UInt64 DataLength;
            public readonly UInt64 ChunkEnd;

            internal Chunk(UInt32 chunkId, UInt64 dataStart, UInt64 dataLength, UInt32 alignment)
            {
                ChunkId = chunkId;
                DataStart = dataStart;
                DataLength = dataLength;

                if (MayaIFFParser.IsGroup(chunkId))
                {
                    ChunkEnd = dataStart + dataLength;
                }
                else
                {
                    ChunkEnd = dataStart + Align(dataLength, alignment);
                }
            }

            protected static UInt64 Align(UInt64 value, UInt32 alignment)
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

            public override string ToString()
            {
                return $"Chunk: {StringFromChunkId(ChunkId)}, Data Start: {DataStart}, Data Length: {DataLength} End: {ChunkEnd}";
            }
        }

        public class GroupChunk : Chunk
        {
            public readonly UInt32 GroupId;
            public List<Chunk> Children = new List<Chunk>();

            internal GroupChunk(UInt32 chunkId, UInt32 groupId, UInt64 dataStart, UInt64 dataLength, UInt32 alignment) : base(chunkId, dataStart, dataLength, alignment)
            {
                GroupId = groupId;
            }

            public override string ToString()
            {
                return $"GroupChunk: {StringFromChunkId(ChunkId)}, Type: {StringFromChunkId(GroupId)}, Data Start: {DataStart}, Data Length: {DataLength}, End: {ChunkEnd}";
            }
        }

        public Chunk Root { get; private set; }

        private BinaryReader stream;
        private UInt32 currentAlignment;

        // IFF group chunks
        protected static readonly UInt32 FORM = ChunkIdFromString("FORM");
        protected static readonly UInt32 FOR4 = ChunkIdFromString("FOR4");
        protected static readonly UInt32 FOR8 = ChunkIdFromString("FOR8");
        protected static readonly UInt32 LIST = ChunkIdFromString("LIST");
        protected static readonly UInt32 LIS4 = ChunkIdFromString("LIS4");
        protected static readonly UInt32 LIS8 = ChunkIdFromString("LIS8");
        protected static readonly UInt32 CAT_ = ChunkIdFromString("CAT ");
        protected static readonly UInt32 CAT4 = ChunkIdFromString("CAT4");
        protected static readonly UInt32 CAT8 = ChunkIdFromString("CAT8");
        protected static readonly UInt32 PROP = ChunkIdFromString("PROP");
        protected static readonly UInt32 PRO4 = ChunkIdFromString("PRO4");
        protected static readonly UInt32 PRO8 = ChunkIdFromString("PRO8");

        public delegate void GroupHandler(GroupChunk group);
        public delegate void ChunkHandler(Chunk chunk);

        private Dictionary<UInt32, GroupHandler> groupHandlers = new Dictionary<UInt32, GroupHandler>();
        private Dictionary<UInt32, ChunkHandler> chunkHandlers = new Dictionary<UInt32, ChunkHandler>();

        public MayaIFFParser(string file) : this(new BinaryReader(File.OpenRead(file)))
        {

        }

        public MayaIFFParser(BinaryReader stream)
        {
            this.stream = stream;
        }

        protected void RegisterGroupHandler(UInt32 chunkId, GroupHandler handler)
        {
            groupHandlers[chunkId] = handler;
        }

        protected void RegisterChunkHandler(UInt32 chunkId, ChunkHandler handler)
        {
            chunkHandlers[chunkId] = handler;
        }

        public void Parse()
        {
            // Determine if this is a 32 or 64 bit maya file
            UInt32 rootType = stream.ReadUInt32BE();
            UInt32 rootAlignment = GetGroupAlignment(rootType);
            stream.BaseStream.Position = 0;

            foreach (Chunk c in Stream())
            {
                Debug.WriteLine(c);
            }

            //Root = ParseChunk(rootAlignment);
            //HandleChunk(Root, rootAlignment);
        }

        public IEnumerable<Chunk> Stream()
        {
            // Determine if this is a 32 or 64 bit maya file
            UInt32 rootType = stream.ReadUInt32BE();
            UInt32 rootAlignment = GetGroupAlignment(rootType);
            stream.BaseStream.Position = 0;

            Root = ParseChunk(rootAlignment);
            yield return Root;

            //HandleChunk(Root, rootAlignment);
            foreach (Chunk chunk in StreamChunk(Root, rootAlignment))
            {
                yield return chunk;
            }
        }

        protected Chunk ParseChunk(UInt32 alignment)
        {
            UInt64 chunkStart = (UInt64) stream.BaseStream.Position;
            UInt32 type = stream.ReadUInt32BE();

            if (alignment == 8)
            {
                stream.ReadUInt32BE();
            }

            UInt64 dataLength = 0;
            switch (alignment)
            {
                case 8:
                    dataLength= stream.ReadUInt64BE();
                    break;
                default:
                    dataLength = stream.ReadUInt32BE();
                    break;
            }

            UInt64 dataStart = (UInt64) stream.BaseStream.Position;

            if (IsGroup(type))
            {
                UInt32 groupType = stream.ReadUInt32BE();
                return new GroupChunk(type, groupType, dataStart, dataLength, alignment);
            }
            else
            {
                return new Chunk(type, dataStart, dataLength, alignment);
            }
        }

        protected IEnumerable<Chunk> StreamChunk(Chunk chunk, UInt32 alignment)
        {
            Int64 dataEnd = (Int64)(chunk.DataStart + chunk.DataLength);

            switch (chunk)
            {
                case GroupChunk c:
                    UInt32 oldAlignment = currentAlignment;
                    currentAlignment = GetGroupAlignment(c.ChunkId);

                    if (!InvokeGroupHandler(c))
                    {
                        while (stream.BaseStream.Position < dataEnd)
                        {
                            Chunk child = ParseChunk(currentAlignment);
                            c.Children.Add(child);
                            yield return child;
                            foreach (Chunk cc in StreamChunk(child, currentAlignment))
                            {
                                yield return cc;
                            }
                        }
                    }
                    currentAlignment = oldAlignment;
                    stream.BaseStream.Position = (Int64)c.ChunkEnd;
                    break;
                default:
                    InvokeChunkHandler(chunk);
                    stream.BaseStream.Position = (Int64)chunk.ChunkEnd;
                    break;
            }
        }

        protected void HandleChunk(Chunk chunk, UInt32 alignment)
        {
            Int64 chunkEnd = (Int64)(chunk.DataStart + chunk.DataLength);

            switch(chunk)
            {
                case GroupChunk c:
                    UInt32 oldAlignment = currentAlignment;
                    currentAlignment = GetGroupAlignment(c.ChunkId);

                    if (!InvokeGroupHandler(c))
                    {
                        while (stream.BaseStream.Position < chunkEnd)
                        {
                            Chunk child = ParseChunk(currentAlignment);
                            c.Children.Add(child);
                            HandleChunk(child, currentAlignment);
                        }
                    }

                    stream.BaseStream.Position = (Int64)c.ChunkEnd;
                    currentAlignment = oldAlignment;
                    break;
                default:
                    InvokeChunkHandler(chunk);
                    stream.BaseStream.Position = (Int64)chunk.ChunkEnd;
                    break;
            }
        }

        private bool InvokeGroupHandler(GroupChunk chunk)
        {
            if (groupHandlers.ContainsKey(chunk.GroupId))
            {
                groupHandlers[chunk.GroupId](chunk);
                return true;
            }
            return false;
        }

        private bool InvokeChunkHandler(Chunk chunk)
        {
            if (chunkHandlers.ContainsKey(chunk.ChunkId))
            {
                chunkHandlers[chunk.ChunkId](chunk);
                return true;
            }
            return false;
        }


        protected static bool IsGroup(UInt32 chunkId)
        {
            if (chunkId == FOR8) return true;
            if (chunkId == FOR4) return true;
            if (chunkId == FORM) return true;
            if (chunkId == LIS8) return true;
            if (chunkId == LIS4) return true;
            if (chunkId == LIST) return true;
            if (chunkId == CAT8) return true;
            if (chunkId == CAT4) return true;
            if (chunkId == CAT_) return true;
            return false;
        }

        protected static UInt32 GetGroupAlignment(UInt32 chunkId)
        {
            if (chunkId == FOR8) return 8;
            if (chunkId == FOR4) return 4;
            if (chunkId == FORM) return 2;
            if (chunkId == LIS8) return 8;
            if (chunkId == LIS4) return 4;
            if (chunkId == LIST) return 2;
            if (chunkId == CAT8) return 8;
            if (chunkId == CAT4) return 4;
            if (chunkId == CAT_) return 2;
            if (chunkId == PRO8) return 8;
            if (chunkId == PRO4) return 4;
            if (chunkId == PROP) return 2;
            return 0;
        }

        protected static UInt32 ChunkIdFromString(string id)
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

        protected static string StringFromChunkId(UInt32 chunkId)
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
