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

namespace Laurent.Lee.CLB.OpenAPI.WeiBo
{
    /// <summary>
    /// 访问令牌
    /// </summary>
    public sealed class TmphToken : TmphIValue
    {
        /// <summary>
        /// 访问令牌
        /// </summary>
        public string access_token;

        /// <summary>
        /// 当前授权用户的UID
        /// </summary>
        public string uid;

        /// <summary>
        /// 生命周期，单位是秒数
        /// </summary>
        public int expires_in;

        /// <summary>
        /// 数据是否有效
        /// </summary>
        public bool IsValue
        {
            get { return access_token.Length != 0 && uid.Length != 0 && expires_in != 0; }
        }

        /// <summary>
        /// 提示信息
        /// </summary>
        public string Message
        {
            get { return null; }
        }
    }
}