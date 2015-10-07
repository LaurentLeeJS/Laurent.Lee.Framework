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

using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;

namespace Laurent.Lee.CLB.Code
{
    /// <summary>
    ///     类型自定义属性信息
    /// </summary>
    public static class TmphTypeAttribute
    {
        /// <summary>
        ///     自定义属性信息集合
        /// </summary>
        private static readonly TmphInterlocked.TmphDictionary<Type, object[]> Attributes =
            new TmphInterlocked.TmphDictionary<Type, object[]>(TmphDictionary.CreateOnly<Type, object[]>());

        /// <summary>
        ///     根据类型获取自定义属性信息集合
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <returns>自定义属性信息集合</returns>
        private static object[] Get(Type type)
        {
            object[] values;
            if (Attributes.TryGetValue(type, out values)) return values;
            Attributes.Set(type, values = type.GetCustomAttributes(false));
            return values;
        }

        /// <summary>
        ///     获取自定义属性集合
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <typeparam name="TAttributeType"></typeparam>
        /// <returns></returns>
        internal static IEnumerable<TAttributeType> GetAttributes<TAttributeType>(Type type)
            where TAttributeType : Attribute
        {
            foreach (var value in Get(type))
            {
                if (typeof(TAttributeType).IsAssignableFrom(value.GetType())) yield return (TAttributeType)value;
            }
        }

        /// <summary>
        ///     根据类型获取自定义属性
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="type">类型</param>
        /// <param name="isBaseType">是否搜索父类属性</param>
        /// <param name="isInheritAttribute">是否包含继承属性</param>
        /// <returns>自定义属性</returns>
        public static TAttributeType GetAttribute<TAttributeType>(Type type, bool isBaseType, bool isInheritAttribute)
            where TAttributeType : Attribute
        {
            while (type != null && type != typeof(object))
            {
                foreach (var attribute in GetAttributes<TAttributeType>(type))
                {
                    if (isInheritAttribute || attribute.GetType() == typeof(TAttributeType)) return attribute;
                }
                if (isBaseType) type = type.BaseType;
                else return null;
            }
            return null;
        }
    }
}