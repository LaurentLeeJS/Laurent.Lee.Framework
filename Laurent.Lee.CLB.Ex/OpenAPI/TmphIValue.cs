using System;

namespace Laurent.Lee.CLB.OpenAPI
{
    /// <summary>
    /// 数据是否有效
    /// </summary>
    public interface TmphIValue
    {
        /// <summary>
        /// 数据是否有效
        /// </summary>
        bool IsValue { get; }
        /// <summary>
        /// 提示信息
        /// </summary>
        string Message { get; }
    }
}
