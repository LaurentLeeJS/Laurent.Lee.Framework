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
    /// 第三方分享的评论列表 请求
    /// </summary>
    public partial class commentQuery : appId
    {
        /// <summary>
        /// 网页的URL，查询评论时的起始位置，一般情况下可以不传值或传入0，表示从第一条开始读取评论列表
        /// </summary>
        public string url;

        /// <summary>
        /// start参数是为一种特殊情况准备的，即需要分页展示评论时，则start可设置为该页显示的条数。例如如果start为10，则会跳过第10条评论，从第11条评论开始读取。如果传入的start比实际总的评论数还要大，则读取到0条评论
        /// </summary>
        public int start;

        /// <summary>
        /// 表示查询评论的返回限制数（即最多期望返回几条评论）。num不传则默认返回200条评论，所有评论不足200条则返回所有评论
        /// </summary>
        public int num;
    }
}