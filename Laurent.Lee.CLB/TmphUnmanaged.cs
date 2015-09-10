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
using System.Threading;

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     非托管内存
    /// </summary>
    public static unsafe class TmphUnmanaged
    {
        ///// <summary>
        ///// 指针
        ///// </summary>
        //private byte* data;
        ///// <summary>
        ///// 指针
        ///// </summary>
        //public byte* Data
        //{
        //    get { return data; }
        //}
        ///// <summary>
        ///// 字节长度
        ///// </summary>
        //private int size;
        ///// <summary>
        ///// 非托管内存
        ///// </summary>
        ///// <param name="size">内存字节数</param>
        ///// <param name="isClear">是否需要清除</param>
        //public unmanaged(int size, bool isClear = true)
        //{
        //    data = Get(size, isClear);
        //    this.size = size;
        //}
        ///// <summary>
        ///// 释放内存
        ///// </summary>
        //public void Free()
        //{
        //    Free(data);
        //    data = null;
        //    size = 0;
        //}
        /// <summary>
        ///     未释放非托管内存句柄数量
        /// </summary>
        private static int _usedCount;

        /// <summary>
        ///     未释放非托管内存句柄数量
        /// </summary>
        public static int UsedCount
        {
            get { return _usedCount; }
        }

        /// <summary>
        ///     申请非托管内存
        /// </summary>
        /// <param name="size">内存字节数</param>
        /// <param name="isClear">是否需要清除</param>
        /// <returns>非托管内存起始指针</returns>
        public static TmphPointer Get(int size, bool isClear = true)
        {
            if (size < 0) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
            if (size != 0)
            {
                var data = (byte*)Marshal.AllocHGlobal(size);
                Interlocked.Increment(ref _usedCount);
                if (isClear) Unsafe.TmphMemory.Fill(data, 0, size);
                return new TmphPointer { Data = data };
            }
            return default(TmphPointer);
        }

        /// <summary>
        ///     批量申请非托管内存
        /// </summary>
        /// <param name="isClear">是否需要清除</param>
        /// <param name="sizes">内存字节数集合</param>
        /// <returns>非托管内存起始指针</returns>
        public static TmphPointer[] Get(bool isClear, params int[] sizes)
        {
            if (sizes.length() != 0)
            {
                var sum = 0;
                foreach (var size in sizes)
                {
                    if (size < 0) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                    checked
                    {
                        sum += size;
                    }
                }
                var pointer = Get(sum, isClear);
                var data = pointer.Byte;
                if (data != null)
                {
                    var index = 0;
                    var datas = new TmphPointer[sizes.Length];
                    foreach (var size in sizes)
                    {
                        datas[index++] = new TmphPointer { Data = data };
                        data += size;
                    }
                    return datas;
                }
            }
            return null;
        }

        /// <summary>
        ///     释放内存
        /// </summary>
        /// <param name="data">非托管内存起始指针</param>
        public static void Free(ref TmphPointer data)
        {
            if (data.Data != null)
            {
                Interlocked.Decrement(ref _usedCount);
                Marshal.FreeHGlobal((IntPtr)data.Data);
                data.Data = null;
            }
        }

        /// <summary>
        ///     释放内存
        /// </summary>
        /// <param name="data">非托管内存起始指针</param>
        public static void Free(void* data)
        {
            if (data != null)
            {
                Interlocked.Decrement(ref _usedCount);
                Marshal.FreeHGlobal((IntPtr)data);
            }
        }
    }
}