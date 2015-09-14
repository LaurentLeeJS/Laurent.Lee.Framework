using System;
using Laurent.Lee.CLB.Code.CSharp;
using System.Threading;

namespace Laurent.Lee.CLB.Sql.Cache.Whole
{
    /// <summary>
    /// 搜索树缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TSortType">排序关键字类型</typeparam>
    public class TmphSearchTreeWhere<TValueType, TModelType, TSortType> : TmphSearchTree<TValueType, TModelType, TSortType>
        where TValueType : class, TModelType
        where TModelType : class
        where TSortType : IComparable<TSortType>
    {
        /// <summary>
        /// 数据匹配器
        /// </summary>
        private Func<TValueType, bool> isValue;
        /// <summary>
        /// 分组列表缓存
        /// </summary>
        /// <param name="cache">整表缓存</param>
        /// <param name="getSort">排序关键字获取器</param>
        /// <param name="isValue">数据匹配器</param>
        public TmphSearchTreeWhere
            (Events.TmphCache<TValueType, TModelType> cache, Func<TValueType, TSortType> getSort, Func<TValueType, bool> isValue)
            : base(cache, getSort, false)
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
            Laurent.Lee.CLB.TmphSearchTree<TSortType, TValueType> newTree = new Laurent.Lee.CLB.TmphSearchTree<TSortType, TValueType>();
            foreach (TValueType value in cache.Values)
            {
                if (isValue(value)) newTree.Add(getSort(value), value);
            }
            tree = newTree;
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
            if (isValue(oldValue)) onDeleted(oldValue);
            if (isValue(value)) onInserted(value);
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
