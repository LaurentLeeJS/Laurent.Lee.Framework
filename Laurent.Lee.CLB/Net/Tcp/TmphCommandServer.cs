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
using Laurent.Lee.CLB.Emit;
using Laurent.Lee.CLB.IO.Compression;
using Laurent.Lee.CLB.Net.Tcp.Http;
using Laurent.Lee.CLB.Threading;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TmphDataSerialize = Laurent.Lee.CLB.Emit.TmphDataSerialize;
using TmphHttp = Laurent.Lee.CLB.Web.TmphHttp;

namespace Laurent.Lee.CLB.Net.Tcp
{
    /// <summary>
    ///     TCP调用服务端
    /// </summary>
    public abstract class TmphCommandServer : TmphServer
    {
        /// <summary>
        ///     数据命令验证会话标识
        /// </summary>
        internal const int VerifyIdentity = 0x060C5113;

        /// <summary>
        ///     序号命令验证会话标识
        /// </summary>
        internal const int IdentityVerifyIdentity = 0x10035113;

        /// <summary>
        ///     空验证命令序号
        /// </summary>
        private const int nullVerifyCommandIdentity = -VerifyIdentity;

        /// <summary>
        ///     用户命令起始位置
        /// </summary>
        public const int CommandStartIndex = 128;

        /// <summary>
        ///     用户命令数据起始位置
        /// </summary>
        public const int CommandDataIndex = 0x20202000;

        /// <summary>
        ///     关闭命令
        /// </summary>
        public const int CloseIdentityCommand = CommandStartIndex - 1;

        /// <summary>
        ///     连接检测命令
        /// </summary>
        public const int CheckIdentityCommand = CloseIdentityCommand - 1;

        /// <summary>
        ///     负载均衡连接检测命令
        /// </summary>
        public const int LoadBalancingCheckIdentityCommand = CheckIdentityCommand - 1;

        /// <summary>
        ///     流合并命令
        /// </summary>
        public const int StreamMergeIdentityCommand = LoadBalancingCheckIdentityCommand - 1;

        /// <summary>
        ///     TCP流回应命令
        /// </summary>
        public const int TcpStreamCommand = StreamMergeIdentityCommand - 1;

        /// <summary>
        ///     忽略分组命令
        /// </summary>
        public const int IgnoreGroupCommand = TcpStreamCommand - 1;

        /// <summary>
        ///     流式套接字错误返回长度值
        /// </summary>
        internal const int ErrorStreamReturnLength = int.MinValue;

        /// <summary>
        ///     关闭链接命令
        /// </summary>
        private static readonly byte[] closeCommandData = BitConverter.GetBytes(CloseIdentityCommand + CommandDataIndex);

        /// <summary>
        ///     流合并命令
        /// </summary>
        private static readonly byte[] streamMergeCommandData =
            BitConverter.GetBytes(StreamMergeIdentityCommand + CommandDataIndex);

        /// <summary>
        ///     连接检测命令
        /// </summary>
        private static readonly byte[] checkCommandData = BitConverter.GetBytes(CheckIdentityCommand + CommandDataIndex);

        /// <summary>
        ///     负载均衡连接检测命令
        /// </summary>
        private static readonly byte[] loadBalancingCheckCommandData =
            BitConverter.GetBytes(LoadBalancingCheckIdentityCommand + CommandDataIndex);

        /// <summary>
        ///     TCP流回馈命令
        /// </summary>
        private static readonly byte[] tcpStreamCommandData = BitConverter.GetBytes(TcpStreamCommand + CommandDataIndex);

        /// <summary>
        ///     忽略分组命令
        /// </summary>
        private static readonly byte[] ignoreGroupCommandData =
            BitConverter.GetBytes(IgnoreGroupCommand + CommandDataIndex);

        /// <summary>
        ///     连接检测套接字
        /// </summary>
        private static readonly TmphCommand mergeCheckCommand = new TmphCommand(check, 0);

        /// <summary>
        ///     资源释放异常
        /// </summary>
        private static readonly Exception objectDisposedException = new ObjectDisposedException("tcpStream");

        /// <summary>
        ///     错误支持异常
        /// </summary>
        private static readonly Exception notSupportedException = new NotSupportedException();

        /// <summary>
        ///     错误操作异常
        /// </summary>
        private static readonly Exception invalidOperationException = new InvalidOperationException();

        /// <summary>
        ///     IO异常
        /// </summary>
        private static readonly Exception ioException = new IOException();

        /// <summary>
        ///     空参数异常
        /// </summary>
        private static readonly Exception argumentNullException = new ArgumentNullException();

        /// <summary>
        ///     参数超出范围异常
        /// </summary>
        private static readonly Exception argumentOutOfRangeException = new ArgumentOutOfRangeException();

        /// <summary>
        ///     参数异常
        /// </summary>
        private static readonly Exception argumentException = new ArgumentException();

        /// <summary>
        ///     负载均衡联通测试时钟周期
        /// </summary>
        private readonly long loadBalancingCheckTicks;

        /// <summary>
        ///     每秒最低接收字节数
        /// </summary>
        private readonly int minReceivePerSecond;

        /// <summary>
        ///     接收命令超时时钟周期
        /// </summary>
        private readonly long receiveCommandTicks;

        /// <summary>
        ///     接收命令超时
        /// </summary>
        private readonly int receiveCommandTimeout;

        /// <summary>
        ///     接收数据超时
        /// </summary>
        private readonly int receiveTimeout;

        /// <summary>
        ///     TCP客户端验证接口
        /// </summary>
        private readonly TmphTcpBase.ITcpVerify verify;

        /// <summary>
        ///     客户端套接字队列
        /// </summary>
        private TmphClientQueue<Socket> clientQueue;

        /// <summary>
        ///     HTTP命令处理委托集合
        /// </summary>
        protected TmphStateSearcher.TmphAscii<TmphHttpCommand> httpCommands;

        /// <summary>
        ///     HTTP服务器
        /// </summary>
        internal TmphHttpServers HttpServers;

        /// <summary>
        ///     序号识别命令处理委托集合
        /// </summary>
        protected TmphCommand[] identityOnCommands;

        /// <summary>
        ///     是否存在客户端标识
        /// </summary>
        protected bool isClientUserInfo;

        /// <summary>
        ///     负载均衡服务TCP服务调用配置
        /// </summary>
        private TmphTcpServer loadBalancingAttribute;

        /// <summary>
        ///     负载均衡联通测试
        /// </summary>
        private Action loadBalancingCheckHandle;

        /// <summary>
        ///     负载均衡联通测试标识
        /// </summary>
        private int loadBalancingCheckIdentity;

        /// <summary>
        ///     最后一次负载均衡联通测试时间
        /// </summary>
        private DateTime loadBalancingCheckTime;

        /// <summary>
        ///     TCP调用服务添加负载均衡服务
        /// </summary>
        private Action loadBalancingHandle;

        /// <summary>
        ///     负载均衡服务TCP验证方
        /// </summary>
        private Action<Exception> loadBalancingOnException;

        /// <summary>
        ///     当前负载均衡套接字
        /// </summary>
        private TmphSocket loadBalancingSocket;

        /// <summary>
        ///     负载均衡服务TCP验证方
        /// </summary>
        private TmphTcpBase.ITcpClientVerifyMethod<TmphCommandLoadBalancingServer.TmphCommandClient> loadBalancingVerifyMethod;

        /// <summary>
        ///     最大命令长度
        /// </summary>
        protected int maxCommandLength;

        /// <summary>
        ///     当前负载均衡套接字
        /// </summary>
        private TmphSocket nextLoadBalancingSocket;

        /// <summary>
        ///     命令处理委托集合
        /// </summary>
        protected TmphStateSearcher.TmphAscii<TmphCommand> onCommands;

        /// <summary>
        ///     发送数据超时
        /// </summary>
        private int sendTimeout;

        /// <summary>
        ///     套接字池最后一个节点
        /// </summary>
        private TmphSocket socketPoolEnd;

        /// <summary>
        ///     套接字池第一个节点
        /// </summary>
        private TmphSocket socketPoolHead;

        /// <summary>
        ///     套接字池访问锁
        /// </summary>
        private int socketPoolLock;

        /// <summary>
        ///     验证命令
        /// </summary>
        protected byte[] verifyCommand = TmphNullValue<byte>.Array;

        /// <summary>
        ///     验证命令序号
        /// </summary>
        protected int verifyCommandIdentity = nullVerifyCommandIdentity;

        /// <summary>
        ///     TCP调用服务端
        /// </summary>
        /// <param name="attribute">配置信息</param>
        public TmphCommandServer(TmphTcpServer attribute) : this(attribute, null)
        {
        }

        /// <summary>
        ///     TCP调用服务端
        /// </summary>
        /// <param name="attribute">配置信息</param>
        /// <param name="verify">TCP客户端验证接口</param>
        public unsafe TmphCommandServer(TmphTcpServer attribute, TmphTcpBase.ITcpVerify verify)
            : base(attribute)
        {
            if (attribute.SendBufferSize <= (sizeof(int) * 2 + sizeof(TmphStreamIdentity)))
                attribute.SendBufferSize = Math.Max(sizeof(int) * 2 + sizeof(TmphStreamIdentity),
                    TmphAppSetting.StreamBufferSize);
            if (attribute.ReceiveBufferSize <= maxCommandLength + (sizeof(int) * 3 + sizeof(TmphStreamIdentity)))
                attribute.ReceiveBufferSize = Math.Max(maxCommandLength + (sizeof(int) * 3 + sizeof(TmphStreamIdentity)),
                    TmphAppSetting.StreamBufferSize);
            if (attribute.ReceiveTimeout > 0)
            {
                receiveTimeout = attribute.ReceiveTimeout * 1000;
                if (receiveTimeout <= 0) receiveTimeout = int.MaxValue;
                sendTimeout = receiveTimeout;
            }
            else sendTimeout = TmphTcpCommand.Default.DefaultTimeout * 1000;
            if (attribute.MinReceivePerSecond > 0)
            {
                minReceivePerSecond = attribute.MinReceivePerSecond << 10;
                if (minReceivePerSecond <= 0) minReceivePerSecond = int.MaxValue;
            }
            receiveCommandTimeout = (attribute.RecieveCommandMinutes > 0
                ? attribute.RecieveCommandMinutes * 60
                : TmphTcpCommand.Default.DefaultTimeout) * 1000;
            if (receiveCommandTimeout <= 0) receiveCommandTimeout = int.MaxValue;
            receiveCommandTicks = TmphDate.MillisecondTicks * receiveCommandTimeout;

            loadBalancingCheckTicks = new TimeSpan(0, 0, Math.Max(attribute.LoadBalancingCheckSeconds - 2, 1)).Ticks;

            this.verify = verify;
            clientQueue = TmphClientQueue<Socket>.Create(attribute.MaxActiveClientCount, attribute.MaxClientCount,
                closeSocket);
        }

        /// <summary>
        ///     启动服务并添加到负载均衡服务
        /// </summary>
        /// <param name="attribute">负载均衡服务TCP服务调用配置</param>
        /// <param name="verifyMethod">TCP验证方法</param>
        /// <param name="onException">异常处理</param>
        /// <returns>是否成功</returns>
        public bool StartLoadBalancing(TmphTcpServer attribute
            , TmphTcpBase.ITcpClientVerifyMethod<TmphCommandLoadBalancingServer.TmphCommandClient> verifyMethod = null
            , Action<Exception> onException = null)
        {
            if (Start())
            {
                if (attribute != null)
                {
                    attribute.IsLoadBalancing = false;
                    loadBalancingAttribute = attribute;
                    loadBalancingVerifyMethod = verifyMethod ?? new TmphVerifyMethod();
                    loadBalancingOnException = onException;
                    loadBalancing();
                }
                return true;
            }
            return false;
        }

        /// <summary>
        ///     TCP调用服务添加负载均衡服务
        /// </summary>
        private void loadBalancing()
        {
            if (isStart != 0)
            {
                try
                {
                    if (new TmphCommandLoadBalancingServer.TmphCommandClient(loadBalancingAttribute,
                        loadBalancingVerifyMethod)
                        .NewServer(new TmphHost { Host = attribute.Host, Port = attribute.Port }))
                    {
                        return;
                    }
                }
                catch (Exception error)
                {
                    if (loadBalancingOnException == null) TmphLog.Error.Add(error, null, false);
                    else loadBalancingOnException(error);
                }
                if (loadBalancingHandle == null) loadBalancingHandle = loadBalancing;
                TmphTimerTask.Default.Add(loadBalancingHandle, TmphDate.NowSecond.AddSeconds(1));
            }
        }

        /// <summary>
        ///     初始化序号识别命令处理委托集合
        /// </summary>
        /// <param name="count">命令数量</param>
        protected void setCommands(int count)
        {
            identityOnCommands = new TmphCommand[count + CommandStartIndex];
            identityOnCommands[CloseIdentityCommand].Set(close, 0);
            identityOnCommands[CheckIdentityCommand].Set(check, 0);
            identityOnCommands[LoadBalancingCheckIdentityCommand].Set(loadBalancingCheck, 0);
            identityOnCommands[StreamMergeIdentityCommand].Set(mergeStreamIdentity);
            identityOnCommands[TcpStreamCommand].Set(tcpStream);
            identityOnCommands[IgnoreGroupCommand].Set(ignoreGroup);
        }

        /// <summary>
        ///     初始化命令处理委托集合
        /// </summary>
        /// <param name="count">命令数量</param>
        /// <param name="index">命令索引位置</param>
        /// <returns>命令处理委托集合</returns>
        protected TmphKeyValue<byte[][], TmphCommand[]> getCommands(int count, out int index)
        {
            index = 6;
            var datas = new byte[count + index][];
            var commands = new TmphCommand[count + index];
            datas[0] = closeCommandData;
            commands[0] = new TmphCommand(close, 0);
            datas[1] = checkCommandData;
            commands[1] = new TmphCommand(check, 0);
            datas[2] = loadBalancingCheckCommandData;
            commands[2] = new TmphCommand(loadBalancingCheck, 0);
            datas[3] = streamMergeCommandData;
            commands[3] = new TmphCommand(mergeStream);
            datas[4] = tcpStreamCommandData;
            commands[4] = new TmphCommand(tcpStream);
            datas[5] = ignoreGroupCommandData;
            commands[5] = new TmphCommand(ignoreGroup);
            return new TmphKeyValue<byte[][], TmphCommand[]>(datas, commands);
        }

        /// <summary>
        ///     客户端请求处理
        /// </summary>
        /// <param name="socket">客户端套接字</param>
        protected override void newSocket(Socket socket)
        {
            var clientQueue = this.clientQueue;
            if (clientQueue != null)
            {
                var TmphClient = clientQueue.NewClient(socket, socket);
                if ((int)TmphClient.Type >= (int)TmphClientQueue.TmphSocketType.Ipv4) newSocket(TmphClient);
            }
            else socket.shutdown();
            //else socket.Close();
        }

        /// <summary>
        ///     客户端请求处理
        /// </summary>
        /// <param name="TmphClient">客户端信息</param>
        private void newSocket(TmphClientQueue<Socket>.TmphClientInfo TmphClient)
        {
            TmphSocket commandSocket = null;
            try
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref socketPoolLock);
                if (socketPoolHead != null)
                {
                    commandSocket = socketPoolHead;
                    socketPoolHead = socketPoolHead.PoolNext;
                    commandSocket.PoolNext = null;
                }
                socketPoolLock = 0;
                if (commandSocket == null)
                {
                    var pool = TmphMemoryPool.StreamBuffers;
                    var sendData = pool.Size == attribute.SendBufferSize
                        ? pool.Get()
                        : new byte[attribute.SendBufferSize];
                    var receiveData = pool.Size == attribute.ReceiveBufferSize
                        ? pool.Get()
                        : new byte[attribute.ReceiveBufferSize];
                    commandSocket = new TmphSocket(TmphClient, this, sendData, receiveData);
                }
                else commandSocket.SetSocket(TmphClient);
                TmphClient.Client = null;
                commandSocket.VerifySocketType();
                commandSocket = null;
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
            }
            finally
            {
                //if (TmphClient.Client != null) TmphClient.Client.Close();
                TmphClient.Client.shutdown();
                if (commandSocket != null) pushSocket(commandSocket);
            }
        }

        /// <summary>
        ///     停止服务
        /// </summary>
        public override void Dispose()
        {
            var loadBalancingAttribute = this.loadBalancingAttribute;
            this.loadBalancingAttribute = null;
            if (loadBalancingAttribute != null)
            {
                try
                {
                    new TmphCommandLoadBalancingServer.TmphCommandClient(loadBalancingAttribute, loadBalancingVerifyMethod)
                        .RemoveServer(new TmphHost { Host = attribute.Host, Port = attribute.Port });
                }
                catch (Exception error)
                {
                    if (loadBalancingOnException == null) TmphLog.Error.Add(error, null, false);
                    else loadBalancingOnException(error);
                }
            }
            base.Dispose();
            TmphPub.Dispose(ref clientQueue);
            TmphInterlocked.NoCheckCompareSetSleep0(ref socketPoolLock);
            try
            {
                while (socketPoolHead != null)
                {
                    socketPoolEnd = socketPoolHead.PoolNext;
                    socketPoolHead.Dispose();
                    socketPoolHead = socketPoolEnd;
                }
            }
            finally
            {
                socketPoolHead = socketPoolEnd = null;
                socketPoolLock = 0;
            }
            TmphPub.Dispose(ref onCommands);
            TmphPub.Dispose(ref httpCommands);
        }

        /// <summary>
        ///     保存套接字
        /// </summary>
        /// <param name="socket">套接字</param>
        protected void pushSocket(TmphSocket socket)
        {
            var TmphClient = push(socket);
            if (TmphClient.Client != null) newSocket(TmphClient);
        }

        /// <summary>
        ///     保存套接字
        /// </summary>
        /// <param name="socket">套接字</param>
        /// <returns>下一个客户端</returns>
        private TmphClientQueue<Socket>.TmphClientInfo push(TmphSocket socket)
        {
            var TmphClient = new TmphClientQueue<Socket>.TmphClientInfo { Ipv4 = socket.Ipv4, Ipv6 = socket.Ipv6 };
            socket.Close();
            TmphInterlocked.NoCheckCompareSetSleep0(ref socketPoolLock);
            //if (socketPool.IndexOf(socket) == -1)
            //{
            ++socket.PushIdentity;
            if (socketPoolHead == null) socketPoolHead = socketPoolEnd = socket;
            else
            {
                socketPoolEnd.PoolNext = socket;
                socketPoolEnd = socket;
            }
            socketPoolLock = 0;
            //}
            //else
            //{
            //    socketPoolLock = 0;
            //    Laurent.Lee.CLB.TmphLog.Error.Add("套接字客户端释放冲突 ", true, false);
            //}
            var clientQueue = this.clientQueue;
            if (clientQueue != null)
            {
                TmphClient.Client = TmphClient.Ipv6.IsNull ? clientQueue.End(TmphClient.Ipv4) : clientQueue.End(TmphClient.Ipv6);
                if (TmphClient.Client != null)
                {
                    TmphClient.Type = TmphClient.Ipv6.IsNull ? TmphClientQueue.TmphSocketType.Ipv4 : TmphClientQueue.TmphSocketType.Ipv6;
                }
            }
            return TmphClient;
        }

        /// <summary>
        ///     获取客户端请求
        /// </summary>
        protected override void getSocket()
        {
            if (verify == null &&
                (identityOnCommands == null ? verifyCommand.Length == 0 : verifyCommandIdentity == int.MinValue))
                TmphLog.Error.Add("缺少TCP客户端验证接口或者方法", true, false);
            var bufferLength = maxCommandLength + sizeof(int);
            if (attribute.ReceiveBufferSize < bufferLength) attribute.ReceiveBufferSize = bufferLength;
            if (attribute.IsHttpClient) HttpServers = new TmphHttpServers(this);
            acceptSocket();
        }

        /// <summary>
        ///     获取接收数据超时时间
        /// </summary>
        /// <param name="length">接收数据字节长度</param>
        /// <returns>接收数据超时时间</returns>
        private DateTime getReceiveTimeout(int length)
        {
            return minReceivePerSecond == 0
                ? DateTime.MaxValue
                : TmphDate.NowSecond.AddSeconds(length / minReceivePerSecond + 2);
        }

        /// <summary>
        ///     流合并命令处理
        /// </summary>
        /// <param name="socket">流套接字</param>
        /// <param name="data">输入数据</param>
        private unsafe void mergeStreamIdentity(TmphSocket socket, TmphSubArray<byte> data)
        {
            var isClose = 0;
            try
            {
                var dataArray = data.Array;
                var command = default(TmphCommand);
                fixed (byte* dataFixed = dataArray)
                {
                    var dataLength = data.Count;
                    var start = dataFixed + data.StartIndex;
                    do
                    {
                        var isCloseCommand = 0;
                        if (dataLength >= sizeof(int) * 2 + sizeof(TmphStreamIdentity))
                        {
                            var commandIdentity = *(int*)start;
                            command.OnCommand = null;
                            if ((uint)commandIdentity < identityOnCommands.Length)
                            {
                                if (commandIdentity == CloseIdentityCommand)
                                {
                                    isCloseCommand = 1;
                                    command = mergeCheckCommand;
                                }
                                else command = identityOnCommands[commandIdentity];
                            }
                            if (command.OnCommand != null)
                            {
                                var length =
                                    *(int*)((start += sizeof(int) * 2 + sizeof(TmphStreamIdentity)) - sizeof(int));
                                if ((uint)length <= command.MaxDataLength &&
                                    (dataLength -= length + (sizeof(int) * 2 + sizeof(TmphStreamIdentity))) >= 0)
                                {
                                    socket.identity =
                                        *(TmphStreamIdentity*)(start - (sizeof(int) + sizeof(TmphStreamIdentity)));
                                    if (isCloseCommand == 0)
                                        command.OnCommand(socket,
                                            TmphSubArray<byte>.Unsafe(dataArray, (int)(start - dataFixed), length));
                                    else isClose = 1;
                                    start += length;
                                    if (dataLength != 0) continue;
                                }
                            }
                        }
                        break;
                    } while (true);
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
            }
            if (isClose != 0) close(socket, default(TmphSubArray<byte>));
        }

        /// <summary>
        ///     流合并命令处理
        /// </summary>
        /// <param name="socket">流套接字</param>
        /// <param name="data">输入数据</param>
        private unsafe void mergeStream(TmphSocket socket, TmphSubArray<byte> data)
        {
            var isClose = 0;
            try
            {
                var dataArray = data.Array;
                var command = default(TmphCommand);
                fixed (byte* dataFixed = dataArray)
                {
                    var dataLength = data.Count;
                    var start = dataFixed + data.StartIndex;
                    do
                    {
                        int commandLength = *(int*)start, isCloseCommand = 0;
                        // + (sizeof(TmphCommandServer.TmphStreamIdentity) - sizeof(int))
                        if ((uint)commandLength < maxCommandLength)
                        {
                            var commandIdentity = start + sizeof(int);
                            if (
                                onCommands.Get(
                                    TmphSubArray<byte>.Unsafe(dataArray, (int)(commandIdentity - dataFixed),
                                        commandLength - (sizeof(int) * 2 + sizeof(TmphStreamIdentity))), ref command))
                            {
                                if (((*(int*)commandIdentity ^ (CloseIdentityCommand + CommandDataIndex)) |
                                     (commandLength ^ (sizeof(int) * 3 + sizeof(TmphStreamIdentity)))) == 0)
                                {
                                    command = mergeCheckCommand;
                                    isCloseCommand = 1;
                                }
                                var length = *(int*)((start += commandLength) - sizeof(int));
                                if ((uint)length <= command.MaxDataLength &&
                                    (dataLength -= commandLength + length) >= 0)
                                {
                                    socket.identity =
                                        *(TmphStreamIdentity*)(start - (sizeof(int) + sizeof(TmphStreamIdentity)));
                                    if (isCloseCommand == 0)
                                        command.OnCommand(socket,
                                            TmphSubArray<byte>.Unsafe(dataArray, (int)(start - dataFixed), length));
                                    else isClose = 1;
                                    start += length;
                                    if (dataLength != 0) continue;
                                }
                            }
                        }
                        break;
                    } while (true);
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
            }
            if (isClose != 0) close(socket, default(TmphSubArray<byte>));
        }

        /// <summary>
        ///     关闭套接字
        /// </summary>
        /// <param name="socket">套接字</param>
        /// <param name="data">输入数据</param>
        private static void close(TmphSocket socket, TmphSubArray<byte> data)
        {
            socket.Close();
        }

        /// <summary>
        ///     TCP流回馈
        /// </summary>
        /// <param name="socket">套接字</param>
        /// <param name="data">输入数据</param>
        private static void tcpStream(TmphSocket socket, TmphSubArray<byte> data)
        {
            socket.SendStream(socket.identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = true });
            var parameter = TmphDataDeSerializer.DeSerialize<TmphTcpStreamParameter>(data);
            if (parameter != null) socket.OnTcpStream(parameter);
        }

        /// <summary>
        ///     忽略分组事件
        /// </summary>
        public event Action<int> OnIgnoreGroup;

        /// <summary>
        ///     忽略分组
        /// </summary>
        /// <param name="socket">套接字</param>
        /// <param name="data">输入数据</param>
        private void ignoreGroup(TmphSocket socket, TmphSubArray<byte> data)
        {
            var groupId = 0;
            try
            {
                if (!TmphDataDeSerializer.DeSerialize(data, ref groupId)) groupId = 0;
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
            }
            finally
            {
                TmphTask.Tiny.Add(ignoreGroup, new TmphKeyValue<TmphSocket, int>(socket, groupId), null);
            }
        }

        /// <summary>
        ///     忽略分组
        /// </summary>
        /// <param name="socket">套接字+分组标识</param>
        private void ignoreGroup(TmphKeyValue<TmphSocket, int> socket)
        {
            if (OnIgnoreGroup != null) OnIgnoreGroup(socket.Value);
            ignoreGroup(socket.Value);
            TmphDomainUnload.WaitTransaction();
            socket.Key.SendStream(socket.Key.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = true });
        }

        /// <summary>
        ///     忽略分组
        /// </summary>
        /// <param name="groupId">分组标识</param>
        protected virtual void ignoreGroup(int groupId)
        {
        }

        /// <summary>
        ///     连接检测套接字
        /// </summary>
        /// <param name="socket">套接字</param>
        /// <param name="data">输入数据</param>
        private static void check(TmphSocket socket, TmphSubArray<byte> data)
        {
            socket.SendStream(socket.identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = true });
        }

        /// <summary>
        ///     负载均衡联通测试
        /// </summary>
        private void loadBalancingCheck()
        {
            if (loadBalancingSocket.LoadBalancingCheck(loadBalancingCheckIdentity))
            {
                if (loadBalancingCheckTime < TmphDate.NowSecond) loadBalancingCheckTime = TmphDate.NowSecond;
                TmphTimerTask.Default.Add(loadBalancingCheckHandle,
                    loadBalancingCheckTime = loadBalancingCheckTime.AddTicks(loadBalancingCheckTicks), null);
            }
            else if (isStart == 0) loadBalancingSocket = nextLoadBalancingSocket = null;
            else
            {
                var socket = nextLoadBalancingSocket;
                if (socket == null)
                {
                    loadBalancingSocket = null;
                    loadBalancing();
                }
                else
                {
                    nextLoadBalancingSocket = null;
                    loadBalancingSocket = socket;
                    socket.LoadBalancingCheckIdentity = ++loadBalancingCheckIdentity;
                    loadBalancingCheck();
                }
            }
        }

        /// <summary>
        ///     负载均衡连接检测套接字
        /// </summary>
        /// <param name="socket">套接字</param>
        /// <param name="data">输入数据</param>
        private void loadBalancingCheck(TmphSocket socket, TmphSubArray<byte> data)
        {
            if (loadBalancingAttribute != null)
            {
                if (Interlocked.CompareExchange(ref loadBalancingSocket, socket, null) == null)
                {
                    if (loadBalancingCheckHandle == null) loadBalancingCheckHandle = loadBalancingCheck;
                    socket.LoadBalancingCheckIdentity = ++loadBalancingCheckIdentity;
                    TmphThreadPool.TinyPool.FastStart(loadBalancingCheckHandle, null, null);
                }
                else nextLoadBalancingSocket = socket;
            }
            check(socket, data);
        }

        /// <summary>
        ///     方法标识名称转TCP调用命令
        /// </summary>
        /// <param name="name">方法标识名称</param>
        /// <returns>TCP调用命令</returns>
        public static unsafe byte[] GetMethodKeyNameCommand(string name)
        {
            int length = name.Length, commandLength = (length + 3) & (int.MaxValue - 3);
            var data = new byte[commandLength + sizeof(int)];
            fixed (byte* dataFixed = data)
            {
                *(int*)dataFixed = commandLength + sizeof(int) + sizeof(TmphStreamIdentity) + sizeof(int);
                if ((length & 3) != 0) *(int*)(dataFixed + sizeof(int) + (length & (int.MaxValue - 3))) = 0x20202020;
                formatMethodKeyName(name, dataFixed + sizeof(int));
            }
            return data;
        }

        /// <summary>
        ///     格式化方法标识名称
        /// </summary>
        /// <param name="name">方法标识名称</param>
        /// <returns>方法标识名称</returns>
        protected internal static unsafe byte[] formatMethodKeyName(string name)
        {
            var length = name.Length;
            var data = new byte[(length + 3) & (int.MaxValue - 3)];
            fixed (byte* dataFixed = data)
            {
                if ((length & 3) != 0) *(int*)(dataFixed + (length & (int.MaxValue - 3))) = 0x20202020;
                formatMethodKeyName(name, dataFixed);
            }
            return data;
        }

        /// <summary>
        ///     格式化方法标识名称
        /// </summary>
        /// <param name="name">方法标识名称</param>
        /// <param name="write">写入数据起始位置</param>
        protected internal static unsafe void formatMethodKeyName(string name, byte* write)
        {
            fixed (char* commandFixed = name)
            {
                for (char* start = commandFixed + name.Length, end = commandFixed;
                    start != end;
                    *write++ = (byte)*--start)
                    ;
            }
        }

        ///// <summary>
        ///// 调试日志
        ///// </summary>
        //private static TmphLog debugLog;
        ///// <summary>
        ///// 调试日志访问锁
        ///// </summary>
        //private static int debugLogLock;
        ///// <summary>
        ///// 调试日志
        ///// </summary>
        //internal static TmphLog DebugLog
        //{
        //    get
        //    {
        //        if (debugLog == null)
        //        {
        //            THInterlocked.CompareSetSleep0NoCheck(ref debugLogLock);
        //            try
        //            {
        //                debugLog = new TmphLog(Laurent.Lee.CLB.Config.appSetting.LogPath + "socketDebug.txt");
        //            }
        //            finally { debugLogLock = 0; }
        //        }
        //        return debugLog;
        //    }
        //}

        /// <summary>
        ///     关闭套接字
        /// </summary>
        /// <param name="socket">套接字</param>
        private static void closeSocket(Socket socket)
        {
            socket.Close();
        }

        /// <summary>
        ///     命令处理委托
        /// </summary>
        public struct TmphCommand
        {
            /// <summary>
            ///     最大参数数据长度,0表示不接受参数数据
            /// </summary>
            public int MaxDataLength;

            /// <summary>
            ///     命令处理委托
            /// </summary>
            public Action<TmphSocket, TmphSubArray<byte>> OnCommand;

            /// <summary>
            ///     命令处理委托
            /// </summary>
            /// <param name="onCommand">命令处理委托</param>
            /// <param name="maxDataLength">最大参数数据长度,0表示不接受参数数据</param>
            public TmphCommand(Action<TmphSocket, TmphSubArray<byte>> onCommand, int maxDataLength = int.MaxValue)
            {
                OnCommand = onCommand;
                MaxDataLength = maxDataLength;
            }

            /// <summary>
            ///     设置命令处理委托
            /// </summary>
            /// <param name="onCommand">命令处理委托</param>
            /// <param name="maxDataLength">最大参数数据长度,0表示不接受参数数据</param>
            public void Set(Action<TmphSocket, TmphSubArray<byte>> onCommand, int maxDataLength = int.MaxValue)
            {
                OnCommand = onCommand;
                MaxDataLength = maxDataLength;
            }
        }

        /// <summary>
        ///     会话标识
        /// </summary>
        public struct TmphStreamIdentity
        {
            /// <summary>
            ///     请求标识
            /// </summary>
            public int Identity;

            /// <summary>
            ///     请求索引
            /// </summary>
            public int Index;
        }

        /// <summary>
        ///     服务器端调用
        /// </summary>
        /// <typeparam name="TSocketType">套接字类型</typeparam>
        public abstract class TmphSocketCall
        {
            /// <summary>
            ///     调用委托
            /// </summary>
            protected Action callHandle;

            /// <summary>
            ///     回话标识
            /// </summary>
            protected TmphStreamIdentity identity;

            /// <summary>
            ///     套接字重用标识
            /// </summary>
            protected int pushIdentity;

            /// <summary>
            ///     套接字
            /// </summary>
            protected TmphSocket socket;

            /// <summary>
            ///     服务器端调用
            /// </summary>
            protected TmphSocketCall()
            {
                callHandle = call;
            }

            /// <summary>
            ///     判断套接字是否有效
            /// </summary>
            protected int isVerify
            {
                get { return pushIdentity ^ socket.PushIdentity; }
            }

            /// <summary>
            ///     调用处理
            /// </summary>
            protected abstract void call();
        }

        /// <summary>
        ///     服务器端调用
        /// </summary>
        /// <typeparam name="TSocketType">套接字类型</typeparam>
        /// <typeparam name="TInputParameterType">输入参数类型</typeparam>
        public abstract class TmphSocketCall<TCallType> : TmphSocketCall
            where TCallType : TmphSocketCall<TCallType>
        {
            /// <summary>
            ///     设置参数
            /// </summary>
            /// <param name="socket">套接字</param>
            /// <param name="identity">回话标识</param>
            /// <param name="inputParameter">输入参数</param>
            private void set(TmphSocket socket, TmphStreamIdentity identity)
            {
                this.socket = socket;
                this.identity = identity;
                pushIdentity = socket.PushIdentity;
            }

            /// <summary>
            ///     获取服务器端调用
            /// </summary>
            /// <param name="socket"></param>
            /// <param name="identity"></param>
            /// <param name="inputParameter"></param>
            /// <returns></returns>
            public static Action Call(TmphSocket socket, TmphStreamIdentity identity)
            {
                var value = TmphTypePool<TCallType>.Pop();
                if (value == null)
                {
                    try
                    {
                        value = TmphConstructor<TCallType>.New();
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                        return null;
                    }
                }
                value.set(socket, identity);
                return value.callHandle;
            }
        }

        /// <summary>
        ///     服务器端调用
        /// </summary>
        /// <typeparam name="TSocketType">套接字类型</typeparam>
        /// <typeparam name="TInputParameterType">输入参数类型</typeparam>
        public abstract class TmphSocketCall<TCallType, TInputParameterType> : TmphSocketCall
            where TCallType : TmphSocketCall<TCallType, TInputParameterType>
        {
            /// <summary>
            ///     输入参数
            /// </summary>
            protected TInputParameterType inputParameter;

            /// <summary>
            ///     设置参数
            /// </summary>
            /// <param name="socket">套接字</param>
            /// <param name="identity">回话标识</param>
            /// <param name="inputParameter">输入参数</param>
            private void set(TmphSocket socket, TmphStreamIdentity identity, TInputParameterType inputParameter)
            {
                this.socket = socket;
                this.identity = identity;
                this.inputParameter = inputParameter;
                pushIdentity = socket.PushIdentity;
            }

            /// <summary>
            ///     获取服务器端调用
            /// </summary>
            /// <param name="socket"></param>
            /// <param name="identity"></param>
            /// <param name="inputParameter"></param>
            /// <returns></returns>
            public static Action Call(TmphSocket socket, TmphStreamIdentity identity, TInputParameterType inputParameter)
            {
                var value = TmphTypePool<TCallType>.Pop();
                if (value == null)
                {
                    try
                    {
                        value = TmphConstructor<TCallType>.New();
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                        return null;
                    }
                }
                value.set(socket, identity, inputParameter);
                return value.callHandle;
            }
        }

        /// <summary>
        ///     服务器端调用
        /// </summary>
        /// <typeparam name="TSocketType">套接字类型</typeparam>
        /// <typeparam name="TServerType">服务器目标对象类型</typeparam>
        public abstract class TmphServerCall<TCallType, TServerType> : TmphSocketCall
            where TCallType : TmphServerCall<TCallType, TServerType>
        {
            /// <summary>
            ///     服务器目标对象
            /// </summary>
            protected TServerType serverValue;

            /// <summary>
            ///     设置参数
            /// </summary>
            /// <param name="socket">套接字</param>
            /// <param name="serverValue">服务器目标对象</param>
            /// <param name="identity">回话标识</param>
            private void set(TmphSocket socket, TServerType serverValue, TmphStreamIdentity identity)
            {
                this.socket = socket;
                this.serverValue = serverValue;
                this.identity = identity;
                pushIdentity = socket.PushIdentity;
            }

            /// <summary>
            ///     获取服务器端调用
            /// </summary>
            /// <param name="socket"></param>
            /// <param name="serverValue"></param>
            /// <param name="identity"></param>
            /// <returns></returns>
            public static Action Call(TmphSocket socket, TServerType serverValue, TmphStreamIdentity identity)
            {
                var value = TmphTypePool<TCallType>.Pop();
                if (value == null)
                {
                    try
                    {
                        value = TmphConstructor<TCallType>.New();
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                        return null;
                    }
                }
                value.set(socket, serverValue, identity);
                return value.callHandle;
            }
        }

        /// <summary>
        ///     服务器端调用
        /// </summary>
        /// <typeparam name="TSocketType">套接字类型</typeparam>
        /// <typeparam name="TServerType">服务器目标对象类型</typeparam>
        /// <typeparam name="TInputParameterType">输入参数类型</typeparam>
        public abstract class TmphServerCall<TCallType, TServerType, TInputParameterType> :
            TmphServerCall<TCallType, TServerType>
            where TCallType : TmphServerCall<TCallType, TServerType, TInputParameterType>
        {
            /// <summary>
            ///     输入参数
            /// </summary>
            protected TInputParameterType inputParameter;

            /// <summary>
            ///     设置参数
            /// </summary>
            /// <param name="socket">套接字</param>
            /// <param name="serverValue">服务器目标对象</param>
            /// <param name="identity">回话标识</param>
            /// <param name="inputParameter">输入参数</param>
            private void set(TmphSocket socket, TServerType serverValue, TmphStreamIdentity identity,
                TInputParameterType inputParameter)
            {
                this.socket = socket;
                this.serverValue = serverValue;
                this.identity = identity;
                this.inputParameter = inputParameter;
                pushIdentity = socket.PushIdentity;
            }

            /// <summary>
            ///     获取服务器端调用
            /// </summary>
            /// <param name="socket"></param>
            /// <param name="serverValue"></param>
            /// <param name="identity"></param>
            /// <param name="inputParameter"></param>
            /// <returns></returns>
            public static Action Call(TmphSocket socket, TServerType serverValue, TmphStreamIdentity identity,
                TInputParameterType inputParameter)
            {
                var value = TmphTypePool<TCallType>.Pop();
                if (value == null)
                {
                    try
                    {
                        value = TmphConstructor<TCallType>.New();
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                        return null;
                    }
                }
                value.set(socket, serverValue, identity, inputParameter);
                return value.callHandle;
            }
        }

        /// <summary>
        ///     TCP流命令类型
        /// </summary>
        internal enum TmphTcpStreamCommand : byte
        {
            /// <summary>
            ///     获取流字节长度
            /// </summary>
            GetLength,

            /// <summary>
            ///     设置流字节长度
            /// </summary>
            SetLength,

            /// <summary>
            ///     获取当前位置
            /// </summary>
            GetPosition,

            /// <summary>
            ///     设置当前位置
            /// </summary>
            SetPosition,

            /// <summary>
            ///     获取读取超时
            /// </summary>
            GetReadTimeout,

            /// <summary>
            ///     设置读取超时
            /// </summary>
            SetReadTimeout,

            /// <summary>
            ///     获取写入超时
            /// </summary>
            GetWriteTimeout,

            /// <summary>
            ///     设置写入超时
            /// </summary>
            SetWriteTimeout,

            /// <summary>
            ///     异步读取
            /// </summary>
            BeginRead,

            /// <summary>
            ///     读取字节序列
            /// </summary>
            Read,

            /// <summary>
            ///     读取字节
            /// </summary>
            ReadByte,

            /// <summary>
            ///     异步写入
            /// </summary>
            BeginWrite,

            /// <summary>
            ///     写入字节序列
            /// </summary>
            Write,

            /// <summary>
            ///     写入字节
            /// </summary>
            WriteByte,

            /// <summary>
            ///     设置流位置
            /// </summary>
            Seek,

            /// <summary>
            ///     清除缓冲区
            /// </summary>
            Flush,

            /// <summary>
            ///     关闭流
            /// </summary>
            Close
        }

        /// <summary>
        ///     TCP流异步接口
        /// </summary>
        internal interface TmphITcpStreamCallback
        {
            /// <summary>
            ///     TCP流异步回调
            /// </summary>
            /// <param name="TmphTcpStreamAsyncResult">TCP流异步操作状态</param>
            /// <param name="parameter">TCP流参数</param>
            void Callback(TmphTcpStreamAsyncResult TmphTcpStreamAsyncResult, TmphTcpStreamParameter parameter);
        }

        /// <summary>
        ///     TCP流参数
        /// </summary>
        internal sealed class TmphTcpStreamParameter
        {
            /// <summary>
            ///     空TCP流参数
            /// </summary>
            public static readonly TmphTcpStreamParameter Null = new TmphTcpStreamParameter();

            /// <summary>
            ///     客户端序号
            /// </summary>
            public int ClientIdentity;

            /// <summary>
            ///     客户端索引
            /// </summary>
            public int ClientIndex;

            /// <summary>
            ///     命令类型
            /// </summary>
            public TmphTcpStreamCommand Command;

            /// <summary>
            ///     数据参数
            /// </summary>
            public TmphSubArray<byte> Data;

            /// <summary>
            ///     命令序号
            /// </summary>
            public int Identity;

            /// <summary>
            ///     命令集合索引
            /// </summary>
            public int Index;

            /// <summary>
            ///     客户端流是否存在
            /// </summary>
            public bool IsClientStream;

            /// <summary>
            ///     客户端命令是否成功
            /// </summary>
            public bool IsCommand;

            /// <summary>
            ///     位置参数
            /// </summary>
            public long Offset;

            /// <summary>
            ///     查找类型参数
            /// </summary>
            public SeekOrigin SeekOrigin;

            /// <summary>
            ///     缓冲区处理
            /// </summary>
            public Action<bool> PushClientBuffer
            {
                get
                {
                    if (Data.array != null && Data.array.Length == TmphCommandSocket.asyncBuffers.Size)
                        return pushClientBuffer;
                    return null;
                }
            }

            /// <summary>
            ///     缓冲区处理
            /// </summary>
            private void pushClientBuffer(bool _)
            {
                TmphCommandSocket.asyncBuffers.Push(ref Data.array);
            }

            /// <summary>
            ///     反序列化
            /// </summary>
            /// <param name="deSerializer">对象反序列化器</param>
            internal unsafe void DeSerialize(TmphDataDeSerializer deSerializer)
            {
                var start = deSerializer.Read;
                int bufferSize = *(int*)(start + (sizeof(int) * 5 + sizeof(long))),
                    dataSize = sizeof(int) * 7 + sizeof(long) + ((bufferSize + 3) & (int.MaxValue - 3));
                if (deSerializer.VerifyRead(dataSize) && *(int*)(start + dataSize - sizeof(int)) == dataSize)
                {
                    Index = *(int*)start;
                    Identity = *(int*)(start + sizeof(int));
                    ClientIndex = *(int*)(start + sizeof(int) * 2);
                    ClientIdentity = *(int*)(start + sizeof(int) * 3);
                    Offset = *(long*)(start + sizeof(int) * 4);
                    SeekOrigin = (SeekOrigin)(*(start + (sizeof(int) * 4 + sizeof(long))));
                    Command = (TmphTcpStreamCommand)(*(start + (sizeof(int) * 4 + sizeof(long) + 1)));
                    IsClientStream = *(start + (sizeof(int) * 4 + sizeof(long) + 2)) != 0;
                    IsCommand = *(start + (sizeof(int) * 4 + sizeof(long) + 3)) != 0;
                    if (bufferSize == 0) Data.UnsafeSet(TmphNullValue<byte>.Array, 0, 0);
                    else
                    {
                        Data.UnsafeSet(new byte[bufferSize], 0, bufferSize);
                        Unsafe.TmphMemory.Copy(start + (sizeof(int) * 6 + sizeof(long)), Data.Array, bufferSize);
                    }
                }
            }

            /// <summary>
            ///     反序列化
            /// </summary>
            /// <param name="deSerializer">对象反序列化器</param>
            [TmphDataSerialize.TmphCustom]
            private static void deSerialize(TmphDataDeSerializer deSerializer, ref TmphTcpStreamParameter value)
            {
                //if (deSerializer.CheckNull() == 0) value = null;
                //else
                //{
                //    if (value == null) value = new TmphTcpStreamParameter();
                //    value.DeSerialize(deSerializer);
                //}
                (value = new TmphTcpStreamParameter()).DeSerialize(deSerializer);
            }

            /// <summary>
            ///     对象序列化
            /// </summary>
            /// <param name="stream">数据流</param>
            private unsafe void serialize(TmphDataSerializer serializer)
            {
                var stream = serializer.Stream;
                var length = sizeof(int) * 7 + sizeof(long) + ((Data.Count + 3) & (int.MaxValue - 3));
                stream.PrepLength(length);
                var unsafeStream = stream.Unsafer;
                unsafeStream.Write(Index);
                unsafeStream.Write(Identity);
                unsafeStream.Write(ClientIndex);
                unsafeStream.Write(ClientIdentity);
                unsafeStream.Write(Offset);
                unsafeStream.Write((byte)SeekOrigin);
                unsafeStream.Write((byte)Command);
                unsafeStream.Write(IsClientStream ? (byte)1 : (byte)0);
                unsafeStream.Write(IsCommand ? (byte)1 : (byte)0);
                unsafeStream.Write(Data.Count);
                if (Data.Count != 0)
                {
                    fixed (byte* dataFixed = Data.Array)
                    {
                        Unsafe.TmphMemory.Copy(dataFixed + Data.StartIndex, stream.CurrentData, Data.Count);
                    }
                    unsafeStream.AddLength((Data.Count + 3) & (int.MaxValue - 3));
                }
                unsafeStream.Write(length);
            }

            /// <summary>
            ///     对象序列化
            /// </summary>
            /// <param name="stream">数据流</param>
            [TmphDataSerialize.TmphCustom]
            private static void serialize(TmphDataSerializer serializer, TmphTcpStreamParameter value)
            {
                value.serialize(serializer);
                //if (value == null) serializer.Stream.Write(Laurent.Lee.CLB.Emit.binarySerializer.NullValue);
                //else value.serialize(serializer);
            }
        }

        /// <summary>
        ///     TCP流异步操作状态
        /// </summary>
        internal class TmphTcpStreamAsyncResult : IAsyncResult
        {
            /// <summary>
            ///     等待异步操作完成
            /// </summary>
            private EventWaitHandle asyncWaitHandle;

            /// <summary>
            ///     等待异步操作完成访问锁
            /// </summary>
            private int asyncWaitHandleLock;

            /// <summary>
            ///     异步回调
            /// </summary>
            public AsyncCallback Callback;

            /// <summary>
            ///     等待异步操作是否完成
            /// </summary>
            public bool IsCallback;

            /// <summary>
            ///     TCP流参数
            /// </summary>
            public TmphTcpStreamParameter Parameter;

            /// <summary>
            ///     TCP流异步接口
            /// </summary>
            public TmphITcpStreamCallback TcpStreamCallback;

            /// <summary>
            ///     用户定义的对象
            /// </summary>
            public object AsyncState { get; set; }

            /// <summary>
            ///     等待异步操作完成
            /// </summary>
            public WaitHandle AsyncWaitHandle
            {
                get
                {
                    if (asyncWaitHandle == null)
                    {
                        TmphInterlocked.NoCheckCompareSetSleep0(ref asyncWaitHandleLock);
                        try
                        {
                            if (asyncWaitHandle == null)
                                asyncWaitHandle = new EventWaitHandle(IsCallback, EventResetMode.ManualReset);
                        }
                        finally
                        {
                            asyncWaitHandleLock = 0;
                        }
                    }
                    return asyncWaitHandle;
                }
            }

            /// <summary>
            ///     是否同步完成
            /// </summary>
            public bool CompletedSynchronously
            {
                get { return false; }
            }

            /// <summary>
            ///     异步操作是否已完成
            /// </summary>
            public bool IsCompleted { get; set; }

            /// <summary>
            ///     异步回调
            /// </summary>
            /// <param name="parameter">TCP流参数</param>
            public void OnCallback(TmphTcpStreamParameter parameter)
            {
                try
                {
                    TcpStreamCallback.Callback(this, parameter);
                }
                finally
                {
                    TmphInterlocked.NoCheckCompareSetSleep0(ref asyncWaitHandleLock);
                    IsCallback = true;
                    var asyncWaitHandle = this.asyncWaitHandle;
                    asyncWaitHandleLock = 0;
                    if (asyncWaitHandle != null) asyncWaitHandle.Set();
                    if (Callback != null) Callback(this);
                }
            }
        }

        /// <summary>
        ///     TCP流读取器
        /// </summary>
        private struct TmphTcpStreamReceiver
        {
            /// <summary>
            ///     异步状态
            /// </summary>
            public TmphTcpStreamAsyncResult AsyncResult;

            /// <summary>
            ///     当前处理序号
            /// </summary>
            public int Identity;

            /// <summary>
            ///     TCP流参数
            /// </summary>
            public TmphTcpStreamParameter Parameter;

            /// <summary>
            ///     TCP流读取等待
            /// </summary>
            public EventWaitHandle ReceiveWait;

            /// <summary>
            ///     获取读取数据
            /// </summary>
            /// <param name="identity">当前处理序号</param>
            /// <param name="parameter">TCP流参数</param>
            /// <returns>是否成功</returns>
            public bool Get(int identity, ref TmphTcpStreamParameter parameter)
            {
                if (identity == Identity)
                {
                    parameter = Parameter;
                    AsyncResult = null;
                    ++Identity;
                    Parameter = null;
                    return true;
                }
                return false;
            }

            /// <summary>
            ///     设置异步状态
            /// </summary>
            /// <param name="asyncResult">异步状态</param>
            public void SetAsyncResult(TmphTcpStreamAsyncResult asyncResult)
            {
                if (asyncResult == null)
                {
                    if (ReceiveWait == null) ReceiveWait = new EventWaitHandle(false, EventResetMode.AutoReset);
                    else ReceiveWait.Reset();
                }
                else AsyncResult = asyncResult;
            }

            /// <summary>
            ///     取消读取
            /// </summary>
            /// <param name="isSetWait">是否设置结束状态</param>
            public void Cancel(bool isSetWait)
            {
                var asyncResult = AsyncResult;
                ++Identity;
                Parameter = null;
                AsyncResult = null;
                if (asyncResult == null)
                {
                    if (isSetWait && ReceiveWait != null) ReceiveWait.Set();
                }
                else asyncResult.OnCallback(null);
            }

            /// <summary>
            ///     取消读取
            /// </summary>
            /// <param name="identity">当前处理序号</param>
            /// <param name="isSetWait">是否设置结束状态</param>
            /// <returns>是否成功</returns>
            public bool Cancel(int identity, bool isSetWait)
            {
                if (identity == Identity)
                {
                    Cancel(isSetWait);
                    return true;
                }
                return false;
            }

            /// <summary>
            ///     设置TCP流参数
            /// </summary>
            /// <param name="parameter">TCP流参数</param>
            /// <param name="asyncResult">异步状态</param>
            /// <returns>是否成功</returns>
            public bool Set(TmphTcpStreamParameter parameter, ref TmphTcpStreamAsyncResult asyncResult)
            {
                if (Identity == parameter.Identity)
                {
                    asyncResult = AsyncResult;
                    if (AsyncResult == null)
                    {
                        Parameter = parameter;
                        ReceiveWait.Set();
                    }
                    else
                    {
                        ++Identity;
                        AsyncResult = null;
                    }
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        ///     HTTP命令处理委托
        /// </summary>
        public struct TmphHttpCommand
        {
            /// <summary>
            ///     是否仅支持POST调用
            /// </summary>
            public bool IsPostOnly;

            /// <summary>
            ///     最大参数数据长度,0表示不接受参数数据
            /// </summary>
            public int MaxDataLength;

            /// <summary>
            ///     HTTP命令处理委托
            /// </summary>
            public Action<TmphSocketBase> OnCommand;

            /// <summary>
            ///     HTTP命令处理委托
            /// </summary>
            /// <param name="onCommand">命令处理委托</param>
            /// <param name="IsPostOnly">是否仅支持POST调用</param>
            /// <param name="maxDataLength">最大参数数据长度,0表示不接受参数数据</param>
            public TmphHttpCommand(Action<TmphSocketBase> onCommand, bool isPostOnly, int maxDataLength = int.MaxValue)
            {
                OnCommand = onCommand;
                MaxDataLength = maxDataLength;
                IsPostOnly = isPostOnly;
            }

            /// <summary>
            ///     设置命令处理委托
            /// </summary>
            /// <param name="onCommand">命令处理委托</param>
            /// <param name="IsPostOnly">是否仅支持POST调用</param>
            /// <param name="maxDataLength">最大参数数据长度,0表示不接受参数数据</param>
            public void Set(Action<TmphSocketBase> onCommand, bool isPostOnly, int maxDataLength = int.MaxValue)
            {
                OnCommand = onCommand;
                MaxDataLength = maxDataLength;
                IsPostOnly = isPostOnly;
            }
        }

        /// <summary>
        ///     HTTP服务器
        /// </summary>
        internal sealed class TmphHttpServers : TmphServers
        {
            /// <summary>
            ///     TCP调用服务端
            /// </summary>
            private readonly TmphCommandServer commandServer;

            /// <summary>
            ///     域名服务信息
            /// </summary>
            private readonly TmphDomainServer domainServer;

            /// <summary>
            ///     HTTP服务器
            /// </summary>
            /// <param name="commandServer">TCP调用服务端</param>
            public TmphHttpServers(TmphCommandServer commandServer)
            {
                this.commandServer = commandServer;
                domainServer = new TmphDomainServer { Server = new TmphServer(this) };
            }

            /// <summary>
            ///     获取HTTP转发代理服务客户端
            /// </summary>
            /// <returns>HTTP转发代理服务客户端,失败返回null</returns>
            internal override TmphClient GetForwardClient()
            {
                return null;
            }

            /// <summary>
            ///     获取域名服务信息
            /// </summary>
            /// <param name="domain">域名</param>
            /// <returns>域名服务信息</returns>
            internal override TmphDomainServer GetServer(TmphSubArray<byte> domain)
            {
                return domainServer;
            }

            /// <summary>
            ///     HTTP服务
            /// </summary>
            public sealed class TmphServer : Http.TmphDomainServer
            {
                /// <summary>
                ///     HTTP服务器
                /// </summary>
                private readonly TmphHttpServers httpServers;

                /// <summary>
                ///     HTTP服务
                /// </summary>
                /// <param name="httpServers">HTTP服务器</param>
                public TmphServer(TmphHttpServers httpServers)
                {
                    this.httpServers = httpServers;
                    Session = new TmphSession<object>();
                }

                /// <summary>
                ///     客户端缓存时间(单位:秒)
                /// </summary>
                protected override int clientCacheSeconds
                {
                    get { return 0; }
                }

                /// <summary>
                ///     最大文件缓存字节数(单位KB)
                /// </summary>
                protected override int maxCacheFileSize
                {
                    get { return 0; }
                }

                /// <summary>
                ///     文件路径
                /// </summary>
                protected override int maxCacheSize
                {
                    get { return 0; }
                }

                /// <summary>
                ///     启动HTTP服务
                /// </summary>
                /// <param name="domains">域名信息集合</param>
                /// <param name="onStop">停止服务处理</param>
                /// <returns>是否启动成功</returns>
                public override bool Start(TmphDomain[] domains, Action onStop)
                {
                    return false;
                }

                /// <summary>
                ///     HTTP请求处理
                /// </summary>
                /// <param name="socket">HTTP套接字</param>
                /// <param name="socketIdentity">套接字操作编号</param>
                public override void Request(TmphSocketBase socket, long socketIdentity)
                {
                    var request = socket.RequestHeader;
                    TmphResponse response = null;
                    try
                    {
                        var commandName = request.Path;
                        if (commandName.Count != 0)
                        {
                            commandName.UnsafeSet(commandName.StartIndex + 1, commandName.Count - 1);
                            var command = default(TmphHttpCommand);
                            if (httpServers.commandServer.httpCommands.Get(commandName, ref command))
                            {
                                if (request.Method == TmphHttp.TmphMethodType.GET)
                                {
                                    if (request.ContentLength == 0 && !command.IsPostOnly)
                                    {
                                        var clientUserInfo = new TmphTcpBase.TmphClient();
                                        var page = TmphTcpBase.TmphHttpPage.Get(socket, this, socketIdentity, request, null);
                                        ((TmphWebPage.TmphPage)page).Response = (response = TmphResponse.Get(true));
                                        clientUserInfo.UserInfo = page;
                                        socket.TcpCommandSocket.ClientUserInfo = clientUserInfo;
                                        command.OnCommand(socket);
                                        return;
                                    }
                                }
                                else if (request.PostType != TmphRequestHeader.TmphPostType.None &&
                                         (uint)request.ContentLength <= command.MaxDataLength)
                                {
                                    socket.GetForm(socketIdentity,
                                        TmphHttpLoadForm.Get(socket, this, socketIdentity, request, command));
                                    return;
                                }
                                socket.ResponseError(socketIdentity, TmphResponse.TmphState.MethodNotAllowed405);
                                return;
                            }
                            else Console.Write("");
                        }
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                    finally
                    {
                        TmphResponse.Push(ref response);
                    }
                    socket.ResponseError(socketIdentity, TmphResponse.TmphState.NotFound404);
                }

                /// <summary>
                ///     创建错误输出数据
                /// </summary>
                protected override void createErrorResponse()
                {
                }
            }
        }

        /// <summary>
        ///     HTTP表单数据加载处理
        /// </summary>
        private sealed class TmphHttpLoadForm : TmphRequestForm.TmphILoadForm
        {
            /// <summary>
            ///     HTTP命令处理委托
            /// </summary>
            private TmphHttpCommand command;

            /// <summary>
            ///     HTTP服务
            /// </summary>
            private TmphHttpServers.TmphServer domainServer;

            /// <summary>
            ///     请求头部信息
            /// </summary>
            private TmphRequestHeader request;

            /// <summary>
            ///     HTTP套接字
            /// </summary>
            private TmphSocketBase socket;

            /// <summary>
            ///     套接字操作编号
            /// </summary>
            private long socketIdentity;

            /// <summary>
            ///     表单回调处理
            /// </summary>
            /// <param name="form">HTTP请求表单</param>
            public void OnGetForm(TmphRequestForm form)
            {
                var socket = this.socket;
                TmphResponse response = null;
                try
                {
                    if (form != null)
                    {
                        socketIdentity = form.Identity;
                        var clientUserInfo = new TmphTcpBase.TmphClient();
                        var page = TmphTcpBase.TmphHttpPage.Get(socket, domainServer, socketIdentity, request, form);
                        ((TmphWebPage.TmphPage)page).Response = (response = TmphResponse.Get(true));
                        clientUserInfo.UserInfo = page;
                        socket.TcpCommandSocket.ClientUserInfo = clientUserInfo;
                        command.OnCommand(socket);
                        return;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                finally
                {
                    this.socket = null;
                    domainServer = null;
                    request = null;
                    TmphTypePool<TmphHttpLoadForm>.Push(this);
                    TmphResponse.Push(ref response);
                }
                socket.ResponseError(socketIdentity, TmphResponse.TmphState.ServerError500);
            }

            /// <summary>
            ///     根据HTTP请求表单值获取内存流最大字节数
            /// </summary>
            /// <param name="value">HTTP请求表单值</param>
            /// <returns>内存流最大字节数</returns>
            public int MaxMemoryStreamSize(TmphRequestForm.TmphValue value)
            {
                return 0;
            }

            /// <summary>
            ///     根据HTTP请求表单值获取保存文件全称
            /// </summary>
            /// <param name="value">HTTP请求表单值</param>
            /// <returns>文件全称</returns>
            public string GetSaveFileName(TmphRequestForm.TmphValue value)
            {
                return null;
            }

            /// <summary>
            ///     获取HTTP请求表单数据加载处理委托
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            /// <param name="socketIdentity">套接字操作编号</param>
            /// <param name="request">请求头部信息</param>
            /// <param name="command">HTTP命令处理委托</param>
            /// <returns>HTTP请求表单数据加载处理委托</returns>
            internal static TmphHttpLoadForm Get
                (TmphSocketBase socket, TmphHttpServers.TmphServer domainServer, long socketIdentity, TmphRequestHeader request,
                    TmphHttpCommand command)
            {
                var loadForm = TmphTypePool<TmphHttpLoadForm>.Pop() ?? new TmphHttpLoadForm();
                loadForm.socket = socket;
                loadForm.domainServer = domainServer;
                loadForm.socketIdentity = socketIdentity;
                loadForm.request = request;
                loadForm.command = command;
                return loadForm;
            }
        }

        /// <summary>
        ///     TCP调用套接字
        /// </summary>
        /// <typeparam name="TServerType">TCP调用服务端类型</typeparam>
        public sealed unsafe class TmphSocket : TmphCommandSocket<TmphCommandServer>
        {
            /// <summary>
            ///     临时数据缓冲区
            /// </summary>
            private static readonly byte[] closeData = new byte[sizeof(TmphStreamIdentity) + sizeof(int)];

            /// <summary>
            ///     创建输出数据并执行
            /// </summary>
            private readonly Action buildOutputHandle;

            /// <summary>
            ///     获取TCP调用客户端套接字类型
            /// </summary>
            private readonly Action<bool> onSocketTypeHandle;

            /// <summary>
            ///     取消TCP流读取
            /// </summary>
            private Action<long> cancelTcpStreamHandle;

            /// <summary>
            ///     当前处理命令
            /// </summary>
            private TmphCommand command;

            /// <summary>
            ///     执行命令委托
            /// </summary>
            private Action<TmphMemoryPool.TmphPushSubArray> doStreamCommandHandle;

            /// <summary>
            ///     TCP流读取器空闲索引集合
            /// </summary>
            private TmphSubArray<int> freeTcpStreamIndexs;

            /// <summary>
            ///     客户端IP地址
            /// </summary>
            internal int Ipv4;

            /// <summary>
            ///     客户端IP地址
            /// </summary>
            internal TmphIpv6Hash Ipv6;

            /// <summary>
            ///     是否正在创建输出信息
            /// </summary>
            private byte isBuildOutput;

            /// <summary>
            ///     是否正在创建输出信息
            /// </summary>
            private int isOutputBuilding;

            /// <summary>
            ///     负载均衡联通测试标识
            /// </summary>
            internal int LoadBalancingCheckIdentity;

            /// <summary>
            ///     接收命令处理
            /// </summary>
            private Action<int> onReceiveStreamCommandHandle;

            /// <summary>
            ///     接收命令长度处理
            /// </summary>
            private Action<int> onReceiveStreamCommandLengthHandle;

            /// <summary>
            ///     输出信息集合访问锁
            /// </summary>
            private int outputLock;

            /// <summary>
            ///     输出信息队列集合
            /// </summary>
            private TmphOutputQueue outputs;

            /// <summary>
            ///     套接字池下一个TCP调用套接字
            /// </summary>
            internal TmphSocket PoolNext;

            /// <summary>
            ///     套接字重用标识
            /// </summary>
            internal int PushIdentity;

            /// <summary>
            ///     同步接收命令
            /// </summary>
            private Action receiveCommandHandle;

            ///// <summary>
            ///// 是否已经设置流接收超时
            ///// </summary>
            //private int isStreamReceiveTimeout;
            /// <summary>
            ///     接收数据缓冲区起始位置
            /// </summary>
            private byte* receiveDataFixed;

            /// <summary>
            ///     接收数据结束位置
            /// </summary>
            private int receiveEndIndex;

            /// <summary>
            ///     接收数据起始位置
            /// </summary>
            private int receiveStartIndex;

            /// <summary>
            ///     TCP流读取器索引
            /// </summary>
            private int tcpStreamReceiveIndex;

            /// <summary>
            ///     TCP流读取器访问锁
            /// </summary>
            private int tcpStreamReceiveLock;

            /// <summary>
            ///     TCP流读取器集合
            /// </summary>
            private TmphTcpStreamReceiver[] tcpStreamReceivers;

            /// <summary>
            ///     验证超时
            /// </summary>
            private DateTime verifyTimeout;

            static TmphSocket()
            {
                fixed (byte* dataFixed = closeData)
                {
                    *(TmphStreamIdentity*)dataFixed = new TmphStreamIdentity { Index = 0, Identity = int.MinValue };
                    *(int*)(dataFixed + sizeof(TmphStreamIdentity)) = 0;
                }
            }

            ///// <summary>
            ///// 是否输出调试信息
            ///// </summary>
            //private bool isOutputDebug;
            /// <summary>
            ///     初始化同步套接字
            /// </summary>
            /// <param name="TmphClient">客户端信息</param>
            /// <param name="server">TCP调用服务</param>
            /// <param name="sendData">发送数据缓冲区</param>
            /// <param name="receiveData">接收数据缓冲区</param>
            internal TmphSocket(TmphClientQueue<Socket>.TmphClientInfo TmphClient, TmphCommandServer server, byte[] sendData,
                byte[] receiveData)
                : base(TmphClient.Client, sendData, receiveData, server, false)
            {
                Ipv4 = TmphClient.Ipv4;
                Ipv6 = TmphClient.Ipv6;
                buildOutputHandle = buildOutput;
                onSocketTypeHandle = onSocketType;
                //isOutputDebug = server.attribute.IsOutputDebug;
            }

            /// <summary>
            ///     默认HTTP内容编码
            /// </summary>
            internal override Encoding HttpEncoding
            {
                get { return commandSocketProxy.attribute.HttpEncoding; }
            }

            /// <summary>
            ///     重新设置套接字
            /// </summary>
            /// <param name="TmphClient">客户端信息</param>
            internal void SetSocket(TmphClientQueue<Socket>.TmphClientInfo TmphClient)
            {
                Socket = TmphClient.Client;
                Ipv4 = TmphClient.Ipv4;
                Ipv6 = TmphClient.Ipv6;
                IsVerifyMethod = false;
                //isStreamReceiveTimeout = 0;
                socketError = SocketError.Success;
                lastException = null;
                ClientUserInfo = null;
                LoadBalancingCheckIdentity = 0;
            }

            /// <summary>
            ///     关闭套接字连接
            /// </summary>
            protected override void dispose()
            {
                base.dispose();
                ClientUserInfo = null;
                TmphInterlocked.NoCheckCompareSetSleep0(ref tcpStreamReceiveLock);
                try
                {
                    if (tcpStreamReceivers != null)
                    {
                        cancelTcpStream();
                        foreach (var tcpStreamReceiver in tcpStreamReceivers)
                        {
                            if (tcpStreamReceiver.ReceiveWait != null)
                            {
                                tcpStreamReceiver.ReceiveWait.Set();
                                tcpStreamReceiver.ReceiveWait.Close();
                            }
                        }
                    }
                }
                finally
                {
                    tcpStreamReceiveLock = 0;
                }
                TmphInterlocked.NoCheckCompareSetSleep0(ref outputLock);
                outputs.Clear();
                outputLock = 0;
            }

            /// <summary>
            ///     取消TCP流
            /// </summary>
            private void cancelTcpStream()
            {
                while (tcpStreamReceiveIndex != 0) tcpStreamReceivers[--tcpStreamReceiveIndex].Cancel(true);
                freeTcpStreamIndexs.Empty();
            }

            /// <summary>
            ///     关闭套接字
            /// </summary>
            protected override void close()
            {
                if (Socket != null)
                {
                    try
                    {
                        TmphInterlocked.NoCheckCompareSetSleep0(ref tcpStreamReceiveLock);
                        try
                        {
                            cancelTcpStream();
                        }
                        finally
                        {
                            tcpStreamReceiveLock = 0;
                        }
                        Socket.Send(closeData, 0, closeData.Length, SocketFlags.None, out socketError);
                    }
                    catch
                    {
                    }
                    finally
                    {
                        base.close();
                    }
                }
            }

            /// <summary>
            ///     关闭套接字
            /// </summary>
            internal void Close()
            {
                close();
                TmphInterlocked.CompareSetSleep1(ref isOutputBuilding);
                isOutputBuilding = 0;
            }

            /// <summary>
            ///     TCP套接字添加到池
            /// </summary>
            internal override void PushPool()
            {
                commandSocketProxy.pushSocket(this);
            }

            /// <summary>
            ///     负载均衡联通测试
            /// </summary>
            /// <param name="identity">负载均衡联通测试标识</param>
            /// <returns>是否成功</returns>
            internal bool LoadBalancingCheck(int identity)
            {
                if (Socket != null && identity == LoadBalancingCheckIdentity)
                {
                    try
                    {
                        var output = TmphOutputParameter.Get(new TmphStreamIdentity { Index = 1 },
                            new TmphAsynchronousMethod.TmphReturnValue { IsReturn = true });
                        if (output != null) return pushOutput(output);
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                }
                return false;
            }

            /// <summary>
            ///     获取TCP调用客户端套接字类型
            /// </summary>
            internal void VerifySocketType()
            {
                var verifySeconds = commandSocketProxy.attribute.VerifySeconds;
                if (verifySeconds > 0) verifyTimeout = TmphDate.NowSecond.AddSeconds(verifySeconds + 1);
                else
                {
                    verifyTimeout = DateTime.MaxValue;
                    verifySeconds = TmphTcpCommand.Default.DefaultTimeout;
                }
                if ((verifySeconds *= 1000) <= 0) verifySeconds = int.MaxValue;
                Socket.ReceiveTimeout = Socket.SendTimeout = verifySeconds;
                receive(onSocketTypeHandle, 0, sizeof(int), verifyTimeout);
            }

            /// <summary>
            ///     获取TCP调用客户端套接字类型
            /// </summary>
            /// <param name="isSocket">是否成功</param>
            private void onSocketType(bool isSocket)
            {
                if (isSocket)
                {
                    fixed (byte* receiveDataFixed = receiveData)
                    {
                        if (*(int*)receiveDataFixed ==
                            (commandSocketProxy.attribute.IsIdentityCommand ? IdentityVerifyIdentity : VerifyIdentity))
                        {
                            if (commandSocketProxy.isClientUserInfo) ClientUserInfo = new TmphTcpBase.TmphClient();
                            if (commandSocketProxy.verify == null)
                            {
                                verifyMethod();
                                return;
                            }
                            try
                            {
                                if (commandSocketProxy.verify.Verify(this))
                                {
                                    verifyMethod();
                                    return;
                                }
                                TmphLog.Default.Add("TCP调用客户端验证失败 " + Socket.RemoteEndPoint, false, false);
                            }
                            catch (Exception error)
                            {
                                TmphLog.Error.Add(error, "TCP调用客户端验证失败 " + Socket.RemoteEndPoint, false);
                            }
                        }
                        else if (commandSocketProxy.attribute.IsHttpClient &&
                                 TmphHttp.GetMethod(receiveDataFixed) != TmphHttp.TmphMethodType.None)
                        {
                            Http.TmphSocket.Start(commandSocketProxy.HttpServers, this);
                            return;
                        }
                        else if (*(int*)receiveDataFixed ==
                                 (commandSocketProxy.attribute.IsIdentityCommand
                                     ? VerifyIdentity
                                     : IdentityVerifyIdentity))
                        {
                            TmphLog.Error.Add("TCP调用客户端命令模式不匹配" + Socket.RemoteEndPoint, false, false);
                        }
                    }
                }
                commandSocketProxy.pushSocket(this);
            }

            /// <summary>
            ///     异步套接字方法验证
            /// </summary>
            private void verifyMethod()
            {
                if (commandSocketProxy.identityOnCommands == null
                    ? commandSocketProxy.verifyCommand.Length == 0
                    : commandSocketProxy.verifyCommandIdentity == nullVerifyCommandIdentity)
                    IsVerifyMethod = true;
                fixed (byte* dataFixed = sendData)
                    *(int*)dataFixed = commandSocketProxy.attribute.IsIdentityCommand
                        ? IdentityVerifyIdentity
                        : VerifyIdentity;
                if (send(sendData, 0, sizeof(int)))
                {
                    if (commandSocketProxy.attribute.IsServerAsynchronousReceive)
                    {
                        if (onReceiveStreamCommandLengthHandle == null)
                        {
                            if (commandSocketProxy.identityOnCommands == null)
                            {
                                onReceiveStreamCommandLengthHandle = onReceiveStreamCommandLength;
                                onReceiveStreamCommandHandle = onReceiveStreamCommand;
                            }
                            else onReceiveStreamCommandLengthHandle = onReceiveStreamIdentityCommand;
                            doStreamCommandHandle = doStreamCommand;
                        }
                        receiveEndIndex = receiveStartIndex = 0;
                        receiveStreamCommand();
                    }
                    else
                    {
                        if (receiveCommandHandle == null) receiveCommandHandle = receiveCommand;
                        TmphThreadPool.TinyPool.FastStart(receiveCommandHandle, null, null);
                    }
                    return;
                }
                commandSocketProxy.pushSocket(this);
            }

            /// <summary>
            ///     同步接收命令
            /// </summary>
            private void receiveCommand()
            {
                try
                {
                    receiveStartIndex = 0;
                    fixed (byte* receiveDataFixed = receiveData)
                    {
                        this.receiveDataFixed = receiveDataFixed;
                        if (commandSocketProxy.identityOnCommands == null) receiveDataCommand();
                        else receiveIdentityCommand();
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                finally
                {
                    commandSocketProxy.pushSocket(this);
                }
            }

            /// <summary>
            ///     接收命令
            /// </summary>
            private void receiveIdentityCommand()
            {
                var commands = commandSocketProxy.identityOnCommands;
                if (commandSocketProxy.verifyCommandIdentity == nullVerifyCommandIdentity)
                {
                    receiveEndIndex = 0;
                    IsVerifyMethod = true;
                }
                else
                {
                    if ((receiveEndIndex = tryReceive(0, sizeof(int) * 2 + sizeof(TmphStreamIdentity), verifyTimeout)) >=
                        sizeof(int) * 2 + sizeof(TmphStreamIdentity))
                    {
                        if (*(int*)receiveDataFixed == commandSocketProxy.verifyCommandIdentity)
                        {
                            command = commands[commandSocketProxy.verifyCommandIdentity];
                            identity = *(TmphStreamIdentity*)(receiveDataFixed + sizeof(int));
                            receiveStartIndex = sizeof(int) * 2 + sizeof(TmphStreamIdentity);
                            doCommand(*(int*)(receiveDataFixed + (sizeof(int) + sizeof(TmphStreamIdentity))));
                        }
                        else
                            TmphLog.Error.Add(null,
                                "TCP验证函数命令匹配失败 " + (*(int*)receiveDataFixed).toString() + "<>" +
                                commandSocketProxy.verifyCommandIdentity.toString(), false);
                    }
                    else
                        TmphLog.Error.Add(null,
                            "TCP验证函数命令数据接受失败 " + receiveEndIndex.toString() + "<" +
                            (sizeof(int) * 2 + sizeof(TmphStreamIdentity)).toString(), false);
                }
                if (IsVerifyMethod)
                {
                    Socket.ReceiveTimeout = commandSocketProxy.receiveCommandTimeout == 0
                        ? -1
                        : commandSocketProxy.receiveCommandTimeout;
                    while (tryReceiveIdentityCommand())
                    {
                        var start = receiveDataFixed + receiveStartIndex;
                        var commandIdentity = *(int*)start;
                        if ((uint)commandIdentity < commands.Length)
                        {
                            command = commands[commandIdentity];
                            identity = *(TmphStreamIdentity*)(start + sizeof(int));
                            receiveStartIndex += sizeof(int) * 2 + sizeof(TmphStreamIdentity);
                            if (doCommand(*(int*)(start + (sizeof(int) + sizeof(TmphStreamIdentity))))) continue;
                        }
                        TmphLog.Default.Add(
                            commandSocketProxy.attribute.ServiceName + " 缺少命令处理委托 [" + commandIdentity.toString() + "]",
                            false, false);
                        break;
                    }
                }
            }

            /// <summary>
            ///     接收命令
            /// </summary>
            /// <returns>是否成功</returns>
            private bool tryReceiveIdentityCommand()
            {
                var receiveLength = receiveEndIndex - receiveStartIndex;
                if (receiveLength >= sizeof(int) * 2 + sizeof(TmphStreamIdentity)) return true;
                if (receiveLength != 0) Buffer.BlockCopy(receiveData, receiveStartIndex, receiveData, 0, receiveLength);
                receiveEndIndex = tryReceive(receiveLength, sizeof(int) * 2 + sizeof(TmphStreamIdentity),
                    TmphDate.NowSecond.AddTicks(commandSocketProxy.receiveCommandTicks));
                if (receiveEndIndex >= sizeof(int) * 2 + sizeof(TmphStreamIdentity))
                {
                    receiveStartIndex = 0;
                    return true;
                }
                return false;
            }

            /// <summary>
            ///     接收命令
            /// </summary>
            private void receiveDataCommand()
            {
                var commands = commandSocketProxy.onCommands;
                if (commandSocketProxy.verifyCommand.Length == 0)
                {
                    receiveEndIndex = 0;
                    IsVerifyMethod = true;
                }
                else
                {
                    receiveStartIndex = commandSocketProxy.verifyCommand.Length +
                                        (sizeof(int) * 2 + sizeof(TmphStreamIdentity));
                    if ((receiveEndIndex = tryReceive(0, receiveStartIndex, verifyTimeout)) >= receiveStartIndex &&
                        *(int*)receiveDataFixed == receiveStartIndex
                        &&
                        commandSocketProxy.verifyCommand.Equals(TmphSubArray<byte>.Unsafe(receiveData, sizeof(int),
                            commandSocketProxy.verifyCommand.Length)))
                    {
                        var start = receiveDataFixed + receiveStartIndex;
                        command = commands.Get(commandSocketProxy.verifyCommand);
                        identity = *(TmphStreamIdentity*)(start - (sizeof(int) + sizeof(TmphStreamIdentity)));
                        doCommand(*(int*)(start - (sizeof(int))));
                    }
                    else TmphLog.Error.Add(null, "TCP验证函数命令匹配失败", false);
                }
                if (IsVerifyMethod)
                {
                    Socket.ReceiveTimeout = commandSocketProxy.receiveCommandTimeout == 0
                        ? -1
                        : commandSocketProxy.receiveCommandTimeout;
                    while (tryReceiveDataCommand())
                    {
                        var start = receiveDataFixed + receiveStartIndex;
                        var commandLength = *(int*)start;
                        if (
                            commands.Get(
                                TmphSubArray<byte>.Unsafe(receiveData, receiveStartIndex + sizeof(int),
                                    commandLength - (sizeof(int) * 2 + sizeof(TmphStreamIdentity))), ref command))
                        {
                            start += commandLength;
                            receiveStartIndex += commandLength;
                            identity = *(TmphStreamIdentity*)(start - (sizeof(int) + sizeof(TmphStreamIdentity)));
                            if (doCommand(*(int*)(start - (sizeof(int))))) continue;
                        }
                        TmphLog.Default.Add(
                            commandSocketProxy.attribute.ServiceName + " 缺少命令处理委托 " +
                            TmphSubArray<byte>.Unsafe(receiveData, receiveStartIndex + sizeof(int),
                                commandLength - (sizeof(int) * 2 + sizeof(TmphStreamIdentity))).GetReverse().DeSerialize(),
                            false, false);
                        break;
                    }
                }
            }

            /// <summary>
            ///     接收命令
            /// </summary>
            /// <returns>是否成功</returns>
            private bool tryReceiveDataCommand()
            {
                var receiveLength = receiveEndIndex - receiveStartIndex;
                if (receiveLength >= sizeof(int) * 3 + sizeof(TmphStreamIdentity))
                {
                    var commandLength = *(int*)(receiveDataFixed + receiveStartIndex);
                    if (receiveLength >= commandLength) return true;
                    Buffer.BlockCopy(receiveData, receiveStartIndex, receiveData, 0, receiveLength);
                    receiveEndIndex = tryReceive(receiveLength, commandLength,
                        TmphDate.NowSecond.AddTicks(commandSocketProxy.receiveCommandTicks));
                    if (receiveEndIndex >= commandLength)
                    {
                        receiveStartIndex = 0;
                        return true;
                    }
                }
                else
                {
                    if (receiveLength != 0)
                        Buffer.BlockCopy(receiveData, receiveStartIndex, receiveData, 0, receiveLength);
                    receiveEndIndex = tryReceive(receiveLength, sizeof(int) * 3 + sizeof(TmphStreamIdentity),
                        TmphDate.NowSecond.AddTicks(commandSocketProxy.receiveCommandTicks));
                    if (receiveEndIndex >= sizeof(int) * 3 + sizeof(TmphStreamIdentity))
                    {
                        var commandLength = *(int*)receiveDataFixed;
                        if (receiveEndIndex >= commandLength)
                        {
                            receiveStartIndex = 0;
                            return true;
                        }
                    }
                }
                return false;
            }

            /// <summary>
            ///     接收命令数据并执行命令
            /// </summary>
            /// <param name="length">数据长度</param>
            private bool doCommand(int length)
            {
                if (length == 0)
                {
                    if (command.MaxDataLength == 0)
                    {
                        command.OnCommand(this, default(TmphSubArray<byte>));
                        return true;
                    }
                }
                else
                {
                    var dataLength = length > 0 ? length : -length;
                    if (dataLength <= command.MaxDataLength)
                    {
                        var receiveLength = receiveEndIndex - receiveStartIndex;
                        if (dataLength <= receiveData.Length)
                        {
                            if (dataLength > receiveLength)
                            {
                                if (receiveLength != 0)
                                    Buffer.BlockCopy(receiveData, receiveStartIndex, receiveData, 0, receiveLength);
                                receiveEndIndex = tryReceive(receiveLength, dataLength,
                                    commandSocketProxy.getReceiveTimeout(dataLength));
                                if (receiveEndIndex < dataLength) return false;
                                receiveStartIndex = 0;
                            }
                            if (length >= 0)
                            {
                                var data = TmphSubArray<byte>.Unsafe(receiveData, receiveStartIndex, dataLength);
                                receiveStartIndex += dataLength;
                                command.OnCommand(this, data);
                            }
                            else
                            {
                                var data = TmphStream.Deflate.GetDeCompressUnsafe(receiveData, receiveStartIndex,
                                    dataLength, TmphMemoryPool.StreamBuffers);
                                receiveStartIndex += dataLength;
                                command.OnCommand(this, data);
                                TmphMemoryPool.StreamBuffers.Push(ref data.array);
                            }
                            return true;
                        }
                        var TmphBuffer = BigBuffers.Get(dataLength);
                        if (receiveLength != 0)
                            Buffer.BlockCopy(receiveData, receiveStartIndex, TmphBuffer, 0, receiveLength);
                        if (receive(TmphBuffer, receiveLength, dataLength, commandSocketProxy.getReceiveTimeout(dataLength)))
                        {
                            if (length >= 0) command.OnCommand(this, TmphSubArray<byte>.Unsafe(TmphBuffer, 0, dataLength));
                            else
                            {
                                var data = TmphStream.Deflate.GetDeCompressUnsafe(TmphBuffer, 0, dataLength,
                                    TmphMemoryPool.StreamBuffers);
                                command.OnCommand(this, data);
                                TmphMemoryPool.StreamBuffers.Push(ref data.array);
                            }
                            receiveStartIndex = receiveEndIndex = 0;
                            BigBuffers.Push(ref TmphBuffer);
                            return true;
                        }
                        BigBuffers.Push(ref TmphBuffer);
                    }
                    else
                    {
                        TmphLog.Default.Add(
                            "接收数据长度超限 " + (length > 0 ? length : -length).toString() + " > " +
                            command.MaxDataLength.toString(), false, false);
                    }
                }
                return false;
            }

            /// <summary>
            ///     接收命令
            /// </summary>
            private void receiveStreamCommand()
            {
                //if (IsVerifyMethod)
                //{
                //    if (Socket == null)
                //    {
                //        commandSocketProxy.pushSocket(this);
                //        return;
                //    }
                //    Socket.ReceiveTimeout = commandSocketProxy.receiveCommandTimeout == 0 ? -1 : commandSocketProxy.receiveCommandTimeout;
                //}
                try
                {
                    NEXT:
                    var receiveLength = receiveEndIndex - receiveStartIndex;
                    if (commandSocketProxy.identityOnCommands == null)
                    {
                        if (receiveLength >= sizeof(int) * 3 + sizeof(TmphStreamIdentity))
                        {
                            if (receiveStreamCommandLength()) goto NEXT;
                        }
                        else
                        {
                            if (receiveLength != 0)
                                Buffer.BlockCopy(receiveData, receiveStartIndex, receiveData, 0, receiveLength);
                            tryReceive(onReceiveStreamCommandLengthHandle, receiveLength,
                                (sizeof(int) * 3 + sizeof(TmphStreamIdentity)) - receiveLength,
                                IsVerifyMethod
                                    ? TmphDate.NowSecond.AddTicks(commandSocketProxy.receiveCommandTicks)
                                    : verifyTimeout);
                        }
                    }
                    else if (receiveLength >= sizeof(int) * 2 + sizeof(TmphStreamIdentity))
                    {
                        if (receiveStreamIdentityCommand()) goto NEXT;
                    }
                    else
                    {
                        if (receiveLength != 0)
                            Buffer.BlockCopy(receiveData, receiveStartIndex, receiveData, 0, receiveLength);
                        tryReceive(onReceiveStreamCommandLengthHandle, receiveLength,
                            (sizeof(int) * 2 + sizeof(TmphStreamIdentity)) - receiveLength,
                            IsVerifyMethod
                                ? TmphDate.NowSecond.AddTicks(commandSocketProxy.receiveCommandTicks)
                                : verifyTimeout);
                    }
                    return;
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                commandSocketProxy.pushSocket(this);
            }

            /// <summary>
            ///     接收命令长度处理
            /// </summary>
            /// <param name="receiveEndIndex">接收数据结束位置</param>
            private void onReceiveStreamCommandLength(int receiveEndIndex)
            {
                try
                {
                    //if (isOutputDebug) DebugLog.Add(commandSocketProxy.attribute.ServiceName + ".onReceiveStreamCommandLength(" + receiveEndIndex.toString() + ")", false, false);
                    if (receiveEndIndex >= sizeof(int) * 3 + sizeof(TmphStreamIdentity))
                    {
                        this.receiveEndIndex = receiveEndIndex;
                        receiveStartIndex = 0;
                        if (receiveStreamCommandLength()) receiveStreamCommand();
                        return;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                commandSocketProxy.pushSocket(this);
            }

            /// <summary>
            ///     接收命令长度处理
            /// </summary>
            /// <returns>是否继续处理下一个命令</returns>
            private bool receiveStreamCommandLength()
            {
                fixed (byte* receiveDataFixed = receiveData)
                {
                    var commandLength = *(int*)(receiveDataFixed + receiveStartIndex);
                    if ((uint)commandLength <= commandSocketProxy.maxCommandLength)
                    {
                        var receiveLength = receiveEndIndex - receiveStartIndex;
                        if (receiveLength >= commandLength)
                        {
                            this.receiveDataFixed = receiveDataFixed;
                            return getStreamCommand();
                        }
                        if (receiveLength != 0)
                            Unsafe.TmphMemory.Copy(receiveDataFixed + receiveStartIndex, receiveDataFixed, receiveLength);
                        tryReceive(onReceiveStreamCommandHandle, receiveLength, commandLength,
                            IsVerifyMethod
                                ? TmphDate.NowSecond.AddTicks(commandSocketProxy.receiveCommandTicks)
                                : verifyTimeout);
                        return false;
                    }
                }
                commandSocketProxy.pushSocket(this);
                return false;
            }

            /// <summary>
            ///     接收命令处理
            /// </summary>
            /// <param name="receiveEndIndex">接收数据结束位置</param>
            private void onReceiveStreamCommand(int receiveEndIndex)
            {
                try
                {
                    //if (isOutputDebug) DebugLog.Add(commandSocketProxy.attribute.ServiceName + ".onReceiveStreamCommand(" + receiveEndIndex.toString() + ")", false, false);
                    fixed (byte* receiveDataFixed = receiveData)
                    {
                        if (receiveEndIndex >= *(int*)receiveDataFixed)
                        {
                            this.receiveEndIndex = receiveEndIndex;
                            this.receiveDataFixed = receiveDataFixed;
                            receiveStartIndex = 0;
                            if (getStreamCommand()) receiveStreamCommand();
                            return;
                        }
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                commandSocketProxy.pushSocket(this);
            }

            /// <summary>
            ///     获取命令委托
            /// </summary>
            /// <returns>是否继续处理下一个命令</returns>
            private bool getStreamCommand()
            {
                var commandLength = *(int*)(receiveDataFixed + receiveStartIndex);
                var commandData = TmphSubArray<byte>.Unsafe(receiveData, receiveStartIndex + sizeof(int),
                    receiveStartIndex + commandLength - (sizeof(int) * 2 + sizeof(TmphStreamIdentity)));
                if (IsVerifyMethod)
                {
                    if (commandSocketProxy.onCommands.Get(commandData, ref command))
                    {
                        receiveStartIndex += commandLength;
                        return getStreamIdentity();
                    }
                    TmphLog.Default.Add(
                        commandSocketProxy.attribute.ServiceName + " 缺少命令处理委托 " + commandData.GetReverse().DeSerialize(),
                        true, false);
                }
                else if (commandSocketProxy.verifyCommand.Equals(commandData))
                {
                    command = commandSocketProxy.onCommands.Get(commandSocketProxy.verifyCommand);
                    receiveStartIndex += commandLength;
                    return getStreamIdentity();
                }
                commandSocketProxy.pushSocket(this);
                return false;
            }

            /// <summary>
            ///     获取会话标识
            /// </summary>
            /// <returns>是否继续处理下一个命令</returns>
            private bool getStreamIdentity()
            {
                var start = receiveDataFixed + receiveStartIndex;
                var length = *(int*)(start - sizeof(int));
                if (length == 0)
                {
                    if (command.MaxDataLength == 0)
                    {
                        identity = *(TmphStreamIdentity*)(start - (sizeof(int) + sizeof(TmphStreamIdentity)));
                        command.OnCommand(this, default(TmphSubArray<byte>));
                        //if ((receiveEndIndex -= receiveStartIndex) != 0) Unsafe.TmphMemory.Copy(receiveDataFixed + receiveStartIndex, receiveDataFixed, receiveEndIndex);
                        return true;
                    }
                }
                else
                {
                    var dataLength = length > 0 ? length : -length;
                    if (dataLength <= command.MaxDataLength)
                    {
                        var receiveLength = receiveEndIndex - receiveStartIndex;
                        identity = *(TmphStreamIdentity*)(start - (sizeof(int) + sizeof(TmphStreamIdentity)));
                        if (dataLength <= receiveLength)
                        {
                            if (length >= 0)
                            {
                                var data = TmphSubArray<byte>.Unsafe(receiveData, receiveStartIndex, dataLength);
                                receiveStartIndex += dataLength;
                                command.OnCommand(this, data);
                                return true;
                            }
                            else
                            {
                                var data = TmphStream.Deflate.GetDeCompressUnsafe(receiveData, receiveStartIndex,
                                    dataLength, TmphMemoryPool.StreamBuffers);
                                receiveStartIndex += dataLength;
                                return
                                    isDoStreamCommand(new TmphMemoryPool.TmphPushSubArray
                                    {
                                        Value = data,
                                        PushPool = TmphMemoryPool.StreamBuffers.PushHandle
                                    });
                            }
                        }
                        receiveStream(doStreamCommandHandle, receiveDataFixed, length,
                            IsVerifyMethod ? commandSocketProxy.getReceiveTimeout(dataLength) : verifyTimeout);
                        return false;
                    }
                    TmphLog.Default.Add(
                        "接收数据长度超限 " + (length > 0 ? length : -length).toString() + " > " +
                        command.MaxDataLength.toString(), false, false);
                }
                commandSocketProxy.pushSocket(this);
                return false;
            }

            /// <summary>
            ///     执行命令委托
            /// </summary>
            /// <param name="data">输出数据</param>
            private void doStreamCommand(TmphMemoryPool.TmphPushSubArray data)
            {
                try
                {
                    if (isDoStreamCommand(data)) receiveStreamCommand();
                    return;
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                commandSocketProxy.pushSocket(this);
            }

            /// <summary>
            ///     执行命令委托
            /// </summary>
            /// <param name="data">输出数据</param>
            /// <returns>是否继续处理下一个命令</returns>
            private bool isDoStreamCommand(TmphMemoryPool.TmphPushSubArray data)
            {
                var TmphBuffer = data.Value.Array;
                if (TmphBuffer != null)
                {
                    command.OnCommand(this, data.Value);
                    data.Push();
                    return true;
                }
                commandSocketProxy.pushSocket(this);
                return false;
            }

            /// <summary>
            ///     接收命令处理
            /// </summary>
            /// <param name="receiveEndIndex">接收数据结束位置</param>
            private void onReceiveStreamIdentityCommand(int receiveEndIndex)
            {
                try
                {
                    //if (isOutputDebug) DebugLog.Add(commandSocketProxy.attribute.ServiceName + ".onReceiveStreamIdentityCommand(" + receiveEndIndex.toString() + ")", false, false);
                    if (receiveEndIndex >= sizeof(int) * 2 + sizeof(TmphStreamIdentity))
                    {
                        this.receiveEndIndex = receiveEndIndex;
                        receiveStartIndex = 0;
                        if (receiveStreamIdentityCommand()) receiveStreamCommand();
                        return;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                commandSocketProxy.pushSocket(this);
            }

            /// <summary>
            ///     接收命令处理
            /// </summary>
            /// <returns>是否继续处理下一个命令</returns>
            private bool receiveStreamIdentityCommand()
            {
                fixed (byte* receiveDataFixed = receiveData)
                {
                    var command = *(int*)(receiveDataFixed + receiveStartIndex);
                    if (IsVerifyMethod)
                    {
                        if ((uint)command < commandSocketProxy.identityOnCommands.Length)
                        {
                            this.command = commandSocketProxy.identityOnCommands[command];
                            if (this.command.OnCommand != null)
                            {
                                this.receiveDataFixed = receiveDataFixed;
                                receiveStartIndex += sizeof(int) * 2 + sizeof(TmphStreamIdentity);
                                return getStreamIdentity();
                            }
                        }
                        TmphLog.Default.Add(
                            commandSocketProxy.attribute.ServiceName + " 缺少命令处理委托 [" + command.toString() + "]", true,
                            false);
                    }
                    else if (command == commandSocketProxy.verifyCommandIdentity)
                    {
                        this.receiveDataFixed = receiveDataFixed;
                        this.command = commandSocketProxy.identityOnCommands[command];
                        receiveStartIndex += sizeof(int) * 2 + sizeof(TmphStreamIdentity);
                        return getStreamIdentity();
                    }
                }
                commandSocketProxy.pushSocket(this);
                return false;
            }

            /// <summary>
            ///     读取数据
            /// </summary>
            /// <param name="onReceive">接收数据处理委托</param>
            /// <param name="dataFixed">接收数据起始位置</param>
            /// <param name="length">数据长度</param>
            /// <param name="timeout">接收超时</param>
            private void receiveStream(Action<TmphMemoryPool.TmphPushSubArray> onReceive, byte* dataFixed, int length,
                DateTime timeout)
            {
                var TmphStreamReceiver = TmphTypePool<TmphStreamReceiver>.Pop();
                if (TmphStreamReceiver == null)
                {
                    try
                    {
                        TmphStreamReceiver = new TmphStreamReceiver();
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                    if (TmphStreamReceiver == null)
                    {
                        onReceive(default(TmphMemoryPool.TmphPushSubArray));
                        return;
                    }
                }
                TmphStreamReceiver.Callback = onReceive;
                TmphStreamReceiver.Socket = this;
                TmphStreamReceiver.Receive(dataFixed, length, timeout);
            }

            /// <summary>
            ///     添加输出信息
            /// </summary>
            /// <param name="output">当前输出信息</param>
            /// <returns>是否成功加入输出队列</returns>
            private bool pushOutput(TmphOutput output)
            {
                if (Socket != null)
                {
                    TmphInterlocked.NoCheckCompareSetSleep0(ref outputLock);
                    var isBuildOutput = this.isBuildOutput;
                    outputs.Push(output);
                    this.isBuildOutput = 1;
                    outputLock = 0;
                    if (isBuildOutput == 0) TmphThreadPool.TinyPool.FastStart(buildOutputHandle, null, null);
                    return true;
                }
                return false;
            }

            /// <summary>
            ///     创建输出数据并执行
            /// </summary>
            private void buildOutput()
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref isOutputBuilding);
                int bufferSize = BigBuffers.Size, bufferSize2 = bufferSize >> 1;
                using (var outputStream = new TmphUnmanagedStream((byte*)&bufferSize, sizeof(int)))
                {
                    var outputBuilder = new TmphOutputBuilder { Socket = this, OutputStream = outputStream };
                    try
                    {
                        START:
                        var TmphBuffer = sendData;
                        fixed (byte* dataFixed = TmphBuffer)
                        {
                            outputBuilder.Reset(dataFixed, TmphBuffer.Length);
                            do
                            {
                                TmphInterlocked.NoCheckCompareSetSleep0(ref outputLock);
                                var output = outputs.Pop();
                                if (output == null)
                                {
                                    if (outputStream.Length == 0)
                                    {
                                        isBuildOutput = 0;
                                        outputLock = 0;
                                        isOutputBuilding = 0;
                                        return;
                                    }
                                    outputLock = 0;
                                    outputBuilder.Send();
                                    if (sendData != TmphBuffer) goto START;
                                }
                                else
                                {
                                    outputLock = 0;
                                    outputBuilder.Build(output);
                                    if (outputStream.Length + outputBuilder.MaxOutputLength > bufferSize)
                                    {
                                        outputBuilder.Send();
                                        if (sendData != TmphBuffer) goto START;
                                    }
                                    if (outputs.Head == null && outputStream.Length <= bufferSize2) Thread.Sleep(0);
                                }
                            } while (true);
                        }
                    }
                    catch (Exception error)
                    {
                        TmphInterlocked.NoCheckCompareSetSleep0(ref outputLock);
                        isBuildOutput = 0;
                        outputLock = 0;
                        isOutputBuilding = 0;
                        Socket.shutdown();
                        TmphLog.Error.Add(error, commandSocketProxy.attribute.ServiceName, false);
                    }
                }
            }

            /// <summary>
            ///     发送数据
            /// </summary>
            /// <param name="identity">会话标识</param>
            /// <param name="value">返回值</param>
            /// <returns>是否成功加入输出队列</returns>
            public bool SendStream(TmphStreamIdentity identity, TmphAsynchronousMethod.TmphReturnValue value)
            {
                var output = TmphOutputParameter.Get(identity, value);
                if (output != null) return pushOutput(output);
                close();
                return false;
            }

            /// <summary>
            ///     发送数据
            /// </summary>
            /// <typeparam name="TOutputParameterType">输出数据类型</typeparam>
            /// <param name="identity">会话标识</param>
            /// <param name="outputParameter">返回值</param>
            /// <returns>是否成功加入输出队列</returns>
            public bool SendStream<TOutputParameterType>(TmphStreamIdentity identity,
                TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType> outputParameter)
            {
                if (outputParameter.IsReturn)
                {
                    var output = TmphOutputParameter<TOutputParameterType>.Get(identity, outputParameter.Value);
                    if (output != null) return pushOutput(output);
                    close();
                    return false;
                }
                return SendStream(identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
            }

            /// <summary>
            ///     发送数据
            /// </summary>
            /// <typeparam name="TOutputParameterType">输出数据类型</typeparam>
            /// <param name="identity">会话标识</param>
            /// <param name="outputParameter">返回值</param>
            /// <returns>是否成功加入输出队列</returns>
            public bool SendStreamJson<TOutputParameterType>(TmphStreamIdentity identity,
                TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType> outputParameter)
            {
                return SendStream(identity, TmphTcpBase.JsonToSerialize(outputParameter));
            }

            /// <summary>
            ///     获取TCP流读取器索引
            /// </summary>
            /// <param name="TmphTcpStreamAsyncResult">TCP流异步操作状态</param>
            /// <returns>TCP流读取器索引</returns>
            private int getTcpStreamIndex(TmphTcpStreamAsyncResult TmphTcpStreamAsyncResult)
            {
                var index = -1;
                TmphInterlocked.NoCheckCompareSetSleep0(ref tcpStreamReceiveLock);
                try
                {
                    if (freeTcpStreamIndexs.Count == 0)
                    {
                        if (tcpStreamReceivers == null)
                        {
                            tcpStreamReceivers = new TmphTcpStreamReceiver[4];
                            index = 0;
                            tcpStreamReceiveIndex = 1;
                        }
                        else
                        {
                            if (tcpStreamReceiveIndex == tcpStreamReceivers.Length)
                            {
                                var newTcpStreamReceivers = new TmphTcpStreamReceiver[tcpStreamReceiveIndex << 1];
                                tcpStreamReceivers.CopyTo(newTcpStreamReceivers, 0);
                                tcpStreamReceivers = newTcpStreamReceivers;
                            }
                            index = tcpStreamReceiveIndex++;
                        }
                    }
                    else index = freeTcpStreamIndexs.UnsafePop();
                    tcpStreamReceivers[index].SetAsyncResult(TmphTcpStreamAsyncResult);
                }
                finally
                {
                    tcpStreamReceiveLock = 0;
                }
                return index;
            }

            /// <summary>
            ///     取消TCP流读取
            /// </summary>
            /// <param name="indexIdentity">TCP流读取器索引+当前处理序号</param>
            private void cancelTcpStream(long indexIdentity)
            {
                cancelTcpStreamIndex((int)(indexIdentity >> 32), (int)indexIdentity, true);
            }

            /// <summary>
            ///     取消TCP流读取
            /// </summary>
            /// <param name="index">TCP流读取器索引</param>
            /// <param name="identity">当前处理序号</param>
            /// <param name="isSetWait">是否设置结束状态</param>
            private void cancelTcpStreamIndex(int index, int identity, bool isSetWait)
            {
                if (tcpStreamReceivers[index].Identity == identity)
                {
                    TmphInterlocked.NoCheckCompareSetSleep0(ref tcpStreamReceiveLock);
                    try
                    {
                        if (tcpStreamReceivers[index].Cancel(identity, isSetWait)) freeTcpStreamIndexs.Add(index);
                    }
                    finally
                    {
                        tcpStreamReceiveLock = 0;
                    }
                }
            }

            /// <summary>
            ///     等待TCP流读取
            /// </summary>
            /// <param name="index">TCP流读取器索引</param>
            /// <param name="identity">当前处理序号</param>
            /// <returns>读取的数据</returns>
            private TmphTcpStreamParameter waitTcpStream(int index, int identity)
            {
                setTcpStreamTimeout(index, identity);
                var receiveWait = tcpStreamReceivers[index].ReceiveWait;
                if (receiveWait.WaitOne())
                {
                    var parameter = TmphTcpStreamParameter.Null;
                    TmphInterlocked.NoCheckCompareSetSleep0(ref tcpStreamReceiveLock);
                    try
                    {
                        if (tcpStreamReceivers[index].Get(identity, ref parameter)) freeTcpStreamIndexs.Add(index);
                    }
                    finally
                    {
                        tcpStreamReceiveLock = 0;
                    }
                    return parameter;
                }
                cancelTcpStreamIndex(index, identity, false);
                return TmphTcpStreamParameter.Null;
            }

            /// <summary>
            ///     设置TCP流读取超时
            /// </summary>
            /// <param name="index">TCP流读取器索引</param>
            /// <param name="identity">当前处理序号</param>
            private void setTcpStreamTimeout(int index, int identity)
            {
                if (cancelTcpStreamHandle == null) cancelTcpStreamHandle = cancelTcpStream;
                TmphTimerTask.Default.Add(cancelTcpStreamHandle, ((long)index << 32) + identity,
                    TmphDate.NowSecond.AddSeconds(TmphTcpCommand.Default.TcpStreamTimeout), null);
            }

            /// <summary>
            ///     TCP流回馈
            /// </summary>
            /// <param name="parameter">TCP流参数</param>
            internal void OnTcpStream(TmphTcpStreamParameter parameter)
            {
                TmphTcpStreamAsyncResult asyncResult = null;
                var index = parameter.Index;
                TmphInterlocked.NoCheckCompareSetSleep0(ref tcpStreamReceiveLock);
                try
                {
                    if (tcpStreamReceivers[index].Set(parameter, ref asyncResult) && asyncResult != null)
                        freeTcpStreamIndexs.Add(index);
                }
                finally
                {
                    tcpStreamReceiveLock = 0;
                    if (asyncResult != null) asyncResult.OnCallback(parameter);
                }
            }

            /// <summary>
            ///     获取TCP参数流
            /// </summary>
            /// <param name="stream">TCP参数流</param>
            /// <returns>字节流</returns>
            public Stream GetTcpStream(TmphTcpBase.TmphTcpStream stream)
            {
                return stream.IsStream ? new TmphTcpStream(this, stream) : null;
            }

            /// <summary>
            ///     异步回调
            /// </summary>
            /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
            /// <typeparam name="TReturnType">返回值类型</typeparam>
            public sealed class TmphCallback
            {
                /// <summary>
                ///     异步回调
                /// </summary>
                private readonly Func<TmphAsynchronousMethod.TmphReturnValue, bool> onReturnHandle;

                /// <summary>
                ///     会话标识
                /// </summary>
                private TmphStreamIdentity identity;

                /// <summary>
                ///     套接字重用标识
                /// </summary>
                private int pushIdentity;

                /// <summary>
                ///     异步套接字
                /// </summary>
                private TmphSocket socket;

                /// <summary>
                ///     异步回调
                /// </summary>
                /// <param name="isKeep">是否保持回调</param>
                private TmphCallback(byte isKeep)
                {
                    if (isKeep == 0) onReturnHandle = onReturn;
                }

                /// <summary>
                ///     异步回调
                /// </summary>
                /// <param name="returnValue">返回值</param>
                /// <returns>是否成功加入回调队列</returns>
                private bool onReturn(TmphAsynchronousMethod.TmphReturnValue returnValue)
                {
                    if (this.socket.PushIdentity == pushIdentity)
                    {
                        var socket = this.socket;
                        var identity = this.identity;
                        this.socket = null;
                        TmphTypePool<TmphCallback>.Push(this);
                        return socket.SendStream(identity, returnValue);
                    }
                    this.socket = null;
                    TmphTypePool<TmphCallback>.Push(this);
                    return false;
                }

                /// <summary>
                ///     异步回调
                /// </summary>
                /// <param name="returnValue">返回值</param>
                /// <returns>是否成功加入回调队列</returns>
                private bool onlyCallback(TmphAsynchronousMethod.TmphReturnValue returnValue)
                {
                    return socket.PushIdentity == pushIdentity && socket.SendStream(identity, returnValue);
                }

                /// <summary>
                ///     异步回调
                /// </summary>
                /// <param name="socket">异步套接字</param>
                /// <param name="outputParameter">输出参数</param>
                /// <returns>异步回调</returns>
                public static Func<TmphAsynchronousMethod.TmphReturnValue, bool> Get(TmphSocket socket)
                {
                    var value = TmphTypePool<TmphCallback>.Pop();
                    if (value == null)
                    {
                        try
                        {
                            value = new TmphCallback(0);
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                            socket.SendStream(socket.identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
                            return null;
                        }
                    }
                    value.socket = socket;
                    value.identity = socket.identity;
                    value.pushIdentity = socket.PushIdentity;
                    return value.onReturnHandle;
                }

                /// <summary>
                ///     异步回调
                /// </summary>
                /// <param name="socket">异步套接字</param>
                /// <param name="outputParameter">输出参数</param>
                /// <returns>异步回调</returns>
                public static Func<TmphAsynchronousMethod.TmphReturnValue, bool> GetKeep(TmphSocket socket)
                {
                    try
                    {
                        var value = new TmphCallback(1);
                        value.socket = socket;
                        value.identity = socket.identity;
                        value.pushIdentity = socket.PushIdentity;
                        return value.onlyCallback;
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                    socket.SendStream(socket.identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
                    return null;
                }
            }

            /// <summary>
            ///     异步回调
            /// </summary>
            /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
            /// <typeparam name="TReturnType">返回值类型</typeparam>
            public sealed class TmphCallback<TOutputParameterType, TReturnType>
                where TOutputParameterType : TmphAsynchronousMethod.IReturnParameter<TReturnType>
            {
                /// <summary>
                ///     异步回调
                /// </summary>
                private readonly Func<TmphAsynchronousMethod.TmphReturnValue<TReturnType>, bool> onReturnHandle;

                /// <summary>
                ///     会话标识
                /// </summary>
                private TmphStreamIdentity identity;

                /// <summary>
                ///     输出参数
                /// </summary>
                private TOutputParameterType outputParameter;

                /// <summary>
                ///     套接字重用标识
                /// </summary>
                private int pushIdentity;

                /// <summary>
                ///     异步套接字
                /// </summary>
                private TmphSocket socket;

                /// <summary>
                ///     异步回调
                /// </summary>
                /// <param name="isKeep">是否保持回调</param>
                private TmphCallback(byte isKeep)
                {
                    if (isKeep == 0) onReturnHandle = onReturn;
                }

                /// <summary>
                ///     异步回调
                /// </summary>
                /// <param name="returnValue">返回值</param>
                /// <returns>是否成功加入回调队列</returns>
                private bool onReturn(TmphAsynchronousMethod.TmphReturnValue<TReturnType> returnValue)
                {
                    if (this.socket.PushIdentity == pushIdentity)
                    {
                        var outputParameter = new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>
                        {
                            IsReturn = returnValue.IsReturn
                        };
                        if (returnValue.IsReturn)
                        {
                            this.outputParameter.Return = returnValue.Value;
                            outputParameter.Value = this.outputParameter;
                        }
                        var socket = this.socket;
                        var identity = this.identity;
                        this.outputParameter = default(TOutputParameterType);
                        this.socket = null;
                        TmphTypePool<TmphCallback<TOutputParameterType, TReturnType>>.Push(this);
                        return socket.SendStream(identity, outputParameter);
                    }
                    this.outputParameter = default(TOutputParameterType);
                    this.socket = null;
                    TmphTypePool<TmphCallback<TOutputParameterType, TReturnType>>.Push(this);
                    return false;
                }

                /// <summary>
                ///     异步回调
                /// </summary>
                /// <param name="returnValue">返回值</param>
                /// <returns>是否成功加入回调队列</returns>
                private bool onlyCallback(TmphAsynchronousMethod.TmphReturnValue<TReturnType> returnValue)
                {
                    if (socket.PushIdentity == pushIdentity)
                    {
                        var outputParameter = new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>
                        {
                            IsReturn = returnValue.IsReturn
                        };
                        if (returnValue.IsReturn)
                        {
                            this.outputParameter.Return = returnValue.Value;
                            outputParameter.Value = this.outputParameter;
                        }
                        return socket.SendStream(identity, outputParameter);
                    }
                    return false;
                }

                /// <summary>
                ///     异步回调
                /// </summary>
                /// <param name="socket">异步套接字</param>
                /// <param name="outputParameter">输出参数</param>
                /// <returns>异步回调</returns>
                public static Func<TmphAsynchronousMethod.TmphReturnValue<TReturnType>, bool> Get(TmphSocket socket,
                    TOutputParameterType outputParameter)
                {
                    var value = TmphTypePool<TmphCallback<TOutputParameterType, TReturnType>>.Pop();
                    if (value == null)
                    {
                        try
                        {
                            value = new TmphCallback<TOutputParameterType, TReturnType>(0);
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                            socket.SendStream(socket.identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
                            return null;
                        }
                    }
                    value.socket = socket;
                    value.outputParameter = outputParameter;
                    value.identity = socket.identity;
                    value.pushIdentity = socket.PushIdentity;
                    return value.onReturnHandle;
                }

                /// <summary>
                ///     异步回调
                /// </summary>
                /// <param name="socket">异步套接字</param>
                /// <param name="outputParameter">输出参数</param>
                /// <returns>异步回调</returns>
                public static Func<TmphAsynchronousMethod.TmphReturnValue<TReturnType>, bool> GetKeep(TmphSocket socket,
                    TOutputParameterType outputParameter)
                {
                    try
                    {
                        var value = new TmphCallback<TOutputParameterType, TReturnType>(1);
                        value.socket = socket;
                        value.outputParameter = outputParameter;
                        value.identity = socket.identity;
                        value.pushIdentity = socket.PushIdentity;
                        return value.onlyCallback;
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                    socket.SendStream(socket.identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
                    return null;
                }
            }

            /// <summary>
            ///     异步回调
            /// </summary>
            /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
            /// <typeparam name="TReturnType">返回值类型</typeparam>
            public sealed class TmphCallbackJson<TOutputParameterType, TReturnType>
                where TOutputParameterType : TmphAsynchronousMethod.IReturnParameter<TReturnType>
            {
                /// <summary>
                ///     异步回调
                /// </summary>
                private readonly Func<TmphAsynchronousMethod.TmphReturnValue<TReturnType>, bool> onReturnHandle;

                /// <summary>
                ///     会话标识
                /// </summary>
                private TmphStreamIdentity identity;

                /// <summary>
                ///     输出参数
                /// </summary>
                private TOutputParameterType outputParameter;

                /// <summary>
                ///     套接字重用标识
                /// </summary>
                private int pushIdentity;

                /// <summary>
                ///     异步套接字
                /// </summary>
                private TmphSocket socket;

                /// <summary>
                ///     异步回调
                /// </summary>
                /// <param name="isKeep">是否保持回调</param>
                private TmphCallbackJson(byte isKeep)
                {
                    if (isKeep == 0) onReturnHandle = onReturn;
                }

                /// <summary>
                ///     异步回调
                /// </summary>
                /// <param name="returnValue">返回值</param>
                /// <returns>是否成功加入回调队列</returns>
                private bool onReturn(TmphAsynchronousMethod.TmphReturnValue<TReturnType> returnValue)
                {
                    if (this.socket.PushIdentity == pushIdentity)
                    {
                        var outputParameter = new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>
                        {
                            IsReturn = returnValue.IsReturn
                        };
                        if (returnValue.IsReturn)
                        {
                            this.outputParameter.Return = returnValue.Value;
                            outputParameter.Value = this.outputParameter;
                        }
                        var socket = this.socket;
                        var identity = this.identity;
                        this.outputParameter = default(TOutputParameterType);
                        this.socket = null;
                        TmphTypePool<TmphCallbackJson<TOutputParameterType, TReturnType>>.Push(this);
                        return socket.SendStreamJson(identity, outputParameter);
                    }
                    this.outputParameter = default(TOutputParameterType);
                    this.socket = null;
                    TmphTypePool<TmphCallbackJson<TOutputParameterType, TReturnType>>.Push(this);
                    return false;
                }

                /// <summary>
                ///     异步回调
                /// </summary>
                /// <param name="returnValue">返回值</param>
                /// <returns>是否成功加入回调队列</returns>
                private bool onlyCallback(TmphAsynchronousMethod.TmphReturnValue<TReturnType> returnValue)
                {
                    if (socket.PushIdentity == pushIdentity)
                    {
                        var outputParameter = new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>
                        {
                            IsReturn = returnValue.IsReturn
                        };
                        if (returnValue.IsReturn)
                        {
                            this.outputParameter.Return = returnValue.Value;
                            outputParameter.Value = this.outputParameter;
                        }
                        return socket.SendStreamJson(identity, outputParameter);
                    }
                    return false;
                }

                /// <summary>
                ///     异步回调
                /// </summary>
                /// <param name="socket">异步套接字</param>
                /// <param name="outputParameter">输出参数</param>
                /// <returns>异步回调</returns>
                public static Func<TmphAsynchronousMethod.TmphReturnValue<TReturnType>, bool> Get(TmphSocket socket,
                    TOutputParameterType outputParameter)
                {
                    var value = TmphTypePool<TmphCallbackJson<TOutputParameterType, TReturnType>>.Pop();
                    if (value == null)
                    {
                        try
                        {
                            value = new TmphCallbackJson<TOutputParameterType, TReturnType>(0);
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                            socket.SendStream(socket.identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
                            return null;
                        }
                    }
                    value.socket = socket;
                    value.outputParameter = outputParameter;
                    value.identity = socket.identity;
                    value.pushIdentity = socket.PushIdentity;
                    return value.onReturnHandle;
                }

                /// <summary>
                ///     异步回调
                /// </summary>
                /// <param name="socket">异步套接字</param>
                /// <param name="outputParameter">输出参数</param>
                /// <returns>异步回调</returns>
                public static Func<TmphAsynchronousMethod.TmphReturnValue<TReturnType>, bool> GetKeep(TmphSocket socket,
                    TOutputParameterType outputParameter)
                {
                    try
                    {
                        var value = new TmphCallbackJson<TOutputParameterType, TReturnType>(1);
                        value.socket = socket;
                        value.outputParameter = outputParameter;
                        value.identity = socket.identity;
                        value.pushIdentity = socket.PushIdentity;
                        return value.onlyCallback;
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                    socket.SendStream(socket.identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
                    return null;
                }
            }

            /// <summary>
            ///     验证函数异步回调
            /// </summary>
            /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
            public sealed class TmphCallback<TOutputParameterType>
                where TOutputParameterType : TmphAsynchronousMethod.IReturnParameter<bool>
            {
                /// <summary>
                ///     异步回调
                /// </summary>
                private readonly Func<TmphAsynchronousMethod.TmphReturnValue<bool>, bool> onReturnHandle;

                /// <summary>
                ///     会话标识
                /// </summary>
                private TmphStreamIdentity identity;

                /// <summary>
                ///     输出参数
                /// </summary>
                private TOutputParameterType outputParameter;

                /// <summary>
                ///     套接字重用标识
                /// </summary>
                private int pushIdentity;

                /// <summary>
                ///     异步套接字
                /// </summary>
                private TmphSocket socket;

                /// <summary>
                ///     异步回调
                /// </summary>
                private TmphCallback()
                {
                    onReturnHandle = onReturn;
                }

                /// <summary>
                ///     异步回调
                /// </summary>
                /// <param name="returnValue">返回值</param>
                /// <returns>是否成功加入回调队列</returns>
                private bool onReturn(TmphAsynchronousMethod.TmphReturnValue<bool> returnValue)
                {
                    if (this.socket.PushIdentity == pushIdentity)
                    {
                        var outputParameter = new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>();
                        if (returnValue.IsReturn)
                        {
                            try
                            {
                                if (returnValue.Value)
                                {
                                    outputParameter.IsReturn = this.socket.IsVerifyMethod = true;
                                    this.outputParameter.Return = returnValue.Value;
                                    outputParameter.Value = this.outputParameter;
                                }
                            }
                            catch (Exception error)
                            {
                                TmphLog.Error.Add(error, null, true);
                            }
                        }
                        var socket = this.socket;
                        var identity = this.identity;
                        this.outputParameter = default(TOutputParameterType);
                        this.socket = null;
                        TmphTypePool<TmphCallback<TOutputParameterType>>.Push(this);
                        var isReturn = socket.SendStream(identity, outputParameter);
                        if (!this.socket.IsVerifyMethod)
                            TmphLog.Default.Add("TCP调用客户端验证失败 " + this.socket.Socket.RemoteEndPoint, false, false);
                        return isReturn;
                    }
                    this.outputParameter = default(TOutputParameterType);
                    this.socket = null;
                    TmphTypePool<TmphCallback<TOutputParameterType>>.Push(this);
                    return false;
                }

                /// <summary>
                ///     异步回调
                /// </summary>
                /// <param name="socket">异步套接字</param>
                /// <param name="outputParameter">输出参数</param>
                /// <returns>异步回调</returns>
                public static Func<TmphAsynchronousMethod.TmphReturnValue<bool>, bool> Get(TmphSocket socket,
                    TOutputParameterType outputParameter)
                {
                    var value = TmphTypePool<TmphCallback<TOutputParameterType>>.Pop();
                    if (value == null)
                    {
                        try
                        {
                            value = new TmphCallback<TOutputParameterType>();
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                            socket.SendStream(socket.identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
                            return null;
                        }
                    }
                    value.socket = socket;
                    value.outputParameter = outputParameter;
                    value.identity = socket.identity;
                    value.pushIdentity = socket.PushIdentity;
                    return value.onReturnHandle;
                }
            }

            /// <summary>
            ///     验证函数异步回调
            /// </summary>
            /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
            public sealed class TmphCallbackJson<TOutputParameterType>
                where TOutputParameterType : TmphAsynchronousMethod.IReturnParameter<bool>
            {
                /// <summary>
                ///     异步回调
                /// </summary>
                private readonly Func<TmphAsynchronousMethod.TmphReturnValue<bool>, bool> onReturnHandle;

                /// <summary>
                ///     会话标识
                /// </summary>
                private TmphStreamIdentity identity;

                /// <summary>
                ///     输出参数
                /// </summary>
                private TOutputParameterType outputParameter;

                /// <summary>
                ///     套接字重用标识
                /// </summary>
                private int pushIdentity;

                /// <summary>
                ///     异步套接字
                /// </summary>
                private TmphSocket socket;

                /// <summary>
                ///     异步回调
                /// </summary>
                private TmphCallbackJson()
                {
                    onReturnHandle = onReturn;
                }

                /// <summary>
                ///     异步回调
                /// </summary>
                /// <param name="returnValue">返回值</param>
                /// <returns>是否成功加入回调队列</returns>
                private bool onReturn(TmphAsynchronousMethod.TmphReturnValue<bool> returnValue)
                {
                    if (this.socket.PushIdentity == pushIdentity)
                    {
                        var outputParameter = new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>();
                        if (returnValue.IsReturn)
                        {
                            try
                            {
                                if (returnValue.Value)
                                {
                                    outputParameter.IsReturn = this.socket.IsVerifyMethod = true;
                                    this.outputParameter.Return = returnValue.Value;
                                    outputParameter.Value = this.outputParameter;
                                }
                            }
                            catch (Exception error)
                            {
                                TmphLog.Error.Add(error, null, true);
                            }
                        }
                        var socket = this.socket;
                        var identity = this.identity;
                        this.outputParameter = default(TOutputParameterType);
                        this.socket = null;
                        TmphTypePool<TmphCallbackJson<TOutputParameterType>>.Push(this);
                        var isReturn = socket.SendStreamJson(identity, outputParameter);
                        if (!this.socket.IsVerifyMethod)
                            TmphLog.Default.Add("TCP调用客户端验证失败 " + this.socket.Socket.RemoteEndPoint, false, false);
                        return isReturn;
                    }
                    this.outputParameter = default(TOutputParameterType);
                    this.socket = null;
                    TmphTypePool<TmphCallbackJson<TOutputParameterType>>.Push(this);
                    return false;
                }

                /// <summary>
                ///     异步回调
                /// </summary>
                /// <param name="socket">异步套接字</param>
                /// <param name="outputParameter">输出参数</param>
                /// <returns>异步回调</returns>
                public static Func<TmphAsynchronousMethod.TmphReturnValue<bool>, bool> Get(TmphSocket socket,
                    TOutputParameterType outputParameter)
                {
                    var value = TmphTypePool<TmphCallbackJson<TOutputParameterType>>.Pop();
                    if (value == null)
                    {
                        try
                        {
                            value = new TmphCallbackJson<TOutputParameterType>();
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                            socket.SendStream(socket.identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
                            return null;
                        }
                    }
                    value.socket = socket;
                    value.outputParameter = outputParameter;
                    value.identity = socket.identity;
                    value.pushIdentity = socket.PushIdentity;
                    return value.onReturnHandle;
                }
            }

            /// <summary>
            ///     异步回调
            /// </summary>
            /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
            /// <typeparam name="TReturnType">返回值类型</typeparam>
            public sealed class TmphCallbackHttp
            {
                /// <summary>
                ///     异步回调
                /// </summary>
                private readonly Func<TmphAsynchronousMethod.TmphReturnValue, bool> onReturnHandle;

                /// <summary>
                ///     HTTP页面
                /// </summary>
                private TmphTcpBase.TmphHttpPage httpPage;

                /// <summary>
                ///     异步回调
                /// </summary>
                private TmphCallbackHttp()
                {
                    onReturnHandle = onReturn;
                }

                /// <summary>
                ///     异步回调
                /// </summary>
                /// <param name="returnValue">返回值</param>
                /// <returns>是否成功加入回调队列</returns>
                private bool onReturn(TmphAsynchronousMethod.TmphReturnValue returnValue)
                {
                    var httpPage = this.httpPage;
                    this.httpPage = null;
                    var isResponse = false;
                    try
                    {
                        TmphTypePool<TmphCallbackHttp>.Push(this);
                    }
                    finally
                    {
                        if (httpPage.Response(returnValue)) isResponse = true;
                    }
                    return isResponse;
                }

                /// <summary>
                ///     异步回调
                /// </summary>
                /// <param name="httpPage">HTTP页面</param>
                /// <returns>异步回调</returns>
                public static Func<TmphAsynchronousMethod.TmphReturnValue, bool> Get(TmphTcpBase.TmphHttpPage httpPage)
                {
                    var value = TmphTypePool<TmphCallbackHttp>.Pop();
                    if (value == null)
                    {
                        try
                        {
                            value = new TmphCallbackHttp();
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                        }
                        if (value == null)
                        {
                            httpPage.Socket.ResponseError(httpPage.SocketIdentity, TmphResponse.TmphState.ServerError500);
                            return null;
                        }
                    }
                    value.httpPage = httpPage;
                    return value.onReturnHandle;
                }
            }

            /// <summary>
            ///     异步回调
            /// </summary>
            /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
            /// <typeparam name="TReturnType">返回值类型</typeparam>
            public sealed class TmphCallbackHttp<TOutputParameterType, TReturnType>
                where TOutputParameterType : TmphAsynchronousMethod.IReturnParameter<TReturnType>
            {
                /// <summary>
                ///     异步回调
                /// </summary>
                private readonly Func<TmphAsynchronousMethod.TmphReturnValue<TReturnType>, bool> onReturnHandle;

                /// <summary>
                ///     HTTP页面
                /// </summary>
                private TmphTcpBase.TmphHttpPage httpPage;

                /// <summary>
                ///     输出参数
                /// </summary>
                private TOutputParameterType outputParameterValue;

                /// <summary>
                ///     异步回调
                /// </summary>
                private TmphCallbackHttp()
                {
                    onReturnHandle = onReturn;
                }

                /// <summary>
                ///     异步回调
                /// </summary>
                /// <param name="returnValue">返回值</param>
                /// <returns>是否成功加入回调队列</returns>
                private bool onReturn(TmphAsynchronousMethod.TmphReturnValue<TReturnType> returnValue)
                {
                    var outputParameter = new TmphAsynchronousMethod.TmphReturnValue<TOutputParameterType>
                    {
                        IsReturn = returnValue.IsReturn
                    };
                    if (returnValue.IsReturn)
                    {
                        outputParameterValue.Return = returnValue.Value;
                        outputParameter.Value = outputParameterValue;
                    }
                    var httpPage = this.httpPage;
                    outputParameterValue = default(TOutputParameterType);
                    this.httpPage = null;
                    var isResponse = false;
                    try
                    {
                        TmphTypePool<TmphCallbackHttp<TOutputParameterType, TReturnType>>.Push(this);
                    }
                    finally
                    {
                        if (httpPage.Response(outputParameter)) isResponse = true;
                    }
                    return isResponse;
                }

                /// <summary>
                ///     异步回调
                /// </summary>
                /// <param name="httpPage">HTTP页面</param>
                /// <param name="outputParameter">输出参数</param>
                /// <returns>异步回调</returns>
                public static Func<TmphAsynchronousMethod.TmphReturnValue<TReturnType>, bool> Get(
                    TmphTcpBase.TmphHttpPage httpPage, TOutputParameterType outputParameter)
                {
                    var value = TmphTypePool<TmphCallbackHttp<TOutputParameterType, TReturnType>>.Pop();
                    if (value == null)
                    {
                        try
                        {
                            value = new TmphCallbackHttp<TOutputParameterType, TReturnType>();
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                        }
                        if (value == null)
                        {
                            httpPage.Socket.ResponseError(httpPage.SocketIdentity, TmphResponse.TmphState.ServerError500);
                            return null;
                        }
                    }
                    value.httpPage = httpPage;
                    value.outputParameterValue = outputParameter;
                    return value.onReturnHandle;
                }
            }

            /// <summary>
            ///     数据读取器
            /// </summary>
            private sealed class TmphStreamReceiver
            {
                /// <summary>
                ///     读取数据回调操作
                /// </summary>
                private readonly Action<bool> onReadCompressDataHandle;

                /// <summary>
                ///     读取数据回调操作
                /// </summary>
                private readonly Action<bool> onReadDataHandle;

                /// <summary>
                ///     回调委托
                /// </summary>
                public Action<TmphMemoryPool.TmphPushSubArray> Callback;

                /// <summary>
                ///     读取数据是否大缓存
                /// </summary>
                private bool isBigBuffer;

                /// <summary>
                ///     TCP客户端套接字
                /// </summary>
                public TmphSocket Socket;

                /// <summary>
                ///     数据读取器
                /// </summary>
                public TmphStreamReceiver()
                {
                    onReadCompressDataHandle = onReadCompressData;
                    onReadDataHandle = onReadData;
                }

                /// <summary>
                ///     读取数据
                /// </summary>
                /// <param name="dataFixed">接收数据起始位置</param>
                /// <param name="length">数据长度</param>
                /// <param name="timeout">接收超时</param>
                public void Receive(byte* dataFixed, int length, DateTime timeout)
                {
                    isBigBuffer = false;
                    int dataLength = length >= 0 ? length : -length,
                        receiveLength = Socket.receiveEndIndex - Socket.receiveStartIndex;
                    if (dataLength <= Socket.receiveData.Length)
                    {
                        Unsafe.TmphMemory.Copy(dataFixed + Socket.receiveStartIndex, dataFixed, receiveLength);
                        Socket.receiveStartIndex = Socket.receiveEndIndex = 0;
                        if (length >= 0)
                            Socket.receive(onReadDataHandle, receiveLength, dataLength - receiveLength, timeout);
                        else
                            Socket.receive(onReadCompressDataHandle, receiveLength, dataLength - receiveLength, timeout);
                    }
                    else
                    {
                        var data = BigBuffers.Get(dataLength);
                        isBigBuffer = true;
                        Unsafe.TmphMemory.Copy(dataFixed + Socket.receiveStartIndex, data, receiveLength);
                        Socket.receiveStartIndex = Socket.receiveEndIndex = 0;
                        if (length >= 0)
                            Socket.receive(onReadDataHandle, data, receiveLength, dataLength - receiveLength, timeout);
                        else
                            Socket.receive(onReadCompressDataHandle, data, receiveLength, dataLength - receiveLength,
                                timeout);
                    }
                }

                /// <summary>
                ///     读取数据回调操作
                /// </summary>
                /// <param name="isSocket">是否操作成功</param>
                private void onReadData(bool isSocket)
                {
                    var data = Socket.currentReceiveData;
                    Socket.currentReceiveData = Socket.receiveData;
                    if (isSocket)
                    {
                        push(new TmphMemoryPool.TmphPushSubArray
                        {
                            Value = TmphSubArray<byte>.Unsafe(data, 0, Socket.currentReceiveEndIndex),
                            PushPool = isBigBuffer ? BigBuffers.PushHandle : null
                        });
                    }
                    else
                    {
                        try
                        {
                            if (isBigBuffer) BigBuffers.Push(ref data);
                            push(default(TmphMemoryPool.TmphPushSubArray));
                        }
                        finally
                        {
                            Socket.close();
                        }
                    }
                }

                /// <summary>
                ///     读取数据回调操作
                /// </summary>
                /// <param name="isSocket">是否操作成功</param>
                private void onReadCompressData(bool isSocket)
                {
                    var data = Socket.currentReceiveData;
                    Socket.currentReceiveData = Socket.receiveData;
                    if (isSocket)
                    {
                        onReadCompressData(TmphSubArray<byte>.Unsafe(data, 0, Socket.currentReceiveEndIndex));
                    }
                    else
                    {
                        try
                        {
                            if (isBigBuffer) BigBuffers.Push(ref data);
                            push(default(TmphMemoryPool.TmphPushSubArray));
                        }
                        finally
                        {
                            Socket.close();
                        }
                    }
                }

                /// <summary>
                ///     读取数据回调操作
                /// </summary>
                private void onReadCompressData(TmphSubArray<byte> data)
                {
                    try
                    {
                        var newData = TmphStream.Deflate.GetDeCompressUnsafe(data.Array, data.StartIndex, data.Count,
                            TmphMemoryPool.StreamBuffers);
                        push(new TmphMemoryPool.TmphPushSubArray
                        {
                            Value = newData,
                            PushPool = TmphMemoryPool.StreamBuffers.PushHandle
                        });
                        return;
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                    finally
                    {
                        if (isBigBuffer) BigBuffers.Push(ref data.array);
                    }
                    try
                    {
                        push(default(TmphMemoryPool.TmphPushSubArray));
                    }
                    finally
                    {
                        Socket.close();
                    }
                }

                /// <summary>
                ///     添加回调对象
                /// </summary>
                /// <param name="data">输出数据</param>
                private void push(TmphMemoryPool.TmphPushSubArray data)
                {
                    var callback = Callback;
                    Socket = null;
                    Callback = null;
                    try
                    {
                        TmphTypePool<TmphStreamReceiver>.Push(this);
                    }
                    finally
                    {
                        if (callback != null)
                        {
                            try
                            {
                                callback(data);
                            }
                            catch (Exception error)
                            {
                                TmphLog.Error.Add(error, null, false);
                            }
                        }
                    }
                }
            }

            /// <summary>
            ///     输出信息队列集合
            /// </summary>
            private struct TmphOutputQueue
            {
                /// <summary>
                ///     最后一个节点
                /// </summary>
                public TmphOutput End;

                /// <summary>
                ///     第一个节点
                /// </summary>
                public TmphOutput Head;

                /// <summary>
                ///     清除输出信息
                /// </summary>
                public void Clear()
                {
                    Head = End = null;
                }

                /// <summary>
                ///     添加输出信息
                /// </summary>
                /// <param name="output"></param>
                public void Push(TmphOutput output)
                {
                    if (Head == null) Head = End = output;
                    else
                    {
                        End.Next = output;
                        End = output;
                    }
                }

                /// <summary>
                ///     获取输出信息
                /// </summary>
                /// <returns></returns>
                public TmphOutput Pop()
                {
                    if (Head == null) return null;
                    var command = Head;
                    Head = Head.Next;
                    command.Next = null;
                    return command;
                }
            }

            /// <summary>
            ///     输出创建
            /// </summary>
            private struct TmphOutputBuilder
            {
                /// <summary>
                ///     输出流字节长度
                /// </summary>
                private int bufferLength;

                /// <summary>
                ///     输出数据
                /// </summary>
                private TmphSubArray<byte> data;

                /// <summary>
                ///     输出流数据起始位置
                /// </summary>
                private byte* dataFixed;

                /// <summary>
                ///     最大输出长度
                /// </summary>
                public int MaxOutputLength;

                /// <summary>
                ///     输出数据流
                /// </summary>
                public TmphUnmanagedStream OutputStream;

                /// <summary>
                ///     TCP客户端输出流处理套接字
                /// </summary>
                public TmphSocket Socket;

                /// <summary>
                ///     重置输出流
                /// </summary>
                /// <param name="data">输出流数据起始位置</param>
                /// <param name="length">输出流字节长度</param>
                public void Reset(byte* data, int length)
                {
                    OutputStream.Reset(dataFixed = data, bufferLength = length);
                    OutputStream.Unsafer.SetLength(0);
                }

                /// <summary>
                ///     创建输出流
                /// </summary>
                /// <param name="output">输出</param>
                public void Build(TmphOutput output)
                {
                    var streamLength = OutputStream.Length;
                    output.Build(OutputStream);
                    var outputLength = OutputStream.Length - streamLength;
                    if (outputLength > MaxOutputLength) MaxOutputLength = outputLength;
                }

                /// <summary>
                ///     发送数据
                /// </summary>
                public void Send()
                {
                    if (OutputStream.Length <= bufferLength)
                    {
                        data.UnsafeSet(Socket.sendData, 0, OutputStream.Length);
                        if (OutputStream.DataLength != bufferLength)
                        {
                            Unsafe.TmphMemory.Copy(OutputStream.Data, dataFixed, OutputStream.Length);
                            OutputStream.Reset(dataFixed, bufferLength);
                        }
                        OutputStream.Unsafer.SetLength(0);
                    }
                    else
                    {
                        var newOutputBuffer = OutputStream.GetSizeArray(bufferLength << 1);
                        TmphMemoryPool.StreamBuffers.Push(ref Socket.sendData);
                        data.UnsafeSet(Socket.sendData = newOutputBuffer, 0, OutputStream.Length);
                    }
                    MaxOutputLength = 0;
                    //if (Socket.isOutputDebug) DebugLog.Add(Socket.commandSocketProxy.attribute.ServiceName + ".Send(" + data.Length.toString() + ")", false, false);
                    Socket.serverSend(data);
                }
            }

            /// <summary>
            ///     输出信息
            /// </summary>
            private abstract class TmphOutput
            {
                /// <summary>
                ///     会话标识
                /// </summary>
                public TmphStreamIdentity Identity;

                /// <summary>
                ///     下一个输出信息
                /// </summary>
                public TmphOutput Next;

                /// <summary>
                ///     创建输出信息
                /// </summary>
                /// <param name="stream">命令内存流</param>
                public abstract void Build(TmphUnmanagedStream stream);
            }

            /// <summary>
            ///     输出信息
            /// </summary>
            private sealed class TmphOutputParameter : TmphOutput
            {
                /// <summary>
                ///     输出参数
                /// </summary>
                public TmphAsynchronousMethod.TmphReturnValue OutputParameter;

                /// <summary>
                ///     创建输出信息
                /// </summary>
                /// <param name="stream">命令内存流</param>
                public override void Build(TmphUnmanagedStream stream)
                {
                    stream.PrepLength(sizeof(TmphStreamIdentity) + sizeof(int));
                    var dataFixed = stream.CurrentData;
                    *(TmphStreamIdentity*)dataFixed = Identity;
                    *(int*)(dataFixed + sizeof(TmphStreamIdentity)) = OutputParameter.IsReturn
                        ? 0
                        : ErrorStreamReturnLength;
                    stream.Unsafer.AddLength(sizeof(TmphStreamIdentity) + sizeof(int));
                    Next = null;
                    TmphTypePool<TmphOutputParameter>.Push(this);
                }

                /// <summary>
                ///     获取输出信息
                /// </summary>
                /// <param name="identity">会话标识</param>
                /// <param name="outputParameter">输出参数</param>
                /// <returns>输出信息</returns>
                public static TmphOutputParameter Get(TmphStreamIdentity identity,
                    TmphAsynchronousMethod.TmphReturnValue outputParameter)
                {
                    var output = TmphTypePool<TmphOutputParameter>.Pop();
                    if (output == null)
                    {
                        try
                        {
                            output = new TmphOutputParameter();
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                            return null;
                        }
                    }
                    output.Identity = identity;
                    output.OutputParameter = outputParameter;
                    return output;
                }
            }

            /// <summary>
            ///     输出信息
            /// </summary>
            /// <typeparam name="TOutputParameterType">输出数据类型</typeparam>
            private sealed class TmphOutputParameter<TOutputParameterType> : TmphOutput
            {
                /// <summary>
                ///     输出参数
                /// </summary>
                public TOutputParameterType OutputParameter;

                /// <summary>
                ///     创建输出信息
                /// </summary>
                /// <param name="stream">命令内存流</param>
                public override void Build(TmphUnmanagedStream stream)
                {
                    var streamLength = stream.Length;
                    stream.PrepLength(sizeof(TmphStreamIdentity) + sizeof(int));
                    stream.Unsafer.AddLength(sizeof(TmphStreamIdentity) + sizeof(int));
                    TmphDataSerializer.Serialize(OutputParameter, stream);
                    var dataLength = stream.Length - streamLength - (sizeof(TmphStreamIdentity) + sizeof(int));
                    var dataFixed = stream.Data + streamLength;
                    *(TmphStreamIdentity*)dataFixed = Identity;
                    *(int*)(dataFixed + sizeof(TmphStreamIdentity)) = dataLength;
                    OutputParameter = default(TOutputParameterType);
                    Next = null;
                    TmphTypePool<TmphOutputParameter<TOutputParameterType>>.Push(this);
                }

                /// <summary>
                ///     获取输出信息
                /// </summary>
                /// <param name="identity">会话标识</param>
                /// <param name="outputParameter">输出参数</param>
                /// <returns>输出信息</returns>
                public static TmphOutputParameter<TOutputParameterType> Get
                    (TmphStreamIdentity identity, TOutputParameterType outputParameter)
                {
                    var output = TmphTypePool<TmphOutputParameter<TOutputParameterType>>.Pop();
                    if (output == null)
                    {
                        try
                        {
                            output = new TmphOutputParameter<TOutputParameterType>();
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                            return null;
                        }
                    }
                    output.Identity = identity;
                    output.OutputParameter = outputParameter;
                    return output;
                }
            }

            ///// <summary>
            ///// 设置流接收超时
            ///// </summary>
            //private void setStreamReceiveTimeout()
            //{
            //    if (IsVerifyMethod && isStreamReceiveTimeout == 0)
            //    {
            //        isStreamReceiveTimeout = 1;
            //        Socket.ReceiveTimeout = commandSocketProxy.receiveCommandTimeout == 0 ? -1 : commandSocketProxy.receiveCommandTimeout;
            //    }
            //}
            /// <summary>
            ///     TCP参数流
            /// </summary>
            private class TmphTcpStream : Stream, TmphITcpStreamCallback
            {
                /// <summary>
                ///     默认空TCP流异步操作状态
                /// </summary>
                private static readonly TmphTcpStreamAsyncResult nullTcpStreamAsyncResult = new TmphTcpStreamAsyncResult
                {
                    Parameter = new TmphTcpStreamParameter { Data = TmphSubArray<byte>.Unsafe(TmphNullValue<byte>.Array, 0, 0) },
                    IsCompleted = true
                };

                /// <summary>
                ///     TCP调用套接字
                /// </summary>
                private readonly TmphSocket _socket;

                /// <summary>
                ///     套接字重用标识
                /// </summary>
                private readonly int pushIdentity;

                /// <summary>
                ///     TCP参数流
                /// </summary>
                private TmphTcpBase.TmphTcpStream _stream;

                /// <summary>
                ///     是否已经释放资源
                /// </summary>
                private int isDisposed;

                /// <summary>
                ///     TCP参数流
                /// </summary>
                /// <param name="socket">TCP调用套接字</param>
                /// <param name="stream">TCP参数流</param>
                public TmphTcpStream(TmphSocket socket, TmphTcpBase.TmphTcpStream stream)
                {
                    _socket = socket;
                    _stream = stream;
                    pushIdentity = socket.PushIdentity;
                }

                /// <summary>
                ///     否支持读取
                /// </summary>
                public override bool CanRead
                {
                    get { return _stream.CanRead; }
                }

                /// <summary>
                ///     否支持查找
                /// </summary>
                public override bool CanSeek
                {
                    get { return _stream.CanSeek; }
                }

                /// <summary>
                ///     是否可以超时
                /// </summary>
                public override bool CanTimeout
                {
                    get { return _stream.CanTimeout; }
                }

                /// <summary>
                ///     否支持写入
                /// </summary>
                public override bool CanWrite
                {
                    get { return _stream.CanWrite; }
                }

                /// <summary>
                ///     流字节长度
                /// </summary>
                public override long Length
                {
                    get
                    {
                        if (isDisposed != 0) throw objectDisposedException;
                        var parameter = get(new TmphTcpStreamParameter { Command = TmphTcpStreamCommand.GetLength });
                        if (parameter.IsCommand) return parameter.Offset;
                        throw notSupportedException;
                    }
                }

                /// <summary>
                ///     当前位置
                /// </summary>
                public override long Position
                {
                    get
                    {
                        if (isDisposed != 0) throw objectDisposedException;
                        var parameter = get(new TmphTcpStreamParameter { Command = TmphTcpStreamCommand.GetPosition });
                        if (parameter.IsCommand) return parameter.Offset;
                        throw notSupportedException;
                    }
                    set
                    {
                        if (isDisposed != 0) throw objectDisposedException;
                        var parameter =
                            get(new TmphTcpStreamParameter { Command = TmphTcpStreamCommand.SetPosition, Offset = value });
                        if (parameter.IsCommand) return;
                        throw notSupportedException;
                    }
                }

                /// <summary>
                ///     读超时毫秒
                /// </summary>
                public override int ReadTimeout
                {
                    get
                    {
                        if (isDisposed == 0)
                        {
                            var parameter = get(new TmphTcpStreamParameter { Command = TmphTcpStreamCommand.GetReadTimeout });
                            if (parameter.IsCommand) return (int)parameter.Offset;
                        }
                        throw invalidOperationException;
                    }
                    set
                    {
                        if (isDisposed == 0)
                        {
                            var parameter =
                                get(new TmphTcpStreamParameter
                                {
                                    Command = TmphTcpStreamCommand.SetReadTimeout,
                                    Offset = value
                                });
                            if (parameter.IsCommand) return;
                        }
                        throw invalidOperationException;
                    }
                }

                /// <summary>
                ///     写超时毫秒
                /// </summary>
                public override int WriteTimeout
                {
                    get
                    {
                        if (isDisposed == 0)
                        {
                            var parameter = get(new TmphTcpStreamParameter { Command = TmphTcpStreamCommand.GetWriteTimeout });
                            if (parameter.IsCommand) return (int)parameter.Offset;
                        }
                        throw invalidOperationException;
                    }
                    set
                    {
                        if (isDisposed == 0)
                        {
                            var parameter =
                                get(new TmphTcpStreamParameter
                                {
                                    Command = TmphTcpStreamCommand.SetWriteTimeout,
                                    Offset = value
                                });
                            if (parameter.IsCommand) return;
                        }
                        throw invalidOperationException;
                    }
                }

                /// <summary>
                ///     TCP流异步回调
                /// </summary>
                /// <param name="TmphTcpStreamAsyncResult">TCP流异步操作状态</param>
                /// <param name="parameter">TCP流参数</param>
                public void Callback(TmphTcpStreamAsyncResult TmphTcpStreamAsyncResult, TmphTcpStreamParameter parameter)
                {
                    if (parameter != null && parameter.IsClientStream)
                    {
                        if (parameter.IsCommand)
                        {
                            switch (TmphTcpStreamAsyncResult.Parameter.Command)
                            {
                                case TmphTcpStreamCommand.BeginRead:
                                    var data = parameter.Data;
                                    if (data.Count != 0)
                                    {
                                        var buffer = TmphTcpStreamAsyncResult.Parameter.Data;
                                        Buffer.BlockCopy(data.Array, data.StartIndex, buffer.Array, buffer.StartIndex,
                                            data.Count);
                                    }
                                    TmphTcpStreamAsyncResult.Parameter.Offset = data.Count;
                                    break;

                                case TmphTcpStreamCommand.BeginWrite:
                                    TmphTcpStreamAsyncResult.IsCompleted = true;
                                    break;
                            }
                        }
                    }
                    else Close();
                }

                /// <summary>
                ///     发送命令获取客户端回馈
                /// </summary>
                /// <param name="parameter">TCP流参数</param>
                /// <returns>客户端回馈</returns>
                private TmphTcpStreamParameter get(TmphTcpStreamParameter parameter)
                {
                    if (pushIdentity == _socket.PushIdentity)
                    {
                        parameter.Index = _socket.getTcpStreamIndex(null);
                        parameter.Identity = _socket.tcpStreamReceivers[parameter.Index].Identity;
                        parameter.ClientIndex = _stream.ClientIndex;
                        parameter.ClientIdentity = _stream.ClientIdentity;
                        try
                        {
                            _socket.SendStream(new TmphStreamIdentity(),
                                new TmphAsynchronousMethod.TmphReturnValue<TmphTcpStreamParameter>
                                {
                                    IsReturn = true,
                                    Value = parameter
                                });
                            var outputParameter = _socket.waitTcpStream(parameter.Index, parameter.Identity);
                            if (outputParameter.IsClientStream) return outputParameter;
                            error();
                        }
                        finally
                        {
                            _socket.cancelTcpStreamIndex(parameter.Index, parameter.Identity, false);
                        }
                    }
                    return TmphTcpStreamParameter.Null;
                }

                /// <summary>
                ///     发送异步命令
                /// </summary>
                /// <param name="TmphTcpStreamAsyncResult">TCP流异步操作状态</param>
                private void send(TmphTcpStreamAsyncResult TmphTcpStreamAsyncResult)
                {
                    if (pushIdentity == _socket.PushIdentity)
                    {
                        var parameter = TmphTcpStreamAsyncResult.Parameter;
                        parameter.Index = _socket.getTcpStreamIndex(TmphTcpStreamAsyncResult);
                        parameter.Identity = _socket.tcpStreamReceivers[parameter.Index].Identity;
                        parameter.ClientIndex = _stream.ClientIndex;
                        parameter.ClientIdentity = _stream.ClientIdentity;
                        try
                        {
                            _socket.SendStream(new TmphStreamIdentity(),
                                new TmphAsynchronousMethod.TmphReturnValue<TmphTcpStreamParameter>
                                {
                                    IsReturn = true,
                                    Value = parameter
                                });
                            _socket.setTcpStreamTimeout(parameter.Index, parameter.Identity);
                            return;
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                        }
                        _socket.cancelTcpStreamIndex(parameter.Index, parameter.Identity, false);
                    }
                    throw ioException;
                }

                /// <summary>
                ///     异步读取
                /// </summary>
                /// <param name="TmphBuffer">缓冲区</param>
                /// <param name="offset">起始位置</param>
                /// <param name="count">接收字节数</param>
                /// <param name="callback">异步回调</param>
                /// <param name="state">用户对象</param>
                /// <returns>异步读取结果</returns>
                public override IAsyncResult BeginRead(byte[] TmphBuffer, int offset, int count, AsyncCallback callback,
                    object state)
                {
                    if (isDisposed != 0) throw objectDisposedException;
                    if (!_stream.CanRead) throw notSupportedException;
                    if (TmphBuffer == null || offset < 0 || count < 0 || offset + count > TmphBuffer.Length)
                        throw argumentException;
                    if (count != 0)
                    {
                        var result = new TmphTcpStreamAsyncResult
                        {
                            TcpStreamCallback = this,
                            Parameter =
                                new TmphTcpStreamParameter
                                {
                                    Command = TmphTcpStreamCommand.BeginRead,
                                    Data = TmphSubArray<byte>.Unsafe(TmphBuffer, offset, count)
                                },
                            Callback = callback,
                            AsyncState = state
                        };
                        send(result);
                        return result;
                    }
                    callback(nullTcpStreamAsyncResult);
                    return nullTcpStreamAsyncResult;
                }

                /// <summary>
                ///     等待挂起的异步读取完成
                /// </summary>
                /// <param name="asyncResult">异步读取结果</param>
                /// <returns>读取的字节数</returns>
                public override int EndRead(IAsyncResult asyncResult)
                {
                    if (isDisposed != 0) throw objectDisposedException;
                    var result = asyncResult as TmphTcpStreamAsyncResult;
                    if (result == null) throw argumentNullException;
                    if (!result.IsCallback) result.AsyncWaitHandle.WaitOne();
                    if (result.IsCompleted) return (int)result.Parameter.Offset;
                    throw ioException;
                }

                /// <summary>
                ///     读取字节序列
                /// </summary>
                /// <param name="TmphBuffer">缓冲区</param>
                /// <param name="offset">起始位置</param>
                /// <param name="count">读取字节数</param>
                /// <returns>读取字节数</returns>
                public override int Read(byte[] TmphBuffer, int offset, int count)
                {
                    if (isDisposed != 0) throw objectDisposedException;
                    if (!_stream.CanRead) throw notSupportedException;
                    if (TmphBuffer == null) throw argumentNullException;
                    if (offset < 0 || count <= 0) throw argumentOutOfRangeException;
                    if (offset + count > TmphBuffer.Length) throw argumentException;
                    var parameter = get(new TmphTcpStreamParameter { Command = TmphTcpStreamCommand.Read, Offset = count });
                    if (parameter.IsCommand)
                    {
                        var data = parameter.Data;
                        if (data.Count != 0) Buffer.BlockCopy(data.Array, data.StartIndex, TmphBuffer, offset, data.Count);
                        return data.Count;
                    }
                    throw ioException;
                }

                /// <summary>
                ///     读取字节
                /// </summary>
                /// <returns>字节</returns>
                public override int ReadByte()
                {
                    if (isDisposed != 0) throw objectDisposedException;
                    if (!_stream.CanRead) throw notSupportedException;
                    var parameter = get(new TmphTcpStreamParameter { Command = TmphTcpStreamCommand.ReadByte });
                    if (parameter.IsCommand) return (int)parameter.Offset;
                    throw ioException;
                }

                /// <summary>
                ///     异步写入
                /// </summary>
                /// <param name="TmphBuffer">缓冲区</param>
                /// <param name="offset">起始位置</param>
                /// <param name="count">接收字节数</param>
                /// <param name="callback">异步回调</param>
                /// <param name="state">用户对象</param>
                /// <returns>异步写入结果</returns>
                public override IAsyncResult BeginWrite(byte[] TmphBuffer, int offset, int count, AsyncCallback callback,
                    object state)
                {
                    if (isDisposed != 0) throw objectDisposedException;
                    if (!_stream.CanWrite) throw notSupportedException;
                    if (TmphBuffer == null || offset < 0 || count < 0 || offset + count > TmphBuffer.Length)
                        throw argumentException;
                    if (count != 0)
                    {
                        var result = new TmphTcpStreamAsyncResult
                        {
                            TcpStreamCallback = this,
                            Parameter =
                                new TmphTcpStreamParameter
                                {
                                    Command = TmphTcpStreamCommand.BeginWrite,
                                    Data = TmphSubArray<byte>.Unsafe(TmphBuffer, offset, count)
                                },
                            Callback = callback,
                            AsyncState = state
                        };
                        send(result);
                        return result;
                    }
                    callback(nullTcpStreamAsyncResult);
                    return nullTcpStreamAsyncResult;
                }

                /// <summary>
                ///     结束异步写操作
                /// </summary>
                /// <param name="asyncResult">异步写入结果</param>
                /// <returns>写入的字节数</returns>
                public override void EndWrite(IAsyncResult asyncResult)
                {
                    if (isDisposed != 0) throw objectDisposedException;
                    var result = asyncResult as TmphTcpStreamAsyncResult;
                    if (result == null) throw argumentNullException;
                    if (!result.IsCallback) result.AsyncWaitHandle.WaitOne();
                    if (result.IsCompleted) return;
                    throw ioException;
                }

                /// <summary>
                ///     写入字节序列
                /// </summary>
                /// <param name="TmphBuffer">缓冲区</param>
                /// <param name="offset">起始位置</param>
                /// <param name="count">读取写入数</param>
                public override void Write(byte[] TmphBuffer, int offset, int count)
                {
                    if (isDisposed != 0) throw objectDisposedException;
                    if (!_stream.CanWrite) throw notSupportedException;
                    if (TmphBuffer == null) throw argumentNullException;
                    if (offset < 0 || count < 0) throw argumentOutOfRangeException;
                    if (offset + count > TmphBuffer.Length) throw argumentException;
                    if (count != 0)
                    {
                        var parameter =
                            get(new TmphTcpStreamParameter
                            {
                                Command = TmphTcpStreamCommand.Write,
                                Data = TmphSubArray<byte>.Unsafe(TmphBuffer, offset, count)
                            });
                        if (parameter.IsCommand) return;
                        throw ioException;
                    }
                }

                /// <summary>
                ///     写入字节
                /// </summary>
                /// <param name="value">字节</param>
                public override void WriteByte(byte value)
                {
                    if (isDisposed != 0) throw objectDisposedException;
                    if (!_stream.CanWrite) throw notSupportedException;
                    var parameter = get(new TmphTcpStreamParameter { Command = TmphTcpStreamCommand.WriteByte, Offset = value });
                    if (parameter.IsCommand) return;
                    throw ioException;
                }

                /// <summary>
                ///     设置流位置
                /// </summary>
                /// <param name="offset">位置</param>
                /// <param name="origin">类型</param>
                /// <returns>流中的新位置</returns>
                public override long Seek(long offset, SeekOrigin origin)
                {
                    if (isDisposed != 0) throw objectDisposedException;
                    var parameter =
                        get(new TmphTcpStreamParameter
                        {
                            Command = TmphTcpStreamCommand.Seek,
                            Offset = offset,
                            SeekOrigin = origin
                        });
                    if (parameter.IsCommand) return parameter.Offset;
                    throw notSupportedException;
                }

                /// <summary>
                ///     设置流长度
                /// </summary>
                /// <param name="value">字节长度</param>
                public override void SetLength(long value)
                {
                    if (isDisposed != 0) throw objectDisposedException;
                    var parameter = get(new TmphTcpStreamParameter { Command = TmphTcpStreamCommand.SetLength, Offset = value });
                    if (parameter.IsCommand) return;
                    throw notSupportedException;
                }

                /// <summary>
                ///     清除缓冲区
                /// </summary>
                public override void Flush()
                {
                    if (isDisposed == 0)
                    {
                        var parameter = get(new TmphTcpStreamParameter { Command = TmphTcpStreamCommand.Flush });
                        if (parameter.IsCommand) return;
                        error();
                    }
                }

                /// <summary>
                ///     错误
                /// </summary>
                private void error()
                {
                    Close();
                    throw ioException;
                }

                /// <summary>
                ///     关闭流
                /// </summary>
                public override void Close()
                {
                    if (Interlocked.Increment(ref isDisposed) == 1)
                    {
                        get(new TmphTcpStreamParameter { Command = TmphTcpStreamCommand.Close });
                        base.Dispose();
                    }
                }

                /// <summary>
                ///     是否资源
                /// </summary>
                public new void Dispose()
                {
                    Close();
                }
            }
        }
    }
}