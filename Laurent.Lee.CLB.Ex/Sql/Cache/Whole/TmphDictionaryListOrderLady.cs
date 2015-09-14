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
    /// 分组列表 延时排序缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TKeyType">分组字典关键字类型</typeparam>
    public class TmphDictionaryListOrderLady<TValueType, TModelType, TKeyType>
        where TValueType : class, TModelType
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        /// 分组数据
        /// </summary>
        protected struct TmphGroup
        {
            /// <summary>
            /// 分组字典关键字
            /// </summary>
            public TKeyType Key;

            /// <summary>
            /// 分组数据
            /// </summary>
            public TmphList<TValueType> List;

            /// <summary>
            /// 分组有序数据索引
            /// </summary>
            public int Index;
        }

        /// <summary>
        /// 整表缓存
        /// </summary>
        protected Events.TmphCache<TValueType, TModelType> cache;

        /// <summary>
        /// 分组字典关键字获取器
        /// </summary>
        protected Func<TModelType, TKeyType> getKey;

        /// <summary>
        /// 排序器
        /// </summary>
        private Func<TmphList<TValueType>, TValueType[]> sorter;

        /// <summary>
        /// 分组数据
        /// </summary>
        protected Dictionary<TKeyType, TmphGroup> groups;

        /// <summary>
        /// 关键字集合
        /// </summary>
        public IEnumerable<TKeyType> Keys
        {
            get { return groups.Keys; }
        }

        /// <summary>
        /// 关键字版本号
        /// </summary>
        protected int keyVersion;

        /// <summary>
        /// 分组列表 延时排序缓存
        /// </summary>
        /// <param name="cache">整表缓存</param>
        /// <param name="getKey">分组字典关键字获取器</param>
        /// <param name="sorter">排序器</param>
        /// <param name="isReset">是否初始化</param>
        public TmphDictionaryListOrderLady(Events.TmphCache<TValueType, TModelType> cache
            , Func<TModelType, TKeyType> getKey, Func<TmphList<TValueType>, TValueType[]> sorter, bool isReset)
        {
            if (cache == null || getKey == null || sorter == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            this.cache = cache;
            this.getKey = getKey;
            this.sorter = sorter;

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
        /// 数据添加器
        /// </summary>
        protected struct TmphInsert
        {
            /// <summary>
            /// 分组数据
            /// </summary>
            public Dictionary<TKeyType, TmphGroup> groups;

            /// <summary>
            /// 分组字典关键字获取器
            /// </summary>
            public Func<TModelType, TKeyType> getKey;

            /// <summary>
            /// 添加数据
            /// </summary>
            /// <param name="value">数据对象</param>
            public void onInserted(TValueType value)
            {
                TKeyType key = getKey(value);
                TmphGroup values;
                if (!groups.TryGetValue(key, out values))
                {
                    groups.Add(key, values = new TmphGroup { Key = key, List = new TmphList<TValueType>(), Index = 0 });
                }
                values.List.Add(value);
            }

            /// <summary>
            /// 添加数据
            /// </summary>
            /// <param name="value">数据对象</param>
            /// <param name="cache">缓存对象</param>
            public void onInserted(TValueType value, TmphDictionaryListOrderLady<TValueType, TModelType, TKeyType> cache)
            {
                TKeyType key = getKey(value);
                TmphGroup values;
                if (!groups.TryGetValue(key, out values))
                {
                    groups.Add(key, values = new TmphGroup { Key = key, List = new TmphList<TValueType>(), Index = 0 });
                    ++cache.keyVersion;
                }
                values.List.Add(value);
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
            TmphInsert insert = new TmphInsert { groups = TmphDictionary<TKeyType>.Create<TmphGroup>(), getKey = getKey };
            foreach (TValueType value in cache.Values) insert.onInserted(value);
            groups = insert.groups;
            ++keyVersion;
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value">数据对象</param>
        protected void onInserted(TValueType value)
        {
            new TmphInsert { groups = groups, getKey = getKey }.onInserted(value, this);
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
                TmphGroup values;
                if (indexOf(value, key, out values) == -1)
                {
                    TmphLog.Error.Add("ERROR " + key.ToString(), false, false);
                }
            }
            else
            {
                onInserted(value);
                onDeleted(value, oldKey);
            }
        }

        /// <summary>
        /// 查找历史位置
        /// </summary>
        /// <param name="value">待查找对象</param>
        /// <param name="key">查找关键字</param>
        /// <param name="values">列表数据集合</param>
        /// <returns>历史位置,失败为-1</returns>
        private int indexOf(TValueType value, TKeyType key, out TmphGroup values)
        {
            if (groups.TryGetValue(key, out values))
            {
                TValueType[] array = values.List.Unsafer.Array;
                int index = Array.LastIndexOf(array, value, values.List.Count - 1);
                if (index != -1)
                {
                    if (values.Index > index)
                    {
                        values.Index = index;
                        groups[key] = values;
                    }
                    return index;
                }
            }
            TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", true, true);
            return -1;
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="value">被删除的数据</param>
        /// <param name="key">被删除数据的关键字</param>
        protected void onDeleted(TValueType value, TKeyType key)
        {
            TmphGroup values;
            int index = indexOf(value, key, out values);
            if (index != -1)
            {
                values.List.Unsafer.AddLength(-1);
                int count = values.List.Count;
                if (count != 0)
                {
                    TValueType[] array = values.List.Unsafer.Array;
                    array[index] = array[count];
                    array[count] = null;
                }
                else groups.Remove(key);
            }
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
        /// 获取匹配数量
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>匹配数量</returns>
        public int Count(TKeyType key)
        {
            TmphGroup values;
            return groups.TryGetValue(key, out values) ? values.List.Count : 0;
        }

        /// <summary>
        /// 获取匹配数量
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="isValue">数据匹配器,禁止锁操作</param>
        /// <returns>匹配数量</returns>
        public int CountLock(TKeyType key, Func<TValueType, bool> isValue)
        {
            TmphGroup values;
            int version = keyVersion;
            if (groups.TryGetValue(key, out values) && values.Key.Equals(key))
            {
                Monitor.Enter(cache.SqlTool.Lock);
                try
                {
                    return values.List.GetCount(isValue);
                }
                finally { Monitor.Exit(cache.SqlTool.Lock); }
            }
            else if (version != keyVersion)
            {
                Monitor.Enter(cache.SqlTool.Lock);
                try
                {
                    if (groups.TryGetValue(key, out values)) return values.List.GetCount(isValue);
                }
                finally { Monitor.Exit(cache.SqlTool.Lock); }
            }
            return 0;
        }

        /// <summary>
        /// 获取索引数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="index"></param>
        /// <param name="nullValue"></param>
        /// <returns></returns>
        public TValueType At(TKeyType key, int index, TValueType nullValue)
        {
            TmphGroup values;
            int version = keyVersion;
            if (groups.TryGetValue(key, out values) && values.Key.Equals(key))
            {
                if (index < values.List.Count) return values.List.Unsafer.Array[index];
            }
            else if (version != keyVersion)
            {
                Monitor.Enter(cache.SqlTool.Lock);
                try
                {
                    if (groups.TryGetValue(key, out values) && index < values.List.Count) return values.List.Unsafer.Array[index];
                }
                finally { Monitor.Exit(cache.SqlTool.Lock); }
            }
            return nullValue;
        }

        /// <summary>
        /// 查找第一个匹配的数据
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="isValue">数据匹配器,禁止锁操作</param>
        /// <returns>第一个匹配的数据,失败返回null</returns>
        public TValueType FirstOrDefaultLock(TKeyType key, Func<TValueType, bool> isValue)
        {
            TmphGroup values;
            int version = keyVersion;
            if (groups.TryGetValue(key, out values) && values.Key.Equals(key))
            {
                Monitor.Enter(cache.SqlTool.Lock);
                try
                {
                    return values.List.FirstOrDefault(isValue);
                }
                finally { Monitor.Exit(cache.SqlTool.Lock); }
            }
            else if (version != keyVersion)
            {
                Monitor.Enter(cache.SqlTool.Lock);
                try
                {
                    if (groups.TryGetValue(key, out values)) return values.List.FirstOrDefault(isValue);
                }
                finally { Monitor.Exit(cache.SqlTool.Lock); }
            }
            return null;
        }

        /// <summary>
        /// 获取匹配的数据集合
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="isValue">数据匹配器,禁止锁操作</param>
        /// <returns>数据集合</returns>
        public TValueType[] GetFindArrayNoOrderLock(TKeyType key, Func<TValueType, bool> isValue)
        {
            TmphGroup values;
            int version = keyVersion;
            if (groups.TryGetValue(key, out values) && values.Key.Equals(key))
            {
                Monitor.Enter(cache.SqlTool.Lock);
                try
                {
                    return values.List.GetFindArray(isValue);
                }
                finally { Monitor.Exit(cache.SqlTool.Lock); }
            }
            else if (version != keyVersion)
            {
                Monitor.Enter(cache.SqlTool.Lock);
                try
                {
                    if (groups.TryGetValue(key, out values)) return values.List.GetFindArray(isValue);
                }
                finally { Monitor.Exit(cache.SqlTool.Lock); }
            }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        /// 获取不排序的数据集合
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>数据集合</returns>
        public TValueType[] GetArrayNoOrder(TKeyType key)
        {
            TmphGroup values;
            int version = keyVersion;
            if (groups.TryGetValue(key, out values) && values.Key.Equals(key))
            {
                Monitor.Enter(cache.SqlTool.Lock);
                try
                {
                    return values.List.GetArray();
                }
                finally { Monitor.Exit(cache.SqlTool.Lock); }
            }
            else if (version != keyVersion)
            {
                Monitor.Enter(cache.SqlTool.Lock);
                try
                {
                    if (groups.TryGetValue(key, out values)) return values.List.GetArray();
                }
                finally { Monitor.Exit(cache.SqlTool.Lock); }
            }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        /// 获取排序数据范围集合
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>排序数据范围集合</returns>
        public TValueType[] GetArray(TKeyType key)
        {
            Monitor.Enter(cache.SqlTool.Lock);
            try
            {
                TmphList<TValueType> list = get(key);
                if (list != null) return list.GetArray();
            }
            finally { Monitor.Exit(cache.SqlTool.Lock); }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>数据列表</returns>
        private TmphList<TValueType> get(TKeyType key)
        {
            TmphGroup values;
            if (groups.TryGetValue(key, out values) && values.Key.Equals(key))
            {
                TmphList<TValueType> list = values.List;
                if (values.Index != list.Count)
                {
                    sorter(list).CopyTo(list.Unsafer.Array, 0);
                    values.Index = list.Count;
                    groups[key] = values;
                }
                return values.List;
            }
            return null;
        }

        /// <summary>
        /// 获取分页数据集合
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="currentPage">分页页号</param>
        /// <param name="count">数据总数</param>
        /// <returns>分页数据集合</returns>
        public TValueType[] GetPage(TKeyType key, int pageSize, int currentPage, out int count)
        {
            TValueType[] values = null;
            Monitor.Enter(cache.SqlTool.Lock);
            try
            {
                TmphList<TValueType> list = get(key);
                if (list == null) count = 0;
                else
                {
                    TmphArray.TmphPage page = new TmphArray.TmphPage(count = list.Count, pageSize, currentPage);
                    values = list.GetSub(page.SkipCount, page.CurrentPageSize);
                }
            }
            finally { Monitor.Exit(cache.SqlTool.Lock); }
            return values ?? TmphNullValue<TValueType>.Array;
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
            Monitor.Enter(cache.SqlTool.Lock);
            try
            {
                TmphList<TValueType> list = get(key);
                if (list == null) count = 0;
                else
                {
                    TmphArray.TmphPage page = new TmphArray.TmphPage(count = list.Count, pageSize, currentPage);
                    values = list.GetSub(page.DescSkipCount, page.CurrentPageSize);
                }
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
        /// <param name="isValue">数据匹配器,禁止锁操作</param>
        /// <param name="count">数据总数</param>
        /// <returns>逆序分页数据集合</returns>
        public TmphSubArray<TValueType> GetPageDescLock
            (TKeyType key, int pageSize, int currentPage, Func<TValueType, bool> isValue, out int count)
        {
            TmphSubArray<TValueType> values = default(TmphSubArray<TValueType>);
            count = 0;
            Monitor.Enter(cache.SqlTool.Lock);
            try
            {
                TmphList<TValueType> list = get(key);
                if (list != null)
                {
                    TValueType[] array = list.Unsafer.Array;
                    TmphArray.TmphPage page = new TmphArray.TmphPage(list.Count, pageSize, currentPage);
                    for (int index = list.Count, skipCount = page.SkipCount, getCount = page.CurrentPageSize; --index >= 0;)
                    {
                        TValueType value = array[index];
                        if (isValue(value))
                        {
                            if (skipCount == 0)
                            {
                                if (getCount != 0)
                                {
                                    if (values.Array == null) values = new TmphSubArray<TValueType>(page.CurrentPageSize);
                                    values.Add(value);
                                    --getCount;
                                }
                            }
                            else --skipCount;
                            ++count;
                        }
                    }
                }
            }
            finally { Monitor.Exit(cache.SqlTool.Lock); }
            return values;
        }

        /// <summary>
        /// 获取排序数据范围集合
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数</param>
        /// <returns>排序数据范围集合</returns>
        public TValueType[] GetSort(TKeyType key, int skipCount, int getCount)
        {
            Monitor.Enter(cache.SqlTool.Lock);
            try
            {
                TmphList<TValueType> list = get(key);
                if (list != null)
                {
                    TmphArray.TmphRange range = new TmphArray.TmphRange(list.Count, skipCount, getCount);
                    if ((getCount = range.GetCount) != 0) return list.GetSub(range.SkipCount, range.GetCount);
                }
            }
            finally { Monitor.Exit(cache.SqlTool.Lock); }
            return TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        /// 获取逆序数据范围集合
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="getCount">获取记录数</param>
        /// <returns>逆序数据范围集合</returns>
        public TValueType[] GetSortDesc(TKeyType key, int skipCount, int getCount)
        {
            TValueType[] values = null;
            Monitor.Enter(cache.SqlTool.Lock);
            try
            {
                TmphList<TValueType> list = get(key);
                if (list != null)
                {
                    TmphArray.TmphRange range = new TmphArray.TmphRange(list.Count, skipCount, getCount);
                    if ((getCount = range.GetCount) != 0)
                    {
                        values = list.GetSub(list.Count - range.SkipCount - range.GetCount, range.GetCount);
                    }
                }
            }
            finally { Monitor.Exit(cache.SqlTool.Lock); }
            return values.reverse();
        }
    }
}