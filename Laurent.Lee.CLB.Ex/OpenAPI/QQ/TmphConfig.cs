using System;
using System.Web;
using System.Threading;
using System.Text;
using Laurent.Lee.CLB;
using Laurent.Lee.CLB.Threading;
using Laurent.Lee.CLB.Net;
using System.Collections.Specialized;

namespace Laurent.Lee.CLB.OpenAPI.QQ
{
    /// <summary>
    /// 应用配置
    /// </summary>
    public class config
    {
        /// <summary>
        /// 编码绑定请求
        /// </summary>
        public static readonly TmphEncodingRequest Request = new TmphEncodingRequest(OpenAPI.TmphRequest.Default, Encoding.UTF8);
#pragma warning disable 649
        /// <summary>
        /// 申请接入时注册的网站名称
        /// </summary>
        public string site;
        /// <summary>
        /// appid
        /// </summary>
        public string client_id;
        /// <summary>
        /// appkey
        /// </summary>
        private string client_secret;
        /// <summary>
        /// 登陆成功回调地址
        /// </summary>
        private string redirect_uri;
#pragma warning restore 649
        /// <summary>
        /// URL编码 登陆成功回调地址
        /// </summary>
        private string encodeRedirectUri;
        /// <summary>
        /// URL编码 登陆成功回调地址
        /// </summary>
        public string EncodeRedirectUri
        {
            get
            {
                if (encodeRedirectUri == null) encodeRedirectUri = HttpUtility.UrlEncode(redirect_uri);
                return encodeRedirectUri;
            }
        }
        /// <summary>
        /// 访问令牌 查询字符串
        /// </summary>
        private const string access_token = "access_token=";
        /// <summary>
        /// 有效期，单位为秒 查询字符串
        /// </summary>
        private const string expires_in = "expires_in=";
        /// <summary>
        /// 获取一个新令牌
        /// </summary>
        /// <param name="TmphCode">authorization_code</param>
        /// <returns>一个新令牌</returns>
        private token getToken(string TmphCode)
        {
            string data = Request.Request("https://graph.qq.com/oauth2.0/token?grant_type=authorization_code&client_id=" + client_id + "&client_secret=" + client_secret + "&TmphCode=" + TmphCode + "&redirect_uri=" + EncodeRedirectUri);
            token token = new token();
            if (data != null)
            {
                foreach (TmphSubString query in data.Split('&'))
                {
                    if (query.StartsWith(access_token)) token.access_token = query.Substring(access_token.Length);
                    else if (query.StartsWith(expires_in)) int.TryParse(query.Substring(expires_in.Length), out token.expires_in);
                }
                if (!token.IsToken) TmphLog.Default.Add(data, false, false);
            }
            return token;
        }
        /// <summary>
        /// 获取api调用
        /// </summary>
        /// <param name="TmphCode">authorization_code</param>
        /// <returns>API调用,失败返回null</returns>
        public api GetApi(string TmphCode)
        {
            if (site.Length == 0) TmphLog.Error.Add("网站名称不能为空", false, true);
            else
            {
                token token = getToken(TmphCode);
                openId openId = token.GetOpenId();
                if (openId.openid != null) return new api(this, token, openId);
            }
            return null;
        }
        /// <summary>
        /// 获取api调用
        /// </summary>
        /// <param name="tokenOpenId">访问令牌+用户身份的标识</param>
        /// <returns>API调用,失败返回null</returns>
        public api GetApi(tokenOpenId tokenOpenId)
        {
            if (tokenOpenId.Token.Length != 0 && tokenOpenId.OpenId.Length != 0)
            {
                return new api(this, new token { access_token = tokenOpenId.Token, expires_in = -1 }, new openId { openid = tokenOpenId.OpenId, client_id = client_id });
            }
            return null;
        }
        /// <summary>
        /// 获取api调用
        /// </summary>
        /// <param name="tokenOpenId">访问令牌+用户身份的标识</param>
        /// <returns>API调用,失败返回null</returns>
        public api GetApiByJson(string tokenOpenId)
        {
            tokenOpenId value = new tokenOpenId();
            return Laurent.Lee.CLB.Emit.TmphJsonParser.Parse(tokenOpenId, ref value) ? GetApi(value) : null;
        }
        /// <summary>
        /// 默认配置
        /// </summary>
        public static readonly config Default = Laurent.Lee.CLB.Config.TmphPub.LoadConfig<config>(new config());
    }
}
