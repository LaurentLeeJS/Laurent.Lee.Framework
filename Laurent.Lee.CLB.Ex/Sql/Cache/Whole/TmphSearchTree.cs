using System;
using Laurent.Lee.CLB.Code.CSharp;
using System.Threading;

namespace Laurent.Lee.CLB.Sql.Cache.Whole
{
    /// <summary>
    /// 搜索树缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TSortType">排序关键字类型</typeparam>
    public class TmphSearchTree<TValueType, TModelType, TSortType>
        where TValueType : class, TModelType
        where TModelType : class
        where TSortType : IComparable<TSortType>
    {
        /// <summary>
        /// 整表缓存
        /// </summary>
        protected Events.TmphCache<TValueType, TModelType> cache;
        /// <summary>
        /// 排序关键字获取器
        /// </summary>
        protected Func<TValueType, TSortType> getSort;
        /// <summary>
        /// 搜索树缓存
        /// </summary>
        protected Laurent.Lee.CLB.TmphSearchTree<TSortType, TValueType> tree;
        /// <summary>
        /// 获取缓存数量
        /// </summary>
        public int Count
        {
            get { return tree.Count; }
        }
        /// <summary>
        /// 分组列表缓存
        /// </summary>
        /// <param name="cache">整表缓存</param>
        /// <param name="getSort">排序关键字获取器</param>
        /// <param name="isReset">是否绑定事件与重置数据</param>
        public TmphSearchTree(Events.TmphCache<TValueType, TModelType> cache, Func<TValueType, TSortType> getSort, bool isReset = true)
        {
            if (cache == null || getSort == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            this.cache = cache;
            this.getSort = getSort;

            if (isReset)
            {
                cache.OnReset += reset;
                cache.OnInserted += onInserted;
                cache.OnUpdated += onUpdated;
                cache.OnDeleted += onDeleted;
                resetLock();
            }
        }
        /// <summary>
        /// 重新加载数据
        /// </summary>
        protected void resetLock()
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
        protected virtual void reset()
        {
            Laurent.Lee.CLB.TmphSearchTree<TSortType, TValueType> newTree = new Laurent.Lee.CLB.TmphSearchTree<TSortType, TValueType>();
            foreach (TValueType value in cache.Values)
            {
                newTree.Add(getSort(value), value);
            }
            tree = newTree;
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value">数据对象</param>
        protected virtual void onInserted(TValueType value)
        {
            tree.Add(getSort(value), value);
        }
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="value">更新后的数据</param>
        /// <param name="oldValue">更新前的数据</param>
        protected virtual void onUpdated(TValueType value, TValueType oldValue)
        {
            onDeleted(oldValue);
            onInserted(value);
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="value">被删除的数据</param>
        protected virtual void onDeleted(TValueType value)
        {
            if (!tree.Remove(getSort(value)))
            {
                TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", false, true);
            }
        }
        /// <summary>
        /// 获取数据集合
        /// </summary>
        /// <returns>数据集合</returns>
        public TValueType[] GetArray()
        {
            TValueType[] values = null;
            Monitor.Enter(cache.SqlTool.Lock);
            try
            {
                values = tree.GetArray();
            }
            finally { Monitor.Exit(cache.SqlTool.Lock); }
            return values;
        }
        /// <summary>
        /// 获取逆序数据集合
        /// </summary>
        /// <returns>逆序数据集合</returns>
        public TValueType[] GetArrayDesc()
        {
            TValueType[] values = null;
            Monitor.Enter(cache.SqlTool.Lock);
            try
            {
                values = tree.GetArray();
            }
            finally { Monitor.Exit(cache.SqlTool.Lock); }
            return values.reverse();
        }
        /// <summary>
        /// 获取分页数据集合
        /// </summary>
        /// <param name="pageSize">分页大小</param>
        /// <param name="currentPage">分页页号</param>
        /// <param name="count">数据总数</param>
        /// <returns>分页数据集合</returns>
        public TValueType[] GetPage(int pageSize, int currentPage, out int count)
        {
            TValueType[] values = null;
            count = 0;
            Monitor.Enter(cache.SqlTool.Lock);
            try
            {
                TmphArray.TmphPage page = new TmphArray.TmphPage(count = tree.Count, pageSize, currentPage);
                values = tree.GetRange(page.SkipCount, page.CurrentPageSize);
            }
            finally { Monitor.Exit(cache.SqlTool.Lock); }
            return values;
        }
        /// <summary>
        /// 获取逆序分页数据集合
        /// </summary>
        /// <param name="pageSize">分页大小</param>
        /// <param name="currentPage">分页页号</param>
        /// <param name="count">数据总数</param>
        /// <returns>逆序分页数据集合</returns>
        public TValueType[] GetPageDesc(int pageSize, int currentPage, out int count)
        {
            TValueType[] values = null;
            count = 0;
            Monitor.Enter(cache.SqlTool.Lock);
            try
            {
                TmphArray.TmphPage page = new TmphArray.TmphPage(count = tree.Count, pageSize, currentPage);
                values = tree.GetRange(count - page.SkipCount - page.CurrentPageSize, page.CurrentPageSize);
            }
            finally { Monitor.Exit(cache.SqlTool.Lock); }
            return values.reverse();
        }
        /// <summary>
        /// 获取上一个数据
        /// </summary>
        /// <param name="key">排序关键字</param>
        /// <returns>上一个数据,失败返回null</returns>
        public TValueType GetPrevious(TSortType key)
        {
            Monitor.Enter(cache.SqlTool.Lock);
            try
            {
                int index = tree.IndexOf(key);
                if (index > 0) return tree.GetIndex(index - 1);
            }
            finally { Monitor.Exit(cache.SqlTool.Lock); }
            return null;
        }
        /// <summary>
        /// 获取上一个数据
        /// </summary>
        /// <param name="key">排序关键字</param>
        /// <returns>上一个数据,失败返回null</returns>
        public TValueType GetNext(TSortType key)
        {
            Monitor.Enter(cache.SqlTool.Lock);
            try
            {
                int index = tree.IndexOf(key) + 1;
                if (index != 0 && index < tree.Count) return tree.GetIndex(index + 1);
            }
            finally { Monitor.Exit(cache.SqlTool.Lock); }
            return null;
        }
        /// <summary>
        /// 根据关键字比它小的节点数量
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>节点数量</returns>
        public int CountLess(TSortType key)
        {
            return tree.CountLess(key);
        }
        /// <summary>
        /// 根据关键字比它大的节点数量
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>节点数量</returns>
        public int CountThan(TSortType key)
        {
            return tree.CountThan(key);
        }
    }
}
