using System;

namespace Laurent.Lee.CLB.OpenAPI.RenRen
{
    /// <summary>
    /// 用户头像
    /// </summary>
    public struct avatar
    {
        /// <summary>
        /// 头像类型
        /// </summary>
        public enum sizeType
        {
            /// <summary>
            /// 大头像
            /// </summary>
            large,
            /// <summary>
            /// 主头像
            /// </summary>
            main,
            /// <summary>
            /// 普通头像
            /// </summary>
            avatar,
            /// <summary>
            /// 小头像
            /// </summary>
            tiny,
        }
        /// <summary>
        /// 头像类型
        /// </summary>
        public sizeType type;
        /// <summary>
        /// 头像地址
        /// </summary>
        public string url;
    }
}
