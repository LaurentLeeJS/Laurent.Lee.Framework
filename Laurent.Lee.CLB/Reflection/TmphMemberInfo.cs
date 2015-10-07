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
using System.Reflection;

namespace Laurent.Lee.CLB.Reflection
{
    /// <summary>
    ///     成员属性相关操作
    /// </summary>
    public static class TmphMemberInfo
    {
        /// <summary>
        ///     根据成员属性获取自定义属性
        /// </summary>
        /// <typeparam name="TAttributeType">自定义属性类型</typeparam>
        /// <param name="member">成员属性</param>
        /// <param name="isBaseType">是否搜索父类属性</param>
        /// <returns>自定义属性</returns>
        public static TAttributeType CustomAttribute<TAttributeType>(this MemberInfo member, bool isBaseType = false)
            where TAttributeType : Attribute
        {
            TAttributeType value = null;
            if (member != null)
            {
                foreach (var attribute in member.GetCustomAttributes(typeof(TAttributeType), isBaseType))
                {
                    if (attribute.GetType() == typeof(TAttributeType)) return attribute as TAttributeType;
                }
            }
            return value;
        }
    }
}