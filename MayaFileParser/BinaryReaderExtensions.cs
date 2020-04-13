using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MayaFileParser
{
    static class BinaryReaderExtensions
    {
        public static UInt16 ReadUInt16LE(this BinaryReader reader)
        {
            if (BitConverter.IsLittleEndian)
            {
                return reader.ReadUInt16();
            }
            else
            {
                return BitConverter.ToUInt16(reader.ReadBytes(sizeof(UInt16)).Reverse(), 0);
            }
        }

        public static Int16 ReadInt16LE(this BinaryReader reader)
        {
            if (BitConverter.IsLittleEndian)
            {
                return reader.ReadInt16();
            }
            else
            {
                return BitConverter.ToInt16(reader.ReadBytes(sizeof(Int16)).Reverse(), 0);
            }
        }

        public static UInt32 ReadUInt32LE(this BinaryReader reader)
        {
            if (BitConverter.IsLittleEndian)
            {
                return reader.ReadUInt32();
            }
            else
            {
                return BitConverter.ToUInt32(reader.ReadBytes(sizeof(UInt32)).Reverse(), 0);
            }
        }

        public static Int32 ReadInt32LE(this BinaryReader reader)
        {
            if (BitConverter.IsLittleEndian)
            {
                return reader.ReadInt32();
            }
            else
            {
                return BitConverter.ToInt32(reader.ReadBytes(sizeof(Int32)).Reverse(), 0);
            }
        }

        public static UInt16 ReadUInt16BE(this BinaryReader reader)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToUInt16(reader.ReadBytes(sizeof(UInt16)).Reverse(), 0);
            }
            else
            {
                return reader.ReadUInt16();
            }
        }

        public static Int16 ReadInt16BE(this BinaryReader reader)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToInt16(reader.ReadBytes(sizeof(Int16)).Reverse(), 0);
            }
            else
            {
                return reader.ReadInt16();
            }
        }

        public static UInt32 ReadUInt32BE(this BinaryReader reader)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToUInt32(reader.ReadBytes(sizeof(UInt32)).Reverse(), 0);
            }
            else
            {
                return reader.ReadUInt32(); ;
            }
        }

        public static Int32 ReadInt32BE(this BinaryReader reader)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToInt32(reader.ReadBytes(sizeof(Int32)).Reverse(), 0);
            }
            else
            {
                return reader.ReadInt32();
            }
        }

        public static UInt64 ReadUInt64BE(this BinaryReader reader)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToUInt64(reader.ReadBytes(sizeof(UInt64)).Reverse(), 0);
            }
            else
            {
                return reader.ReadUInt64(); ;
            }
        }

        public static Int64 ReadInt64BE(this BinaryReader reader)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToInt64(reader.ReadBytes(sizeof(Int64)).Reverse(), 0);
            }
            else
            {
                return reader.ReadInt64();
            }
        }

        private static byte[] Reverse(this byte[] b)
        {
            Array.Reverse(b);
            return b;
        }
    }
}
