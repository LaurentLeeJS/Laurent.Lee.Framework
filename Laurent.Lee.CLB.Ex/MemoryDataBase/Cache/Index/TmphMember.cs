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