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

namespace Laurent.Lee.CLB.OpenAPI.RenRen
{
    /// <summary>
    /// API调用http://wiki.dev.renren.com/wiki/Authorization#.E6.9C.8D.E5.8A.A1.E7.AB.AF.E6.B5.81.E7.A8.8B
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
        /// 访问令牌用户
        /// </summary>
        public tokenUser User
        {
            get { return token.user; }
        }

        /// <summary>
        /// 访问令牌+用户身份的标识
        /// </summary>
        public refreshToken RefreshToken
        {
            get
            {
                return new refreshToken { access_token = token.access_token, refresh_token = token.refresh_token, id = token.user.id };
            }
        }

        /// <summary>
        /// API调用
        /// </summary>
        /// <param name="config">应用配置</param>
        /// <param name="token">访问令牌</param>
        public api(config config, token token)
        {
            this.config = config;
            this.token = token;
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
            return config.Request.RequestJson<TJsonType, TFormType>(url, form);
        }

        /// <summary>
        /// 发表一个网页分享，分享应用中的内容给好友
        /// </summary>
        /// <param name="value">网页分享</param>
        /// <returns>是否成功,失败返回null</returns>
        public share AddShare(shareQuery value)
        {
            value.method = "share.share";
            return form<share, shareQuery>(@"https://api.renren.com/restserver.do", value);
        }

        //string query = "access_token=" + accessToken + "&method=users.getInfo&fields=uid,name,sex,mainurl&call_id=" + DateTime.Now.Ticks.ToString() + @"&v=1.0&format=JSON";
        //using (MD5 md5 = new MD5CryptoServiceProvider())
        //{
        //    url = new Uri("http://api.renren.com/restserver.do?" + query + "&sig" + md5.ComputeHash((query + Laurent.Lee.CLB.Config.web.login.renren.secretKey).bytes()).toLowerHex());
        //}
        //data = null;
        //Monitor.Enter(WebClientLock);
        //try
        //{
        //    data = WebClient.DownloadData(url);
        //}
        //catch (Exception error)
        //{
        //    TmphLog.Default.Add(error, null, true);
        //}
    }
}