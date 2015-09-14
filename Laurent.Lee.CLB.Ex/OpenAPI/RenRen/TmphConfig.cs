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

using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Laurent.Lee.CLB.OpenAPI.RenRen
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

        /// <summary>
        /// 默认空表单
        /// </summary>
        private static readonly NameValueCollection defaultForm = new NameValueCollection();

#pragma warning disable 649

        /// <summary>
        /// appid
        /// </summary>
        private string client_id;

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
        /// 获取api调用
        /// </summary>
        /// <param name="TmphCode">authorization_code</param>
        /// <returns>API调用,失败返回null</returns>
        public api GetApi(string TmphCode)
        {
            token token = Request.RequestJson<token>("https://graph.renren.com/oauth/token?grant_type=authorization_code&client_id=" + client_id + "&redirect_uri=" + EncodeRedirectUri + "&client_secret=" + client_secret + "&TmphCode=" + TmphCode, defaultForm);
            if (token != null) return new api(this, token);
            return null;
        }

        /// <summary>
        /// 默认配置
        /// </summary>
        public static readonly config Default = Laurent.Lee.CLB.Config.TmphPub.LoadConfig<config>(new config());
    }
}