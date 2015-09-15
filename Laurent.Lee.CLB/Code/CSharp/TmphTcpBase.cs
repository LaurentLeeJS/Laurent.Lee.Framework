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

using Laurent.Lee.CLB.Config;
using Laurent.Lee.CLB.Emit;
using Laurent.Lee.CLB.Net.Tcp;
using Laurent.Lee.CLB.Net.Tcp.Http;
using System;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using memory = Laurent.Lee.CLB.Unsafe.TmphMemory;

namespace Laurent.Lee.CLB.Code.CSharp
{
    /// <summary>
    ///     TCP调用配置基类
    /// </summary>
    public abstract class TmphTcpBase : TmphIgnoreMember
    {
        /// <summary>
        ///     泛型类型服务器端调用类型名称
        /// </summary>
        public const string GenericTypeServerName = "tcpServer";

        /// <summary>
        ///     参数序列化标识
        /// </summary>
        private const uint ParameterSerializeValue = 0x10030000;

        ///// <summary>
        ///// 是否输出调试信息
        ///// </summary>
        //public bool IsOutputDebug;
        /// <summary>
        ///     复制TCP服务配置字段集合
        /// </summary>
        private static readonly FieldInfo[] CopyFields;

        /// <summary>
        ///     获取泛型回调委托函数信息
        /// </summary>
        private static readonly MethodInfo GetGenericParameterCallbackMethod =
            typeof(TmphTcpBase).GetMethod("getGenericParameterCallback", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     主机名称或者IP地址(服务配置)
        /// </summary>
        private string _host;

        /// <summary>
        ///     默认HTTP编码
        /// </summary>
        private TmphAsynchronousMethod.TmphReturnValue<Encoding> _httpEncoding;

        /// <summary>
        ///     IP地址
        /// </summary>
        private IPAddress _ipAddress;

        /// <summary>
        ///     客户端连接检测时间(服务配置),0表示不检测(单位:秒)
        /// </summary>
        public int ClientCheckSeconds = 50;

        /// <summary>
        ///     客户端代码复制到目标目录(服务配置)
        /// </summary>
        public string ClientSegmentationCopyPath;

        /// <summary>
        ///     命令序号(有效域为方法)(不能重复,比如使用枚举),IsIdentityCommand=true时有效
        /// </summary>
        public int CommandIentity = int.MaxValue;

        /// <summary>
        ///     分组标识，0标识无分组(有效域为方法)
        /// </summary>
        public int GroupId;

        /// <summary>
        ///     主机名称或者IP地址(服务配置)
        /// </summary>
        public string Host;

        /// <summary>
        ///     默认HTTP编码名称(服务配置)
        /// </summary>
        public string HttpEncodingName;

        /// <summary>
        ///     HTTP调用名称(有效域为方法)
        /// </summary>
        public string HttpName;

        /// <summary>
        ///     输入参数最大数据长度,0表示不限(有效域为方法)
        /// </summary>
        public int InputParameterMaxLength;

        /// <summary>
        ///     成员是否匹配自定义属性类型(有效域为class)
        /// </summary>
        public bool IsAttribute = true;

        /// <summary>
        ///     是否搜索父类自定义属性(有效域为class)
        /// </summary>
        public bool IsBaseTypeAttribute;

        /// <summary>
        ///     客户端是否提供异步调用(有效域为方法)
        /// </summary>
        public bool IsClientAsynchronous;

        /// <summary>
        ///     客户端是否异步接收数据(服务配置)
        /// </summary>
        public bool IsClientAsynchronousReceive = false;

        /// <summary>
        ///     客户端异步回调是否公用输入参数(有效域为方法)
        /// </summary>
        public bool IsClientAsynchronousReturnInputParameter;

        /// <summary>
        ///     客户端回调是否使用任务池(有效域为方法)。警告：如果设置为false，在回调中不能调用远程同步方法，否则死锁
        /// </summary>
        public bool IsClientCallbackTask = true;

        /// <summary>
        ///     客户端是否提供同步调用(有效域为方法)
        /// </summary>
        public bool IsClientSynchronous = true;

        /// <summary>
        ///     是否压缩数据(服务配置)
        /// </summary>
        public bool IsCompress;

        /// <summary>
        ///     是否支持HTTP客户端(服务配置)
        /// </summary>
        public bool IsHttpClient;

        /// <summary>
        ///     HTTP调用是否仅支持POST(有效域为方法)
        /// </summary>
        public bool IsHttpPostOnly = true;

        /// <summary>
        ///     是否采用命令序号识别函数,用于接口稳定的网络服务(服务配置)
        /// </summary>
        public bool IsIdentityCommand;

        /// <summary>
        ///     成员匹配自定义属性是否可继承(有效域为class)
        /// </summary>
        public bool IsInheritAttribute = true;

        /// <summary>
        ///     是否使用JSON序列化(服务配置)
        /// </summary>
        public bool IsJsonSerialize;

        /// <summary>
        ///     是否保持异步回调(有效域为方法)
        /// </summary>
        public bool IsKeepCallback;

        /// <summary>
        ///     是否预申请TCP服务实例(服务配置)
        /// </summary>
        public bool IsPerpleRegister = true;

        /// <summary>
        ///     是否生成命令序号记忆代码(服务配置)，IsIdentityCommand=true时有效
        /// </summary>
        public bool IsRememberIdentityCommeand = true;

        /// <summary>
        ///     是否分割客户端代码(服务配置)
        /// </summary>
        public bool IsSegmentation;

        /// <summary>
        ///     服务器端函数是否显示异步回调(服务配置),(返回值必须为void，最后一个参数必须为回调委托action(Laurent.Lee.CLB.Code.CSharp.methodInfo.asynchronousReturn))
        /// </summary>
        public bool IsServerAsynchronousCallback;

        /// <summary>
        ///     服务器端是否异步接收数据(服务配置)
        /// </summary>
        public bool IsServerAsynchronousReceive = true;

        /// <summary>
        ///     服务器端模拟异步调用是否使用任务池(有效域为方法,对于同步服务器端的验证调用无效)
        /// </summary>
        public bool IsServerAsynchronousTask = true;

        /// <summary>
        ///     是否只允许一个TCP服务实例(服务配置)
        /// </summary>
        public bool IsSingleRegister = true;

        /// <summary>
        ///     是否验证方法(有效域为方法),一个TCP调用服务只能指定一个验证方法,且返回值类型必须为bool
        /// </summary>
        public bool IsVerifyMethod;

        /// <summary>
        ///     每IP最大活动客户端连接数,小于等于0表示不限(服务配置)
        /// </summary>
        public int MaxActiveClientCount;

        /// <summary>
        ///     每IP最大客户端连接数,小于等于0表示不限(服务配置)
        /// </summary>
        public int MaxClientCount;

        /// <summary>
        ///     每秒最低接收数据(单位:KB)(服务配置)
        /// </summary>
        public int MinReceivePerSecond;

        /// <summary>
        ///     监听端口(服务配置)
        /// </summary>
        public int Port;

        /// <summary>
        ///     服务器端接收数据缓冲区字节数(服务配置)
        /// </summary>
        public int ReceiveBufferSize = TmphAppSetting.StreamBufferSize;

        /// <summary>
        ///     接收数据超时的秒数(服务配置)
        /// </summary>
        public int ReceiveTimeout;

        /// <summary>
        ///     接收命令超时分钟数(服务配置)
        /// </summary>
        public int RecieveCommandMinutes;

        /// <summary>
        ///     服务器端发送数据缓冲区初始字节数(服务配置)
        /// </summary>
        public int SendBufferSize = TmphAppSetting.StreamBufferSize;

        /// <summary>
        ///     服务名称(服务配置)
        /// </summary>
        public string Service;

        /// <summary>
        ///     TCP注册服务名称(服务配置)
        /// </summary>
        public string TcpRegister;

        /// <summary>
        ///     客户端验证方法类(服务配置),必须继承接口Laurent.Lee.CLB.Code.CSharp.tcpBase.ITcpClientVerifyMethod
        /// </summary>
        public Type VerifyMethodType;

        /// <summary>
        ///     验证超时秒数(服务配置)
        /// </summary>
        public int VerifySeconds = 20;

        /// <summary>
        ///     验证类(服务配置),必须继承接口Laurent.Lee.CLB.Code.CSharp.tcpBase.ITcpVerify或Laurent.Lee.CLB.Code.CSharp.tcpBase.ITcpVerifyAsynchronous
        /// </summary>
        public Type VerifyType;

        static TmphTcpBase()
        {
            CopyFields = typeof(TmphTcpBase).GetFields(BindingFlags.Instance | BindingFlags.Public)
                .getFindArray(
                    field =>
                        field.Name != "Service" && field.Name != "IsAttribute" && field.Name != "IsBaseTypeAttribute" &&
                        field.Name != "IsInheritAttribute");
        }

        /// <summary>
        ///     TCP注册服务名称
        /// </summary>
        public virtual string TcpRegisterName
        {
            get { return TcpRegister; }
        }

        /// <summary>
        ///     IP地址
        /// </summary>
        internal IPAddress IpAddress
        {
            get
            {
                if (_ipAddress == null || _host != Host)
                {
                    _ipAddress = HostToIpAddress(_host = Host) ?? IPAddress.Any;
                }
                return _ipAddress;
            }
        }

        /// <summary>
        ///     服务名称
        /// </summary>
        public virtual string ServiceName
        {
            get { return Service; }
        }

        /// <summary>
        ///     默认HTTP编码
        /// </summary>
        internal Encoding HttpEncoding
        {
            get
            {
                if (!_httpEncoding.IsReturn)
                {
                    if (HttpEncodingName != null)
                    {
                        try
                        {
                            _httpEncoding = Encoding.GetEncoding(HttpEncodingName);
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, HttpEncodingName, false);
                        }
                    }
                    _httpEncoding.IsReturn = true;
                }
                return _httpEncoding.Value;
            }
        }

        /// <summary>
        ///     是否支持HTTP客户端 或者 是否使用JSON序列化
        /// </summary>
        public bool IsHttpClientOrJsonSerialize
        {
            get { return IsHttpClient || IsJsonSerialize; }
        }

        /// <summary>
        ///     JSON反序列化
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="value">目标对象</param>
        /// <param name="data">序列化数据</param>
        /// <returns>是否成功</returns>
        public static bool JsonDeSerialize<TValueType>(ref TValueType value, TmphSubArray<byte> data)
        {
            var json = new TmphParameterJsonToSerialize<TValueType> { Return = value };
            if (TmphDataDeSerializer.DeSerialize(data, ref json))
            {
                value = json.Return;
                return true;
            }
            return false;
        }

        /// <summary>
        ///     JSON转换序列化
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="value">目标对象</param>
        /// <returns>序列化对象</returns>
        internal static TmphAsynchronousMethod.TmphReturnValue<TmphParameterJsonToSerialize<TValueType>> JsonToSerialize
            <TValueType>(TmphAsynchronousMethod.TmphReturnValue<TValueType> value)
        {
            if (value.IsReturn) return new TmphParameterJsonToSerialize<TValueType> { Return = value.Value };
            return new TmphAsynchronousMethod.TmphReturnValue<TmphParameterJsonToSerialize<TValueType>> { IsReturn = false };
        }

        /// <summary>
        ///     复制TCP服务配置
        /// </summary>
        /// <param name="value">TCP服务配置</param>
        public void CopyFrom(TmphTcpBase value)
        {
            foreach (var field in CopyFields) field.SetValue(this, field.GetValue(value));
        }

        /// <summary>
        ///     获取泛型参数集合
        /// </summary>
        /// <param name="_"></param>
        /// <param name="types">泛型参数集合</param>
        /// <returns>泛型参数集合</returns>
        public static TmphRemoteType[] GetGenericParameters(int _, params Type[] types)
        {
            return types.getArray(type => new TmphRemoteType(type));
        }

        /// <summary>
        ///     获取泛型回调委托
        /// </summary>
        /// <typeparam name="TValueType">返回值类型</typeparam>
        /// <param name="callback">回调委托</param>
        /// <returns>泛型回调委托</returns>
        public static object GetGenericParameterCallback<TValueType>(
            Func<TmphAsynchronousMethod.TmphReturnValue<object>, bool> callback)
        {
            return callback != null ? TmphGenericParameterCallback<TValueType>.Create(callback) : null;
        }

        /// <summary>
        ///     获取泛型回调委托
        /// </summary>
        /// <param name="type">返回值类型</param>
        /// <param name="callback">回调委托</param>
        /// <returns>泛型回调委托</returns>
        public static object GetGenericParameterCallback(TmphRemoteType type,
            Func<TmphAsynchronousMethod.TmphReturnValue<object>, bool> callback)
        {
            return
                ((Func<Func<TmphAsynchronousMethod.TmphReturnValue<object>, bool>, object>)
                    Delegate.CreateDelegate(
                        typeof(Func<Func<TmphAsynchronousMethod.TmphReturnValue<object>, bool>, object>),
                        GetGenericParameterCallbackMethod.MakeGenericMethod(type.Type)))(callback);
        }

        /// <summary>
        ///     主机名称转换成IP地址
        /// </summary>
        /// <param name="host">主机名称</param>
        /// <returns>IP地址</returns>
        internal static IPAddress HostToIpAddress(string host)
        {
            if (host.Length() != 0)
            {
                IPAddress ipAddress;
                if (!IPAddress.TryParse(host, out ipAddress))
                {
                    try
                    {
                        ipAddress = Dns.GetHostEntry(host).AddressList[0];
                    }
                    catch (Exception error)
                    {
                        TmphLog.Default.Add(error, host);
                    }
                }
                return ipAddress;
            }
            return null;
        }

        /// <summary>
        ///     TCP客户端验证接口
        /// </summary>
        public interface ITcpClientVerify
        {
            /// <summary>
            ///     TCP客户端验证
            /// </summary>
            /// <param name="socket">TCP调用客户端套接字</param>
            /// <returns>是否通过验证</returns>
            bool Verify(TmphCommandClient.TmphSocket socket);
        }

        /// <summary>
        ///     TCP客户端验证函数接口(tcpCall)
        /// </summary>
        public interface ITcpClientVerifyMethod
        {
            /// <summary>
            ///     TCP客户端验证
            /// </summary>
            /// <returns>是否通过验证</returns>
            bool Verify();
        }

        /// <summary>
        ///     TCP客户端验证函数接口(tcpServer)
        /// </summary>
        /// <typeparam name="TClientType">TCP客户端类型</typeparam>
        public interface ITcpClientVerifyMethod<TClientType>
        {
            /// <summary>
            ///     TCP客户端验证
            /// </summary>
            /// <param name="TmphClient">TCP调用客户端</param>
            /// <returns>是否通过验证</returns>
            bool Verify(TClientType TmphClient);
        }

        ///// <summary>
        ///// TCP客户端验证
        ///// </summary>
        ///// <typeparam name="TClientType">TCP客户端类型</typeparam>
        ///// <param name="TmphClient">TCP客户端</param>
        ///// <param name="verify">TCP客户端验证接口</param>
        ///// <returns>验证是否成功</returns>
        //public static bool Verify<TClientType>(TClientType TmphClient, ITcpClientVerifyMethod<TClientType> verify)
        //{
        //    if (verify == null) return true;
        //    try
        //    {
        //        return verify.Verify(TmphClient);
        //    }
        //    catch (Exception error)
        //    {
        //        log.Default.Add(error, null, false);
        //    }
        //    return false;
        //}
        /// <summary>
        ///     TCP服务器端同步验证客户端接口
        /// </summary>
        public interface ITcpVerify : ITcpClientVerify
        {
            /// <summary>
            ///     TCP客户端同步验证
            /// </summary>
            /// <param name="socket">同步套接字</param>
            /// <returns>是否通过验证</returns>
            bool Verify(TmphCommandServer.TmphSocket socket);
        }

        /// <summary>
        ///     客户端标识
        /// </summary>
        public sealed class TmphClient
        {
            /// <summary>
            ///     客户端用户信息
            /// </summary>
            public object UserInfo;

            /// <summary>
            ///     HTTP页面
            /// </summary>
            public TmphHttpPage HttpPage
            {
                get { return (TmphHttpPage)UserInfo; }
            }
        }

        /// <summary>
        ///     HTTP页面
        /// </summary>
        public sealed class TmphHttpPage : TmphWebPage.TmphPage
        {
            /// <summary>
            ///     HTTP请求头部
            /// </summary>
            public TmphRequestHeader HttpRequestHeader
            {
                get { return RequestHeader; }
            }

            /// <summary>
            ///     HTTP请求表单
            /// </summary>
            public TmphRequestForm HttpForm
            {
                get { return Form; }
            }

            /// <summary>
            ///     WEB页面回收
            /// </summary>
            internal override void PushPool()
            {
                Clear();
                TmphTypePool<TmphHttpPage>.Push(this);
            }

            /// <summary>
            ///     参数反序列化
            /// </summary>
            /// <typeparam name="TValueType">参数类型</typeparam>
            /// <param name="value">参数值</param>
            /// <returns>是否成功</returns>
            public bool DeSerialize<TValueType>(ref TValueType value)
            {
                if (HttpForm != null && HttpForm.Json != null)
                {
                    if (HttpForm.Json.Length != 0
                        && !TmphJsonParser.Parse(HttpForm.Json, ref value))
                    {
                        return false;
                    }
                }
                else
                {
                    var queryJson = HttpRequestHeader.QueryJson;
                    if (queryJson.Length != 0
                        && !TmphJsonParser.Parse(queryJson, ref value))
                    {
                        return false;
                    }
                }
                return true;
            }

            /// <summary>
            ///     输出
            /// </summary>
            /// <param name="returnValue">是否调用成功</param>
            /// <returns>是否输出成功</returns>
            public new bool Response(TmphAsynchronousMethod.TmphReturnValue returnValue)
            {
                var socket = Socket;
                TmphResponse response = null;
                var identity = SocketIdentity;
                try
                {
                    base.Response = (response = TmphResponse.Copy(base.Response));
                    ResponseData(returnValue.IsReturn ? Web.TmphAjax.Object : Web.TmphAjax.Null);
                    if (ResponseEnd(ref response)) return true;
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                finally
                {
                    TmphResponse.Push(ref response);
                }
                if (socket.ResponseError(identity, TmphResponse.TmphState.ServerError500)) PushPool();
                return false;
            }

            /// <summary>
            ///     输出
            /// </summary>
            /// <typeparam name="TOutputParameter">输出数据类型</typeparam>
            /// <param name="returnValue">输出数据</param>
            /// <returns>是否输出成功</returns>
            public new unsafe bool Response<TOutputParameter>(
                TmphAsynchronousMethod.TmphReturnValue<TOutputParameter> returnValue)
            {
                var socket = Socket;
                TmphResponse response = null;
                var identity = SocketIdentity;
                try
                {
                    base.Response = (response = TmphResponse.Copy(base.Response));
                    if (returnValue.IsReturn)
                    {
                        if (returnValue.Value == null) ResponseData(Web.TmphAjax.Object);
                        else
                        {
                            var TmphBuffer = TmphUnmanagedPool.StreamBuffers.Get();
                            try
                            {
                                using (
                                    var jsonStream = response.ResetJsonStream(TmphBuffer.Data,
                                        TmphUnmanagedPool.StreamBuffers.Size))
                                {
                                    if (ResponseEncoding.CodePage == Encoding.Unicode.CodePage)
                                    {
                                        TmphJsonSerializer.Serialize(returnValue.Value, jsonStream, response.BodyStream,
                                            null);
                                    }
                                    else
                                    {
                                        TmphJsonSerializer.ToJson(returnValue.Value, jsonStream);
                                        ResponseData(Web.TmphAjax.FormatJavascript(jsonStream));
                                    }
                                }
                            }
                            finally
                            {
                                TmphUnmanagedPool.StreamBuffers.Push(ref TmphBuffer);
                            }
                        }
                    }
                    else ResponseData(Web.TmphAjax.Null);
                    if (ResponseEnd(ref response)) return true;
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                finally
                {
                    TmphResponse.Push(ref response);
                }
                if (socket.ResponseError(identity, TmphResponse.TmphState.ServerError500)) PushPool();
                return false;
            }

            /// <summary>
            ///     获取HTTP页面
            /// </summary>
            /// <param name="socket">HTTP套接字接口设置</param>
            /// <param name="domainServer">域名服务</param>
            /// <param name="socketIdentity">套接字操作编号</param>
            /// <param name="requestHeader">HTTP请求头部信息</param>
            /// <param name="form">HTTP表单</param>
            /// <returns>HTTP页面</returns>
            internal static TmphHttpPage Get(TmphSocketBase socket, TmphDomainServer domainServer
                , long socketIdentity, TmphRequestHeader requestHeader, TmphRequestForm form)
            {
                var httpPage = TmphTypePool<TmphHttpPage>.Pop() ?? new TmphHttpPage();
                httpPage.Socket = socket;
                httpPage.DomainServer = domainServer;
                httpPage.SocketIdentity = socketIdentity;
                httpPage.RequestHeader = requestHeader;
                httpPage.ResponseEncoding = socket.TcpCommandSocket.HttpEncoding ?? domainServer.ResponseEncoding;
                httpPage.Form = form;
                return httpPage;
            }
        }

        /// <summary>
        ///     泛型函数信息
        /// </summary>
        public struct TmphGenericMethod : IEquatable<TmphGenericMethod>
        {
            /// <summary>
            ///     泛型参数数量
            /// </summary>
            public int ArgumentCount;

            /// <summary>
            ///     哈希值
            /// </summary>
            public int HashCode;

            /// <summary>
            ///     函数名称
            /// </summary>
            public string Name;

            /// <summary>
            ///     参数名称集合
            /// </summary>
            public string[] ParameterTypeNames;

            /// <summary>
            ///     泛型函数信息
            /// </summary>
            /// <param name="method">泛型函数信息</param>
            public TmphGenericMethod(MethodInfo method)
            {
                Name = method.Name;
                ArgumentCount = method.GetGenericArguments().Length;
                ParameterTypeNames =
                    TmphParameterInfo.Get(method).getArray(value => value.ParameterRef + value.ParameterType.FullName);
                HashCode = Name.GetHashCode() ^ ArgumentCount;
                SetHashCode();
            }

            /// <summary>
            ///     泛型函数信息
            /// </summary>
            /// <param name="name">函数名称</param>
            /// <param name="argumentCount">泛型参数数量</param>
            /// <param name="typeNames">参数名称集合</param>
            public TmphGenericMethod(string name, int argumentCount, params string[] typeNames)
            {
                Name = name;
                ArgumentCount = argumentCount;
                ParameterTypeNames = typeNames;
                HashCode = Name.GetHashCode() ^ ArgumentCount;
                SetHashCode();
            }

            /// <summary>
            ///     比较是否相等
            /// </summary>
            /// <param name="other">比较对象</param>
            /// <returns>是否相等</returns>
            public bool Equals(TmphGenericMethod other)
            {
                if (HashCode == other.HashCode && Name == other.Name
                    && ParameterTypeNames.Length == other.ParameterTypeNames.Length)
                {
                    var index = 0;
                    foreach (var name in other.ParameterTypeNames)
                    {
                        if (ParameterTypeNames[index++] != name) return false;
                    }
                    return true;
                }
                return false;
            }

            /// <summary>
            ///     计算哈希值
            /// </summary>
            private void SetHashCode()
            {
                foreach (var name in ParameterTypeNames) HashCode ^= name.GetHashCode();
            }

            /// <summary>
            ///     哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override int GetHashCode()
            {
                return HashCode;
            }

            /// <summary>
            ///     比较是否相等
            /// </summary>
            /// <param name="other">比较对象</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object other)
            {
                return Equals((TmphGenericMethod)other);
                //return other != null && other.GetType() == typeof(genericMethod) && Equals((genericMethod)other);
            }
        }

        /// <summary>
        ///     泛型回调委托
        /// </summary>
        /// <typeparam name="TValueType">返回值类型</typeparam>
        private sealed class TmphGenericParameterCallback<TValueType>
        {
            /// <summary>
            ///     回调处理
            /// </summary>
            private Func<TmphAsynchronousMethod.TmphReturnValue<object>, bool> _callback;

            /// <summary>
            ///     回调处理
            /// </summary>
            /// <param name="value">返回值</param>
            /// <returns></returns>
            private bool OnReturn(TmphAsynchronousMethod.TmphReturnValue<TValueType> value)
            {
                return
                    _callback(new TmphAsynchronousMethod.TmphReturnValue<object>
                    {
                        IsReturn = value.IsReturn,
                        Value = value.IsReturn ? (object)value.Value : null
                    });
            }

            /// <summary>
            ///     泛型回调委托
            /// </summary>
            /// <param name="callback">回调处理</param>
            /// <returns>泛型回调委托</returns>
            public static Func<TmphAsynchronousMethod.TmphReturnValue<TValueType>, bool> Create(
                Func<TmphAsynchronousMethod.TmphReturnValue<object>, bool> callback)
            {
                return new TmphGenericParameterCallback<TValueType> { _callback = callback }.OnReturn;
            }
        }

        /// <summary>
        ///     负载均衡回调
        /// </summary>
        /// <typeparam name="TReturnType">返回值类型</typeparam>
        public abstract class TmphLoadBalancingCallback<TReturnType>
        {
            /// <summary>
            ///     回调委托
            /// </summary>
            protected Action<TmphAsynchronousMethod.TmphReturnValue<TReturnType>> OnReturn;

            /// <summary>
            ///     回调委托
            /// </summary>
            protected Action<TmphAsynchronousMethod.TmphReturnValue<TReturnType>> OnReturnHandle;

            /// <summary>
            ///     错误尝试次数
            /// </summary>
            protected int TryCount;

            /// <summary>
            ///     负载均衡回调
            /// </summary>
            protected TmphLoadBalancingCallback()
            {
                OnReturnHandle = OnReturnValue;
            }

            /// <summary>
            ///     TCP客户端回调
            /// </summary>
            /// <param name="returnValue">返回值</param>
            private void OnReturnValue(TmphAsynchronousMethod.TmphReturnValue<TReturnType> returnValue)
            {
                if (returnValue.IsReturn || --TryCount <= 0) _push_(returnValue);
                else
                {
                    Thread.Sleep(1);
                    _call_();
                }
            }

            /// <summary>
            ///     TCP客户端调用
            /// </summary>
            protected abstract void _call_();

            /// <summary>
            ///     添加到回调池负载均衡回调
            /// </summary>
            /// <param name="returnValue">返回值</param>
            protected void _push_(TmphAsynchronousMethod.TmphReturnValue<TReturnType> returnValue)
            {
                var onReturn = OnReturn;
                OnReturn = null;
                _push_(returnValue.IsReturn);
                onReturn(returnValue);
            }

            /// <summary>
            ///     添加到回调池负载均衡回调
            /// </summary>
            /// <param name="isReturn">是否回调成功</param>
            protected abstract void _push_(bool isReturn);
        }

        /// <summary>
        ///     负载均衡回调
        /// </summary>
        public abstract class TmphLoadBalancingCallback
        {
            /// <summary>
            ///     回调委托
            /// </summary>
            protected Action<TmphAsynchronousMethod.TmphReturnValue> OnReturn;

            /// <summary>
            ///     回调委托
            /// </summary>
            protected Action<TmphAsynchronousMethod.TmphReturnValue> OnReturnHandle;

            /// <summary>
            ///     错误尝试次数
            /// </summary>
            protected int TryCount;

            /// <summary>
            ///     负载均衡回调
            /// </summary>
            protected TmphLoadBalancingCallback()
            {
                OnReturnHandle = OnReturnValue;
            }

            /// <summary>
            ///     TCP客户端回调
            /// </summary>
            /// <param name="returnValue">返回值</param>
            private void OnReturnValue(TmphAsynchronousMethod.TmphReturnValue returnValue)
            {
                if (returnValue.IsReturn || --TryCount <= 0) _push_(returnValue);
                else
                {
                    Thread.Sleep(1);
                    _call_();
                }
            }

            /// <summary>
            ///     TCP客户端调用
            /// </summary>
            protected abstract void _call_();

            /// <summary>
            ///     添加到回调池负载均衡回调
            /// </summary>
            /// <param name="returnValue">返回值</param>
            protected void _push_(TmphAsynchronousMethod.TmphReturnValue returnValue)
            {
                var onReturn = OnReturn;
                OnReturn = null;
                _push_(returnValue.IsReturn);
                onReturn(returnValue);
            }

            /// <summary>
            ///     添加到回调池负载均衡回调
            /// </summary>
            /// <param name="isReturn">是否回调成功</param>
            protected abstract void _push_(bool isReturn);
        }

        /// <summary>
        ///     命令序号记忆数据
        /// </summary>
        public struct TmphRememberIdentityCommeand
        {
            /// <summary>
            ///     命令序号集合
            /// </summary>
            public int[] Identitys;

            /// <summary>
            ///     命令名称集合
            /// </summary>
            public string[] Names;
        }

        /// <summary>
        ///     TCP参数流
        /// </summary>
        [Emit.TmphDataSerialize(IsReferenceMember = false, IsMemberMap = false)]
        public struct TmphTcpStream
        {
            /// <summary>
            ///     否支持读取
            /// </summary>
            public bool CanRead;

            /// <summary>
            ///     否支持查找
            /// </summary>
            public bool CanSeek;

            /// <summary>
            ///     是否可以超时
            /// </summary>
            public bool CanTimeout;

            /// <summary>
            ///     否支持写入
            /// </summary>
            public bool CanWrite;

            /// <summary>
            ///     客户端序号
            /// </summary>
            public int ClientIdentity;

            /// <summary>
            ///     客户端索引
            /// </summary>
            public int ClientIndex;

            /// <summary>
            ///     是否有效
            /// </summary>
            public bool IsStream;
        }

        /// <summary>
        ///     字节数组缓冲区反序列化事件
        /// </summary>
        public struct TmphSubByteArrayEvent
        {
            /// <summary>
            ///     字节数组缓冲区
            /// </summary>
            public TmphSubArray<byte> TmphBuffer;

            /// <summary>
            ///     反序列化事件
            /// </summary>
            public Action<TmphSubArray<byte>> Event;

            /// <summary>
            ///     反序列化事件
            /// </summary>
            private void CallEvent()
            {
                if (Event != null) Event(TmphBuffer);
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="TmphBuffer">字节数组缓冲区</param>
            /// <returns>内存数据流</returns>
            public static implicit operator TmphSubByteArrayEvent(TmphSubArray<byte> TmphBuffer)
            {
                return new TmphSubByteArrayEvent { TmphBuffer = TmphBuffer };
            }

            /// <summary>
            ///     反序列化
            /// </summary>
            /// <param name="deSerializer">序列化数据</param>
            /// <param name="value"></param>
            [Emit.TmphDataSerialize.TmphCustom]
            private static void DeSerialize(TmphDataDeSerializer deSerializer, ref TmphSubByteArrayEvent value)
            {
                deSerializer.DeSerialize(ref value.TmphBuffer);
                value.CallEvent();
            }

            /// <summary>
            ///     对象序列化
            /// </summary>
            /// <param name="serializer"></param>
            /// <param name="value"></param>
            [Emit.TmphDataSerialize.TmphCustom]
            private static void Serialize(TmphDataSerializer serializer, TmphSubByteArrayEvent value)
            {
                if (value.TmphBuffer.array == null) serializer.Stream.Write(TmphBinarySerializer.NullValue);
                else TmphBinarySerializer.Serialize(serializer.Stream, value.TmphBuffer);
            }

            /// <summary>
            ///     对象序列化
            /// </summary>
            /// <param name="toJsoner"></param>
            /// <param name="value"></param>
            [TmphJsonSerialize.TmphCustom]
            private static void ToJson(TmphJsonSerializer toJsoner, TmphSubByteArrayEvent value)
            {
                using (var dataStream = new TmphUnmanagedStream(toJsoner.JsonStream))
                {
                    try
                    {
                        if (value.TmphBuffer.array == null) dataStream.Write(TmphBinarySerializer.NullValue);
                        else TmphBinarySerializer.Serialize(dataStream, value.TmphBuffer);
                    }
                    finally
                    {
                        toJsoner.JsonStream.From(dataStream);
                    }
                }
            }

            /// <summary>
            ///     反序列化
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">参数</param>
            [TmphJsonParse.TmphCustom]
            private static unsafe void ParseJson(TmphJsonParser parser, ref TmphSubByteArrayEvent value)
            {
                var read = TmphBinaryDeSerializer.DeSerialize((byte*)parser.Current, (byte*)parser.End, parser.TmphBuffer,
                    ref value.TmphBuffer);
                if (read == null) parser.Error(TmphJsonParser.TmphParseState.CrashEnd);
                else
                {
                    parser.Current = (char*)read;
                    value.CallEvent();
                }
            }
        }

        /// <summary>
        ///     字节数组缓冲区(反序列化数据必须立即使用,否则可能脏数据)
        /// </summary>
        public struct TmphSubByteArrayBuffer
        {
            /// <summary>
            ///     字节数组缓冲区
            /// </summary>
            public TmphSubArray<byte> TmphBuffer;

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="TmphBuffer">字节数组缓冲区</param>
            /// <returns>内存数据流</returns>
            public static implicit operator TmphSubByteArrayBuffer(TmphSubArray<byte> TmphBuffer)
            {
                return new TmphSubByteArrayBuffer { TmphBuffer = TmphBuffer };
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="TmphBuffer">内存数据流</param>
            /// <returns>字节数组缓冲区</returns>
            public static implicit operator TmphSubArray<byte>(TmphSubByteArrayBuffer TmphBuffer)
            {
                return TmphBuffer.TmphBuffer;
            }

            /// <summary>
            ///     反序列化
            /// </summary>
            /// <param name="deSerializer">序列化数据</param>
            /// <param name="value"></param>
            [Emit.TmphDataSerialize.TmphCustom]
            private static void DeSerialize(TmphDataDeSerializer deSerializer, ref TmphSubByteArrayBuffer value)
            {
                deSerializer.DeSerialize(ref value.TmphBuffer);
            }

            /// <summary>
            ///     对象序列化
            /// </summary>
            /// <param name="serializer"></param>
            /// <param name="value"></param>
            [Emit.TmphDataSerialize.TmphCustom]
            private static void Serialize(TmphDataSerializer serializer, TmphSubByteArrayBuffer value)
            {
                if (value.TmphBuffer.array == null) serializer.Stream.Write(TmphBinarySerializer.NullValue);
                else TmphBinarySerializer.Serialize(serializer.Stream, value.TmphBuffer);
            }

            /// <summary>
            ///     对象转换成JSON字符串
            /// </summary>
            /// <param name="toJsoner">对象转换成JSON字符串</param>
            /// <param name="value">参数</param>
            [TmphJsonSerialize.TmphCustom]
            private static void ToJson(TmphJsonSerializer toJsoner, TmphSubByteArrayBuffer value)
            {
                memory.ToJson(toJsoner.JsonStream, value.TmphBuffer);
            }

            /// <summary>
            ///     对象转换成JSON字符串
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">参数</param>
            [TmphJsonParse.TmphCustom]
            private static void ParseJson(TmphJsonParser parser, ref TmphSubByteArrayBuffer value)
            {
                byte[] TmphBuffer = null;
                TmphJsonParser.TmphTypeParser<byte>.Array(parser, ref TmphBuffer);
                if (TmphBuffer == null) value.TmphBuffer.Null();
                else value.TmphBuffer.UnsafeSet(TmphBuffer, 0, TmphBuffer.Length);
            }
        }

        /// <summary>
        ///     内存数据流(序列化输入流,反序列化字节数组)(反序列化数据必须立即使用,否则可能脏数据)
        /// </summary>
        public struct TmphSubByteUnmanagedStream
        {
            /// <summary>
            ///     内存数据流
            /// </summary>
            public TmphSubArray<byte> TmphBuffer;

            /// <summary>
            ///     内存数据流
            /// </summary>
            public TmphUnmanagedStream Stream;

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="stream">内存数据流</param>
            /// <returns>内存数据流</returns>
            public static implicit operator TmphSubByteUnmanagedStream(TmphUnmanagedStream stream)
            {
                return new TmphSubByteUnmanagedStream { Stream = stream };
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="stream">内存数据流</param>
            /// <returns>内存数据流</returns>
            public static implicit operator TmphSubArray<byte>(TmphSubByteUnmanagedStream stream)
            {
                return stream.TmphBuffer;
            }

            /// <summary>
            ///     反序列化
            /// </summary>
            /// <param name="deSerializer">序列化数据</param>
            [Emit.TmphDataSerialize.TmphCustom]
            private static void DeSerialize(TmphDataDeSerializer deSerializer, ref TmphSubByteUnmanagedStream value)
            {
                deSerializer.DeSerialize(ref value.TmphBuffer);
            }

            /// <summary>
            ///     对象序列化
            /// </summary>
            [Emit.TmphDataSerialize.TmphCustom]
            private static unsafe void Serialize(TmphDataSerializer serializer, TmphSubByteUnmanagedStream value)
            {
                if (value.Stream == null) serializer.Stream.Write(TmphBinarySerializer.NullValue);
                else
                {
                    var stream = serializer.Stream;
                    int streamLength = value.Stream.Length, length = ((streamLength + 3) & (int.MaxValue - 3));
                    stream.PrepLength(length + sizeof(int));
                    var data = stream.CurrentData;
                    *(int*)data = streamLength;
                    memory.Copy(value.Stream.Data, data + sizeof(int), length);
                    stream.Unsafer.AddLength(length + sizeof(int));
                }
            }

            /// <summary>
            ///     对象转换成JSON字符串
            /// </summary>
            /// <param name="toJsoner">对象转换成JSON字符串</param>
            /// <param name="value">参数</param>
            [TmphJsonSerialize.TmphCustom]
            private static void ToJson(TmphJsonSerializer toJsoner, TmphSubByteUnmanagedStream value)
            {
                memory.ToJson(toJsoner.JsonStream, value.TmphBuffer);
            }

            /// <summary>
            ///     对象转换成JSON字符串
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">参数</param>
            [TmphJsonParse.TmphCustom]
            private static void ParseJson(TmphJsonParser parser, ref TmphSubByteUnmanagedStream value)
            {
                byte[] TmphBuffer = null;
                TmphJsonParser.TmphTypeParser<byte>.Array(parser, ref TmphBuffer);
                if (TmphBuffer == null) value.TmphBuffer.Null();
                else value.TmphBuffer.UnsafeSet(TmphBuffer, 0, TmphBuffer.Length);
            }
        }

        /// <summary>
        ///     JSON参数
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        public struct TmphParameterJsonToSerialize<TValueType>
        {
            /// <summary>
            ///     参数值
            /// </summary>
            public TValueType Return;

            /// <summary>
            ///     反序列化
            /// </summary>
            /// <param name="deSerializer"></param>
            /// <param name="value"></param>
            [Emit.TmphDataSerialize.TmphCustom]
            private static unsafe void DeSerialize(TmphDataDeSerializer deSerializer,
                ref TmphParameterJsonToSerialize<TValueType> value)
            {
                var length = *(int*)deSerializer.Read;
                if (length == TmphBinarySerializer.NullValue)
                {
                    value.Return = default(TValueType);
                    return;
                }
                if ((length & 1) == 0 && length > 0)
                {
                    var start = deSerializer.Read;
                    if (deSerializer.VerifyRead((length + (2 + sizeof(int))) & (int.MaxValue - 3))
                        &&
                        TmphJsonParser.Parse((char*)(start + sizeof(int)), length >> 1, ref value.Return, null,
                            deSerializer.TmphBuffer))
                    {
                        return;
                    }
                }
                deSerializer.Error(TmphBinaryDeSerializer.TmphDeSerializeState.IndexOutOfRange);
            }

            /// <summary>
            ///     对象序列化
            /// </summary>
            /// <param name="serializer"></param>
            /// <param name="value"></param>
            [Emit.TmphDataSerialize.TmphCustom]
            private static unsafe void Serialize(TmphDataSerializer serializer,
                TmphParameterJsonToSerialize<TValueType> value)
            {
                if (value.Return == null) serializer.Stream.Write(TmphBinarySerializer.NullValue);
                else
                {
                    var TmphBuffer = TmphUnmanagedPool.StreamBuffers.Get();
                    try
                    {
                        using (
                            var jsonStream = serializer.ResetJsonStream(TmphBuffer.Data, TmphUnmanagedPool.StreamBuffers.Size)
                            )
                        {
                            TmphJsonSerializer.Serialize(value.Return, jsonStream, serializer.Stream, null);
                        }
                    }
                    finally
                    {
                        TmphUnmanagedPool.StreamBuffers.Push(ref TmphBuffer);
                    }
                }
            }
        }
    }
}

namespace Laurent.Lee.CLB.Code
{
    /// <summary>
    ///     参数信息
    /// </summary>
    public sealed partial class TmphParameterInfo
    {
        /// <summary>
        ///     流参数名称
        /// </summary>
        public string StreamParameterName
        {
            get { return ParameterName; }
        }
    }
}