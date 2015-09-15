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