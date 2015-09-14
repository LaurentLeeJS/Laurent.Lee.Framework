using System;
using System.Reflection;
using Laurent.Lee.CLB.Code.CSharp;
using Laurent.Lee.CLB.Reflection;
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
