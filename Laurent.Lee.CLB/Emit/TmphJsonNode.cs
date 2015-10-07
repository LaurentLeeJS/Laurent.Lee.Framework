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

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    ///     JSON节点
    /// </summary>
    public struct TmphJsonNode
    {
        /// <summary>
        ///     节点类型
        /// </summary>
        public enum TmphType : byte
        {
            /// <summary>
            ///     空值
            /// </summary>
            Null,

            /// <summary>
            ///     字符串
            /// </summary>
            String,

            /// <summary>
            ///     未解析字符串
            /// </summary>
            QuoteString,

            /// <summary>
            ///     数字字符串
            /// </summary>
            NumberString,

            /// <summary>
            ///     非数值
            /// </summary>
            NaN,

            /// <summary>
            ///     时间周期值
            /// </summary>
            DateTimeTick,

            /// <summary>
            ///     逻辑值
            /// </summary>
            Bool,

            /// <summary>
            ///     列表
            /// </summary>
            List,

            /// <summary>
            ///     字典
            /// </summary>
            Dictionary
        }

        /// <summary>
        ///     字典
        /// </summary>
        private TmphKeyValue<TmphJsonNode, TmphJsonNode>[] _dictionary;

        /// <summary>
        ///     列表
        /// </summary>
        private TmphJsonNode[] _list;

        /// <summary>
        ///     64位整数值
        /// </summary>
        internal long Int64;

        /// <summary>
        ///     字符串
        /// </summary>
        internal TmphSubString String;

        /// <summary>
        ///     字典
        /// </summary>
        internal TmphSubArray<TmphKeyValue<TmphJsonNode, TmphJsonNode>> Dictionary
        {
            get { return TmphSubArray<TmphKeyValue<TmphJsonNode, TmphJsonNode>>.Unsafe(_dictionary, 0, (int)Int64); }
        }

        /// <summary>
        ///     根据名称获取JSON节点
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TmphJsonNode this[string name]
        {
            get
            {
                for (int index = 0, count = (int)Int64; index != count; ++index)
                {
                    if (_dictionary[index].Key.KeyName.Equals(name)) return _dictionary[index].Value;
                }
                return default(TmphJsonNode);
            }
        }

        /// <summary>
        ///     字典名称
        /// </summary>
        private TmphSubString KeyName
        {
            get
            {
                if (Type == TmphType.QuoteString)
                {
                    String = TmphJsonParser.ParseQuoteString(String, (int)(Int64 >> 32), (char)Int64, (int)Int64 >> 16);
                    Type = TmphType.String;
                }
                return String;
            }
        }

        /// <summary>
        ///     列表
        /// </summary>
        public TmphSubArray<TmphJsonNode> List
        {
            get
            {
                return Type == TmphType.List
                    ? TmphSubArray<TmphJsonNode>.Unsafe(_list, 0, (int)Int64)
                    : default(TmphSubArray<TmphJsonNode>);
            }
        }

        /// <summary>
        ///     字典或列表数据量
        /// </summary>
        public int Count
        {
            get { return Type == TmphType.Dictionary || Type == TmphType.List ? (int)Int64 : 0; }
        }

        /// <summary>
        ///     类型
        /// </summary>
        public TmphType Type { get; internal set; }

        /// <summary>
        ///     设置列表
        /// </summary>
        /// <param name="list"></param>
        internal void SetList(TmphSubArray<TmphJsonNode> list)
        {
            _list = list.array;
            Int64 = list.Count;
            Type = TmphType.List;
        }

        /// <summary>
        ///     设置字典
        /// </summary>
        /// <param name="dictionary"></param>
        internal void SetDictionary(TmphSubArray<TmphKeyValue<TmphJsonNode, TmphJsonNode>> dictionary)
        {
            _dictionary = dictionary.array;
            Int64 = dictionary.Count;
            Type = TmphType.Dictionary;
        }

        /// <summary>
        ///     未解析字符串
        /// </summary>
        /// <param name="escapeIndex">未解析字符串起始位置</param>
        /// <param name="quote">字符串引号</param>
        /// <param name="isTempString"></param>
        internal void SetQuoteString(int escapeIndex, char quote, bool isTempString)
        {
            Type = TmphType.QuoteString;
            Int64 = ((long)escapeIndex << 32) + quote;
            if (isTempString) Int64 += 0x10000;
        }

        /// <summary>
        ///     设置数字字符串
        /// </summary>
        /// <param name="quote"></param>
        internal void SetNumberString(char quote)
        {
            Int64 = quote;
            Type = TmphType.NumberString;
        }

        /// <summary>
        ///     创建字典节点
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static TmphJsonNode CreateDictionary(TmphSubArray<TmphKeyValue<TmphSubString, TmphJsonNode>> dictionary)
        {
            var node = new TmphJsonNode { Type = TmphType.Dictionary };
            node.SetDictionary(
                dictionary.GetArray(
                    value =>
                        new TmphKeyValue<TmphJsonNode, TmphJsonNode>(new TmphJsonNode { Type = TmphType.String, String = value.Key },
                            value.Value)).toSubArray());
            return node;
        }
    }
}