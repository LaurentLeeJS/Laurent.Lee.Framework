using System;
using Laurent.Lee.CLB.Code.CSharp;
using System.Collections.Generic;
using System.Threading;

namespace Laurent.Lee.CLB.Sql.Cache.Whole
{
    /// <summary>
    /// 分组列表缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TKeyType">分组字典关键字类型</typeparam>
    public class TmphDictionaryList<TValueType, TModelType, TKeyType>
        where TValueType : class, TModelType
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        /// 整表缓存
        /// </summary>
        protected Events.TmphCache<TValueType, TModelType> cache;
        /// <summary>
        /// 分组字典关键字获取器
        /// </summary>
        protected Func<TValueType, TKeyType> getKey;
        /// <summary>
        /// 分组数据
        /// </summary>
        protected Dictionary<TKeyType, TmphKeyValue<TKeyType, TmphList<TValueType>>> groups;
        /// <summary>
        /// 关键字版本号
        /// </summary>
        protected int keyVersion;
        /// <summary>
        /// 移除数据并使用最后一个数据移动到当前位置
        /// </summary>
        protected bool isRemoveEnd;
        /// <summary>
        /// 分组列表缓存
        /// </summary>
        /// <param name="cache">整表缓存</param>
        /// <param name="getKey">分组字典关键字获取器</param>
        /// <param name="isRemoveEnd">移除数据并使用最后一个数据移动到当前位置</param>
        /// <param name="isReset">是否绑定事件并重置数据</param>
        public TmphDictionaryList(Events.TmphCache<TValueType, TModelType> cache, Func<TValueType, TKeyType> getKey, bool isRemoveEnd, bool isReset)
        {
            if (cache == null || getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            this.cache = cache;
            this.getKey = getKey;
            this.isRemoveEnd = isRemoveEnd;

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
            TmphInsert insert = new TmphInsert { Groups = TmphDictionary<TKeyType>.Create<TmphKeyValue<TKeyType, TmphList<TValueType>>>(), GetKey = getKey };
            foreach (TValueType value in cache.Values) insert.OnInserted(value);
            this.groups = insert.Groups;
            ++keyVersion;
        }
        /// <summary>
        /// 数据添加器
        /// </summary>
        protected struct TmphInsert
        {
            /// <summary>
            /// 分组数据
            /// </summary>
            public Dictionary<TKeyType, TmphKeyValue<TKeyType, TmphList<TValueType>>> Groups;
            /// <summary>
            /// 分组字典关键字获取器
            /// </summary>
            public Func<TValueType, TKeyType> GetKey;
            /// <summary>
            /// 添加数据
            /// </summary>
            /// <param name="value">数据对象</param>
            public void OnInserted(TValueType value)
            {
                TmphKeyValue<TKeyType, TmphList<TValueType>> list;
                TKeyType key = GetKey(value);
                if (!Groups.TryGetValue(key, out list)) Groups.Add(key, list = new TmphKeyValue<TKeyType, TmphList<TValueType>>(key, new TmphList<TValueType>()));
                list.Value.Add(value);
            }
            /// <summary>
            /// 添加数据
            /// </summary>
            /// <param name="value">数据对象</param>
            /// <param name="cache">缓存对象</param>
            public void OnInserted(TValueType value, TmphDictionaryList<TValueType, TModelType, TKeyType> cache)
            {
                TmphKeyValue<TKeyType, TmphList<TValueType>> list;
                TKeyType key = GetKey(value);
                if (!Groups.TryGetValue(key, out list))
                {
                    Groups.Add(key, list = new TmphKeyValue<TKeyType, TmphList<TValueType>>(key, new TmphList<TValueType>()));
                    ++cache.keyVersion;
                }
                list.Value.Add(value);
            }
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value">数据对象</param>
        protected void onInserted(TValueType value)
        {
            new TmphInsert { Groups = groups, GetKey = getKey }.OnInserted(value, this);
        }
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="value">更新后的数据</param>
        /// <param name="oldValue">更新前的数据</param>
        protected void onUpdated(TValueType value, TValueType oldValue)
        {
            TKeyType oldKey = getKey(oldValue);
            if (!getKey(value).Equals(oldKey))
            {
                onInserted(value);
                onDeleted(value, oldKey);
            }
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="value">被删除的数据</param>
        /// <param name="key">被删除的数据关键字</param>
        protected void onDeleted(TValueType value, TKeyType key)
        {
            TmphKeyValue<TKeyType, TmphList<TValueType>> keyList;
            if (groups.TryGetValue(key, out keyList))
            {
                TmphList<TValueType> list = keyList.Value;
                int index = Array.LastIndexOf(list.Unsafer.Array, value, list.Count - 1);
                if (index != -1)
                {
                    if (list.Count != 1)
                    {
                        if (isRemoveEnd) list.RemoveAtEnd(index);
                        else list.RemoveAt(index);
                    }
                    else groups.Remove(key);
                    return;
                }
            }
            TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", false, true);
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
        /// 获取匹配数据数量
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>匹配数据数量</returns>
        public int Count(TKeyType key)
        {
            TmphKeyValue<TKeyType, TmphList<TValueType>> list;
            return groups.TryGetValue(key, out list) ? list.Value.Count : 0;
        }
        /// <summary>
        /// 获取匹配数据数量
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="isValue">数据匹配器,禁止锁操作</param>
        /// <returns>匹配数据数量</returns>
        public int CountLock(TKeyType key, Func<TValueType, bool> isValue)
        {
            TmphKeyValue<TKeyType, TmphList<TValueType>> list;
            int version = keyVersion;
            if (groups.TryGetValue(key, out list) && list.Key.Equals(key))
            {
                Monitor.Enter(cache.SqlTool.Lock);
                try
                {
                    return list.Value.GetCount(isValue);
                }
                finally { Monitor.Exit(cache.SqlTool.Lock); }
            }
            else if (version != keyVersion)
            {
                Monitor.Enter(cache.SqlTool.Lock);
                try
                {
                    if (groups.TryGetValue(key, out list)) return list.Value.GetCount(isValue);
                }
                finally { Monitor.Exit(cache.SqlTool.Lock); }
            }
            return 0;
        }
        /// <summary>
        /// 获取第一个数据
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>数据集合</returns>
        public TValueType First(TKeyType key, TValueType nullValue = null)
        {
            TmphKeyValue<TKeyType, TmphList<TValueType>> list;
            int version = keyVersion;
            if (groups.TryGetValue(key, out list) && list.Key.Equals(key))
            {
                return list.Value.Count == 0 ? nullValue : list.Value.Unsafer.Array[0];
            }
            else if (version != keyVersion)
            {
                Monitor.Enter(cache.SqlTool.Lock);
                try
                {
                    if (groups.TryGetValue(key, out list))
                    {
                        return list.Value.Count == 0 ? nullValue : list.Value.Unsafer.Array[0];
                    }
                }
                finally { Monitor.Exit(cache.SqlTool.Lock); }
            }
            return nullValue;
        }
        /// <summary>
        /// 获取第一个匹配数据
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="isValue">数据匹配器,禁止锁操作</param>
        /// <returns>第一个匹配数据</returns>
        public TValueType FirstOrDefaultLock(TKeyType key, Func<TValueType, bool> isValue)
        {
            TmphKeyValue<TKeyType, TmphList<TValueType>> list;
            int version = keyVersion;
            if (groups.TryGetValue(key, out list) && list.Key.Equals(key))
            {
                Monitor.Enter(cache.SqlTool.Lock);
                try
                {
                    return list.Value.FirstOrDefault(isValue);
                }
                finally { Monitor.Exit(cache.SqlTool.Lock); }
            }
            else if (version != keyVersion)
            {
                Monitor.Enter(cache.SqlTool.Lock);
                try
                {
                    if (groups.TryGetValue(key, out list)) return list.Value.FirstOrDefault(isValue);
                }
                finally { Monitor.Exit(cache.SqlTool.Lock); }
            }
            return null;
        }
        /// <summary>
        /// 获取数据集合
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>数据集合</returns>
        public TValueType[] GetArray(TKeyType key)
        {
            TmphKeyValue<TKeyType, TmphList<TValueType>> list;
            int version = keyVersion;
            if (!groups.TryGetValue(key, out list) || !list.Key.Equals(key) && version != keyVersion)
            {
                Monitor.Enter(cache.SqlTool.Lock);
                try
                {
                    groups.TryGetValue(key, out list);
                }
                finally { Monitor.Exit(cache.SqlTool.Lock); }
            }
            return list.Value.getArray();
        }
        /// <summary>
        /// 获取匹配数据集合
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="isValue">数据匹配器,禁止锁操作</param>
        /// <returns>匹配数据集合</returns>
        public TValueType[] GetFindArrayLock(TKeyType key, Func<TValueType, bool> isValue)
        {
            TmphKeyValue<TKeyType, TmphList<TValueType>> list;
            int version = keyVersion;
            if (groups.TryGetValue(key, out list) && list.Key.Equals(key))
            {
                Monitor.Enter(cache.SqlTool.Lock);
                try
                {
                    return list.Value.GetFindArray(isValue);
                }
                finally { Monitor.Exit(cache.SqlTool.Lock); }
            }
            else if (version != keyVersion)
            {
                Monitor.Enter(cache.SqlTool.Lock);
                try
                {
                    if (groups.TryGetValue(key, out list)) return list.Value.GetFindArray(isValue);
                }
                finally { Monitor.Exit(cache.SqlTool.Lock); }
            }
            return TmphNullValue<TValueType>.Array;
        }
        /// <summary>
        /// 获取匹配数据集合
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="isValue">数据匹配器,禁止锁操作</param>
        /// <returns>匹配数据集合</returns>
        public TValueType[] GetFindArray(TKeyType key, Func<TValueType, bool> isValue)
        {
            TmphKeyValue<TKeyType, TmphList<TValueType>> list;
            int version = keyVersion;
            if (!groups.TryGetValue(key, out list) || !list.Key.Equals(key) && version != keyVersion)
            {
                Monitor.Enter(cache.SqlTool.Lock);
                try
                {
                    groups.TryGetValue(key, out list);
                }
                finally { Monitor.Exit(cache.SqlTool.Lock); }
            }
            return list.Value.toSubArray().GetFindArray(isValue);
        }
        /// <summary>
        /// 获取逆序分页数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pageSize"></param>
        /// <param name="currentPage"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public TValueType[] GetPageDesc(TKeyType key, int pageSize, int currentPage, out int count)
        {
            TmphKeyValue<TKeyType, TmphList<TValueType>> list;
            int version = keyVersion;
            if (!groups.TryGetValue(key, out list) || !list.Key.Equals(key) && version != keyVersion)
            {
                Monitor.Enter(cache.SqlTool.Lock);
                try
                {
                    groups.TryGetValue(key, out list);
                }
                finally { Monitor.Exit(cache.SqlTool.Lock); }
            }
            count = list.Value.Count();
            return list.Value.toSubArray().GetPageDesc(pageSize, currentPage);
        }
    }
}
