/*
-------------------------------------------------- -----------------------------------------
The frame content is protected by copyright law. In order to facilitate individual learning,
allows to download the program source information, but does not allow individuals or a third
party for profit, the commercial use of the source information. Without consent,
does not allow any form (even if partial, or modified) database storage,
copy the source of information. If the source content provided by third parties,
which corresponds to the third party content is also protected by copyright.

If you are found to have infringed copyright behavior, please give me a hint. THX!

Here in particular it emphasized that the third party is not allowed to contact addresses
published in this "version copyright statement" to send advertising material.
I will take legal means to resist sending spam.
-------------------------------------------------- ----------------------------------------
The framework under the GNU agreement, Detail View GNU License.
If you think about this item affection join the development team,
Please contact me: LaurentLeeJS@gmail.com
-------------------------------------------------- ----------------------------------------
Laurent.Lee.Framework Coded by Laurent Lee
*/

using System;
using System.Collections.Generic;
using System.Threading;

namespace Laurent.Lee.CLB.Sql.Cache.Whole
{
    /// <summary>
    /// 字典+搜索树缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TKeyType">分组关键字类型</typeparam>
    /// <typeparam name="TSortType">排序关键字类型</typeparam>
    public class TmphDictionarySearchTree<TValueType, TModelType, TKeyType, TSortType>
        where TValueType : class, TModelType
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
        where TSortType : IComparable<TSortType>
    {
        /// <summary>
        /// 整表缓存
        /// </summary>
        protected Events.TmphCache<TValueType, TModelType> cache;

        /// <summary>
        /// 分组关键字获取器
        /// </summary>
        protected Func<TValueType, TKeyType> getKey;

        /// <summary>
        /// 排序关键字获取器
        /// </summary>
        protected Func<TValueType, TSortType> getSort;

        /// <summary>
        /// 字典+搜索树缓存
        /// </summary>
        protected Dictionary<TKeyType, Laurent.Lee.CLB.TmphSearchTree<TSortType, TValueType>> groups;

        /// <summary>
        /// 分组列表缓存
        /// </summary>
        /// <param name="cache">整表缓存</param>
        /// <param name="getKey">分组关键字获取器</param>
        /// <param name="getSort">排序关键字获取器</param>
        public TmphDictionarySearchTree
            (Events.TmphCache<TValueType, TModelType> cache, Func<TValueType, TKeyType> getKey, Func<TValueType, TSortType> getSort)
        {
            if (cache == null || getKey == null || getSort == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            this.cache = cache;
            this.getKey = getKey;
            this.getSort = getSort;

            cache.OnReset += reset;
            cache.OnInserted += onInserted;
            cache.OnUpdated += onUpdated;
            cache.OnDeleted += onDeleted;
            resetLock();
        }

        /// <summary>
        /// 重新加载数据
        /// </summary>
        private void resetLock()
        {
            Monitor.Enter(cache.SqlTool.Lock);
            try
            {
                reset();
            }
            finally { Monitor.Exit(cache.SqlTool.Lock); }
        }

        /// <summary>
        /// 重新加载数据
        /// </summary>
        private void reset()
        {
            Dictionary<TKeyType, Laurent.Lee.CLB.TmphSearchTree<TSortType, TValueType>> newValues = TmphDictionary<TKeyType>.Create<Laurent.Lee.CLB.TmphSearchTree<TSortType, TValueType>>();
            Laurent.Lee.CLB.TmphSearchTree<TSortType, TValueType> tree;
            foreach (TValueType value in cache.Values)
            {
                TKeyType key = getKey(value);
                if (!newValues.TryGetValue(key, out tree)) newValues.Add(key, tree = new Laurent.Lee.CLB.TmphSearchTree<TSortType, TValueType>());
                tree.Add(getSort(value), value);
            }
            groups = newValues;
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value">数据对象</param>
        protected void onInserted(TValueType value)
        {
            onInserted(value, getKey(value));
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value">数据对象</param>
        /// <param name="key">数据对象的关键字</param>
        protected void onInserted(TValueType value, TKeyType key)
        {
            Laurent.Lee.CLB.TmphSearchTree<TSortType, TValueType> tree;
            if (!groups.TryGetValue(key, out tree)) groups.Add(key, tree = new Laurent.Lee.CLB.TmphSearchTree<TSortType, TValueType>());
            tree.Add(getSort(value), value);
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="value">更新后的数据</param>
        /// <param name="oldValue">更新前的数据</param>
        protected void onUpdated(TValueType value, TValueType oldValue)
        {
            TKeyType key = getKey(value), oldKey = getKey(oldValue);
            if (key.Equals(oldKey))
            {
                TSortType sortKey = getSort(value), oldSortKey = getSort(oldValue);
                if (!key.Equals(oldKey))
                {
                    Laurent.Lee.CLB.TmphSearchTree<TSortType, TValueType> tree;
                    if (groups.TryGetValue(key, out tree) && tree.Remove(oldSortKey))
                    {
                        tree.Add(sortKey, value);
                    }
                    else TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", false, true);
                }
            }
            else
            {
                onInserted(value, key);
                onDeleted(oldValue, oldKey);
            }
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="value">被删除的数据</param>
        /// <param name="key">分组关键字</param>
        protected void onDeleted(TValueType value, TKeyType key)
        {
            Laurent.Lee.CLB.TmphSearchTree<TSortType, TValueType> tree;
            if (groups.TryGetValue(key, out tree) && tree.Remove(getSort(value)))
            {
                if (tree.Count == 0) groups.Remove(key);
            }
            else TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", false, true);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="value">被删除的数据</param>
        protected void onDeleted(TValueType value)
        {
            onDeleted(value, getKey(value));
        }

        /// <summary>
        /// 获取逆序数据集合
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>逆序数据集合</returns>
        public TValueType[] GetArrayDesc(TKeyType key)
        {
            TValueType[] values = null;
            Laurent.Lee.CLB.TmphSearchTree<TSortType, TValueType> tree;
            Monitor.Enter(cache.SqlTool.Lock);
            try
            {
                if (groups.TryGetValue(key, out tree)) values = tree.GetArray();
            }
            finally { Monitor.Exit(cache.SqlTool.Lock); }
            return values.reverse();
        }

        /// <summary>
        /// 获取逆序分页数据集合
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="currentPage">分页页号</param>
        /// <param name="count">数据总数</param>
        /// <returns>逆序分页数据集合</returns>
        public TValueType[] GetPageDesc(TKeyType key, int pageSize, int currentPage, out int count)
        {
            TValueType[] values = null;
            count = 0;
            Laurent.Lee.CLB.TmphSearchTree<TSortType, TValueType> tree;
            Monitor.Enter(cache.SqlTool.Lock);
            try
            {
                if (groups.TryGetValue(key, out tree))
                {
                    TmphArray.TmphPage page = new TmphArray.TmphPage(count = tree.Count, pageSize, currentPage);
                    values = tree.GetRange(count - page.SkipCount - page.CurrentPageSize, page.CurrentPageSize);
                }
            }
            finally { Monitor.Exit(cache.SqlTool.Lock); }
            return values.reverse();
        }
    }
}