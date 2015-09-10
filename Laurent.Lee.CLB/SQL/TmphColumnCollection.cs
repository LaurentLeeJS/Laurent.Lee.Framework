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

namespace Laurent.Lee.CLB.Sql
{
    /// <summary>
    ///     列集合
    /// </summary>
    internal sealed class THColumnCollection
    {
        /// <summary>
        ///     列集合类型
        /// </summary>
        public enum TmphType
        {
            /// <summary>
            ///     普通集合
            /// </summary>
            None,

            /// <summary>
            ///     主键
            /// </summary>
            PrimaryKey,

            /// <summary>
            ///     普通索引
            /// </summary>
            Index,

            /// <summary>
            ///     唯一索引
            /// </summary>
            UniqueIndex
        }

        /// <summary>
        ///     列集合
        /// </summary>
        public TmphColumn[] Columns;

        /// <summary>
        ///     列集合名称
        /// </summary>
        public string Name;

        /// <summary>
        ///     列集合类型
        /// </summary>
        public TmphType Type;
    }
}