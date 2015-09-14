using System;
using System.Collections.Generic;

namespace Laurent.Lee.CLB.Sql.Cache
{
    /// <summary>
    /// 自定义缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    public interface TmphICustom<TValueType>
    {
        /// <summary>
        /// 添加缓存数据
        /// </summary>
        /// <param name="value">缓存数据</param>
        /// <returns>是否添加数据</returns>
        bool Add(TValueType value);
        /// <summary>
        /// 更新缓存数据
        /// </summary>
        /// <param name="value">缓存数据</param>
        /// <param name="oldValue">旧数据</param>
        /// <returns>添加数据返回正数，删除数据返回负数，没有变化返回0</returns>
        int Update(TValueType value, TValueType oldValue);
        /// <summary>
        /// 删除缓存数据
        /// </summary>
        /// <param name="value">缓存数据</param>
        /// <returns>是否存在被删除数据</returns>
        bool Remove(TValueType value);
        /// <summary>
        /// 所有缓存数据
        /// </summary>
        IEnumerable<TValueType> Values { get; }
    }
}
