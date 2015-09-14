using System;

namespace Laurent.Lee.CLB.OpenAPI.QQ
{
    /// <summary>
    /// API调用表单
    /// </summary>
    public abstract class apiForm : appId
    {
        /// <summary>
        /// 访问令牌
        /// </summary>
        public string access_token;
        /// <summary>
        /// 用户身份的标识
        /// </summary>
        public string openid;
    }
}
