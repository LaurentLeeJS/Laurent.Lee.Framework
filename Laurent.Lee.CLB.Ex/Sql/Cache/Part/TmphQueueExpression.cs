using System;
using System.Linq.Expressions;
using Laurent.Lee.CLB.Code.CSharp;

namespace Laurent.Lee.CLB.Sql.Cache.Part
{
    /// <summary>
    /// 先进先出优先队列缓存
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TCounterKeyType">缓存统计关键字类型</typeparam>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    /// <typeparam name="TCacheValueType">缓存数据类型</typeparam>
    public abstract class TmphQueueExpression<TValueType, TModelType, TCounterKeyType, TKeyType, TCacheValueType>
        : TmphQueue<TValueType, TModelType, TCounterKeyType, TKeyType, TCacheValueType>
        where TValueType : class, TModelType
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
        where TCounterKeyType : IEquatable<TCounterKeyType>
        where TCacheValueType : class
    {
        /// <summary>
        /// 条件表达式获取器
        /// </summary>
        protected Func<TKeyType, Expression<Func<TModelType, bool>>> getWhere;
        /// <summary>
        /// 先进先出优先队列缓存
        /// </summary>
        /// <param name="counter">缓存计数器</param>
        /// <param name="getKey">缓存关键字获取器</param>
        /// <param name="maxCount">缓存默认最大容器大小</param>
        /// <param name="where">条件表达式</param>
        protected TmphQueueExpression(Events.TmphCounter<TValueType, TModelType, TCounterKeyType> counter, Expression<Func<TModelType, TKeyType>> getKey, int maxCount
            , Func<TKeyType, Expression<Func<TModelType, bool>>> getWhere)
            : base(counter, getKey, maxCount)
        {
            if (getWhere == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            this.getWhere = getWhere;
        }
    }
}
