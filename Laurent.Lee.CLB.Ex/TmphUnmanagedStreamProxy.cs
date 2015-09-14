using System.IO;

namespace Laurent.Lee.CLB
{
    public sealed unsafe class TmphUnmanagedStreamProxy : TmphUnmanagedStream
    {
        private Stream stream;
        private int prepDepth;

        public TmphUnmanagedStreamProxy(Stream stream, int length = DefaultLength)
            : base(length)
        {
            if (stream == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            this.stream = stream;
        }

        public TmphUnmanagedStreamProxy(Stream stream, byte* data, int dataLength)
            : base(data, dataLength)
        {
            if (stream == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            this.stream = stream;
        }

        public override void PrepLength(int length)
        {
            if (prepDepth == 0) writeStream();
            PrepLength(length);
            ++prepDepth;
        }

        public override void PrepLength()
        {
            --prepDepth;
        }

        public override void Close()
        {
            base.Close();
            Offset = 0;
        }

        public override void Clear()
        {
            base.Clear();
            Offset = 0;
        }

        private void writeStream()
        {
            if (length != 0)
            {
                Offset += length;
                byte* start = Data;
                byte[] buffer = TmphMemoryPool.StreamBuffers.Get();
                try
                {
                    fixed (byte* bufferFixed = buffer)
                    {
                        while (length > buffer.Length)
                        {
                            Unsafe.TmphMemory.Copy(start, bufferFixed, buffer.Length);
                            stream.Write(buffer, 0, buffer.Length);
                            length -= buffer.Length;
                            start += buffer.Length;
                        }
                        Laurent.Lee.CLB.Unsafe.TmphMemory.Copy(start, bufferFixed, length);
                        stream.Write(buffer, 0, length);
                    }
                }
                finally { Laurent.Lee.CLB.TmphMemoryPool.StreamBuffers.Push(ref buffer); }
                length = 0;
            }
        }

        public override void Dispose()
        {
            try
            {
                writeStream();
            }
            finally { base.Dispose(); }
        }
    }
}