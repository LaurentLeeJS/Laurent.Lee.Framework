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
    /// 分享
    /// </summary>
    public sealed class share : TmphIValue
    {
        /// <summary>
        /// 分享的ID
        /// </summary>
        public long id;

        /// <summary>
        /// 数据是否有效
        /// </summary>
        public bool IsValue
        {
            get { return id != 0; }
        }

        /// <summary>
        /// 提示信息
        /// </summary>
        public string Message
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// 发起本次分享的用户ID
        /// </summary>
        public int user_id;

        /// <summary>
        /// 最初发起此分享的用户ID，若为0，表示user_id即为original_user_id
        /// </summary>
        public int original_user_id;

        /// <summary>
        /// 被分享资源的ID
        /// </summary>
        public long resource_id;

        /// <summary>
        /// 被分享资源所有者的用户ID
        /// </summary>
        public int resource_owner_id;

        /// <summary>
        /// 分享的标题
        /// </summary>
        public string title;

        /// <summary>
        /// 分享的链接地址
        /// </summary>
        public string url;

        /// <summary>
        /// 分享的缩略图
        /// </summary>
        public string thumbnail_url;

        /// <summary>
        /// 分享的内容的摘要
        /// </summary>
        public string summary;

        /// <summary>
        /// 分享被评论的次数
        /// </summary>
        public int comment_count;

        /// <summary>
        /// 分享产生的时间
        /// </summary>
        public string share_time;

        /// <summary>
        /// 当前access_token(或session_key)对应用户是否like（赞）
        /// </summary>
        public int my_like;

        /// <summary>
        /// 分享被like（赞）的次数
        /// </summary>
        public int like_count;
    }
}