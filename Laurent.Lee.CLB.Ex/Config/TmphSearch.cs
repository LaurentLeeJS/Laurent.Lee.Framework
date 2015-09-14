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

namespace Laurent.Lee.CLB.Config
{
    /// <summary>
    ///     中文分词+搜索 相关参数
    /// </summary>
    public sealed class TmphSearch
    {
        /// <summary>
        ///     默认中文分词+搜索 相关参数
        /// </summary>
        public static readonly TmphSearch Default = new TmphSearch();

        /// <summary>
        ///     是否初始化搜索模块
        /// </summary>
        private readonly bool _isSearch = !TmphPub.Default.IsDebug;

        /// <summary>
        ///     中文分词序列化文件名称
        /// </summary>
        private readonly string _wordSerializeFileName = CLB.TmphPub.ApplicationPath +
                                                         CLB.TmphPub.LaurentLeeFramework + ".search.word.data";

        /// <summary>
        ///     中文分词文本文件名称
        /// </summary>
        private readonly string _wordTxtFileName = CLB.TmphPub.ApplicationPath + CLB.TmphPub.LaurentLeeFramework +
                                                   ".search.word.txt";

        /// <summary>
        ///     中文分词+搜索 相关参数
        /// </summary>
        private TmphSearch()
        {
            TmphPub.LoadConfig(this);
        }

        /// <summary>
        ///     是否初始化搜索模块
        /// </summary>
        public bool IsSearch
        {
            get { return _isSearch; }
        }

        /// <summary>
        ///     中文分词文本文件名称
        /// </summary>
        public string WordTxtFileName
        {
            get { return _wordTxtFileName; }
        }

        /// <summary>
        ///     中文分词序列化文件名称
        /// </summary>
        public string WordSerializeFileName
        {
            get { return _wordSerializeFileName; }
        }
    }
}