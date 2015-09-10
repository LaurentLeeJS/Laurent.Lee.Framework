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

namespace Laurent.Lee.CLB.Sql
{
    /// <summary>
    ///     JSON成员
    /// </summary>
    /// <typeparam name="TValueType"></typeparam>
    public struct TmphJsonMember<TValueType>
    {
        /// <summary>
        ///     是否值类型
        /// </summary>
        private static readonly bool isValueType = typeof(TValueType).IsValueType;

        /// <summary>
        ///     数据对象
        /// </summary>
        public TValueType Value;

        /// <summary>
        ///     强制转换定义
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator TmphJsonMember<TValueType>(TValueType value)
        {
            return new TmphJsonMember<TValueType> { Value = value };
        }

        /// <summary>
        ///     强制转换定义
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static implicit operator TmphJsonMember<TValueType>(string json)
        {
            if (json.Length == 0) return default(TmphJsonMember<TValueType>);
            return new TmphJsonMember<TValueType> { Value = TmphJsonParser.Parse<TValueType>(json) };
        }

        /// <summary>
        ///     强制转换定义
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static implicit operator string (TmphJsonMember<TValueType> array)
        {
            return (isValueType || array.Value != null) ? TmphJsonSerializer.ToJson(array.Value) : string.Empty;
        }

        /// <summary>
        ///     对象序列化
        /// </summary>
        /// <param name="serializer">对象序列化器</param>
        [TmphDataSerialize.TmphCustom]
        private static void serialize(TmphDataSerializer serializer, TmphJsonMember<TValueType> value)
        {
            if (value.Value == null) serializer.Stream.Write(TmphBinarySerializer.NullValue);
            else TmphDataSerializer.TmphTypeSerializer<TValueType>.Serialize(serializer, value.Value);
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <param name="deSerializer">对象反序列化器</param>
        /// <returns>是否成功</returns>
        [TmphDataSerialize.TmphCustom]
        private static void deSerialize(TmphDataDeSerializer deSerializer, ref TmphJsonMember<TValueType> value)
        {
            if (deSerializer.CheckNull() == 0) value.Value = default(TValueType);
            else TmphDataDeSerializer.TmphTypeDeSerializer<TValueType>.DeSerialize(deSerializer, ref value.Value);
        }
    }
}