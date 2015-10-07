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

using Laurent.Lee.CLB.Code;
using Laurent.Lee.CLB.MemoryDataBase.Cache;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Laurent.Lee.CLB.MemoryDatabase.Cache.Index
{
    /// <summary>
    /// 分组字典缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TKeyType">分组字典关键字类型</typeparam>
    public class TmphMemberDictionary<TValueType, TModelType, TKeyType, TValueKeyType, TTargetType> : TmphMember<TValueType, TModelType, TKeyType, TTargetType, Dictionary<TValueKeyType, TValueType>>
        where TValueType : class, TModelType
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
        where TTargetType : class
        where TValueKeyType : IEquatable<TValueKeyType>
    {
        /// <summary>
        /// 获取数据关键字委托
        /// </summary>
        private Func<TModelType, TValueKeyType> getValueKey;

        /// <summary>
        /// 分组字典缓存
        /// </summary>
        /// <param name="cache">整表缓存</param>
        /// <param name="getKey">分组字典关键字获取器</param>
        /// <param name="getValue">获取目标对象委托</param>
        /// <param name="member">缓存字段表达式</param>
        /// <param name="isRemoveEnd">移除数据并使用最后一个数据移动到当前位置</param>
        /// <param name="isReset">是否绑定事件并重置数据</param>
        public TmphMemberDictionary(TmphILoadCache<TValueType, TModelType> cache, Func<TModelType, TKeyType> getKey, Func<TKeyType, TTargetType> getValue, Expression<Func<TTargetType, Dictionary<TValueKeyType, TValueType>>> member, Func<TModelType, TValueKeyType> getValueKey, bool isReset = true)
            : base(cache, getKey, getValue, member)
        {
            if (getValueKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            this.getValueKey = getValueKey;

            if (isReset)
            {
                cache.OnInserted += onInserted;
                cache.OnUpdated += onUpdated;
                cache.OnDeleted += onDeleted;
                reset();
            }
        }

        /// <summary>
        /// 重新加载数据
        /// </summary>
        protected virtual void reset()
        {
            cache.WaitLoad();
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
                Dictionary<TValueKeyType, TValueType> dictionary = getMember(target);
                if (dictionary == null) setMember(target, dictionary = new Dictionary<TValueKeyType, TValueType>());
                dictionary.Add(getValueKey(value), value);
            }
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="value">更新后的数据</param>
        /// <param name="oldValue">更新前的数据</param>
        protected void onUpdated(TValueType value, TValueType oldValue, TmphMemberMap memberMap)
        {
            TKeyType oldKey = getKey(oldValue), newKey = getKey(value);
            if (newKey.Equals(oldKey))
            {
                TValueKeyType oldValueKey = getValueKey(oldValue), newValueKey = getValueKey(value);
                if (!oldValueKey.Equals(newValueKey))
                {
                    TTargetType target = getValue(newKey);
                    if (target == null) TmphLog.Error.Add(typeof(TValueType).FullName + " 没有找到缓存目标对象 " + newKey.ToString(), false, true);
                    else
                    {
                        Dictionary<TValueKeyType, TValueType> dictionary = getMember(target);
                        if (dictionary != null)
                        {
                            if (dictionary.Remove(oldValueKey))
                            {
                                dictionary.Add(newValueKey, value);
                                return;
                            }
                            dictionary.Add(newValueKey, value);
                        }
                        TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", false, true);
                    }
                }
            }
            else
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
                Dictionary<TValueKeyType, TValueType> dictionary = getMember(target);
                if (dictionary == null || !dictionary.Remove(getValueKey(value)))
                {
                    TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", false, true);
                }
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
        public ICollection<TValueType> GetCache(TKeyType key)
        {
            TTargetType target = getValue(key);
            if (target != null)
            {
                Dictionary<TValueKeyType, TValueType> dictionary = getMember(target);
                if (dictionary != null) return dictionary.Values;
            }
            return TmphNullValue<TValueType>.Array;
        }
    }

    /// <summary>
    /// 分组字典缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TKeyType">分组字典关键字类型</typeparam>
    public class TmphMemberDictionary<TModelType, TKeyType, TValueKeyType, TTargetType> : TmphMemberDictionary<TModelType, TModelType, TKeyType, TValueKeyType, TTargetType>
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
        where TTargetType : class
        where TValueKeyType : IEquatable<TValueKeyType>
    {
        /// <summary>
        /// 分组字典缓存
        /// </summary>
        /// <param name="cache">整表缓存</param>
        /// <param name="getKey">分组字典关键字获取器</param>
        /// <param name="getValue">获取目标对象委托</param>
        /// <param name="member">缓存字段表达式</param>
        /// <param name="isRemoveEnd">移除数据并使用最后一个数据移动到当前位置</param>
        /// <param name="isReset">是否绑定事件并重置数据</param>
        public TmphMemberDictionary(TmphILoadCache<TModelType, TModelType> cache, Func<TModelType, TKeyType> getKey, Func<TKeyType, TTargetType> getValue, Expression<Func<TTargetType, Dictionary<TValueKeyType, TModelType>>> member, Func<TModelType, TValueKeyType> getValueKey)
            : base(cache, getKey, getValue, member, getValueKey, true)
        {
        }
    }
}