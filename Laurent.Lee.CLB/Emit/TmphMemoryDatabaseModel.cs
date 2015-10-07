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