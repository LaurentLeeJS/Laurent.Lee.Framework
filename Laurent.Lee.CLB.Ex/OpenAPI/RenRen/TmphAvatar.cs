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

namespace Laurent.Lee.CLB.OpenAPI.RenRen
{
    /// <summary>
    /// 用户头像
    /// </summary>
    public struct avatar
    {
        /// <summary>
        /// 头像类型
        /// </summary>
        public enum sizeType
        {
            /// <summary>
            /// 大头像
            /// </summary>
            large,

            /// <summary>
            /// 主头像
            /// </summary>
            main,

            /// <summary>
            /// 普通头像
            /// </summary>
            avatar,

            /// <summary>
            /// 小头像
            /// </summary>
            tiny,
        }

        /// <summary>
        /// 头像类型
        /// </summary>
        public sizeType type;

        /// <summary>
        /// 头像地址
        /// </summary>
        public string url;
    }
}