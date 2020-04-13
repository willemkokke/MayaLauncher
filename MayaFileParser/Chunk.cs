using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace MayaFileParser
{
    public partial class IFFParser
    {
        public class Chunk
        {
            public readonly Int32 ChunkId;
            public readonly Int32 Alignment;
            public readonly Int64 DataStart;
            public readonly Int64 DataLength;
            public readonly Int64 ChunkEnd;

            // IFF group chunks
            public static readonly Int32 FORM = ChunkIdFromString("FORM");
            public static readonly Int32 FOR4 = ChunkIdFromString("FOR4");
            public static readonly Int32 FOR8 = ChunkIdFromString("FOR8");
            public static readonly Int32 LIST = ChunkIdFromString("LIST");
            public static readonly Int32 LIS4 = ChunkIdFromString("LIS4");
            public static readonly Int32 LIS8 = ChunkIdFromString("LIS8");
            public static readonly Int32 CAT_ = ChunkIdFromString("CAT ");
            public static readonly Int32 CAT4 = ChunkIdFromString("CAT4");
            public static readonly Int32 CAT8 = ChunkIdFromString("CAT8");
            public static readonly Int32 PROP = ChunkIdFromString("PROP");
            public static readonly Int32 PRO4 = ChunkIdFromString("PRO4");
            public static readonly Int32 PRO8 = ChunkIdFromString("PRO8");

            protected Chunk(Int32 chunkId, Int64 dataStart, Int64 dataLength, GroupChunk parent = null)
            {
                ChunkId = chunkId;
                DataStart = dataStart;
                DataLength = dataLength;

                if (IsGroup())
                {
                    Alignment = GetGroupAlignment(chunkId);
                    ChunkEnd = dataStart + dataLength;
                }
                else
                {
                    Alignment = parent.Alignment;
                    ChunkEnd = dataStart + Align(dataLength, Alignment);
                }

                if (parent != null)
                {
                    parent.Children.Add(this);
                }
            }

            public bool IsFORM()
            {
                if (ChunkId == FOR8) return true;
                if (ChunkId == FOR4) return true;
                if (ChunkId == FORM) return true;
                return false;
            }

            public bool IsLIST()
            {
                if (ChunkId == LIS8) return true;
                if (ChunkId == LIS4) return true;
                if (ChunkId == LIST) return true;
                return false;
            }

            public bool IsCAT()
            {
                if (ChunkId == CAT8) return true;
                if (ChunkId == CAT4) return true;
                if (ChunkId == CAT_) return true;
                return false;
            }

            // Read the whole chunk as a string.
            public string ReadAsString(BinaryReader stream)
            {
                byte[] bytes = stream.ReadBytes((int)DataLength);
                return Encoding.ASCII.GetString(bytes);
            }

            // Read a null terminated string.
            public string ReadString(BinaryReader stream)
            {
                byte b;
                List<byte> bytes = new List<byte>();
                while ((b = stream.ReadByte()) != 0)
                {
                    bytes.Add(b);
                }
                return Encoding.ASCII.GetString(bytes.ToArray());
            }

            public override string ToString()
            {
                return $"Chunk: {StringFromChunkId(ChunkId)}, Data Start: {DataStart}, Data Length: {DataLength} End: {ChunkEnd}";
            }

            public static Int32 ChunkIdFromString(string id)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(id);

                if (bytes.Length != 4)
                {
                    throw new ArgumentException("ChunkId's must be four ascii characters");
                }

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bytes);
                    return BitConverter.ToInt32(bytes, 0);
                }
                return BitConverter.ToInt32(bytes, 0);
            }

            public static string StringFromChunkId(Int32 chunkId)
            {
                byte[] bytes = BitConverter.GetBytes(chunkId);

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bytes);
                }
                return Encoding.ASCII.GetString(bytes);
            }

            internal static Chunk ParseFromStream(BinaryReader stream, GroupChunk parent = null)
            {
                Int32 type = stream.ReadInt32BE();
                Int32 alignment = parent != null ? parent.Alignment : Chunk.GetGroupAlignment(type);

                Int64 dataLength;
                switch (alignment)
                {
                    case 8:
                        stream.ReadInt32BE();
                        dataLength = stream.ReadInt64BE();
                        break;
                    default:
                        dataLength = stream.ReadInt32BE();
                        break;
                }

                Int64 dataStart = stream.BaseStream.Position;

                if (IsGroup(type))
                {
                    Int32 groupType = stream.ReadInt32BE();
                    return new GroupChunk(type, groupType, dataStart, dataLength, parent);
                }
                else
                {
                    return new Chunk(type, dataStart, dataLength, parent);
                }
            }

            public bool IsGroup()
            {
                return IsGroup(ChunkId);
            }

            private static bool IsGroup(Int32 chunkId)
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

            private static Int32 GetGroupAlignment(Int32 chunkId)
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

            private static Int64 Align(Int64 value, Int32 alignment)
            {
                Int64 mask = alignment - 1;
                Int64 inverse_mask = ~mask;
                bool isAligned = (value & mask) == 0;
                if (!isAligned)
                {
                    return (value & inverse_mask) + alignment;
                }
                return value;
            }
        }
    }
}
