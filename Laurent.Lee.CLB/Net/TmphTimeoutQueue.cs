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
using System.Net.Sockets;

namespace Laurent.Lee.CLB.Net
{
    /// <summary>
    ///     超时队列
    /// </summary>
    public sealed class TmphTimeoutQueue
    {
        /// <summary>
        ///     超时检测
        /// </summary>
        private readonly Action checkHandle;

        /// <summary>
        ///     当前处理超时集合
        /// </summary>
        private readonly TmphCallback[] timeoutSockets;

        /// <summary>
        ///     回调信息集合结束位置
        /// </summary>
        private int endIndex;

        /// <summary>
        ///     是否正在处理超时集合
        /// </summary>
        private int isTask;

        /// <summary>
        ///     回调信息集合访问锁
        /// </summary>
        private int socketLock;

        /// <summary>
        ///     回调信息集合
        /// </summary>
        private TmphCallback[] sockets;

        /// <summary>
        ///     回调信息集合起始位置
        /// </summary>
        private int startIndex;

        /// <summary>
        ///     超时时钟周期
        /// </summary>
        private long timeoutTicks;

        /// <summary>
        ///     超时队列
        /// </summary>
        /// <param name="seconds">超时秒数</param>
        public TmphTimeoutQueue(int seconds)
        {
            timeoutTicks = TmphDate.SecondTicks * seconds;
            sockets = new TmphCallback[256];
            timeoutSockets = new TmphCallback[256];
            checkHandle = check;
        }

        /// <summary>
        ///     超时时钟周期
        /// </summary>
        public long CallbackTimeoutTicks
        {
            get { return timeoutTicks + TmphDate.SecondTicks; }
        }

        /// <summary>
        ///     超时秒数
        /// </summary>
        public int TimeoutSeconds
        {
            set
            {
                var newTicks = TmphDate.SecondTicks * value;
                TmphInterlocked.NoCheckCompareSetSleep0(ref socketLock);
                var ticks = newTicks - timeoutTicks;
                timeoutTicks = newTicks;
                for (int index = startIndex, end = startIndex > endIndex ? sockets.Length : endIndex;
                    index != end;
                    sockets[index++].AddTimeout(newTicks))
                    ;
                if (startIndex > endIndex)
                {
                    for (var index = 0; index != endIndex; sockets[index++].AddTimeout(newTicks)) ;
                }
                socketLock = 0;
            }
        }

        /// <summary>
        ///     添加超时回调信息
        /// </summary>
        /// <param name="socket">套接字</param>
        /// <param name="isTimeout">是否超时判断</param>
        /// <param name="idnetity">超时判断标识</param>
        public void Add(Socket socket, Func<int, bool> isTimeout, int idnetity = 0)
        {
            int isError = 0, isTask;
            var timeout = TmphDate.NowSecond.AddTicks(timeoutTicks);
            TmphInterlocked.NoCheckCompareSetSleep0(ref socketLock);
            isTask = this.isTask;
            sockets[endIndex].Set(timeout, socket, isTimeout, idnetity);
            if (++endIndex == sockets.Length) endIndex = 0;
            this.isTask = 1;
            if (endIndex == startIndex)
            {
                try
                {
                    var newSockets = new TmphCallback[sockets.Length << 1];
                    if (startIndex == 0) Array.Copy(sockets, 0, newSockets, 0, sockets.Length);
                    else
                    {
                        Array.Copy(sockets, startIndex, newSockets, 0, idnetity = sockets.Length - startIndex);
                        Array.Copy(sockets, 0, newSockets, idnetity, startIndex);
                    }
                    endIndex = sockets.Length;
                    startIndex = 0;
                    sockets = newSockets;
                }
                catch (Exception error)
                {
                    if (endIndex == 0) endIndex = sockets.Length - 1;
                    else --endIndex;
                    isError = 1;
                    this.isTask = isTask;
                    sockets[endIndex].Clear();
                    TmphLog.Error.Add(error, null, false);
                }
                finally
                {
                    socketLock = 0;
                }
            }
            else socketLock = 0;
            if (isError == 0)
            {
                if (isTask == 0)
                {
                    var maxTimeout = TmphDate.NowSecond.AddTicks(TmphDate.MinutesTicks);
                    TmphTimerTask.Default.Add(checkHandle, maxTimeout < timeout ? maxTimeout : timeout);
                }
            }
            else socket.Shutdown(SocketShutdown.Both);
        }

        /// <summary>
        ///     超时检测
        /// </summary>
        private void check()
        {
            var count = 0;
            do
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref socketLock);
                if (startIndex > endIndex)
                {
                    while (sockets[startIndex].Check(ref timeoutSockets[count]))
                    {
                        ++count;
                        if (++startIndex == sockets.Length)
                        {
                            startIndex = 0;
                            break;
                        }
                        if (count == timeoutSockets.Length) break;
                    }
                }
                else
                {
                    if (startIndex == endIndex)
                    {
                        isTask = 0;
                        socketLock = 0;
                        return;
                    }
                    while (sockets[startIndex].Check(ref timeoutSockets[count]))
                    {
                        ++count;
                        if (++startIndex == endIndex)
                        {
                            startIndex = endIndex = 0;
                            break;
                        }
                        if (count == timeoutSockets.Length) break;
                    }
                }
                socketLock = 0;
                if (count == 0)
                {
                    DateTime maxTimeout = TmphDate.NowSecond.AddTicks(TmphDate.MinutesTicks),
                        timeout = sockets[startIndex].Timeout;
                    TmphTimerTask.Default.Add(checkHandle, maxTimeout < timeout ? maxTimeout : timeout);
                    return;
                }
                while (count != 0) timeoutSockets[--count].Check();
            } while (true);
        }

        /// <summary>
        ///     回调信息
        /// </summary>
        private struct TmphCallback
        {
            /// <summary>
            ///     超时判断标识
            /// </summary>
            private int identity;

            /// <summary>
            ///     是否超时判断
            /// </summary>
            private Func<int, bool> isTimeout;

            /// <summary>
            ///     套接字
            /// </summary>
            private Socket socket;

            /// <summary>
            ///     超时时间
            /// </summary>
            public DateTime Timeout;

            /// <summary>
            ///     设置回调信息
            /// </summary>
            /// <param name="timeout">超时时间</param>
            /// <param name="socket">套接字</param>
            /// <param name="isTimeout">是否超时判断</param>
            /// <param name="idnetity">超时判断标识</param>
            public void Set(DateTime timeout, Socket socket, Func<int, bool> isTimeout, int idnetity)
            {
                Timeout = timeout;
                this.socket = socket;
                this.isTimeout = isTimeout;
                identity = idnetity;
            }

            /// <summary>
            ///     设置回调信息
            /// </summary>
            /// <param name="socket">套接字</param>
            /// <param name="isTimeout">是否超时判断</param>
            /// <param name="idnetity">超时判断标识</param>
            private void set(Socket socket, Func<int, bool> isTimeout, int idnetity)
            {
                this.socket = socket;
                this.isTimeout = isTimeout;
                identity = idnetity;
            }

            /// <summary>
            ///     超时检测
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public bool Check(ref TmphCallback value)
            {
                if (TmphDate.NowSecond >= Timeout)
                {
                    value.set(socket, isTimeout, identity);
                    Clear();
                    return true;
                }
                return false;
            }

            /// <summary>
            ///     超时检测
            /// </summary>
            public void Check()
            {
                if (isTimeout(identity)) socket.Shutdown(SocketShutdown.Both);
                Clear();
            }

            /// <summary>
            ///     清除回调信息
            /// </summary>
            public void Clear()
            {
                socket = null;
                isTimeout = null;
            }

            /// <summary>
            ///     修改超时时间
            /// </summary>
            /// <param name="ticks"></param>
            public void AddTimeout(long ticks)
            {
                Timeout = Timeout.AddTicks(ticks);
            }
        }
    }
}