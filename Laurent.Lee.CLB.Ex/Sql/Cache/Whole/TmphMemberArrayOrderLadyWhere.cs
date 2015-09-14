using System;
using Laurent.Lee.CLB.Code.CSharp;
using System.Threading;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Laurent.Lee.CLB.Sql.Cache.Whole
{
    /// <summary>
    /// 分组列表 延时排序缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TKeyType">分组字典关键字类型</typeparam>
    public class TmphMemberArrayOrderLadyWhere<TValueType, TModelType, TKeyType, TTargetType>
        : TmphMemberArrayOrderLady<TValueType, TModelType, TKeyType, TTargetType>
        where TValueType : class, TModelType
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
        where TTargetType : class
    {
        /// <summary>
        /// 缓存值判定
        /// </summary>
        private Func<TValueType, bool> isValue;
        /// <summary>
        /// 分组列表 延时排序缓存
        /// </summary>
        /// <param name="cache">整表缓存</param>
        /// <param name="getKey">分组字典关键字获取器</param>
        /// <param name="sorter">排序器</param>
        /// <param name="isValue">缓存值判定</param>
        public TmphMemberArrayOrderLadyWhere
            (Events.TmphCache<TValueType, TModelType> cache
            , Func<TModelType, TKeyType> getKey, Func<TKeyType, TTargetType> getValue, Expression<Func<TTargetType, TmphMemberLadyOrderArray<TValueType>>> member, Func<TmphSubArray<TValueType>, TmphSubArray<TValueType>> sorter
            , Func<TValueType, bool> isValue)
            : base(cache, getKey, getValue, member, sorter, false)
        {
            if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            this.isValue = isValue;

            cache.OnReset += reset;
            cache.OnInserted += onInserted;
            cache.OnUpdated += onUpdated;
            cache.OnDeleted += onDeleted;
            resetLock();
        }
        /// <summary>
        /// 重新加载数据
        /// </summary>
        protected override void reset()
        {
            foreach (TValueType value in cache.Values) if (isValue(value)) onInserted(value);
        }
         /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value">数据对象</param>
        private new void onInserted(TValueType value)
        {
            if (isValue(value)) base.onInserted(value);
        }
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="value">更新后的数据</param>
        /// <param name="oldValue">更新前的数据</param>
        private new void onUpdated(TValueType value, TValueType oldValue)
        {
            if (isValue(value))
            {
                if (isValue(oldValue)) base.onUpdated(value, oldValue);
                else base.onInserted(value);
            }
            else if (isValue(oldValue)) onDeleted(value, getKey(oldValue));
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="value">被删除的数据</param>
        private new void onDeleted(TValueType value)
        {
            if (isValue(value)) base.onDeleted(value);
        }
    }
}