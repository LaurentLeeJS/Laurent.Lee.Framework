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

using Laurent.Lee.CLB.Emit;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Laurent.Lee.CLB.Code
{
    /// <summary>
    ///     成员信息
    /// </summary>
    public partial class TmphMemberInfo : TmphMemberIndex
    {
        /// <summary>
        ///     类型成员集合缓存
        /// </summary>
        private static readonly Dictionary<Type, TmphMemberInfo[]> MemberCache =
            TmphDictionary.CreateOnly<Type, TmphMemberInfo[]>();

        /// <summary>
        ///     成员信息
        /// </summary>
        /// <param name="type">成员类型</param>
        /// <param name="name">成员名称</param>
        /// <param name="index">成员编号</param>
        public TmphMemberInfo(TmphMemberType type, string name, int index)
            : base(index)
        {
            MemberType = type;
            MemberName = name;
        }

        /// <summary>
        ///     成员信息
        /// </summary>
        /// <param name="member">成员信息</param>
        internal TmphMemberInfo(TmphMemberIndex member)
            : base(member)
        {
            MemberType = member.Type;
            MemberName = Member.Name;
            if (CanGet && CanSet)
            {
                var sqlMember = GetAttribute<TmphDataMember>(true, false);
                if (sqlMember != null && sqlMember.DataType != null)
                    MemberType = new TmphMemberType(MemberType, sqlMember.DataType);
            }
        }

        /// <summary>
        ///     成员信息
        /// </summary>
        /// <param name="method">成员方法信息</param>
        /// <param name="filter">选择类型</param>
        protected TmphMemberInfo(MethodInfo method, TmphMemberFilters filter)
            : base(method, filter, 0)
        {
            Member = method;
            MemberName = method.Name;
        }

        /// <summary>
        ///     成员类型
        /// </summary>
        public TmphMemberType MemberType { get; private set; }

        /// <summary>
        ///     成员名称
        /// </summary>
        public string MemberName { get; private set; }

        /// <summary>
        ///     根据类型获取成员信息集合
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>成员信息集合</returns>
        private static TmphMemberInfo[] GetMembers(Type type)
        {
            TmphMemberInfo[] members;
            if (!MemberCache.TryGetValue(type, out members))
            {
                var group = TmphMemberIndexGroup.Get(type);
                MemberCache[type] = members =
                    TmphArray.concat(group.PublicFields.getArray(value => new TmphMemberInfo(value)),
                        group.NonPublicFields.getArray(value => new TmphMemberInfo(value)),
                        group.PublicProperties.getArray(value => new TmphMemberInfo(value)),
                        group.NonPublicProperties.getArray(value => new TmphMemberInfo(value)));
            }
            return members;
        }

        /// <summary>
        ///     根据类型获取成员信息集合
        /// </summary>
        /// <param name="type"></param>
        /// <param name="filter">选择类型</param>
        /// <returns>成员信息集合</returns>
        public static TmphMemberInfo[] GetMembers(Type type, TmphMemberFilters filter)
        {
            return GetMembers(type).getFindArray(value => (value.Filter & filter) != 0);
        }

        /// <summary>
        ///     根据类型获取成员信息集合
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="type"></param>
        /// <param name="filter">选择类型</param>
        /// <param name="isAttribute">是否匹配自定义属性类型</param>
        /// <param name="isBaseType">是否搜索父类属性</param>
        /// <param name="isInheritAttribute">是否包含继承属性</param>
        /// <returns>成员信息集合</returns>
        public static TmphMemberInfo[] GetMembers<TAttributeType>(Type type, TmphMemberFilters filter, bool isAttribute,
            bool isBaseType, bool isInheritAttribute)
            where TAttributeType : TmphIgnoreMember
        {
            return Find<TmphMemberInfo, TAttributeType>(GetMembers(type, filter), isAttribute, isBaseType,
                isInheritAttribute);
        }
    }

    /// <summary>
    ///     成员信息
    /// </summary>
    /// <typeparam name="TMemberType">成员类型</typeparam>
    internal abstract class TmphMemberInfo<TMemberType> : TmphMemberInfo where TMemberType : MemberInfo
    {
        /// <summary>
        ///     成员信息
        /// </summary>
        public new TMemberType Member;

        /// <summary>
        ///     成员信息
        /// </summary>
        /// <param name="member">成员信息</param>
        protected TmphMemberInfo(TmphMemberIndex<TMemberType> member)
            : base(member)
        {
            Member = member.Member;
        }
    }
}