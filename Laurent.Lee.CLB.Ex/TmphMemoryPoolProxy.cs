using System;
using System.Reflection;

#if MONO
#else

namespace Laurent.Lee.CLB
{
    public static class TmphMemoryPoolProxy
    {
        public static Func<int, TmphMemoryPool> GetPool = (Func<int, TmphMemoryPool>)Delegate.CreateDelegate(typeof(Func<int, TmphMemoryPool>), typeof(TmphMemoryPool).GetMethod("GetPool", BindingFlags.NonPublic | BindingFlags.Static));
    }
}

#endif