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
    /// API调用表单
    /// </summary>
    public abstract class apiForm
    {
        /// <summary>
        /// 调用名称
        /// </summary>
        public string method;

        /// <summary>
        /// API的版本号，固定值为1.0
        /// </summary>
        public string v = "1.0";

        /// <summary>
        /// OAuth2.0验证授权后获得的token。同时兼容session_key方式调用。（必填项）
        /// </summary>
        public string access_token;

        /// <summary>
        /// 返回值的格式。请指定为JSON或者XML，推荐使用JSON，缺省值为XML
        /// </summary>
        public string format = "JSON";
    }
}