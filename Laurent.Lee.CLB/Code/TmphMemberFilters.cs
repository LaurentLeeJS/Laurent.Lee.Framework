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

namespace Laurent.Lee.CLB.Code
{
    /// <summary>
    ///     成员选择类型
    /// </summary>
    [Flags]
    public enum TmphMemberFilters
    {
        /// <summary>
        ///     未知成员
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///     公共动态字段
        /// </summary>
        PublicInstanceField = 1,

        /// <summary>
        ///     非公共动态字段
        /// </summary>
        NonPublicInstanceField = 2,

        /// <summary>
        ///     公共动态属性
        /// </summary>
        PublicInstanceProperty = 4,

        /// <summary>
        ///     非公共动态属性
        /// </summary>
        NonPublicInstanceProperty = 8,

        /// <summary>
        ///     公共静态字段
        /// </summary>
        PublicStaticField = 0x10,

        /// <summary>
        ///     非公共静态字段
        /// </summary>
        NonPublicStaticField = 0x20,

        /// <summary>
        ///     公共静态属性
        /// </summary>
        PublicStaticProperty = 0x40,

        /// <summary>
        ///     非公共静态属性
        /// </summary>
        NonPublicStaticProperty = 0x80,

        /// <summary>
        ///     公共动态成员
        /// </summary>
        PublicInstance = PublicInstanceField + PublicInstanceProperty,

        /// <summary>
        ///     非公共动态成员
        /// </summary>
        NonPublicInstance = NonPublicInstanceField + NonPublicInstanceProperty,

        /// <summary>
        ///     公共静态成员
        /// </summary>
        PublicStatic = PublicStaticField + PublicStaticProperty,

        /// <summary>
        ///     非公共静态成员
        /// </summary>
        NonPublicStatic = NonPublicStaticField + NonPublicStaticProperty,

        /// <summary>
        ///     动态字段成员
        /// </summary>
        InstanceField = PublicInstanceField + NonPublicInstanceField,

        /// <summary>
        ///     动态属性成员
        /// </summary>
        InstanceProperty = PublicInstanceProperty + NonPublicInstanceProperty,

        /// <summary>
        ///     静态字段成员
        /// </summary>
        StaticField = PublicStaticField + NonPublicStaticField,

        /// <summary>
        ///     静态属性成员
        /// </summary>
        StaticProperty = PublicStaticProperty + NonPublicStaticProperty,

        ///// <summary>
        ///// 公共成员
        ///// </summary>
        //Public = PublicInstance + PublicStatic,
        ///// <summary>
        ///// 非公共成员
        ///// </summary>
        //NonPublic = NonPublicInstance + NonPublicStatic,
        /// <summary>
        ///     动态成员
        /// </summary>
        Instance = PublicInstance + NonPublicInstance,

        /// <summary>
        ///     静态成员
        /// </summary>
        Static = PublicStatic + NonPublicStatic
    }
}