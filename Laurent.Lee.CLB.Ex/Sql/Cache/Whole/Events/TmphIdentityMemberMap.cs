using System;
using Laurent.Lee.CLB.Code.CSharp;

namespace Laurent.Lee.CLB.Sql.Cache.Whole.Events
{
    /// <summary>
    /// 自增ID整表缓存
    /// </summary>
    /// <typeparam name="TValueType">表格类型</typeparam>
    /// <typeparam name="TModelType">模型类型</typeparam>
    public abstract class TmphIdentityMemberMap<TValueType, TModelType> : TmphCache<TValueType, TModelType>
        where TValueType : class, TModelType
        where TModelType : class
    {
        /// <summary>
        /// 自增ID获取器
        /// </summary>
        protected Func<TModelType, int> getIdentity;
        /// <summary>
        /// 基础ID
        /// </summary>
        protected int baseIdentity;
        /// <summary>
        /// 缓存数据数量
        /// </summary>
        public int Count { get; protected set; }
        /// <summary>
        /// SQL操作缓存
        /// </summary>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="baseIdentity">基础ID</param>
        /// <param name="group">数据分组</param>
        protected TmphIdentityMemberMap(Laurent.Lee.CLB.Emit.TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, int group, int baseIdentity)
            : base(sqlTool, group)
        {
            getIdentity = Laurent.Lee.CLB.Emit.TmphSqlModel<TModelType>.IdentityGetter(this.baseIdentity = baseIdentity);
        }
    }
}
