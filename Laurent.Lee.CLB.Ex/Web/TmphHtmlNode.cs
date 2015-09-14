using System;
using System.Collections.Generic;
using System.Web;

namespace Laurent.Lee.CLB.Web
{
    /// <summary>
    /// HTML节点
    /// </summary>
    public sealed unsafe class TmphHtmlNode
    {
        /// <summary>
        /// 初始化特殊属性名称唯一哈希
        /// </summary>
        public struct TmphNoLowerAttributeName : IEquatable<TmphNoLowerAttributeName>
        {
            /// <summary>
            /// 初始化特殊属性名称
            /// </summary>
            public string Name;

            /// <summary>
            /// 隐式转换
            /// </summary>
            /// <param name="name">初始化特殊属性名称</param>
            /// <returns>初始化特殊属性名称唯一哈希</returns>
            public static implicit operator TmphNoLowerAttributeName(string name) { return new TmphNoLowerAttributeName { Name = name }; }

            /// <summary>
            /// 获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override int GetHashCode()
            {
                if (Name.Length < 8) return 2;
                return Name[1] & 1;
            }

            /// <summary>
            /// 判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public bool Equals(TmphNoLowerAttributeName other)
            {
                return Name == other.Name;
            }

            /// <summary>
            /// 判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((TmphNoLowerAttributeName)obj);
            }
        }

        /// <summary>
        /// 空隔字符位图
        /// </summary>
        private readonly static TmphString.TmphAsciiMap spaceMap;

        /// <summary>
        /// 空隔+结束字符位图
        /// </summary>
        private readonly static TmphPointer spaceSplitMap;

        /// <summary>
        /// 标签名称结束字符位图
        /// </summary>
        private readonly static TmphPointer tagNameMap;

        /// <summary>
        /// 标签名称开始字符位图
        /// </summary>
        private readonly static TmphPointer tagNameSplitMap;

        /// <summary>
        /// 标签属性分隔结束字符位图
        /// </summary>
        private readonly static TmphPointer attributeSplitMap;

        /// <summary>
        /// 标签属性名称结束字符位图
        /// </summary>
        private readonly static TmphPointer attributeNameSplitMap;

        /// <summary>
        /// 初始化特殊属性名称集合(不能用全小写字母表示的属性名称)
        /// </summary>
        private readonly static TmphUniqueDictionary<TmphNoLowerAttributeName, string> noLowerAttributeNames;

        /// <summary>
        /// 检测属性名称
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <returns>属性名称</returns>
        private static string checkName(string name)
        {
            return noLowerAttributeNames.Get(name, name);
        }

        /// <summary>
        /// HTML编码与文本值
        /// </summary>
        private struct TmphHtmlText
        {
            /// <summary>
            /// 编码过的HTML编码
            /// </summary>
            public string FormatHtml;

            /// <summary>
            /// HTML编码
            /// </summary>
            public string Html
            {
                get
                {
                    if (FormatHtml == null && FormatText != null) FormatHtml = HttpUtility.HtmlEncode(FormatText);
                    return FormatHtml;
                }
                set
                {
                    FormatHtml = value;
                    FormatText = null;
                }
            }

            /// <summary>
            /// 编码过的HTML文本值
            /// </summary>
            public string FormatText;

            /// <summary>
            /// HTML文本值
            /// </summary>
            public string Text
            {
                get
                {
                    if (FormatText == null && FormatHtml != null) FormatText = HttpUtility.HtmlDecode(FormatHtml);
                    return FormatText;
                }
                set
                {
                    FormatText = value;
                    FormatHtml = null;
                }
            }
        }

        /// <summary>
        /// 文本节点值
        /// </summary>
        private TmphHtmlText nodeText;

        /// <summary>
        /// 父节点
        /// </summary>
        public TmphHtmlNode Parent { get; private set; }

        /// <summary>
        /// 标签名称
        /// </summary>
        public string TagName { get; private set; }

        /// <summary>
        /// 子节点集合
        /// </summary>
        private TmphList<TmphHtmlNode> children;

        /// <summary>
        /// 子节点数量
        /// </summary>
        public int ChildrenCount
        {
            get { return children.Count(); }
        }

        /// <summary>
        /// 子节点集合
        /// </summary>
        public TmphHtmlNode[] Children
        {
            get { return children == null ? null : children.ToArray(); }
        }

        /// <summary>
        /// 子节点索引位置
        /// </summary>
        /// <param name="value">子节点</param>
        /// <returns>索引位置</returns>
        public int this[TmphHtmlNode value]
        {
            get
            {
                int index = -1;
                if (value != null && value.Parent == this && children != null)
                {
                    index = children.Count;
                    while (--index >= 0 && children[index] != value) ;
                }
                return index;
            }
        }

        /// <summary>
        /// 属性
        /// </summary>
        private Dictionary<TmphHashString, TmphHtmlText> attributes;

        /// <summary>
        /// 获取或设置属性值
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <returns>属性值</returns>
        public string this[string name]
        {
            get
            {
                if (attributes != null)
                {
                    TmphHtmlText value = new TmphHtmlText();
                    if (name != null && name.Length != 0)
                    {
                        TmphHashString nameKey = checkName(name.ToLower());
                        if (attributes.TryGetValue(nameKey, out value))
                        {
                            if (value.FormatText != value.Text) attributes[nameKey] = value;
                        }
                    }
                    return value.Text;
                }
                return null;
            }
            set
            {
                if (name != null && name.Length != 0)
                {
                    if (value != null)
                    {
                        if (attributes == null) attributes = TmphDictionary.CreateHashString<TmphHtmlText>();
                        attributes[checkName(name.ToLower())] = new TmphHtmlText { FormatText = value };
                    }
                    else if (attributes != null) attributes.Remove(checkName(name.ToLower()));
                }
            }
        }

        /// <summary>
        /// 获取或设置属性值
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <param name="value">属性值</param>
        /// <returns>是否存在属性名称</returns>
        public bool Get(string name, ref string value)
        {
            if (attributes != null)
            {
                TmphHtmlText attribute = new TmphHtmlText();
                if (name != null && name.Length != 0)
                {
                    TmphHashString nameKey = checkName(name.ToLower());
                    if (attributes.TryGetValue(nameKey, out attribute))
                    {
                        value = attribute.Text;
                        if (attribute.FormatText != attribute.Text) attributes[nameKey] = attribute;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 属性名称集合
        /// </summary>
        public string[] AttributeNames
        {
            get
            {
                return attributes != null ? attributes.Keys.GetArray(value => value.ToString()) : TmphNullValue<string>.Array;
            }
        }

        /// <summary>
        /// HTML节点
        /// </summary>
        private TmphHtmlNode() { }

        /// <summary>
        /// 根据HTML解析节点
        /// </summary>
        /// <param name="html">HTML</param>
        public TmphHtmlNode(string html)
        {
            createCheck(html);
        }

        ///// <summary>
        ///// 根据网址解析HTML节点
        ///// </summary>
        ///// <param name="url">网址</param>
        ///// <param name="encoding">编码</param>
        //public htmlNode(Uri url, Encoding encoding)
        //{
        //    using (webClient webClient = new webClient()) createCheck(webClient.CrawlHtml(new webClient.TmphRequest { Uri = url }, encoding));
        //}
        /// <summary>
        /// 根据HTML解析节点
        /// </summary>
        /// <param name="html"></param>
        private void createCheck(string html)
        {
            if (html != null && html.Length != 0)
            {
                char endChar = html[html.Length - 1];
                if (!spaceMap.Get(endChar)) html = html + " ";
                try
                {
                    create(html);
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
            }
            TagName = string.Empty;
        }

        /// <summary>
        /// 解析HTML节点
        /// </summary>
        /// <param name="html"></param>
        private void create(string html)
        {
            int length = html.Length;
            children = new TmphList<TmphHtmlNode>();
            if (length < 2)
            {
                children.Add(new TmphHtmlNode { nodeText = new TmphHtmlText { FormatHtml = html }, Parent = this });
            }
            else
            {
                int nextIndex, nodeCount;
                TmphHtmlNode nextNode;
                fixed (char* htmlFixed = html + "<")
                {
                    TmphFixedMap spaceFixedMap = new TmphFixedMap(spaceMap.Map);
                    TmphFixedMap spaceSplitFixedMap = new TmphFixedMap(spaceSplitMap);
                    TmphFixedMap tagNameFixedMap = new TmphFixedMap(tagNameMap);
                    TmphFixedMap tagNameSplitFixedMap = new TmphFixedMap(tagNameSplitMap);
                    TmphFixedMap attributeSplitFixedMap = new TmphFixedMap(attributeSplitMap);
                    TmphFixedMap attributeNameSplitFixedMap = new TmphFixedMap(attributeNameSplitMap);
                    int startIndex, tagNameLength;
                    string name, htmlValue;
                    char* startChar = htmlFixed, currentChar = htmlFixed, endChar = htmlFixed + length, scriptChar;
                    char splitChar;
                    while (currentChar != endChar)
                    {
                        for (*endChar = '<'; *currentChar != '<'; ++currentChar) ;
                        if (currentChar != endChar)
                        {
                            if ((*++currentChar & 0xff80) == 0)
                            {
                                if (tagNameFixedMap.Get(*currentChar))
                                {
                                    while ((*startChar & 0xffc0) == 0 && spaceFixedMap.Get(*startChar)) ++startChar;
                                    if (startChar != currentChar - 1)
                                    {
                                        for (scriptChar = currentChar - 2; (*scriptChar & 0xffc0) == 0 && spaceFixedMap.Get(*scriptChar); --scriptChar) ;
                                        children.Add(new TmphHtmlNode { nodeText = new TmphHtmlText { FormatHtml = html.Substring((int)(startChar - htmlFixed), (int)(scriptChar - startChar) + 1) } });
                                    }
                                    if (*currentChar == '/')
                                    {
                                        #region 标签回合

                                        startChar = currentChar - 1;
                                        if (++currentChar != endChar)
                                        {
                                            while ((*currentChar & 0xffc0) == 0 && spaceFixedMap.Get(*currentChar)) ++currentChar;
                                            if (currentChar != endChar)
                                            {
                                                if ((uint)((*currentChar | 0x20) - 'a') <= 26)
                                                {
                                                    for (*endChar = '>'; (*currentChar & 0xffc0) != 0 || !tagNameSplitFixedMap.Get(*currentChar); ++currentChar) ;
                                                    TagName = html.Substring((int)((startChar += 2) - htmlFixed), (int)(currentChar - startChar)).ToLower();
                                                    for (startIndex = children.Count - 1; startIndex >= 0 && (children[startIndex].nodeText.FormatHtml != null || children[startIndex].TagName != TagName); --startIndex) ;
                                                    if (startIndex != -1)
                                                    {
                                                        for (nextIndex = children.Count - 1; nextIndex != startIndex; --nextIndex)
                                                        {
                                                            nextNode = children[nextIndex];
                                                            if (nextNode.nodeText.FormatHtml == null)
                                                            {
                                                                if (Web.TmphHtml.MustRoundTagNames.Contains(nextNode.TagName) && (nodeCount = (children.Count - nextIndex - 1)) != 0)
                                                                {
                                                                    nextNode.children = new TmphList<TmphHtmlNode>(children.GetSub(nextIndex + 1, nodeCount), true);
                                                                    children.RemoveRange(nextIndex + 1, nodeCount);
                                                                    foreach (TmphHtmlNode value in nextNode.children) value.Parent = nextNode;
                                                                }
                                                            }
                                                            else if (nextNode.nodeText.FormatHtml.Length == 0) nextNode.nodeText.FormatHtml = null;
                                                        }
                                                        nextNode = children[startIndex];
                                                        if ((nodeCount = children.Count - ++startIndex) != 0)
                                                        {
                                                            nextNode.children = new TmphList<TmphHtmlNode>(children.GetSub(startIndex, nodeCount), true);
                                                            children.RemoveRange(startIndex, nodeCount);
                                                            foreach (TmphHtmlNode value in nextNode.children) value.Parent = nextNode;
                                                        }
                                                        nextNode.nodeText.FormatHtml = string.Empty;//已回合标识
                                                    }
                                                    while (*currentChar != '>') ++currentChar;
                                                    if (currentChar != endChar) ++currentChar;
                                                }
                                                else
                                                {
                                                    for (*endChar = '>'; *currentChar != '>'; ++currentChar) ;
                                                    if (currentChar != endChar) ++currentChar;
                                                    htmlValue = html.Substring((int)(startChar - htmlFixed), (int)(currentChar - startChar));
                                                    children.Add(new TmphHtmlNode { TagName = "/", nodeText = new TmphHtmlText { FormatHtml = htmlValue, FormatText = htmlValue } });
                                                }
                                                startChar = currentChar;
                                            }
                                        }

                                        #endregion 标签回合
                                    }
                                    else if (*currentChar != '!')
                                    {
                                        #region 标签开始

                                        startChar = currentChar;
                                        children.Add(nextNode = new TmphHtmlNode());
                                        for (*endChar = '>'; (*currentChar & 0xffc0) != 0 || !tagNameSplitFixedMap.Get(*currentChar); ++currentChar) ;
                                        nextNode.TagName = html.Substring((int)(startChar - htmlFixed), (int)(currentChar - startChar)).ToLower();
                                        if (currentChar == endChar) startChar = endChar;
                                        else
                                        {
                                            #region 属性解析

                                            if (*currentChar != '>')
                                            {
                                                startChar = ++currentChar;
                                                while (currentChar != endChar)
                                                {
                                                    while ((*currentChar & 0xffc0) == 0 && attributeSplitFixedMap.Get(*currentChar)) ++currentChar;
                                                    if (*currentChar == '>')
                                                    {
                                                        if (currentChar != endChar)
                                                        {
                                                            if (*(currentChar - 1) == '/') nextNode.nodeText.FormatHtml = string.Empty;
                                                            startChar = ++currentChar;
                                                        }
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        for (startChar = currentChar++; (*currentChar & 0xffc0) != 0 || !tagNameSplitFixedMap.Get(*currentChar); ++currentChar) ;
                                                        htmlValue = name = checkName(html.Substring((int)(startChar - htmlFixed), (int)(currentChar - startChar)).ToLower());
                                                        if (currentChar != endChar && ((*currentChar & 0xffc0) != 0 || !attributeNameSplitFixedMap.Get(*currentChar)))
                                                        {
                                                            if (*currentChar != '=')
                                                            {
                                                                while ((*currentChar & 0xffc0) == 0 && spaceFixedMap.Get(*currentChar)) ++currentChar;
                                                            }
                                                            if (*currentChar == '=')
                                                            {
                                                                while ((*++currentChar & 0xffc0) == 0 && spaceFixedMap.Get(*currentChar)) ;
                                                                if ((splitChar = *currentChar) != '>')
                                                                {
                                                                    if (splitChar == '"' || splitChar == '\'')
                                                                    {
                                                                        for (startChar = ++currentChar, *endChar = splitChar; *currentChar != splitChar; ++currentChar) ;
                                                                        *endChar = '>';
                                                                    }
                                                                    else
                                                                    {
                                                                        for (startChar = currentChar++; (*currentChar & 0xffc0) != 0 || !spaceSplitFixedMap.Get(*currentChar); ++currentChar) ;
                                                                    }
                                                                    htmlValue = html.Substring((int)(startChar - htmlFixed), (int)(currentChar - startChar));
                                                                }
                                                            }
                                                        }
                                                        if (nextNode.attributes == null) nextNode.attributes = TmphDictionary.CreateHashString<TmphHtmlText>();
                                                        nextNode.attributes[name] = new TmphHtmlText { FormatHtml = htmlValue };
                                                        if (currentChar != endChar)
                                                        {
                                                            if (*currentChar == '>')
                                                            {
                                                                if (*(currentChar - 1) == '/') nextNode.nodeText.FormatHtml = string.Empty;
                                                                startChar = ++currentChar;
                                                                break;
                                                            }
                                                            startChar = ++currentChar;
                                                        }
                                                    }
                                                }
                                            }
                                            else startChar = ++currentChar;

                                            #endregion 属性解析

                                            #region 非解析标签

                                            if (currentChar == endChar) startChar = endChar;
                                            else if (Web.TmphHtml.NonanalyticTagNames.Contains(TagName = nextNode.TagName))
                                            {
                                                scriptChar = endChar;
                                                tagNameLength = TagName.Length + 2;
                                                fixed (char* tagNameFixed = TagName)
                                                {
                                                    while ((int)(endChar - currentChar) > tagNameLength)
                                                    {
                                                        for (currentChar += tagNameLength; *currentChar != '>'; ++currentChar) ;
                                                        if (currentChar != endChar && *(int*)(currentChar - tagNameLength) == (('/' << 16) + '<'))
                                                        {
                                                            if (Unsafe.TmphString.EqualCase(currentChar - TagName.Length, tagNameFixed, TagName.Length))
                                                            {
                                                                scriptChar = currentChar - tagNameLength;
                                                                if (currentChar != endChar) ++currentChar;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                                if (startChar != scriptChar)
                                                {
                                                    nextNode.nodeText.FormatHtml = nextNode.nodeText.FormatText = html.Substring((int)(startChar - htmlFixed), (int)(scriptChar - startChar));
                                                }
                                                if (scriptChar == endChar) currentChar = endChar;
                                                startChar = currentChar;
                                            }

                                            #endregion 非解析标签
                                        }

                                        #endregion 标签开始
                                    }
                                    else
                                    {
                                        #region 注释

                                        startChar = currentChar - 1;
                                        if (++currentChar != endChar)
                                        {
                                            *endChar = '>';
                                            if ((length = (int)(endChar - currentChar)) > 2 && *(int*)currentChar == (('-' << 16) + '-'))
                                            {
                                                for (currentChar += 2; *currentChar != '>'; ++currentChar) ;
                                                while (currentChar != endChar && *(int*)(currentChar - 2) != (('-' << 16) + '-'))
                                                {
                                                    if ((currentChar += 3) < endChar)
                                                    {
                                                        while (*currentChar != '>') ++currentChar;
                                                    }
                                                    else currentChar = endChar;
                                                }
                                            }
                                            else if (length > 9
                                                && (*(int*)currentChar & 0x200000) == ('[' + ('c' << 16))
                                                && (*(int*)(currentChar + 2) & 0x200020) == ('d' + ('a' << 16))
                                                && (*(int*)(currentChar + 4) & 0x200020) == ('t' + ('a' << 16))
                                                && *(currentChar + 6) == '[')
                                            {
                                                for (currentChar += 9; *currentChar != '>'; ++currentChar) ;
                                                while (currentChar != endChar && *(int*)(currentChar - 2) != ((']' << 16) + ']'))
                                                {
                                                    if ((currentChar += 3) < endChar)
                                                    {
                                                        while (*currentChar != '>') ++currentChar;
                                                    }
                                                    else currentChar = endChar;
                                                }
                                            }
                                            else
                                            {
                                                while (*currentChar != '>') ++currentChar;
                                            }
                                            if (currentChar != endChar) ++currentChar;
                                        }
                                        htmlValue = html.Substring((int)(startChar - htmlFixed), (int)(currentChar - startChar) + (*(currentChar - 1) == '>' ? 0 : 1));
                                        children.Add(new TmphHtmlNode { TagName = "!", nodeText = new TmphHtmlText { FormatHtml = htmlValue, FormatText = htmlValue } });
                                        startChar = currentChar;

                                        #endregion 注释
                                    }
                                }
                            }
                            else ++currentChar;
                        }
                    }
                    if (startChar != endChar)
                    {
                        *endChar = '>';
                        while ((*startChar & 0xffc0) == 0 && spaceFixedMap.Get(*startChar)) ++startChar;
                        if (startChar != endChar)
                        {
                            for (scriptChar = endChar - 1; (*scriptChar & 0xffc0) == 0 && spaceFixedMap.Get(*scriptChar); --scriptChar) ;
                            children.Add(new TmphHtmlNode { nodeText = new TmphHtmlText { FormatHtml = html.Substring((int)(startChar - htmlFixed), (int)(scriptChar - startChar) + 1) } });
                        }
                    }
                }
                for (nextIndex = children.Count - 1; nextIndex != -1; nextIndex--)
                {
                    nextNode = children[nextIndex];
                    if (nextNode.nodeText.FormatHtml == null)
                    {
                        if (Web.TmphHtml.MustRoundTagNames.Contains(nextNode.TagName)
                            && (nodeCount = (children.Count - nextIndex - 1)) != 0)
                        {
                            nextNode.children = new TmphList<TmphHtmlNode>(children.GetSub(nextIndex + 1, nodeCount), true);
                            children.RemoveRange(nextIndex + 1, nodeCount);
                            foreach (TmphHtmlNode value in children) value.Parent = nextNode;
                        }
                    }
                    else if (nextNode.nodeText.FormatHtml.Length == 0) nextNode.nodeText.FormatHtml = null;
                }
                foreach (TmphHtmlNode value in children) value.Parent = this;
            }
        }

        /// <summary>
        /// 节点索引
        /// </summary>
        private struct TmphNodeIndex
        {
            /// <summary>
            /// 节点集合
            /// </summary>
            public TmphList<TmphHtmlNode> Values;

            /// <summary>
            /// 当前访问位置
            /// </summary>
            public int Index;
        }

        /// <summary>
        /// 子孙节点枚举
        /// </summary>
        public IEnumerable<TmphHtmlNode> Nodes
        {
            get
            {
                if (children.Count() != 0)
                {
                    TmphHtmlNode node;
                    TmphList<TmphNodeIndex> values = new TmphList<TmphNodeIndex>();
                    TmphNodeIndex index = new TmphNodeIndex { Values = children };
                    while (true)
                    {
                        if (index.Values == null)
                        {
                            if (values.Count == 0) break;
                            else index = values.Pop();
                        }
                        yield return node = index.Values[index.Index];
                        if (node.children.Count() == 0)
                        {
                            if (++index.Index == index.Values.Count) index.Values = null;
                        }
                        else
                        {
                            if (++index.Index != index.Values.Count) values.Add(index);
                            index.Values = node.children;
                            index.Index = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 节点筛选器
        /// </summary>
        private sealed class TmphFilter
        {
            /// <summary>
            /// 功能字符集合
            /// </summary>
            public const string Filters = @"\/.#*:@";

            /// <summary>
            /// 功能字符位图
            /// </summary>
            private static readonly TmphPointer filterMap = new TmphString.TmphAsciiMap(TmphUnmanaged.Get(TmphString.TmphAsciiMap.MapBytes, true), Filters, true).Pointer;

            /// <summary>
            /// 节点筛选器解析缓存
            /// </summary>
            private static readonly Dictionary<TmphHashString, TmphFilter> cache = TmphDictionary.CreateHashString<TmphFilter>();

            /// <summary>
            /// 当前筛选节点功能调用
            /// </summary>
            private Func<TmphFilter, TmphKeyValue<TmphList<TmphHtmlNode>, bool>, TmphKeyValue<TmphList<TmphHtmlNode>, bool>> filterMethod;

            /// <summary>
            /// 下级筛选器
            /// </summary>
            private TmphFilter nextFilter;

            /// <summary>
            /// 当前筛选节点匹配名称
            /// </summary>
            private string name;

            /// <summary>
            /// 当前筛选节点匹配值
            /// </summary>
            private string value;

            /// <summary>
            /// 当前筛选节点匹配多值集合
            /// </summary>
            private TmphStaticHashSet<string> values;//showjim应该修改为状态机

            /// <summary>
            /// 当前筛选节点匹配位置
            /// </summary>
            private int index = -1;

            /// <summary>
            /// 当前筛选节点匹配多位置集合
            /// </summary>
            private TmphStaticHashSet<int> indexs;

            /// <summary>
            /// 功能字符位图
            /// </summary>
            private byte* filterFixedMap;

            /// <summary>
            /// 节点筛选器解析
            /// </summary>
            /// <param name="start">起始字符位置</param>
            /// <param name="end">结束字符位置</param>
            private TmphFilter(char* start, char* end)
            {
                filterFixedMap = filterMap.Byte;
                switch (*start)
                {
                    case '/':
                        filterMethod = filterChild;
                        if (++start != end && start != (end = next(start, end)))
                        {
                            char* index = end;
                            if (*--index == ']' && (index = Unsafe.TmphString.Find(start, index, '[')) != null)
                            {
                                Unsafe.TmphString.ToLower(start, index);
                                getValue(start, index);
                                getIndex(++index, --end);
                            }
                            else
                            {
                                Unsafe.TmphString.ToLower(start, end);
                                getValues(start, end);
                            }
                        }
                        break;

                    case '.':
                        filterMethod = filterClass;
                        if (++start != end) getValues(start, end = next(start, end));
                        break;

                    case '#':
                        name = "id";
                        filterMethod = filterValue;
                        if (++start != end) getValues(start, end = next(start, end));
                        break;

                    case '*':
                        name = "name";
                        filterMethod = filterValue;
                        if (++start != end)
                        {
                            end = next(start, end);
                            Unsafe.TmphString.ToLower(start, end);
                            getValues(start, end);
                        }
                        break;

                    case ':':
                        name = "type";
                        filterMethod = filterValue;
                        if (++start != end) getValues(start, end = next(start, end));
                        break;

                    case '@':
                        filterMethod = filterValue;
                        if (++start != end)
                        {
                            end = next(start, end);
                            char* value = Unsafe.TmphString.Find(start, end, '=');
                            if (value != null)
                            {
                                getName(start, value);
                                if (++value == end) this.value = string.Empty;
                                else getValues(value, end);
                            }
                            else getName(start, end);
                        }
                        break;

                    default:
                        filterMethod = filterNode;
                        if (*start == '\\') ++start;
                        if (start != end)
                        {
                            end = next(start, end);
                            Unsafe.TmphString.ToLower(start, end);
                            getValues(start, end);
                        }
                        break;
                }
            }

            /// <summary>
            /// 解析下一个筛选功能
            /// </summary>
            /// <param name="start">起始字符位置</param>
            /// <param name="end">结束字符位置</param>
            /// <returns>字符位置</returns>
            private char* next(char* start, char* end)
            {
                start = Unsafe.TmphString.FindAscii(start, end, filterFixedMap, Filters[0]);
                if (start != null)
                {
                    nextFilter = new TmphFilter(start, end);
                    return start;
                }
                return end;
            }

            /// <summary>
            /// 解析多值集合
            /// </summary>
            /// <param name="start">起始字符位置</param>
            /// <param name="end">结束字符位置</param>
            private void getValues(char* start, char* end)
            {
                if (start != end)
                {
                    value = new string(start, 0, (int)(end - start));
                    if (Unsafe.TmphString.Find(start, end, ',') != null)
                    {
                        values = new TmphStaticHashSet<string>(value.Split(','));
                        value = null;
                    }
                }
            }

            /// <summary>
            /// 解析值
            /// </summary>
            /// <param name="start">起始字符位置</param>
            /// <param name="end">结束字符位置</param>
            private void getValue(char* start, char* end)
            {
                if (start != end) value = new string(start, 0, (int)(end - start));
            }

            /// <summary>
            /// 解析名称
            /// </summary>
            /// <param name="start">起始字符位置</param>
            /// <param name="end">结束字符位置</param>
            private void getName(char* start, char* end)
            {
                if (start != end)
                {
                    Unsafe.TmphString.ToLower(start, end);
                    name = TmphHtmlNode.checkName(new string(start, 0, (int)(end - start)));
                }
            }

            /// <summary>
            /// 解析索引位置
            /// </summary>
            /// <param name="start">起始字符位置</param>
            /// <param name="end">结束字符位置</param>
            private void getIndex(char* start, char* end)
            {
                if (start != end)
                {
                    value = new string(start, 0, (int)(end - start));
                    if (Unsafe.TmphString.Find(start, end, ',') == null)
                    {
                        if (!int.TryParse(value, out index)) index = -1;
                    }
                    else
                    {
                        TmphSubArray<int> indexs = value.splitIntNoCheck(',');
                        if (indexs.Count != 0)
                        {
                            if (indexs.Count != 1) this.indexs = new TmphStaticHashSet<int>(indexs.ToArray());
                            else index = indexs.Array[0];
                        }
                    }
                    value = null;
                }
            }

            /// <summary>
            /// 获取匹配HTML节点集合
            /// </summary>
            /// <param name="value">筛选节点集合</param>
            /// <returns>匹配的HTML节点集合</returns>
            private TmphList<TmphHtmlNode> get(TmphKeyValue<TmphList<TmphHtmlNode>, bool> value)
            {
                value = filterMethod(this, value);
                return nextFilter == null || value.Key == null ? value.Key : nextFilter.get(value);
            }

            /// <summary>
            /// 根据筛选路径解析筛选器
            /// </summary>
            /// <param name="path">筛选路径</param>
            /// <returns>筛选器</returns>
            private static TmphFilter get(string path)
            {
                TmphFilter value;
                TmphHashString pathKey = path;
                if (!cache.TryGetValue(pathKey, out value))
                {
                    fixed (char* pathFixed = path)
                    {
                        cache[pathKey] = value = new TmphFilter(pathFixed, pathFixed + path.Length);
                    }
                }
                return value;
            }

            /// <summary>
            /// 根据筛选路径值匹配HTML节点集合
            /// </summary>
            /// <param name="path">筛选路径</param>
            /// <param name="node">筛选节点</param>
            /// <returns>匹配的HTML节点集合</returns>
            public static TmphList<TmphHtmlNode> Get(string path, TmphHtmlNode node)
            {
                if (path != null && path.Length != 0)
                {
                    TmphList<TmphHtmlNode> nodes = new TmphList<TmphHtmlNode>();
                    nodes.Add(node);
                    return get(path).get(new TmphKeyValue<TmphList<TmphHtmlNode>, bool>(nodes, false));
                }
                return null;
            }

            /// <summary>
            /// 根据筛选路径值匹配HTML节点集合
            /// </summary>
            /// <param name="path">筛选路径</param>
            /// <param name="nodes">筛选节点集合</param>
            /// <returns>匹配的HTML节点集合</returns>
            public static TmphList<TmphHtmlNode> Get(string path, TmphList<TmphHtmlNode> nodes)
            {
                if (path != null && path.Length != 0)
                {
                    return get(path).get(new TmphKeyValue<TmphList<TmphHtmlNode>, bool>(nodes, true)) ?? new TmphList<TmphHtmlNode>();
                }
                return null;
            }

            /// <summary>
            /// 子孙节点筛选
            /// </summary>
            /// <param name="path">筛选器</param>
            /// <param name="value">筛选节点集合</param>
            /// <returns>匹配的HTML节点集合</returns>
            private static TmphKeyValue<TmphList<TmphHtmlNode>, bool> filterNode(TmphFilter path, TmphKeyValue<TmphList<TmphHtmlNode>, bool> value)
            {
                TmphList<TmphNodeIndex> values = new TmphList<TmphNodeIndex>();
                TmphNodeIndex index = new TmphNodeIndex { Values = value.Key.GetList() };
                if (index.Values.Count != 0)
                {
                    if (value.Value)
                    {
                        HashSet<TmphHtmlNode> newValues = TmphHashSet.CreateOnly<TmphHtmlNode>(), historyNodes = TmphHashSet.CreateOnly<TmphHtmlNode>();
                        if (path.values == null)
                        {
                            if (path.value != null)
                            {
                                string tagName = path.value;
                                while (true)
                                {
                                    if (index.Values == null)
                                    {
                                        if (values.Count == 0) break;
                                        else index = values.Pop();
                                    }
                                    TmphHtmlNode node = index.Values[index.Index];
                                    if (node.TagName == tagName) newValues.Add(node);
                                    if (node.children.Count() == 0 || historyNodes.Contains(node))
                                    {
                                        if (++index.Index == index.Values.Count) index.Values = null;
                                    }
                                    else
                                    {
                                        if (++index.Index != index.Values.Count) values.Add(index);
                                        historyNodes.Add(node);
                                        index.Values = node.children;
                                        index.Index = 0;
                                    }
                                }
                            }
                            else
                            {
                                while (true)
                                {
                                    if (index.Values == null)
                                    {
                                        if (values.Count == 0) break;
                                        else index = values.Pop();
                                    }
                                    TmphHtmlNode node = index.Values[index.Index];
                                    newValues.Add(node);
                                    if (node.children.Count() == 0 || historyNodes.Contains(node))
                                    {
                                        if (++index.Index == index.Values.Count) index.Values = null;
                                    }
                                    else
                                    {
                                        if (++index.Index != index.Values.Count) values.Add(index);
                                        historyNodes.Add(node);
                                        index.Values = node.children;
                                        index.Index = 0;
                                    }
                                }
                            }
                        }
                        else
                        {
                            TmphStaticHashSet<string> tagNames = path.values;
                            while (true)
                            {
                                if (index.Values == null)
                                {
                                    if (values.Count == 0) break;
                                    else index = values.Pop();
                                }
                                TmphHtmlNode node = index.Values[index.Index];
                                if (tagNames.Contains(node.TagName)) newValues.Add(node);
                                if (node.children.Count() == 0 || historyNodes.Contains(node))
                                {
                                    if (++index.Index == index.Values.Count) index.Values = null;
                                }
                                else
                                {
                                    if (++index.Index != index.Values.Count) values.Add(index);
                                    historyNodes.Add(node);
                                    index.Values = node.children;
                                    index.Index = 0;
                                }
                            }
                        }
                        if (newValues.Count != 0)
                        {
                            return new TmphKeyValue<TmphList<TmphHtmlNode>, bool>(new TmphList<TmphHtmlNode>(newValues), newValues.Count > 1);
                        }
                    }
                    else
                    {
                        TmphList<TmphHtmlNode> newValues = new TmphList<TmphHtmlNode>();
                        if (path.values == null)
                        {
                            if (path.value != null)
                            {
                                string tagName = path.value;
                                while (true)
                                {
                                    if (index.Values == null)
                                    {
                                        if (values.Count == 0) break;
                                        else index = values.Pop();
                                    }
                                    TmphHtmlNode node = index.Values[index.Index];
                                    if (node.TagName == tagName) newValues.Add(node);
                                    if (node.children.Count() == 0)
                                    {
                                        if (++index.Index == index.Values.Count) index.Values = null;
                                    }
                                    else
                                    {
                                        if (++index.Index != index.Values.Count) values.Add(index);
                                        index.Values = node.children;
                                        index.Index = 0;
                                    }
                                }
                            }
                            else
                            {
                                while (true)
                                {
                                    if (index.Values == null)
                                    {
                                        if (values.Count == 0) break;
                                        else index = values.Pop();
                                    }
                                    TmphHtmlNode node = index.Values[index.Index];
                                    newValues.Add(node);
                                    if (node.children.Count() == 0)
                                    {
                                        if (++index.Index == index.Values.Count) index.Values = null;
                                    }
                                    else
                                    {
                                        if (++index.Index != index.Values.Count) values.Add(index);
                                        index.Values = node.children;
                                        index.Index = 0;
                                    }
                                }
                            }
                        }
                        else
                        {
                            TmphStaticHashSet<string> tagNames = path.values;
                            while (true)
                            {
                                if (index.Values == null)
                                {
                                    if (values.Count == 0) break;
                                    else index = values.Pop();
                                }
                                TmphHtmlNode node = index.Values[index.Index];
                                if (tagNames.Contains(node.TagName)) newValues.Add(node);
                                if (node.children.Count() == 0)
                                {
                                    if (++index.Index == index.Values.Count) index.Values = null;
                                }
                                else
                                {
                                    if (++index.Index != index.Values.Count) values.Add(index);
                                    index.Values = node.children;
                                    index.Index = 0;
                                }
                            }
                        }
                        if (newValues.Count != 0)
                        {
                            return new TmphKeyValue<TmphList<TmphHtmlNode>, bool>(newValues, newValues.Count > 1);
                        }
                    }
                }
                return new TmphKeyValue<TmphList<TmphHtmlNode>, bool>(null, false);
            }

            /// <summary>
            /// class样式筛选
            /// </summary>
            /// <param name="path">筛选器</param>
            /// <param name="value">筛选节点集合</param>
            /// <returns>匹配的HTML节点集合</returns>
            private static TmphKeyValue<TmphList<TmphHtmlNode>, bool> filterClass(TmphFilter path, TmphKeyValue<TmphList<TmphHtmlNode>, bool> value)
            {
                TmphList<TmphHtmlNode>.TmphUnsafer newValues = new TmphList<TmphHtmlNode>(value.Key.Count).Unsafer;
                if (path.values == null)
                {
                    string name = path.value;
                    foreach (TmphHtmlNode node in value.Key)
                    {
                        string className = node["class"];
                        if (className != null && className.Split(' ').indexOf(name) != -1) newValues.Add(node);
                    }
                }
                else
                {
                    TmphStaticHashSet<string> names = path.values;
                    foreach (TmphHtmlNode node in value.Key)
                    {
                        string className = node["class"];
                        if (className != null)
                        {
                            foreach (string name in className.Split(' '))
                            {
                                if (names.Contains(name))
                                {
                                    newValues.Add(node);
                                    break;
                                }
                            }
                        }
                    }
                }
                return new TmphKeyValue<TmphList<TmphHtmlNode>, bool>(newValues.List.Count != 0 ? newValues.List : null, value.Value && newValues.List.Count > 1);
            }

            /// <summary>
            /// 子节点筛选
            /// </summary>
            /// <param name="path">筛选器</param>
            /// <param name="value">筛选节点集合</param>
            /// <returns>匹配的HTML节点集合</returns>
            private static TmphKeyValue<TmphList<TmphHtmlNode>, bool> filterChild(TmphFilter path, TmphKeyValue<TmphList<TmphHtmlNode>, bool> value)
            {
                if (path.index < 0)
                {
                    if (path.indexs == null)
                    {
                        if (path.values == null)
                        {
                            if (path.value != null)
                            {
                                string tagName = path.value;
                                TmphList<TmphHtmlNode> newValues = new TmphList<TmphHtmlNode>(value.Key.Count);
                                foreach (TmphHtmlNode nodes in value.Key)
                                {
                                    if (nodes.children.Count() > 0)
                                    {
                                        foreach (TmphHtmlNode node in nodes.children)
                                        {
                                            if (node.TagName == tagName) newValues.Add(node);
                                        }
                                    }
                                }
                                return new TmphKeyValue<TmphList<TmphHtmlNode>, bool>(newValues.Count != 0 ? newValues : null, value.Value && newValues.Count > 1);
                            }
                            else
                            {
                                int index = 0;
                                foreach (TmphHtmlNode nodes in value.Key) if (nodes.children != null) index += nodes.children.Count;
                                if (index != 0)
                                {
                                    TmphHtmlNode[] newValues = new TmphHtmlNode[index];
                                    index = 0;
                                    foreach (TmphHtmlNode nodes in value.Key)
                                    {
                                        if (nodes.children != null)
                                        {
                                            nodes.children.CopyTo(newValues, index);
                                            index += nodes.children.Count;
                                        }
                                    }
                                    return new TmphKeyValue<TmphList<TmphHtmlNode>, bool>(new TmphList<TmphHtmlNode>(newValues, true), value.Value && newValues.Length != 1);
                                }
                                return new TmphKeyValue<TmphList<TmphHtmlNode>, bool>(null, false);
                            }
                        }
                        else
                        {
                            TmphStaticHashSet<string> tagNames = path.values;
                            TmphList<TmphHtmlNode>.TmphUnsafer newValues = new TmphList<TmphHtmlNode>(value.Key.Count).Unsafer;
                            foreach (TmphHtmlNode nodes in value.Key)
                            {
                                if (nodes.children.Count() != 0)
                                {
                                    foreach (TmphHtmlNode node in nodes.children)
                                    {
                                        if (tagNames.Contains(node.TagName)) newValues.Add(node);
                                    }
                                }
                            }
                            return new TmphKeyValue<TmphList<TmphHtmlNode>, bool>(newValues.List.Count != 0 ? newValues.List : null, value.Value && newValues.List.Count > 1);
                        }
                    }
                    else
                    {
                        TmphList<TmphHtmlNode>.TmphUnsafer newValues = new TmphList<TmphHtmlNode>(value.Key.Count).Unsafer;
                        if (path.value != null)
                        {
                            string tagName = path.value;
                            TmphStaticHashSet<int> indexs = path.indexs;
                            foreach (TmphHtmlNode nodes in value.Key)
                            {
                                if (nodes.children.Count() != 0)
                                {
                                    int index = 0;
                                    foreach (TmphHtmlNode node in nodes.children)
                                    {
                                        if (node.TagName == tagName)
                                        {
                                            if (indexs.Contains(index)) newValues.Add(node);
                                            ++index;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            int[] indexs = path.indexs.GetList().ToArray();
                            foreach (TmphHtmlNode nodes in value.Key)
                            {
                                int count = nodes.children.Count();
                                if (count > 0)
                                {
                                    TmphList<TmphHtmlNode> children = nodes.children;
                                    for (int index = indexs.Length; --index >= 0;)
                                    {
                                        if (index < count) newValues.Add(children[index]);
                                    }
                                }
                            }
                        }
                        return new TmphKeyValue<TmphList<TmphHtmlNode>, bool>(newValues.List.Count != 0 ? newValues.List : null, value.Value && newValues.List.Count > 1);
                    }
                }
                else
                {
                    TmphList<TmphHtmlNode>.TmphUnsafer newValues = new TmphList<TmphHtmlNode>(value.Key.Count).Unsafer;
                    if (path.value != null)
                    {
                        string tagName = path.value;
                        int index = path.index;
                        foreach (TmphHtmlNode nodes in value.Key)
                        {
                            if (nodes.children.Count() != 0)
                            {
                                int count = 0;
                                foreach (TmphHtmlNode node in nodes.children)
                                {
                                    if (node.TagName == tagName)
                                    {
                                        if (count == index)
                                        {
                                            newValues.Add(node);
                                            break;
                                        }
                                        ++count;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        int index = path.index;
                        foreach (TmphHtmlNode nodes in value.Key)
                        {
                            if (index < nodes.children.Count()) newValues.List.Add(nodes.children[index]);
                        }
                    }
                    return new TmphKeyValue<TmphList<TmphHtmlNode>, bool>(newValues.List.Count != 0 ? newValues.List : null, value.Value && newValues.List.Count > 1);
                }
            }

            /// <summary>
            /// 属性值筛选
            /// </summary>
            /// <param name="path">筛选器</param>
            /// <param name="value">筛选节点集合</param>
            /// <returns>匹配的HTML节点集合</returns>
            private static TmphKeyValue<TmphList<TmphHtmlNode>, bool> filterValue(TmphFilter path, TmphKeyValue<TmphList<TmphHtmlNode>, bool> value)
            {
                string name = path.name;
                TmphList<TmphHtmlNode>.TmphUnsafer newValues = new TmphList<TmphHtmlNode>(value.Key.Count).Unsafer;
                if (path.values == null && path.value == null)
                {
                    foreach (TmphHtmlNode node in value.Key)
                    {
                        if (node.attributes != null && node.attributes.ContainsKey(name)) newValues.Add(node);
                    }
                }
                else
                {
                    if (path.values == null)
                    {
                        string nameValue = path.value;
                        foreach (TmphHtmlNode node in value.Key)
                        {
                            if (node[name] == nameValue) newValues.Add(node);
                        }
                    }
                    else
                    {
                        TmphStaticHashSet<string> values = path.values;
                        foreach (TmphHtmlNode node in value.Key)
                        {
                            if (values.Contains(node[name])) newValues.Add(node);
                        }
                    }
                }
                return new TmphKeyValue<TmphList<TmphHtmlNode>, bool>(newValues.List.Count != 0 ? newValues.List : null, value.Value && newValues.List.Count > 1);
            }

            static TmphFilter()
            {
                if (Laurent.Lee.CLB.Config.TmphAppSetting.IsCheckMemory) TmphCheckMemory.Add(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            }
        }

        /// <summary>
        /// 根据筛选路径值匹配HTML节点集合
        /// </summary>
        /// <param name="value">筛选路径</param>
        /// <param name="nodes">筛选节点集合</param>
        /// <returns>匹配HTML节点集合</returns>
        public static TmphList<TmphHtmlNode> Path(string value, TmphList<TmphHtmlNode> nodes)
        {
            return TmphFilter.Get(value, nodes);
        }

        /// <summary>
        /// 根据筛选路径值匹配HTML节点集合
        /// </summary>
        /// <param name="value">筛选路径</param>
        /// <returns>匹配HTML节点集合</returns>
        public TmphList<TmphHtmlNode> Path(string value)
        {
            return TmphFilter.Get(value, this);
        }

        /// <summary>
        /// 清除所有属性
        /// </summary>
        public void ClearAttributes()
        {
            if (attributes != null) attributes.Clear();
        }

        /// <summary>
        /// 删除匹配的子孙节点
        /// </summary>
        /// <param name="isRemove">删除节点匹配器</param>
        public void Remove(Func<TmphHtmlNode, bool> isRemove)
        {
            if (isRemove != null)
            {
                removeChilds(isRemove);
                if (children.Count() != 0)
                {
                    TmphHtmlNode node;
                    TmphList<TmphNodeIndex> values = new TmphList<TmphNodeIndex>();
                    TmphNodeIndex index = new TmphNodeIndex { Values = children };
                    while (true)
                    {
                        if (index.Values == null)
                        {
                            if (values.Count == 0) break;
                            else index = values.Pop();
                        }
                        node = index.Values[index.Index];
                        if (node.children != null) node.removeChilds(isRemove);
                        if (node.children.Count() == 0)
                        {
                            if (++index.Index == index.Values.Count) index.Values = null;
                        }
                        else
                        {
                            if (++index.Index != index.Values.Count) values.Add(index);
                            index.Values = node.children;
                            index.Index = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 根据节点名称获取第一个子节点
        /// </summary>
        /// <param name="tagName">节点名称</param>
        /// <returns>第一个匹配子节点</returns>
        public TmphHtmlNode GetFirstChildByTagName(string tagName)
        {
            if (children != null)
            {
                string lowerTagName = tagName.ToLower();
                foreach (TmphHtmlNode value in children)
                {
                    if (value.TagName == lowerTagName) return value;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据节点名称获取子节点集合
        /// </summary>
        /// <param name="tagName">节点名称</param>
        /// <returns>子节点集合</returns>
        public TmphList<TmphHtmlNode> GetChildsByTagName(string tagName)
        {
            return tagName != null && tagName.Length != 0 ? getChildsByTagName(tagName.ToLower()) : new TmphList<TmphHtmlNode>();
        }

        /// <summary>
        /// 根据节点名称获取子节点集合
        /// </summary>
        /// <param name="tagName">节点名称</param>
        /// <returns>子节点集合</returns>
        public TmphList<TmphHtmlNode> getChildsByTagName(string tagName)
        {
            TmphList<TmphHtmlNode>.TmphUnsafer values = new TmphList<TmphHtmlNode>(children.Count()).Unsafer;
            if (children != null)
            {
                foreach (TmphHtmlNode value in children)
                {
                    if (value.TagName == tagName) values.Add(value);
                }
            }
            return values.List;
        }

        /// <summary>
        /// 根据节点名称删除子节点
        /// </summary>
        /// <param name="tagName">节点名称</param>
        public void RemoveChildsByTagName(string tagName)
        {
            if (tagName != null && tagName.Length != 0) removeChildsByTagName(tagName.ToLower());
        }

        /// <summary>
        /// 根据节点名称删除子节点
        /// </summary>
        /// <param name="tagName">节点名称</param>
        public void removeChildsByTagName(string tagName)
        {
            if (children != null)
            {
                int count = children.Count;
                while (--count >= 0 && children[count].TagName != tagName) ;
                if (count >= 0)
                {
                    TmphList<TmphHtmlNode>.TmphUnsafer values = new TmphList<TmphHtmlNode>(children.Count).Unsafer;
                    int index = 0;
                    for (; index != count; ++index)
                    {
                        if (children[index].TagName != tagName) values.Add(children[index]);
                    }
                    for (count = children.Count; ++index != count; values.Add(children[index])) ;
                    children = values.List;
                }
            }
        }

        /// <summary>
        /// 删除匹配的子节点
        /// </summary>
        /// <param name="isRemove">删除子节点匹配器</param>
        public void RemoveChilds(Func<TmphHtmlNode, bool> isRemove)
        {
            if (isRemove != null) removeChilds(isRemove);
        }

        /// <summary>
        /// 删除匹配的子节点
        /// </summary>
        /// <param name="isRemove">删除子节点匹配器</param>
        private void removeChilds(Func<TmphHtmlNode, bool> isRemove)
        {
            if (children != null)
            {
                int count = children.Count;
                while (--count >= 0 && !isRemove(children[count])) ;
                if (count >= 0)
                {
                    TmphList<TmphHtmlNode>.TmphUnsafer values = new TmphList<TmphHtmlNode>(children.Count).Unsafer;
                    int index = 0;
                    for (; index != count; ++index)
                    {
                        if (!isRemove(children[index])) values.Add(children[index]);
                    }
                    for (count = children.Count; ++index != count; values.Add(children[index])) ;
                    children = values.List;
                }
            }
        }

        /// <summary>
        /// 根据节点名称获取子孙节点集合
        /// </summary>
        /// <param name="tagName">节点名称</param>
        /// <returns>匹配的子孙节点集合</returns>
        public TmphList<TmphHtmlNode> GetNodesByTagName(string tagName)
        {
            return tagName != null && tagName.Length != 0 ? getNodesByTagName(tagName.ToLower()) : new TmphList<TmphHtmlNode>();
        }

        /// <summary>
        /// 根据节点名称获取子孙节点集合
        /// </summary>
        /// <param name="tagName">节点名称</param>
        /// <returns>匹配的子孙节点集合</returns>
        public TmphList<TmphHtmlNode> getNodesByTagName(string tagName)
        {
            TmphList<TmphHtmlNode> values = new TmphList<TmphHtmlNode>();
            foreach (TmphHtmlNode value in Nodes)
            {
                if (value.TagName == tagName) values.Add(value);
            }
            return values;
        }

        /// <summary>
        /// 判断是否存在匹配的子孙节点
        /// </summary>
        /// <param name="node">匹配节点</param>
        /// <returns>是否存在匹配的子孙节点</returns>
        public bool IsNode(TmphHtmlNode node)
        {
            while (node != null && node != this) node = node.Parent;
            return node != null;
        }

        /// <summary>
        /// 解析HTML节点并插入
        /// </summary>
        /// <param name="index">插入位置</param>
        /// <param name="html"></param>
        /// <returns>是否插入成功</returns>
        public bool InsertChild(int index, string html)
        {
            bool isInsert = false;
            if (TagName != null && !Web.TmphHtml.NonanalyticTagNames.Contains(TagName))
            {
                TmphHtmlNode value = new TmphHtmlNode(html);
                if (value.children != null)
                {
                    foreach (TmphHtmlNode child in value.children) child.Parent = this;
                    if (children == null) children = value.children;
                    else if (index >= children.Count) children.Add(value.children);
                    else children.Insert(index < 0 ? 0 : index, value.children);
                }
                isInsert = true;
            }
            return isInsert;
        }

        /// <summary>
        /// 删除子节点
        /// </summary>
        /// <param name="node">待删除的子节点</param>
        /// <returns>是否存在子节点</returns>
        public bool RemoveChild(TmphHtmlNode node)
        {
            int index = this[node];
            if (index != -1) children.RemoveAt(index);
            return index != -1;
        }

        /// <summary>
        /// 替换子节点
        /// </summary>
        /// <param name="oldNode">待替换的子节点</param>
        /// <param name="newNode">新的子节点</param>
        /// <returns>是否存在待替换的子节点</returns>
        public bool ReplaceChild(TmphHtmlNode oldNode, TmphHtmlNode newNode)
        {
            bool isReplace = false;
            if (oldNode != null && newNode != null)
            {
                int oldIndex = this[oldNode];
                if (oldIndex != -1)
                {
                    if (newNode.TagName == string.Empty)
                    {
                        oldNode.Parent = null;
                        if (newNode.children == null) children.RemoveAt(oldIndex);
                        else
                        {
                            foreach (TmphHtmlNode value in newNode.children) value.Parent = this;
                            if (newNode.children.Count == 1) children[oldIndex] = newNode.children[0];
                            else if (oldIndex == children.Count - 1)
                            {
                                children.RemoveAt(oldIndex);
                                children.Add(newNode.children);
                            }
                            else
                            {
                                TmphList<TmphHtmlNode>.TmphUnsafer values = new TmphList<TmphHtmlNode>(children.Count + newNode.children.Count).Unsafer;
                                int newIndex = 0;
                                while (newIndex != oldIndex) values.Add(children[newIndex++]);
                                values.List.Add(newNode.children);
                                for (oldIndex = children.Count; ++newIndex != oldIndex; values.Add(children[newIndex])) ;
                                children = values.List;
                            }
                        }
                        newNode.children = null;
                        isReplace = true;
                    }
                    else if (!newNode.IsNode(this))
                    {
                        int newIndex = this[newNode];
                        if (newIndex == -1)
                        {
                            if (newNode.Parent != null) newNode.Parent.RemoveChild(newNode);
                            newNode.Parent = this;
                            children[oldIndex] = newNode;
                            oldNode.Parent = null;
                        }
                        else
                        {
                            children[oldIndex] = newNode;
                            oldNode.Parent = null;
                            children.RemoveAt(newIndex);
                        }
                        isReplace = true;
                    }
                }
            }
            return isReplace;
        }

        /// <summary>
        /// 文本内容
        /// </summary>
        public unsafe string Text
        {
            get
            {
                if (TagName == null) return nodeText.Text;
                else if (!Web.TmphHtml.NoTextTagNames.Contains(TagName))
                {
                    if (children.Count() != 0)
                    {
                        TmphHtmlNode node;
                        TmphList<TmphNodeIndex> values = new TmphList<TmphNodeIndex>();
                        TmphNodeIndex index = new TmphNodeIndex { Values = children };
                        TmphPointer buffer = CLB.TmphUnmanagedPool.StreamBuffers.Get();
                        bool isSpace = false, isEnter = false;
                        try
                        {
                            using (TmphCharStream strings = new TmphCharStream(buffer.Char, CLB.TmphUnmanagedPool.StreamBuffers.Size >> 1))
                            {
                                while (true)
                                {
                                    if (index.Values == null)
                                    {
                                        if (values.Count == 0) break;
                                        else
                                        {
                                            index = values.Pop();
                                            string nodeTagName = index.Values[index.Index].TagName;
                                            isEnter = nodeTagName == "p" || nodeTagName == "div";
                                            if (++index.Index == index.Values.Count)
                                            {
                                                index.Values = null;
                                                continue;
                                            }
                                        }
                                    }
                                    node = index.Values[index.Index];
                                    if (node.TagName == "p" || node.TagName == "br")
                                    {
                                        isSpace = isEnter = false;
                                        strings.Write(@"
");
                                    }
                                    if (node.children.Count() == 0
                                        || node.TagName == null || Web.TmphHtml.NoTextTagNames.Contains(node.TagName))
                                    {
                                        if (node.TagName == null)
                                        {
                                            if (isEnter) strings.Write(@"
");
                                            else if (isSpace) strings.Write(' ');
                                            strings.Write(node.nodeText.Text);
                                            isSpace = true;
                                        }
                                        if (++index.Index == index.Values.Count) index.Values = null;
                                    }
                                    else
                                    {
                                        values.Add(index);
                                        index.Values = node.children;
                                        index.Index = 0;
                                    }
                                }
                                return strings.ToString();
                            }
                        }
                        finally { CLB.TmphUnmanagedPool.StreamBuffers.Push(ref buffer); }
                    }
                    else return string.Empty;
                }
                return null;
            }
        }

        /// <summary>
        /// 生成标签html
        /// </summary>
        /// <param name="strings">html流</param>
        private void tagHtml(TmphCharStream strings)
        {
            if (TagName.Length != 0)
            {
                strings.Write('<');
                strings.Write(TagName);
                if (attributes != null)
                {
                    foreach (KeyValuePair<TmphHashString, TmphHtmlText> attribute in attributes)
                    {
                        strings.Write(' ');
                        strings.Write(HttpUtility.HtmlEncode(attribute.Key.ToString()));
                        strings.Write(@"=""");
                        strings.Write(attribute.Value.Html);
                        strings.Write(@"""");
                    }
                }
                if (Web.TmphHtml.CanNonRoundTagNames.Contains(TagName) && children == null && nodeText.Html == null) strings.Write(" /");
                strings.Write('>');
            }
        }

        /// <summary>
        /// 生成标签结束
        /// </summary>
        /// <param name="strings">html流</param>
        private void tagRound(TmphCharStream strings)
        {
            if (TagName.Length != 0
                && (!Web.TmphHtml.CanNonRoundTagNames.Contains(TagName) || children != null || nodeText.Html != null))
            {
                strings.Write("</");
                strings.Write(TagName);
                strings.Write(">");
            }
        }

        /// <summary>
        /// HTML
        /// </summary>
        public string InnerHTML
        {
            get
            {
                return Html(false);
            }
        }

        /// <summary>
        /// 生成HTML
        /// </summary>
        /// <param name="isTag">是否包含当前标签</param>
        /// <returns>HTML</returns>
        public unsafe string Html(bool isTag)
        {
            if (TagName != null)
            {
                if (Web.TmphHtml.NonanalyticTagNames.Contains(TagName))
                {
                    if (isTag && TagName.Length != 1)
                    {
                        TmphPointer buffer = CLB.TmphUnmanagedPool.StreamBuffers.Get();
                        try
                        {
                            using (TmphCharStream strings = new TmphCharStream(buffer.Char, CLB.TmphUnmanagedPool.StreamBuffers.Size >> 1))
                            {
                                tagHtml(strings);
                                strings.Write(nodeText.Html);
                                tagRound(strings);
                                return strings.ToString();
                            }
                        }
                        finally { CLB.TmphUnmanagedPool.StreamBuffers.Push(ref buffer); }
                    }
                }
                else
                {
                    TmphPointer buffer = CLB.TmphUnmanagedPool.StreamBuffers.Get();
                    try
                    {
                        using (TmphCharStream strings = new TmphCharStream(buffer.Char, CLB.TmphUnmanagedPool.StreamBuffers.Size >> 1))
                        {
                            if (isTag) tagHtml(strings);
                            if (children.Count() != 0)
                            {
                                TmphHtmlNode node;
                                TmphList<TmphNodeIndex> values = new TmphList<TmphNodeIndex>();
                                TmphNodeIndex index = new TmphNodeIndex { Values = children };
                                while (true)
                                {
                                    if (index.Values == null)
                                    {
                                        if (values.Count == 0) break;
                                        {
                                            index = values.Pop();
                                            index.Values[index.Index].tagRound(strings);
                                            if (++index.Index == index.Values.Count)
                                            {
                                                index.Values = null;
                                                continue;
                                            }
                                        }
                                    }
                                    node = index.Values[index.Index];
                                    string nodeTagName = node.TagName;
                                    bool isNonanalyticTagNames = nodeTagName != null && Web.TmphHtml.NonanalyticTagNames.Contains(nodeTagName);
                                    if (node.children.Count() == 0 || nodeTagName == null || isNonanalyticTagNames)
                                    {
                                        if (nodeTagName != null && nodeTagName.Length != 1) node.tagHtml(strings);
                                        strings.Write(node.nodeText.Html);
                                        if (nodeTagName != null && nodeTagName.Length != 1) node.tagRound(strings);
                                        if (++index.Index == index.Values.Count) index.Values = null;
                                    }
                                    else
                                    {
                                        node.tagHtml(strings);
                                        values.Add(index);
                                        index.Values = node.children;
                                        index.Index = 0;
                                    }
                                }
                            }
                            if (isTag) tagRound(strings);
                            return strings.ToString();
                        }
                    }
                    finally { CLB.TmphUnmanagedPool.StreamBuffers.Push(ref buffer); }
                }
            }
            return nodeText.Html;
        }

        static TmphHtmlNode()
        {
            int dataIndex = 0;
            TmphPointer[] datas = TmphUnmanaged.Get(true
                , TmphString.TmphAsciiMap.MapBytes, TmphString.TmphAsciiMap.MapBytes, TmphString.TmphAsciiMap.MapBytes
                , TmphString.TmphAsciiMap.MapBytes, TmphString.TmphAsciiMap.MapBytes, TmphString.TmphAsciiMap.MapBytes);
            spaceMap = new TmphString.TmphAsciiMap(datas[dataIndex++], "\t\r\n ", true);
            spaceSplitMap = new TmphString.TmphAsciiMap(datas[dataIndex++], "\t\r\n >", true).Pointer;
            tagNameSplitMap = new TmphString.TmphAsciiMap(datas[dataIndex++], "\t\r\n \"'/=>", true).Pointer;
            attributeSplitMap = new TmphString.TmphAsciiMap(datas[dataIndex++], "\t\r\n \"'/=", true).Pointer;
            attributeNameSplitMap = new TmphString.TmphAsciiMap(datas[dataIndex++], "\"'/>", true).Pointer;
            TmphString.TmphAsciiMap tagNameAsciiMap = new TmphString.TmphAsciiMap(tagNameMap = datas[dataIndex++], "!/", true);
            tagNameAsciiMap.Unsafer.Set('a', 26);
            tagNameAsciiMap.Unsafer.Set('A', 26);
            noLowerAttributeNames = new TmphUniqueDictionary<TmphNoLowerAttributeName, string>(new TmphNoLowerAttributeName[] { "readOnly", "className" }, value => value.Name.ToLower(), 2);
        }
    }
}