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

using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Laurent.Lee.CLB.Net
{
    /// <summary>
    ///     客户端队列
    /// </summary>
    public abstract class TmphClientQueue
    {
        /// <summary>
        ///     套接字类型
        /// </summary>
        public enum TmphSocketType
        {
            /// <summary>
            ///     客户端错误
            /// </summary>
            Error,

            /// <summary>
            ///     队列
            /// </summary>
            Queue,

            /// <summary>
            ///     IPv4
            /// </summary>
            Ipv4,

            /// <summary>
            ///     IPv4
            /// </summary>
            Ipv6
        }

        /// <summary>
        ///     IPv4客户端连接 访问锁
        /// </summary>
        protected int ipv4Lock;

        /// <summary>
        ///     IPv4客户端连接 访问锁
        /// </summary>
        protected int ipv6Lock;

        /// <summary>
        ///     每IP最大活动连接数量,等于0表示不限
        /// </summary>
        protected int maxActiveCount;

        /// <summary>
        ///     每IP最大连接数量,等于0表示不限
        /// </summary>
        protected int maxCount;

        /// <summary>
        ///     客户端队列
        /// </summary>
        /// <param name="maxActiveCount">每IP最大活动连接数量,等于0表示不限</param>
        /// <param name="maxCount">每IP最大连接数量,等于0表示不限</param>
        protected TmphClientQueue(int maxActiveCount, int maxCount)
        {
            this.maxActiveCount = maxActiveCount;
            this.maxCount = maxCount;
        }
    }

    /// <summary>
    ///     客户端队列
    /// </summary>
    /// <typeparam name="TClientType">客户端实例类型</typeparam>
    public class TmphClientQueue<TClientType> : TmphClientQueue, IDisposable
    {
        /// <summary>
        ///     释放客户端
        /// </summary>
        private readonly Action<TClientType> disposeClient;

        /// <summary>
        ///     IPv4客户端连接数量信息
        /// </summary>
        private readonly Dictionary<int, TmphCount> ipv4Count;

        /// <summary>
        ///     IPv6客户端连接数量信息
        /// </summary>
        private readonly Dictionary<TmphIpv6Hash, TmphCount> ipv6Count;

        /// <summary>
        ///     客户端队列
        /// </summary>
        /// <param name="maxActiveCount">每IP最大活动连接数量,等于0表示不限</param>
        /// <param name="maxCount">每IP最大连接数量,等于0表示不限</param>
        /// <param name="disposeClient">释放客户端</param>
        protected TmphClientQueue(int maxActiveCount, int maxCount, Action<TClientType> disposeClient)
            : base(maxActiveCount, maxCount)
        {
            this.disposeClient = disposeClient;
            ipv4Count = TmphDictionary.CreateInt<TmphCount>();
            ipv6Count = TmphDictionary.Create<TmphIpv6Hash, TmphCount>();
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            if (disposeClient != null)
            {
                TmphCount[] ipv4Sockets = null, ipv6Sockets = null;
                TmphInterlocked.NoCheckCompareSetSleep0(ref ipv4Lock);
                try
                {
                    ipv4Sockets = ipv4Count.Values.GetArray();
                    ipv4Count.Clear();
                }
                finally
                {
                    ipv4Lock = 0;
                }
                TmphInterlocked.NoCheckCompareSetSleep0(ref ipv6Lock);
                try
                {
                    ipv6Sockets = ipv6Count.Values.GetArray();
                    ipv6Count.Clear();
                }
                finally
                {
                    ipv6Lock = 0;
                }
                dispose(ipv4Sockets);
                dispose(ipv6Sockets);
            }
        }

        /// <summary>
        ///     释放客户端
        /// </summary>
        /// <param name="counts">客户端集合</param>
        private void dispose(TmphCount[] counts)
        {
            foreach (var count in counts)
            {
                var index = count.Sockets.Count;
                if (index != 0)
                {
                    foreach (var TmphClient in count.Sockets.array)
                    {
                        try
                        {
                            disposeClient(TmphClient);
                        }
                        catch
                        {
                        }
                        if (--index == 0) break;
                    }
                    Array.Clear(count.Sockets.array, 0, count.Sockets.Count);
                    count.Sockets.Empty();
                }
                TmphTypePool<TmphCount>.Push(count);
            }
        }

        /// <summary>
        ///     添加客户端
        /// </summary>
        /// <param name="TmphClient">客户端</param>
        /// <param name="socket">套接字</param>
        /// <param name="ipv4">ipv4地址</param>
        /// <param name="ipv6">ipv6地址</param>
        /// <returns>套接字操作类型</returns>
        public virtual TmphSocketType NewClient(TClientType TmphClient, Socket socket, ref int ipv4, ref TmphIpv6Hash ipv6)
        {
            var type = TmphSocketType.Error;
            try
            {
                var ipEndPoint = (IPEndPoint)socket.RemoteEndPoint;
                TmphCount count;
                if (ipEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    if (!(ipv6 = ipEndPoint.Address).IsNull)
                    {
                        TmphInterlocked.NoCheckCompareSetSleep0(ref ipv6Lock);
                        if (ipv6Count.TryGetValue(ipv6, out count))
                        {
                            if (count.Count < maxActiveCount)
                            {
                                ++count.Count;
                                ipv6Lock = 0;
                                type = TmphSocketType.Ipv6;
                            }
                            else if (count.Count < maxCount)
                            {
                                try
                                {
                                    count.Sockets.Add(TmphClient);
                                    ++count.Count;
                                }
                                finally
                                {
                                    ipv6Lock = 0;
                                }
                                type = TmphSocketType.Queue;
                            }
                            else ipv6Lock = 0;
                        }
                        else
                        {
                            try
                            {
                                ipv6Count.Add(ipv6, TmphCount.Get());
                            }
                            finally
                            {
                                ipv6Lock = 0;
                            }
                            type = TmphSocketType.Ipv6;
                        }
                    }
                }
                else
                {
#pragma warning disable 618
                    ipv4 = (int)ipEndPoint.Address.Address;
#pragma warning restore 618
                    var ipKey = ipv4 ^ TmphRandom.Hash;
                    TmphInterlocked.NoCheckCompareSetSleep0(ref ipv4Lock);
                    if (ipv4Count.TryGetValue(ipKey, out count))
                    {
                        if (count.Count < maxActiveCount)
                        {
                            ++count.Count;
                            ipv4Lock = 0;
                            type = TmphSocketType.Ipv4;
                        }
                        else if (count.Count < maxCount)
                        {
                            try
                            {
                                count.Sockets.Add(TmphClient);
                                ++count.Count;
                            }
                            finally
                            {
                                ipv4Lock = 0;
                            }
                            type = TmphSocketType.Queue;
                        }
                        else ipv4Lock = 0;
                    }
                    else
                    {
                        try
                        {
                            ipv4Count.Add(ipKey, TmphCount.Get());
                        }
                        finally
                        {
                            ipv4Lock = 0;
                        }
                        type = TmphSocketType.Ipv4;
                    }
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
            }
            if (type == TmphSocketType.Error) socket.Close();
            return type;
        }

        /// <summary>
        ///     添加客户端
        /// </summary>
        /// <param name="TmphClient">客户端</param>
        /// <param name="socket">套接字</param>
        /// <returns>客户端信息</returns>
        public TmphClientInfo NewClient(TClientType TmphClient, Socket socket)
        {
            var clientInfo = new TmphClientInfo { Client = TmphClient };
            clientInfo.Type = NewClient(TmphClient, socket, ref clientInfo.Ipv4, ref clientInfo.Ipv6);
            return clientInfo;
        }

        /// <summary>
        ///     请求处理结束
        /// </summary>
        /// <param name="ipv4">ipv4地址</param>
        /// <returns>下一个客户端</returns>
        public virtual TClientType End(int ipv4)
        {
            var socket = default(TClientType);
            TmphCount count;
            var ipKey = ipv4 ^ TmphRandom.Hash;
            TmphInterlocked.NoCheckCompareSetSleep0(ref ipv4Lock);
            if (ipv4Count.TryGetValue(ipKey, out count))
            {
                if (count.Count <= maxActiveCount)
                {
                    if (--count.Count == 0)
                    {
                        try
                        {
                            ipv4Count.Remove(ipKey);
                        }
                        finally
                        {
                            ipv4Lock = 0;
                        }
                        TmphTypePool<TmphCount>.Push(ref count);
                    }
                    else ipv4Lock = 0;
                }
                else
                {
                    socket = count.Sockets.UnsafePopReset();
                    --count.Count;
                    ipv4Lock = 0;
                }
            }
            else ipv4Lock = 0;
            return socket;
        }

        /// <summary>
        ///     请求处理结束
        /// </summary>
        /// <param name="ipv6">ipv6地址</param>
        /// <returns>下一个客户端</returns>
        public virtual TClientType End(TmphIpv6Hash ipv6)
        {
            var socket = default(TClientType);
            TmphCount count;
            TmphInterlocked.NoCheckCompareSetSleep0(ref ipv6Lock);
            if (ipv6Count.TryGetValue(ipv6, out count))
            {
                if (count.Count <= maxActiveCount)
                {
                    if (--count.Count == 0)
                    {
                        try
                        {
                            ipv6Count.Remove(ipv6);
                        }
                        finally
                        {
                            ipv6Lock = 0;
                        }
                        TmphTypePool<TmphCount>.Push(ref count);
                    }
                    else ipv6Lock = 0;
                }
                else
                {
                    socket = count.Sockets.UnsafePopReset();
                    --count.Count;
                    ipv6Lock = 0;
                }
            }
            else ipv6Lock = 0;
            return socket;
        }

        /// <summary>
        ///     客户端套接字队列
        /// </summary>
        /// <param name="maxActiveCount">每IP最大活动连接数量,小于等于0表示不限</param>
        /// <param name="maxCount">每IP最大连接数量,小于等于0表示不限</param>
        /// <param name="disposeClient">释放客户端</param>
        public static TmphClientQueue<TClientType> Create(int maxActiveCount, int maxCount,
            Action<TClientType> disposeClient)
        {
            if (maxActiveCount <= 0) return TmphNullQueue.Default;
            if (disposeClient == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            return new TmphClientQueue<TClientType>(maxActiveCount, maxCount < maxActiveCount ? maxActiveCount : maxCount,
                disposeClient);
        }

        /// <summary>
        ///     客户端空队列
        /// </summary>
        private sealed class TmphNullQueue : TmphClientQueue<TClientType>
        {
            /// <summary>
            ///     客户端空队列
            /// </summary>
            public static readonly TmphNullQueue Default = new TmphNullQueue();

            /// <summary>
            ///     客户端空队列
            /// </summary>
            private TmphNullQueue() : base(0, 0, null)
            {
            }

            /// <summary>
            ///     添加客户端
            /// </summary>
            /// <param name="TmphClient">客户端</param>
            /// <param name="socket">套接字</param>
            /// <param name="ipv4">ipv4地址</param>
            /// <param name="ipv6">ipv6地址</param>
            /// <returns>套接字操作类型</returns>
            public override TmphSocketType NewClient(TClientType TmphClient, Socket socket, ref int ipv4, ref TmphIpv6Hash ipv6)
            {
                ipv4 = 0;
                return TmphSocketType.Ipv4;
            }

            /// <summary>
            ///     请求处理结束
            /// </summary>
            /// <param name="ipv4">ipv4地址</param>
            /// <returns>下一个客户端</returns>
            public override TClientType End(int ipv4)
            {
                return default(TClientType);
            }

            /// <summary>
            ///     请求处理结束
            /// </summary>
            /// <param name="ipv6">ipv6地址</param>
            /// <returns>下一个客户端</returns>
            public override TClientType End(TmphIpv6Hash ipv6)
            {
                return default(TClientType);
            }
        }

        /// <summary>
        ///     客户端连接数量
        /// </summary>
        private sealed class TmphCount
        {
            /// <summary>
            ///     户端连接数量
            /// </summary>
            public int Count;

            /// <summary>
            ///     等待处理的客户端
            /// </summary>
            public TmphSubArray<TClientType> Sockets;

            /// <summary>
            ///     获取HTTP客户端连接数量
            /// </summary>
            /// <returns>HTTP客户端连接数量</returns>
            public static TmphCount Get()
            {
                var count = TmphTypePool<TmphCount>.Pop() ?? new TmphCount();
                count.Count = 1;
                return count;
            }
        }

        /// <summary>
        ///     客户端信息
        /// </summary>
        public struct TmphClientInfo
        {
            /// <summary>
            ///     客户端
            /// </summary>
            public TClientType Client;

            /// <summary>
            ///     IPv4地址
            /// </summary>
            public int Ipv4;

            /// <summary>
            ///     IPv6地址
            /// </summary>
            public TmphIpv6Hash Ipv6;

            /// <summary>
            ///     套接字类型
            /// </summary>
            public TmphSocketType Type;
        }
    }
}