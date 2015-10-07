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
using System;
using System.Net.Sockets;
using System.Text;

namespace Laurent.Lee.CLB.Net.Tcp
{
    /// <summary>
    ///     TCP调用套接字
    /// </summary>
    public abstract class TmphCommandSocket : TmphSocket
    {
        /// <summary>
        ///     大数据缓存
        /// </summary>
        internal static readonly TmphMemoryPool BigBuffers = TmphMemoryPool.GetPool(TmphTcpCommand.Default.BigBufferSize);

        /// <summary>
        ///     异步(包括流式)缓冲区
        /// </summary>
        protected internal static readonly TmphMemoryPool asyncBuffers =
            TmphMemoryPool.GetPool(TmphTcpCommand.Default.AsyncBufferSize);

        /// <summary>
        ///     当前处理会话标识
        /// </summary>
        protected internal TmphCommandServer.TmphStreamIdentity identity;

        /// <summary>
        ///     是否通过验证方法
        /// </summary>
        public bool IsVerifyMethod;

        /// <summary>
        ///     接收数据缓冲区
        /// </summary>
        protected internal byte[] receiveData;

        /// <summary>
        ///     发送命令缓冲区
        /// </summary>
        protected byte[] sendData;

        /// <summary>
        ///     TCP客户端套接字
        /// </summary>
        /// <param name="socket">TCP套接字</param>
        /// <param name="sendData">发送数据缓冲区</param>
        /// <param name="receiveData">接收数据缓冲区</param>
        /// <param name="isErrorDispose">操作错误是否自动调用析构函数</param>
        protected TmphCommandSocket(Socket socket, byte[] sendData, byte[] receiveData, bool isErrorDispose)
            : base(socket, isErrorDispose)
        {
            this.sendData = sendData;
            currentReceiveData = this.receiveData = receiveData;
        }

        /// <summary>
        ///     当前处理会话标识
        /// </summary>
        public TmphCommandServer.TmphStreamIdentity Identity
        {
            get { return identity; }
        }

        /// <summary>
        ///     客户端标识
        /// </summary>
        public TmphTcpBase.TmphClient ClientUserInfo { get; protected internal set; }

        /// <summary>
        ///     默认HTTP内容编码
        /// </summary>
        internal virtual Encoding HttpEncoding
        {
            get { return null; }
        }

        ///// <summary>
        ///// TCP客户端套接字
        ///// </summary>
        ///// <param name="sendData">接收数据缓冲区</param>
        ///// <param name="receiveData">发送数据缓冲区</param>
        //protected commandSocket(byte[] sendData, byte[] receiveData)
        //    : base(false)
        //{
        //    this.sendData = sendData;
        //    currentReceiveData = this.receiveData = receiveData;
        //}
        /// <summary>
        ///     关闭套接字连接
        /// </summary>
        protected override void dispose()
        {
            TmphMemoryPool.StreamBuffers.Push(ref sendData);
            TmphMemoryPool.StreamBuffers.Push(ref receiveData);
        }

        /// <summary>
        ///     TCP套接字添加到池
        /// </summary>
        internal abstract void PushPool();
    }

    /// <summary>
    ///     TCP调用套接字
    /// </summary>
    /// <typeparam name="TSocketType">TCP调用类型</typeparam>
    public abstract class TmphCommandSocket<TCommandSocketType> : TmphCommandSocket
        where TCommandSocketType : class, IDisposable
    {
        /// <summary>
        ///     TCP调用代理
        /// </summary>
        protected internal TCommandSocketType commandSocketProxy;

        /// <summary>
        ///     TCP客户端套接字
        /// </summary>
        /// <param name="socket">TCP套接字</param>
        /// <param name="sendData">发送数据缓冲区</param>
        /// <param name="receiveData">接收数据缓冲区</param>
        /// <param name="commandSocketProxy">TCP调用类型</param>
        /// <param name="isErrorDispose">操作错误是否自动调用析构函数</param>
        protected TmphCommandSocket(Socket socket, byte[] sendData, byte[] receiveData,
            TCommandSocketType commandSocketProxy, bool isErrorDispose)
            : base(socket, sendData, receiveData, isErrorDispose)
        {
            this.commandSocketProxy = commandSocketProxy;
        }
    }
}