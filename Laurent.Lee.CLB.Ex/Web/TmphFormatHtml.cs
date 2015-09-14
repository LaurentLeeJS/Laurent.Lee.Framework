using System;
using System.Collections.Generic;

namespace Laurent.Lee.CLB.Web
{
    /// <summary>
    /// HTML安全格式化
    /// </summary>
    public static class TmphFormatHtml
    {
        /// <summary>
        /// 允许tag名称集合
        /// </summary>
        public static readonly string[] TagNames = new string[] { "a", "b", "big", "blockquote", "br", "center", "TmphCode", "dd", "del", "div", "dl", "dt", "em", "font", "h1", "h2", "h3", "h4", "h5", "h6", "hr", "i", "img", "ins", "li", "ol", "p", "pre", "s", "small", "span", "strike", "strong", "sub", "sup", "table", "tbody", "td", "th", "thead", "title", "tr", "u", "ul" };
        /// <summary>
        /// 允许tag名称集合
        /// </summary>
        private static readonly CLB.TmphStateSearcher.TmphAscii<string> tagNames = new CLB.TmphStateSearcher.TmphAscii<string>(TagNames, TagNames);
        /// <summary>
        /// 安全格式化
        /// </summary>
        /// <param name="html">HTML</param>
        /// <returns>HTML</returns>
        public static unsafe string Format(string html)
        {
            if (html.Length() != 0)
            {
                TmphHtmlNode document = new TmphHtmlNode(html);
                document.Remove(node => node.TagName != null && !tagNames.ContainsKey(node.TagName));
                foreach (TmphHtmlNode node in document.Nodes)
                {
                    foreach (string name in node.AttributeNames)
                    {
                        if (!Laurent.Lee.CLB.Web.TmphHtml.SafeAttributes.Contains(name))
                        {
                            if (name == "style") node[name] = formatStyle(node[name]);
                            else if (Laurent.Lee.CLB.Web.TmphHtml.UriAttributes.Contains(name))
                            {
                                if (!IsHttp(node[name])) node[name] = null;
                            }
                            else node[name] = null;
                        }
                    }
                    if (node.TagName != null && node.TagName.Length == 1 && node.TagName[0] == 'a')
                    {
                        string href = node["href"];
                        if (href != null && href.Length != 0 && href[0] != '/') node["target"] = "_blank";
                    }
                }
                return document.Html(true);
            }
            return html;
        }
        /// <summary>
        /// 格式化样式表
        /// </summary>
        /// <param name="style">样式表</param>
        /// <returns>样式表</returns>
        private static unsafe string formatStyle(string style)
        {
            if (style != null)
            {
                Dictionary<TmphHashString, string> values = TmphDictionary.CreateHashString<string>();
                foreach (string value in style.Split(';'))
                {
                    int index = value.IndexOf(':');
                    if (index != -1)
                    {
                        string name = value.Substring(0, index).ToLower();
                        if (Laurent.Lee.CLB.Web.TmphHtml.SafeStyleAttributes.Contains(name)) values[name] = value.Substring(++index);
                    }
                }
                if (values.Count != 0)
                {
                    return values.JoinString(';', value => value.Key.ToString() + ":" + value.Value.Replace('<', ' ').Replace('>', ' ').Replace('&', ' ').Replace('\'', ' ').Replace('"', ' '));
                }
            }
            return null;
        }
        /// <summary>
        /// 判断连接地址是否以http或者https开头
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public unsafe static bool IsHttp(string url)
        {
            if (url.Length() > 7 && url[6] == '/')
            {
                fixed (char* urlFixed = url)
                {
                    if ((*(int*)urlFixed | 0x200020) == 'h' + ('t' << 16) && (*(int*)(urlFixed + 2) | 0x200020) == 't' + ('p' << 16))
                    {
                        if (*(int*)(urlFixed + 4) == ':' + ('/' << 16)) return true;
                        else if ((*(int*)(urlFixed + 4) | 0x20) == 's' + (':' << 16) && urlFixed[7] == '/') return true;
                    }
                }
            }
            return false;
        }
    }
}
