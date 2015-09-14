using System;
using System.Collections.Generic;
using System.Threading;
using Laurent.Lee.CLB.Code.CSharp;

namespace Laurent.Lee.CLB.Sql.Cache.Whole.Events
{
    /// <summary>
    /// 关键字整表缓存(反射模式)
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    public sealed class TmphPrimaryKey<TValueType, TModelType, TKeyType> : TmphKey<TValueType, TModelType, TKeyType>
        where TValueType : class, TModelType
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        /// 关键字整表缓存
        /// </summary>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="getKey">键值获取器</param>
        /// <param name="group">数据分组</param>
        public TmphPrimaryKey(Laurent.Lee.CLB.Emit.TmphSqlTable<TValueType, TModelType, TKeyType> sqlTool, int group = 0)
            : base(sqlTool, sqlTool.GetPrimaryKey, group)
        { }
    }
}
