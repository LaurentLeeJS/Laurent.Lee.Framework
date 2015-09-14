using System;
using System.Collections.Generic;
using System.Threading;
using Laurent.Lee.CLB.Code.CSharp;
using System.Linq.Expressions;

namespace Laurent.Lee.CLB.Sql.Cache.Whole
{
    /// <summary>
    /// 分组列表 延时排序缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TKeyType">分组字典关键字类型</typeparam>
    public class TmphMemberArrayOrderLady<TValueType, TModelType, TKeyType, TTargetType> : TmphMember<TValueType, TModelType, TKeyType, TTargetType, TmphMemberLadyOrderArray<TValueType>>
        where TValueType : class, TModelType
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
        where TTargetType : class
    {
        /// <summary>
        /// 排序器
        /// </summary>
        private Func<TmphSubArray<TValueType>, TmphSubArray<TValueType>> sorter;
        /// <summary>
        /// 分组列表 延时排序缓存
        /// </summary>
        /// <param name="cache">整表缓存</param>
        /// <param name="getKey">分组字典关键字获取器</param>
        /// <param name="sorter">排序器</param>
        /// <param name="isReset">是否初始化</param>
        public TmphMemberArrayOrderLady(Events.TmphCache<TValueType, TModelType> cache
            , Func<TModelType, TKeyType> getKey, Func<TKeyType, TTargetType> getValue, Expression<Func<TTargetType, TmphMemberLadyOrderArray<TValueType>>> member, Func<TmphSubArray<TValueType>, TmphSubArray<TValueType>> sorter, bool isReset)
            : base(cache, getKey, getValue, member)
        {
            if (sorter == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
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
            foreach (TValueType value in cache.Values) onInserted(value);
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
        protected void onInserted(TValueType value, TKeyType key)
        {
            TTargetType target = getValue(key);
            if (target == null) TmphLog.Error.Add(typeof(TValueType).FullName + " 没有找到缓存目标对象 " + key.ToString(), false, true);
            else
            {
                TmphMemberLadyOrderArray<TValueType> array = getMember(target);
                if (array == null) setMember(target, array = new TmphMemberLadyOrderArray<TValueType>());
                array.Insert(value);
            }
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
                TTargetType target = getValue(key);
                if (target == null) TmphLog.Error.Add(typeof(TValueType).FullName + " 没有找到缓存目标对象 " + key.ToString(), false, true);
                else
                {
                    TmphMemberLadyOrderArray<TValueType> array = getMember(target);
                    if (array == null) TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", true, true);
                    else array.Update(value);
                }
            }
            else
            {
                onInserted(value, key);
                onDeleted(value, oldKey);
            }
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="value">被删除的数据</param>
        /// <param name="key">被删除数据的关键字</param>
        protected void onDeleted(TValueType value, TKeyType key)
        {
            TTargetType target = getValue(key);
            if (target == null) TmphLog.Error.Add(typeof(TValueType).FullName + " 没有找到缓存目标对象 " + key.ToString(), false, true);
            else
            {
                TmphMemberLadyOrderArray<TValueType> array = getMember(target);
                if (array == null) TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", true, true);
                else array.Delete(value);
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
        /// 获取缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private TmphMemberLadyOrderArray<TValueType> getCache(TKeyType key)
        {
            TTargetType target = getValue(key);
            return target != null ? getMember(target) : null;
        }
        /// <summary>
        /// 获取匹配数量
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>匹配数量</returns>
        public int Count(TKeyType key)
        {
            TmphMemberLadyOrderArray<TValueType> array = getCache(key);
            return array == null ? 0 : array.Count;
        }
        /// <summary>
        /// 获取匹配数量
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="isValue">数据匹配器,禁止锁操作</param>
        /// <returns>匹配数量</returns>
        public int Count(TKeyType key, Func<TValueType, bool> isValue)
        {
            TmphMemberLadyOrderArray<TValueType> array = getCache(key);
            return array == null ? 0 : array.CurrentArray.GetCount(isValue);
        }
        /// <summary>
        /// 查找第一个匹配的数据
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>第一个匹配的数据,失败返回null</returns>
        public TValueType FirstOrDefault(TKeyType key, Func<TValueType, bool> isValue)
        {
            TmphMemberLadyOrderArray<TValueType> array = getCache(key);
            return array == null ? null : array.CurrentArray.FirstOrDefault(isValue);
        }
        /// <summary>
        /// 获取匹配的数据集合
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>数据集合</returns>
        public TValueType[] GetFindArray(TKeyType key, Func<TValueType, bool> isValue)
        {
            TmphMemberLadyOrderArray<TValueType> array = getCache(key);
            return array == null ? TmphNullValue<TValueType>.Array : array.CurrentArray.GetFindArray(isValue);
        }
        /// <summary>
        /// 获取不排序的数据集合
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>数据集合</returns>
        public TmphSubArray<TValueType> GetCache(TKeyType key)
        {
            TmphMemberLadyOrderArray<TValueType> array = getCache(key);
            return array == null ? default(TmphSubArray<TValueType>) : array.CurrentArray;
        }
        /// <summary>
        /// 获取有序数据
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="index">关键字</param>
        /// <returns>获取有序数据</returns>
        public TValueType At(TKeyType key, int index)
        {
            TmphMemberLadyOrderArray<TValueType> array = getCache(key);
            return array == null ? null : array.At(cache.SqlTool.Lock, sorter, index);
        }
        /// <summary>
        /// 获取排序数据范围集合
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>排序数据范围集合</returns>
        public TValueType[] GetArray(TKeyType key)
        {
            TmphMemberLadyOrderArray<TValueType> array = getCache(key);
            return array == null ? TmphNullValue<TValueType>.Array : array.GetArray(cache.SqlTool.Lock, sorter);
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
            TmphMemberLadyOrderArray<TValueType> array = getCache(key);
            if (array != null) return array.GetPage(cache.SqlTool.Lock, sorter, pageSize, currentPage, out count);
            count = 0;
            return TmphNullValue<TValueType>.Array;
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
            TmphMemberLadyOrderArray<TValueType> array = getCache(key);
            if (array != null) return array.GetPageDesc(cache.SqlTool.Lock, sorter, pageSize, currentPage, out count);
            count = 0;
            return TmphNullValue<TValueType>.Array;
        }
    }
}