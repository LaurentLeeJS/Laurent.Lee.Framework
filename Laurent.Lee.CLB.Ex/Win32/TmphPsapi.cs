using System;
using System.Runtime.InteropServices;

namespace Laurent.Lee.CLB.Win32
{
    /// <summary>
    /// psapi.dll API
    /// </summary>
    public static class TmphPsapi
    {
        /// <summary>
        /// 获取进程内存统计信息
        /// </summary>
        /// <param name="hProcess">进程句柄</param>
        /// <param name="counters">进程内存统计信息</param>
        /// <param name="size">进程内存统计信息字节数</param>
        /// <returns>是否获取成功</returns>
        [DllImport("psapi.dll", SetLastError = true)]
        public static extern bool GetProcessMemoryInfo(IntPtr hProcess, out Diagnostics.TmphProcess.TmphMemoryCounters counters, uint size);
    }
}