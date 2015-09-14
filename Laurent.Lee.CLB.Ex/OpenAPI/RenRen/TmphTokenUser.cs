using System;

namespace Laurent.Lee.CLB.OpenAPI.RenRen
{
    /// <summary>
    /// 访问令牌用户
    /// </summary>
    public struct tokenUser
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int id;
        /// <summary>
        /// 用户名称
        /// </summary>
        public string name;
        /// <summary>
        /// 头像集合
        /// </summary>
        public avatar[] avatar;
    }
}
