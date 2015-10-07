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

using System.Reflection;

namespace Laurent.Lee.CLB.Code
{
    /// <summary>
    ///     属性索引
    /// </summary>
    internal sealed class TmphPropertyIndex : TmphMemberIndex<PropertyInfo>
    {
        /// <summary>
        ///     属性信息
        /// </summary>
        /// <param name="property">属性信息</param>
        /// <param name="filter">选择类型</param>
        /// <param name="index">成员编号</param>
        public TmphPropertyIndex(PropertyInfo property, TmphMemberFilters filter, int index)
            : base(property, filter, index)
        {
            CanSet = property.CanWrite;
            CanGet = property.CanRead;
            Type = property.PropertyType;
        }
    }
}