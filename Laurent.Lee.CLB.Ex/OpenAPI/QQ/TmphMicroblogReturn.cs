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
    /// 微博
    /// </summary>
    public sealed class microblogReturn : errorCode
    {
        /// <summary>
        /// 微博ID
        /// </summary>
        public struct microblogId
        {
            /// <summary>
            /// 微博消息的ID，用来唯一标识一条微博消息
            /// </summary>
            public string id;

            /// <summary>
            /// 微博消息的发表时间
            /// </summary>
            public long time;
        }

        /// <summary>
        /// 微博ID
        /// </summary>
        public microblogId data;

        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="value">微博信息</param>
        /// <returns>微博ID</returns>
        public static implicit operator string (microblogReturn value) { return value != null && value.IsValue ? value.data.id : null; }
    }
}