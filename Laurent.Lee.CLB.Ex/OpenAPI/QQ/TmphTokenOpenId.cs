using System;

namespace Laurent.Lee.CLB.OpenAPI.QQ
{
    /// <summary>
    /// 访问令牌+用户身份的标识，用于保存
    /// </summary>
    public struct tokenOpenId
    {
        /// <summary>
        /// 访问令牌
        /// </summary>
        public string Token;
        /// <summary>
        /// 用户身份的标识
        /// </summary>
        public string OpenId;
    }
}
