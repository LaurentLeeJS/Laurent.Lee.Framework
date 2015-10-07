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
    /// 空间日志地址
    /// </summary>
    public sealed class blogReturn : isValue
    {
        /// <summary>
        /// 空间日志ID
        /// </summary>
        public int blogid;

        /// <summary>
        /// 固定为http://i.qq.com。用户在登录态下，可以通过该URL直接进入空间首页（这里不直接返回新发表日志的URL是为了避免泄漏用户信息）
        /// </summary>
        public string url;
    }
}