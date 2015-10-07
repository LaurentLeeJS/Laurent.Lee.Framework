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
    /// 空间日志
    /// </summary>
    public partial class blog : apiForm
    {
        /// <summary>
        /// 日志标题，纯文本，最大长度128个字节，UTF-8编码，必填项
        /// </summary>
        public string title;

        /// <summary>
        /// 日志内容。HTML格式字符串，最大长度100*1024个字节，UTF-8编码，必填项。注意：字符串中不允许包括以下特殊标签：html，head，body，script，input，frame，meta，form，applet，xml，textarea，base，link等
        /// </summary>
        public string content;
    }
}