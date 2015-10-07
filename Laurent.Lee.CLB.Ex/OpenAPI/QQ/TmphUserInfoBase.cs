﻿/*
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
    /// 登录用户在QQ空间的信息
    /// </summary>
    public abstract class userInfoBase : isValue
    {
        /// <summary>
        /// 昵称
        /// </summary>
        public string nickname;

        /// <summary>
        /// 大小为30×30像素的头像URL
        /// </summary>
        public string figureurl;

        /// <summary>
        /// 大小为50×50像素的头像URL
        /// </summary>
        public string figureurl_1;

        /// <summary>
        /// 大小为100×100像素的头像URL
        /// </summary>
        public string figureurl_2;
    }
}