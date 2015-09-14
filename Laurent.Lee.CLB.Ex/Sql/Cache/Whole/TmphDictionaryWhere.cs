using System;
using Laurent.Lee.CLB.Code.CSharp;
using System.Threading;
using System.Collections.Generic;

namespace Laurent.Lee.CLB.Sql.Cache.Whole
{
    /// <summary>
    /// 字典缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    public class TmphDictionaryWhere<TValueType, TModelType, TKeyType> : TmphDictionary<TValueType, TModelType, TKeyType>
        where TValueType : class, TModelType
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        /// 数据匹配器
        /// </summary>
        private Func<TValueType, bool> isValue;
        /// <summary>
        /// 字典缓存
        /// </summary>
        /// <param name="cache">整表缓存</param>
        /// <param name="getKey">分组字典关键字获取器</param>
        /// <param name="isValue">数据匹配器</param>
        public TmphDictionaryWhere(Events.TmphCache<TValueType, TModelType> cache, Func<TValueType, TKeyType> getKey, Func<TValueType, bool> isValue)
            : base(cache, getKey, false)
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
            Dictionary<TKeyType, TValueType> newValues = TmphDictionary<TKeyType>.Create<TValueType>();
            foreach (TValueType value in cache.Values)
            {
                if (isValue(value)) newValues.Add(getKey(value), value);
            }
            values = newValues;
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value">数据对象</param>
        protected override void onInserted(TValueType value)
        {
            if (isValue(value)) base.onInserted(value);
        }
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="value">更新后的数据</param>
        /// <param name="oldValue">更新前的数据</param>
        protected override void onUpdated(TValueType value, TValueType oldValue)
        {
            if (isValue(oldValue))
            {
                if (isValue(value)) base.onUpdated(value, oldValue);
                else onDeleted(oldValue);
            }
            else onInserted(value);
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="value">被删除的数据</param>
        protected override void onDeleted(TValueType value)
        {
            if (isValue(value)) base.onDeleted(value);
        }
    }
}
