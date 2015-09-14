using Laurent.Lee.CLB.MemoryDataBase.Cache;
using System;
using System.Linq.Expressions;

namespace Laurent.Lee.CLB.MemoryDatabase.Cache.Index
{
    /// <summary>
    /// 成员绑定缓存
    /// </summary>
    /// <typeparam name="TValueType"></typeparam>
    /// <typeparam name="TModelType"></typeparam>
    /// <typeparam name="TKeyType"></typeparam>
    /// <typeparam name="TTargetType"></typeparam>
    /// <typeparam name="TCacheType"></typeparam>
    public abstract class TmphMember<TValueType, TModelType, TKeyType, TTargetType, TCacheType>
        where TValueType : class, TModelType
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
        where TTargetType : class
        where TCacheType : class
    {
        /// <summary>
        /// 整表缓存
        /// </summary>
        protected TmphILoadCache<TValueType, TModelType> cache;
        /// <summary>
        /// 分组字典关键字获取器
        /// </summary>
        protected Func<TModelType, TKeyType> getKey;
        /// <summary>
        /// 获取缓存目标对象
        /// </summary>
        protected Func<TKeyType, TTargetType> getValue;
        /// <summary>
        /// 获取缓存委托
        /// </summary>
        protected Func<TTargetType, TCacheType> getMember;
        /// <summary>
        /// 设置缓存委托
        /// </summary>
        protected Action<TTargetType, TCacheType> setMember;
        /// <summary>
        /// 分组列表缓存
        /// </summary>
        /// <param name="cache">整表缓存</param>
        /// <param name="getKey">分组字典关键字获取器</param>
        /// <param name="getValue">获取目标对象委托</param>
        /// <param name="member">缓存字段表达式</param>
        public TmphMember(TmphILoadCache<TValueType, TModelType> cache, Func<TModelType, TKeyType> getKey, Func<TKeyType, TTargetType> getValue, Expression<Func<TTargetType, TCacheType>> member)
        {
            if (cache == null || getKey == null || getValue == null || member == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            Sql.TmphMemberExpression<TTargetType, TCacheType> expression = new Sql.TmphMemberExpression<TTargetType, TCacheType>(member);
            if (expression.Field == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
            this.cache = cache;
            this.getKey = getKey;
            this.getValue = getValue;
            getMember = expression.GetMember;
            setMember = expression.SetMember;
        }
    }
}