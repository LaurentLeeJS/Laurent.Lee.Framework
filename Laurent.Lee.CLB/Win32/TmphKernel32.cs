/*
 *  Copyright 2015 Tony Lee

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 */

using System;
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