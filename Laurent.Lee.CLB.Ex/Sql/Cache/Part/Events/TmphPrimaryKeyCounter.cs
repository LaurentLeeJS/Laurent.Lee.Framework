using System;
using Laurent.Lee.CLB.Code.CSharp;

namespace Laurent.Lee.CLB.Sql.Cache.Part.Events
{
    /// <summary>
    /// 关键字缓存计数器
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    public sealed class TmphPrimaryKeyCounter<TValueType, TModelType, TKeyType> : TmphCounter<TValueType, TModelType, TKeyType>
        where TValueType : class, TModelType
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        /// 关键字缓存计数器
        /// </summary>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="group">数据分组</param>
        public TmphPrimaryKeyCounter
            (Laurent.Lee.CLB.Emit.TmphSqlTable<TValueType, TModelType, TKeyType> sqlTool, int group = 0)
            : base(sqlTool, group, sqlTool.GetPrimaryKey)
        {
        }
    }
}
