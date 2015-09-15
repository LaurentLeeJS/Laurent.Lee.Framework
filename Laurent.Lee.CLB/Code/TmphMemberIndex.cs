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
using System.Collections.Generic;
using System.Reflection;

namespace Laurent.Lee.CLB.Code
{
    /// <summary>
    ///     成员索引
    /// </summary>
    public abstract class TmphMemberIndex
    {
        /// <summary>
        ///     自定义属性集合
        /// </summary>
        private object[] _attributes;

        /// <summary>
        ///     自定义属性集合(包括基类成员属性)
        /// </summary>
        private object[] _baseAttributes;

        /// <summary>
        ///     是否忽略该成员
        /// </summary>
        private bool? _isIgnore;

        /// <summary>
        ///     选择类型
        /// </summary>
        internal TmphMemberFilters Filter;

        /// <summary>
        ///     是否字段
        /// </summary>
        internal bool IsField;

        /// <summary>
        ///     成员信息
        /// </summary>
        /// <param name="member">成员信息</param>
        protected TmphMemberIndex(TmphMemberIndex member)
        {
            Member = member.Member;
            Type = member.Type;
            MemberIndex = member.MemberIndex;
            Filter = member.Filter;
            IsField = member.IsField;
            CanSet = member.CanSet;
            CanGet = member.CanGet;
            _isIgnore = member._isIgnore;
        }

        /// <summary>
        ///     成员信息
        /// </summary>
        /// <param name="member">成员信息</param>
        /// <param name="filter">选择类型</param>
        /// <param name="index">成员编号</param>
        protected TmphMemberIndex(MemberInfo member, TmphMemberFilters filter, int index)
        {
            Member = member;
            MemberIndex = index;
            Filter = filter;
        }

        /// <summary>
        ///     成员信息
        /// </summary>
        /// <param name="index">成员编号</param>
        protected TmphMemberIndex(int index)
        {
            MemberIndex = index;
            IsField = CanSet = CanSet = true;
            Filter = TmphMemberFilters.PublicInstance;
        }

        /// <summary>
        ///     成员信息
        /// </summary>
        public MemberInfo Member { get; protected set; }

        /// <summary>
        ///     成员类型
        /// </summary>
        public Type Type { get; protected set; }

        /// <summary>
        ///     成员编号
        /// </summary>
        public int MemberIndex { get; protected set; }

        /// <summary>
        ///     是否可赋值
        /// </summary>
        public bool CanSet { get; protected set; }

        /// <summary>
        ///     是否可读取
        /// </summary>
        public bool CanGet { get; protected set; }

        /// <summary>
        ///     是否忽略该成员
        /// </summary>
        public bool IsIgnore
        {
            get
            {
                if (_isIgnore == null) _isIgnore = Member != null && GetAttribute<TmphIgnore>(true, false) != null;
                return (bool)_isIgnore;
            }
        }

        /// <summary>
        ///     获取自定义属性集合
        /// </summary>
        /// <typeparam name="TAttributeType"></typeparam>
        /// <param name="isBaseType">是否搜索父类属性</param>
        /// <returns></returns>
        internal IEnumerable<TAttributeType> Attributes<TAttributeType>(bool isBaseType)
            where TAttributeType : Attribute
        {
            if (Member != null)
            {
                object[] values;
                if (isBaseType)
                {
                    if (_baseAttributes == null)
                    {
                        _baseAttributes = Member.GetCustomAttributes(true);
                        if (_baseAttributes.Length == 0) _attributes = _baseAttributes;
                    }
                    values = _baseAttributes;
                }
                else
                {
                    if (_attributes == null) _attributes = Member.GetCustomAttributes(false);
                    values = _attributes;
                }
                foreach (var value in values)
                {
                    if (typeof(TAttributeType).IsAssignableFrom(value.GetType())) yield return (TAttributeType)value;
                }
            }
        }

        /// <summary>
        ///     根据成员属性获取自定义属性
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="isBaseType">是否搜索父类属性</param>
        /// <param name="isInheritAttribute">是否包含继承属性</param>
        /// <returns>自定义属性</returns>
        internal TAttributeType GetAttribute<TAttributeType>(bool isBaseType, bool isInheritAttribute)
            where TAttributeType : Attribute
        {
            TAttributeType value = null;
            var minDepth = int.MaxValue;
            foreach (var attribute in Attributes<TAttributeType>(isBaseType))
            {
                if (isInheritAttribute)
                {
                    var depth = 0;
                    for (var type = attribute.GetType(); type != typeof(TAttributeType); type = type.BaseType) ++depth;
                    if (depth < minDepth)
                    {
                        if (depth == 0) return attribute;
                        minDepth = depth;
                        value = attribute;
                    }
                }
                else if (attribute.GetType() == typeof(TAttributeType)) return attribute;
            }
            return value;
        }

        /// <summary>
        ///     获取自定义属性
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="isBaseType">是否搜索父类属性</param>
        /// <param name="isInheritAttribute">是否包含继承属性</param>
        /// <returns>自定义属性,失败返回null</returns>
        public TAttributeType GetSetupAttribute<TAttributeType>(bool isBaseType, bool isInheritAttribute)
            where TAttributeType : TmphIgnoreMember
        {
            if (!IsIgnore)
            {
                var value = GetAttribute<TAttributeType>(isBaseType, isInheritAttribute);
                if (value != null && value.IsSetup) return value;
            }
            return null;
        }

        /// <summary>
        ///     判断是否存在自定义属性
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="isBaseType">是否搜索父类属性</param>
        /// <param name="isInheritAttribute">是否包含继承属性</param>
        /// <returns>是否存在自定义属性</returns>
        internal bool IsAttribute<TAttributeType>(bool isBaseType, bool isInheritAttribute)
            where TAttributeType : TmphIgnoreMember
        {
            return GetSetupAttribute<TAttributeType>(isBaseType, isInheritAttribute) != null;
        }

        /// <summary>
        ///     判断是否忽略自定义属性
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="isBaseType">是否搜索父类属性</param>
        /// <param name="isInheritAttribute">是否包含继承属性</param>
        /// <returns>是否忽略自定义属性</returns>
        internal bool IsIgnoreAttribute<TAttributeType>(bool isBaseType, bool isInheritAttribute)
            where TAttributeType : TmphIgnoreMember
        {
            if (IsIgnore) return true;
            var value = GetAttribute<TAttributeType>(isBaseType, isInheritAttribute);
            return value != null && !value.IsSetup;
        }

        /// <summary>
        ///     根据类型获取成员信息集合
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <typeparam name="TMemberType"></typeparam>
        /// <param name="members">待匹配的成员信息集合</param>
        /// <param name="isAttribute">是否匹配自定义属性类型</param>
        /// <param name="isBaseType">是否搜索父类属性</param>
        /// <param name="isInheritAttribute">是否包含继承属性</param>
        /// <returns>成员信息集合</returns>
        protected static TMemberType[] Find<TMemberType, TAttributeType>(TMemberType[] members, bool isAttribute,
            bool isBaseType, bool isInheritAttribute)
            where TMemberType : TmphMemberIndex
            where TAttributeType : TmphIgnoreMember
        {
            return
                members.getFindArray(
                    value =>
                        isAttribute
                            ? value.IsAttribute<TAttributeType>(isBaseType, isInheritAttribute)
                            : !value.IsIgnoreAttribute<TAttributeType>(isBaseType, isInheritAttribute));
        }

        /// <summary>
        ///     动态成员分组
        /// </summary>
        internal struct TmphGroup
        {
            /// <summary>
            ///     非公有动态字段
            /// </summary>
            public FieldInfo[] NonPublicFields;

            /// <summary>
            ///     非公有动态属性
            /// </summary>
            public PropertyInfo[] NonPublicProperties;

            /// <summary>
            ///     公有动态字段
            /// </summary>
            public FieldInfo[] PublicFields;

            /// <summary>
            ///     公有动态属性
            /// </summary>
            public PropertyInfo[] PublicProperties;

            /// <summary>
            ///     动态成员分组
            /// </summary>
            /// <param name="type">目标类型</param>
            public TmphGroup(Type type)
            {
                var members = TmphDictionary.CreateHashString<TmphTypeDepth>();
                TmphTypeDepth oldMember;
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    var member = new TmphTypeDepth(type, field, true);
                    TmphHashString nameKey = field.Name;
                    if (!members.TryGetValue(nameKey, out oldMember) || member.Depth < oldMember.Depth)
                        members[nameKey] = member;
                }
                foreach (var field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (field.Name[0] != '<')
                    {
                        var member = new TmphTypeDepth(type, field, false);
                        TmphHashString nameKey = field.Name;
                        if (!members.TryGetValue(nameKey, out oldMember) || member.Depth < oldMember.Depth)
                            members[nameKey] = member;
                    }
                }
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var member = new TmphTypeDepth(type, property, true);
                    TmphHashString nameKey = property.Name;
                    if (!members.TryGetValue(nameKey, out oldMember) || member.Depth < oldMember.Depth)
                        members[nameKey] = member;
                }
                foreach (var property in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    var member = new TmphTypeDepth(type, property, false);
                    TmphHashString nameKey = property.Name;
                    if (!members.TryGetValue(nameKey, out oldMember) || member.Depth < oldMember.Depth)
                        members[nameKey] = member;
                }
                PublicFields = members.Values.GetArray(value => value.PublicField).getFindArray(value => value != null);
                NonPublicFields =
                    members.Values.GetArray(value => value.NonPublicField).getFindArray(value => value != null);
                PublicProperties =
                    members.Values.GetArray(value => value.PublicProperty).getFindArray(value => value != null);
                NonPublicProperties =
                    members.Values.GetArray(value => value.NonPublicProperty).getFindArray(value => value != null);
            }

            /// <summary>
            ///     类型深度
            /// </summary>
            private struct TmphTypeDepth
            {
                /// <summary>
                ///     是否字段
                /// </summary>
                private readonly bool _isField;

                /// <summary>
                ///     是否共有成员
                /// </summary>
                private readonly bool _isPublic;

                /// <summary>
                ///     成员信息
                /// </summary>
                private readonly MemberInfo _member;

                /// <summary>
                ///     类型深度
                /// </summary>
                public readonly int Depth;

                /// <summary>
                ///     类型深度
                /// </summary>
                /// <param name="type">类型</param>
                /// <param name="field">成员字段</param>
                /// <param name="isPublic">是否共有成员</param>
                public TmphTypeDepth(Type type, FieldInfo field, bool isPublic)
                {
                    var memberType = field.DeclaringType;
                    _member = field;
                    _isField = true;
                    _isPublic = isPublic;
                    for (Depth = 0; type != memberType; ++Depth) if (type != null) type = type.BaseType;
                }

                /// <summary>
                ///     类型深度
                /// </summary>
                /// <param name="type">类型</param>
                /// <param name="property">成员属性</param>
                /// <param name="isPublic">是否共有成员</param>
                public TmphTypeDepth(Type type, PropertyInfo property, bool isPublic)
                {
                    var memberType = property.DeclaringType;
                    _member = property;
                    _isField = false;
                    _isPublic = isPublic;
                    for (Depth = 0; type != memberType; ++Depth) if (type != null) type = type.BaseType;
                }

                /// <summary>
                ///     共有字段成员
                /// </summary>
                public FieldInfo PublicField
                {
                    get { return _isPublic && _isField ? (FieldInfo)_member : null; }
                }

                /// <summary>
                ///     非共有字段成员
                /// </summary>
                public FieldInfo NonPublicField
                {
                    get { return !_isPublic && _isField ? (FieldInfo)_member : null; }
                }

                /// <summary>
                ///     共有属性成员
                /// </summary>
                public PropertyInfo PublicProperty
                {
                    get { return _isPublic && !_isField ? (PropertyInfo)_member : null; }
                }

                /// <summary>
                ///     非共有属性成员
                /// </summary>
                public PropertyInfo NonPublicProperty
                {
                    get { return !_isPublic && !_isField ? (PropertyInfo)_member : null; }
                }
            }
        }
    }

    /// <summary>
    ///     成员索引
    /// </summary>
    /// <typeparam name="TMemberType">成员类型</typeparam>
    internal abstract class TmphMemberIndex<TMemberType> : TmphMemberIndex where TMemberType : MemberInfo
    {
        /// <summary>
        ///     成员信息
        /// </summary>
        public new TMemberType Member;

        /// <summary>
        ///     成员信息
        /// </summary>
        /// <param name="member">成员信息</param>
        /// <param name="filter">选择类型</param>
        /// <param name="index">成员编号</param>
        protected TmphMemberIndex(TMemberType member, TmphMemberFilters filter, int index)
            : base(member, filter, index)
        {
            Member = member;
        }
    }
}