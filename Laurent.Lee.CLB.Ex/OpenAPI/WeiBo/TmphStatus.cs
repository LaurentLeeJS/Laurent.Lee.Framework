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
    /// 微博信息
    /// </summary>
    public sealed class TmphStatus : TmphIValue
    {
        /// <summary>
        /// 数据是否有效
        /// </summary>
        public bool IsValue
        {
            get { return id != 0 && idstr.Length != 0; }
        }

        /// <summary>
        /// 提示信息
        /// </summary>
        public string Message
        {
            get { return null; }
        }

        /// <summary>
        /// 微博创建时间
        /// </summary>
        public string created_at;

        /// <summary>
        /// 微博ID
        /// </summary>
        public long id;

        /// <summary>
        /// 微博MID
        /// </summary>
        public string mid;

        /// <summary>
        /// 字符串型的微博ID
        /// </summary>
        public string idstr;

        /// <summary>
        /// 微博信息内容
        /// </summary>
        public string text;

        /// <summary>
        /// 微博来源
        /// </summary>
        public string source;

        /// <summary>
        /// 是否已收藏，true：是，false：否
        /// </summary>
        public bool favorited;

        /// <summary>
        /// 是否被截断，true：是，false：否
        /// </summary>
        public bool truncated;

        /// <summary>
        /// （暂未支持）回复ID
        /// </summary>
        public string in_reply_to_status_id;

        /// <summary>
        /// （暂未支持）回复人UID
        /// </summary>
        public string in_reply_to_user_id;

        /// <summary>
        /// （暂未支持）回复人昵称
        /// </summary>
        public string in_reply_to_screen_name;

        /// <summary>
        /// 缩略图片地址，没有时不返回此字段
        /// </summary>
        public string thumbnail_pic;

        /// <summary>
        /// 中等尺寸图片地址，没有时不返回此字段
        /// </summary>
        public string bmiddle_pic;

        /// <summary>
        /// 原始图片地址，没有时不返回此字段
        /// </summary>
        public string original_pic;

        /// <summary>
        /// 地理信息字段
        /// </summary>
        public TmphGeo geo;

        /// <summary>
        /// 微博作者的用户信息字段
        /// </summary>
        public TmphUser user;

        /// <summary>
        /// 被转发的原微博信息字段，当该微博为转发微博时返回
        /// </summary>
        public TmphStatus retweeted_status;

        /// <summary>
        /// 转发数
        /// </summary>
        public int reposts_count;

        /// <summary>
        /// 评论数
        /// </summary>
        public int comments_count;

        /// <summary>
        /// 表态数
        /// </summary>
        public int attitudes_count;

        /// <summary>
        /// 暂未支持
        /// </summary>
        public int mlevel;

        /// <summary>
        /// 微博的可见性及指定可见分组信息
        /// </summary>
        public TmphVisible visible;
    }
}