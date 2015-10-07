/*
-------------------------------------------------- -----------------------------------------
The frame content is protected by copyright law. In order to facilitate individual learning,
allows to download the program source information, but does not allow individuals or a third
party for profit, the commercial use of the source information. Without consent,
does not allow any form (even if partial, or modified) database storage,
copy the source of information. If the source content provided by third parties,
which corresponds to the third party content is also protected by copyright.

If you are found to have infringed copyright behavior, please give me a hint. THX!

Here in particular it emphasized that the third party is not allowed to contact addresses
published in this "version copyright statement" to send advertising material.
I will take legal means to resist sending spam.
-------------------------------------------------- ----------------------------------------
The framework under the GNU agreement, Detail View GNU License.
If you think about this item affection join the development team,
Please contact me: LaurentLeeJS@gmail.com
-------------------------------------------------- ----------------------------------------
Laurent.Lee.Framework Coded by Laurent Lee
*/

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