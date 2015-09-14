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