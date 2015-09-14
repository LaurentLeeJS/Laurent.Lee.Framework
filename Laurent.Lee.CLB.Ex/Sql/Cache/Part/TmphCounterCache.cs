using System;
using Laurent.Lee.CLB.Code.CSharp;

namespace Laurent.Lee.CLB.Sql.Cache.Part
{
    /// <summary>
    /// 计数缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    public abstract class TmphCounterCache<TValueType, TModelType, TKeyType>
        where TValueType : class, TModelType
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        /// 缓存计数器
        /// </summary>
        protected Events.TmphCounter<TValueType, TModelType, TKeyType> counter;
        /// <summary>
        /// 计数缓存
        /// </summary>
        /// <param name="counter">缓存计数器</param>
        protected TmphCounterCache(Events.TmphCounter<TValueType, TModelType, TKeyType> counter)
        {
            if (counter == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            this.counter = counter;
        }
    }
}
