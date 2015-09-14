using System;
using Laurent.Lee.CLB.Code.CSharp;
using System.Collections.Generic;

namespace Laurent.Lee.CLB.Sql.Cache.Whole.Events
{
    /// <summary>
    /// 事件缓存
    /// </summary>
    /// <typeparam name="TValueType">表格类型</typeparam>
    /// <typeparam name="TModelType">模型类型</typeparam>
    public abstract class TmphCache<TValueType, TModelType> : TmphCopy<TValueType, TModelType>
        where TValueType : class, TModelType
        where TModelType : class
    {
        /// <summary>
        /// 数据集合
        /// </summary>
        public abstract IEnumerable<TValueType> Values { get; }
        /// <summary>
        /// 添加记录事件
        /// </summary>
        public event Action<TValueType> OnInserted;
        /// <summary>
        /// 添加记录
        /// </summary>
        /// <param name="value">新添加的对象</param>
        protected void callOnInserted(TValueType value)
        {
            if (OnInserted != null) OnInserted(value);
        }
        /// <summary>
        /// 更新记录事件
        /// </summary>
        public event Action<TValueType, TValueType> OnUpdated;
        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="value">更新后的对象</param>
        /// <param name="oldValue">更新前的对象</param>
        protected void callOnUpdated(TValueType value, TValueType oldValue)
        {
            if (OnUpdated != null) OnUpdated(value, oldValue);
        }
        /// <summary>
        /// 删除记录事件
        /// </summary>
        public event Action<TValueType> OnDeleted;
        /// <summary>
        /// 删除记录
        /// </summary>
        /// <param name="value">被删除的对象</param>
        protected void callOnDeleted(TValueType value)
        {
            if (OnDeleted != null) OnDeleted(value);
        }
        /// <summary>
        /// SQL操作缓存
        /// </summary>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="group">数据分组</param>
        protected TmphCache(Laurent.Lee.CLB.Emit.TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, int group)
            : base(sqlTool, group)
        {
        }
    }
}
