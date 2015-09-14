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

using System;
using System.Collections.Generic;
using System.IO;

namespace Laurent.Lee.CLB.Search
{
    /// <summary>
    /// 默认中文分词词语
    /// </summary>
    [Laurent.Lee.CLB.Emit.TmphDataSerialize(IsReferenceMember = false, IsMemberMap = false)]
    public sealed class TmphWord
    {
        /// <summary>
        /// 词
        /// </summary>
        public string Name;

        /// <summary>
        /// 频率
        /// </summary>
        public int Frequency;

        /// <summary>
        /// 词性
        /// </summary>
        public sealed class TmphPartOfSpeech : Attribute
        {
            /// <summary>
            /// 词性简写分类
            /// </summary>
            private static Dictionary<TmphHashString, TmphCategory> shortNameCategorys;

            /// <summary>
            /// 根据词性简写获取词性分类
            /// </summary>
            /// <param name="name">词性简写</param>
            /// <returns>词性分类</returns>
            public static TmphCategory GetCategoryByShortName(string name)
            {
                if (shortNameCategorys == null)
                {
                    shortNameCategorys = (System.Enum.GetValues(typeof(TmphCategory)) as TmphCategory[])
                        .getDictionary(value => (TmphHashString)Laurent.Lee.CLB.TmphEnum<TmphCategory, TmphPartOfSpeech>.Dictionary(value).ShortName);
                }
                TmphCategory category;
                return shortNameCategorys.TryGetValue(name.ToLower(), out category) ? category : TmphCategory.None;
            }

            /// <summary>
            /// 名称
            /// </summary>
            public string Name;

            /// <summary>
            /// 简写
            /// </summary>
            public string ShortName;

            /// <summary>
            /// 父级分类
            /// </summary>
            public TmphCategory Parent;
        }

        /// <summary>
        /// 词性分类大小
        /// </summary>
        public const int CategoryCapacity = 0x10000;

        /// <summary>
        /// 词性分类
        /// </summary>
        public enum TmphCategory
        {
            [TmphPartOfSpeech(ShortName = "", Name = "未知", Parent = TmphCategory.None)]
            None = 0,

            #region 名词

            /// <summary>
            /// 名词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "n", Name = "名词", Parent = TmphCategory.Noun)]
            Noun = 1,

            /// <summary>
            /// 人名
            /// </summary>
            [TmphPartOfSpeech(ShortName = "nr", Name = "人名", Parent = TmphCategory.Noun)]
            Names,

            /// <summary>
            /// 汉语姓氏
            /// </summary>
            [TmphPartOfSpeech(ShortName = "nr1", Name = "汉语姓氏", Parent = TmphCategory.Noun)]
            ChineseLastName,

            /// <summary>
            /// 汉语名字
            /// </summary>
            [TmphPartOfSpeech(ShortName = "nr2", Name = "汉语名字", Parent = TmphCategory.Noun)]
            ChineseName,

            /// <summary>
            /// 日语人名
            /// </summary>
            [TmphPartOfSpeech(ShortName = "nrj", Name = "日语人名", Parent = TmphCategory.Noun)]
            JapaneseName,

            /// <summary>
            /// 音译人名
            /// </summary>
            [TmphPartOfSpeech(ShortName = "nrf", Name = "音译人名", Parent = TmphCategory.Noun)]
            TransliterationName,

            /// <summary>
            /// 地名
            /// </summary>
            [TmphPartOfSpeech(ShortName = "ns", Name = "地名", Parent = TmphCategory.Noun)]
            PlaceName,

            /// <summary>
            /// 音译地名
            /// </summary>
            [TmphPartOfSpeech(ShortName = "nsf", Name = "音译地名", Parent = TmphCategory.Noun)]
            TransliterationPlaceName,

            /// <summary>
            /// 机构团体名
            /// </summary>
            [TmphPartOfSpeech(ShortName = "nt", Name = "机构团体名", Parent = TmphCategory.Noun)]
            OrganizationGroupName,

            /// <summary>
            /// 其它专名
            /// </summary>
            [TmphPartOfSpeech(ShortName = "nz", Name = "其它专名", Parent = TmphCategory.Noun)]
            OtherProfessionalName,

            /// <summary>
            /// 名词性惯用语
            /// </summary>
            [TmphPartOfSpeech(ShortName = "nl", Name = "名词性惯用语", Parent = TmphCategory.Noun)]
            NounPhrase,

            /// <summary>
            /// 名词性语素
            /// </summary>
            [TmphPartOfSpeech(ShortName = "ng", Name = "名词性语素", Parent = TmphCategory.Noun)]
            NounMorpheme,

            #endregion 名词

            #region 时间词

            /// <summary>
            /// 时间词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "t", Name = "时间词", Parent = TmphCategory.Time)]
            Time = 2 * CategoryCapacity,

            /// <summary>
            /// 时间词性语素
            /// </summary>
            [TmphPartOfSpeech(ShortName = "tg", Name = "时间词性语素", Parent = TmphCategory.Time)]
            TimeMorpheme,

            #endregion 时间词

            #region 处所词

            /// <summary>
            /// 处所词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "s", Name = "处所词", Parent = TmphCategory.Premises)]
            Premises = 3 * CategoryCapacity,

            #endregion 处所词

            #region 方位词

            /// <summary>
            /// 方位词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "f", Name = "方位词", Parent = TmphCategory.Position)]
            Position = 4 * CategoryCapacity,

            #endregion 方位词

            #region 动词

            /// <summary>
            /// 动词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "v", Name = "动词", Parent = TmphCategory.Verb)]
            Verb = 5 * CategoryCapacity,

            /// <summary>
            /// 副动词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "vd", Name = "副动词", Parent = TmphCategory.Verb)]
            ViceVerb,

            /// <summary>
            /// 名动词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "vn", Name = "名动词", Parent = TmphCategory.Verb)]
            NameVerb,

            /// <summary>
            /// 动词"是"
            /// </summary>
            [TmphPartOfSpeech(ShortName = "vshi", Name = @"动词""是""", Parent = TmphCategory.Verb)]
            IsVerb,

            /// <summary>
            /// 动词"有"
            /// </summary>
            [TmphPartOfSpeech(ShortName = "vyou", Name = @"动词""有""", Parent = TmphCategory.Verb)]
            HaveVerb,

            /// <summary>
            /// 趋向动词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "vf", Name = "趋向动词", Parent = TmphCategory.Verb)]
            TendVerb,

            /// <summary>
            /// 形式动词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "vx", Name = "形式动词", Parent = TmphCategory.Verb)]
            FormVerb,

            /// <summary>
            /// 不及物动词（内动词）
            /// </summary>
            [TmphPartOfSpeech(ShortName = "vi", Name = "不及物动词（内动词）", Parent = TmphCategory.Verb)]
            IntransitiveVerb,

            /// <summary>
            /// 动词性惯用语
            /// </summary>
            [TmphPartOfSpeech(ShortName = "vl", Name = "动词性惯用语", Parent = TmphCategory.Verb)]
            VerbIdiom,

            /// <summary>
            /// 动词性语素
            /// </summary>
            [TmphPartOfSpeech(ShortName = "vg", Name = "动词性语素", Parent = TmphCategory.Verb)]
            VerbMorpheme,

            #endregion 动词

            #region 形容词

            /// <summary>
            /// 形容词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "a", Name = "形容词", Parent = TmphCategory.Adjective)]
            Adjective = 6 * CategoryCapacity,

            /// <summary>
            /// 副形词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "ad", Name = "副形词", Parent = TmphCategory.Adjective)]
            ViceAdjective,

            /// <summary>
            /// 名形词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "an", Name = "名形词", Parent = TmphCategory.Adjective)]
            NameAdjective,

            /// <summary>
            /// 形容词性语素
            /// </summary>
            [TmphPartOfSpeech(ShortName = "ag", Name = "形容词性语素", Parent = TmphCategory.Adjective)]
            AdjectiveMorpheme,

            /// <summary>
            /// 形容词性惯用语
            /// </summary>
            [TmphPartOfSpeech(ShortName = "al", Name = "形容词性惯用语", Parent = TmphCategory.Adjective)]
            AdjectivePhrase,

            #endregion 形容词

            #region 区别词

            /// <summary>
            /// 区别词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "b", Name = "区别词", Parent = TmphCategory.Difference)]
            Difference = 7 * CategoryCapacity,

            /// <summary>
            /// 区别词性惯用语
            /// </summary>
            [TmphPartOfSpeech(ShortName = "bl", Name = "区别词性惯用语", Parent = TmphCategory.Difference)]
            DifferenceIdiom,

            #endregion 区别词

            #region 状态词

            /// <summary>
            /// 状态词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "z", Name = "状态词", Parent = TmphCategory.State)]
            State = 8 * CategoryCapacity,

            #endregion 状态词

            #region 代词

            /// <summary>
            /// 代词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "r", Name = "代词", Parent = TmphCategory.Pronoun)]
            Pronoun = 9 * CategoryCapacity,

            /// <summary>
            /// 人称代词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "rr", Name = "人称代词", Parent = TmphCategory.Pronoun)]
            PersonPronoun,

            /// <summary>
            /// 指示代词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "rz", Name = "指示代词", Parent = TmphCategory.Pronoun)]
            Demonstrative,

            /// <summary>
            /// 时间指示代词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "rzt", Name = "时间指示代词", Parent = TmphCategory.Pronoun)]
            TimePronoun,

            /// <summary>
            /// 处所指示代词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "rzs", Name = "处所指示代词", Parent = TmphCategory.Pronoun)]
            PremisesPronoun,

            /// <summary>
            /// 谓词性指示代词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "rzv", Name = "谓词性指示代词", Parent = TmphCategory.Pronoun)]
            PredicatePronoun,

            /// <summary>
            /// 疑问代词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "ry", Name = "疑问代词", Parent = TmphCategory.Pronoun)]
            InterrogativePronoun,

            /// <summary>
            /// 时间疑问代词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "ryt", Name = "时间疑问代词", Parent = TmphCategory.Pronoun)]
            TimeInterrogativePronoun,

            /// <summary>
            /// 处所疑问代词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "rys", Name = "处所疑问代词", Parent = TmphCategory.Pronoun)]
            PremisesInterrogativePronoun,

            /// <summary>
            /// 谓词性疑问代词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "ryv", Name = "谓词性疑问代词", Parent = TmphCategory.Pronoun)]
            VerbInterrogativePronoun,

            /// <summary>
            /// 代词性语素
            /// </summary>
            [TmphPartOfSpeech(ShortName = "rg", Name = "代词性语素", Parent = TmphCategory.Pronoun)]
            PronounMorpheme,

            #endregion 代词

            #region 数词

            /// <summary>
            /// 数词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "m", Name = "数词", Parent = TmphCategory.Numeral)]
            Numeral = 10 * CategoryCapacity,

            /// <summary>
            /// 数量词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "mq", Name = "数量词", Parent = TmphCategory.Numeral)]
            Quantifier,

            #endregion 数词

            #region 量词

            /// <summary>
            /// 量词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "q", Name = "量词", Parent = TmphCategory.Measure)]
            Measure = 11 * CategoryCapacity,

            /// <summary>
            /// 动量词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "qv", Name = "动量词", Parent = TmphCategory.Measure)]
            Momentum,

            /// <summary>
            /// 时量词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "qt", Name = "时量词", Parent = TmphCategory.Measure)]
            WhenMomentum,

            #endregion 量词

            #region 副词

            /// <summary>
            /// 副词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "d", Name = "副词", Parent = TmphCategory.Adverb)]
            Adverb = 12 * CategoryCapacity,

            #endregion 副词

            #region 介词

            /// <summary>
            /// 介词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "p", Name = "介词", Parent = TmphCategory.Preposition)]
            Preposition = 13 * CategoryCapacity,

            /// <summary>
            /// 介词"把"
            /// </summary>
            [TmphPartOfSpeech(ShortName = "pba", Name = @"介词""把""", Parent = TmphCategory.Preposition)]
            ToPreposition,

            /// <summary>
            /// 介词"被"
            /// </summary>
            [TmphPartOfSpeech(ShortName = "pbei", Name = @"介词""被""", Parent = TmphCategory.Preposition)]
            ByPreposition,

            #endregion 介词

            #region 连词

            /// <summary>
            /// 连词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "c", Name = "连词", Parent = TmphCategory.Conjunction)]
            Conjunction = 14 * CategoryCapacity,

            /// <summary>
            /// 并列连词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "cc", Name = "并列连词", Parent = TmphCategory.Conjunction)]
            CoordinatingConjunction,

            #endregion 连词

            #region 助词

            /// <summary>
            /// 助词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "u", Name = "助词", Parent = TmphCategory.Particle)]
            Particle = 15 * CategoryCapacity,

            /// <summary>
            /// 着
            /// </summary>
            [TmphPartOfSpeech(ShortName = "uzhe", Name = "着", Parent = TmphCategory.Particle)]
            With,

            /// <summary>
            /// 了 喽
            /// </summary>
            [TmphPartOfSpeech(ShortName = "ule", Name = "了 喽", Parent = TmphCategory.Particle)]
            PastTenseMarker,

            /// <summary>
            /// 过
            /// </summary>
            [TmphPartOfSpeech(ShortName = "uguo", Name = "过", Parent = TmphCategory.Particle)]
            Had,

            /// <summary>
            /// 的 底
            /// </summary>
            [TmphPartOfSpeech(ShortName = "ude1", Name = "的 底", Parent = TmphCategory.Particle)]
            Of,

            /// <summary>
            /// 地
            /// </summary>
            [TmphPartOfSpeech(ShortName = "ude2", Name = "地", Parent = TmphCategory.Particle)]
            AdverbialParticle,

            /// <summary>
            /// 得
            /// </summary>
            [TmphPartOfSpeech(ShortName = "ude3", Name = "得", Parent = TmphCategory.Particle)]
            OughtTo,

            /// <summary>
            /// 所
            /// </summary>
            [TmphPartOfSpeech(ShortName = "usuo", Name = "所", Parent = TmphCategory.Particle)]
            The,

            /// <summary>
            /// 等 等等 云云
            /// </summary>
            [TmphPartOfSpeech(ShortName = "udeng", Name = "等 等等 云云", Parent = TmphCategory.Particle)]
            EtCetera,

            /// <summary>
            /// 一样 一般 似的 般
            /// </summary>
            [TmphPartOfSpeech(ShortName = "uyy", Name = "一样 一般 似的 般", Parent = TmphCategory.Particle)]
            Like,

            /// <summary>
            /// 的话
            /// </summary>
            [TmphPartOfSpeech(ShortName = "udh", Name = "的话", Parent = TmphCategory.Particle)]
            If,

            /// <summary>
            /// 来讲 来说 而言 说来
            /// </summary>
            [TmphPartOfSpeech(ShortName = "uls", Name = "来讲 来说 而言 说来", Parent = TmphCategory.Particle)]
            InTermsOf,

            /// <summary>
            /// 之
            /// </summary>
            [TmphPartOfSpeech(ShortName = "uzhi", Name = "之", Parent = TmphCategory.Particle)]
            SubordinateParticle,

            /// <summary>
            /// 连
            /// </summary>
            [TmphPartOfSpeech(ShortName = "ulian", Name = "连", Parent = TmphCategory.Particle)]
            Even,

            #endregion 助词

            #region 叹词

            /// <summary>
            /// 叹词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "e", Name = "叹词", Parent = TmphCategory.Interjection)]
            Interjection = 16 * CategoryCapacity,

            #endregion 叹词

            #region 语气词

            /// <summary>
            /// 语气词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "y", Name = "语气词", Parent = TmphCategory.Tone)]
            Tone = 17 * CategoryCapacity,

            #endregion 语气词

            #region 拟声词

            /// <summary>
            /// 拟声词
            /// </summary>
            [TmphPartOfSpeech(ShortName = "o", Name = "拟声词", Parent = TmphCategory.Onomatopoeia)]
            Onomatopoeia = 18 * CategoryCapacity,

            #endregion 拟声词

            #region 前缀

            /// <summary>
            /// 前缀
            /// </summary>
            [TmphPartOfSpeech(ShortName = "h", Name = "前缀", Parent = TmphCategory.prefix)]
            prefix = 19 * CategoryCapacity,

            #endregion 前缀

            #region 后缀

            /// <summary>
            /// 后缀
            /// </summary>
            [TmphPartOfSpeech(ShortName = "k", Name = "后缀", Parent = TmphCategory.Suffix)]
            Suffix = 20 * CategoryCapacity,

            #endregion 后缀

            #region 字符串

            /// <summary>
            /// 字符串
            /// </summary>
            [TmphPartOfSpeech(ShortName = "x", Name = "字符串", Parent = TmphCategory.String)]
            String = 21 * CategoryCapacity,

            /// <summary>
            /// 非语素字
            /// </summary>
            [TmphPartOfSpeech(ShortName = "xx", Name = "非语素字", Parent = TmphCategory.String)]
            NonMorpheme,

            /// <summary>
            /// 网址URL
            /// </summary>
            [TmphPartOfSpeech(ShortName = "xu", Name = "网址URL", Parent = TmphCategory.String)]
            Url,

            #endregion 字符串

            #region 标点符号

            /// <summary>
            /// 标点符号
            /// </summary>
            [TmphPartOfSpeech(ShortName = "w", Name = "标点符号", Parent = TmphCategory.Punctuation)]
            Punctuation = 22 * CategoryCapacity,

            /// <summary>
            /// 左括号，全角：（ 〔  ［  ｛  《 【  〖〈   半角：( [ { <
            /// </summary>
            [TmphPartOfSpeech(ShortName = "wkz", Name = "左括号", Parent = TmphCategory.Punctuation)]
            LeftParenthesis,

            /// <summary>
            /// 右括号，全角：） 〕  ］ ｝ 》  】 〗 〉 半角： ) ] { >
            /// </summary>
            [TmphPartOfSpeech(ShortName = "wky", Name = "右括号", Parent = TmphCategory.Punctuation)]
            RightParenthesis,

            /// <summary>
            /// 左引号，全角：" ' 『
            /// </summary>
            [TmphPartOfSpeech(ShortName = "wyz", Name = "左引号", Parent = TmphCategory.Punctuation)]
            LeftQuote,

            /// <summary>
            /// 右引号，全角：" ' 』
            /// </summary>
            [TmphPartOfSpeech(ShortName = "wyy", Name = "右引号", Parent = TmphCategory.Punctuation)]
            RightQuote,

            /// <summary>
            /// 句号，全角：。
            /// </summary>
            [TmphPartOfSpeech(ShortName = "wj", Name = "句号", Parent = TmphCategory.Punctuation)]
            Period,

            /// <summary>
            /// 问号，全角：？ 半角：?
            /// </summary>
            [TmphPartOfSpeech(ShortName = "ww", Name = "句号", Parent = TmphCategory.Punctuation)]
            QuestionMark,

            /// <summary>
            /// 叹号，全角：！ 半角：!
            /// </summary>
            [TmphPartOfSpeech(ShortName = "wt", Name = "叹号", Parent = TmphCategory.Punctuation)]
            ExclamationMark,

            /// <summary>
            /// 逗号，全角：， 半角：,
            /// </summary>
            [TmphPartOfSpeech(ShortName = "wd", Name = "逗号", Parent = TmphCategory.Punctuation)]
            Comma,

            /// <summary>
            /// 分号，全角：； 半角： ;
            /// </summary>
            [TmphPartOfSpeech(ShortName = "wf", Name = "分号", Parent = TmphCategory.Punctuation)]
            Semicolon,

            /// <summary>
            /// 顿号，全角：、
            /// </summary>
            [TmphPartOfSpeech(ShortName = "wn", Name = "顿号", Parent = TmphCategory.Punctuation)]
            CommaAnd,

            /// <summary>
            /// 冒号，全角：： 半角： :
            /// </summary>
            [TmphPartOfSpeech(ShortName = "wm", Name = "冒号", Parent = TmphCategory.Punctuation)]
            Colon,

            /// <summary>
            /// 省略号，全角：……  …
            /// </summary>
            [TmphPartOfSpeech(ShortName = "ws", Name = "省略号", Parent = TmphCategory.Punctuation)]
            Ellipsis,

            /// <summary>
            /// 破折号，全角：--   －－   --－   半角：---  ----
            /// </summary>
            [TmphPartOfSpeech(ShortName = "wp", Name = "破折号", Parent = TmphCategory.Punctuation)]
            Dash,

            /// <summary>
            /// 百分号千分号，全角：％ ‰   半角：%
            /// </summary>
            [TmphPartOfSpeech(ShortName = "wb", Name = "百分号千分号", Parent = TmphCategory.Punctuation)]
            PercentThousands,

            /// <summary>
            /// 单位符号，全角：￥ ＄ ￡  °  ℃  半角：$
            /// </summary>
            [TmphPartOfSpeech(ShortName = "wh", Name = "单位符号", Parent = TmphCategory.Punctuation)]
            UnitSymbol,

            #endregion 标点符号
        }

        /// <summary>
        /// 词性
        /// </summary>
        public TmphCategory Catagory;

        ///// <summary>
        ///// 中文分词词语集合
        ///// </summary>
        //private readonly static word[] cache;
        /// <summary>
        /// 中文分词词语集合
        /// </summary>
        public static string[] Words { get; private set; }

        /// <summary>
        /// 中文分词词语集合写入文本文件
        /// </summary>
        private unsafe static void writeTxtFile()
        {
            string[] words = Words;
            using (TmphUnmanagedStream wordStream = new TmphUnmanagedStream())
            {
                *(int*)wordStream.Data = words.Length;
                wordStream.Unsafer.AddLength(sizeof(int));
                foreach (string word in words)
                {
                    wordStream.Write(word);
                    wordStream.Write((char)0);
                }
                TmphSubArray<byte> data = IO.Compression.TmphStream.Deflate.GetCompress(wordStream.GetArray(), 0);
                using (FileStream fileStream = new FileStream(Laurent.Lee.CLB.Config.TmphSearch.Default.WordTxtFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    fileStream.Write(data.Array, data.StartIndex, data.Count);
                }
            }
        }

        ///// <summary>
        ///// 从原始分词文件获取中文分词词语集合
        ///// </summary>
        ///// <param name="txt">原始分词文件内容</param>
        ///// <returns>中文分词词语集合</returns>
        //private static Dictionary<hashString, word> getWordsFormText(string txt)
        //{
        //    string[] strings = txt.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        //    if ((strings.Length & 3) == 0)
        //    {
        //        Dictionary<hashString, word> values = dictionary.CreateOnly<string, word>();
        //        for (int index = 0; index != strings.Length; index += 4)
        //        {
        //            word value = new word
        //            {
        //                Name = strings[index + 1],
        //                Catagory = partOfSpeech.GetCategoryByShortName(strings[index + 3])
        //            };
        //            if (!int.TryParse(strings[index + 2], out value.Frequency)) return null;
        //            if (value.Name.Length * 3 != Encoding.UTF8.GetByteCount(value.Name))
        //            {
        //                Console.WriteLine(value.Name);
        //            }
        //            values[value.Name] = value;
        //        }
        //        return values;
        //    }
        //    return null;
        //}

        unsafe static TmphWord()
        {
            if (Config.TmphSearch.Default.IsSearch)
            {
                try
                {
                    string txtFile = Laurent.Lee.CLB.Config.TmphSearch.Default.WordTxtFileName;
                    if (txtFile != null && File.Exists(txtFile))
                    {
                        TmphSubArray<byte> data;
                        using (FileStream fileStream = new FileStream(txtFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            data = IO.Compression.TmphStream.Deflate.GetDeCompress(fileStream);
                        }
                        fixed (byte* dataFixed = data.Array)
                        {
                            string[] words = new string[*(int*)dataFixed];
                            int index = 0;
                            for (char* start = (char*)(dataFixed + sizeof(int)), read = start; index != words.Length;)
                            {
                                while (*(short*)read != 0) ++read;
                                words[index++] = new string(start, 0, (int)(read - start));
                                start = ++read;
                            }
                            Words = words;
                        }
                    }
                    if (Words == null)
                    {
                        string dataFile = Laurent.Lee.CLB.Config.TmphSearch.Default.WordSerializeFileName;
                        if (dataFile != null && File.Exists(dataFile))
                        {
                            TmphSubArray<byte> data;
                            using (FileStream fileStream = new FileStream(dataFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                data = IO.Compression.TmphStream.Deflate.GetDeCompress(fileStream);
                            }
                            TmphWord[] words = Laurent.Lee.CLB.Emit.TmphDataDeSerializer.DeSerialize<TmphWord[]>(data);
                            Words = words.getArray(value => value.Name);
                            if (Words.Length != 0 && txtFile != null) Laurent.Lee.CLB.Threading.TmphThreadPool.TinyPool.Start(writeTxtFile);
                        }
                    }
                }
                catch (Exception error)
                {
                    CLB.TmphLog.Error.Add(error, null, false);
                }
            }
            if (Words == null) Words = TmphNullValue<string>.Array;
        }
    }
}