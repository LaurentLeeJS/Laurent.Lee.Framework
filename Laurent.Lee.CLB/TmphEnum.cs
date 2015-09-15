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

using Laurent.Lee.CLB.Config;
using Laurent.Lee.CLB.Reflection;
using System;
using System.Reflection;

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     枚举相关操作
    /// </summary>
    public static class TmphEnum
    {
        /// <summary>
        ///     默认枚举值
        /// </summary>
        public const int DefaultEnumValue = -1;

        /// <summary>
        ///     获取最大枚举值
        /// </summary>
        /// <typeparam name="TEnumType">枚举类型</typeparam>
        /// <param name="nullValue">默认空值</param>
        /// <returns>最大枚举值,失败返回默认空值</returns>
        public static int GetMaxValue<TEnumType>(int nullValue) where TEnumType : IConvertible
        {
            var type = typeof(TEnumType);
            var isEnum = type.IsEnum;
            if (isEnum)
            {
                var values = Enum.GetValues(type).toArray<TEnumType>();
                if (values.Length != 0)
                {
                    var maxValue = int.MinValue;
                    foreach (var value in Enum.GetValues(type).toArray<TEnumType>())
                    {
                        var intValue = value.ToInt32(null);
                        if (intValue > maxValue) maxValue = intValue;
                    }
                    return maxValue;
                }
            }
            return nullValue;
        }

        /// <summary>
        ///     获取枚举数组
        /// </summary>
        /// <typeparam name="TEnumType">枚举类型</typeparam>
        /// <returns>枚举数组</returns>
        public static TEnumType[] Array<TEnumType>()
        {
            var array = Enum.GetValues(typeof(TEnumType));
            var values = new TEnumType[array.Length];
            var count = 0;
            foreach (TEnumType value in array) values[count++] = value;
            return values;
        }

        /// <summary>
        ///     获取枚举属性集合
        /// </summary>
        /// <typeparam name="TEnumType">枚举类型</typeparam>
        /// <typeparam name="TAttributeType">属性类型</typeparam>
        /// <returns>枚举属性集合</returns>
        public static TAttributeType[] GetAttributes<TEnumType, TAttributeType>()
            where TEnumType : IConvertible
            where TAttributeType : Attribute
        {
            var length = GetMaxValue<TEnumType>(-1) + 1;
            if (length != 0)
            {
                if (length >= Config.TmphPub.Default.MaxEnumArraySize)
                    TmphLog.Error.Add(typeof(TEnumType) + " 枚举数组过大 " + length.toString(), false, false);
                int index;
                var names = new TAttributeType[length];
                var enumAttributeType = typeof(TAttributeType);
                foreach (var field in typeof(TEnumType).GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    var attribute = TmphMemberInfo.CustomAttribute<TAttributeType>(field);
                    if (attribute != null && (index = ((IConvertible)field.GetValue(null)).ToInt32(null)) < length)
                        names[index] = attribute;
                }
                return names;
            }
            return null;
        }
    }

    /// <summary>
    ///     枚举属性获取器
    /// </summary>
    /// <typeparam name="TEnumType">枚举类型</typeparam>
    /// <typeparam name="TAttributeType">属性类型</typeparam>
    public static class TmphEnum<TEnumType, TAttributeType>
        where TEnumType : IConvertible
        where TAttributeType : Attribute
    {
        /// <summary>
        ///     属性集合
        /// </summary>
        private static TAttributeType[] attributeArray;

        /// <summary>
        ///     属性集合
        /// </summary>
        private static TmphStaticDictionary<int, TAttributeType> attributeDictionary;

        static TmphEnum()
        {
            if (TmphAppSetting.IsCheckMemory) TmphCheckMemory.Add(MethodBase.GetCurrentMethod().DeclaringType);
        }

        /// <summary>
        ///     属性集合
        /// </summary>
        internal static TAttributeType[] AttributeArray
        {
            get
            {
                if (attributeArray == null) attributeArray = TmphEnum.GetAttributes<TEnumType, TAttributeType>();
                return attributeArray;
            }
        }

        /// <summary>
        ///     属性集合
        /// </summary>
        internal static TmphStaticDictionary<int, TAttributeType> AttributeDictionary
        {
            get
            {
                if (attributeDictionary == null)
                {
                    var attributes = new TmphSubArray<TmphKeyValue<int, TAttributeType>>();
                    foreach (var field in typeof(TEnumType).GetFields(BindingFlags.Public | BindingFlags.Static))
                    {
                        TAttributeType attribute = Reflection.TmphMemberInfo.CustomAttribute<TAttributeType>(field);
                        if (attribute != null)
                            attributes.Add(
                                new TmphKeyValue<int, TAttributeType>(((TEnumType)field.GetValue(null)).ToInt32(null),
                                    attribute));
                    }
                    attributeDictionary = new TmphStaticDictionary<int, TAttributeType>(attributes.ToArray());
                }
                return attributeDictionary;
            }
        }

        /// <summary>
        ///     根据索引获取属性
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>属性</returns>
        public static TAttributeType Array(int index)
        {
            return AttributeArray.get(index, null);
        }

        /// <summary>
        ///     根据索引获取属性
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>属性</returns>
        public static TAttributeType Array(TEnumType index)
        {
            return AttributeArray.get(index.ToInt32(null), null);
        }

        /// <summary>
        ///     匹配自定义属性获取枚举集合
        /// </summary>
        /// <param name="isValue">自定义属性匹配器</param>
        /// <returns>枚举集合</returns>
        public static TmphSubArray<TEnumType> SubArray(Func<TAttributeType, bool> isValue)
        {
            var enums = Enum.GetValues(typeof(TEnumType));
            var values = new TEnumType[enums.Length];
            var count = 0;
            foreach (TEnumType value in enums)
            {
                if (isValue(AttributeArray.get(value.ToInt32(null), null))) values[count++] = value;
            }
            return TmphSubArray<TEnumType>.Unsafe(values, 0, count);
        }

        /// <summary>
        ///     根据索引获取属性
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>属性</returns>
        public static TAttributeType Dictionary(TEnumType type)
        {
            return AttributeDictionary.Get(type.ToInt32(null), null);
        }
    }
}