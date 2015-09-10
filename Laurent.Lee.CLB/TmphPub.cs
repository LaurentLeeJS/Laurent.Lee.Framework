using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Laurent.Lee.CLB
{
    public static class TmphPub
    {
        public const string LaurentLeeFramework = "LaurentLeeFramework";
        public const int RadixSortSize = 1 << 9;
        public const int RadixSortSize64 = 4 << 9;
        public const char NullChar = (char)0;
        public static readonly string ApplicationPath = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).FullName.ToLower();
        public static readonly int CpuCount = Environment.ProcessorCount;
        public static readonly DateTime StartTime = DateTime.Now;
        public static readonly DateTime MinTime = new DateTime(1900, 1, 1);
        public static readonly Encoding Gb2312 = Encoding.GetEncoding("GB2312");
        public static readonly Encoding Gb18030 = Encoding.GetEncoding("GB18030");
        public static readonly Encoding Gbk = Encoding.GetEncoding("GBK");
        public static readonly Encoding Big5 = Encoding.GetEncoding("BIG5");
        private static int identity32;
        private static long identity;
        private static readonly SHA1CryptoServiceProvider sha1CryptoServiceProvider = new SHA1CryptoServiceProvider();
        private static readonly object sha1Lock = new object();
        public static readonly int MemoryBits;
        public static readonly int MemoryBytes;

        static unsafe TmphPub()
        {
            byte* bytes = stackalloc byte[4];
            if (((long)bytes >> 32) == 0)
                MemoryBits = ((long)(bytes + 0x100000000L) >> 32) == 0 ? 32 : 64;
            else MemoryBits = 64;
            MemoryBytes = MemoryBits >> 3;
        }

        internal static int Identity32
        {
            get { return Interlocked.Increment(ref identity32); }
        }

        public static long Identity
        {
            get { return Interlocked.Increment(ref identity); }
        }

        public static byte[] Sha1(byte[] TmphBuffer, int startIndex, int length)
        {
            Monitor.Enter(sha1Lock);
            try
            {
                TmphBuffer = sha1CryptoServiceProvider.ComputeHash(TmphBuffer, startIndex, length);
            }
            finally
            {
                Monitor.Exit(sha1Lock);
            }
            return TmphBuffer;
        }

        public static void Dispose<TValueType>(TValueType resource)
            where TValueType : class, IDisposable
        {
            if (resource != null)
            {
                try
                {
                    resource.Dispose();
                }
                catch (Exception exception)
                {
                    TmphLog.Default.Add(exception, null, false);
                }
            }
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        /// <typeparam name="TValueType">资源类型</typeparam>
        /// <param name="resource">资源引用</param>
        public static void Dispose<TValueType>(ref TValueType resource)
            where TValueType : class, IDisposable
        {
            Exception exception = null;
            Dispose(ref resource, ref exception);
        }

        public static void Dispose<TValueType>(ref TValueType resource, ref Exception exception)
            where TValueType : class, IDisposable
        {
            var value = Interlocked.Exchange(ref resource, null);
            if (value != null)
            {
                try
                {
                    value.Dispose();
                }
                catch (Exception error)
                {
                    exception = error;
                }
            }
        }

        public static void Dispose<TValueType>(object accessLock, ref TValueType resource)
            where TValueType : class, IDisposable
        {
            Exception exception = null;
            Dispose(accessLock, ref resource, ref exception);
        }

        public static void Dispose<TValueType>(object accessLock, ref TValueType resource, ref Exception exception)
            where TValueType : class, IDisposable
        {
            TValueType value = resource;
            if (value != null)
            {
                Monitor.Enter(accessLock);
                try
                {
                    if (resource != null)
                    {
                        value.Dispose();
                        resource = null;
                    }
                }
                catch (Exception error)
                {
                    exception = error;
                }
                finally
                {
                    Monitor.Exit(accessLock);
                }
            }
        }

        public static TValueType Action<TValueType>(this TValueType value, Action<TValueType> method)
            where TValueType : class
        {
            if (method == null)
                TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            if (value != null)
                method(value);
            return value;
        }
    }
}