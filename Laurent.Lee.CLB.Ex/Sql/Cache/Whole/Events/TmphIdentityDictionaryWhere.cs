using System;
using System.Collections.Generic;
using System.Threading;
using Laurent.Lee.CLB.Code.CSharp;

namespace Laurent.Lee.CLB.Sql.Cache.Whole.Events
{
    /// <summary>
    /// 自增ID整表缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    public sealed class TmphIdentityDictionaryWhere<TValueType, TModelType> :TmphIdentityMemberMap<TValueType, TModelType>
        where TValueType : class, TModelType
        where TModelType : class
    {
        /// <summary>
        /// 数据匹配器
        /// </summary>
        private Func<TValueType, bool> isValue;
        /// <summary>
        /// 数据缓存集合
        /// </summary>
        private Dictionary<int, TValueType> values;
        /// <summary>
        /// 数据集合,请使用GetArray()
        /// </summary>
        public override IEnumerable<TValueType> Values
        {
            get
            {
                return GetArray();
            }
        }
        /// <summary>
        /// 自增ID版本号
        /// </summary>
        private int identityVersion;
        /// <summary>
        /// 自增ID整表数组缓存
        /// </summary>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="isValue">数据匹配器,必须保证更新数据的匹配一致性</param>
        /// <param name="baseIdentity">基础ID</param>
        /// <param name="group">数据分组</param>
        public TmphIdentityDictionaryWhere(Laurent.Lee.CLB.Emit.TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool
            , Func<TValueType, bool> isValue, int group = 0, int baseIdentity = 0)
            : base(sqlTool, group, baseIdentity)
        {
            if (isValue == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            this.isValue = isValue;

            sqlTool.OnInserted += onInserted;
            sqlTool.OnUpdated += onUpdated;
            sqlTool.OnDeleted += onDeleted;

            resetLock();
        }
        /// <summary>
        /// 重新加载数据
        /// </summary>
        protected override void reset()
        {
            TValueType[] values = SqlTool.Where(null, memberMap).getFindArray(isValue);
            Dictionary<int, TValueType> newValues = TmphDictionary.CreateInt<TValueType>(values.Length);
            int maxIdentity = 0;
            foreach (TValueType value in values)
            {
                int identity = getIdentity(value);
                if (identity > maxIdentity) maxIdentity = identity;
                newValues.Add(identity, value);
            }
            if (memberGroup == 0) SqlTool.Identity64 = maxIdentity + baseIdentity;
            this.values = newValues;
            ++identityVersion;
            Count = values.Length;
        }
        /// <summary>
        /// 增加数据
        /// </summary>
        /// <param name="value">新增的数据</param>
        private void onInserted(TValueType value)
        {
            if (isValue(value)) add(value);
        }
        /// <summary>
        /// 增加数据
        /// </summary>
        /// <param name="value">新增的数据</param>
        private void add(TValueType value)
        {
            TValueType newValue = Laurent.Lee.CLB.Emit.TmphConstructor<TValueType>.New();
            Laurent.Lee.CLB.Emit.TmphMemberCopyer<TModelType>.Copy(newValue, value, memberMap);
            values.Add(getIdentity(value), newValue);
            ++identityVersion;
            ++Count;
            callOnInserted(newValue);
        }
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="value">更新后的数据</param>
        /// <param name="oldValue">更新前的数据</param>
        /// <param name="memberMap">更新成员位图</param>
        private void onUpdated(TValueType value, TValueType oldValue, Laurent.Lee.CLB.Code.TmphMemberMap memberMap)
        {
            if (isValue(value))
            {
                TValueType TCacheValue;
                if (values.TryGetValue(getIdentity(value), out TCacheValue))
                {
                    using (Laurent.Lee.CLB.Code.TmphMemberMap newMemberMap = updateMemberMap(memberMap))
                    {
                        update(TCacheValue, value, oldValue, newMemberMap);
                        callOnUpdated(TCacheValue, oldValue);
                    }
                }
                else TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", false, true);
            }
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="value">被删除的数据</param>
        private void onDeleted(TValueType value)
        {
            if (isValue(value))
            {
                TValueType TCacheValue;
                int identity = getIdentity(value);
                if (values.TryGetValue(identity, out TCacheValue))
                {
                    values.Remove(identity);
                    --Count;
                    callOnDeleted(TCacheValue);
                }
                else TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", false, true);
            }
        }
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="id">数据自增ID</param>
        /// <param name="errorValue">错误返回值</param>
        /// <returns>数据</returns>
        public TValueType Get(int id, TValueType errorValue)
        {
            TValueType value;
            int version = identityVersion;
            if (values.TryGetValue(id, out value) && getIdentity(value) == id) return value;
            else if (version != identityVersion)
            {
                Monitor.Enter(SqlTool.Lock);
                try
                {
                    if (values.TryGetValue(id, out value)) return value;
                }
                finally { Monitor.Exit(SqlTool.Lock); }
            }
            return errorValue;
        }
        /// <summary>
        /// 获取数据集合
        /// </summary>
        /// <returns>数据集合</returns>
        public TValueType[] GetArray()
        {
            Monitor.Enter(SqlTool.Lock);
            try
            {
                return values.Values.GetArray();
            }
            finally { Monitor.Exit(SqlTool.Lock); }
        }
    }
}
