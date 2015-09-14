using System;
using Laurent.Lee.CLB;

namespace Laurent.Lee.CLB.OpenAPI.WeiBo
{
    /// <summary>
    /// 访问令牌
    /// </summary>
    public sealed class TmphToken : TmphIValue
    {
        /// <summary>
        /// 访问令牌
        /// </summary>
        public string access_token;
        /// <summary>
        /// 当前授权用户的UID
        /// </summary>
        public string uid;
        /// <summary>
        /// 生命周期，单位是秒数
        /// </summary>
        public int expires_in;
        /// <summary>
        /// 数据是否有效
        /// </summary>
        public bool IsValue
        {
            get { return access_token.Length != 0 && uid.Length != 0 && expires_in != 0; }
        }
        /// <summary>
        /// 提示信息
        /// </summary>
        public string Message
        {
            get { return null; }
        }
    }
}
