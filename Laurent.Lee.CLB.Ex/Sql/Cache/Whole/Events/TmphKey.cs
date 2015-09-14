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

namespace Laurent.Lee.CLB.Sql.Cache.Whole.Events
{
    /// <summary>
    /// 关键字整表缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    public abstract class TmphKey<TValueType, TModelType, TKeyType> : TmphCache<TValueType, TModelType>
        where TValueType : class, TModelType
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        /// 键值获取器
        /// </summary>
        private Func<TModelType, TKeyType> getKey;

        /// <summary>
        /// 字典缓存数据
        /// </summary>
        protected Dictionary<TKeyType, TValueType> dictionary;

        /// <summary>
        /// 数据集合,请使用GetArray
        /// </summary>
        public override IEnumerable<TValueType> Values
        {
            get
            {
                return GetArray();
            }
        }

        /// <summary>
        /// 关键字版本
        /// </summary>
        protected int keyVersion;

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>数据</returns>
        public TValueType this[TKeyType key]
        {
            get
            {
                TValueType value;
                int version = keyVersion;
                if (dictionary.TryGetValue(key, out value) && getKey(value).Equals(key)) return value;
                else if (version != keyVersion)
                {
                    Monitor.Enter(SqlTool.Lock);
                    try
                    {
                        if (dictionary.TryGetValue(key, out value)) return value;
                    }
                    finally { Monitor.Exit(SqlTool.Lock); }
                }
                return null;
            }
        }

        /// <summary>
        /// 缓存数据数量
        /// </summary>
        public int Count
        {
            get { return dictionary.Count; }
        }

        /// <summary>
        /// 关键字整表缓存
        /// </summary>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="getKey">键值获取器</param>
        /// <param name="group">数据分组</param>
        public TmphKey(Laurent.Lee.CLB.Emit.TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, Func<TModelType, TKeyType> getKey, int group = 0)
            : base(sqlTool, group)
        {
            if (getKey == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            this.getKey = getKey;

            sqlTool.OnInsertedLock += onInserted;
            sqlTool.OnUpdatedLock += onUpdated;
            sqlTool.OnDeletedLock += onDeleted;

            resetLock();
        }

        /// <summary>
        /// 重新加载数据
        /// </summary>
        protected override void reset()
        {
            dictionary = SqlTool.Where(null, memberMap).getDictionary<TValueType, TKeyType>(getKey);
            ++keyVersion;
        }

        /// <summary>
        /// 增加数据
        /// </summary>
        /// <param name="value">新增的数据</param>
        protected void onInserted(TValueType value)
        {
            TValueType newValue = Laurent.Lee.CLB.Emit.TmphConstructor<TValueType>.New();
            Laurent.Lee.CLB.Emit.TmphMemberCopyer<TModelType>.Copy(newValue, value, memberMap);
            dictionary.Add(getKey(value), newValue);
            ++keyVersion;
            callOnInserted(newValue);
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="value">更新后的数据</param>
        /// <param name="oldValue">更新前的数据</param>
        /// <param name="memberMap">更新成员位图</param>
        protected void onUpdated(TValueType value, TValueType oldValue, Laurent.Lee.CLB.Code.TmphMemberMap memberMap)
        {
            TValueType TCacheValue;
            if (dictionary.TryGetValue(getKey(value), out TCacheValue))
            {
                using (Laurent.Lee.CLB.Code.TmphMemberMap newMemberMap = updateMemberMap(memberMap))
                {
                    update(TCacheValue, value, oldValue, newMemberMap);
                    callOnUpdated(TCacheValue, oldValue);
                }
            }
            else TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", false, true);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="value">被删除的数据</param>
        protected void onDeleted(TValueType value)
        {
            TValueType TCacheValue;
            if (dictionary.TryGetValue(getKey(value), out TCacheValue))
            {
                dictionary.Remove(getKey(value));
                callOnDeleted(TCacheValue);
            }
            else TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", false, true);
        }

        /// <summary>
        /// 获取数据数组集合
        /// </summary>
        /// <returns>数据数组集合</returns>
        public TValueType[] GetArray()
        {
            Monitor.Enter(SqlTool.Lock);
            try
            {
                return dictionary.Values.GetArray();
            }
            finally { Monitor.Exit(SqlTool.Lock); }
        }
    }
}