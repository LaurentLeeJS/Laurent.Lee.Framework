using Laurent.Lee.CLB.Code.CSharp;
using Laurent.Lee.CLB.Diagnostics;
using Laurent.Lee.CLB.Emit;
using Laurent.Lee.CLB.IO;
using Laurent.Lee.CLB.MemoryDataBase;
using Laurent.Lee.CLB.Net.Tcp;
using Laurent.Lee.CLB.Net.Tcp.Http;
using Laurent.Lee.CLB.Threading;

///本文件由程序自动生成,请不要自行修改

using System;

#pragma warning disable

namespace Laurent.Lee.CLB.Net.Tcp
{
    public partial class TmphTcpRegister : TmphTcpServer.TmphITcpServer
    {
        internal static class TmphTcpServer
        {
            public static bool verify(TmphTcpRegister _value_, string value)
            {
                return _value_.verify(value);
            }

            public static TmphRegisterResult register(TmphTcpRegister _value_, TmphClientId client, TmphService service)
            {
                return _value_.register(client, service);
            }

            public static void poll(TmphTcpRegister _value_, TmphClientId client,
                Func<TmphAsynchronousMethod.TmphReturnValue<TmphPollResult>, bool> onRegisterChanged)
            {
                _value_.poll(client, onRegisterChanged);
            }

            public static void removeRegister(TmphTcpRegister _value_, TmphClientId client, string serviceName)
            {
                _value_.removeRegister(client, serviceName);
            }

            public static TmphServices[] getServices(TmphTcpRegister _value_, out int version)
            {
                return _value_.getServices(out version);
            }

            public static TmphClientId register(TmphTcpRegister _value_)
            {
                return _value_.register();
            }

            public static void removeRegister(TmphTcpRegister _value_, TmphClientId client)
            {
                _value_.removeRegister(client);
            }
        }
    }
}

namespace Laurent.Lee.CLB.TcpServer
{
    /// <summary>
    ///     TCP服务
    /// </summary>
    public partial class TmphTcpRegister : TmphCommandServer
    {
        /// <summary>
        ///     TCP服务目标对象
        /// </summary>
        private readonly Net.Tcp.TmphTcpRegister _value_;

        /// <summary>
        ///     TCP调用服务端
        /// </summary>
        public TmphTcpRegister() : this(null, null)
        {
        }

        /// <summary>
        ///     TCP调用服务端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verify">TCP验证实例</param>
        public TmphTcpRegister(TmphTcpServer attribute, Net.Tcp.TmphTcpRegister value)
            : base(attribute ?? TmphTcpServer.GetConfig("tcpRegister", typeof(Net.Tcp.TmphTcpRegister)))
        {
            _value_ = value ?? new Net.Tcp.TmphTcpRegister();
            _value_.SetTcpServer(this);
            setCommands(7);
            identityOnCommands[verifyCommandIdentity = 0 + 128].Set(_M0, 1024);
            identityOnCommands[1 + 128].Set(_M1);
            identityOnCommands[2 + 128].Set(_M2);
            identityOnCommands[3 + 128].Set(_M3);
            identityOnCommands[4 + 128].Set(_M4);
            identityOnCommands[5 + 128].Set(_M5, 0);
            identityOnCommands[6 + 128].Set(_M6);
        }

        private void _M0(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i0();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    _s0 /**/.Call(socket, _value_, socket.Identity, inputParameter)();
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M1(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i1();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    TmphTask.Tiny.Add(_s1 /**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M2(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i2();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    var callbackReturn = TmphSocket.TmphCallback<_o2, Net.Tcp.TmphTcpRegister.TmphPollResult>.GetKeep(socket,
                        new _o2());
                    if (callbackReturn != null)
                    {
                        Net.Tcp.TmphTcpRegister.TmphTcpServer.poll(_value_, inputParameter.client, callbackReturn);
                        return;
                    }
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M3(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i3();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    TmphTask.Tiny.Add(_s3 /**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M4(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i4();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    TmphTask.Tiny.Add(_s4 /**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M5(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                {
                    _s5 /**/.Call(socket, _value_, socket.Identity)();
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M6(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i6();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    TmphTask.Tiny.Add(_s6 /**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i0
        {
            public string value;
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _o0 : TmphAsynchronousMethod.IReturnParameter<bool>
        {
            public bool Ret;

            public bool Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        private sealed class _s0 : TmphServerCall<_s0, Net.Tcp.TmphTcpRegister, _i0>
        {
            private TmphAsynchronousMethod.TmphReturnValue<_o0> get()
            {
                try
                {
                    var Return =
                        Net.Tcp.TmphTcpRegister.TmphTcpServer.verify(serverValue, inputParameter.value);
                    if (Return) socket.IsVerifyMethod = true;
                    return new _o0
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue<_o0> { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s0>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i1
        {
            public Net.Tcp.TmphTcpRegister.TmphClientId client;
            public Net.Tcp.TmphTcpRegister.TmphService service;
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _o1 : TmphAsynchronousMethod.IReturnParameter<Net.Tcp.TmphTcpRegister.TmphRegisterResult>
        {
            public Net.Tcp.TmphTcpRegister.TmphRegisterResult Ret;

            public Net.Tcp.TmphTcpRegister.TmphRegisterResult Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        private sealed class _s1 : TmphServerCall<_s1, Net.Tcp.TmphTcpRegister, _i1>
        {
            private TmphAsynchronousMethod.TmphReturnValue<_o1> get()
            {
                try
                {
                    var Return =
                        Net.Tcp.TmphTcpRegister.TmphTcpServer.register(serverValue, inputParameter.client,
                            inputParameter.service);
                    return new _o1
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue<_o1> { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s1>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i2
        {
            public Net.Tcp.TmphTcpRegister.TmphClientId client;
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _o2 : TmphAsynchronousMethod.IReturnParameter<Net.Tcp.TmphTcpRegister.TmphPollResult>
        {
            public Net.Tcp.TmphTcpRegister.TmphPollResult Ret;

            public Net.Tcp.TmphTcpRegister.TmphPollResult Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i3
        {
            public Net.Tcp.TmphTcpRegister.TmphClientId client;
            public string serviceName;
        }

        private sealed class _s3 : TmphServerCall<_s3, Net.Tcp.TmphTcpRegister, _i3>
        {
            private TmphAsynchronousMethod.TmphReturnValue get()
            {
                try
                {
                    Net.Tcp.TmphTcpRegister.TmphTcpServer.removeRegister(serverValue, inputParameter.client,
                        inputParameter.serviceName);
                    return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = true };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s3>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i4
        {
            public int version;
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _o4 : TmphAsynchronousMethod.IReturnParameter<Net.Tcp.TmphTcpRegister.TmphServices[]>
        {
            public Net.Tcp.TmphTcpRegister.TmphServices[] Ret;
            public int version;

            public Net.Tcp.TmphTcpRegister.TmphServices[] Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        private sealed class _s4 : TmphServerCall<_s4, Net.Tcp.TmphTcpRegister, _i4>
        {
            private TmphAsynchronousMethod.TmphReturnValue<_o4> get()
            {
                try
                {
                    var Return =
                        Net.Tcp.TmphTcpRegister.TmphTcpServer.getServices(serverValue, out inputParameter.version);
                    return new _o4
                    {
                        version = inputParameter.version,
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue<_o4> { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s4>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _o5 : TmphAsynchronousMethod.IReturnParameter<Net.Tcp.TmphTcpRegister.TmphClientId>
        {
            public Net.Tcp.TmphTcpRegister.TmphClientId Ret;

            public Net.Tcp.TmphTcpRegister.TmphClientId Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        private sealed class _s5 : TmphServerCall<_s5, Net.Tcp.TmphTcpRegister>
        {
            private TmphAsynchronousMethod.TmphReturnValue<_o5> get()
            {
                try
                {
                    var Return =
                        Net.Tcp.TmphTcpRegister.TmphTcpServer.register(serverValue);
                    return new _o5
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue<_o5> { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s5>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i6
        {
            public Net.Tcp.TmphTcpRegister.TmphClientId client;
        }

        private sealed class _s6 : TmphServerCall<_s6, Net.Tcp.TmphTcpRegister, _i6>
        {
            private TmphAsynchronousMethod.TmphReturnValue get()
            {
                try
                {
                    Net.Tcp.TmphTcpRegister.TmphTcpServer.removeRegister(serverValue, inputParameter.client);
                    return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = true };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s6>.Push(this);
            }
        }
    }
}

namespace Laurent.Lee.CLB.TcpClient
{
    public class TmphTcpRegister : IDisposable
    {
        /// <summary>
        ///     TCP调用客户端
        /// </summary>
        public TmphTcpRegister() : this(null, null)
        {
        }

        /// <summary>
        ///     TCP调用客户端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verifyMethod">TCP验证方法</param>
        public TmphTcpRegister(TmphTcpServer attribute, TmphTcpBase.ITcpClientVerifyMethod<TmphTcpRegister> verifyMethod)
        {
            _TcpClient_ =
                new TmphCommandClient<TmphTcpRegister>(
                    attribute ?? TmphTcpServer.GetConfig("TmphTcpRegister", typeof(Net.Tcp.TmphTcpRegister)), 24,
                    verifyMethod ?? new TmphVerifyMethod(), this);
        }

        /// <summary>
        ///     TCP调用客户端
        /// </summary>
        public TmphCommandClient<TmphTcpRegister> _TcpClient_ { get; private set; }

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            var client = _TcpClient_;
            _TcpClient_ = null;
            TmphPub.Dispose(ref client);
        }

        public TmphAsynchronousMethod.TmphReturnValue<bool> verify(string value)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall<TcpServer.TmphTcpRegister._o0>.Get();
            if (_wait_ != null)
            {
                verify(value, _wait_.OnReturn, false);
                var _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {
                    var _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new TmphAsynchronousMethod.TmphReturnValue<bool> { IsReturn = false };
        }

        private void verify(string value,
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphTcpRegister._o0>> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.VerifyStreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphTcpRegister._i0
                    {
                        value = value
                    };

                    var _outputParameter_ = new TcpServer.TmphTcpRegister._o0();
                    _socket_.Get(_onReturn_, 0 + 128, _inputParameter_, 1024, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null)
                _onReturn_(new TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphTcpRegister._o0> { IsReturn = false });
        }

        public TmphAsynchronousMethod.TmphReturnValue<Net.Tcp.TmphTcpRegister.TmphRegisterResult> register(
            Net.Tcp.TmphTcpRegister.TmphClientId client, Net.Tcp.TmphTcpRegister.TmphService service)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall<TcpServer.TmphTcpRegister._o1>.Get();
            if (_wait_ != null)
            {
                register(client, service, _wait_.OnReturn, false);
                var _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {
                    var _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new TmphAsynchronousMethod.TmphReturnValue<Net.Tcp.TmphTcpRegister.TmphRegisterResult> { IsReturn = false };
        }

        private void register(Net.Tcp.TmphTcpRegister.TmphClientId client, Net.Tcp.TmphTcpRegister.TmphService service,
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphTcpRegister._o1>> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphTcpRegister._i1
                    {
                        client = client,
                        service = service
                    };

                    var _outputParameter_ = new TcpServer.TmphTcpRegister._o1();
                    _socket_.Get(_onReturn_, 1 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null)
                _onReturn_(new TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphTcpRegister._o1> { IsReturn = false });
        }

        public TmphCommandClient.TmphStreamCommandSocket.TmphKeepCallback poll(Net.Tcp.TmphTcpRegister.TmphClientId client,
            Action<TmphAsynchronousMethod.TmphReturnValue<Net.Tcp.TmphTcpRegister.TmphPollResult>> _onReturn_)
        {
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphTcpRegister._o2>> _onOutput_ = null;
            _onOutput_ =
                TmphAsynchronousMethod.TmphCallReturn<Net.Tcp.TmphTcpRegister.TmphPollResult, TcpServer.TmphTcpRegister._o2>.Get(
                    _onReturn_);
            if (_onReturn_ == null || _onOutput_ != null)
            {
                return poll(client, _onOutput_, true);
            }
            return null;
        }

        private TmphCommandClient.TmphStreamCommandSocket.TmphKeepCallback poll(Net.Tcp.TmphTcpRegister.TmphClientId client,
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphTcpRegister._o2>> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphTcpRegister._i2
                    {
                        client = client
                    };

                    var _outputParameter_ = new TcpServer.TmphTcpRegister._o2();

                    return _socket_.Get(_onReturn_, 2 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_,
                        true);
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null)
                _onReturn_(new TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphTcpRegister._o2> { IsReturn = false });
            return null;
        }

        public TmphAsynchronousMethod.TmphReturnValue removeRegister(Net.Tcp.TmphTcpRegister.TmphClientId client,
            string serviceName)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall.Get();
            if (_wait_ != null)
            {
                removeRegister(client, serviceName, _wait_.OnReturn, false);
                return _wait_.Value;
            }
            return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false };
        }

        public void removeRegister(Net.Tcp.TmphTcpRegister.TmphClientId client, string serviceName, Action<bool> _onReturn_)
        {
            removeRegister(client, serviceName, _onReturn_, true);
        }

        private void removeRegister(Net.Tcp.TmphTcpRegister.TmphClientId client, string serviceName, Action<bool> _onReturn_,
            bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphTcpRegister._i3
                    {
                        client = client,
                        serviceName = serviceName
                    };
                    _socket_.Call(_onReturn_, 3 + 128, _inputParameter_, 2147483647, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(false);
        }

        public TmphAsynchronousMethod.TmphReturnValue<Net.Tcp.TmphTcpRegister.TmphServices[]> getServices(out int version)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall<TcpServer.TmphTcpRegister._o4>.Get();
            if (_wait_ != null)
            {
                getServices(out version, _wait_.OnReturn, false);
                var _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {
                    var _outputParameterValue_ = _outputParameter_.Value;
                    version = _outputParameterValue_.version;
                    return _outputParameterValue_.Return;
                }
            }
            version = default(int);
            return new TmphAsynchronousMethod.TmphReturnValue<Net.Tcp.TmphTcpRegister.TmphServices[]> { IsReturn = false };
        }

        private void getServices(out int version,
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphTcpRegister._o4>> _onReturn_, bool _isTask_)
        {
            version = default(int);
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphTcpRegister._i4();

                    var _outputParameter_ = new TcpServer.TmphTcpRegister._o4();
                    _socket_.Get(_onReturn_, 4 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null)
                _onReturn_(new TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphTcpRegister._o4> { IsReturn = false });
        }

        public TmphAsynchronousMethod.TmphReturnValue<Net.Tcp.TmphTcpRegister.TmphClientId> register()
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall<TcpServer.TmphTcpRegister._o5>.Get();
            if (_wait_ != null)
            {
                register(_wait_.OnReturn, false);
                var _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {
                    var _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new TmphAsynchronousMethod.TmphReturnValue<Net.Tcp.TmphTcpRegister.TmphClientId> { IsReturn = false };
        }

        private void register(Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphTcpRegister._o5>> _onReturn_,
            bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _outputParameter_ = new TcpServer.TmphTcpRegister._o5();
                    _socket_.Get(_onReturn_, 5 + 128, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null)
                _onReturn_(new TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphTcpRegister._o5> { IsReturn = false });
        }

        public TmphAsynchronousMethod.TmphReturnValue removeRegister(Net.Tcp.TmphTcpRegister.TmphClientId client)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall.Get();
            if (_wait_ != null)
            {
                removeRegister(client, _wait_.OnReturn, false);
                return _wait_.Value;
            }
            return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false };
        }

        public void removeRegister(Net.Tcp.TmphTcpRegister.TmphClientId client, Action<bool> _onReturn_)
        {
            removeRegister(client, _onReturn_, true);
        }

        private void removeRegister(Net.Tcp.TmphTcpRegister.TmphClientId client, Action<bool> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphTcpRegister._i6
                    {
                        client = client
                    };
                    _socket_.Call(_onReturn_, 6 + 128, _inputParameter_, 2147483647, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(false);
        }
    }
}

namespace Laurent.Lee.CLB.Net.Tcp.Http
{
    public partial class TmphServers : TmphTcpServer.TmphITcpServer
    {
        internal static class TmphTcpServer
        {
            public static bool verify(TmphServers _value_, string value)
            {
                return _value_.verify(value);
            }

            public static bool setForward(TmphServers _value_, TmphHost host)
            {
                return _value_.setForward(host);
            }

            public static void stop(TmphServers _value_, TmphDomain[] domains)
            {
                _value_.stop(domains);
            }

            public static void stop(TmphServers _value_, TmphDomain domain)
            {
                _value_.stop(domain);
            }

            public static TmphStartState start(TmphServers _value_, string assemblyPath, string serverType,
                TmphDomain[] domains, bool isShareAssembly)
            {
                return _value_.start(assemblyPath, serverType, domains, isShareAssembly);
            }

            public static TmphStartState start(TmphServers _value_, string assemblyPath, string serverType, TmphDomain domain,
                bool isShareAssembly)
            {
                return _value_.start(assemblyPath, serverType, domain, isShareAssembly);
            }

            public static void removeForward(TmphServers _value_)
            {
                _value_.removeForward();
            }
        }
    }
}

namespace Laurent.Lee.CLB.TcpServer
{
    /// <summary>
    ///     TCP服务
    /// </summary>
    public partial class TmphHttpServer : TmphCommandServer
    {
        /// <summary>
        ///     TCP服务目标对象
        /// </summary>
        private readonly TmphServers _value_;

        /// <summary>
        ///     TCP调用服务端
        /// </summary>
        public TmphHttpServer() : this(null, null)
        {
        }

        /// <summary>
        ///     TCP调用服务端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verify">TCP验证实例</param>
        public TmphHttpServer(TmphTcpServer attribute, TmphServers value)
            : base(attribute ?? TmphTcpServer.GetConfig("httpServer", typeof(TmphServers)))
        {
            _value_ = value ?? new TmphServers();
            _value_.SetTcpServer(this);
            setCommands(7);
            identityOnCommands[verifyCommandIdentity = 0 + 128].Set(_M0, 1024);
            identityOnCommands[1 + 128].Set(_M1);
            identityOnCommands[2 + 128].Set(_M2);
            identityOnCommands[3 + 128].Set(_M3);
            identityOnCommands[4 + 128].Set(_M4);
            identityOnCommands[5 + 128].Set(_M5);
            identityOnCommands[6 + 128].Set(_M6, 0);
        }

        private void _M0(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i0();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    _s0 /**/.Call(socket, _value_, socket.Identity, inputParameter)();
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M1(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i1();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    _s1 /**/.Call(socket, _value_, socket.Identity, inputParameter)();
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M2(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i2();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    TmphTask.Tiny.Add(_s2 /**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M3(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i3();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    TmphTask.Tiny.Add(_s3 /**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M4(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i4();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    TmphTask.Tiny.Add(_s4 /**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M5(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i5();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    TmphTask.Tiny.Add(_s5 /**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M6(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                {
                    _s6 /**/.Call(socket, _value_, socket.Identity)();
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i0
        {
            public string value;
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _o0 : TmphAsynchronousMethod.IReturnParameter<bool>
        {
            public bool Ret;

            public bool Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        private sealed class _s0 : TmphServerCall<_s0, TmphServers, _i0>
        {
            private TmphAsynchronousMethod.TmphReturnValue<_o0> get()
            {
                try
                {
                    var Return =
                        TmphServers.TmphTcpServer.verify(serverValue, inputParameter.value);
                    if (Return) socket.IsVerifyMethod = true;
                    return new _o0
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue<_o0> { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s0>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i1
        {
            public TmphHost host;
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _o1 : TmphAsynchronousMethod.IReturnParameter<bool>
        {
            public bool Ret;

            public bool Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        private sealed class _s1 : TmphServerCall<_s1, TmphServers, _i1>
        {
            private TmphAsynchronousMethod.TmphReturnValue<_o1> get()
            {
                try
                {
                    var Return =
                        TmphServers.TmphTcpServer.setForward(serverValue, inputParameter.host);
                    return new _o1
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue<_o1> { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s1>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i2
        {
            public TmphDomain[] domains;
        }

        private sealed class _s2 : TmphServerCall<_s2, TmphServers, _i2>
        {
            private TmphAsynchronousMethod.TmphReturnValue get()
            {
                try
                {
                    TmphServers.TmphTcpServer.stop(serverValue, inputParameter.domains);
                    return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = true };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s2>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i3
        {
            public TmphDomain domain;
        }

        private sealed class _s3 : TmphServerCall<_s3, TmphServers, _i3>
        {
            private TmphAsynchronousMethod.TmphReturnValue get()
            {
                try
                {
                    TmphServers.TmphTcpServer.stop(serverValue, inputParameter.domain);
                    return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = true };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s3>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i4
        {
            public string assemblyPath;
            public TmphDomain[] domains;
            public bool isShareAssembly;
            public string serverType;
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _o4 : TmphAsynchronousMethod.IReturnParameter<TmphServers.TmphStartState>
        {
            public TmphServers.TmphStartState Ret;

            public TmphServers.TmphStartState Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        private sealed class _s4 : TmphServerCall<_s4, TmphServers, _i4>
        {
            private TmphAsynchronousMethod.TmphReturnValue<_o4> get()
            {
                try
                {
                    var Return =
                        TmphServers.TmphTcpServer.start(serverValue, inputParameter.assemblyPath, inputParameter.serverType,
                            inputParameter.domains, inputParameter.isShareAssembly);
                    return new _o4
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue<_o4> { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s4>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i5
        {
            public string assemblyPath;
            public TmphDomain domain;
            public bool isShareAssembly;
            public string serverType;
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _o5 : TmphAsynchronousMethod.IReturnParameter<TmphServers.TmphStartState>
        {
            public TmphServers.TmphStartState Ret;

            public TmphServers.TmphStartState Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        private sealed class _s5 : TmphServerCall<_s5, TmphServers, _i5>
        {
            private TmphAsynchronousMethod.TmphReturnValue<_o5> get()
            {
                try
                {
                    var Return =
                        TmphServers.TmphTcpServer.start(serverValue, inputParameter.assemblyPath, inputParameter.serverType,
                            inputParameter.domain, inputParameter.isShareAssembly);
                    return new _o5
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue<_o5> { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s5>.Push(this);
            }
        }

        private sealed class _s6 : TmphServerCall<_s6, TmphServers>
        {
            private TmphAsynchronousMethod.TmphReturnValue get()
            {
                try
                {
                    TmphServers.TmphTcpServer.removeForward(serverValue);
                    return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = true };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s6>.Push(this);
            }
        }
    }
}

namespace Laurent.Lee.CLB.TcpClient
{
    public class TmphHttpServer : IDisposable
    {
        /// <summary>
        ///     TCP调用客户端
        /// </summary>
        public TmphHttpServer() : this(null, null)
        {
        }

        /// <summary>
        ///     TCP调用客户端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verifyMethod">TCP验证方法</param>
        public TmphHttpServer(TmphTcpServer attribute, TmphTcpBase.ITcpClientVerifyMethod<TmphHttpServer> verifyMethod)
        {
            _TcpClient_ =
                new TmphCommandClient<TmphHttpServer>(attribute ?? TmphTcpServer.GetConfig("httpServer", typeof(TmphServers)), 24, verifyMethod ?? new TmphVerifyMethod(), this);
        }

        /// <summary>
        ///     TCP调用客户端
        /// </summary>
        public TmphCommandClient<TmphHttpServer> _TcpClient_ { get; private set; }

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            var client = _TcpClient_;
            _TcpClient_ = null;
            TmphPub.Dispose(ref client);
        }

        public TmphAsynchronousMethod.TmphReturnValue<bool> verify(string value)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall<TcpServer.TmphHttpServer._o0>.Get();
            if (_wait_ != null)
            {
                verify(value, _wait_.OnReturn, false);
                var _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {
                    var _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new TmphAsynchronousMethod.TmphReturnValue<bool> { IsReturn = false };
        }

        private void verify(string value,
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphHttpServer._o0>> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.VerifyStreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphHttpServer._i0
                    {
                        value = value
                    };

                    var _outputParameter_ = new TcpServer.TmphHttpServer._o0();
                    _socket_.Get(_onReturn_, 0 + 128, _inputParameter_, 1024, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null)
                _onReturn_(new TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphHttpServer._o0> { IsReturn = false });
        }

        public TmphAsynchronousMethod.TmphReturnValue<bool> setForward(TmphHost host)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall<TcpServer.TmphHttpServer._o1>.Get();
            if (_wait_ != null)
            {
                setForward(host, _wait_.OnReturn, false);
                var _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {
                    var _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new TmphAsynchronousMethod.TmphReturnValue<bool> { IsReturn = false };
        }

        private void setForward(TmphHost host,
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphHttpServer._o1>> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphHttpServer._i1
                    {
                        host = host
                    };

                    var _outputParameter_ = new TcpServer.TmphHttpServer._o1();
                    _socket_.Get(_onReturn_, 1 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null)
                _onReturn_(new TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphHttpServer._o1> { IsReturn = false });
        }

        public TmphAsynchronousMethod.TmphReturnValue stop(TmphDomain[] domains)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall.Get();
            if (_wait_ != null)
            {
                stop(domains, _wait_.OnReturn, false);
                return _wait_.Value;
            }
            return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false };
        }

        public void stop(TmphDomain[] domains, Action<bool> _onReturn_)
        {
            stop(domains, _onReturn_, true);
        }

        private void stop(TmphDomain[] domains, Action<bool> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphHttpServer._i2
                    {
                        domains = domains
                    };
                    _socket_.Call(_onReturn_, 2 + 128, _inputParameter_, 2147483647, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(false);
        }

        public TmphAsynchronousMethod.TmphReturnValue stop(TmphDomain domain)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall.Get();
            if (_wait_ != null)
            {
                stop(domain, _wait_.OnReturn, false);
                return _wait_.Value;
            }
            return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false };
        }

        public void stop(TmphDomain domain, Action<bool> _onReturn_)
        {
            stop(domain, _onReturn_, true);
        }

        private void stop(TmphDomain domain, Action<bool> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphHttpServer._i3
                    {
                        domain = domain
                    };
                    _socket_.Call(_onReturn_, 3 + 128, _inputParameter_, 2147483647, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(false);
        }

        public TmphAsynchronousMethod.TmphReturnValue<TmphServers.TmphStartState> start(string assemblyPath, string serverType,
            TmphDomain[] domains, bool isShareAssembly)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall<TcpServer.TmphHttpServer._o4>.Get();
            if (_wait_ != null)
            {
                start(assemblyPath, serverType, domains, isShareAssembly, _wait_.OnReturn, false);
                var _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {
                    var _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new TmphAsynchronousMethod.TmphReturnValue<TmphServers.TmphStartState> { IsReturn = false };
        }

        private void start(string assemblyPath, string serverType, TmphDomain[] domains, bool isShareAssembly,
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphHttpServer._o4>> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphHttpServer._i4
                    {
                        assemblyPath = assemblyPath,
                        serverType = serverType,
                        domains = domains,
                        isShareAssembly = isShareAssembly
                    };

                    var _outputParameter_ = new TcpServer.TmphHttpServer._o4();
                    _socket_.Get(_onReturn_, 4 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null)
                _onReturn_(new TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphHttpServer._o4> { IsReturn = false });
        }

        public TmphAsynchronousMethod.TmphReturnValue<TmphServers.TmphStartState> start(string assemblyPath, string serverType,
            TmphDomain domain, bool isShareAssembly)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall<TcpServer.TmphHttpServer._o5>.Get();
            if (_wait_ != null)
            {
                start(assemblyPath, serverType, domain, isShareAssembly, _wait_.OnReturn, false);
                var _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {
                    var _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new TmphAsynchronousMethod.TmphReturnValue<TmphServers.TmphStartState> { IsReturn = false };
        }

        private void start(string assemblyPath, string serverType, TmphDomain domain, bool isShareAssembly,
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphHttpServer._o5>> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphHttpServer._i5
                    {
                        assemblyPath = assemblyPath,
                        serverType = serverType,
                        domain = domain,
                        isShareAssembly = isShareAssembly
                    };

                    var _outputParameter_ = new TcpServer.TmphHttpServer._o5();
                    _socket_.Get(_onReturn_, 5 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null)
                _onReturn_(new TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphHttpServer._o5> { IsReturn = false });
        }

        public TmphAsynchronousMethod.TmphReturnValue removeForward()
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall.Get();
            if (_wait_ != null)
            {
                removeForward(_wait_.OnReturn, false);
                return _wait_.Value;
            }
            return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false };
        }

        public void removeForward(Action<bool> _onReturn_)
        {
            removeForward(_onReturn_, true);
        }

        private void removeForward(Action<bool> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    _socket_.Call(_onReturn_, 6 + 128, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(false);
        }
    }
}

namespace Laurent.Lee.CLB.Diagnostics
{
    public partial class TmphProcessCopyServer : TmphTcpServer.TmphITcpServer
    {
        internal static class TmphTcpServer
        {
            public static bool verify(TmphProcessCopyServer _value_, string value)
            {
                return _value_.Verify(value);
            }

            public static void guard(TmphProcessCopyServer _value_, TmphCopyer copyer)
            {
                _value_.Guard(copyer);
            }

            public static void copyStart(TmphProcessCopyServer _value_, TmphCopyer copyer)
            {
                _value_.copyStart(copyer);
            }

            public static void remove(TmphProcessCopyServer _value_, TmphCopyer copyer)
            {
                _value_.Remove(copyer);
            }
        }
    }
}

namespace Laurent.Lee.CLB.TcpServer
{
    /// <summary>
    ///     TCP服务
    /// </summary>
    public partial class TmphProcessCopy : TmphCommandServer
    {
        /// <summary>
        ///     TCP服务目标对象
        /// </summary>
        private readonly TmphProcessCopyServer _value_;

        /// <summary>
        ///     TCP调用服务端
        /// </summary>
        public TmphProcessCopy() : this(null, null)
        {
        }

        /// <summary>
        ///     TCP调用服务端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verify">TCP验证实例</param>
        public TmphProcessCopy(TmphTcpServer attribute, TmphProcessCopyServer value)
            : base(attribute ?? TmphTcpServer.GetConfig("processCopy", typeof(TmphProcessCopyServer)))
        {
            _value_ = value ?? new TmphProcessCopyServer();
            _value_.SetTcpServer(this);
            setCommands(4);
            identityOnCommands[verifyCommandIdentity = 0 + 128].Set(_M0, 1024);
            identityOnCommands[1 + 128].Set(_M1);
            identityOnCommands[2 + 128].Set(_M2);
            identityOnCommands[3 + 128].Set(_M3);
        }

        private void _M0(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i0();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    _s0 /**/.Call(socket, _value_, socket.Identity, inputParameter)();
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M1(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i1();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    TmphTask.Tiny.Add(_s1 /**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M2(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i2();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    TmphTask.Tiny.Add(_s2 /**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M3(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i3();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    TmphTask.Tiny.Add(_s3 /**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i0
        {
            public string value;
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _o0 : TmphAsynchronousMethod.IReturnParameter<bool>
        {
            public bool Ret;

            public bool Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        private sealed class _s0 : TmphServerCall<_s0, TmphProcessCopyServer, _i0>
        {
            private TmphAsynchronousMethod.TmphReturnValue<_o0> get()
            {
                try
                {
                    var Return =
                        TmphProcessCopyServer.TmphTcpServer.verify(serverValue, inputParameter.value);
                    if (Return) socket.IsVerifyMethod = true;
                    return new _o0
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue<_o0> { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s0>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i1
        {
            public TmphProcessCopyServer.TmphCopyer copyer;
        }

        private sealed class _s1 : TmphServerCall<_s1, TmphProcessCopyServer, _i1>
        {
            private TmphAsynchronousMethod.TmphReturnValue get()
            {
                try
                {
                    TmphProcessCopyServer.TmphTcpServer.guard(serverValue, inputParameter.copyer);
                    return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = true };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s1>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i2
        {
            public TmphProcessCopyServer.TmphCopyer copyer;
        }

        private sealed class _s2 : TmphServerCall<_s2, TmphProcessCopyServer, _i2>
        {
            private TmphAsynchronousMethod.TmphReturnValue get()
            {
                try
                {
                    TmphProcessCopyServer.TmphTcpServer.copyStart(serverValue, inputParameter.copyer);
                    return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = true };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s2>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i3
        {
            public TmphProcessCopyServer.TmphCopyer copyer;
        }

        private sealed class _s3 : TmphServerCall<_s3, TmphProcessCopyServer, _i3>
        {
            private TmphAsynchronousMethod.TmphReturnValue get()
            {
                try
                {
                    TmphProcessCopyServer.TmphTcpServer.remove(serverValue, inputParameter.copyer);
                    return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = true };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s3>.Push(this);
            }
        }
    }
}

namespace Laurent.Lee.CLB.TcpClient
{
    public class TmphProcessCopy : IDisposable
    {
        /// <summary>
        ///     TCP调用客户端
        /// </summary>
        public TmphProcessCopy() : this(null, null)
        {
        }

        /// <summary>
        ///     TCP调用客户端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verifyMethod">TCP验证方法</param>
        public TmphProcessCopy(TmphTcpServer attribute, TmphTcpBase.ITcpClientVerifyMethod<TmphProcessCopy> verifyMethod)
        {
            _TcpClient_ =
                new TmphCommandClient<TmphProcessCopy>(
                    attribute ?? TmphTcpServer.GetConfig("processCopy", typeof(TmphProcessCopyServer)), 24,
                    verifyMethod ?? new TmphVerifyMethod(), this);
        }

        /// <summary>
        ///     TCP调用客户端
        /// </summary>
        public TmphCommandClient<TmphProcessCopy> _TcpClient_ { get; private set; }

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            var client = _TcpClient_;
            _TcpClient_ = null;
            TmphPub.Dispose(ref client);
        }

        public TmphAsynchronousMethod.TmphReturnValue<bool> verify(string value)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall<TcpServer.TmphProcessCopy._o0>.Get();
            if (_wait_ != null)
            {
                verify(value, _wait_.OnReturn, false);
                var _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {
                    var _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new TmphAsynchronousMethod.TmphReturnValue<bool> { IsReturn = false };
        }

        private void verify(string value,
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphProcessCopy._o0>> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.VerifyStreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphProcessCopy._i0
                    {
                        value = value
                    };

                    var _outputParameter_ = new TcpServer.TmphProcessCopy._o0();
                    _socket_.Get(_onReturn_, 0 + 128, _inputParameter_, 1024, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null)
                _onReturn_(new TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphProcessCopy._o0> { IsReturn = false });
        }

        public TmphAsynchronousMethod.TmphReturnValue guard(TmphProcessCopyServer.TmphCopyer copyer)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall.Get();
            if (_wait_ != null)
            {
                guard(copyer, _wait_.OnReturn, false);
                return _wait_.Value;
            }
            return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false };
        }

        public void guard(TmphProcessCopyServer.TmphCopyer copyer, Action<bool> _onReturn_)
        {
            guard(copyer, _onReturn_, true);
        }

        private void guard(TmphProcessCopyServer.TmphCopyer copyer, Action<bool> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphProcessCopy._i1
                    {
                        copyer = copyer
                    };
                    _socket_.Call(_onReturn_, 1 + 128, _inputParameter_, 2147483647, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(false);
        }

        public TmphAsynchronousMethod.TmphReturnValue copyStart(TmphProcessCopyServer.TmphCopyer copyer)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall.Get();
            if (_wait_ != null)
            {
                copyStart(copyer, _wait_.OnReturn, false);
                return _wait_.Value;
            }
            return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false };
        }

        public void copyStart(TmphProcessCopyServer.TmphCopyer copyer, Action<bool> _onReturn_)
        {
            copyStart(copyer, _onReturn_, true);
        }

        private void copyStart(TmphProcessCopyServer.TmphCopyer copyer, Action<bool> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphProcessCopy._i2
                    {
                        copyer = copyer
                    };
                    _socket_.Call(_onReturn_, 2 + 128, _inputParameter_, 2147483647, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(false);
        }

        public TmphAsynchronousMethod.TmphReturnValue remove(TmphProcessCopyServer.TmphCopyer copyer)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall.Get();
            if (_wait_ != null)
            {
                remove(copyer, _wait_.OnReturn, false);
                return _wait_.Value;
            }
            return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false };
        }

        public void remove(TmphProcessCopyServer.TmphCopyer copyer, Action<bool> _onReturn_)
        {
            remove(copyer, _onReturn_, true);
        }

        private void remove(TmphProcessCopyServer.TmphCopyer copyer, Action<bool> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphProcessCopy._i3
                    {
                        copyer = copyer
                    };
                    _socket_.Call(_onReturn_, 3 + 128, _inputParameter_, 2147483647, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(false);
        }
    }
}

namespace Laurent.Lee.CLB.MemoryDataBase
{
    public partial class TmphPhysicalServer
    {
        internal static class TmphTcpServer
        {
            public static bool verify(TmphPhysicalServer _value_, string value)
            {
                return _value_.verify(value);
            }

            public static MemoryDataBase.TmphPhysicalServer.TmphPhysicalIdentity open(TmphPhysicalServer _value_, string fileName)
            {
                return _value_.open(fileName);
            }

            public static void close(TmphPhysicalServer _value_, MemoryDataBase.TmphPhysicalServer.TmphTimeIdentity identity)
            {
                _value_.close(identity);
            }

            public static bool create(TmphPhysicalServer _value_, TmphTcpBase.TmphSubByteUnmanagedStream stream)
            {
                return _value_.create(stream);
            }

            public static TmphTcpBase.TmphSubByteArrayBuffer load(TmphPhysicalServer _value_, MemoryDataBase.TmphPhysicalServer.TmphTimeIdentity identity)
            {
                return _value_.load(identity);
            }

            public static bool loaded(TmphPhysicalServer _value_, MemoryDataBase.TmphPhysicalServer.TmphTimeIdentity identity, bool isLoaded)
            {
                return _value_.loaded(identity, isLoaded);
            }

            public static int append(TmphPhysicalServer _value_, TmphTcpBase.TmphSubByteUnmanagedStream dataStream)
            {
                return _value_.append(dataStream);
            }

            public static void waitBuffer(TmphPhysicalServer _value_, MemoryDataBase.TmphPhysicalServer.TmphTimeIdentity identity)
            {
                _value_.waitBuffer(identity);
            }

            public static bool flush(TmphPhysicalServer _value_, MemoryDataBase.TmphPhysicalServer.TmphTimeIdentity identity)
            {
                return _value_.flush(identity);
            }

            public static bool flushFile(TmphPhysicalServer _value_, MemoryDataBase.TmphPhysicalServer.TmphTimeIdentity identity, bool isDiskFile)
            {
                return _value_.flushFile(identity, isDiskFile);
            }

            public static TmphTcpBase.TmphSubByteArrayBuffer loadHeader(TmphPhysicalServer _value_, MemoryDataBase.TmphPhysicalServer.TmphTimeIdentity identity)
            {
                return _value_.loadHeader(identity);
            }
        }
    }
}

namespace Laurent.Lee.CLB.TcpServer
{
    /// <summary>
    ///     TCP服务
    /// </summary>
    public partial class TmphMemoryDatabasePhysical : TmphCommandServer
    {
        /// <summary>
        ///     TCP服务目标对象
        /// </summary>
        private readonly TmphPhysicalServer _value_;

        /// <summary>
        ///     TCP调用服务端
        /// </summary>
        public TmphMemoryDatabasePhysical() : this(null, null)
        {
        }

        /// <summary>
        ///     TCP调用服务端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verify">TCP验证实例</param>
        public TmphMemoryDatabasePhysical(TmphTcpServer attribute, TmphPhysicalServer value)
            : base(attribute ?? TmphTcpServer.GetConfig("memoryDatabasePhysical", typeof(TmphPhysicalServer)))
        {
            _value_ = value ?? new TmphPhysicalServer();
            setCommands(11);
            identityOnCommands[verifyCommandIdentity = 0 + 128].Set(_M0, 1024);
            identityOnCommands[1 + 128].Set(_M1);
            identityOnCommands[2 + 128].Set(_M2);
            identityOnCommands[3 + 128].Set(_M3);
            identityOnCommands[4 + 128].Set(_M4);
            identityOnCommands[5 + 128].Set(_M5);
            identityOnCommands[6 + 128].Set(_M6);
            identityOnCommands[7 + 128].Set(_M7);
            identityOnCommands[8 + 128].Set(_M8);
            identityOnCommands[9 + 128].Set(_M9);
            identityOnCommands[10 + 128].Set(_M10);
        }

        private void _M0(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i0();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    _s0 /**/.Call(socket, _value_, socket.Identity, inputParameter)();
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M1(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i1();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    TmphTask.Tiny.Add(_s1 /**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M2(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i2();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    _s2 /**/.Call(socket, _value_, socket.Identity, inputParameter)();
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M3(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i3();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    _s3 /**/.Call(socket, _value_, socket.Identity, inputParameter)();
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M4(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i4();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    TmphTask.Tiny.Add(_s4 /**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M5(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i5();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    TmphTask.Tiny.Add(_s5 /**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M6(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i6();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    _s6 /**/.Call(socket, _value_, socket.Identity, inputParameter)();
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M7(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i7();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    TmphTask.Tiny.Add(_s7 /**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M8(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i8();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    _s8 /**/.Call(socket, _value_, socket.Identity, inputParameter)();
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M9(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i9();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    TmphTask.Tiny.Add(_s9 /**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M10(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i10();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    TmphTask.Tiny.Add(_s10 /**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i0
        {
            public string value;
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _o0 : TmphAsynchronousMethod.IReturnParameter<bool>
        {
            public bool Ret;

            public bool Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        private sealed class _s0 : TmphServerCall<_s0, TmphPhysicalServer, _i0>
        {
            private TmphAsynchronousMethod.TmphReturnValue<_o0> get()
            {
                try
                {
                    var Return =
                        TmphPhysicalServer.TmphTcpServer.verify(serverValue, inputParameter.value);
                    if (Return) socket.IsVerifyMethod = true;
                    return new _o0
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue<_o0> { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s0>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i1
        {
            public string fileName;
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _o1 : TmphAsynchronousMethod.IReturnParameter<TmphPhysicalServer.TmphPhysicalIdentity>
        {
            public TmphPhysicalServer.TmphPhysicalIdentity Ret;

            public TmphPhysicalServer.TmphPhysicalIdentity Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        private sealed class _s1 : TmphServerCall<_s1, TmphPhysicalServer, _i1>
        {
            private TmphAsynchronousMethod.TmphReturnValue<_o1> get()
            {
                try
                {
                    var Return =
                        TmphPhysicalServer.TmphTcpServer.open(serverValue, inputParameter.fileName);
                    return new _o1
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue<_o1> { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s1>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i2
        {
            public TmphPhysicalServer.TmphTimeIdentity identity;
        }

        private sealed class _s2 : TmphServerCall<_s2, TmphPhysicalServer, _i2>
        {
            private TmphAsynchronousMethod.TmphReturnValue get()
            {
                try
                {
                    TmphPhysicalServer.TmphTcpServer.close(serverValue, inputParameter.identity);
                    return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = true };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s2>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i3
        {
            public TmphTcpBase.TmphSubByteUnmanagedStream stream;
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _o3 : TmphAsynchronousMethod.IReturnParameter<bool>
        {
            public bool Ret;

            public bool Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        private sealed class _s3 : TmphServerCall<_s3, TmphPhysicalServer, _i3>
        {
            private TmphAsynchronousMethod.TmphReturnValue<_o3> get()
            {
                try
                {
                    var Return =
                        TmphPhysicalServer.TmphTcpServer.create(serverValue, inputParameter.stream);
                    return new _o3
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue<_o3> { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s3>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i4
        {
            public TmphPhysicalServer.TmphTimeIdentity identity;
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _o4 : TmphAsynchronousMethod.IReturnParameter<TmphTcpBase.TmphSubByteArrayBuffer>
        {
            public TmphTcpBase.TmphSubByteArrayBuffer Ret;

            public TmphTcpBase.TmphSubByteArrayBuffer Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        private sealed class _s4 : TmphServerCall<_s4, TmphPhysicalServer, _i4>
        {
            private TmphAsynchronousMethod.TmphReturnValue<_o4> get()
            {
                try
                {
                    var Return =
                        TmphPhysicalServer.TmphTcpServer.load(serverValue, inputParameter.identity);
                    return new _o4
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue<_o4> { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s4>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i5
        {
            public TmphPhysicalServer.TmphTimeIdentity identity;
            public bool isLoaded;
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _o5 : TmphAsynchronousMethod.IReturnParameter<bool>
        {
            public bool Ret;

            public bool Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        private sealed class _s5 : TmphServerCall<_s5, TmphPhysicalServer, _i5>
        {
            private TmphAsynchronousMethod.TmphReturnValue<_o5> get()
            {
                try
                {
                    var Return =
                        TmphPhysicalServer.TmphTcpServer.loaded(serverValue, inputParameter.identity,
                            inputParameter.isLoaded);
                    return new _o5
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue<_o5> { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s5>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i6
        {
            public TmphTcpBase.TmphSubByteUnmanagedStream dataStream;
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _o6 : TmphAsynchronousMethod.IReturnParameter<int>
        {
            public int Ret;

            public int Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        private sealed class _s6 : TmphServerCall<_s6, TmphPhysicalServer, _i6>
        {
            private TmphAsynchronousMethod.TmphReturnValue<_o6> get()
            {
                try
                {
                    var Return =
                        TmphPhysicalServer.TmphTcpServer.append(serverValue, inputParameter.dataStream);
                    return new _o6
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue<_o6> { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s6>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i7
        {
            public TmphPhysicalServer.TmphTimeIdentity identity;
        }

        private sealed class _s7 : TmphServerCall<_s7, TmphPhysicalServer, _i7>
        {
            private TmphAsynchronousMethod.TmphReturnValue get()
            {
                try
                {
                    TmphPhysicalServer.TmphTcpServer.waitBuffer(serverValue, inputParameter.identity);
                    return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = true };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s7>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i8
        {
            public TmphPhysicalServer.TmphTimeIdentity identity;
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _o8 : TmphAsynchronousMethod.IReturnParameter<bool>
        {
            public bool Ret;

            public bool Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        private sealed class _s8 : TmphServerCall<_s8, TmphPhysicalServer, _i8>
        {
            private TmphAsynchronousMethod.TmphReturnValue<_o8> get()
            {
                try
                {
                    var Return =
                        TmphPhysicalServer.TmphTcpServer.flush(serverValue, inputParameter.identity);
                    return new _o8
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue<_o8> { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s8>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i9
        {
            public TmphPhysicalServer.TmphTimeIdentity identity;
            public bool isDiskFile;
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _o9 : TmphAsynchronousMethod.IReturnParameter<bool>
        {
            public bool Ret;

            public bool Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        private sealed class _s9 : TmphServerCall<_s9, TmphPhysicalServer, _i9>
        {
            private TmphAsynchronousMethod.TmphReturnValue<_o9> get()
            {
                try
                {
                    var Return =
                        TmphPhysicalServer.TmphTcpServer.flushFile(serverValue, inputParameter.identity,
                            inputParameter.isDiskFile);
                    return new _o9
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue<_o9> { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s9>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i10
        {
            public TmphPhysicalServer.TmphTimeIdentity identity;
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _o10 : TmphAsynchronousMethod.IReturnParameter<TmphTcpBase.TmphSubByteArrayBuffer>
        {
            public TmphTcpBase.TmphSubByteArrayBuffer Ret;

            public TmphTcpBase.TmphSubByteArrayBuffer Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        private sealed class _s10 : TmphServerCall<_s10, TmphPhysicalServer, _i10>
        {
            private TmphAsynchronousMethod.TmphReturnValue<_o10> get()
            {
                try
                {
                    var Return =
                        TmphPhysicalServer.TmphTcpServer.loadHeader(serverValue, inputParameter.identity);
                    return new _o10
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue<_o10> { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s10>.Push(this);
            }
        }
    }
}

namespace Laurent.Lee.CLB.TcpClient
{
    public class TmphMemoryDatabasePhysical : IDisposable
    {
        /// <summary>
        ///     TCP调用客户端
        /// </summary>
        public TmphMemoryDatabasePhysical() : this(null, null)
        {
        }

        /// <summary>
        ///     TCP调用客户端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verifyMethod">TCP验证方法</param>
        public TmphMemoryDatabasePhysical(TmphTcpServer attribute,
            TmphTcpBase.ITcpClientVerifyMethod<TmphMemoryDatabasePhysical> verifyMethod)
        {
            _TcpClient_ =
                new TmphCommandClient<TmphMemoryDatabasePhysical>(
                    attribute ?? TmphTcpServer.GetConfig("memoryDatabasePhysical", typeof(TmphPhysicalServer)), 24,
                    verifyMethod ?? new TmphVerifyMethod(), this);
        }

        /// <summary>
        ///     TCP调用客户端
        /// </summary>
        public TmphCommandClient<TmphMemoryDatabasePhysical> _TcpClient_ { get; private set; }

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            var client = _TcpClient_;
            _TcpClient_ = null;
            TmphPub.Dispose(ref client);
        }

        public TmphAsynchronousMethod.TmphReturnValue<bool> verify(string value)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall<TcpServer.TmphMemoryDatabasePhysical._o0>.Get();
            if (_wait_ != null)
            {
                verify(value, _wait_.OnReturn, false);
                var _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {
                    var _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new TmphAsynchronousMethod.TmphReturnValue<bool> { IsReturn = false };
        }

        private void verify(string value,
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphMemoryDatabasePhysical._o0>> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.VerifyStreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphMemoryDatabasePhysical._i0
                    {
                        value = value
                    };

                    var _outputParameter_ = new TcpServer.TmphMemoryDatabasePhysical._o0();
                    _socket_.Get(_onReturn_, 0 + 128, _inputParameter_, 1024, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null)
                _onReturn_(new TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphMemoryDatabasePhysical._o0>
                {
                    IsReturn = false
                });
        }

        public void open(string fileName,
            Action<TmphAsynchronousMethod.TmphReturnValue<TmphPhysicalServer.TmphPhysicalIdentity>> _onReturn_)
        {
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphMemoryDatabasePhysical._o1>> _onOutput_ = null;
            _onOutput_ =
                TmphAsynchronousMethod
                    .TmphCallReturn<TmphPhysicalServer.TmphPhysicalIdentity, TcpServer.TmphMemoryDatabasePhysical._o1>.Get(
                        _onReturn_);
            if (_onReturn_ == null || _onOutput_ != null)
            {
                open(fileName, _onOutput_, true);
            }
        }

        private void open(string fileName,
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphMemoryDatabasePhysical._o1>> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphMemoryDatabasePhysical._i1
                    {
                        fileName = fileName
                    };

                    var _outputParameter_ = new TcpServer.TmphMemoryDatabasePhysical._o1();
                    _socket_.Get(_onReturn_, 1 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null)
                _onReturn_(new TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphMemoryDatabasePhysical._o1>
                {
                    IsReturn = false
                });
        }

        public TmphAsynchronousMethod.TmphReturnValue close(TmphPhysicalServer.TmphTimeIdentity identity)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall.Get();
            if (_wait_ != null)
            {
                close(identity, _wait_.OnReturn, false);
                return _wait_.Value;
            }
            return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false };
        }

        public void close(TmphPhysicalServer.TmphTimeIdentity identity, Action<bool> _onReturn_)
        {
            close(identity, _onReturn_, true);
        }

        private void close(TmphPhysicalServer.TmphTimeIdentity identity, Action<bool> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphMemoryDatabasePhysical._i2
                    {
                        identity = identity
                    };
                    _socket_.Call(_onReturn_, 2 + 128, _inputParameter_, 2147483647, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(false);
        }

        public TmphAsynchronousMethod.TmphReturnValue<bool> create(TmphTcpBase.TmphSubByteUnmanagedStream stream)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall<TcpServer.TmphMemoryDatabasePhysical._o3>.Get();
            if (_wait_ != null)
            {
                create(stream, _wait_.OnReturn, false);
                var _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {
                    var _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new TmphAsynchronousMethod.TmphReturnValue<bool> { IsReturn = false };
        }

        private void create(TmphTcpBase.TmphSubByteUnmanagedStream stream,
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphMemoryDatabasePhysical._o3>> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphMemoryDatabasePhysical._i3
                    {
                        stream = stream
                    };

                    var _outputParameter_ = new TcpServer.TmphMemoryDatabasePhysical._o3();
                    _socket_.Get(_onReturn_, 3 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null)
                _onReturn_(new TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphMemoryDatabasePhysical._o3>
                {
                    IsReturn = false
                });
        }

        public void load(TmphPhysicalServer.TmphTimeIdentity identity,
            Action<TmphAsynchronousMethod.TmphReturnValue<TmphTcpBase.TmphSubByteArrayBuffer>> _onReturn_)
        {
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphMemoryDatabasePhysical._o4>> _onOutput_ = null;
            _onOutput_ =
                TmphAsynchronousMethod.TmphCallReturn<TmphTcpBase.TmphSubByteArrayBuffer, TcpServer.TmphMemoryDatabasePhysical._o4>
                    .Get(_onReturn_);
            if (_onReturn_ == null || _onOutput_ != null)
            {
                load(identity, _onOutput_, false);
            }
        }

        private void load(TmphPhysicalServer.TmphTimeIdentity identity,
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphMemoryDatabasePhysical._o4>> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphMemoryDatabasePhysical._i4
                    {
                        identity = identity
                    };

                    var _outputParameter_ = new TcpServer.TmphMemoryDatabasePhysical._o4();
                    _socket_.Get(_onReturn_, 4 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null)
                _onReturn_(new TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphMemoryDatabasePhysical._o4>
                {
                    IsReturn = false
                });
        }

        public TmphAsynchronousMethod.TmphReturnValue<bool> loaded(TmphPhysicalServer.TmphTimeIdentity identity, bool isLoaded)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall<TcpServer.TmphMemoryDatabasePhysical._o5>.Get();
            if (_wait_ != null)
            {
                loaded(identity, isLoaded, _wait_.OnReturn, false);
                var _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {
                    var _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new TmphAsynchronousMethod.TmphReturnValue<bool> { IsReturn = false };
        }

        private void loaded(TmphPhysicalServer.TmphTimeIdentity identity, bool isLoaded,
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphMemoryDatabasePhysical._o5>> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphMemoryDatabasePhysical._i5
                    {
                        identity = identity,
                        isLoaded = isLoaded
                    };

                    var _outputParameter_ = new TcpServer.TmphMemoryDatabasePhysical._o5();
                    _socket_.Get(_onReturn_, 5 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null)
                _onReturn_(new TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphMemoryDatabasePhysical._o5>
                {
                    IsReturn = false
                });
        }

        public TmphAsynchronousMethod.TmphReturnValue<int> append(TmphTcpBase.TmphSubByteUnmanagedStream dataStream)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall<TcpServer.TmphMemoryDatabasePhysical._o6>.Get();
            if (_wait_ != null)
            {
                append(dataStream, _wait_.OnReturn, false);
                var _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {
                    var _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new TmphAsynchronousMethod.TmphReturnValue<int> { IsReturn = false };
        }

        public void append(TmphTcpBase.TmphSubByteUnmanagedStream dataStream,
            Action<TmphAsynchronousMethod.TmphReturnValue<int>> _onReturn_)
        {
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphMemoryDatabasePhysical._o6>> _onOutput_ = null;
            _onOutput_ = TmphAsynchronousMethod.TmphCallReturn<int, TcpServer.TmphMemoryDatabasePhysical._o6>.Get(_onReturn_);
            if (_onReturn_ == null || _onOutput_ != null)
            {
                append(dataStream, _onOutput_, false);
            }
        }

        private void append(TmphTcpBase.TmphSubByteUnmanagedStream dataStream,
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphMemoryDatabasePhysical._o6>> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphMemoryDatabasePhysical._i6
                    {
                        dataStream = dataStream
                    };

                    var _outputParameter_ = new TcpServer.TmphMemoryDatabasePhysical._o6();
                    _socket_.Get(_onReturn_, 6 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null)
                _onReturn_(new TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphMemoryDatabasePhysical._o6>
                {
                    IsReturn = false
                });
        }

        public TmphAsynchronousMethod.TmphReturnValue waitBuffer(TmphPhysicalServer.TmphTimeIdentity identity)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall.Get();
            if (_wait_ != null)
            {
                waitBuffer(identity, _wait_.OnReturn, false);
                return _wait_.Value;
            }
            return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false };
        }

        public void waitBuffer(TmphPhysicalServer.TmphTimeIdentity identity, Action<bool> _onReturn_)
        {
            waitBuffer(identity, _onReturn_, true);
        }

        private void waitBuffer(TmphPhysicalServer.TmphTimeIdentity identity, Action<bool> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphMemoryDatabasePhysical._i7
                    {
                        identity = identity
                    };
                    _socket_.Call(_onReturn_, 7 + 128, _inputParameter_, 2147483647, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(false);
        }

        public TmphAsynchronousMethod.TmphReturnValue<bool> flush(TmphPhysicalServer.TmphTimeIdentity identity)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall<TcpServer.TmphMemoryDatabasePhysical._o8>.Get();
            if (_wait_ != null)
            {
                flush(identity, _wait_.OnReturn, false);
                var _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {
                    var _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new TmphAsynchronousMethod.TmphReturnValue<bool> { IsReturn = false };
        }

        private void flush(TmphPhysicalServer.TmphTimeIdentity identity,
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphMemoryDatabasePhysical._o8>> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphMemoryDatabasePhysical._i8
                    {
                        identity = identity
                    };

                    var _outputParameter_ = new TcpServer.TmphMemoryDatabasePhysical._o8();
                    _socket_.Get(_onReturn_, 8 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null)
                _onReturn_(new TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphMemoryDatabasePhysical._o8>
                {
                    IsReturn = false
                });
        }

        public TmphAsynchronousMethod.TmphReturnValue<bool> flushFile(TmphPhysicalServer.TmphTimeIdentity identity,
            bool isDiskFile)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall<TcpServer.TmphMemoryDatabasePhysical._o9>.Get();
            if (_wait_ != null)
            {
                flushFile(identity, isDiskFile, _wait_.OnReturn, false);
                var _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {
                    var _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new TmphAsynchronousMethod.TmphReturnValue<bool> { IsReturn = false };
        }

        private void flushFile(TmphPhysicalServer.TmphTimeIdentity identity, bool isDiskFile,
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphMemoryDatabasePhysical._o9>> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphMemoryDatabasePhysical._i9
                    {
                        identity = identity,
                        isDiskFile = isDiskFile
                    };

                    var _outputParameter_ = new TcpServer.TmphMemoryDatabasePhysical._o9();
                    _socket_.Get(_onReturn_, 9 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null)
                _onReturn_(new TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphMemoryDatabasePhysical._o9>
                {
                    IsReturn = false
                });
        }

        public void loadHeader(TmphPhysicalServer.TmphTimeIdentity identity,
            Action<TmphAsynchronousMethod.TmphReturnValue<TmphTcpBase.TmphSubByteArrayBuffer>> _onReturn_)
        {
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphMemoryDatabasePhysical._o10>> _onOutput_ = null;
            _onOutput_ =
                TmphAsynchronousMethod
                    .TmphCallReturn<TmphTcpBase.TmphSubByteArrayBuffer, TcpServer.TmphMemoryDatabasePhysical._o10>.Get(_onReturn_);
            if (_onReturn_ == null || _onOutput_ != null)
            {
                loadHeader(identity, _onOutput_, false);
            }
        }

        private void loadHeader(TmphPhysicalServer.TmphTimeIdentity identity,
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphMemoryDatabasePhysical._o10>> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphMemoryDatabasePhysical._i10
                    {
                        identity = identity
                    };

                    var _outputParameter_ = new TcpServer.TmphMemoryDatabasePhysical._o10();
                    _socket_.Get(_onReturn_, 10 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null)
                _onReturn_(new TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphMemoryDatabasePhysical._o10>
                {
                    IsReturn = false
                });
        }
    }
}

namespace Laurent.Lee.CLB.IO
{
    public partial class TmphFileBlockServer
    {
        internal static class TmphTcpServer
        {
            public static bool verify(TmphFileBlockServer _value_, string value)
            {
                return _value_.verify(value);
            }

            public static void read(TmphFileBlockServer _value_, TmphFileBlockStream.TmphIndex index,
                ref TmphTcpBase.TmphSubByteArrayEvent TmphBuffer,
                Func<TmphAsynchronousMethod.TmphReturnValue<TmphTcpBase.TmphSubByteArrayEvent>, bool> onReturn)
            {
                _value_.read(index, ref TmphBuffer, onReturn);
            }

            public static long write(TmphFileBlockServer _value_, TmphTcpBase.TmphSubByteUnmanagedStream dataStream)
            {
                return _value_.write(dataStream);
            }

            public static void waitBuffer(TmphFileBlockServer _value_)
            {
                _value_.waitBuffer();
            }

            public static bool flush(TmphFileBlockServer _value_, bool isDiskFile)
            {
                return _value_.flush(isDiskFile);
            }
        }
    }
}

namespace Laurent.Lee.CLB.TcpServer
{
    /// <summary>
    ///     TCP服务
    /// </summary>
    public partial class TmphFileBlock : TmphCommandServer
    {
        /// <summary>
        ///     TCP服务目标对象
        /// </summary>
        private readonly TmphFileBlockServer _value_;

        /// <summary>
        ///     TCP调用服务端
        /// </summary>
        public TmphFileBlock() : this(null, null)
        {
        }

        /// <summary>
        ///     TCP调用服务端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verify">TCP验证实例</param>
        public TmphFileBlock(TmphTcpServer attribute, TmphFileBlockServer value)
            : base(attribute ?? TmphTcpServer.GetConfig("fileBlock", typeof(TmphFileBlockServer)))
        {
            _value_ = value ?? new TmphFileBlockServer();
            setCommands(5);
            identityOnCommands[verifyCommandIdentity = 0 + 128].Set(_M0, 1024);
            identityOnCommands[1 + 128].Set(_M1);
            identityOnCommands[2 + 128].Set(_M2);
            identityOnCommands[3 + 128].Set(_M3, 0);
            identityOnCommands[4 + 128].Set(_M4);
        }

        private void _M0(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i0();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    _s0 /**/.Call(socket, _value_, socket.Identity, inputParameter)();
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M1(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i1();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    var callbackReturn = TmphSocket.TmphCallback<_o1, TmphTcpBase.TmphSubByteArrayEvent>.Get(socket, new _o1());
                    if (callbackReturn != null)
                    {
                        TmphFileBlockServer.TmphTcpServer.read(_value_, inputParameter.index, ref inputParameter.TmphBuffer,
                            callbackReturn);
                        return;
                    }
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M2(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i2();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    _s2 /**/.Call(socket, _value_, socket.Identity, inputParameter)();
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M3(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                {
                    TmphTask.Tiny.Add(_s3 /**/.Call(socket, _value_, socket.Identity));
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        private void _M4(TmphSocket socket, TmphSubArray<byte> data)
        {
            try
            {
                var inputParameter = new _i4();
                if (TmphDataDeSerializer.DeSerialize(data, ref inputParameter))
                {
                    TmphTask.Tiny.Add(_s4 /**/.Call(socket, _value_, socket.Identity, inputParameter));
                    return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            socket.SendStream(socket.Identity, new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false });
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i0
        {
            public string value;
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _o0 : TmphAsynchronousMethod.IReturnParameter<bool>
        {
            public bool Ret;

            public bool Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        private sealed class _s0 : TmphServerCall<_s0, TmphFileBlockServer, _i0>
        {
            private TmphAsynchronousMethod.TmphReturnValue<_o0> get()
            {
                try
                {
                    var Return =
                        TmphFileBlockServer.TmphTcpServer.verify(serverValue, inputParameter.value);
                    if (Return) socket.IsVerifyMethod = true;
                    return new _o0
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue<_o0> { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s0>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i1
        {
            public TmphTcpBase.TmphSubByteArrayEvent TmphBuffer;
            public TmphFileBlockStream.TmphIndex index;
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _o1 : TmphAsynchronousMethod.IReturnParameter<TmphTcpBase.TmphSubByteArrayEvent>
        {
            public TmphTcpBase.TmphSubByteArrayEvent TmphBuffer;
            public TmphTcpBase.TmphSubByteArrayEvent Ret;

            public TmphTcpBase.TmphSubByteArrayEvent Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i2
        {
            public TmphTcpBase.TmphSubByteUnmanagedStream dataStream;
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _o2 : TmphAsynchronousMethod.IReturnParameter<long>
        {
            public long Ret;

            public long Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        private sealed class _s2 : TmphServerCall<_s2, TmphFileBlockServer, _i2>
        {
            private TmphAsynchronousMethod.TmphReturnValue<_o2> get()
            {
                try
                {
                    var Return =
                        TmphFileBlockServer.TmphTcpServer.write(serverValue, inputParameter.dataStream);
                    return new _o2
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue<_o2> { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s2>.Push(this);
            }
        }

        private sealed class _s3 : TmphServerCall<_s3, TmphFileBlockServer>
        {
            private TmphAsynchronousMethod.TmphReturnValue get()
            {
                try
                {
                    TmphFileBlockServer.TmphTcpServer.waitBuffer(serverValue);
                    return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = true };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s3>.Push(this);
            }
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _i4
        {
            public bool isDiskFile;
        }

        [Emit.TmphDataSerialize(IsMemberMap = false)]
        internal struct _o4 : TmphAsynchronousMethod.IReturnParameter<bool>
        {
            public bool Ret;

            public bool Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        private sealed class _s4 : TmphServerCall<_s4, TmphFileBlockServer, _i4>
        {
            private TmphAsynchronousMethod.TmphReturnValue<_o4> get()
            {
                try
                {
                    var Return =
                        TmphFileBlockServer.TmphTcpServer.flush(serverValue, inputParameter.isDiskFile);
                    return new _o4
                    {
                        Return = Return
                    };
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                return new TmphAsynchronousMethod.TmphReturnValue<_o4> { IsReturn = false };
            }

            protected override void call()
            {
                if (isVerify == 0) socket.SendStream(identity, get());
                TmphTypePool<_s4>.Push(this);
            }
        }
    }
}

namespace Laurent.Lee.CLB.TcpClient
{
    public class TmphFileBlock : IDisposable
    {
        /// <summary>
        ///     TCP调用客户端
        /// </summary>
        public TmphFileBlock() : this(null, null)
        {
        }

        /// <summary>
        ///     TCP调用客户端
        /// </summary>
        /// <param name="attribute">TCP调用服务器端配置信息</param>
        /// <param name="verifyMethod">TCP验证方法</param>
        public TmphFileBlock(TmphTcpServer attribute, TmphTcpBase.ITcpClientVerifyMethod<TmphFileBlock> verifyMethod)
        {
            _TcpClient_ =
                new TmphCommandClient<TmphFileBlock>(
                    attribute ?? TmphTcpServer.GetConfig("fileBlock", typeof(TmphFileBlockServer)), 24,
                    verifyMethod ?? new TmphVerifyMethod(), this);
        }

        /// <summary>
        ///     TCP调用客户端
        /// </summary>
        public TmphCommandClient<TmphFileBlock> _TcpClient_ { get; private set; }

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            var client = _TcpClient_;
            _TcpClient_ = null;
            TmphPub.Dispose(ref client);
        }

        public TmphAsynchronousMethod.TmphReturnValue<bool> verify(string value)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall<TcpServer.TmphFileBlock._o0>.Get();
            if (_wait_ != null)
            {
                verify(value, _wait_.OnReturn, false);
                var _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {
                    var _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new TmphAsynchronousMethod.TmphReturnValue<bool> { IsReturn = false };
        }

        private void verify(string value,
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphFileBlock._o0>> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.VerifyStreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphFileBlock._i0
                    {
                        value = value
                    };

                    var _outputParameter_ = new TcpServer.TmphFileBlock._o0();
                    _socket_.Get(_onReturn_, 0 + 128, _inputParameter_, 1024, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null)
                _onReturn_(new TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphFileBlock._o0> { IsReturn = false });
        }

        public TmphAsynchronousMethod.TmphReturnValue<TmphTcpBase.TmphSubByteArrayEvent> read(TmphFileBlockStream.TmphIndex index,
            ref TmphTcpBase.TmphSubByteArrayEvent TmphBuffer)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall<TcpServer.TmphFileBlock._o1>.Get();
            if (_wait_ != null)
            {
                read(index, ref TmphBuffer, _wait_.OnReturn, false);
                var _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {
                    var _outputParameterValue_ = _outputParameter_.Value;
                    TmphBuffer = _outputParameterValue_.TmphBuffer;
                    return _outputParameterValue_.Return;
                }
            }
            return new TmphAsynchronousMethod.TmphReturnValue<TmphTcpBase.TmphSubByteArrayEvent> { IsReturn = false };
        }

        private void read(TmphFileBlockStream.TmphIndex index, ref TmphTcpBase.TmphSubByteArrayEvent TmphBuffer,
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphFileBlock._o1>> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphFileBlock._i1
                    {
                        index = index,
                        TmphBuffer = TmphBuffer
                    };

                    var _outputParameter_ = new TcpServer.TmphFileBlock._o1();
                    _outputParameter_.TmphBuffer = _inputParameter_.TmphBuffer;
                    _socket_.Get(_onReturn_, 1 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null)
                _onReturn_(new TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphFileBlock._o1> { IsReturn = false });
        }

        public TmphAsynchronousMethod.TmphReturnValue<long> write(TmphTcpBase.TmphSubByteUnmanagedStream dataStream)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall<TcpServer.TmphFileBlock._o2>.Get();
            if (_wait_ != null)
            {
                write(dataStream, _wait_.OnReturn, false);
                var _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {
                    var _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new TmphAsynchronousMethod.TmphReturnValue<long> { IsReturn = false };
        }

        public void write(TmphTcpBase.TmphSubByteUnmanagedStream dataStream,
            Action<TmphAsynchronousMethod.TmphReturnValue<long>> _onReturn_)
        {
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphFileBlock._o2>> _onOutput_ = null;
            _onOutput_ = TmphAsynchronousMethod.TmphCallReturn<long, TcpServer.TmphFileBlock._o2>.Get(_onReturn_);
            if (_onReturn_ == null || _onOutput_ != null)
            {
                write(dataStream, _onOutput_, false);
            }
        }

        private void write(TmphTcpBase.TmphSubByteUnmanagedStream dataStream,
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphFileBlock._o2>> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphFileBlock._i2
                    {
                        dataStream = dataStream
                    };

                    var _outputParameter_ = new TcpServer.TmphFileBlock._o2();
                    _socket_.Get(_onReturn_, 2 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null)
                _onReturn_(new TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphFileBlock._o2> { IsReturn = false });
        }

        public TmphAsynchronousMethod.TmphReturnValue waitBuffer()
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall.Get();
            if (_wait_ != null)
            {
                waitBuffer(_wait_.OnReturn, false);
                return _wait_.Value;
            }
            return new TmphAsynchronousMethod.TmphReturnValue { IsReturn = false };
        }

        public void waitBuffer(Action<bool> _onReturn_)
        {
            waitBuffer(_onReturn_, true);
        }

        private void waitBuffer(Action<bool> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    _socket_.Call(_onReturn_, 3 + 128, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null) _onReturn_(false);
        }

        public TmphAsynchronousMethod.TmphReturnValue<bool> flush(bool isDiskFile)
        {
            var _wait_ = TmphAsynchronousMethod.TmphWaitCall<TcpServer.TmphFileBlock._o4>.Get();
            if (_wait_ != null)
            {
                flush(isDiskFile, _wait_.OnReturn, false);
                var _outputParameter_ = _wait_.Value;
                if (_outputParameter_.IsReturn)
                {
                    var _outputParameterValue_ = _outputParameter_.Value;
                    return _outputParameterValue_.Return;
                }
            }
            return new TmphAsynchronousMethod.TmphReturnValue<bool> { IsReturn = false };
        }

        private void flush(bool isDiskFile,
            Action<TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphFileBlock._o4>> _onReturn_, bool _isTask_)
        {
            try
            {
                var _socket_ = _TcpClient_.StreamSocket;
                if (_socket_ != null)
                {
                    var _inputParameter_ = new TcpServer.TmphFileBlock._i4
                    {
                        isDiskFile = isDiskFile
                    };

                    var _outputParameter_ = new TcpServer.TmphFileBlock._o4();
                    _socket_.Get(_onReturn_, 4 + 128, _inputParameter_, 2147483647, _outputParameter_, _isTask_, false);
                    return;
                }
            }
            catch (Exception _error_)
            {
                TmphLog.Error.Add(_error_, null, false);
            }
            if (_onReturn_ != null)
                _onReturn_(new TmphAsynchronousMethod.TmphReturnValue<TcpServer.TmphFileBlock._o4> { IsReturn = false });
        }
    }
}