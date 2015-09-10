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

using System;

namespace Laurent.Lee.CLB.Web
{
    /// <summary>
    ///     HTML代码参数及其相关操作
    /// </summary>
    public static class TmphHtml
    {
        /// <summary>
        ///     字符集类型
        /// </summary>
        public enum TmphCharsetType
        {
            /// <summary>
            ///     UTF-8
            /// </summary>
            [TmphCharsetInfo(CharsetString = "UTF-8")]
            Utf8,

            /// <summary>
            ///     GB2312
            /// </summary>
            [TmphCharsetInfo(CharsetString = "GB2312")]
            Gb2312
        }

        /// <summary>
        ///     标准引用类型
        /// </summary>
        public enum TmphDocType
        {
            /// <summary>
            ///     过渡(HTML4.01)
            /// </summary>
            [TmphDocInfo(
                Html =
                    @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">"
                )]
            Transitional = 0,

            /// <summary>
            ///     严格(不能使用任何表现层的标识和属性，例如<br>)
            /// </summary>
            [TmphDocInfo(
                Html =
                    @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">"
                )]
            Strict,

            /// <summary>
            ///     框架(专门针对框架页面设计使用的DTD，如果你的页面中包含有框架，需要采用这种DTD)
            /// </summary>
            [TmphDocInfo(
                Html =
                    @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Frameset//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-frameset.dtd"">"
                )]
            Frameset,

            /// <summary>
            /// </summary>
            Xhtml11,

            /// <summary>
            ///     HTML5
            /// </summary>
            [TmphDocInfo(Html = @"<!DOCTYPE html>")]
            Html5
        }

        /// <summary>
        ///     注释开始
        /// </summary>
        public const string NoteStart = @"
<![CDATA[
";

        /// <summary>
        ///     注释结束
        /// </summary>
        public const string NoteEnd = @"
]]>
";

        /// <summary>
        ///     javscript开始
        /// </summary>
        public const string JsStart = @"
<script language=""javascript"" type=""text/javascript"">
//<![CDATA[
";

        /// <summary>
        ///     javscript结束
        /// </summary>
        public const string JsEnd = @"
//]]>
</script>
";

        /// <summary>
        ///     style开始
        /// </summary>
        public const string StyleStart = @"
<style type=""text/css"">
<![CDATA[
";

        /// <summary>
        ///     style结束
        /// </summary>
        public const string StyletEnd = @"
]]>
</style>
";

        /// <summary>
        ///     标准文档集合
        /// </summary>
        private static readonly TmphDocInfo[] docTypes = TmphEnum.GetAttributes<TmphDocType, TmphDocInfo>();

        /// <summary>
        ///     字符集类型名称集合
        /// </summary>
        private static readonly TmphCharsetInfo[] CharsetTypes = TmphEnum.GetAttributes<TmphCharsetType, TmphCharsetInfo>();

        /// <summary>
        ///     允许不回合的标签名称集合
        /// </summary>
        public static readonly TmphUniqueHashSet<TmphCanNonRoundTagName> CanNonRoundTagNames =
            new TmphUniqueHashSet<TmphCanNonRoundTagName>(
                new TmphCanNonRoundTagName[]
                {"area", "areatext", "basefont", "br", "col", "colgroup", "hr", "img", "input", "li", "p", "spacer"}, 27);

        /// <summary>
        ///     必须回合的标签名称集合
        /// </summary>
        public static readonly TmphUniqueHashSet<TmphMustRoundTagName> MustRoundTagNames =
            new TmphUniqueHashSet<TmphMustRoundTagName>(
                new TmphMustRoundTagName[]
                {
                    "a", "b", "bgsound", "big", "body", "button", "caption", "center", "div", "em", "embed", "font", "form",
                    "h1", "h2", "h3", "h4", "h5", "h6", "hn", "html", "i", "iframe", "map", "marquee", "multicol",
                    "nobr", "ol", "option", "pre", "s", "select", "small", "span", "strike", "strong", "sub", "sup",
                    "table", "tbody", "td", "textarea", "tfoot", "th", "thead", "tr", "u", "ul"
                }, 239);

        /// <summary>
        ///     脚本安全属性名称集合
        /// </summary>
        public static readonly TmphUniqueHashSet<TmphSafeAttribute> SafeAttributes =
            new TmphUniqueHashSet<TmphSafeAttribute>(
                new TmphSafeAttribute[]
                {
                    "align", "allowtransparency", "alt", "behavior", "bgcolor", "border", "bordercolor", "bordercolordark",
                    "bordercolorlight", "cellpadding", "cellspacing", "checked", "class", "clear", "color", "cols",
                    "colspan", "controls", "coords", "direction", "face", "frame", "frameborder", "gutter", "height",
                    "hspace", "loop", "loopdelay", "marginheight", "marginwidth", "maxlength", "method", "multiple",
                    "rows", "rowspan", "rules", "scrollamount", "scrolldelay", "scrolling", "selected", "shape", "size",
                    "span", "start", "target", "title", "type", "unselectable", "usemap", "valign", "value", "vspace",
                    "width", "wrap"
                }, 253);

        /// <summary>
        ///     URI属性名称集合
        /// </summary>
        public static readonly TmphUniqueHashSet<TmphUriAttribute> UriAttributes =
            new TmphUniqueHashSet<TmphUriAttribute>(new TmphUriAttribute[] { "background", "dynsrc", "href", "src" }, 5);

        /// <summary>
        ///     安全样式名称集合
        /// </summary>
        public static readonly TmphUniqueHashSet<TmphSafeStyleAttribute> SafeStyleAttributes =
            new TmphUniqueHashSet<TmphSafeStyleAttribute>(
                new TmphSafeStyleAttribute[]
                {"font", "font-family", "font-size", "font-weight", "color", "text-decoration"}, 8);

        /// <summary>
        ///     非解析标签名称集合
        /// </summary>
        public static readonly TmphUniqueHashSet<TmphNonanalyticTagName> NonanalyticTagNames =
            new TmphUniqueHashSet<TmphNonanalyticTagName>(new TmphNonanalyticTagName[] { "script", "style", "!", "/" }, 6);

        /// <summary>
        ///     非文本标签名称集合
        /// </summary>
        public static readonly TmphUniqueHashSet<TmphNoTextTagName> NoTextTagNames =
            new TmphUniqueHashSet<TmphNoTextTagName>(
                new TmphNoTextTagName[]
                {"script", "style", "pre", "areatext", "!", "/", "input", "iframe", "img", "link", "head"}, 15);

        /// <summary>
        ///     默认HTML编码器
        /// </summary>
        internal static readonly TmphEncoder HtmlEncoder = new TmphEncoder(@"& <>""'");

        /// <summary>
        ///     TextArea编码器
        /// </summary>
        internal static readonly TmphEncoder TextAreaEncoder = new TmphEncoder(@"&<>");

        /// <summary>
        ///     默认HTML编码器
        /// </summary>
        public static TmphIEncoder HtmlIEncoder
        {
            get { return HtmlEncoder; }
        }

        /// <summary>
        ///     TextArea编码器
        /// </summary>
        public static TmphIEncoder TextAreaIEncoder
        {
            get { return TextAreaEncoder; }
        }

        /// <summary>
        ///     获取标准引用代码
        /// </summary>
        /// <returns>文档类型</returns>
        public static string GetHtml(this TmphDocType type)
        {
            var typeIndex = (int)type;
            if (typeIndex < 0 || typeIndex >= docTypes.Length) typeIndex = 0;
            return docTypes[typeIndex].Html + @"
<html xmlns=""http://www.w3.org/1999/xhtml"">
";
        }

        /// <summary>
        ///     获取字符集代码
        /// </summary>
        /// <returns>字符集代码</returns>
        public static string GetHtml(this TmphCharsetType type)
        {
            var typeIndex = (int)type;
            if (typeIndex >= CharsetTypes.Length) typeIndex = -1;
            var html = string.Empty;
            if (typeIndex >= 0)
                html = @"<meta http-equiv=""content-type"" content=""text/html; charset=" +
                       CharsetTypes[typeIndex].CharsetString + @""">
";
            return html;
        }

        /// <summary>
        ///     加载js文件
        /// </summary>
        /// <param name="fileName">被加载的js文件地址</param>
        /// <returns>加载js文件的HTML代码</returns>
        public static string JsFile(string fileName)
        {
            return @"<script language=""javascript"" type=""text/javascript"" src=""" + fileName + @"""></script>";
        }

        /// <summary>
        ///     加载css文件
        /// </summary>
        /// <param name="fileName">被加载的css文件地址</param>
        /// <returns>加载css文件的HTML代码</returns>
        public static string CssFile(string fileName)
        {
            return @"<style type=""text/css"" link=""" + fileName + @"""></style>";
        }

        /// <summary>
        ///     文档类型属性
        /// </summary>
        public sealed class TmphDocInfo : Attribute
        {
            /// <summary>
            ///     标准文档类型头部
            /// </summary>
            public string Html;
        }

        /// <summary>
        ///     字符集类型属性
        /// </summary>
        public sealed class TmphCharsetInfo : Attribute
        {
            /// <summary>
            ///     字符串表示
            /// </summary>
            public string CharsetString;
        }

        /// <summary>
        ///     允许不回合的标签名称唯一哈希
        /// </summary>
        public struct TmphCanNonRoundTagName : IEquatable<TmphCanNonRoundTagName>
        {
            /// <summary>
            ///     允许不回合的标签名称
            /// </summary>
            public string Name;

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public bool Equals(TmphCanNonRoundTagName other)
            {
                return Name == other.Name;
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="name">允许不回合的标签名称</param>
            /// <returns>允许不回合的标签名称唯一哈希</returns>
            public static implicit operator TmphCanNonRoundTagName(string name)
            {
                return new TmphCanNonRoundTagName { Name = name };
            }

            /// <summary>
            ///     获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override int GetHashCode()
            {
                if (Name.Length == 0) return 1;
                var code = (Name[Name.Length - 1] << 7) + Name[0];
                return ((code >> 5) ^ code) & ((1 << 5) - 1);
            }

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((TmphCanNonRoundTagName)obj);
            }
        }

        /// <summary>
        ///     必须回合的标签名称唯一哈希
        /// </summary>
        public struct TmphMustRoundTagName : IEquatable<TmphMustRoundTagName>
        {
            /// <summary>
            ///     必须回合的标签名称
            /// </summary>
            public string Name;

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public bool Equals(TmphMustRoundTagName other)
            {
                return Name == other.Name;
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="name">必须回合的标签名称</param>
            /// <returns>必须回合的标签名称唯一哈希</returns>
            public static implicit operator TmphMustRoundTagName(string name)
            {
                return new TmphMustRoundTagName { Name = name };
            }

            /// <summary>
            ///     获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override int GetHashCode()
            {
                if (Name.Length == 0) return 0;
                var code = (Name[Name.Length >> 1] << 14) + (Name[0] << 7) + Name[Name.Length - 1];
                return ((code >> 15) ^ (code >> 13) ^ (code >> 1)) & ((1 << 8) - 1);
            }

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((TmphMustRoundTagName)obj);
            }
        }

        /// <summary>
        ///     脚本安全属性名称
        /// </summary>
        public struct TmphSafeAttribute : IEquatable<TmphSafeAttribute>
        {
            /// <summary>
            ///     脚本安全属性名称
            /// </summary>
            public string Name;

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public bool Equals(TmphSafeAttribute other)
            {
                return Name == other.Name;
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="name">脚本安全属性名称</param>
            /// <returns>脚本安全属性名称唯一哈希</returns>
            public static implicit operator TmphSafeAttribute(string name)
            {
                return new TmphSafeAttribute { Name = name };
            }

            /// <summary>
            ///     获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override int GetHashCode()
            {
                if (Name.Length < 3) return 0;
                var code = (Name[Name.Length - 2] << 14) + (Name[Name.Length >> 1] << 7) + Name[Name.Length >> 3];
                return ((code >> 8) ^ (code >> 3) ^ (code >> 1)) & ((1 << 8) - 1);
            }

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((TmphSafeAttribute)obj);
            }
        }

        /// <summary>
        ///     URI属性名称唯一哈希
        /// </summary>
        public struct TmphUriAttribute : IEquatable<TmphUriAttribute>
        {
            /// <summary>
            ///     URI属性名称
            /// </summary>
            public string Name;

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public bool Equals(TmphUriAttribute other)
            {
                return Name == other.Name;
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="name">URI属性名称</param>
            /// <returns>URI属性名称唯一哈希</returns>
            public static implicit operator TmphUriAttribute(string name)
            {
                return new TmphUriAttribute { Name = name };
            }

            /// <summary>
            ///     获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override int GetHashCode()
            {
                return Name[0] & 7;
            }

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((TmphUriAttribute)obj);
            }
        }

        /// <summary>
        ///     安全样式名称唯一哈希
        /// </summary>
        public struct TmphSafeStyleAttribute : IEquatable<TmphSafeStyleAttribute>
        {
            /// <summary>
            ///     安全样式名称
            /// </summary>
            public string Name;

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public bool Equals(TmphSafeStyleAttribute other)
            {
                return Name == other.Name;
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="name">安全样式名称</param>
            /// <returns>安全样式名称唯一哈希</returns>
            public static implicit operator TmphSafeStyleAttribute(string name)
            {
                return new TmphSafeStyleAttribute { Name = name };
            }

            /// <summary>
            ///     获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override int GetHashCode()
            {
                return Name.Length < 4 ? 0 : Name[Name.Length - 4] & 7;
            }

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((TmphSafeStyleAttribute)obj);
            }
        }

        /// <summary>
        ///     非解析标签名称唯一哈希
        /// </summary>
        public struct TmphNonanalyticTagName : IEquatable<TmphNonanalyticTagName>
        {
            /// <summary>
            ///     非解析标签名称
            /// </summary>
            public string Name;

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public bool Equals(TmphNonanalyticTagName other)
            {
                return Name == other.Name;
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="name">非解析标签名称</param>
            /// <returns>非解析标签名称唯一哈希</returns>
            public static implicit operator TmphNonanalyticTagName(string name)
            {
                return new TmphNonanalyticTagName { Name = name };
            }

            /// <summary>
            ///     获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override int GetHashCode()
            {
                if (Name.Length == 0) return 2;
                return (Name[Name.Length - 1] >> 2) & ((1 << 3) - 1);
            }

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((TmphNonanalyticTagName)obj);
            }
        }

        /// <summary>
        ///     非文本标签名称唯一哈希
        /// </summary>
        public struct TmphNoTextTagName : IEquatable<TmphNoTextTagName>
        {
            /// <summary>
            ///     非文本标签名称
            /// </summary>
            public string Name;

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public bool Equals(TmphNoTextTagName other)
            {
                return Name == other.Name;
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="name">非文本标签名称</param>
            /// <returns>非文本标签名称唯一哈希</returns>
            public static implicit operator TmphNoTextTagName(string name)
            {
                return new TmphNoTextTagName { Name = name };
            }

            /// <summary>
            ///     获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override int GetHashCode()
            {
                if (Name.Length == 0) return 5;
                var code = (Name[0] << 7) + Name[Name.Length >> 2];
                return ((code >> 7) ^ (code >> 2)) & ((1 << 4) - 1);
            }

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((TmphNoTextTagName)obj);
            }
        }

        /// <summary>
        ///     HTML编码器
        /// </summary>
        public interface TmphIEncoder
        {
            /// <summary>
            ///     文本转HTML
            /// </summary>
            /// <param name="value">文本值</param>
            /// <returns>HTML编码</returns>
            string ToHtml(string value);

            /// <summary>
            ///     文本转HTML
            /// </summary>
            /// <param name="value">文本值</param>
            /// <returns>HTML编码</returns>
            TmphSubString ToHtml(TmphSubString value);

            /// <summary>
            ///     文本转HTML
            /// </summary>
            /// <param name="value">文本值</param>
            /// <param name="stream">HTML编码流</param>
            void ToHtml(TmphSubString value, TmphUnmanagedStream stream);
        }

        /// <summary>
        ///     HTML编码器
        /// </summary>
        internal unsafe struct TmphEncoder : TmphIEncoder
        {
            /// <summary>
            ///     HTML转义字符集合
            /// </summary>
            private readonly uint* htmls;

            /// <summary>
            ///     最大值
            /// </summary>
            private readonly int size;

            /// <summary>
            ///     HTML编码器
            /// </summary>
            /// <param name="htmls">HTML转义字符集合</param>
            public TmphEncoder(string htmls)
            {
                size = 0;
                foreach (var htmlChar in htmls)
                {
                    if (htmlChar > size) size = htmlChar;
                }
                this.htmls = TmphUnmanaged.Get(++size * sizeof(uint), true).UInt;
                foreach (var value in htmls)
                {
                    var div = (value * (int)TmphNumber.Div10_16Mul) >> TmphNumber.Div10_16Shift;
                    this.htmls[value] = (uint)(((value - div * 10) << 16) | div | 0x300030);
                }
            }

            /// <summary>
            ///     文本转HTML
            /// </summary>
            /// <param name="value">文本值</param>
            /// <returns>HTML编码</returns>
            public string ToHtml(string value)
            {
                if (value != null)
                {
                    var length = value.Length;
                    fixed (char* valueFixed = value)
                    {
                        var end = valueFixed + length;
                        var count = encodeCount(valueFixed, end);
                        if (count != 0)
                        {
                            value = TmphString.FastAllocateString(length += count << 2);
                            fixed (char* data = value) toHtml(valueFixed, end, data);
                        }
                    }
                }
                return value;
            }

            /// <summary>
            ///     文本转HTML
            /// </summary>
            /// <param name="value">文本值</param>
            /// <returns>HTML编码</returns>
            public TmphSubString ToHtml(TmphSubString value)
            {
                if (value.Length != 0)
                {
                    var length = value.Length;
                    fixed (char* valueFixed = value.value)
                    {
                        char* start = valueFixed + value.StartIndex, end = start + length;
                        var count = encodeCount(start, end);
                        if (count != 0)
                        {
                            var newValue = TmphString.FastAllocateString(length += count << 2);
                            fixed (char* data = newValue) toHtml(start, end, data);
                            return TmphSubString.Unsafe(newValue, 0, newValue.Length);
                        }
                    }
                }
                return value;
            }

            /// <summary>
            ///     文本转HTML
            /// </summary>
            /// <param name="value">文本值</param>
            /// <param name="stream">HTML编码流</param>
            public void ToHtml(TmphSubString value, TmphUnmanagedStream stream)
            {
                if (value.Length != 0)
                {
                    if (stream == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                    var length = value.Length;
                    fixed (char* valueFixed = value.value)
                    {
                        char* start = valueFixed + value.StartIndex, end = start + length;
                        var count = encodeCount(start, end);
                        if (count == 0)
                        {
                            stream.PrepLength(length <<= 1);
                            Unsafe.TmphMemory.Copy(start, stream.CurrentData, length);
                        }
                        else
                        {
                            length += count << 2;
                            stream.PrepLength(length <<= 1);
                            toHtml(start, end, (char*)stream.CurrentData);
                        }
                        stream.Unsafer.AddLength(length);
                    }
                }
            }

            /// <summary>
            ///     HTML转义
            /// </summary>
            /// <param name="start">起始位置</param>
            /// <param name="end">结束位置</param>
            /// <param name="write">写入位置</param>
            private void toHtml(char* start, char* end, char* write)
            {
                while (start != end)
                {
                    var code = *start++;
                    if (code < size)
                    {
                        var html = htmls[code];
                        if (html == 0) *write++ = code;
                        else
                        {
                            *(int*)write = '&' + ('#' << 16);
                            write += 2;
                            *(uint*)write = html;
                            write += 2;
                            *write++ = ';';
                        }
                    }
                    else *write++ = code;
                }
            }

            /// <summary>
            ///     计算编码字符数量
            /// </summary>
            /// <param name="start">起始位置</param>
            /// <param name="end">结束位置</param>
            /// <returns>编码字符数量</returns>
            private int encodeCount(char* start, char* end)
            {
                var count = 0;
                while (start != end)
                {
                    if (*start < size && htmls[*start] != 0) ++count;
                    ++start;
                }
                return count;
            }
        }
    }
}