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

using Laurent.Lee.CLB.Emit;
using Laurent.Lee.CLB.IO;

namespace Laurent.Lee.CLB.Sql
{
    /// <summary>
    ///     文件分块成员
    /// </summary>
    /// <typeparam name="TValueType"></typeparam>
    public struct TmphFileBlockMember<TValueType>
    {
        /// <summary>
        ///     是否值类型
        /// </summary>
        private static readonly bool isValueType = typeof(TValueType).IsValueType;

        /// <summary>
        ///     文件索引
        /// </summary>
        private TmphFileBlockStream.TmphIndex index;

        /// <summary>
        ///     是否已经加载数据对象
        /// </summary>
        private int isValue;

        /// <summary>
        ///     数据对象
        /// </summary>
        private TValueType value;

        /// <summary>
        ///     数据对象
        /// </summary>
        public TValueType Value
        {
            get
            {
                if (isValue == 0)
                {
                    if (index.Size == 0) isValue = 1;
                }
                return value;
            }
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <param name="serializer">对象序列化器</param>
        [TmphDataSerialize.TmphCustom]
        private static void serialize(TmphDataSerializer serializer, TmphFileBlockMember<TValueType> value)
        {
            if (value.index.Size == 0) serializer.Stream.Write(TmphBinarySerializer.NullValue);
            else
            {
                var stream = serializer.Stream;
                stream.PrepLength(sizeof(int) * 2 + sizeof(long));
                stream.UnsafeWrite(Emit.TmphPub.PuzzleValue);
                stream.UnsafeWrite(value.index.Size);
                stream.Unsafer.Write(value.index.Index);
                stream.PrepLength();
            }
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <param name="deSerializer">对象反序列化器</param>
        /// <returns>是否成功</returns>
        [TmphDataSerialize.TmphCustom]
        private static unsafe void deSerialize(TmphDataDeSerializer deSerializer, ref TmphFileBlockMember<TValueType> value)
        {
            if (deSerializer.CheckNull() == 0)
            {
                value.index.Null();
                value.isValue = 0;
                value.value = default(TValueType);
            }
            else
            {
                var read = deSerializer.Read;
                if (*(int*)read == Emit.TmphPub.PuzzleValue)
                {
                    if (value.index.ReSet(*(long*)(read + sizeof(int) * 2), *(int*)(read + sizeof(int))) == 0)
                    {
                        value.isValue = 0;
                        value.value = default(TValueType);
                    }
                    deSerializer.Read += sizeof(int) * 2 + sizeof(long);
                }
                else deSerializer.Error(TmphBinaryDeSerializer.TmphDeSerializeState.UnknownData);
            }
        }
    }
}