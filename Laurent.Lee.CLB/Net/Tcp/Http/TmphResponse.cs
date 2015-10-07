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
using Laurent.Lee.CLB.IO.Compression;
using Laurent.Lee.CLB.Web;
using System;
using System.IO;
using System.Threading;

namespace Laurent.Lee.CLB.Net.Tcp.Http
{
    /// <summary>
    ///     HTTP响应
    /// </summary>
    //[Laurent.Lee.CLB.Code.CSharp.serialize(IsReferenceMember = false)]
    public sealed class TmphResponse : IDisposable
    {
        /// <summary>
        ///     HTTP状态类型
        /// </summary>
        public enum TmphState
        {
            /// <summary>
            ///     未知状态
            /// </summary>
            Unknown,

            /// <summary>
            ///     允许客户端继续发送数据
            /// </summary>
            [TmphStateInfo(Number = 100, Text = @" 100 Continue
")]
            Continue100,

            /// <summary>
            ///     WebSocket握手
            /// </summary>
            [TmphStateInfo(Number = 101, Text = @" 101 Switching Protocols
")]
            WebSocket101,

            /// <summary>
            ///     客户端请求成功
            /// </summary>
            [TmphStateInfo(Number = 200, Text = @" 200 OK
")]
            Ok200,

            /// <summary>
            ///     成功处理了Range头的GET请求
            /// </summary>
            [TmphStateInfo(Number = 206, Text = @" 206 Partial Content
")]
            PartialContent206,

            /// <summary>
            ///     永久重定向
            /// </summary>
            [TmphStateInfo(Number = 301, Text = @" 301 Moved Permanently
")]
            MovedPermanently301,

            /// <summary>
            ///     临时重定向
            /// </summary>
            [TmphStateInfo(Number = 302, Text = @" 302 Found
")]
            Found302,

            /// <summary>
            ///     资源未修改
            /// </summary>
            [TmphStateInfo(Number = 304, Text = @" 304 Not Changed
")]
            NotChanged304,

            /// <summary>
            ///     客户端请求有语法错误，不能被服务器所理解
            /// </summary>
            [TmphStateInfo(IsError = true, Number = 400, Text = @" 400 Bad Request
")]
            BadRequest400,

            /// <summary>
            ///     请求未经授权，这个状态代码必须和WWW-Authenticate报头域一起使用
            /// </summary>
            [TmphStateInfo(IsError = true, Number = 401, Text = @" 401 Unauthorized
")]
            Unauthorized401,

            /// <summary>
            ///     服务器收到请求，但是拒绝提供服务
            ///     WWW-Authenticate响应报头域必须被包含在401（未授权的）响应消息中，客户端收到401响应消息时候，并发送Authorization报头域请求服务器对其进行验证时，服务端响应报头就包含该报头域。
            ///     eg：WWW-Authenticate:Basic realm="Basic Auth Test!"  可以看出服务器对请求资源采用的是基本验证机制。
            /// </summary>
            [TmphStateInfo(IsError = true, Number = 403, Text = @" 403 Forbidden
")]
            Forbidden403,

            /// <summary>
            ///     请求资源不存在
            /// </summary>
            [TmphStateInfo(IsError = true, Number = 404, Text = @" 404 Not Found
")]
            NotFound404,

            /// <summary>
            ///     不允许使用的方法
            /// </summary>
            [TmphStateInfo(IsError = true, Number = 405, Text = @" 405 Method Not Allowed
")]
            MethodNotAllowed405,

            /// <summary>
            ///     Request Timeout
            /// </summary>
            [TmphStateInfo(IsError = true, Number = 408, Text = @" 408 Request Timeout
")]
            RequestTimeout408,

            /// <summary>
            ///     Range请求无效
            /// </summary>
            [TmphStateInfo(IsError = true, Number = 416, Text = @" 416 Request Range Not Satisfiable
")]
            RangeNotSatisfiable416,

            /// <summary>
            ///     服务器发生不可预期的错误
            /// </summary>
            [TmphStateInfo(IsError = true, Number = 500, Text = @" 500 Internal Server Error
")]
            ServerError500,

            /// <summary>
            ///     服务器当前不能处理客户端的请求，一段时间后可能恢复正常
            /// </summary>
            [TmphStateInfo(IsError = true, Number = 503, Text = @" 503 Server Unavailable
")]
            ServerUnavailable503
        }

        /// <summary>
        ///     资源未修改
        /// </summary>
        internal static readonly TmphResponse NotChanged304 = new TmphResponse { State = TmphState.NotChanged304 };

        /// <summary>
        ///     Range请求无效
        /// </summary>
        internal static readonly TmphResponse RangeNotSatisfiable416 = new TmphResponse
        {
            State = TmphState.RangeNotSatisfiable416
        };

        /// <summary>
        ///     空页面输出
        /// </summary>
        internal static readonly TmphResponse Blank = new TmphResponse
        {
            State = TmphState.Ok200,
            CacheControl = ("public, max-age=9999999").GetBytes(),
            LastModified = ("Mon, 20 Apr 1981 08:03:16 GMT").GetBytes()
        };

        /// <summary>
        ///     默认内容类型头部
        /// </summary>
        internal static readonly byte[] HtmlContentType =
            ("text/html; charset=" + TmphAppSetting.Encoding.WebName).GetBytes();

        /// <summary>
        ///     默认内容类型头部
        /// </summary>
        internal static readonly byte[] JsContentType =
            ("application/x-javascript; charset=" + TmphAppSetting.Encoding.WebName).GetBytes();

        /// <summary>
        ///     ZIP文件输出类型
        /// </summary>
        private static readonly byte[] zipContentType = TmphContentTypeInfo.GetContentType("zip");

        /// <summary>
        ///     文本文件输出类型
        /// </summary>
        private static readonly byte[] textContentType = TmphContentTypeInfo.GetContentType("txt");

        /// <summary>
        ///     GZIP压缩响应头部
        /// </summary>
        internal static readonly byte[] GZipEncoding = ("gzip").GetBytes();

        /// <summary>
        ///     非缓存参数输出
        /// </summary>
        private static readonly byte[] noStoreBytes = ("public, no-store").GetBytes();

        /// <summary>
        ///     缓存过期
        /// </summary>
        private static readonly byte[] zeroAgeBytes = ("public, max-age=0").GetBytes();

        /// <summary>
        ///     GZIP压缩响应头部字节尺寸
        /// </summary>
        internal static readonly int GZipSize = TmphHeader.ContentEncoding.Length + GZipEncoding.Length + 2;

        /// <summary>
        ///     HTTP响应数量
        /// </summary>
        private static int newCount;

        /// <summary>
        ///     输出内容
        /// </summary>
        internal TmphSubArray<byte> Body;

        /// <summary>
        ///     输出内容重定向文件
        /// </summary>
        private string bodyFile;

        /// <summary>
        ///     输出缓存流
        /// </summary>
        internal TmphUnmanagedStream BodyStream;

        /// <summary>
        ///     临时缓存区
        /// </summary>
        internal byte[] Buffer;

        /// <summary>
        ///     缓存参数
        /// </summary>
        public byte[] CacheControl;

        /// <summary>
        ///     是否可以覆盖HTTP预留头部
        /// </summary>
        internal bool CanHeader;

        /// <summary>
        ///     内容描述
        /// </summary>
        public byte[] ContentDisposition;

        /// <summary>
        ///     输出内容压缩编码
        /// </summary>
        internal byte[] ContentEncoding;

        /// <summary>
        ///     输出内容类型
        /// </summary>
        public byte[] ContentType;

        /// <summary>
        ///     Cookie集合
        /// </summary>
        internal TmphList<TmphCookie> Cookies = new TmphList<TmphCookie>();

        /// <summary>
        ///     缓存匹配标识
        /// </summary>
        internal byte[] ETag;

        /// <summary>
        ///     输出数据是否一次性(不可重用)
        /// </summary>
        private bool isBodyOnlyOnce;

        /// <summary>
        ///     是否已经释放资源
        /// </summary>
        private int isDisposed;

        /// <summary>
        ///     是否使用HTTP响应池
        /// </summary>
        private bool isPool;

        /// <summary>
        ///     JSON输出流
        /// </summary>
        private TmphCharStream jsonStream;

        /// <summary>
        ///     最后修改时间
        /// </summary>
        public byte[] LastModified;

        /// <summary>
        ///     重定向
        /// </summary>
        internal TmphSubArray<byte> Location;

        /// <summary>
        ///     HTTP响应状态
        /// </summary>
        public TmphState State;

        /// <summary>
        ///     HTTP响应
        /// </summary>
        private TmphResponse()
        {
        }

        /// <summary>
        ///     获取包含HTTP头部的输出内容
        /// </summary>
        internal TmphSubArray<byte> HeaderBody
        {
            get
            {
                var size = Unsafe.TmphMemory.GetInt(Body.array);
                return size == 0
                    ? default(TmphSubArray<byte>)
                    : TmphSubArray<byte>.Unsafe(Body.array, Body.StartIndex - size, Body.Count + size);
            }
        }

        /// <summary>
        ///     输出内容数组
        /// </summary>
        public byte[] BodyData
        {
            get { return Body.Array; }
        }

        /// <summary>
        ///     输出内容重定向文件
        /// </summary>
        public string BodyFile
        {
            get { return bodyFile; }
            set
            {
                bodyFile = value;
                if (value != null) Body.UnsafeSetLength(0);
            }
        }

        /// <summary>
        ///     输出内容长度
        /// </summary>
        public long BodySize
        {
            get
            {
                if (BodyFile == null) return Body.Count;
                try
                {
                    return new FileInfo(BodyFile).Length;
                }
                catch (Exception error)
                {
                    TmphLog.Default.Add(error, BodyFile, false);
                    return 0;
                }
            }
        }

        /// <summary>
        ///     HTTP响应数量
        /// </summary>
        public static int NewCount
        {
            get { return newCount; }
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Increment(ref isDisposed) == 1) Interlocked.Decrement(ref newCount);
            TmphPub.Dispose(ref BodyStream);
        }

        /// <summary>
        ///     清除数据
        /// </summary>
        public void Clear()
        {
            if (isBodyOnlyOnce)
            {
                isBodyOnlyOnce = false;
                Body.UnsafeSet(Buffer, 0, 0);
                Buffer = TmphNullValue<byte>.Array;
            }
            else
            {
                byte[] buffer = Buffer, data = Body.Array;
                if (buffer.Length > data.Length)
                {
                    Body.UnsafeSet(buffer, 0, 0);
                    Buffer = data;
                }
                else Body.UnsafeSetLength(0);
            }
            State = TmphState.ServerError500;
            CanHeader = false;
            bodyFile = null;
            Location.Null();
            LastModified = CacheControl = ContentType = ContentEncoding = ETag = ContentDisposition = null;
            Cookies.Empty();
            BodyStream.Clear();
        }

        /// <summary>
        ///     设置输出数据
        /// </summary>
        /// <param name="data">输出数据</param>
        /// <param name="isBodyData">输出数据是否一次性(不可重用)</param>
        public void SetBody(TmphSubArray<byte> data, bool isBodyOnlyOnce = true, bool canHeader = false)
        {
            CanHeader = canHeader;
            if (data.Count == 0) Body.UnsafeSetLength(0);
            else
            {
                if (!this.isBodyOnlyOnce) Buffer = Body.Array;
                Body = data;
                this.isBodyOnlyOnce = isBodyOnlyOnce;
            }
        }

        /// <summary>
        ///     获取JSON序列化输出缓冲区
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal unsafe TmphCharStream ResetJsonStream(void* data, int size)
        {
            if (jsonStream == null) return jsonStream = new TmphCharStream((char*)data, size >> 1);
            jsonStream.Reset((byte*)data, size);
            return jsonStream;
        }

        /// <summary>
        ///     设置非缓存参数输出
        /// </summary>
        internal void NoStore()
        {
            if (LastModified == null && CacheControl == null && ETag == null) CacheControl = noStoreBytes;
        }

        ///// <summary>
        ///// 设置缓存过期
        ///// </summary>
        //public void ZeroAge()
        //{
        //    CacheControl = zeroAgeBytes;
        //}
        /// <summary>
        ///     设置缓存匹配标识
        /// </summary>
        /// <param name="eTag">缓存匹配标识</param>
        public void SetETag(byte[] eTag)
        {
            ETag = eTag;
            if (CacheControl == null) CacheControl = zeroAgeBytes;
        }

        /// <summary>
        ///     设置js内容类型
        /// </summary>
        /// <param name="domainServer">域名服务</param>
        internal void SetJsContentType(TmphDomainServer domainServer)
        {
            ContentType = domainServer.JsContentType;
        }

        /// <summary>
        ///     设置zip内容类型
        /// </summary>
        public void SetZipContentType()
        {
            ContentType = zipContentType;
        }

        /// <summary>
        ///     设置文本内容类型
        /// </summary>
        public void SetTextContentType()
        {
            ContentType = textContentType;
        }

        /// <summary>
        ///     获取压缩数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>压缩数据,失败返回null</returns>
        internal static TmphSubArray<byte> GetCompress(TmphSubArray<byte> data, TmphMemoryPool memoryPool = null, int seek = 0)
        {
            if (data.Count > GZipSize)
            {
                var compressData = TmphStream.GZip.GetCompress(data.Array, data.StartIndex, data.Count, seek, memoryPool);
                if (compressData.Count != 0)
                {
                    if (compressData.Count + GZipSize < data.Count) return compressData;
                    if (memoryPool != null) memoryPool.Push(ref compressData.array);
                }
            }
            return default(TmphSubArray<byte>);
        }

        /// <summary>
        ///     获取HTTP响应
        /// </summary>
        /// <param name="isPool">是否使用HTTP响应池</param>
        /// <returns>HTTP响应</returns>
        public static TmphResponse Get(bool isPool = false)
        {
            if (isPool)
            {
                var response = TmphTypePool<TmphResponse>.Pop();
                if (response != null) return response;
                Interlocked.Increment(ref newCount);
                return new TmphResponse
                {
                    BodyStream = new TmphUnmanagedStream(),
                    isPool = true,
                    Body = TmphSubArray<byte>.Unsafe(TmphNullValue<byte>.Array, 0, 0),
                    Buffer = TmphNullValue<byte>.Array
                };
            }
            return new TmphResponse
            {
                isPool = false,
                Body = TmphSubArray<byte>.Unsafe(TmphNullValue<byte>.Array, 0, 0),
                Buffer = TmphNullValue<byte>.Array
            };
        }

        /// <summary>
        ///     复制HTTP响应
        /// </summary>
        /// <param name="response">HTTP响应</param>
        /// <returns>HTTP响应</returns>
        internal static TmphResponse Copy(TmphResponse response)
        {
            var value = Get(true);
            if (response != null)
            {
                value.CacheControl = response.CacheControl;
                value.ContentEncoding = response.ContentEncoding;
                value.ContentType = response.ContentType;
                value.ETag = response.ETag;
                value.LastModified = response.LastModified;
                value.ContentDisposition = response.ContentDisposition;
                var count = response.Cookies.Count;
                if (count != 0) value.Cookies.Add(response.Cookies.array, 0, count);
            }
            return value;
        }

        /// <summary>
        ///     添加到HTTP响应池
        /// </summary>
        /// <param name="response">HTTP响应</param>
        internal static void Push(ref TmphResponse response)
        {
            var value = Interlocked.Exchange(ref response, null);
            if (value != null && value.isPool)
            {
                value.Clear();
                TmphTypePool<TmphResponse>.Push(value);
            }
        }

        /// <summary>
        ///     HTTP响应状态
        /// </summary>
        public sealed class TmphStateInfo : Attribute
        {
            /// <summary>
            ///     状态输出字节
            /// </summary>
            private byte[] bytes;

            /// <summary>
            ///     是否错误状态类型
            /// </summary>
            public bool IsError;

            /// <summary>
            ///     编号
            /// </summary>
            public int Number;

            /// <summary>
            ///     状态输出文本
            /// </summary>
            public string Text;

            /// <summary>
            ///     状态输出字节
            /// </summary>
            internal byte[] Bytes
            {
                get
                {
                    if (bytes == null) bytes = Text.GetBytes();
                    return bytes;
                }
            }
        }
    }
}