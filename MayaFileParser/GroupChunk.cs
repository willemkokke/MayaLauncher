using System;
using System.Collections.Generic;

namespace MayaFileParser
{
    public partial class IFFParser
    {
        public sealed class GroupChunk : Chunk
        {
            public readonly Int32 GroupId;
            public List<Chunk> Children = new List<Chunk>();

            public Int64 ChildrenStart { get { return DataStart + 4; } }

            internal GroupChunk(Int32 chunkId, Int32 groupId, Int64 dataStart, Int64 dataLength, GroupChunk parent = null) : base(chunkId, dataStart, dataLength, parent)
            {
                GroupId = groupId;
            }

            public override string ToString()
            {
                return $"GroupChunk: {StringFromChunkId(ChunkId)}, Type: {StringFromChunkId(GroupId)}, Data Start: {DataStart}, Data Length: {DataLength}, End: {ChunkEnd}";
            }
        }
    }
}
