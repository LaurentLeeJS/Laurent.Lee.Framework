using System;
using Laurent.Lee.CLB.Code.CSharp;

namespace Laurent.Lee.CLB.Sql.Cache.Part.Events
{
    /// <summary>
    /// 自增id标识缓存计数器(反射模式)
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    public sealed class TmphIdentityCounter<TValueType, TModelType> : TmphCounter<TValueType, TModelType, long>
        where TValueType : class, TModelType
        where TModelType : class
    {
        /// <summary>
        /// 自增id标识缓存计数器
        /// </summary>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="getIdentity">自增id获取器</param>
        /// <param name="group">数据分组</param>
        public TmphIdentityCounter
            (Laurent.Lee.CLB.Emit.TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, int group = 0)
            : base(sqlTool, group, Laurent.Lee.CLB.Emit.TmphSqlModel<TModelType>.GetIdentity)
        {
        }
    }
    /// <summary>
    /// 自增id标识缓存计数器(反射模式)
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    public sealed class identityCounter32<TValueType, TModelType> : TmphCounter<TValueType, TModelType, int>
        where TValueType : class, TModelType
        where TModelType : class
    {
        /// <summary>
        /// 自增id标识缓存计数器
        /// </summary>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="getIdentity">自增id获取器</param>
        /// <param name="group">数据分组</param>
        public identityCounter32
            (Laurent.Lee.CLB.Emit.TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, int group = 0)
            : base(sqlTool, group, Laurent.Lee.CLB.Emit.TmphSqlModel<TModelType>.GetIdentity32)
        {
        }
    }
}