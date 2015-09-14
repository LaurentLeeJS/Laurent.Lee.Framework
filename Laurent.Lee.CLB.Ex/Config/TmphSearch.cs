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