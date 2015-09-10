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

using Laurent.Lee.CLB.Code;
using Laurent.Lee.CLB.Code.CSharp;
using Laurent.Lee.CLB.Config;
using System;
using System.Reflection;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    ///     内存数据库表格模型
    /// </summary>
    /// <typeparam name="TValueType"></typeparam>
    internal abstract class TmphMemoryDatabaseModel<TValueType> : TmphDatabaseModel<TValueType>
    {
        /// <summary>
        ///     内存数据库表格模型配置
        /// </summary>
        internal static readonly TmphMemoryDatabaseModel Attribute;

        /// <summary>
        ///     内存数据库基本成员集合
        /// </summary>
        internal static readonly TmphMemberMap MemberMap = TmphMemberMap<TValueType>.Empty();

        /// <summary>
        ///     是否所有成员
        /// </summary>
        internal static readonly int IsAllMember;

        /// <summary>
        ///     设置自增标识
        /// </summary>
        internal static readonly Action<TValueType, int> SetIdentity;

        /// <summary>
        ///     自增标识获取器
        /// </summary>
        internal static readonly Func<TValueType, int> GetIdentity;

        /// <summary>
        ///     自增字段
        /// </summary>
        internal static readonly TmphMemoryDatabaseModel.TmphFieldInfo Identity;

        /// <summary>
        ///     关键字集合
        /// </summary>
        internal static readonly TmphMemoryDatabaseModel.TmphFieldInfo[] PrimaryKeys;

        static TmphMemoryDatabaseModel()
        {
            var type = typeof(TValueType);
            Attribute = TmphTypeAttribute.GetAttribute<TmphMemoryDatabaseModel>(type, true, true) ??
                        TmphMemoryDatabaseModel.Default;
            var fieldArray = TmphMemberIndexGroup<TValueType>.GetFields(Attribute.MemberFilter);
            TmphSubArray<TmphMemoryDatabaseModel.TmphFieldInfo> fields = new TmphSubArray<TmphMemoryDatabaseModel.TmphFieldInfo>(),
                primaryKeys = new TmphSubArray<TmphMemoryDatabaseModel.TmphFieldInfo>();
            var identity = default(TmphMemoryDatabaseModel.TmphFieldInfo);
            int isCase = 0, isIdentity = 0;
            foreach (var field in fieldArray)
            {
                var memberType = field.Member.FieldType;
                if (!memberType.IsPointer && (!memberType.IsArray || memberType.GetArrayRank() == 1) && !field.IsIgnore)
                {
                    var memberAttribute = field.GetAttribute<TmphDataMember>(true, true) ?? TmphDataMember.NullDataMember;
                    if (memberAttribute.IsSetup)
                    {
                        fields.Add(new TmphMemoryDatabaseModel.TmphFieldInfo(field, memberAttribute));
                        MemberMap.SetMember(field.MemberIndex);
                        if (isIdentity == 0)
                        {
                            if (memberAttribute != null && memberAttribute.IsIdentity)
                            {
                                identity = new TmphMemoryDatabaseModel.TmphFieldInfo(field, memberAttribute);
                                isIdentity = 1;
                            }
                            else if (isCase == 0 && field.Member.Name == TmphMemoryDatabase.Default.DefaultIdentityName)
                            {
                                identity = new TmphMemoryDatabaseModel.TmphFieldInfo(field, memberAttribute);
                                isCase = 1;
                            }
                            else if (identity.Field == null &&
                                     field.Member.Name.ToLower() == TmphMemoryDatabase.Default.DefaultIdentityName)
                                identity = new TmphMemoryDatabaseModel.TmphFieldInfo(field, memberAttribute);
                        }
                        if (memberAttribute.PrimaryKeyIndex != 0)
                            primaryKeys.Add(new TmphMemoryDatabaseModel.TmphFieldInfo(field, memberAttribute));
                    }
                }
            }
            IsAllMember = fields.Count == fieldArray.Length ? 1 : 0;
            if ((Identity = identity).Field != null)
            {
                GetIdentity = getIdentityGetter32("GetMemoryDatabaseIdentity", identity.Field);
                SetIdentity = getIdentitySetter32("SetMemoryDatabaseIdentity", identity.Field);
            }
            PrimaryKeys = primaryKeys.ToArray();
        }

        /// <summary>
        ///     关键字集合
        /// </summary>
        internal static FieldInfo[] PrimaryKeyFields
        {
            get { return PrimaryKeys.getArray(value => value.Field); }
        }
    }
}