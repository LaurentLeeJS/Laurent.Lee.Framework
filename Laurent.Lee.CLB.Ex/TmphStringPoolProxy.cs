using System;
using System.Reflection;

#if MONO
#else

namespace Laurent.Lee.CLB
{
    public static class TmphStringPoolProxy
    {
        public static Func<int, TmphStringPool> GetPool = (Func<int, TmphStringPool>)Delegate.CreateDelegate(typeof(Func<int, TmphStringPool>), typeof(TmphStringPool).GetMethod("GetPool", BindingFlags.NonPublic | BindingFlags.Static));
    }
}

#endif