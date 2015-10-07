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

using Laurent.Lee.CLB.Code.CSharp;
using Laurent.Lee.CLB.Config;
using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;

namespace Laurent.Lee.CLB.Net.Tcp.Http
{
    /// <summary>
    ///     会话标识接口
    /// </summary>
    public interface TmphISession
    {
        /// <summary>
        ///     删除Session
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        void Remove(TmphUint128 sessionId);

        /// <summary>
        ///     设置Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="value">值</param>
        /// <returns>Session名称</returns>
        TmphUint128 Set(TmphUint128 sessionId, object value);

        /// <summary>
        ///     获取Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="nullValue">失败返回值</param>
        /// <returns>Session值</returns>
        object Get(TmphUint128 sessionId, object nullValue);
    }

    /// <summary>
    ///     会话标识接口
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public interface TmphISession<TValueType> : TmphISession
    {
        /// <summary>
        ///     设置Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="value">值</param>
        /// <returns>Session名称</returns>
        TmphUint128 Set(TmphUint128 sessionId, TValueType value);

        /// <summary>
        ///     获取Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="nullValue">失败返回值</param>
        /// <returns>Session值</returns>
        TValueType Get(TmphUint128 sessionId, TValueType nullValue);
    }

    /// <summary>
    ///     会话标识服务客户端接口
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public interface TmphISessionClient<TValueType>
    {
        /// <summary>
        ///     设置Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="value">值</param>
        /// <returns>Session名称</returns>
        TmphAsynchronousMethod.TmphReturnValue<TmphUint128> Set(TmphUint128 sessionId, TValueType value);

        /// <summary>
        ///     获取Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="nullValue">失败返回值</param>
        /// <returns>是否存在返回值</returns>
        TmphAsynchronousMethod.TmphReturnValue<bool> tryGet(TmphUint128 sessionId, out TValueType value);

        /// <summary>
        ///     删除Session
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        TmphAsynchronousMethod.TmphReturnValue Remove(TmphUint128 sessionId);
    }

    /// <summary>
    ///     会话标识
    /// </summary>
    public abstract unsafe class TmphSession
    {
        /// <summary>
        ///     随机数高位
        /// </summary>
        private static ulong highRandom = TmphRandom.Default.SecureNextULong();

        /// <summary>
        ///     超时检测
        /// </summary>
        protected Action refresh;

        /// <summary>
        ///     超时刷新时钟周期
        /// </summary>
        protected long refreshTicks;

        /// <summary>
        ///     Session集合访问锁
        /// </summary>
        protected int sessionLock;

        /// <summary>
        ///     超时时钟周期
        /// </summary>
        protected long timeoutTicks;

        /// <summary>
        ///     会话标识
        /// </summary>
        protected TmphSession()
        {
        }

        /// <summary>
        ///     会话标识
        /// </summary>
        /// <param name="timeoutMinutes">超时分钟数</param>
        /// <param name="refreshMinutes">超时刷新分钟数</param>
        protected TmphSession(int timeoutMinutes, int refreshMinutes)
        {
            timeoutTicks = new TimeSpan(0, timeoutMinutes, 0).Ticks;
            refreshTicks = new TimeSpan(0, refreshMinutes, 0).Ticks;
            TmphTimerTask.Default.Add(refresh = refreshTimeout, TmphDate.NowSecond.AddTicks(refreshTicks), null);
        }

        /// <summary>
        ///     超时检测
        /// </summary>
        protected virtual void refreshTimeout()
        {
        }

        /// <summary>
        ///     Session服务端注册验证函数
        /// </summary>
        /// <returns>是否验证成功</returns>
        [TmphTcpServer(IsVerifyMethod = true, IsServerAsynchronousTask = false, InputParameterMaxLength = 1024)]
        protected virtual bool verify(string value)
        {
            if (TmphHttp.Default.SessionVerify == null && !Config.TmphPub.Default.IsDebug)
            {
                TmphLog.Error.Add("Session服务注册验证数据不能为空", false, true);
                return false;
            }
            return TmphHttp.Default.SessionVerify == value;
        }

        /// <summary>
        ///     新建一个会话标识
        /// </summary>
        /// <returns>会话标识</returns>
        protected static TmphUint128 newSessionId()
        {
            var low = TmphRandom.Default.SecureNextULongNotZero();
            highRandom ^= low;
            return new TmphUint128 { Low = low, High = highRandom = (highRandom << 11) | (highRandom >> 53) };
        }

        /// <summary>
        ///     从Cookie
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static TmphUint128 FromCookie(TmphSubArray<byte> data)
        {
            if (data.Count == 32)
            {
                var sessionId = new TmphUint128();
                fixed (byte* dataFixed = data.array) sessionId.ParseHex(dataFixed + data.StartIndex);
                return sessionId;
            }
            return new TmphUint128 { High = 1 };
        }
    }

    /// <summary>
    ///     会话标识
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public class TmphSession<TValueType> : TmphSession, TmphISession<TValueType>
    {
        /// <summary>
        ///     Session集合
        /// </summary>
        private readonly Dictionary<TmphUint128, TmphValue> values = TmphDictionary.CreateUInt128<TmphValue>();

        /// <summary>
        ///     超时检测Session集合
        /// </summary>
        private TmphUint128[] refreshValues = new TmphUint128[256];

        /// <summary>
        ///     会话标识
        /// </summary>
        public TmphSession() : base(TmphHttp.Default.SessionMinutes, TmphHttp.Default.SessionRefreshMinutes)
        {
        }

        /// <summary>
        ///     会话标识
        /// </summary>
        /// <param name="timeoutMinutes">超时分钟数</param>
        /// <param name="refreshMinutes">超时刷新分钟数</param>
        public TmphSession(int timeoutMinutes, int refreshMinutes) : base(timeoutMinutes, refreshMinutes)
        {
        }

        /// <summary>
        ///     设置Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="value">值</param>
        /// <returns>Session名称</returns>
        public TmphUint128 Set(TmphUint128 sessionId, object value)
        {
            return Set(sessionId, (TValueType)value);
        }

        /// <summary>
        ///     设置Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="value">值</param>
        /// <returns>Session名称</returns>
        [TmphTcpServer(IsServerAsynchronousTask = false)]
        public TmphUint128 Set(TmphUint128 sessionId, TValueType value)
        {
            var timeout = TmphDate.NowSecond.AddTicks(timeoutTicks);
            if (sessionId.Low == 0)
            {
                do
                {
                    sessionId = newSessionId();
                    TmphInterlocked.NoCheckCompareSetSleep0(ref sessionLock);
                    if (values.ContainsKey(sessionId)) sessionLock = 0;
                    else
                    {
                        try
                        {
                            values.Add(sessionId, new TmphValue { Timeout = timeout, Value = value });
                            break;
                        }
                        finally
                        {
                            sessionLock = 0;
                        }
                    }
                } while (true);
            }
            else
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref sessionLock);
                try
                {
                    values[sessionId] = new TmphValue { Timeout = timeout, Value = value };
                }
                finally
                {
                    sessionLock = 0;
                }
            }
            return sessionId;
        }

        /// <summary>
        ///     获取Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="nullValue">失败返回值</param>
        /// <returns>Session值</returns>
        public object Get(TmphUint128 sessionId, object nullValue)
        {
            TValueType value;
            return tryGet(sessionId, out value) ? value : nullValue;
        }

        /// <summary>
        ///     获取Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="nullValue">失败返回值</param>
        /// <returns>Session值</returns>
        public TValueType Get(TmphUint128 sessionId, TValueType nullValue)
        {
            TValueType value;
            return tryGet(sessionId, out value) ? value : nullValue;
        }

        /// <summary>
        ///     删除Session
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        [TmphTcpServer(IsServerAsynchronousTask = false)]
        public void Remove(TmphUint128 sessionId)
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref sessionLock);
            try
            {
                values.Remove(sessionId);
            }
            finally
            {
                sessionLock = 0;
            }
        }

        /// <summary>
        ///     超时检测
        /// </summary>
        protected override unsafe void refreshTimeout()
        {
            var time = TmphDate.NowSecond;
            TmphInterlocked.NoCheckCompareSetSleep0(ref sessionLock);
            try
            {
                if (refreshValues.Length < this.values.Count)
                    refreshValues = new TmphUint128[Math.Max(refreshValues.Length << 1, this.values.Count)];
                fixed (TmphUint128* refreshFixed = refreshValues)
                {
                    var write = refreshFixed;
                    foreach (var values in this.values)
                    {
                        if (time >= values.Value.Timeout) *write++ = values.Key;
                    }
                    while (write != refreshFixed) this.values.Remove(*--write);
                }
            }
            finally
            {
                sessionLock = 0;
                TmphTimerTask.Default.Add(refresh, TmphDate.NowSecond.AddTicks(refreshTicks), null);
            }
        }

        /// <summary>
        ///     获取Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="nullValue">失败返回值</param>
        /// <returns>是否存在返回值</returns>
        [TmphTcpServer(IsServerAsynchronousTask = false)]
        protected bool tryGet(TmphUint128 sessionId, out TValueType value)
        {
            var timeout = TmphDate.NowSecond.AddTicks(timeoutTicks);
            TmphValue session;
            TmphInterlocked.NoCheckCompareSetSleep0(ref sessionLock);
            if (values.TryGetValue(sessionId, out session))
            {
                sessionLock = 0;
                value = session.Value;
                return true;
            }
            //try
            //{
            //    this.values[sessionId] = new value { Timeout = timeout, Value = session.Value };
            //}
            //finally { sessionLock = 0; }
            sessionLock = 0;
            value = session.Value;
            return true;
        }

        /// <summary>
        ///     Session值
        /// </summary>
        private struct TmphValue
        {
            /// <summary>
            ///     超时时间
            /// </summary>
            public DateTime Timeout;

            /// <summary>
            ///     Session值集合
            /// </summary>
            public TValueType Value;
        }
    }

    /// <summary>
    ///     会话标识服务客户端
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public sealed class TmphSessionClient<TValueType> : TmphSession, TmphISession<TValueType>
    {
        /// <summary>
        ///     会话标识服务客户端
        /// </summary>
        private readonly TmphISessionClient<TValueType> TmphClient;

        /// <summary>
        ///     会话标识服务客户端
        /// </summary>
        /// <param name="TmphClient">会话标识服务客户端</param>
        public TmphSessionClient(TmphISessionClient<TValueType> TmphClient)
        {
            this.TmphClient = TmphClient;
        }

        /// <summary>
        ///     设置Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="value">值</param>
        /// <returns>Session名称</returns>
        public TmphUint128 Set(TmphUint128 sessionId, object value)
        {
            return Set(sessionId, (TValueType)value);
        }

        /// <summary>
        ///     设置Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="value">值</param>
        /// <returns>Session名称</returns>
        public TmphUint128 Set(TmphUint128 sessionId, TValueType value)
        {
            return TmphClient.Set(sessionId, value);
        }

        /// <summary>
        ///     获取Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="nullValue">失败返回值</param>
        /// <returns>Session值</returns>
        public object Get(TmphUint128 sessionId, object nullValue)
        {
            TValueType value;
            return TmphClient.tryGet(sessionId, out value).Value ? value : nullValue;
        }

        /// <summary>
        ///     获取Session值
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        /// <param name="nullValue">失败返回值</param>
        /// <returns>Session值</returns>
        public TValueType Get(TmphUint128 sessionId, TValueType nullValue)
        {
            TValueType value;
            return TmphClient.tryGet(sessionId, out value).Value ? value : nullValue;
        }

        /// <summary>
        ///     删除Session
        /// </summary>
        /// <param name="sessionId">Session名称</param>
        public void Remove(TmphUint128 sessionId)
        {
            TmphClient.Remove(sessionId);
        }
    }
}