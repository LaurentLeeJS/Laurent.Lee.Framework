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

using Laurent.Lee.CLB.Config;
using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Laurent.Lee.CLB.MemoryDataBase
{
    /// <summary>
    ///     数据库物理层集合
    /// </summary>
    internal sealed class TmphPhysicalSet : IDisposable
    {
        /// <summary>
        ///     错误索引
        /// </summary>
        private const int errorIndex = int.MinValue;

        /// <summary>
        ///     数据库物理层集合
        /// </summary>
        public static readonly TmphPhysicalSet Default = new TmphPhysicalSet();

        /// <summary>
        ///     数据库物理层文件名与索引集合
        /// </summary>
        private readonly Dictionary<TmphHashString, int> fileNameIndexs = TmphDictionary.CreateHashString<int>();

        /// <summary>
        ///     数据库物理层空闲索引集合
        /// </summary>
        private TmphSubArray<int> freeIndexs;

        /// <summary>
        ///     是否已经释放资源
        /// </summary>
        private int isDisposed;

        /// <summary>
        ///     数据库物理层最大索引号
        /// </summary>
        private int maxIndex;

        /// <summary>
        ///     数据库物理层集合访问锁
        /// </summary>
        private int physicalLock;

        /// <summary>
        ///     数据库物理层集合
        /// </summary>
        private TmphPhysicalInfo[] physicals = new TmphPhysicalInfo[255];

        static TmphPhysicalSet()
        {
            if (TmphAppSetting.IsCheckMemory) TmphCheckMemory.Add(MethodBase.GetCurrentMethod().DeclaringType);
        }

        /// <summary>
        ///     数据库物理层集合
        /// </summary>
        private TmphPhysicalSet()
        {
            TmphDomainUnload.Add(Dispose);
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Increment(ref isDisposed) == 1)
            {
                TmphDomainUnload.Remove(Dispose, false);
                TmphInterlocked.NoCheckCompareSetSleep0(ref physicalLock);
                var physicals = this.physicals;
                try
                {
                    this.physicals = new TmphPhysicalInfo[this.physicals.Length];
                    fileNameIndexs.Clear();
                    freeIndexs.Empty();
                    while (maxIndex != 0) physicals[--maxIndex].Close(false);
                }
                finally
                {
                    physicalLock = 0;
                }
                physicals = null;
            }
        }

        /// <summary>
        ///     获取一个可用的集合索引
        /// </summary>
        /// <returns>集合索引</returns>
        private int newIndex()
        {
            if (freeIndexs.Count != 0) return freeIndexs.UnsafePop();
            if (maxIndex == physicals.Length)
            {
                var newPhysicals = new TmphPhysicalInfo[maxIndex << 1];
                Array.Copy(physicals, 0, newPhysicals, 0, maxIndex);
                physicals = newPhysicals;
            }
            return maxIndex++;
        }

        /// <summary>
        ///     获取数据库物理层集合唯一标识
        /// </summary>
        /// <param name="fileName">数据文件名</param>
        /// <returns>数据库物理层集合唯一标识</returns>
        internal TmphIdentity GetIdentity(string fileName)
        {
            int index;
            var identity = new TmphIdentity { Index = errorIndex };
            TmphHashString key = fileName;
            if (fileNameIndexs.TryGetValue(key, out index))
            {
                identity.Identity = physicals[index].Identity;
                int nextIndex;
                if (fileNameIndexs.TryGetValue(key, out nextIndex) && index == nextIndex) identity.Index = index;
            }
            return identity;
        }

        /// <summary>
        ///     打开数据库
        /// </summary>
        /// <param name="fileName">数据文件名</param>
        /// <returns>数据库物理层初始化信息</returns>
        internal TmphPhysicalServer.TmphPhysicalIdentity Open(string fileName)
        {
            var physicalInfo = new TmphPhysicalServer.TmphPhysicalIdentity
            {
                Identity = new TmphPhysicalServer.TmphTimeIdentity { TimeTick = 0, Index = -1 }
            };
            if (isDisposed == 0)
            {
                TmphHashString key = fileName;
                TmphInterlocked.NoCheckCompareSetSleep0(ref physicalLock);
                try
                {
                    if (!fileNameIndexs.ContainsKey(key))
                        fileNameIndexs.Add(key, physicalInfo.Identity.Index = newIndex());
                }
                finally
                {
                    physicalLock = 0;
                }
                if (physicalInfo.Identity.Index != -1)
                {
                    try
                    {
                        var physical = new TmphPhysical(fileName, false);
                        if (!physical.IsDisposed)
                        {
                            TmphInterlocked.NoCheckCompareSetSleep0(ref physicalLock);
                            physicals[physicalInfo.Identity.Index].Set(fileName, physical);
                            physicalLock = 0;
                            physicalInfo.Identity.Identity = physicals[physicalInfo.Identity.Index].Identity;
                            physicalInfo.Identity.TimeTick = TmphPub.StartTime.Ticks;
                            physicalInfo.IsLoader = physical.IsLoader;
                        }
                    }
                    finally
                    {
                        if (physicalInfo.Identity.TimeTick == 0)
                        {
                            TmphInterlocked.NoCheckCompareSetSleep0(ref physicalLock);
                            try
                            {
                                fileNameIndexs.Remove(key);
                            }
                            finally
                            {
                                physicalLock = 0;
                            }
                        }
                    }
                }
            }
            return physicalInfo;
        }

        /// <summary>
        ///     创建数据库文件
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        /// <param name="header">文件头数据</param>
        /// <returns>是否创建成功</returns>
        internal bool Create(TmphIdentity identity, TmphSubArray<byte> header)
        {
            var physical = physicals[identity.Index];
            if (physical.Identity == identity.Identity)
            {
                if (physical.Physical.Create(header)) return true;
                Close(identity, false);
            }
            return false;
        }

        /// <summary>
        ///     关闭数据库文件
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        /// <param name="isWait">是否等待关闭</param>
        internal void Close(TmphIdentity identity, bool isWait)
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref physicalLock);
            var physical = physicals[identity.Index];
            try
            {
                if (physical.Identity == identity.Identity)
                {
                    physicals[identity.Index].Clear();
                    fileNameIndexs.Remove(physical.FileName);
                    freeIndexs.Add(identity.Index);
                }
            }
            finally
            {
                physicalLock = 0;
            }
            if (physical.Identity == identity.Identity) physical.Close(isWait);
        }

        /// <summary>
        ///     数据库文件头数据加载
        /// </summary>
        /// <param name="identity"></param>
        /// <returns>文件数据,null表示失败</returns>
        internal TmphSubArray<byte> LoadHeader(TmphIdentity identity)
        {
            var physical = physicals[identity.Index];
            if (physical.Identity == identity.Identity)
            {
                var data = physical.Physical.LoadHeader();
                if (data.array != null) return data;
                Close(identity, false);
            }
            return default(TmphSubArray<byte>);
        }

        /// <summary>
        ///     数据库文件数据加载
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        /// <returns>文件数据,空数组表示结束,null表示失败</returns>
        internal TmphSubArray<byte> Load(TmphIdentity identity)
        {
            var physical = physicals[identity.Index];
            if (physical.Identity == identity.Identity)
            {
                var data = physical.Physical.Load();
                if (data.array != null) return data;
                Close(identity, false);
            }
            return default(TmphSubArray<byte>);
        }

        /// <summary>
        ///     数据库文件加载完毕
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        /// <returns>是否加载成功</returns>
        internal bool Loaded(TmphIdentity identity, bool isLoaded)
        {
            var physical = physicals[identity.Index];
            if (physical.Identity == identity.Identity)
            {
                if (physical.Physical.Loaded(isLoaded)) return true;
                Close(identity, false);
            }
            return false;
        }

        /// <summary>
        ///     写入日志
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        /// <param name="data">日志数据</param>
        /// <returns>是否成功写入缓冲区</returns>
        internal int Append(TmphIdentity identity, TmphSubArray<byte> data)
        {
            var physical = physicals[identity.Index];
            if (physical.Identity == identity.Identity)
            {
                var value = physical.Physical.Append(data);
                if (value != 0) return value;
                Close(identity, false);
            }
            return 0;
        }

        /// <summary>
        ///     等待缓存写入
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        internal void WaitBuffer(TmphIdentity identity)
        {
            var physical = physicals[identity.Index];
            if (physical.Identity == identity.Identity) physical.Physical.WaitBuffer();
        }

        /// <summary>
        ///     刷新缓存
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        /// <returns>是否成功</returns>
        internal bool Flush(TmphIdentity identity)
        {
            var physical = physicals[identity.Index];
            if (physical.Identity == identity.Identity)
            {
                if (physical.Physical.Flush()) return true;
                Close(identity, false);
            }
            return false;
        }

        /// <summary>
        ///     刷新写入文件缓存区
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        /// <param name="isDiskFile">是否写入到磁盘文件</param>
        /// <returns>是否成功</returns>
        internal bool FlushFile(TmphIdentity identity, bool isDiskFile)
        {
            var physical = physicals[identity.Index];
            if (physical.Identity == identity.Identity)
            {
                if (physical.Physical.FlushFile(isDiskFile)) return true;
                Close(identity, false);
            }
            return false;
        }

        /// <summary>
        ///     数据库物理层集合唯一标识
        /// </summary>
        public struct TmphIdentity
        {
            /// <summary>
            ///     数据库物理层集合索引编号
            /// </summary>
            public int Identity;

            /// <summary>
            ///     数据库物理层集合索引
            /// </summary>
            public int Index;

            /// <summary>
            ///     索引是否有效
            /// </summary>
            public bool IsValid
            {
                get { return Index != errorIndex; }
            }

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="value">比较值</param>
            /// <returns>0表示相等</returns>
            public int Equals(TmphIdentity value)
            {
                return (Index ^ value.Index) | (Identity ^ value.Identity);
            }

            /// <summary>
            ///     索引无效是设置索引
            /// </summary>
            /// <param name="value">目标值</param>
            /// <returns>是否成功</returns>
            public bool SetIfNull(TmphIdentity value)
            {
                if (Index == errorIndex)
                {
                    Index = value.Index;
                    Identity = value.Identity;
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        ///     数据库物理层集合索引编号
        /// </summary>
        private struct TmphPhysicalInfo
        {
            /// <summary>
            ///     数据文件名
            /// </summary>
            public TmphHashString FileName;

            /// <summary>
            ///     索引编号
            /// </summary>
            public int Identity;

            /// <summary>
            ///     数据库物理层
            /// </summary>
            public TmphPhysical Physical;

            /// <summary>
            ///     设置数据库物理层
            /// </summary>
            /// <param name="fileName">数据文件名</param>
            /// <param name="physical">数据库物理层</param>
            public void Set(string fileName, TmphPhysical physical)
            {
                FileName = fileName;
                Physical = physical;
            }

            /// <summary>
            ///     清除数据库物理层
            /// </summary>
            /// <param name="identity">索引编号</param>
            public void Clear()
            {
                Physical = null;
                FileName.Null();
                ++Identity;
            }

            /// <summary>
            ///     关闭数据库物理层
            /// </summary>
            /// <param name="isWait">是否等待结束</param>
            public void Close(bool isWait)
            {
                if (Physical != null)
                {
                    if (isWait) Physical.Dispose();
                    else TmphThreadPool.TinyPool.Start(Physical.Dispose);
                }
            }
        }
    }
}