///本文件由程序自动生成,请不要自行修改

#pragma warning disable

namespace Laurent.Lee.CLB.TcpServer
{
    /// <summary>
    ///     TCP服务
    /// </summary>
    public partial class TmphTcpRegister
    {
        /// <summary>
        ///     命令序号记忆数据
        /// </summary>
        private static TmphKeyValue<TmphHashString, int>[] _identityCommandNames_()
        {
            var names = new TmphKeyValue<TmphHashString, int>[7];
            names[0].Set(@"(string)verify", 0);
            names[1].Set(
                @"(Laurent.Lee.CLB.Net.Tcp.TmphTcpRegister.clientId,Laurent.Lee.CLB.Net.Tcp.TmphTcpRegister.service)register",
                1);
            names[2].Set(
                @"(Laurent.Lee.CLB.Net.Tcp.TmphTcpRegister.clientId,System.Func<Laurent.Lee.CLB.Code.CSharp.TmphAsynchronousMethod.returnValue<Laurent.Lee.CLB.Net.Tcp.TmphTcpRegister.pollResult>,bool>)poll",
                2);
            names[3].Set(@"(Laurent.Lee.CLB.Net.Tcp.TmphTcpRegister.clientId,string)removeRegister", 3);
            names[4].Set(@"(out int)getServices", 4);
            names[5].Set(@"()register", 5);
            names[6].Set(@"(Laurent.Lee.CLB.Net.Tcp.TmphTcpRegister.clientId)removeRegister", 6);
            return names;
        }
    }
}

namespace Laurent.Lee.CLB.TcpServer
{
    /// <summary>
    ///     TCP服务
    /// </summary>
    public partial class TmphHttpServer
    {
        /// <summary>
        ///     命令序号记忆数据
        /// </summary>
        private static TmphKeyValue<TmphHashString, int>[] _identityCommandNames_()
        {
            var names = new TmphKeyValue<TmphHashString, int>[7];
            names[0].Set(@"(string)verify", 0);
            names[1].Set(@"(Laurent.Lee.CLB.Net.Tcp.host)setForward", 1);
            names[2].Set(@"(Laurent.Lee.CLB.Net.Tcp.Http.domain[])stop", 2);
            names[3].Set(@"(Laurent.Lee.CLB.Net.Tcp.Http.domain)stop", 3);
            names[4].Set(@"(string,string,Laurent.Lee.CLB.Net.Tcp.Http.domain[],bool)start", 4);
            names[5].Set(@"(string,string,Laurent.Lee.CLB.Net.Tcp.Http.domain,bool)start", 5);
            names[6].Set(@"()removeForward", 6);
            return names;
        }
    }
}

namespace Laurent.Lee.CLB.TcpServer
{
    /// <summary>
    ///     TCP服务
    /// </summary>
    public partial class TmphProcessCopy
    {
        /// <summary>
        ///     命令序号记忆数据
        /// </summary>
        private static TmphKeyValue<TmphHashString, int>[] _identityCommandNames_()
        {
            var names = new TmphKeyValue<TmphHashString, int>[4];
            names[0].Set(@"(string)verify", 0);
            names[1].Set(@"(Laurent.Lee.CLB.Diagnostics.processCopyServer.copyer)guard", 1);
            names[2].Set(@"(Laurent.Lee.CLB.Diagnostics.processCopyServer.copyer)copyStart", 2);
            names[3].Set(@"(Laurent.Lee.CLB.Diagnostics.processCopyServer.copyer)remove", 3);
            return names;
        }
    }
}

namespace Laurent.Lee.CLB.TcpServer
{
    /// <summary>
    ///     TCP服务
    /// </summary>
    public partial class TmphMemoryDatabasePhysical
    {
        /// <summary>
        ///     命令序号记忆数据
        /// </summary>
        private static TmphKeyValue<TmphHashString, int>[] _identityCommandNames_()
        {
            var names = new TmphKeyValue<TmphHashString, int>[11];
            names[0].Set(@"(string)verify", 0);
            names[1].Set(@"(string)open", 1);
            names[2].Set(@"(Laurent.Lee.CLB.TmphMemoryDatabase.TmphPhysicalServer.timeIdentity)close", 2);
            names[3].Set(@"(Laurent.Lee.CLB.Code.CSharp.tcpBase.subByteUnmanagedStream)create", 3);
            names[4].Set(@"(Laurent.Lee.CLB.TmphMemoryDatabase.TmphPhysicalServer.timeIdentity)load", 4);
            names[5].Set(@"(Laurent.Lee.CLB.TmphMemoryDatabase.TmphPhysicalServer.timeIdentity,bool)loaded", 5);
            names[6].Set(@"(Laurent.Lee.CLB.Code.CSharp.TmphTcpBase.subByteUnmanagedStream)append", 6);
            names[7].Set(@"(Laurent.Lee.CLB.TmphMemoryDatabase.TmphPhysicalServer.timeIdentity)waitBuffer", 7);
            names[8].Set(@"(Laurent.Lee.CLB.TmphMemoryDatabase.TmphPhysicalServer.timeIdentity)flush", 8);
            names[9].Set(@"(Laurent.Lee.CLB.TmphMemoryDatabase.TmphPhysicalServer.timeIdentity,bool)flushFile", 9);
            names[10].Set(@"(Laurent.Lee.CLB.TmphMemoryDatabase.TmphPhysicalServer.timeIdentity)loadHeader", 10);
            return names;
        }
    }
}

namespace Laurent.Lee.CLB.TcpServer
{
    /// <summary>
    ///     TCP服务
    /// </summary>
    public partial class TmphFileBlock
    {
        /// <summary>
        ///     命令序号记忆数据
        /// </summary>
        private static TmphKeyValue<TmphHashString, int>[] _identityCommandNames_()
        {
            var names = new TmphKeyValue<TmphHashString, int>[5];
            names[0].Set(@"(string)verify", 0);
            names[1].Set(
                @"(Laurent.Lee.CLB.IO.TmphFileBlockStream.index,ref Laurent.Lee.CLB.Code.CSharp.TmphTcpBase.subByteArrayEvent,System.Func<Laurent.Lee.CLB.Code.CSharp.TmphAsynchronousMethod.returnValue<Laurent.Lee.CLB.Code.CSharp.TmphTcpBase.subByteArrayEvent>,bool>)read",
                1);
            names[2].Set(@"(Laurent.Lee.CLB.Code.CSharp.TmphTcpBase.subByteUnmanagedStream)write", 2);
            names[3].Set(@"()waitBuffer", 3);
            names[4].Set(@"(bool)flush", 4);
            return names;
        }
    }
}