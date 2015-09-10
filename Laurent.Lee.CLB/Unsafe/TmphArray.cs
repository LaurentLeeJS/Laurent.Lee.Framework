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

namespace Laurent.Lee.CLB.Unsafe
{
    /// <summary>
    ///     数组扩展操作(非安全,请自行确保数据可靠性)
    /// </summary>
    public static class TmphArray
    {
        /// <summary>
        ///     移动数据块
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">待处理数组</param>
        /// <param name="index">原始数据位置</param>
        /// <param name="writeIndex">目标数据位置</param>
        /// <param name="count">移动数据数量</param>
        public static void Move<TValueType>(TValueType[] array, int index, int writeIndex, int count)
        {
            var endIndex = index + count;
            if (index < writeIndex && endIndex > writeIndex)
            {
                for (var writeEndIndex = writeIndex + count;
                    endIndex != index;
                    array[--writeEndIndex] = array[--endIndex])
                    ;
            }
            else Array.Copy(array, index, array, writeIndex, count);
        }

        /// <summary>
        ///     移除数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="array">数组数据</param>
        /// <param name="index">移除数据位置</param>
        /// <returns>移除数据后的数组</returns>
        public static TValueType[] GetRemoveAt<TValueType>(TValueType[] array, int index)
        {
            var newValues = new TValueType[array.Length - 1];
            Array.Copy(array, 0, newValues, 0, index);
            Array.Copy(array, index + 1, newValues, index, array.Length - index - 1);
            return newValues;
        }
    }
}