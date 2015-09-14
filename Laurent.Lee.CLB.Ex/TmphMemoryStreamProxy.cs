using System;
using System.IO;
using System.Reflection;

#if MONO
#else

namespace Laurent.Lee.CLB
{
    public static class TmphMemoryStreamProxy
    {
        public static Func<byte[], MemoryStream> Get = (Func<byte[], MemoryStream>)Delegate.CreateDelegate(typeof(Func<byte[], MemoryStream>), typeof(TmphMemoryStream).GetMethod("Get", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(byte[]) }, null));
    }
}

#endif