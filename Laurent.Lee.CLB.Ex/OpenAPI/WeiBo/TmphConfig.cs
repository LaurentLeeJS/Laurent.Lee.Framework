using System;
using System.Text;
using Laurent.Lee.CLB;

namespace Laurent.Lee.CLB.OpenAPI.WeiBo
{
    /// <summary>
    /// 应用配置
    /// </summary>
    public class TmphConfig
    {
        /// <summary>
        /// 编码绑定请求
        /// </summary>
        public static readonly TmphEncodingRequest Request = new TmphEncodingRequest(OpenAPI.TmphRequest.Default, Encoding.UTF8);
        /// <summary>
        /// appid
        /// </summary>
        protected string client_id;
        /// <summary>
        /// appkey
        /// </summary>
        protected string client_secret;
        /// <summary>
        /// 登陆成功回调地址
        /// </summary>
        protected string redirect_uri;
        /// <summary>
        /// 获取api调用
        /// </summary>
        /// <param name="TmphCode">authorization_code</param>
        /// <returns>API调用,失败返回null</returns>
        public TmphApi GetApi(string TmphCode)
        {
            TmphToken token = Request.RequestJson<TmphToken, TmphTokenRequest>(@"https://api.weibo.com/oauth2/access_token", new TmphTokenRequest
            {
                client_id = client_id,
                client_secret = client_secret,
                redirect_uri = redirect_uri,
                TmphCode = TmphCode
            });
            return token != null ? new TmphApi(this, token) : null;
        }
        /// <summary>
        /// 获取api调用
        /// </summary>
        /// <param name="tokenUid">访问令牌+用户身份的标识</param>
        /// <returns>API调用,失败返回null</returns>
        public TmphApi GetApi(TmphTokenUid tokenUid)
        {
            if (tokenUid.Token.Length != 0 && tokenUid.Uid.Length != 0)
            {
                return new TmphApi(this, new TmphToken { access_token = tokenUid.Token, uid = tokenUid.Uid, expires_in = -1 });
            }
            return null;
        }
        /// <summary>
        /// 获取api调用
        /// </summary>
        /// <param name="tokenOpenId">访问令牌+用户身份的标识</param>
        /// <returns>API调用,失败返回null</returns>
        public TmphApi GetApiByJson(string tokenOpenId)
        {
            TmphTokenUid value = new TmphTokenUid();
            return Laurent.Lee.CLB.Emit.TmphJsonParser.Parse(tokenOpenId, ref value) ? GetApi(value) : null;
        }
        /// <summary>
        /// 默认配置
        /// </summary>
        public static readonly TmphConfig Default = Laurent.Lee.CLB.Config.TmphPub.LoadConfig<TmphConfig>(new TmphConfig());
    }
}
