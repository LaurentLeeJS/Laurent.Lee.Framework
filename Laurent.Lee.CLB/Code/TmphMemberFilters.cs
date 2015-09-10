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