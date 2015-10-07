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
using System.Linq.Expressions;
using System.Threading;

namespace Laurent.Lee.CLB.Sql.Cache.Whole
{
    /// <summary>
    /// 分组列表缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TKeyType">分组字典关键字类型</typeparam>
    public class TmphMemberList<TValueType, TModelType, TKeyType, TTargetType> : TmphMember<TValueType, TModelType, TKeyType, TTargetType, TmphList<TValueType>>
        where TValueType : class, TModelType
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
        where TTargetType : class
    {
        /// <summary>
        /// 移除数据并使用最后一个数据移动到当前位置
        /// </summary>
        protected bool isRemoveEnd;

        /// <summary>
        /// 分组列表缓存
        /// </summary>
        /// <param name="cache">整表缓存</param>
        /// <param name="getKey">分组字典关键字获取器</param>
        /// <param name="getValue">获取目标对象委托</param>
        /// <param name="member">缓存字段表达式</param>
        /// <param name="isRemoveEnd">移除数据并使用最后一个数据移动到当前位置</param>
        /// <param name="isReset">是否绑定事件并重置数据</param>
        public TmphMemberList(Events.TmphCache<TValueType, TModelType> cache, Func<TModelType, TKeyType> getKey, Func<TKeyType, TTargetType> getValue, Expression<Func<TTargetType, TmphList<TValueType>>> member, bool isRemoveEnd = false, bool isReset = true)
            : base(cache, getKey, getValue, member)
        {
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
                TmphList<TValueType> list = getMember(target);
                if (list == null) setMember(target, list = new TmphList<TValueType>());
                list.Add(value);
            }
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="value">更新后的数据</param>
        /// <param name="oldValue">更新前的数据</param>
        protected void onUpdated(TValueType value, TValueType oldValue)
        {
            TKeyType oldKey = getKey(oldValue), newKey = getKey(value);
            if (!newKey.Equals(oldKey))
            {
                onInserted(value, newKey);
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
            TTargetType target = getValue(key);
            if (target == null) TmphLog.Error.Add(typeof(TValueType).FullName + " 没有找到缓存目标对象 " + key.ToString(), false, true);
            else
            {
                TmphList<TValueType> list = getMember(target);
                if (list != null)
                {
                    int index = Array.LastIndexOf(list.Unsafer.Array, value, list.Count - 1);
                    if (index != -1)
                    {
                        if (isRemoveEnd) list.RemoveAtEnd(index);
                        else list.RemoveAt(index);
                        return;
                    }
                }
                TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", false, true);
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
        public TmphList<TValueType> GetCache(TKeyType key)
        {
            TTargetType target = getValue(key);
            return target != null ? getMember(target) : null;
        }

        /// <summary>
        /// 获取匹配数据数量
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>匹配数据数量</returns>
        public int Count(TKeyType key)
        {
            return GetCache(key).Count();
        }

        /// <summary>
        /// 获取匹配数据数量
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配数据数量</returns>
        public int Count(TKeyType key, Func<TValueType, bool> isValue)
        {
            return GetCache(key).count(isValue);
        }

        /// <summary>
        /// 获取第一个数据
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>数据集合</returns>
        public TValueType First(TKeyType key, TValueType nullValue = null)
        {
            TmphList<TValueType> list = GetCache(key);
            return list.Count() != 0 ? list.Unsafer.Array[0] : nullValue;
        }

        /// <summary>
        /// 获取第一个匹配数据
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>第一个匹配数据</returns>
        public TValueType FirstOrDefault(TKeyType key, Func<TValueType, bool> isValue)
        {
            return GetCache(key).firstOrDefault(isValue);
        }

        /// <summary>
        /// 获取数据集合
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>数据集合</returns>
        public TValueType[] GetArray(TKeyType key)
        {
            return GetCache(key).getArray();
        }

        /// <summary>
        /// 获取匹配数据集合
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="isValue">数据匹配器</param>
        /// <returns>匹配数据集合</returns>
        public TValueType[] GetFindArray(TKeyType key, Func<TValueType, bool> isValue)
        {
            return GetCache(key).toSubArray().GetFindArray(isValue);
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
            TmphList<TValueType> list = GetCache(key);
            count = list.Count();
            return list.toSubArray().GetPageDesc(pageSize, currentPage);
        }
    }
}