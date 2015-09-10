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

namespace Laurent.Lee.CLB.Net.Tcp.Http
{
    /// <summary>
    ///     索引位置
    /// </summary>
    public struct TmphBufferIndex
    {
        /// <summary>
        ///     长度
        /// </summary>
        public short Length;

        /// <summary>
        ///     起始位置
        /// </summary>
        public short StartIndex;

        /// <summary>
        ///     索引位置
        /// </summary>
        /// <param name="startIndex">起始位置</param>
        /// <param name="length">长度</param>
        public void Set(long startIndex, long length)
        {
            StartIndex = (short)startIndex;
            Length = (short)length;
        }

        /// <summary>
        ///     索引位置
        /// </summary>
        /// <param name="startIndex">起始位置</param>
        /// <param name="length">长度</param>
        public void Set(int startIndex, int length)
        {
            StartIndex = (short)startIndex;
            Length = (short)length;
        }

        /// <summary>
        ///     索引位置
        /// </summary>
        /// <param name="startIndex">起始位置</param>
        /// <param name="length">长度</param>
        public void Set(short startIndex, short length)
        {
            StartIndex = startIndex;
            Length = length;
        }

        /// <summary>
        ///     索引位置置空
        /// </summary>
        public void Null()
        {
            StartIndex = Length = 0;
        }
    }
}