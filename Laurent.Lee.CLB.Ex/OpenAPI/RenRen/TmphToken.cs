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
    /// 访问令牌
    /// </summary>
    public sealed class token : TmphIValue
    {
#pragma warning disable

        /// <summary>
        /// 访问令牌
        /// </summary>
        public string access_token;

        /// <summary>
        /// 访问令牌用户
        /// </summary>
        public tokenUser user;

        /// <summary>
        /// 刷新访问令牌
        /// </summary>
        public string refresh_token;

        /// <summary>
        /// 生命周期，单位是秒数
        /// </summary>
        public int expires_in;

        /// <summary>
        /// 访问范围
        /// </summary>
        public string scope;

        /// <summary>
        /// 错误码
        /// </summary>
        public string error;

        /// <summary>
        /// 一段人类可读的文字，用来帮助理解和解决发生的错误
        /// </summary>
        public string error_description;

        /// <summary>
        /// 一个人类可读的网页URI，带有关于错误的信息，用来为终端用户提供与错误有关的额外信息
        /// </summary>
        public string error_uri;

#pragma warning restore

        /// <summary>
        /// 数据是否有效
        /// </summary>
        public bool IsValue
        {
            get { return access_token.Length != 0 && expires_in != 0 && user.id != 0; }
        }

        /// <summary>
        /// 提示信息
        /// </summary>
        public string Message
        {
            get
            {
                return error + " | " + error_uri + @"
" + error_description;
            }
        }
    }
}