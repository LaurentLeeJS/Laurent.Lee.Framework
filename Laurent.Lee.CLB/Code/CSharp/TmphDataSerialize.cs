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
using Laurent.Lee.CLB.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
using TmphDataSerialize = Laurent.Lee.CLB.Code.CSharp.TmphDataSerialize;

namespace Laurent.Lee.CLB.Code.CSharp
{
    /// <summary>
    ///     序列化代码生成自定义属性
    /// </summary>
    public class TmphDataSerialize : TmphMemberFilter.TmphPublicInstanceField
    {
        /// <summary>
        ///     固定类型字节数
        /// </summary>
        internal static readonly Dictionary<Type, byte> FixedSizes;

        /// <summary>
        ///     是否序列化成员位图
        /// </summary>
        public bool IsMemberMap;

        static unsafe TmphDataSerialize()
        {
            FixedSizes = TmphDictionary.CreateOnly<Type, byte>();
            FixedSizes.Add(typeof(bool), sizeof(bool));
            FixedSizes.Add(typeof(byte), sizeof(byte));
            FixedSizes.Add(typeof(sbyte), sizeof(sbyte));
            FixedSizes.Add(typeof(short), sizeof(short));
            FixedSizes.Add(typeof(ushort), sizeof(ushort));
            FixedSizes.Add(typeof(int), sizeof(int));
            FixedSizes.Add(typeof(uint), sizeof(uint));
            FixedSizes.Add(typeof(long), sizeof(long));
            FixedSizes.Add(typeof(ulong), sizeof(ulong));
            FixedSizes.Add(typeof(char), sizeof(char));
            FixedSizes.Add(typeof(DateTime), sizeof(long));
            FixedSizes.Add(typeof(float), sizeof(float));
            FixedSizes.Add(typeof(double), sizeof(double));
            FixedSizes.Add(typeof(decimal), sizeof(decimal));
            FixedSizes.Add(typeof(Guid), (byte)sizeof(Guid));
        }

        /// <summary>
        ///     获取字段成员集合
        /// </summary>
        /// <param name="type"></param>
        /// <param name="attribute"></param>
        /// <param name="memberCountVerify"></param>
        /// <param name="fixedSize"></param>
        /// <param name="nullMapSize"></param>
        /// <returns>字段成员集合</returns>
        public static TmphSubArray<TmphMemberInfo> GetFields(Type type, TmphDataSerialize attribute, out int memberCountVerify,
            out int fixedSize, out int nullMapSize)
        {
            var fieldIndexs =
                (TmphFieldIndex[])
                    typeof(TmphMemberIndexGroup<>).MakeGenericType(type)
                        .GetMethod("GetFields", BindingFlags.Static | BindingFlags.Public)
                        .Invoke(null, new object[] { attribute.MemberFilter });
            var fields = new TmphSubArray<TmphMemberInfo>(fieldIndexs.Length);
            var nullMapIndex = 0;
            fixedSize = nullMapSize = 0;
            foreach (var field in fieldIndexs)
            {
                type = field.Member.FieldType;
                if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !field.IsIgnore)
                {
                    var memberAttribute = field.GetAttribute<TmphBinarySerialize.TmphMember>(true, true);
                    if (memberAttribute == null || memberAttribute.IsSetup)
                    {
                        var value = TmphMemberInfo.GetSerialize(field);
                        if (type != typeof(bool)) fixedSize += value.SerializeFixedSize;
                        nullMapSize += value.NullMapSize;
                        if (value.NullMapSize == 2)
                        {
                            value.SerializeNullMapIndex = nullMapIndex;
                            nullMapIndex += 2;
                            --fixedSize;
                        }
                        fields.Add(value);
                    }
                }
            }
            memberCountVerify = fields.Count + 0x40000000;
            fixedSize = (fixedSize + 3) & (int.MaxValue - 3);
            nullMapSize = ((nullMapSize + 31) >> 5) << 2;
            fields.Sort(TmphMemberInfo.SerializeFixedSizeSort);
            foreach (var value in fields)
            {
                if (value.NullMapSize == 1) value.SerializeNullMapIndex = nullMapIndex++;
            }
            return fields;
        }

        /// <summary>
        ///     序列化接口
        /// </summary>
        public interface ISerialize
        {
            /// <summary>
            ///     对象序列化
            /// </summary>
            /// <param name="serializer">对象序列化器</param>
            void Serialize(TmphDataSerializer serializer);

            /// <summary>
            ///     反序列化
            /// </summary>
            /// <param name="deSerializer">对象反序列化器</param>
            void DeSerialize(TmphDataDeSerializer deSerializer);
        }
    }
}

namespace Laurent.Lee.CLB.Code
{
    /// <summary>
    ///     成员信息
    /// </summary>
    public partial class TmphMemberInfo
    {
        /// <summary>
        ///     空值位图位数
        /// </summary>
        internal byte NullMapSize;

        /// <summary>
        ///     基本序列化字节数
        /// </summary>
        public byte SerializeFixedSize;

        /// <summary>
        ///     空值位图索引
        /// </summary>
        public int SerializeNullMapIndex;

        /// <summary>
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        internal static TmphMemberInfo GetSerialize(TmphFieldIndex field)
        {
            var member = new TmphMemberInfo(field);
            var type = field.Member.FieldType;
            if (type.IsEnum)
                TmphDataSerialize.FixedSizes.TryGetValue(type.GetEnumUnderlyingType(), out member.SerializeFixedSize);
            else if (type.IsValueType)
            {
                var nullType = type.nullableType();
                if (nullType == null)
                {
                    TmphDataSerialize.FixedSizes.TryGetValue(type, out member.SerializeFixedSize);
                    if (type == typeof(bool)) member.NullMapSize = 1;
                }
                else
                {
                    member.NullMapSize = type == typeof(bool?) ? (byte)2 : (byte)1;
                    TmphDataSerialize.FixedSizes.TryGetValue(nullType, out member.SerializeFixedSize);
                }
            }
            else member.NullMapSize = 1;
            return member;
        }

        /// <summary>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        internal static int SerializeFixedSizeSort(TmphMemberInfo left, TmphMemberInfo right)
        {
            return (int)(right.SerializeFixedSize & (0U - right.SerializeFixedSize)) -
                   (int)(left.SerializeFixedSize & (0U - left.SerializeFixedSize));
        }
    }
}