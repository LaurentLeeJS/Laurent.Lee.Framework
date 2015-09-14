using System;
using System.Collections.Generic;
using Laurent.Lee.CLB.Code.CSharp;

namespace Laurent.Lee.CLB.Sql.Cache.Whole.Events
{
    /// <summary>
    /// 自增ID整表数组缓存
    /// </summary>
    /// <typeparam name="TValueType">表格类型</typeparam>
    /// <typeparam name="TModelType">模型类型</typeparam>
    public class TmphIdentityArray<TValueType, TModelType> : TmphIdentityCache<TValueType, TModelType>
        where TValueType : class, TModelType
        where TModelType : class
    {
        /// <summary>
        /// 数组长度递增
        /// </summary>
        private int addLength;
        /// <summary>
        /// SQL操作缓存
        /// </summary>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="baseIdentity">基础ID</param>
        /// <param name="group">数据分组</param>
        /// <param name="isReset">是否初始化事件与数据</param>
        /// <param name="addLength">数组长度递增</param>
        public TmphIdentityArray(Laurent.Lee.CLB.Emit.TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, int group = 0, int baseIdentity = 0, int addLength = 0, bool isReset = true)
            : base(sqlTool, group, baseIdentity, isReset)
        {
            this.addLength = addLength == 0 ? (1 << 16) : (addLength < 1024 ? 1024 : addLength);

            if (isReset)
            {
                sqlTool.OnInsertedLock += onInserted;
                sqlTool.OnDeletedLock += onDeleted;

                resetLock();
            }
        }
        /// <summary>
        /// 重新加载数据
        /// </summary>
        /// <param name="values">数据集合</param>
        protected void reset(TValueType[] values)
        {
            int maxIdentity = values.maxKey(value => getIdentity(value), 0);
            if (memberGroup == 0) SqlTool.Identity64 = maxIdentity + baseIdentity;
            TValueType[] newValues = new TValueType[(maxIdentity / addLength + 1) * addLength];
            foreach (TValueType value in values) newValues[getIdentity(value)] = value;
            this.values = newValues;
            Count = values.Length;
        }
        /// <summary>
        /// 重新加载数据
        /// </summary>
        protected override void reset()
        {
            reset(SqlTool.Where(null, memberMap).getArray());
        }
        /// <summary>
        /// 增加数据
        /// </summary>
        /// <param name="value">新增的数据</param>
        protected void onInserted(TValueType value)
        {
            int identity = getIdentity(value);
            if (identity >= values.Length)
            {
                int newLength = values.Length + addLength;
                while (newLength <= identity) newLength += addLength;
                TValueType[] newValues = new TValueType[newLength];
                values.CopyTo(newValues, 0);
                values = newValues;
            }
            TValueType newValue = Laurent.Lee.CLB.Emit.TmphConstructor<TValueType>.New();
            Laurent.Lee.CLB.Emit.TmphMemberCopyer<TModelType>.Copy(newValue, value, memberMap);
            values[identity] = newValue;
            ++Count;
            callOnInserted(newValue);
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="value">被删除的数据</param>
        protected void onDeleted(TValueType value)
        {
            int identity = getIdentity(value);
            TValueType TCacheValue = values[identity];
            --Count;
            values[identity] = null;
            callOnDeleted(TCacheValue);
        }
    }
}
