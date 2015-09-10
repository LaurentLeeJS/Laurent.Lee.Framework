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

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    ///     成员复制
    /// </summary>
    public static class TmphMemberCopy
    {
        /// <summary>
        ///     自定义类型成员复制函数标识配置
        /// </summary>
        public sealed class TmphCustom : Attribute
        {
        }
    }
}