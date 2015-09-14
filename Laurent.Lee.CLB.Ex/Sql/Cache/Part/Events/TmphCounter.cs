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
using System.Linq.Expressions;

namespace Laurent.Lee.CLB.Sql.Cache.Part.Events
{
    /// <summary>
    /// 缓存计数器
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    public abstract class TmphCounter<TValueType, TModelType, TKeyType> : TmphCopy<TValueType, TModelType>
        where TValueType : class, TModelType
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        /// 缓存关键字获取器
        /// </summary>
        public Func<TModelType, TKeyType> GetKey { get; private set; }

        /// <summary>
        /// 缓存数据
        /// </summary>
        private Dictionary<TKeyType, TmphKeyValue<TValueType, int>> values;

        /// <summary>
        /// 缓存数据数量
        /// </summary>
        public int Count
        {
            get { return values.Count; }
        }

        /// <summary>
        /// 获取缓存值
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>缓存值</returns>
        public TValueType this[TKeyType key]
        {
            get
            {
                return Get(key);
            }
        }

        /// <summary>
        /// 缓存计数
        /// </summary>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="getKey">缓存关键字获取器</param>
        /// <param name="group">数据分组</param>
        protected TmphCounter(Laurent.Lee.CLB.Emit.TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, int group, Expression<Func<TModelType, TKeyType>> getKey)
            : this(sqlTool, group, getKey == null ? null : getKey.Compile())
        {
            sqlTool.SetSelectMember(getKey);
        }

        /// <summary>
        /// 缓存计数
        /// </summary>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="getKey">缓存关键字获取器</param>
        /// <param name="group">数据分组</param>
        protected TmphCounter(Laurent.Lee.CLB.Emit.TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, int group, Func<TModelType, TKeyType> getKey)
            : base(sqlTool, group)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            GetKey = getKey;
            values = TmphDictionary<TKeyType>.Create<TmphKeyValue<TValueType, int>>();

            sqlTool.OnUpdatedLock += onUpdated;
            sqlTool.OnDeletedLock += onDeleted;
        }

        /// <summary>
        /// 重置缓存
        /// </summary>
        protected override void reset()
        {
            values.Clear();
        }

        /// <summary>
        /// 更新记录事件
        /// </summary>
        public event Action<TValueType, TValueType, TValueType> OnUpdated;

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="value">更新后的数据</param>
        /// <param name="oldValue">更新前的数据</param>
        /// <param name="memberMap">更新成员位图</param>
        private void onUpdated(TValueType value, TValueType oldValue, Laurent.Lee.CLB.Code.TmphMemberMap memberMap)
        {
            TmphKeyValue<TValueType, int> TCacheValue;
            TKeyType key = GetKey(value);
            if (values.TryGetValue(key, out TCacheValue)) update(TCacheValue.Key, value, memberMap);
            if (OnUpdated != null) OnUpdated(TCacheValue.Key, value, oldValue);
        }

        /// <summary>
        /// 删除记录事件
        /// </summary>
        public event Action<TValueType> OnDeleted;

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="value">被删除的数据</param>
        private void onDeleted(TValueType value)
        {
            TmphKeyValue<TValueType, int> TCacheValue;
            TKeyType key = GetKey(value);
            if (values.TryGetValue(key, out TCacheValue))
            {
                values.Remove(GetKey(value));
                if (OnDeleted != null) OnDeleted(TCacheValue.Key);
            }
        }

        /// <summary>
        /// 获取缓存数据
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>缓存数据</returns>
        public TValueType Get(TKeyType key)
        {
            TmphKeyValue<TValueType, int> valueCount;
            return values.TryGetValue(key, out valueCount) ? valueCount.Key : null;
        }

        /// <summary>
        /// 获取缓存数据
        /// </summary>
        /// <param name="value">查询数据</param>
        /// <returns>缓存数据</returns>
        public TValueType Get(TValueType value)
        {
            TmphKeyValue<TValueType, int> valueCount;
            return values.TryGetValue(GetKey(value), out valueCount) ? valueCount.Key : null;
        }

        /// <summary>
        /// 添加缓存数据
        /// </summary>
        /// <param name="value">缓存数据</param>
        /// <returns>缓存数据</returns>
        public TValueType Add(TValueType value)
        {
            TmphKeyValue<TValueType, int> valueCount;
            TKeyType key = GetKey(value);
            if (values.TryGetValue(key, out valueCount))
            {
                ++valueCount.Value;
                values[key] = valueCount;
                return valueCount.Key;
            }
            TValueType copyValue = Laurent.Lee.CLB.Emit.TmphConstructor<TValueType>.New();
            Laurent.Lee.CLB.Emit.TmphMemberCopyer<TModelType>.Copy(copyValue, value, memberMap);
            values.Add(key, new TmphKeyValue<TValueType, int>(copyValue, 0));
            return copyValue;
        }

        /// <summary>
        /// 删除缓存数据
        /// </summary>
        /// <param name="value">缓存数据</param>
        public void Remove(TValueType value)
        {
            TmphKeyValue<TValueType, int> valueCount;
            TKeyType key = GetKey(value);
            if (values.TryGetValue(key, out valueCount))
            {
                if (valueCount.Value == 0) values.Remove(key);
                else
                {
                    --valueCount.Value;
                    values[key] = valueCount;
                }
            }
            else TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", false, true);
        }
    }
}