using System;
using System.Collections.Specialized;
using System.Text;
using Laurent.Lee.CLB.Net;
using System.Threading;

namespace Laurent.Lee.CLB.OpenAPI
{
    /// <summary>
    /// web请求
    /// </summary>
    public class TmphRequest : IDisposable
    {
        /// <summary>
        /// web客户端
        /// </summary>
        private readonly TmphWebClient webClient = new TmphWebClient();
        /// <summary>
        /// web客户端 访问锁
        /// </summary>
        private readonly object webClientLock = new object();
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            webClient.Dispose();
            Laurent.Lee.CLB.TmphDomainUnload.Remove(Dispose, false);
        }
        /// <summary>
        /// 公用web请求
        /// </summary>
        public TmphRequest()
        {
            webClient.KeepAlive = false;
            Laurent.Lee.CLB.TmphDomainUnload.Add(Dispose);
        }
        /// <summary>
        /// API请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="encoding">请求地址</param>
        /// <param name="form">POST表单内容</param>
        /// <returns>返回内容,失败为null</returns>
        public string Request(string url, Encoding encoding, NameValueCollection form = null)
        {
            Monitor.Enter(webClientLock);
            try
            {
                return webClient.CrawlHtml(new TmphWebClient.TmphRequest
                {
                    Uri = new Uri(url),
                    Form = form,
                    IsErrorOut = true,
                    IsErrorOutUri = true
                }, encoding);
            }
            finally { Monitor.Exit(webClientLock); }
        }
        /// <summary>
        /// 公用web请求
        /// </summary>
        public static readonly TmphRequest Default = new TmphRequest();
    }
}
