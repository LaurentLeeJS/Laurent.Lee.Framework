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

using Laurent.Lee.CLB.Code.CSharp;
using Laurent.Lee.CLB.Config;
using Laurent.Lee.CLB.Net.Tcp;

namespace Laurent.Lee.CLB.MemoryDataBase
{
    /// <summary>
    ///     数据库物理层服务
    /// </summary>
    [Code.CSharp.TmphTcpServer(Service = "memoryDatabasePhysical", IsIdentityCommand = true,
        IsServerAsynchronousReceive = false, IsClientAsynchronousReceive = false,
        VerifyMethodType = typeof(TmphVerifyMethod))]
    public partial class TmphPhysicalServer
    {
        /// <summary>
        ///     数据库物理层服务验证函数
        /// </summary>
        /// <returns>是否验证成功</returns>
        [Code.CSharp.TmphTcpServer(IsVerifyMethod = true, IsServerAsynchronousTask = false, InputParameterMaxLength = 1024
            )]
        protected virtual bool verify(string value)
        {
            if (TmphMemoryDatabase.Default.PhysicalVerify == null && !Config.TmphPub.Default.IsDebug)
            {
                TmphLog.Error.Add("数据库物理层服务验证数据不能为空", false, true);
                return false;
            }
            return TmphMemoryDatabase.Default.PhysicalVerify == value;
        }

        /// <summary>
        ///     打开数据库
        /// </summary>
        /// <param name="fileName">数据文件名</param>
        /// <returns>数据库物理层初始化信息</returns>
        [Code.CSharp.TmphTcpServer(IsClientAsynchronous = true, IsClientSynchronous = false)]
        private TmphPhysicalIdentity open(string fileName)
        {
            return TmphPhysicalSet.Default.Open(fileName);
        }

        /// <summary>
        ///     关闭数据库文件
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        [Code.CSharp.TmphTcpServer(IsServerAsynchronousTask = false)]
        private void close(TmphTimeIdentity identity)
        {
            if (identity.TimeTick == TmphPub.StartTime.Ticks) TmphPhysicalSet.Default.Close(identity.GetIdentity(), false);
        }

        ///// <summary>
        ///// 关闭数据库文件
        ///// </summary>
        ///// <param name="identity">数据库物理层唯一标识</param>
        //[Laurent.Lee.CLB.Code.CSharp.tcpServer]
        //private void waitClose(timeIdentity identity)
        //{
        //    if (identity.TimeTick == Laurent.Lee.CLB.pub.StartTime.Ticks) physicalSet.Default.Close(identity.GetIdentity(), true);
        //}
        ///// <summary>
        ///// 关闭数据库文件
        ///// </summary>
        ///// <param name="identity">数据文件名</param>
        //[Laurent.Lee.CLB.Code.CSharp.tcpServer(IsServerAsynchronousTask = false)]
        //private void close(string fileName)
        //{
        //    physicalSet.identity identity = physicalSet.Default.GetIdentity(fileName);
        //    if (identity.IsValid) physicalSet.Default.Close(identity, false);
        //}
        ///// <summary>
        ///// 关闭数据库文件
        ///// </summary>
        ///// <param name="identity">数据文件名</param>
        //[Laurent.Lee.CLB.Code.CSharp.tcpServer]
        //private void waitClose(string fileName)
        //{
        //    physicalSet.identity identity = physicalSet.Default.GetIdentity(fileName);
        //    if (identity.IsValid) physicalSet.Default.Close(identity, true);
        //}
        /// <summary>
        ///     创建数据库文件
        /// </summary>
        /// <param name="stream">文件头数据流</param>
        /// <returns>是否成功</returns>
        [Code.CSharp.TmphTcpServer(IsServerAsynchronousTask = false)]
        private bool create(TmphTcpBase.TmphSubByteUnmanagedStream stream)
        {
            var identity = getIdentity(ref stream.TmphBuffer);
            return stream.TmphBuffer.array != null && TmphPhysicalSet.Default.Create(identity.GetIdentity(), stream.TmphBuffer);
        }

        /// <summary>
        ///     数据库文件数据加载
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        [Code.CSharp.TmphTcpServer(IsClientCallbackTask = false, IsClientAsynchronous = true, IsClientSynchronous = false)
        ]
        private TmphTcpBase.TmphSubByteArrayBuffer loadHeader(TmphTimeIdentity identity)
        {
            if (identity.TimeTick == TmphPub.StartTime.Ticks)
                return TmphPhysicalSet.Default.LoadHeader(identity.GetIdentity());
            return default(TmphTcpBase.TmphSubByteArrayBuffer);
        }

        /// <summary>
        ///     数据库文件数据加载
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        [Code.CSharp.TmphTcpServer(IsClientCallbackTask = false, IsClientAsynchronous = true, IsClientSynchronous = false)
        ]
        private TmphTcpBase.TmphSubByteArrayBuffer load(TmphTimeIdentity identity)
        {
            if (identity.TimeTick == TmphPub.StartTime.Ticks) return TmphPhysicalSet.Default.Load(identity.GetIdentity());
            return default(TmphTcpBase.TmphSubByteArrayBuffer);
        }

        /// <summary>
        ///     数据库文件加载完毕
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        /// <param name="isLoaded">是否加载成功</param>
        /// <returns>是否加载成功</returns>
        [Code.CSharp.TmphTcpServer]
        private bool loaded(TmphTimeIdentity identity, bool isLoaded)
        {
            return identity.TimeTick == TmphPub.StartTime.Ticks &&
                   TmphPhysicalSet.Default.Loaded(identity.GetIdentity(), isLoaded);
        }

        /// <summary>
        ///     写入日志
        /// </summary>
        /// <param name="dataStream">日志数据</param>
        /// <returns>是否成功写入缓冲区</returns>
        [Code.CSharp.TmphTcpServer(IsServerAsynchronousTask = false, IsClientCallbackTask = false,
            IsClientAsynchronous = true)]
        private int append(TmphTcpBase.TmphSubByteUnmanagedStream dataStream)
        {
            var identity = getIdentity(ref dataStream.TmphBuffer);
            return dataStream.TmphBuffer.array != null
                ? TmphPhysicalSet.Default.Append(identity.GetIdentity(), dataStream.TmphBuffer)
                : 0;
        }

        /// <summary>
        ///     等待缓存写入
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        [Code.CSharp.TmphTcpServer]
        private void waitBuffer(TmphTimeIdentity identity)
        {
            if (identity.TimeTick == TmphPub.StartTime.Ticks) TmphPhysicalSet.Default.WaitBuffer(identity.GetIdentity());
        }

        /// <summary>
        ///     写入缓存
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        /// <returns>是否成功</returns>
        [Code.CSharp.TmphTcpServer(IsServerAsynchronousTask = false)]
        private bool flush(TmphTimeIdentity identity)
        {
            return identity.TimeTick == TmphPub.StartTime.Ticks && TmphPhysicalSet.Default.Flush(identity.GetIdentity());
        }

        /// <summary>
        ///     写入缓存
        /// </summary>
        /// <param name="identity">数据库物理层唯一标识</param>
        /// <param name="isDiskFile">是否写入到磁盘文件</param>
        /// <returns>是否成功</returns>
        [Code.CSharp.TmphTcpServer]
        private bool flushFile(TmphTimeIdentity identity, bool isDiskFile)
        {
            return identity.TimeTick == TmphPub.StartTime.Ticks &&
                   TmphPhysicalSet.Default.FlushFile(identity.GetIdentity(), isDiskFile);
        }

        /// <summary>
        ///     获取数据库物理层唯一标识
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>数据库物理层唯一标识</returns>
        private static unsafe TmphTimeIdentity getIdentity(ref TmphSubArray<byte> data)
        {
            TmphTimeIdentity identity;
            fixed (byte* dataFixed = data.Array)
            {
                identity = *(TmphTimeIdentity*)(dataFixed + data.StartIndex);
                if (identity.TimeTick == TmphPub.StartTime.Ticks)
                {
                    data.UnsafeSet(data.StartIndex + sizeof(TmphTimeIdentity), data.Count - sizeof(TmphTimeIdentity));
                }
                else data.UnsafeSet(null, 0, 0);
            }
            return identity;
        }

        /// <summary>
        ///     数据库物理层唯一标识
        /// </summary>
        [Emit.TmphDataSerialize(IsReferenceMember = false, IsMemberMap = false)]
        public struct TmphTimeIdentity
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
            ///     服务器启动时间
            /// </summary>
            public long TimeTick;

            /// <summary>
            ///     转换成数据库物理层集合唯一标识
            /// </summary>
            /// <returns>数据库物理层集合唯一标识</returns>
            internal TmphPhysicalSet.TmphIdentity GetIdentity()
            {
                return new TmphPhysicalSet.TmphIdentity { Index = Index, Identity = Identity };
            }
        }

        /// <summary>
        ///     数据库物理层初始化信息
        /// </summary>
        [Emit.TmphDataSerialize(IsReferenceMember = false, IsMemberMap = false)]
        public struct TmphPhysicalIdentity
        {
            /// <summary>
            ///     数据库物理层唯一标识
            /// </summary>
            public TmphTimeIdentity Identity;

            /// <summary>
            ///     是否新建文件
            /// </summary>
            public bool IsLoader;

            /// <summary>
            ///     数据库文件是否成功打开
            /// </summary>
            public bool IsOpen
            {
                get { return Identity.TimeTick != 0; }
            }
        }
    }
}