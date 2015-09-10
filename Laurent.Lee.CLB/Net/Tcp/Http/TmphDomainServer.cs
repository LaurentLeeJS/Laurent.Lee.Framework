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

using Laurent.Lee.CLB.Code;
using Laurent.Lee.CLB.Code.CSharp;
using Laurent.Lee.CLB.Config;
using Laurent.Lee.CLB.IO;
using Laurent.Lee.CLB.Threading;
using Laurent.Lee.CLB.Web;
using System;
using System.IO;
using System.Text;
using System.Threading;
using TmphHttp = Laurent.Lee.CLB.Web.TmphHttp;

namespace Laurent.Lee.CLB.Net.Tcp.Http
{
    /// <summary>
    ///     域名服务
    /// </summary>
    public abstract class TmphDomainServer : IDisposable
    {
        /// <summary>
        ///     默认扩展名集合
        /// </summary>
        private static readonly TmphUniqueHashSet<TmphDefaultExtensionName> defaultExtensionNames =
            new TmphUniqueHashSet<TmphDefaultExtensionName>(
                new TmphDefaultExtensionName[]
                {
                    "avi", "bmp", "css", "cur", "doc", "docx", "gif", "htm", "html", "ico", "jpg", "jpeg", "js", "mp3",
                    "mp4", "mpg", "pdf", "png", "rar", "rm", "rmvb", "svg", "swf", "txt", "wav", "xml", "xls", "xlsx",
                    "zip", "z7"
                }, 107);

        /// <summary>
        ///     默认非压缩扩展名集合
        /// </summary>
        private static readonly TmphUniqueHashSet<TmphDefaultCompressExtensionName> defaultCompressExtensionNames =
            new TmphUniqueHashSet<TmphDefaultCompressExtensionName>(
                new TmphDefaultCompressExtensionName[]
                {
                    "avi", "cur", "gif", "ico", "jpg", "jpeg", "mp3", "mp4", "mpg", "png", "rar", "rm", "rmvb", "wav",
                    "zip", "z7"
                }, 27);

        /// <summary>
        ///     文件缓存名称缓冲区
        /// </summary>
        private static readonly TmphMemoryPool cacheNameBuffer = TmphMemoryPool.GetPool(TmphFile.MaxFullNameLength << 1);

        /// <summary>
        ///     默认内容类型头部
        /// </summary>
        internal readonly byte[] HtmlContentType;

        /// <summary>
        ///     默认内容类型头部
        /// </summary>
        internal readonly byte[] JsContentType;

        /// <summary>
        ///     输出编码
        /// </summary>
        internal readonly Encoding ResponseEncoding;

        /// <summary>
        ///     网站生成配置
        /// </summary>
        protected internal readonly TmphWebConfig WebConfig;

        /// <summary>
        ///     文件缓存
        /// </summary>
        private TmphFifoPriorityQueue<TmphHashBytes, TmphFileCache> cache;

        /// <summary>
        ///     缓存控制参数
        /// </summary>
        protected byte[] cacheControl;

        /// <summary>
        ///     文件缓存访问锁
        /// </summary>
        private int cacheLock;

        /// <summary>
        ///     最大缓存字节数
        /// </summary>
        private int cacheSize;

        /// <summary>
        ///     当前缓存字节数
        /// </summary>
        private int currentCacheSize;

        /// <summary>
        ///     域名信息集合
        /// </summary>
        private TmphDomain[] domains;

        /// <summary>
        ///     错误输出数据
        /// </summary>
        protected TmphKeyValue<TmphResponse, TmphResponse>[] errorResponse;

        /// <summary>
        ///     最大文件缓存字节数
        /// </summary>
        private int fileSize;

        /// <summary>
        ///     文件缓存是否预留HTTP头部
        /// </summary>
        private bool isCacheHeader;

        /// <summary>
        ///     HTML文件缓存是否预留HTTP头部
        /// </summary>
        private bool isCacheHtmlHeader;

        /// <summary>
        ///     是否停止服务
        /// </summary>
        private int isDisposed;

        ///// <summary>
        ///// 是否支持请求范围
        ///// </summary>
        //protected internal virtual bool isRequestRange { get { return false; } }
        /// <summary>
        ///     是否启动服务
        /// </summary>
        private int isStart;

        /// <summary>
        ///     加载检测路径
        /// </summary>
        internal string LoadCheckPath;

        /// <summary>
        ///     停止服务处理
        /// </summary>
        private Action onStop;

        /// <summary>
        ///     域名服务
        /// </summary>
        protected TmphDomainServer()
        {
            WebConfig = getWebConfig() ?? TmphNullWebConfig.Default;
            if (WebConfig != null) ResponseEncoding = WebConfig.Encoding;
            if (ResponseEncoding == null) ResponseEncoding = TmphAppSetting.Encoding;
            if (ResponseEncoding.CodePage == TmphAppSetting.Encoding.CodePage)
            {
                HtmlContentType = TmphResponse.HtmlContentType;
                JsContentType = TmphResponse.JsContentType;
            }
            else
            {
                HtmlContentType = ("text/html; charset=" + ResponseEncoding.WebName).GetBytes();
                JsContentType = ("application/x-javascript; charset=" + ResponseEncoding.WebName).GetBytes();
            }
        }

        /// <summary>
        ///     内容类型
        /// </summary>
        protected static byte[] ContentTypeBytes
        {
            get { return TmphHeader.ContentTypeBytes; }
        }

        /// <summary>
        ///     文件路径
        /// </summary>
        protected virtual string path
        {
            get { return null; }
        }

        /// <summary>
        ///     文件路径
        /// </summary>
        public string WorkPath { get; private set; }

        /// <summary>
        ///     最大缓存字节数(单位MB)
        /// </summary>
        protected virtual int maxCacheSize
        {
            get { return TmphWeb.Default.MaxCacheSize; }
        }

        /// <summary>
        ///     最大文件缓存字节数(单位KB)
        /// </summary>
        protected virtual int maxCacheFileSize
        {
            get { return TmphWeb.Default.MaxCacheFileSize; }
        }

        /// <summary>
        ///     获取Session
        /// </summary>
        public TmphISession Session { get; protected set; }

        /// <summary>
        ///     客户端缓存时间(单位:秒)
        /// </summary>
        protected virtual int clientCacheSeconds
        {
            get { return TmphWeb.Default.ClientCacheSeconds; }
        }

        /// <summary>
        ///     文件缓存是否预留HTTP头部
        /// </summary>
        protected virtual bool isCacheHttpHeader
        {
            get { return false; }
        }

        /// <summary>
        ///     HTML文件缓存是否预留HTTP头部
        /// </summary>
        protected virtual bool isCacheHtmlHttpHeader
        {
            get { return false; }
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            dispose();
        }

        /// <summary>
        ///     网站生成配置
        /// </summary>
        /// <returns>网站生成配置</returns>
        protected virtual TmphWebConfig getWebConfig()
        {
            return null;
        }

        /// <summary>
        ///     启动HTTP服务
        /// </summary>
        /// <param name="domains">域名信息集合</param>
        /// <param name="onStop">停止服务处理</param>
        /// <returns>是否启动成功</returns>
        public abstract bool Start(TmphDomain[] domains, Action onStop);

        /// <summary>
        ///     创建错误输出数据
        /// </summary>
        protected virtual unsafe void createErrorResponse()
        {
            var errorResponse = new TmphKeyValue<TmphResponse, TmphResponse>[TmphEnum.GetMaxValue<TmphResponse.TmphState>(-1) + 1];
            var isResponse = 0;
            try
            {
                var path = new byte[9];
                fixed (byte* pathFixed = path)
                {
                    *pathFixed = (byte)'/';
                    *(int*)(pathFixed + sizeof(int)) = '.' + ('h' << 8) + ('t' << 16) + ('m' << 24);
                    *(pathFixed + sizeof(int) * 2) = (byte)'l';
                    foreach (TmphResponse.TmphState type in Enum.GetValues(typeof(TmphResponse.TmphState)))
                    {
                        var state = TmphEnum<TmphResponse.TmphState, TmphResponse.TmphStateInfo>.Array((int)type);
                        if (state != null && state.IsError)
                        {
                            int stateValue = state.Number, value = stateValue / 100;
                            *(pathFixed + 1) = (byte)(value + '0');
                            stateValue -= value * 100;
                            *(pathFixed + 2) = (byte)((value = stateValue / 10) + '0');
                            *(pathFixed + 3) = (byte)((stateValue - value * 10) + '0');
                            var cache = file(TmphSubArray<byte>.Unsafe(path, 0, path.Length), default(TmphSubArray<byte>));
                            var fileCache = cache.Value;
                            if (fileCache == null)
                            {
                                if (cache.Key != null)
                                {
                                    errorResponse[(int)type].Set(cache.Key, cache.Key);
                                    isResponse = 1;
                                }
                            }
                            else
                            {
                                TmphResponse response = TmphResponse.Get(), gzipResponse = TmphResponse.Get();
                                response.State = gzipResponse.State = type;
                                response.SetBody(fileCache.Data, true,
                                    fileCache.Data.StartIndex == TmphFileCache.HttpHeaderSize);
                                gzipResponse.SetBody(fileCache.GZipData, true,
                                    fileCache.GZipData.StartIndex == TmphFileCache.HttpHeaderSize);
                                gzipResponse.ContentEncoding = TmphResponse.GZipEncoding;
                                errorResponse[(int)type].Set(response, gzipResponse);
                                isResponse = 1;
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
            }
            if (isResponse != 0) this.errorResponse = errorResponse;
        }

        /// <summary>
        ///     HTTP请求处理
        /// </summary>
        /// <param name="socket">HTTP套接字</param>
        /// <param name="socketIdentity">套接字操作编号</param>
        public abstract void Request(TmphSocketBase socket, long socketIdentity);

        /// <summary>
        ///     获取WEB视图URL重写路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public virtual byte[] GetViewRewrite(TmphSubArray<byte> path)
        {
            return null;
        }

        /// <summary>
        ///     获取错误数据
        /// </summary>
        /// <param name="state">错误状态</param>
        /// <param name="isGzip">是否支持GZip压缩</param>
        /// <returns>错误数据</returns>
        internal TmphResponse GetErrorResponseData(TmphResponse.TmphState state, bool isGzip)
        {
            if (errorResponse != null)
            {
                return isGzip ? errorResponse[(int)state].Value : errorResponse[(int)state].Key;
            }
            return null;
        }

        /// <summary>
        ///     设置文件缓存
        /// </summary>
        protected void setCache()
        {
            cache = new TmphFifoPriorityQueue<TmphHashBytes, TmphFileCache>();
            cacheSize = maxCacheSize << 20;
            if (cacheSize < 0)
                TmphLog.Default.Add("最大缓存字节数(单位MB) " + maxCacheSize.toString() + " << 20 = " + cacheSize.toString(), false,
                    false);
            fileSize = maxCacheFileSize << 10;
            if (fileSize < 0)
                TmphLog.Default.Add("最大文件缓存字节数(单位MB) " + maxCacheSize.toString() + " << 10 = " + fileSize.toString(),
                    false, false);
            if (fileSize > cacheSize) fileSize = cacheSize;
            cacheControl = ("public, max-age=" + clientCacheSeconds.toString()).GetBytes();
            isCacheHeader = isCacheHttpHeader;
            isCacheHtmlHeader = isCacheHtmlHttpHeader;
        }

        /// <summary>
        ///     HTTP文件请求处理
        /// </summary>
        /// <param name="path">请求路径</param>
        /// <param name="ifModifiedSince">文件修改时间</param>
        /// <returns>文件缓存+HTTP响应输出</returns>
        protected unsafe TmphKeyValue<TmphResponse, TmphFileCache> file(TmphSubArray<byte> path, TmphSubArray<byte> ifModifiedSince)
        {
            string cacheFileName = null;
            try
            {
                if (path.Count != 0 && WorkPath.Length + path.Count <= TmphFile.MaxFullNameLength)
                {
                    var TmphBuffer = cacheNameBuffer.Get();
                    try
                    {
                        fixed (byte* bufferFixed = TmphBuffer, pathFixed = path.Array)
                        {
                            byte* pathStart = pathFixed + path.StartIndex,
                                lowerPath = bufferFixed,
                                pathEnd = pathStart + path.Count;
                            if (*pathStart == '/') ++pathStart;
                            *pathEnd = (byte)':';
                            var directorySeparatorChar = (byte)Path.DirectorySeparatorChar;
                            while (*pathStart != ':')
                            {
                                if ((uint)(*pathStart - 'A') < 26) *lowerPath++ = (byte)(*pathStart++ | 0x20);
                                else
                                {
                                    *lowerPath++ = *pathStart == '/' ? directorySeparatorChar : *pathStart;
                                    ++pathStart;
                                }
                            }
                            if (pathStart != pathEnd)
                                return new TmphKeyValue<TmphResponse, TmphFileCache>(TmphResponse.Blank, null);
                            TmphHashBytes cacheKey = TmphSubArray<byte>.Unsafe(TmphBuffer, 0, (int)(lowerPath - bufferFixed));
                            TmphInterlocked.NoCheckCompareSetSleep0(ref cacheLock);
                            var fileCache = cache.Get(cacheKey, null);
                            cacheLock = 0;
                            if (fileCache == null)
                            {
                                cacheFileName = TmphString.FastAllocateString(WorkPath.Length + cacheKey.Length);
                                fixed (char* nameFixed = cacheFileName)
                                {
                                    Unsafe.TmphString.Copy(WorkPath, nameFixed);
                                    Unsafe.TmphMemory.ToLower(bufferFixed, bufferFixed + cacheKey.Length,
                                        nameFixed + WorkPath.Length);
                                }
                                var file = new FileInfo(cacheFileName);
                                if (file.Exists)
                                {
                                    var fileName = file.FullName;
                                    if (fileName.Length <= TmphFile.MaxFullNameLength &&
                                        fileName.ToLower().StartsWith(WorkPath, StringComparison.Ordinal))
                                    {
                                        var extensionName = TmphSubString.Unsafe(fileName, 0, 0);
                                        var extensionIndex = fileName.LastIndexOf('.');
                                        if (++extensionIndex != 0)
                                        {
                                            var pathIndex = fileName.LastIndexOf(Path.DirectorySeparatorChar);
                                            if (pathIndex < extensionIndex)
                                                extensionName.UnsafeSet(extensionIndex, fileName.Length - extensionIndex);
                                        }
                                        if (isFile(extensionName))
                                        {
                                            var isNewFile = false;
                                            cacheKey = cacheKey.Copy();
                                            TmphInterlocked.NoCheckCompareSetSleep0(ref cacheLock);
                                            try
                                            {
                                                if ((fileCache = cache.Get(cacheKey, null)) == null)
                                                {
                                                    cache.Set(cacheKey, fileCache = new TmphFileCache());
                                                    isNewFile = true;
                                                }
                                            }
                                            finally
                                            {
                                                cacheLock = 0;
                                            }
                                            if (isNewFile)
                                            {
                                                try
                                                {
                                                    fileCache.lastModified = file.LastWriteTimeUtc.toBytes();
                                                    if (ifModifiedSince.Count == fileCache.lastModified.Length &&
                                                        Unsafe.TmphMemory.Equal(fileCache.lastModified,
                                                            pathFixed + ifModifiedSince.StartIndex,
                                                            ifModifiedSince.Count))
                                                        return
                                                            new TmphKeyValue<TmphResponse, TmphFileCache>(
                                                                TmphResponse.NotChanged304, null);
                                                    if (file.Length <= fileSize)
                                                    {
                                                        var isHtml = extensionName == "html" || extensionName == "htm";
                                                        var fileData = readCacheFile(extensionName);
                                                        fileCache.Set(fileData,
                                                            isHtml
                                                                ? HtmlContentType
                                                                : TmphContentTypeInfo.GetContentType(extensionName),
                                                            isCompress(extensionName), isHtml);
                                                        int cacheSize = fileCache.Size,
                                                            minSize = this.cacheSize <= cacheSize
                                                                ? cacheSize
                                                                : this.cacheSize;
                                                        TmphInterlocked.NoCheckCompareSetSleep0(ref cacheLock);
                                                        try
                                                        {
                                                            cache.Set(cacheKey, fileCache);
                                                            for (currentCacheSize += cacheSize;
                                                                currentCacheSize > minSize;
                                                                currentCacheSize -= cache.Pop().Value.Size)
                                                                ;
                                                        }
                                                        finally
                                                        {
                                                            cacheLock = 0;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var fileResponse = TmphResponse.Get(true);
                                                        fileResponse.State = TmphResponse.TmphState.Ok200;
                                                        fileResponse.BodyFile = fileName;
                                                        fileResponse.CacheControl = cacheControl;
                                                        fileResponse.ContentType = extensionName == "html" ||
                                                                                   extensionName == "htm"
                                                            ? HtmlContentType
                                                            : TmphContentTypeInfo.GetContentType(extensionName);
                                                        fileResponse.LastModified = fileCache.lastModified;
                                                        return new TmphKeyValue<TmphResponse, TmphFileCache>(fileResponse, null);
                                                    }
                                                }
                                                finally
                                                {
                                                    if (fileCache.IsData == 0)
                                                    {
                                                        fileCache.IsData = 1;
                                                        TmphInterlocked.NoCheckCompareSetSleep0(ref cacheLock);
                                                        try
                                                        {
                                                            if (cache.Remove(cacheKey, out fileCache))
                                                                fileCache.Dispose();
                                                        }
                                                        finally
                                                        {
                                                            cacheLock = 0;
                                                            fileCache = null;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            return new TmphKeyValue<TmphResponse, TmphFileCache>(null, fileCache);
                        }
                    }
                    finally
                    {
                        cacheNameBuffer.Push(ref TmphBuffer);
                    }
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, cacheFileName, false);
            }
            return default(TmphKeyValue<TmphResponse, TmphFileCache>);
        }

        /// <summary>
        ///     读取缓存文件内容
        /// </summary>
        /// <param name="extensionName">文件扩展名</param>
        /// <returns>文件内容</returns>
        protected virtual TmphSubArray<byte> readCacheFile(TmphSubString extensionName)
        {
            return ReadCacheFile(extensionName.value, WebConfig.IsFileCacheHeader);
        }

        /// <summary>
        ///     读取缓存文件内容
        /// </summary>
        /// <param name="extensionName"></param>
        /// <param name="isFileCacheHeader"></param>
        /// <returns></returns>
        public static TmphSubArray<byte> ReadCacheFile(string extensionName, bool isFileCacheHeader)
        {
            if (isFileCacheHeader)
            {
                using (var fileStream = new FileStream(extensionName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var length = (int)fileStream.Length;
                    var data = new byte[TmphFileCache.HttpHeaderSize + length];
                    fileStream.Read(data, TmphFileCache.HttpHeaderSize, length);
                    return TmphSubArray<byte>.Unsafe(data, TmphFileCache.HttpHeaderSize, length);
                }
            }
            return new TmphSubArray<byte>(File.ReadAllBytes(extensionName));
        }

        /// <summary>
        ///     HTTP文件请求处理
        /// </summary>
        /// <param name="request">请求头部信息</param>
        /// <returns>HTTP响应</returns>
        protected TmphResponse file(TmphRequestHeader request)
        {
            try
            {
                return file(request, file(request.Path, request.IfModifiedSince));
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
            }
            return null;
        }

        /// <summary>
        ///     HTTP文件请求处理
        /// </summary>
        /// <param name="request">请求头部信息</param>
        /// <param name="path">重定向URL</param>
        /// <returns>HTTP响应</returns>
        protected TmphResponse file(TmphRequestHeader request, byte[] path)
        {
            try
            {
                return file(request, file(TmphSubArray<byte>.Unsafe(path, 0, path.Length), request.IfModifiedSince));
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
            }
            return null;
        }

        /// <summary>
        ///     HTTP文件请求处理
        /// </summary>
        /// <param name="request">请求头部信息</param>
        /// <param name="cache">文件输出信息</param>
        /// <returns>HTTP响应</returns>
        private TmphResponse file(TmphRequestHeader request, TmphKeyValue<TmphResponse, TmphFileCache> cache)
        {
            var fileCache = cache.Value;
            if (fileCache == null)
            {
                var fileResponse = cache.Key;
                if (fileResponse != null && fileResponse.BodyFile != null
                    && request.IsRange && !request.FormatRange(fileResponse.BodySize))
                {
                    return TmphResponse.RangeNotSatisfiable416;
                }
                return fileResponse;
            }
            if (request.IsRange && !request.FormatRange(fileCache.Data.Count))
            {
                return TmphResponse.RangeNotSatisfiable416;
            }
            var body = request.IsGZip && !request.IsRange ? fileCache.GZipData : fileCache.Data;
            var response = TmphResponse.Get(true);
            response.State = TmphResponse.TmphState.Ok200;
            response.SetBody(body, true,
                body.StartIndex == TmphFileCache.HttpHeaderSize && (fileCache.IsHtml ? isCacheHtmlHeader : isCacheHeader));
            response.CacheControl = cacheControl;
            response.ContentType = fileCache.ContentType;
            if (body.Array != fileCache.Data.Array) response.ContentEncoding = TmphResponse.GZipEncoding;
            response.LastModified = fileCache.lastModified;
            return response;
        }

        /// <summary>
        ///     设置文件缓存
        /// </summary>
        /// <param name="request">请求头部信息</param>
        /// <param name="response">HTTP响应信息</param>
        /// <param name="contentType">HTTP响应输出类型</param>
        protected unsafe void setCache(TmphRequestHeader request, TmphResponse response, byte[] contentType)
        {
            var path = request.Path;
            if (path.Count != 0 && path.Count <= TmphFile.MaxFullNameLength)
            {
                try
                {
                    fixed (byte* pathFixed = path.Array)
                    {
                        byte[] TmphBuffer;
                        var pathStart = pathFixed + path.StartIndex;
                        if (*pathStart == '/')
                        {
                            ++pathStart;
                            TmphBuffer = new byte[path.Count - 1];
                        }
                        else TmphBuffer = new byte[path.Count];
                        fixed (byte* bufferFixed = TmphBuffer)
                            Unsafe.TmphMemory.ToLower(pathStart, pathStart + TmphBuffer.Length, bufferFixed);
                        TmphHashBytes cacheKey = TmphSubArray<byte>.Unsafe(TmphBuffer, 0, TmphBuffer.Length);
                        var fileCache = new TmphFileCache();
                        fileCache.Set(response.Body, contentType, false, response.LastModified, false);
                        int cacheSize = fileCache.Size,
                            minSize = this.cacheSize <= cacheSize ? cacheSize : this.cacheSize;
                        TmphInterlocked.NoCheckCompareSetSleep0(ref cacheLock);
                        try
                        {
                            var oldFileCache = cache.Set(cacheKey, fileCache);
                            currentCacheSize += cacheSize;
                            if (oldFileCache == null)
                            {
                                while (currentCacheSize > minSize)
                                {
                                    var removeFileCache = cache.Pop().Value;
                                    currentCacheSize -= removeFileCache.Size;
                                    removeFileCache.Dispose();
                                }
                            }
                            else
                            {
                                currentCacheSize -= oldFileCache.Size;
                                oldFileCache.Dispose();
                            }
                        }
                        finally
                        {
                            cacheLock = 0;
                        }
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
            }
        }

        /// <summary>
        ///     是否允许文件扩展名
        /// </summary>
        /// <param name="extensionName">文件扩展名</param>
        /// <returns>是否允许文件扩展名</returns>
        protected virtual bool isFile(TmphSubString extensionName)
        {
            return extensionName.Length != 0 && defaultExtensionNames.Contains(extensionName);
        }

        /// <summary>
        ///     是否允许压缩文件扩展名
        /// </summary>
        /// <param name="extensionName">文件扩展名</param>
        /// <returns>是否允许压缩文件扩展名</returns>
        protected virtual bool isCompress(string extensionName)
        {
            return !defaultCompressExtensionNames.Contains(extensionName);
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        protected virtual bool dispose()
        {
            isStart = 1;
            if (Interlocked.Increment(ref isDisposed) == 1)
            {
                if (onStop != null) onStop();
                if (cache != null)
                {
                    while (cache.Count != 0) cache.Pop().Value.Dispose();
                    cache = null;
                }
                return true;
            }
            isDisposed = 1;
            return false;
        }

        /// <summary>
        ///     网站生成配置
        /// </summary>
        private sealed class TmphNullWebConfig : TmphWebConfig
        {
            /// <summary>
            ///     网站生成配置
            /// </summary>
            public static readonly TmphNullWebConfig Default = new TmphNullWebConfig();

            /// <summary>
            ///     默认Cookie域名
            /// </summary>
            public override string CookieDomain
            {
                get { return null; }
            }

            /// <summary>
            ///     视图加载失败重定向
            /// </summary>
            public override string NoViewLocation
            {
                get { return null; }
            }
        }

        /// <summary>
        ///     文件缓存
        /// </summary>
        protected sealed class TmphFileCache : IDisposable
        {
            /// <summary>
            ///     HTTP头部预留字节数
            /// </summary>
            public const int HttpHeaderSize = 256 + 64;

            /// <summary>
            ///     文件数据
            /// </summary>
            private TmphSubArray<byte> data;

            /// <summary>
            ///     文件压缩数据
            /// </summary>
            private TmphSubArray<byte> gZipData;

            /// <summary>
            ///     是否已经获取数据
            /// </summary>
            internal int IsData;

            /// <summary>
            ///     是否HTML
            /// </summary>
            internal bool IsHtml;

            /// <summary>
            ///     最后修改时间
            /// </summary>
            internal byte[] lastModified;

            /// <summary>
            ///     文件数据
            /// </summary>
            public TmphSubArray<byte> Data
            {
                get
                {
                    if (IsData == 0)
                    {
                        Thread.Sleep(0);
                        while (IsData == 0) Thread.Sleep(1);
                    }
                    return data;
                }
            }

            /// <summary>
            ///     文件数据
            /// </summary>
            public TmphSubArray<byte> GZipData
            {
                get
                {
                    if (IsData == 0)
                    {
                        Thread.Sleep(0);
                        while (IsData == 0) Thread.Sleep(1);
                    }
                    return gZipData;
                }
            }

            /// <summary>
            ///     最后修改时间
            /// </summary>
            public byte[] LastModified
            {
                get { return lastModified; }
            }

            /// <summary>
            ///     HTTP响应输出内容类型
            /// </summary>
            public byte[] ContentType { get; internal set; }

            /// <summary>
            ///     文件数据字节数
            /// </summary>
            public int Size
            {
                get
                {
                    var size = data.Count;
                    if (data.Array != gZipData.Array) size += gZipData.Count;
                    return size;
                }
            }

            /// <summary>
            ///     释放资源
            /// </summary>
            public void Dispose()
            {
                TmphDate.ByteBuffers.Push(ref lastModified);
            }

            /// <summary>
            ///     文件缓存
            /// </summary>
            /// <param name="data">文件数据</param>
            /// <param name="contentType">HTTP响应输出内容类型</param>
            /// <param name="isGZip">是否压缩</param>
            /// <param name="lastModified">最后修改时间</param>
            internal void Set(TmphSubArray<byte> data, byte[] contentType, bool isGZip, byte[] lastModified, bool isHtml)
            {
                lastModified = TmphDate.CopyBytes(lastModified);
                Set(data, contentType, isGZip, isHtml);
            }

            /// <summary>
            ///     文件缓存
            /// </summary>
            /// <param name="data">文件数据</param>
            /// <param name="contentType">HTTP响应输出内容类型</param>
            /// <param name="isGZip">是否压缩</param>
            internal void Set(TmphSubArray<byte> data, byte[] contentType, bool isGZip, bool isHtml)
            {
                ContentType = contentType;
                IsHtml = isHtml;
                try
                {
                    this.data = data;
                    if (isGZip) gZipData = TmphResponse.GetCompress(data, null, data.StartIndex);
                    if (gZipData.Count == 0) gZipData = data;
                }
                finally
                {
                    IsData = 1;
                }
            }
        }

        /// <summary>
        ///     默认扩展名唯一哈希
        /// </summary>
        private struct TmphDefaultExtensionName : IEquatable<TmphDefaultExtensionName>
        {
            /// <summary>
            ///     扩展名
            /// </summary>
            public TmphSubString ExtensionName;

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public bool Equals(TmphDefaultExtensionName other)
            {
                return ExtensionName.Equals(other.ExtensionName);
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="name">扩展名</param>
            /// <returns>默认扩展名唯一哈希</returns>
            public static implicit operator TmphDefaultExtensionName(string name)
            {
                return new TmphDefaultExtensionName { ExtensionName = TmphSubString.Unsafe(name, 0) };
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="name">扩展名</param>
            /// <returns>默认扩展名唯一哈希</returns>
            public static implicit operator TmphDefaultExtensionName(TmphSubString name)
            {
                return new TmphDefaultExtensionName { ExtensionName = name };
            }

            /// <summary>
            ///     获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override unsafe int GetHashCode()
            {
                fixed (char* nameFixed = ExtensionName.value)
                {
                    var start = nameFixed + ExtensionName.StartIndex;
                    var code = (uint)(start[ExtensionName.Length - 2] << 16) +
                               (uint)(start[ExtensionName.Length >> 2] << 8) + start[ExtensionName.Length - 1];
                    return (int)(((code >> 11) ^ (code >> 6) ^ code) & ((1U << 7) - 1));
                }
            }

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((TmphDefaultExtensionName)obj);
            }
        }

        /// <summary>
        ///     默认非压缩扩展名唯一哈希
        /// </summary>
        private struct TmphDefaultCompressExtensionName : IEquatable<TmphDefaultCompressExtensionName>
        {
            /// <summary>
            ///     非压缩扩展名
            /// </summary>
            public string ExtensionName;

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public bool Equals(TmphDefaultCompressExtensionName other)
            {
                return ExtensionName == other.ExtensionName;
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="name">非压缩扩展名</param>
            /// <returns>默认扩展名唯一哈希</returns>
            public static implicit operator TmphDefaultCompressExtensionName(string name)
            {
                return new TmphDefaultCompressExtensionName { ExtensionName = name };
            }

            /// <summary>
            ///     获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override unsafe int GetHashCode()
            {
                fixed (char* nameFixed = ExtensionName)
                {
                    var code = (nameFixed[ExtensionName.Length >> 2] << 8) + nameFixed[1];
                    return (nameFixed[ExtensionName.Length - 1] ^ (code >> 3) ^ (code >> 5)) & ((1 << 5) - 1);
                }
            }

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((TmphDefaultCompressExtensionName)obj);
            }
        }

        /// <summary>
        ///     重定向服务
        /// </summary>
        public abstract class TmphLocationServer : TmphDomainServer
        {
            /// <summary>
            ///     重定向域名
            /// </summary>
            private byte[] locationDomain;

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
                if (isStart == 0)
                {
                    var domain = getLocationDomain();
                    if (domain.Length() != 0)
                    {
                        if (domain[domain.Length - 1] != '/') domain += "/";
                        var domainData = domain.GetBytes();
                        if (Interlocked.CompareExchange(ref isStart, 1, 0) == 0)
                        {
                            locationDomain = domainData;
                            this.domains = domains;
                            this.onStop = onStop;
                            return true;
                        }
                    }
                }
                return false;
            }

            /// <summary>
            ///     获取包含协议的重定向域名,比如 http://www.ligudan.com
            /// </summary>
            /// <returns>获取包含协议的重定向域名</returns>
            protected abstract string getLocationDomain();

            /// <summary>
            ///     HTTP请求处理
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            /// <param name="socketIdentity">套接字操作编号</param>
            public override unsafe void Request(TmphSocketBase socket, long socketIdentity)
            {
                var request = socket.RequestHeader;
                if ((request.ContentLength | request.Boundary.Count) == 0)
                {
                    var response = TmphResponse.Get(true);
                    response.State = TmphResponse.TmphState.MovedPermanently301;
                    var uri = request.Uri;
                    if (uri.Count != 0 && locationDomain.Length + uri.Count <= cacheNameBuffer.Size)
                    {
                        fixed (byte* uriFixed = uri.Array)
                        {
                            var uriStart = uriFixed + uri.StartIndex;
                            var length = uri.Count;
                            if (*uriStart == '/')
                            {
                                --length;
                                ++uriStart;
                            }
                            if (length != 0)
                            {
                                var TmphBuffer = cacheNameBuffer.Get(0);
                                try
                                {
                                    response.Location.UnsafeSet(TmphBuffer, 0, locationDomain.Length + length);
                                    fixed (byte* locationFixed = TmphBuffer)
                                    {
                                        Unsafe.TmphMemory.Copy(locationDomain, locationFixed, locationDomain.Length);
                                        Unsafe.TmphMemory.Copy(uriStart, locationFixed + locationDomain.Length, length);
                                    }
                                    socket.Response(socketIdentity, ref response);
                                    return;
                                }
                                finally
                                {
                                    cacheNameBuffer.Push(ref TmphBuffer);
                                }
                            }
                        }
                    }
                    response.Location.UnsafeSet(locationDomain, 0, locationDomain.Length);
                    TmphInterlocked.NoCheckCompareSetSleep0(ref cacheLock);
                    try
                    {
                        socket.Response(socketIdentity, ref response);
                    }
                    finally
                    {
                        cacheLock = 0;
                    }
                }
                else socket.ResponseError(socketIdentity, TmphResponse.TmphState.BadRequest400);
            }
        }

        /// <summary>
        ///     文件服务
        /// </summary>
        public abstract class TmphFileServer : TmphDomainServer
        {
            /// <summary>
            ///     文件监视器
            /// </summary>
            private FileSystemWatcher fileWatcher;

            /// <summary>
            ///     启动HTTP服务
            /// </summary>
            /// <param name="domains">域名信息集合</param>
            /// <param name="onStop">停止服务处理</param>
            /// <returns>是否启动成功</returns>
            public override bool Start(TmphDomain[] domains, Action onStop)
            {
                var path = (this.path.ToLower() ?? LoadCheckPath).pathSuffix().ToLower();
                if (Directory.Exists(path) && Interlocked.CompareExchange(ref isStart, 1, 0) == 0)
                {
                    WorkPath = path;
                    this.domains = domains;
                    this.onStop = onStop;
                    setCache();
                    fileWatcher = new FileSystemWatcher(path);
                    fileWatcher.IncludeSubdirectories = true;
                    fileWatcher.EnableRaisingEvents = true;
                    fileWatcher.Changed += fileChanged;
                    fileWatcher.Deleted += fileChanged;
                    createErrorResponse();
                    return true;
                }
                return false;
            }

            /// <summary>
            ///     文件更新事件
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private unsafe void fileChanged(object sender, FileSystemEventArgs e)
            {
                var fullPath = e.FullPath;
                var TmphBuffer = cacheNameBuffer.Get();
                try
                {
                    fixed (byte* bufferFixed = TmphBuffer)
                    fixed (char* pathFixed = fullPath)
                    {
                        var write = bufferFixed;
                        char* start = pathFixed + WorkPath.Length, end = start + fullPath.Length;
                        while (start != end)
                        {
                            var value = *start++;
                            if ((uint)(value - 'A') < 26) *write++ = (byte)(value | 0x20);
                            else *write++ = (byte)value;
                        }
                        TmphHashBytes cacheKey = TmphSubArray<byte>.Unsafe(TmphBuffer, 0, (int)(write - bufferFixed));
                        TmphFileCache cacheData;
                        TmphInterlocked.NoCheckCompareSetSleep0(ref cacheLock);
                        try
                        {
                            if (cache.Remove(cacheKey, out cacheData))
                            {
                                currentCacheSize -= cacheData.Size;
                                cacheData.Dispose();
                            }
                        }
                        finally
                        {
                            cacheLock = 0;
                        }
                    }
                }
                finally
                {
                    cacheNameBuffer.Push(ref TmphBuffer);
                }
            }

            /// <summary>
            ///     HTTP请求处理
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            /// <param name="socketIdentity">套接字操作编号</param>
            public override void Request(TmphSocketBase socket, long socketIdentity)
            {
                var response = file(socket.RequestHeader);
                if (response != null) socket.Response(socketIdentity, ref response);
                else socket.ResponseError(socketIdentity, TmphResponse.TmphState.NotFound404);
            }

            /// <summary>
            ///     释放资源
            /// </summary>
            protected override bool dispose()
            {
                if (base.dispose())
                {
                    if (fileWatcher != null)
                    {
                        fileWatcher.EnableRaisingEvents = false;
                        fileWatcher.Changed -= fileChanged;
                        fileWatcher.Deleted -= fileChanged;
                        fileWatcher.Dispose();
                    }
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        ///     静态文件服务
        /// </summary>
        public abstract class TmphStaticFileServer : TmphFileServer
        {
            /// <summary>
            ///     客户端缓存时间(单位:秒)
            /// </summary>
            protected override int clientCacheSeconds
            {
                get { return 10 * 365 * 24 * 60 * 60; }
            }

            /// <summary>
            ///     文件缓存是否预留HTTP头部
            /// </summary>
            protected override bool isCacheHttpHeader
            {
                get { return true; }
            }

            /// <summary>
            ///     HTTP请求处理
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            /// <param name="socketIdentity">套接字操作编号</param>
            public override void Request(TmphSocketBase socket, long socketIdentity)
            {
                var request = socket.RequestHeader;
                if (request.IfModifiedSince.Count == 0) this.request(socket, socketIdentity, request);
                else socket.Response(socketIdentity, TmphResponse.NotChanged304);
            }

            /// <summary>
            ///     HTTP请求处理
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            /// <param name="socketIdentity">套接字操作编号</param>
            /// <param name="request">请求头部信息</param>
            protected virtual void request(TmphSocketBase socket, long socketIdentity, TmphRequestHeader request)
            {
                var response = file(request);
                if (response != null) socket.Response(socketIdentity, ref response);
                else socket.ResponseError(socketIdentity, TmphResponse.TmphState.NotFound404);
            }
        }

        /// <summary>
        ///     WEB视图服务
        /// </summary>
        public abstract class TmphViewServer : TmphFileServer
        {
            /// <summary>
            ///     WEB调用处理委托集合
            /// </summary>
            private TmphStateSearcher.TmphAscii<Action<TmphSocketBase, long, TmphRequestHeader>> callMethods;

            /// <summary>
            ///     WEB视图处理委托集合
            /// </summary>
            private TmphStateSearcher.TmphAscii<Action<TmphSocketBase, long, TmphRequestHeader>> viewMethods;

            /// <summary>
            ///     WEB视图URL重写路径集合
            /// </summary>
            private TmphStateSearcher.TmphAscii<byte[]> viewRewrites;

            /// <summary>
            ///     WEB视图处理集合
            /// </summary>
            protected virtual TmphKeyValue<string[], Action<TmphSocketBase, long, TmphRequestHeader>[]> views
            {
                get
                {
                    return
                        new TmphKeyValue<string[], Action<TmphSocketBase, long, TmphRequestHeader>[]>(
                            TmphNullValue<string>.Array, TmphNullValue<Action<TmphSocketBase, long, TmphRequestHeader>>.Array);
                }
            }

            /// <summary>
            ///     WEB视图URL重写路径集合
            /// </summary>
            protected virtual TmphKeyValue<string[], string[]> rewrites
            {
                get { return new TmphKeyValue<string[], string[]>(TmphNullValue<string>.Array, TmphNullValue<string>.Array); }
            }

            /// <summary>
            ///     WEB调用处理集合
            /// </summary>
            protected virtual TmphKeyValue<string[], Action<TmphSocketBase, long, TmphRequestHeader>[]> calls
            {
                get
                {
                    return
                        new TmphKeyValue<string[], Action<TmphSocketBase, long, TmphRequestHeader>[]>(
                            TmphNullValue<string>.Array, TmphNullValue<Action<TmphSocketBase, long, TmphRequestHeader>>.Array);
                }
            }

            /// <summary>
            ///     HTML文件缓存是否预留HTTP头部
            /// </summary>
            protected override bool isCacheHtmlHttpHeader
            {
                get { return true; }
            }

            /// <summary>
            ///     释放资源
            /// </summary>
            /// <returns></returns>
            protected override bool dispose()
            {
                if (base.dispose())
                {
                    TmphPub.Dispose(ref viewMethods);
                    TmphPub.Dispose(ref viewRewrites);
                    TmphPub.Dispose(ref callMethods);
                    return true;
                }
                return false;
            }

            /// <summary>
            ///     启动HTTP服务
            /// </summary>
            /// <param name="domains">域名信息集合</param>
            /// <param name="onStop">停止服务处理</param>
            /// <returns>是否启动成功</returns>
            public override bool Start(TmphDomain[] domains, Action onStop)
            {
                var views = this.views;
                viewMethods = new TmphStateSearcher.TmphAscii<Action<TmphSocketBase, long, TmphRequestHeader>>(views.Key,
                    views.Value);
                var calls = this.calls;
                callMethods = new TmphStateSearcher.TmphAscii<Action<TmphSocketBase, long, TmphRequestHeader>>(calls.Key,
                    calls.Value);
                var rewrites = this.rewrites;
                viewRewrites = new TmphStateSearcher.TmphAscii<byte[]>(rewrites.Key,
                    rewrites.Value.getArray(value => value.GetBytes()));
                return base.Start(domains, onStop);
            }

            /// <summary>
            ///     HTTP请求处理
            /// </summary>
            /// <param name="socket">HTTP套接字</param>
            /// <param name="socketIdentity">套接字操作编号</param>
            public override void Request(TmphSocketBase socket, long socketIdentity)
            {
                var request = socket.RequestHeader;
                Action<TmphSocketBase, long, TmphRequestHeader> view = null;
                if (request.IsSearchEngine)
                {
                    if (request.IsViewPath)
                    {
                        var path = viewRewrites.Get(WebConfig.IgnoreCase ? request.LowerPath : request.Path);
                        if (path != null) view = viewMethods.Get(path);
                    }
                    if (view == null)
                        view = viewMethods.Get(WebConfig.IgnoreCase ? request.LowerPath : request.Path) ??
                               callMethods.Get(WebConfig.IgnoreCase ? request.LowerPath : request.Path);
                }
                else view = callMethods.Get(WebConfig.IgnoreCase ? request.LowerPath : request.Path);
                if (view == null)
                {
                    if (!request.IsSearchEngine && request.IsViewPath)
                    {
                        var path = viewRewrites.Get(WebConfig.IgnoreCase ? request.LowerPath : request.Path);
                        if (path != null)
                        {
                            var response = file(socket.RequestHeader, path);
                            if (response != null)
                            {
                                socket.Response(socketIdentity, ref response);
                                return;
                            }
                        }
                        socket.ResponseError(socketIdentity, TmphResponse.TmphState.NotFound404);
                    }
                    else base.Request(socket, socketIdentity);
                }
                else
                {
                    try
                    {
                        view(socket, socketIdentity, request);
                    }
                    catch (Exception error)
                    {
                        socket.ResponseError(socketIdentity, TmphResponse.TmphState.ServerError500);
                        TmphLog.Error.Add(error, null, false);
                    }
                }
            }

            /// <summary>
            ///     获取WEB视图URL重写路径
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public override byte[] GetViewRewrite(TmphSubArray<byte> path)
            {
                return viewRewrites.Get(path);
            }

            /// <summary>
            ///     加载页面视图
            /// </summary>
            /// <param name="socket">HTTP套接字接口</param>
            /// <param name="socketIdentity">套接字操作编号</param>
            /// <param name="request">HTTP请求头</param>
            /// <param name="view">WEB视图接口</param>
            /// <param name="isPool">是否使用WEB视图池</param>
            protected void load<viewType>(TmphSocketBase socket, long socketIdentity, TmphRequestHeader request,
                viewType view, bool isPool)
                where viewType : TmphWebView.TmphView
            {
                if ((request.ContentLength | request.Boundary.Count) == 0 && request.Method == TmphHttp.TmphMethodType.GET)
                {
                    view.Socket = socket;
                    view.DomainServer = this;
                    if (view.LoadHeader(socketIdentity, request, isPool))
                    {
                        view.Load(null, false);
                        return;
                    }
                }
                else if (isPool) TmphTypePool<viewType>.Push(view);
                socket.ResponseError(socketIdentity, TmphResponse.TmphState.ServerError500);
            }

            /// <summary>
            ///     加载web调用
            /// </summary>
            /// <param name="socket">HTTP套接字接口</param>
            /// <param name="socketIdentity">套接字操作编号</param>
            /// <param name="request">HTTP请求头</param>
            /// <param name="call">web调用</param>
            /// <param name="maxPostDataSize"></param>
            /// <param name="maxMemoryStreamSize"></param>
            /// <param name="isOnlyPost"></param>
            /// <param name="isPool"></param>
            protected void load<TCallType, webType>(TmphSocketBase socket, long socketIdentity, TmphRequestHeader request,
                TCallType call
                , int maxPostDataSize, int maxMemoryStreamSize, bool isOnlyPost, bool isPool)
                where TCallType : TmphWebCall.TmphCallPool<TCallType, webType>
                where webType : TmphWebPage.TmphPage, TmphWebCall.IWebCall
            {
                if (request.ContentLength <= maxPostDataSize &&
                    (request.Method == TmphHttp.TmphMethodType.POST || !isOnlyPost))
                {
                    var webCall = call.WebCall;
                    webCall.Socket = socket;
                    webCall.DomainServer = this;
                    webCall.LoadHeader(socketIdentity, request, isPool);
                    if (request.Method == TmphHttp.TmphMethodType.POST)
                    {
                        socket.GetForm(socketIdentity,
                            TmphLoadForm<TCallType, webType>.Get(socket, request, call, maxMemoryStreamSize));
                        return;
                    }
                    webCall.RequestForm = null;
                    var response = webCall.Response = TmphResponse.Get(true);
                    try
                    {
                        if (call.Call()) return;
                    }
                    finally
                    {
                        TmphResponse.Push(ref response);
                    }
                }
                else
                {
                    if (isPool) TmphTypePool<webType>.Push(ref call.WebCall);
                    TmphTypePool<TCallType>.Push(call);
                }
                socket.ResponseError(socketIdentity, TmphResponse.TmphState.ServerError500);
            }

            /// <summary>
            ///     加载web调用
            /// </summary>
            /// <param name="socket">HTTP套接字接口</param>
            /// <param name="socketIdentity">套接字操作编号</param>
            /// <param name="request">HTTP请求头</param>
            /// <param name="call">web调用</param>
            protected void loadAjax<TCallType, webType>(TmphSocketBase socket, long socketIdentity, TmphRequestHeader request,
                TCallType call)
                where TCallType : TmphWebCall.TmphCallPool<TCallType, webType>
                where webType : TmphWebCall.TmphCall, TmphWebCall.IWebCall
            {
                var webCall = call.WebCall;
                webCall.Socket = socket;
                webCall.DomainServer = this;
                if (webCall.LoadHeader(socketIdentity, request, true) && call.Call()) return;
                socket.ResponseError(socketIdentity, TmphResponse.TmphState.ServerError500);
            }

            /// <summary>
            ///     表单加载
            /// </summary>
            private class TmphLoadForm<TCallType, webType> : TmphRequestForm.TmphILoadForm
                where TCallType : TmphWebCall.TmphCallPool<TCallType, webType>
                where webType : TmphWebPage.TmphPage, TmphWebCall.IWebCall
            {
                /// <summary>
                ///     内存流最大字节数
                /// </summary>
                private int maxMemoryStreamSize;

                /// <summary>
                ///     HTTP请求头
                /// </summary>
                private TmphRequestHeader request;

                /// <summary>
                ///     HTTP套接字接口
                /// </summary>
                private TmphSocketBase socket;

                /// <summary>
                ///     WEB调用
                /// </summary>
                private TCallType webCall;

                /// <summary>
                ///     表单加载
                /// </summary>
                private TmphLoadForm()
                {
                }

                /// <summary>
                ///     表单回调处理
                /// </summary>
                /// <param name="form">HTTP请求表单</param>
                public void OnGetForm(TmphRequestForm form)
                {
                    long identity;
                    if (form == null)
                    {
                        identity = webCall.WebCall.SocketIdentity;
                        webCall.WebCall.PushPool();
                        webCall.WebCall = null;
                        TmphTypePool<TCallType>.Push(webCall);
                    }
                    else
                    {
                        identity = form.Identity;
                        TmphResponse response = null;
                        try
                        {
                            var call = webCall.WebCall;
                            call.Response = TmphResponse.Get(true);
                            call.SocketIdentity = identity;
                            call.RequestForm = form;
                            if (webCall.Call()) return;
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                        }
                        finally
                        {
                            TmphResponse.Push(ref response);
                        }
                    }
                    socket.ResponseError(identity, TmphResponse.TmphState.ServerError500);
                    socket = null;
                    request = null;
                    webCall = null;
                    TmphTypePool<TmphLoadForm<TCallType, webType>>.Push(this);
                }

                /// <summary>
                ///     根据HTTP请求表单值获取内存流最大字节数
                /// </summary>
                /// <param name="value">HTTP请求表单值</param>
                /// <returns>内存流最大字节数</returns>
                public int MaxMemoryStreamSize(TmphRequestForm.TmphValue value)
                {
                    return maxMemoryStreamSize > 0 ? maxMemoryStreamSize : TmphAppSetting.StreamBufferSize;
                }

                /// <summary>
                ///     根据HTTP请求表单值获取保存文件全称
                /// </summary>
                /// <param name="value">HTTP请求表单值</param>
                /// <returns>文件全称</returns>
                public string GetSaveFileName(TmphRequestForm.TmphValue value)
                {
                    return webCall.WebCall.GetSaveFileName(value);
                }

                /// <summary>
                ///     获取表单加载
                /// </summary>
                /// <param name="socket">HTTP套接字接口</param>
                /// <param name="request">HTTP请求头</param>
                /// <param name="webCall">WEB调用接口</param>
                /// <param name="maxMemoryStreamSize">内存流最大字节数</param>
                /// <returns>表单加载</returns>
                public static TmphLoadForm<TCallType, webType> Get(TmphSocketBase socket, TmphRequestHeader request
                    , TCallType webCall, int maxMemoryStreamSize)
                {
                    var loadForm = TmphTypePool<TmphLoadForm<TCallType, webType>>.Pop() ??
                                   new TmphLoadForm<TCallType, webType>();
                    loadForm.socket = socket;
                    loadForm.request = request;
                    loadForm.webCall = webCall;
                    loadForm.maxMemoryStreamSize = maxMemoryStreamSize;
                    return loadForm;
                }
            }
        }

        /// <summary>
        ///     WEB视图服务
        /// </summary>
        public abstract class TmphViewServer<sessionType> : TmphViewServer
        {
            /// <summary>
            ///     Session
            /// </summary>
            private TmphISession<sessionType> session;

            /// <summary>
            ///     启动HTTP服务
            /// </summary>
            /// <param name="domains">域名信息集合</param>
            /// <param name="onStop">停止服务处理</param>
            /// <returns>是否启动成功</returns>
            public override bool Start(TmphDomain[] domains, Action onStop)
            {
                Session = session = getSession();
                return base.Start(domains, onStop);
            }

            /// <summary>
            ///     获取Session
            /// </summary>
            /// <returns>Session</returns>
            protected virtual TmphISession<sessionType> getSession()
            {
                return new TmphSession<sessionType>();
            }
        }
    }
}