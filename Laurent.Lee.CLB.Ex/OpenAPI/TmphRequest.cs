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

using Laurent.Lee.CLB.Net;
using System;
using System.Collections.Specialized;
using System.Text;
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