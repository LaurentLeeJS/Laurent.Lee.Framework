using System;

namespace Laurent.Lee.CLB.OpenAPI.RenRen
{
    /// <summary>
    /// 访问令牌+用户身份的标识，用于保存
    /// </summary>
    public struct refreshToken
    {
        /// <summary>
        /// 访问令牌
        /// </summary>
        public string access_token;
        /// <summary>
        /// 刷新访问令牌
        /// </summary>
        public string refresh_token;
        /// <summary>
        /// 用户ID
        /// </summary>
        public int id;
    }
}
