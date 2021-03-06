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
    /// API调用http://wiki.opensns.qq.com/wiki/%E3%80%90QQ%E7%99%BB%E5%BD%95%E3%80%91API%E6%96%87%E6%A1%A3
    /// </summary>
    public class api
    {
        /// <summary>
        /// 应用配置
        /// </summary>
        private config config;

        /// <summary>
        /// 访问令牌
        /// </summary>
        private token token;

        /// <summary>
        /// 用户身份的标识
        /// </summary>
        private openId openId;

        /// <summary>
        /// 用户身份的标识
        /// </summary>
        public string OpenId
        {
            get { return openId.openid; }
        }

        /// <summary>
        /// 访问令牌+用户身份的标识
        /// </summary>
        public tokenOpenId TokenOpenId
        {
            get
            {
                return new tokenOpenId { Token = token.access_token, OpenId = openId.openid };
            }
        }

        /// <summary>
        /// 请求字符串
        /// </summary>
        private string query;

        /// <summary>
        /// API调用
        /// </summary>
        /// <param name="config">应用配置</param>
        /// <param name="token">访问令牌</param>
        /// <param name="openId">用户身份的标识</param>
        public api(config config, token token, openId openId)
        {
            this.config = config;
            this.token = token;
            this.openId = openId;
            query = "access_token=" + token.access_token + "&oauth_consumer_key=" + config.client_id + "&openid=" + openId.openid;
        }

        /// <summary>
        /// 登录用户在QQ空间的信息[仅网站]
        /// </summary>
        /// <returns>登录用户在QQ空间的信息,失败返回null</returns>
        public userInfo GetUserInfo()
        {
            return config.Request.RequestJson<userInfo>(@"https://graph.qq.com/user/get_user_info?" + query + "&format=json");
        }

        /// <summary>
        /// 登录用户在QQ空间的简单个人信息gde
        /// </summary>
        /// <returns>登录用户在QQ空间的简单个人信息,失败返回null</returns>
        public simpleUserInfo GetSimpleUserInfo()
        {
            return config.Request.RequestJson<simpleUserInfo>(@"https://graph.qq.com/user/get_simple_userinfo?" + query);
        }

        /// <summary>
        /// 表单提交
        /// </summary>
        /// <typeparam name="TJsonType">json数据数据类型</typeparam>
        /// <typeparam name="TFormType">表单数据类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="form">POST表单</param>
        /// <returns>数据对象,失败放回null</returns>
        private TJsonType format<TJsonType, TFormType>(string url, TFormType form)
            where TJsonType : class, TmphIValue
            where TFormType : apiFormat
        {
            form.format = "json";
            return this.form<TJsonType, TFormType>(url, form);
        }

        /// <summary>
        /// 表单提交
        /// </summary>
        /// <typeparam name="TJsonType">json数据数据类型</typeparam>
        /// <typeparam name="TFormType">表单数据类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="form">POST表单</param>
        /// <returns>数据对象,失败放回null</returns>
        private TJsonType form<TJsonType, TFormType>(string url, TFormType form)
            where TJsonType : class, TmphIValue
            where TFormType : apiForm
        {
            form.access_token = token.access_token;
            form.oauth_consumer_key = config.client_id;
            form.openid = openId.openid;
            return config.Request.RequestJson<TJsonType, TFormType>(url, form);
        }

        /// <summary>
        /// 发表一个网页分享，分享应用中的内容给好友
        /// </summary>
        /// <param name="value">网页分享</param>
        /// <returns>是否成功</returns>
        public bool AddShare(feeds value)
        {
            value.site = config.site;
            return format<isValue, feeds>(@"https://graph.qq.com/share/add_share", value);
        }

        ///// <summary>
        ///// 发表日志到QQ空间
        ///// </summary>
        ///// <param name="value">空间日志</param>
        ///// <returns>空间日志地址,失败返回null</returns>
        //public blogReturn AddBlog(blog value)
        //{
        //    return form<blogReturn, blog, blog.memberMap>(@"https://graph.qq.com/blog/add_one_blog", value);
        //}
        ///// <summary>
        ///// 拉取第三方分享的评论列表
        ///// </summary>
        ///// <param name="value">评论列表请求</param>
        ///// <returns>第三方分享的评论列表</returns>
        //public list<comment> GetComment(commentQuery value)
        //{
        //    //需要xml解析
        //    value.oauth_consumer_key = config.client_id;
        //    //https://graph.qq.com/share/get_comment
        //    return null;
        //}
        /// <summary>
        /// 发表一条微博信息（纯文本）到腾讯微博平台上
        /// </summary>
        /// <param name="value">微博信息</param>
        /// <returns>微博ID,失败返回null</returns>
        public string AddMicroblog(microblog value)
        {
            return format<microblogReturn, microblog>(@"https://graph.qq.com/t/add_t", value);
        }
    }
}