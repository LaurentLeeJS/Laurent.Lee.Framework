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
    ///     表格信息
    /// </summary>
    internal sealed class TmphTable
    {
        /// <summary>
        ///     列集合
        /// </summary>
        public THColumnCollection Columns;

        /// <summary>
        ///     自增列
        /// </summary>
        public TmphColumn Identity;

        /// <summary>
        ///     索引集合
        /// </summary>
        public THColumnCollection[] Indexs;

        /// <summary>
        ///     主键
        /// </summary>
        public THColumnCollection PrimaryKey;
    }
}