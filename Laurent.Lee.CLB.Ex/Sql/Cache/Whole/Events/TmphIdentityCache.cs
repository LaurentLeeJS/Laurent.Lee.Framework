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

using System.Collections.Generic;

namespace Laurent.Lee.CLB.Sql.Cache.Whole.Events
{
    /// <summary>
    /// 自增ID整表缓存
    /// </summary>
    /// <typeparam name="TValueType">表格类型</typeparam>
    /// <typeparam name="TModelType">模型类型</typeparam>
    public abstract class TmphIdentityCache<TValueType, TModelType> : TmphIdentityMemberMap<TValueType, TModelType>
        where TValueType : class, TModelType
        where TModelType : class
    {
        /// <summary>
        /// 缓存数据集合
        /// </summary>
        protected TValueType[] values;

        /// <summary>
        /// 数据集合
        /// </summary>
        public override IEnumerable<TValueType> Values
        {
            get
            {
                int count = Count;
                foreach (TValueType value in values)
                {
                    if (value != null)
                    {
                        yield return value;
                        if (--count == 0) break;
                    }
                }
            }
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="identity">数据自增ID</param>
        /// <returns>数据</returns>
        public TValueType this[int identity]
        {
            get
            {
                TValueType[] values = this.values;
                return (uint)identity < (uint)values.Length ? values[identity] : null;
            }
        }

        /// <summary>
        /// SQL操作缓存
        /// </summary>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="baseIdentity">基础ID</param>
        /// <param name="group">数据分组</param>
        /// <param name="isEvent">是否绑定更新事件</param>
        protected TmphIdentityCache(Laurent.Lee.CLB.Emit.TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, int group, int baseIdentity, bool isEvent)
            : base(sqlTool, group, baseIdentity)
        {
            if (isEvent)
            {
                sqlTool.OnUpdatedLock += onUpdated;
            }
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="value">更新后的数据</param>
        /// <param name="oldValue">更新前的数据</param>
        /// <param name="memberMap">更新成员位图</param>
        protected void onUpdated(TValueType value, TValueType oldValue, Laurent.Lee.CLB.Code.TmphMemberMap memberMap)
        {
            TValueType TCacheValue = values[getIdentity(value)];
            using (Laurent.Lee.CLB.Code.TmphMemberMap newMemberMap = updateMemberMap(memberMap))
            {
                update(TCacheValue, value, oldValue, newMemberMap);
                callOnUpdated(TCacheValue, oldValue);
            }
        }
    }
}