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
using Laurent.Lee.CLB.Net.Tcp;
using System;

namespace Laurent.Lee.CLB.IO
{
    /// <summary>
    ///     文件分块服务
    /// </summary>
    [Code.CSharp.TmphTcpServer(Service = "fileBlock", IsIdentityCommand = true, IsServerAsynchronousReceive = false,
        IsClientAsynchronousReceive = false, VerifyMethodType = typeof(TmphVerifyMethod))]
    public partial class TmphFileBlockServer : IDisposable
    {
        /// <summary>
        ///     文件分块写入流
        /// </summary>
        private TmphFileBlockStream fileStream;

        /// <summary>
        ///     文件分块服务
        /// </summary>
        public TmphFileBlockServer()
        {
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
        }

        /// <summary>
        ///     文件分块服务
        /// </summary>
        /// <param name="fileName">文件全名</param>
        public TmphFileBlockServer(string fileName)
        {
            fileStream = new TmphFileBlockStream(fileName);
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            TmphPub.Dispose(ref fileStream);
        }

        /// <summary>
        ///     文件分块服务验证函数
        /// </summary>
        /// <returns>是否验证成功</returns>
        [Code.CSharp.TmphTcpServer(IsVerifyMethod = true, IsServerAsynchronousTask = false, InputParameterMaxLength = 1024
            )]
        private bool verify(string value)
        {
            if (TmphFileBlock.Default.Verify == null && !Config.TmphPub.Default.IsDebug)
            {
                TmphLog.Error.Add("文件分块服务验证数据不能为空", false, true);
                return false;
            }
            return TmphFileBlock.Default.Verify == value;
        }

        /// <summary>
        ///     读取文件分块数据
        /// </summary>
        /// <param name="index">文件分块数据位置</param>
        /// <param name="TmphBuffer">数据缓冲区</param>
        [Code.CSharp.TmphTcpServer(IsServerAsynchronousTask = false, IsServerAsynchronousCallback = true)]
        private void read(TmphFileBlockStream.TmphIndex index, ref TmphTcpBase.TmphSubByteArrayEvent TmphBuffer,
            Func<TmphAsynchronousMethod.TmphReturnValue<TmphTcpBase.TmphSubByteArrayEvent>, bool> onReturn)
        {
            var fileStream = this.fileStream;
            if (fileStream == null) onReturn(default(TmphTcpBase.TmphSubByteArrayEvent));
            else fileStream.Read(index, onReturn);
        }

        /// <summary>
        ///     写入文件分块数据
        /// </summary>
        /// <param name="dataStream">文件分块数据</param>
        /// <returns>写入文件位置</returns>
        [Code.CSharp.TmphTcpServer(IsServerAsynchronousTask = false, IsClientCallbackTask = false,
            IsClientAsynchronous = true)]
        private unsafe long write(TmphTcpBase.TmphSubByteUnmanagedStream dataStream)
        {
            if (fileStream != null)
            {
                var TmphBuffer = dataStream.TmphBuffer;
                if (TmphBuffer.Count != 0)
                {
                    fixed (byte* bufferFixed = TmphBuffer.array)
                    {
                        var start = bufferFixed - sizeof(int);
                        *(int*)start = TmphBuffer.Count;
                        return fileStream.UnsafeWrite(start, TmphBuffer.Count + (-TmphBuffer.Count & 3) + sizeof(int));
                    }
                }
            }
            return -1;
        }

        /// <summary>
        ///     等待缓存写入
        /// </summary>
        [Code.CSharp.TmphTcpServer]
        private void waitBuffer()
        {
            if (fileStream != null) fileStream.WaitWriteBuffer();
        }

        /// <summary>
        ///     写入缓存
        /// </summary>
        /// <param name="isDiskFile">是否写入到磁盘文件</param>
        /// <returns>是否成功</returns>
        [Code.CSharp.TmphTcpServer]
        private bool flush(bool isDiskFile)
        {
            return fileStream != null && fileStream.Flush(isDiskFile) != null;
        }
    }
}