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

using System.Runtime.InteropServices;

namespace Laurent.Lee.CLB.Win32
{
    /// <summary>
    ///     kernel32.dll API
    /// </summary>
    public static class TmphKernel32
    {
        /// <summary>
        ///     内存复制
        /// </summary>
        /// <param name="dest">目标位置</param>
        /// <param name="src">源位置</param>
        /// <param name="length">字节长度</param>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern unsafe void RtlMoveMemory(void* dest, void* src, int length);

        /// <summary>
        ///     获取指定磁盘的信息，包括磁盘的可用空间。
        /// </summary>
        /// <param name="bootPath">磁盘根目录。如：@"C:\"</param>
        /// <param name="sectorsPerCluster">每个簇所包含的扇区个数</param>
        /// <param name="bytesPerSector">每个扇区所占的字节数</param>
        /// <param name="numberOfFreeClusters">空闲的簇的个数</param>
        /// <param name="totalNumberOfClusters">簇的总个数</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool GetDiskFreeSpace(string bootPath, out uint sectorsPerCluster,
            out uint bytesPerSector, out uint numberOfFreeClusters, out uint totalNumberOfClusters);
    }
}