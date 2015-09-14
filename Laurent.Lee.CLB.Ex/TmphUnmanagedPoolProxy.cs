using System;
using System.Reflection;

#if MONO
#else

namespace Laurent.Lee.CLB
{
    public static class TmphUnmanagedPoolProxy
    {
        public static Func<int, TmphUnmanagedPool> GetPool = (Func<int, TmphUnmanagedPool>)Delegate.CreateDelegate(typeof(Func<int, TmphUnmanagedPool>), typeof(TmphUnmanagedPool).GetMethod("GetPool", BindingFlags.NonPublic | BindingFlags.Static));
    }
}

#endif