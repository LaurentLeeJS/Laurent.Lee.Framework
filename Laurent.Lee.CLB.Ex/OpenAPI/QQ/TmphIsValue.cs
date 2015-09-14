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

namespace Laurent.Lee.CLB.OpenAPI.QQ
{
    /// <summary>
    /// 数据是否有效
    /// </summary>
    public abstract class isValue : TmphIValue
    {
        /// <summary>
        /// 返回码，0表示正确返回
        /// </summary>
        public int ret = -1;

#pragma warning disable

        /// <summary>
        /// 如果ret小于0，会有相应的错误信息提示
        /// </summary>
        public string msg;

#pragma warning restore

        /// <summary>
        /// 数据是否有效
        /// </summary>
        public bool IsValue
        {
            get { return ret == 0; }
        }

        /// <summary>
        /// 提示信息
        /// </summary>
        public virtual string Message
        {
            get
            {
                return "[" + ret.toString() + "]" + msg;
            }
        }

        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="value">数据是否有效</param>
        /// <returns>数据是否有效</returns>
        public static implicit operator bool (isValue value) { return value != null && value.IsValue; }
    }
}