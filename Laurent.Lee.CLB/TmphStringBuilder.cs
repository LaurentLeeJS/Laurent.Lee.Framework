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

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     字符串连接(适应于较长的字符串链接,一个一个字符添加的请用unmanagedStream)
    /// </summary>
    public sealed class TmphStringBuilder : TmphList<string>
    {
        /// <summary>
        ///     初始化字符串连接
        /// </summary>
        public TmphStringBuilder()
        {
        }

        /// <summary>
        ///     初始化字符串连接
        /// </summary>
        /// <param name="length">字符串集合初始大小</param>
        public TmphStringBuilder(int length) : base(length)
        {
        }

        /// <summary>
        ///     初始化字符串连接
        /// </summary>
        /// <param name="value">字符串值</param>
        public TmphStringBuilder(string value)
        {
            Add(value);
        }

        /// <summary>
        ///     初始化字符串连接
        /// </summary>
        /// <param name="value">字符串集合</param>
        public TmphStringBuilder(params string[] value) : base(value, false)
        {
        }

        #region 连接字符串

        /// <summary>
        ///     连接字符串
        /// </summary>
        /// <param name="joinValue">字符串连接</param>
        /// <returns>连接后的字符串</returns>
        public string Join(string joinValue)
        {
            return joinValue == null ? ToString() : string.Join(joinValue, ToArray());
        }

        #endregion 连接字符串

        #region 生成字符串

        /// <summary>
        ///     生成字符串
        /// </summary>
        /// <returns>字符串</returns>
        public override string ToString()
        {
            return string.Concat(ToArray());
        }

        #endregion 生成字符串

        #region 添加字符串

        /// <summary>
        ///     添加字符
        /// </summary>
        /// <param name="value">字符值</param>
        public void Append(char value)
        {
            Add(new string(value, 1));
        }

        /// <summary>
        ///     添加字符
        /// </summary>
        /// <param name="value">字符值</param>
        /// <param name="count">字符数</param>
        public void Append(char value, int count)
        {
            if (count > 0) Add(new string(value, count));
        }

        /// <summary>
        ///     添加字符串
        /// </summary>
        /// <param name="values">字符串集合</param>
        /// <returns>当前字符串连接</returns>
        public void Append(params string[] values)
        {
            Add(values);
        }

        /// <summary>
        ///     添加字符串
        /// </summary>
        /// <param name="values">字符串集合</param>
        /// <returns>当前字符串连接</returns>
        public void AppendConcat(params string[] values)
        {
            Add(string.Concat(values));
        }

        #endregion 添加字符串
    }
}