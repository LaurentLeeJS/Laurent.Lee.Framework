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