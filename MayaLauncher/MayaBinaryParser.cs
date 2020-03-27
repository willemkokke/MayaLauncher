using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace MayaLauncher
{
    class MayaBinaryParser : IDisposable
    {
        BinaryReader _stream;

        private static UTF8Encoding utf8 = new UTF8Encoding();

        public MayaBinaryParser(string file) : this(new BinaryReader(File.OpenRead(file)))
        {

        }

        public MayaBinaryParser(BinaryReader stream)
        {
            _stream = stream;
        }

        public void Parse()
        {
            Debug.WriteLine(utf8.GetString(_stream.ReadBytes(4)));
            Debug.WriteLine(_stream.ReadUInt32());
            Debug.WriteLine(_stream.ReadUInt64());
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
