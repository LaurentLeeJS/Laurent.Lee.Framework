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

using System;

namespace Laurent.Lee.CLB.Data
{
    /// <summary>
    ///     数据对象拆包器
    /// </summary>
    public unsafe class TmphDataReader
    {
        /// <summary>
        ///     字节数组集合
        /// </summary>
        private readonly byte[][] _bytes;

        /// <summary>
        ///     字符串集合
        /// </summary>
        private readonly string[] _strings;

        /// <summary>
        ///     当前字节数组索引
        /// </summary>
        private int _byteIndex;

        /// <summary>
        ///     数据流
        /// </summary>
        private byte* _data;

        /// <summary>
        ///     当前字符串索引
        /// </summary>
        private int _stringIndex;

        /// <summary>
        ///     数据对象拆包器
        /// </summary>
        /// <param name="data">数据流</param>
        /// <param name="strings">字符串集合</param>
        /// <param name="bytes">字节数组集合</param>
        public TmphDataReader(byte* data, string[] strings, byte[][] bytes)
        {
            _data = data;
            _strings = strings;
            _bytes = bytes;
        }

        /// <summary>
        ///     获取下一个数据对象
        /// </summary>
        /// <param name="typeIndex">数据类型</param>
        /// <returns>数据对象</returns>
        public object Get(byte typeIndex)
        {
            object value;
            switch (typeIndex)
            {
                case 0:
                    value = *(int*)_data;
                    _data += sizeof(int);
                    return value;

                case 1:
                    value = (int?)*(int*)_data;
                    _data += sizeof(int);
                    return value;

                case 2:
                    return _strings[_stringIndex++];

                case 3:
                    value = new DateTime(*(long*)_data);
                    _data += sizeof(long);
                    return value;

                case 4:
                    value = (DateTime?)new DateTime(*(long*)_data);
                    _data += sizeof(long);
                    return value;

                case 5:
                    value = *(double*)_data;
                    _data += sizeof(double);
                    return value;

                case 6:
                    value = (double?)*(double*)_data;
                    _data += sizeof(double);
                    return value;

                case 7:
                    value = *(float*)_data;
                    _data += sizeof(float);
                    return value;

                case 8:
                    value = (float?)*(float*)_data;
                    _data += sizeof(float);
                    return value;

                case 9:
                    value = *(decimal*)_data;
                    _data += sizeof(decimal);
                    return value;

                case 10:
                    value = (decimal?)*(decimal*)_data;
                    _data += sizeof(decimal);
                    return value;

                case 11:
                    value = *(Guid*)_data;
                    _data += sizeof(Guid);
                    return value;

                case 12:
                    value = (Guid?)*(Guid*)_data;
                    _data += sizeof(Guid);
                    return value;

                case 13:
                    value = *_data != 0;
                    ++_data;
                    return value;

                case 14:
                    value = (bool?)(*_data != 0);
                    ++_data;
                    return value;

                case 15:
                    value = *_data;
                    ++_data;
                    return value;

                case 16:
                    value = (byte?)*_data;
                    ++_data;
                    return value;

                case 17:
                    return _bytes[_byteIndex++];

                case 18:
                    value = *(sbyte*)_data;
                    ++_data;
                    return value;

                case 19:
                    value = (sbyte?)*(sbyte*)_data;
                    ++_data;
                    return value;

                case 20:
                    value = *(short*)_data;
                    _data += sizeof(short);
                    return value;

                case 21:
                    value = (short?)*(short*)_data;
                    _data += sizeof(short);
                    return value;

                case 22:
                    value = *(ushort*)_data;
                    _data += sizeof(ushort);
                    return value;

                case 23:
                    value = (ushort?)*(ushort*)_data;
                    _data += sizeof(ushort);
                    return value;

                case 24:
                    value = *(uint*)_data;
                    _data += sizeof(uint);
                    return value;

                case 25:
                    value = (uint?)*(uint*)_data;
                    _data += sizeof(uint);
                    return value;

                case 26:
                    value = *(long*)_data;
                    _data += sizeof(long);
                    return value;

                case 27:
                    value = (long?)*(long*)_data;
                    _data += sizeof(long);
                    return value;

                case 28:
                    value = *(ulong*)_data;
                    _data += sizeof(ulong);
                    return value;

                case 29:
                    value = (ulong?)*(ulong*)_data;
                    _data += sizeof(ulong);
                    return value;
            }
            return null;
        }
    }
}