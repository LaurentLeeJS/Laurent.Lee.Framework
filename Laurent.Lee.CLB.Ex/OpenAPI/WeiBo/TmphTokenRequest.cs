using System;

namespace Laurent.Lee.CLB.OpenAPI.WeiBo
{
    /// <summary>
    /// 访问令牌请求
    /// </summary>
    public partial class TmphTokenRequest : TmphConfig
    {
#pragma warning disable 414
        /// <summary>
        /// 请求的类型
        /// </summary>
        private string grant_type = "authorization_code";
#pragma warning restore 414
        /// <summary>
        /// authorization_code
        /// </summary>
        public string TmphCode;
    }
}
