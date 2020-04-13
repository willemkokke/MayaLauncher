using System;
using System.Collections.Generic;
using System.IO;

namespace MayaFileParser
{
    public partial class IFFParser : IDisposable
    {
        public Chunk Root { get; private set; }

        protected BinaryReader stream;

        private bool abort = false;

        public delegate void GroupHandler(GroupChunk group);
        public delegate void ChunkHandler(Chunk chunk);

        private Dictionary<Int32, GroupHandler> groupHandlers = new Dictionary<Int32, GroupHandler>();
        private Dictionary<Int32, ChunkHandler> chunkHandlers = new Dictionary<Int32, ChunkHandler>();

        public IFFParser(string file) : this(new BinaryReader(File.OpenRead(file)))
        {

        }

        public IFFParser(BinaryReader stream)
        {
            this.stream = stream;
        }

        protected void RegisterGroupHandler(Int32 chunkId, GroupHandler handler)
        {
            groupHandlers[chunkId] = handler;
        }

        protected void RegisterChunkHandler(Int32 chunkId, ChunkHandler handler)
        {
            chunkHandlers[chunkId] = handler;
        }

        public void Parse(Int32 type)
        {
            foreach (Chunk chunk in Stream(null, type))
            {

            }
        }

        public IEnumerable<Chunk> Stream(GroupChunk parent = null, Int32 type = 0)
        {
            GroupChunk root = parent;
            if (root == null)
            {
                Chunk chunk = Chunk.ParseFromStream(stream);
                root = chunk as GroupChunk;
                if (Root == null)
                {
                    Root = root;
                }
            }

            if (root == null)
            {
                throw new ArgumentException("Parsing has to start with a group chunk!");
            }

            if (type != 0 && root.GroupId != type)
            {
                throw new ArgumentException("Wrong file type, parsing has to start with a group chunk of type " + Chunk.StringFromChunkId(type));
            }

            Stack<GroupChunk> stack = new Stack<GroupChunk>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                if (abort)
                {
                    yield break;
                }

                GroupChunk group = stack.Peek();
                Int64 dataEnd = group.DataStart + group.DataLength;

                if (stream.BaseStream.Position == group.ChildrenStart)
                {
                    if (root != parent)
                    {
                        if (!InvokeGroupHandler(group))
                        {
                            yield return group;
                        }
                    }
                }

                while (stream.BaseStream.Position < dataEnd)
                {
                    if (abort)
                    {
                        yield break;
                    }

                    Chunk child = Chunk.ParseFromStream(stream, group);
                    if (child is GroupChunk)
                    {
                        stack.Push(child as GroupChunk);
                        break;
                    }
                    else
                    {
                        if (!InvokeChunkHandler(child))
                        {
                            yield return child;
                        }

                        stream.BaseStream.Position = child.ChunkEnd;
                    }
                }

                if (stack.Count > 0 && stack.Peek() == group)
                {
                    stack.Pop();
                    stream.BaseStream.Position = group.ChunkEnd;
                }
            }
        }

        public void Abort()
        {
            abort = true;
        }

        private bool InvokeGroupHandler(GroupChunk group)
        {
            if (groupHandlers.ContainsKey(group.GroupId))
            {
                GroupHandler handler = groupHandlers[group.GroupId];
                groupHandlers.Remove(group.GroupId);
                handler(group);
                groupHandlers[group.GroupId] = handler;
                return true;
            }
            return false;
        }

        private bool InvokeChunkHandler(Chunk chunk)
        {
            if (chunkHandlers.ContainsKey(chunk.ChunkId))
            {
                ChunkHandler handler = chunkHandlers[chunk.ChunkId];
                chunkHandlers.Remove(chunk.ChunkId);
                handler(chunk);
                chunkHandlers[chunk.ChunkId] = handler;
                return true;
            }
            return false;
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
