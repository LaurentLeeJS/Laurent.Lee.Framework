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

namespace Laurent.Lee.CLB.Sql
{
    /// <summary>
    ///     SQL函数调用
    /// </summary>
    internal static class TmphExpressionCall
    {
        /// <summary>
        ///     计数
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        /// <returns>计数</returns>
        public static int Count<TValueType>(TValueType value)
        {
            return 0;
        }

        /// <summary>
        ///     求和
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        /// <returns>求和</returns>
        public static int Sum<TValueType>(TValueType value)
        {
            return 0;
        }

        /// <summary>
        ///     获取当前时间
        /// </summary>
        /// <returns>当前时间</returns>
        public static DateTime GetDate()
        {
            return TmphDate.NowSecond;
        }

        /// <summary>
        ///     IN表达式
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value">数值</param>
        /// <param name="values">数值集合</param>
        /// <returns>是否包含数值</returns>
        public static bool In<TValueType>(TValueType value, params TValueType[] values)
        {
            return false;
        }
    }
}