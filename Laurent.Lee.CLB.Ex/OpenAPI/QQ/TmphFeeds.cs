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

using System;

namespace Laurent.Lee.CLB.OpenAPI.QQ
{
    /// <summary>
    /// 发表动态
    /// </summary>
    public partial class feeds : apiFormat
    {
        /// <summary>
        /// 最长36个中文字，超出部分会被截断，必填项
        /// </summary>
        public string title;

        /// <summary>
        /// 分享所在网页资源的链接，点击后跳转至第三方网页，必填项
        /// </summary>
        public string url;

        /// <summary>
        /// 分享的来源网站名称，请填写网站申请接入时注册的网站名称，必填项
        /// </summary>
        public string site;

        /// <summary>
        /// 分享的来源网站对应的网站地址url，必填项
        /// </summary>
        public string fromurl;

        /// <summary>
        /// 数据是否有效
        /// </summary>
        public bool IsValue
        {
            get
            {
                return (title.Length | url.Length | fromurl.Length) != 0
                    && url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && fromurl.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// 用户评论内容，也叫发表分享时的分享理由，禁止使用系统生产的语句进行代替，最长40个中文字，超出部分会被截断
        /// </summary>
        public string comment;

        /// <summary>
        /// 所分享的网页资源的摘要内容，或者是网页的概要描述，最长80个中文字，超出部分会被截断
        /// </summary>
        public string summary;

        /// <summary>
        /// 所分享的网页资源的代表性图片链接"，长度限制255字符，多张图片以竖线（|）分隔，目前只有第一张图片有效，图片规格100*100为佳
        /// </summary>
        public string images;

        /// <summary>
        /// 分享内容的类型。4表示网页；5表示视频（type=5时，必须传入playurl）
        /// </summary>
        public string type = "4";

        /// <summary>
        /// 长度限制为256字节。仅在type=5的时候有效，表示视频的swf播放地址
        /// </summary>
        public string playurl;

        /// <summary>
        /// 值为1时，表示分享不默认同步到微博，其他值或者不传此参数表示默认同步到微博
        /// </summary>
        public string nswb;
    }
}