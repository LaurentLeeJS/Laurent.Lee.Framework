/*
 *  Copyright 2015 Tony Lee

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 */

using Laurent.Lee.CLB.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Laurent.Lee.CLB.Code
{
    /// <summary>
    ///     成员索引分组
    /// </summary>
    public struct TmphMemberIndexGroup
    {
        /// <summary>
        ///     成员索引分组集合
        /// </summary>
        private static readonly Dictionary<Type, TmphMemberIndexGroup> Cache =
            TmphDictionary.CreateOnly<Type, TmphMemberIndexGroup>();

        /// <summary>
        ///     成员索引分组集合访问锁
        /// </summary>
        private static readonly object CacheLock = new object();

        /// <summary>
        ///     所有成员数量
        /// </summary>
        public readonly int MemberCount;

        /// <summary>
        ///     非公有动态字段
        /// </summary>
        internal readonly TmphFieldIndex[] NonPublicFields;

        /// <summary>
        ///     非公有动态属性
        /// </summary>
        internal readonly TmphPropertyIndex[] NonPublicProperties;

        /// <summary>
        ///     公有动态字段
        /// </summary>
        internal readonly TmphFieldIndex[] PublicFields;

        /// <summary>
        ///     公有动态属性
        /// </summary>
        internal readonly TmphPropertyIndex[] PublicProperties;

        /// <summary>
        ///     成员索引分组
        /// </summary>
        /// <param name="type">对象类型</param>
        private TmphMemberIndexGroup(Type type)
        {
            var index = 0;
            if (type.IsEnum)
            {
                PublicFields =
                    type.GetFields(BindingFlags.Public | BindingFlags.Static)
                        .getArray(member => new TmphFieldIndex(member, TmphMemberFilters.PublicStaticField, index++));
                NonPublicFields = TmphNullValue<TmphFieldIndex>.Array;
                PublicProperties = NonPublicProperties = TmphNullValue<TmphPropertyIndex>.Array;
            }
            else
            {
                var group = new TmphMemberIndex.TmphGroup(type);
                if (type.getTypeName() == null)
                {
                    PublicFields =
                        group.PublicFields.sort(
                            (left, right) => string.Compare(left.Name, right.Name, StringComparison.Ordinal))
                            .getArray(value => new TmphFieldIndex(value, TmphMemberFilters.PublicInstanceField, index++));
                    NonPublicFields =
                        group.NonPublicFields.sort(
                            (left, right) => string.Compare(left.Name, right.Name, StringComparison.Ordinal))
                            .getArray(value => new TmphFieldIndex(value, TmphMemberFilters.NonPublicInstanceField, index++));
                    PublicProperties =
                        group.PublicProperties.sort(
                            (left, right) => string.Compare(left.Name, right.Name, StringComparison.Ordinal))
                            .getArray(
                                value => new TmphPropertyIndex(value, TmphMemberFilters.PublicInstanceProperty, index++));
                    NonPublicProperties =
                        group.NonPublicProperties.sort(
                            (left, right) => string.Compare(left.Name, right.Name, StringComparison.Ordinal))
                            .getArray(
                                value => new TmphPropertyIndex(value, TmphMemberFilters.NonPublicInstanceProperty, index++));
                }
                else
                {
                    PublicFields = NonPublicFields = TmphNullValue<TmphFieldIndex>.Array;
                    PublicProperties = NonPublicProperties = TmphNullValue<TmphPropertyIndex>.Array;
                }
            }
            MemberCount = index;
        }

        /// <summary>
        ///     获取成员索引集合
        /// </summary>
        /// <param name="isValue">成员匹配委托</param>
        /// <returns>成员索引集合</returns>
        private TmphMemberIndex[] Get(Func<TmphMemberIndex, bool> isValue)
        {
            return TmphArray.concat(PublicFields.getFindArray(isValue), NonPublicFields.getFindArray(isValue),
                PublicProperties.getFindArray(isValue), NonPublicProperties.getFindArray(isValue));
        }

        /// <summary>
        ///     根据类型获取成员信息集合
        /// </summary>
        /// <param name="filter">选择类型</param>
        /// <returns>成员信息集合</returns>
        public TmphMemberIndex[] Find(TmphMemberFilters filter)
        {
            return Get(value => (value.Filter & filter) != 0);
        }

        ///// <summary>
        ///// 根据类型获取成员信息集合
        ///// </summary>
        ///// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        ///// <param name="filter">成员选择</param>
        ///// <returns>成员信息集合</returns>
        //internal memberIndex[] Find<TAttributeType>(TAttributeType filter) where TAttributeType : Laurent.Lee.CLB.Code.memberFilter
        //{
        //    return Find(filter.MemberFilter).getFindArray(value => filter.IsAttribute ? value.IsAttribute<TAttributeType>(filter.IsBaseTypeAttribute, filter.IsInheritAttribute) : !value.IsIgnoreAttribute<TAttributeType>(filter.IsBaseTypeAttribute, filter.IsInheritAttribute));
        //}
        /// <summary>
        ///     根据类型获取成员信息集合
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="filter">成员选择</param>
        /// <returns>成员信息集合</returns>
        internal TmphMemberIndex[] Find<TAttributeType>(TmphMemberFilter filter) where TAttributeType : TmphIgnoreMember
        {
            return
                Find(filter.MemberFilter)
                    .getFindArray(
                        value =>
                            filter.IsAttribute
                                ? value.IsAttribute<TAttributeType>(filter.IsBaseTypeAttribute,
                                    filter.IsInheritAttribute)
                                : !value.IsIgnoreAttribute<TAttributeType>(filter.IsBaseTypeAttribute,
                                    filter.IsInheritAttribute));
        }

        /// <summary>
        ///     根据类型获取成员索引分组
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <returns>成员索引分组</returns>
        public static TmphMemberIndexGroup Get(Type type)
        {
            TmphMemberIndexGroup value;
            Monitor.Enter(CacheLock);
            try
            {
                if (!Cache.TryGetValue(type, out value)) Cache.Add(type, value = new TmphMemberIndexGroup(type));
            }
            finally
            {
                Monitor.Exit(CacheLock);
            }
            return value;
        }
    }

    /// <summary>
    ///     成员索引分组
    /// </summary>
    /// <typeparam name="TValueType">对象类型</typeparam>
    internal static class TmphMemberIndexGroup<TValueType>
    {
        /// <summary>
        ///     成员索引分组
        /// </summary>
        public static readonly TmphMemberIndexGroup Group = TmphMemberIndexGroup.Get(typeof(TValueType));

        /// <summary>
        ///     所有成员数量
        /// </summary>
        public static readonly int MemberCount = Group.MemberCount;

        /// <summary>
        ///     字段成员数量
        /// </summary>
        public static readonly int FieldCount = Group.PublicFields.Length + Group.NonPublicFields.Length;

        /// <summary>
        ///     成员集合
        /// </summary>
        public static TmphMemberIndex[] GetAllMembers()
        {
            var members = new TmphSubArray<TmphMemberIndex>(MemberCount);
            members.Add(Group.PublicFields.ToGeneric<TmphMemberIndex>());
            members.Add(Group.NonPublicFields.ToGeneric<TmphMemberIndex>());
            members.Add(Group.PublicProperties.ToGeneric<TmphMemberIndex>());
            members.Add(Group.NonPublicProperties.ToGeneric<TmphMemberIndex>());
            return members.ToArray();
        }

        /// <summary>
        ///     获取字段集合
        /// </summary>
        /// <param name="memberFilter">成员选择类型</param>
        /// <returns></returns>
        public static TmphFieldIndex[] GetFields(TmphMemberFilters memberFilter = TmphMemberFilters.InstanceField)
        {
            if ((memberFilter & TmphMemberFilters.PublicInstanceField) == 0)
            {
                if ((memberFilter & TmphMemberFilters.NonPublicInstanceField) == 0)
                    return TmphNullValue<TmphFieldIndex>.Array;
                return Group.NonPublicFields;
            }
            if ((memberFilter & TmphMemberFilters.NonPublicInstanceField) == 0) return Group.PublicFields;
            return Group.PublicFields.concat(Group.NonPublicFields);
        }

        /// <summary>
        ///     获取属性集合
        /// </summary>
        /// <param name="memberFilter">成员选择类型</param>
        /// <returns></returns>
        public static TmphPropertyIndex[] GetProperties(TmphMemberFilters memberFilter = TmphMemberFilters.InstanceField)
        {
            if ((memberFilter & TmphMemberFilters.PublicInstanceProperty) == 0)
            {
                if ((memberFilter & TmphMemberFilters.NonPublicInstanceProperty) == 0)
                    return TmphNullValue<TmphPropertyIndex>.Array;
                return Group.NonPublicProperties;
            }
            if ((memberFilter & TmphMemberFilters.NonPublicInstanceProperty) == 0) return Group.PublicProperties;
            return Group.PublicProperties.concat(Group.NonPublicProperties);
        }
    }
}