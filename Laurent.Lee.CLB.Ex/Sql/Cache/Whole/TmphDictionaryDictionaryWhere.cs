using System;
using Laurent.Lee.CLB.Code.CSharp;
using System.Collections.Generic;
using System.Threading;

namespace Laurent.Lee.CLB.Sql.Cache.Whole
{
    /// <summary>
    /// 分组字典缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TmphGroupKeyType">分组关键字类型</typeparam>
    /// <typeparam name="TKeyType">字典关键字类型</typeparam>
    public sealed class TmphDictionaryDictionaryWhere<TValueType, TModelType, TmphGroupKeyType, TKeyType> : TmphDictionaryDictionary<TValueType, TModelType, TmphGroupKeyType, TKeyType>
        where TValueType : class, TModelType
        where TModelType : class
        where TmphGroupKeyType : IEquatable<TmphGroupKeyType>
        where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        /// 数据匹配器
        /// </summary>
        private Func<TValueType, bool> isValue;
        /// <summary>
        /// 分组字典缓存
        /// </summary>
        /// <param name="cache">整表缓存</param>
        /// <param name="getGroupKey">分组关键字获取器</param>
        /// <param name="getKey">字典关键字获取器</param>
        /// <param name="isValue">数据匹配器</param>
        public TmphDictionaryDictionaryWhere(Events.TmphCache<TValueType, TModelType> cache
            , Func<TValueType, TmphGroupKeyType> getGroupKey, Func<TValueType, TKeyType> getKey, Func<TValueType, bool> isValue)
            : base(cache, getGroupKey, getKey, false)
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
            groups = TmphDictionary<TmphGroupKeyType>.Create<Dictionary<TKeyType, TValueType>>();
            foreach (TValueType value in cache.Values) onInserted(value);
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
                if (isValue(oldValue))
                {
                    base.onUpdated(value, oldValue);
                }
                else base.onInserted(value);
            }
            else if (isValue(oldValue)) base.onDeleted(oldValue);
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