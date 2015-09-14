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
using System.Threading;

namespace Laurent.Lee.CLB.Sql.Cache
{
    /// <summary>
    /// SQL操作缓存
    /// </summary>
    /// <typeparam name="TValueType">表格类型</typeparam>
    /// <typeparam name="TModelType">模型类型</typeparam>
    public abstract class TmphCopy<TValueType, TModelType> : TmphSqlTool<TValueType, TModelType>
        where TValueType : class, TModelType
        where TModelType : class
    {
        /// <summary>
        /// 更新缓存数据
        /// </summary>
        /// <param name="value">缓存数据</param>
        /// <param name="newValue">更新后的新数据</param>
        /// <param name="memberMap">更新成员位图</param>
        protected void update(TValueType value, TValueType newValue, Laurent.Lee.CLB.Code.TmphMemberMap memberMap)
        {
            using (Laurent.Lee.CLB.Code.TmphMemberMap newMemberMap = updateMemberMap(memberMap))
            {
                if (!newMemberMap.IsDefault) Laurent.Lee.CLB.Emit.TmphMemberCopyer<TModelType>.Copy(value, newValue, newMemberMap);
            }
        }

        /// <summary>
        /// 更新缓存数据
        /// </summary>
        /// <param name="value">缓存数据</param>
        /// <param name="newValue">更新后的新数据</param>
        /// <param name="oldValue">更新前的数据</param>
        /// <param name="updateMemberMap">更新成员位图</param>
        protected void update(TValueType value, TValueType newValue, TValueType oldValue, Laurent.Lee.CLB.Code.TmphMemberMap updateMemberMap)
        {
            using (Laurent.Lee.CLB.Code.TmphMemberMap oldMemberMap = memberMap.Copy())
            {
                oldMemberMap.Xor(updateMemberMap);
                if (!oldMemberMap.IsDefault) Laurent.Lee.CLB.Emit.TmphMemberCopyer<TModelType>.Copy(oldValue, value, oldMemberMap);
                if (!updateMemberMap.IsDefault) Laurent.Lee.CLB.Emit.TmphMemberCopyer<TModelType>.Copy(value, newValue, updateMemberMap);
            }
        }

        /// <summary>
        /// SQL操作缓存
        /// </summary>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="group">数据分组</param>
        protected TmphCopy(Laurent.Lee.CLB.Emit.TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, int group)
            : base(sqlTool, group)
        {
        }

        /// <summary>
        /// 重新加载缓存事件
        /// </summary>
        public event Action OnReset;

        /// <summary>
        /// 重置缓存
        /// </summary>
        protected void resetLock()
        {
            Monitor.Enter(SqlTool.Lock);
            try
            {
                reset();
            }
            finally { Monitor.Exit(SqlTool.Lock); }
            if (OnReset != null) OnReset();
        }

        /// <summary>
        /// 重置缓存
        /// </summary>
        protected abstract void reset();
    }
}