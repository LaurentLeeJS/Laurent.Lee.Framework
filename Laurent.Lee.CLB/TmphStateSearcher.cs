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

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     状态搜索器
    /// </summary>
    public static unsafe class TmphStateSearcher
    {
        /// <summary>
        ///     字节数组比较大小
        /// </summary>
        private static readonly Func<TmphKeyValue<byte[], int>, TmphKeyValue<byte[], int>, int> CompareHanlde = Compare;

        /// <summary>
        ///     字符串比较大小
        /// </summary>
        private static readonly Func<TmphKeyValue<string, int>, TmphKeyValue<string, int>, int> StringCompare = Compare;

        /// <summary>
        ///     字节数组比较大小
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private static int Compare(TmphKeyValue<byte[], int> left, TmphKeyValue<byte[], int> right)
        {
            var length = Math.Min(left.Key.Length, right.Key.Length);
            fixed (byte* leftFixed = left.Key, rightFixed = right.Key)
            {
                for (byte* start = leftFixed, end = leftFixed + length, read = rightFixed;
                    start != end;
                    ++start, ++read)
                {
                    if (*start != *read) return *start - *read;
                }
            }
            return left.Key.Length - right.Key.Length;
        }

        /// <summary>
        ///     字符串比较大小
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private static int Compare(TmphKeyValue<string, int> left, TmphKeyValue<string, int> right)
        {
            return string.CompareOrdinal(left.Key, right.Key);
        }

        /// <summary>
        ///     ASCII字节搜索器
        /// </summary>
        private struct TmphAscii
        {
            /// <summary>
            ///     ASCII字符查找表
            /// </summary>
            private readonly byte* _charsAscii;

            /// <summary>
            ///     状态集合
            /// </summary>
            private readonly byte* _state;

            /// <summary>
            ///     查询矩阵单位尺寸类型
            /// </summary>
            private readonly byte _tableType;

            /// <summary>
            ///     当前状态
            /// </summary>
            private byte* _currentState;

            /// <summary>
            ///     ASCII字节搜索器
            /// </summary>
            /// <param name="data">数据起始位置</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public TmphAscii(TmphPointer data)
            {
                if (data.Data == null)
                {
                    _state = _charsAscii = _currentState = null;
                    _tableType = 0;
                }
                else
                {
                    var stateCount = *data.Int;
                    _currentState = _state = data.Byte + sizeof(int);
                    _charsAscii = _state + stateCount * 3 * sizeof(int);
                    if (stateCount < 256) _tableType = 0;
                    else if (stateCount < 65536) _tableType = 1;
                    else _tableType = 2;
                }
            }

            /// <summary>
            ///     获取状态索引
            /// </summary>
            /// <param name="start">匹配起始位置</param>
            /// <param name="end">匹配结束位置</param>
            /// <returns>状态索引,失败返回-1</returns>
            public int Search(byte* start, byte* end)
            {
                if (_state == null || start >= end) return -1;
                _currentState = _state;
                do
                {
                    for (var prefix = _currentState + *(int*)_currentState; *prefix != 0; ++prefix, ++start)
                    {
                        if (start == end || *start != *prefix) return -1;
                    }
                    if (start == end) return *(int*)(_currentState + sizeof(int) * 2);
                    if (*(int*)(_currentState + sizeof(int)) == 0) return -1;
                    int index = *(_charsAscii + *start);
                    var table = _currentState + *(int*)(_currentState + sizeof(int));
                    if (_tableType == 0)
                    {
                        if ((index = *(table + index)) == 0) return -1;
                        _currentState = _state + index * 3 * sizeof(int);
                    }
                    else if (_tableType == 1)
                    {
                        if ((index = *(ushort*)(table + index * sizeof(ushort))) == 0) return -1;
                        _currentState = _state + index * 3 * sizeof(int);
                    }
                    else
                    {
                        if ((index = *(int*)(table + index * sizeof(int))) == 0) return -1;
                        _currentState = _state + index;
                    }
                    ++start;
                } while (true);
            }

            /// <summary>
            ///     获取状态索引
            /// </summary>
            /// <param name="data">匹配状态</param>
            /// <returns>状态索引,失败返回-1</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public int Search(byte[] data)
            {
                if (data != null && data.Length != 0)
                {
                    fixed (byte* dataFixed = data) return Search(dataFixed, dataFixed + data.Length);
                }
                return -1;
            }

            /// <summary>
            ///     获取状态索引
            /// </summary>
            /// <param name="data">匹配状态</param>
            /// <returns>状态索引,失败返回-1</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public int Search(TmphSubArray<byte> data)
            {
                if (data.Count != 0)
                {
                    fixed (byte* dataFixed = data.array)
                    {
                        var start = dataFixed + data.StartIndex;
                        return Search(start, start + data.Count);
                    }
                }
                return -1;
            }

            /// <summary>
            ///     获取状态索引
            /// </summary>
            /// <param name="start">匹配起始位置</param>
            /// <param name="end">匹配结束位置</param>
            /// <returns>状态索引,失败返回-1</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public int Search(char* start, char* end)
            {
                if (_state == null || start >= end) return -1;
                _currentState = _state;
                do
                {
                    for (var prefix = _currentState + *(int*)_currentState; *prefix != 0; ++prefix, ++start)
                    {
                        if (start == end || *start != *prefix) return -1;
                    }
                    if (start == end) return *(int*)(_currentState + sizeof(int) * 2);
                    if (*(int*)(_currentState + sizeof(int)) == 0) return -1;
                    int index = *(_charsAscii + *start);
                    var table = _currentState + *(int*)(_currentState + sizeof(int));
                    if (_tableType == 0)
                    {
                        if ((index = *(table + index)) == 0) return -1;
                        _currentState = _state + index * 3 * sizeof(int);
                    }
                    else if (_tableType == 1)
                    {
                        if ((index = *(ushort*)(table + index * sizeof(ushort))) == 0) return -1;
                        _currentState = _state + index * 3 * sizeof(int);
                    }
                    else
                    {
                        if ((index = *(int*)(table + index * sizeof(int))) == 0) return -1;
                        _currentState = _state + index;
                    }
                    ++start;
                } while (true);
            }

            /// <summary>
            ///     删除状态索引
            /// </summary>
            /// <param name="start">匹配起始位置</param>
            /// <param name="end">匹配结束位置</param>
            /// <returns>状态索引,失败返回-1</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public int Remove(char* start, char* end)
            {
                if (_state == null || start >= end) return -1;
                _currentState = _state;
                do
                {
                    for (var prefix = _currentState + *(int*)_currentState; *prefix != 0; ++prefix, ++start)
                    {
                        if (start == end || *start != *prefix) return -1;
                    }
                    if (start == end)
                    {
                        var removeIndex = *(int*)(_currentState + sizeof(int) * 2);
                        *(int*)(_currentState + sizeof(int) * 2) = -1;
                        return removeIndex;
                    }
                    if (*(int*)(_currentState + sizeof(int)) == 0) return -1;
                    int index = *(_charsAscii + *start);
                    var table = _currentState + *(int*)(_currentState + sizeof(int));
                    if (_tableType == 0)
                    {
                        if ((index = *(table + index)) == 0) return -1;
                        _currentState = _state + index * 3 * sizeof(int);
                    }
                    else if (_tableType == 1)
                    {
                        if ((index = *(ushort*)(table + index * sizeof(ushort))) == 0) return -1;
                        _currentState = _state + index * 3 * sizeof(int);
                    }
                    else
                    {
                        if ((index = *(int*)(table + index * sizeof(int))) == 0) return -1;
                        _currentState = _state + index;
                    }
                    ++start;
                } while (true);
            }

            /// <summary>
            ///     获取状态索引
            /// </summary>
            /// <param name="value">匹配状态</param>
            /// <returns>状态索引,失败返回-1</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public int Search(string value)
            {
                if (value != null && value.Length != 0)
                {
                    fixed (char* valueFixed = value) return Search(valueFixed, valueFixed + value.Length);
                }
                return -1;
            }

            /// <summary>
            ///     删除状态索引
            /// </summary>
            /// <param name="value">匹配状态</param>
            /// <returns>状态索引,失败返回-1</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public int Remove(string value)
            {
                if (value != null && value.Length != 0)
                {
                    fixed (char* valueFixed = value) return Remove(valueFixed, valueFixed + value.Length);
                }
                return -1;
            }

            /// <summary>
            ///     获取状态索引
            /// </summary>
            /// <param name="value">匹配状态</param>
            /// <returns>状态索引,失败返回-1</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public int Search(TmphSubString value)
            {
                if (value.Length != 0)
                {
                    fixed (char* valueFixed = value.value)
                    {
                        var start = valueFixed + value.StartIndex;
                        return Search(start, start + value.Length);
                    }
                }
                return -1;
            }

            /// <summary>
            ///     创建状态查找数据
            /// </summary>
            /// <param name="states">状态集合</param>
            /// <returns>状态查找数据</returns>
            public static TmphPointer Create(string[] states)
            {
                if (states.length() != 0)
                {
                    var index = 0;
                    var strings = new TmphKeyValue<string, int>[states.Length];
                    foreach (var name in states)
                    {
                        strings[index].Set(name, index);
                        ++index;
                    }
                    strings = strings.sort(StringCompare);
                    return new TmphStringBuilder(strings).Data;
                }
                return new TmphPointer();
            }

            /// <summary>
            ///     创建状态查找数据
            /// </summary>
            /// <param name="states">状态集合</param>
            /// <returns>状态查找数据</returns>
            public static TmphPointer Create(byte[][] states)
            {
                if (states.length() != 0)
                {
                    var index = 0;
                    var datas = new TmphKeyValue<byte[], int>[states.Length];
                    foreach (var name in states)
                    {
                        datas[index].Set(name, index);
                        ++index;
                    }
                    datas = datas.sort(CompareHanlde);
                    return new TmphByteArrayBuilder(datas).Data;
                }
                return new TmphPointer();
            }

            /// <summary>
            ///     状态数据创建器
            /// </summary>
            private struct TmphStringBuilder
            {
                /// <summary>
                ///     状态字符数量
                /// </summary>
                private readonly int _charCount;

                /// <summary>
                ///     ASCII字符查找表
                /// </summary>
                private readonly byte* _charsAscii;

                /// <summary>
                ///     状态数据
                /// </summary>
                public readonly TmphPointer Data;

                /// <summary>
                ///     查询矩阵单位尺寸类型
                /// </summary>
                private readonly int _tableType;

                /// <summary>
                ///     状态集合
                /// </summary>
                private readonly TmphKeyValue<string, int>[] _values;

                /// <summary>
                ///     状态字符集合
                /// </summary>
                private TmphFixedMap _chars;

                /// <summary>
                ///     前缀集合
                /// </summary>
                private byte* _prefix;

                /// <summary>
                ///     前缀数量
                /// </summary>
                private int _prefixSize;

                /// <summary>
                ///     状态集合
                /// </summary>
                private byte* _state;

                /// <summary>
                ///     状态数量
                /// </summary>
                private int stateCount;

                /// <summary>
                ///     状态矩阵
                /// </summary>
                private byte* table;

                /// <summary>
                ///     矩阵状态数量
                /// </summary>
                private int tableCount;

                /// <summary>
                ///     状态数据创建器
                /// </summary>
                /// <param name="values">状态集合</param>
                public TmphStringBuilder(TmphKeyValue<string, int>[] values)
                {
                    this._values = values;
                    _prefixSize = tableCount = stateCount = _tableType = _charCount = 0;
                    _state = _charsAscii = _prefix = table = null;
                    if (values.Length > 1)
                    {
                        byte* chars = stackalloc byte[128 >> 3];
                        this._chars = new TmphFixedMap(chars, 128 >> 3, 0);
                        Data = new TmphPointer();
                        count(0, values.Length, 0);
                        for (byte* start = chars, end = chars + (128 >> 3); start != end; start += sizeof(int))
                        {
                            _charCount += (*(uint*)start).bitCount();
                        }
                        var size = (1 + (stateCount += tableCount) * 3) * sizeof(int) + 128 + 4 +
                                   (_prefixSize & (int.MaxValue - 3));
                        if (stateCount < 256) size += tableCount * (_charCount + 1);
                        else if (stateCount < 65536)
                        {
                            size += tableCount * (_charCount + 1) * sizeof(ushort);
                            _tableType = 1;
                        }
                        else
                        {
                            size += tableCount * (_charCount + 1) * sizeof(int);
                            _tableType = 2;
                        }
                        Data = TmphUnmanaged.Get(size, true);
                        *Data.Int = stateCount; //状态数量[int]
                        _state = Data.Byte + sizeof(int); //状态集合[stateCount*(前缀位置[int]+状态位置[int]+名称索引[int])]
                        _charsAscii = _state + (stateCount * 3) * sizeof(int); //ascii字符查找表[128*byte]
                        byte charIndex = 0;
                        for (byte index = 1; index != 128; ++index)
                        {
                            if (this._chars.Get(index)) *(_charsAscii + index) = ++charIndex;
                        }
                        _prefix = _charsAscii + 128; //前缀集合
                        table = _prefix + ((_prefixSize & (int.MaxValue - 3)) + 4);
                        //状态矩阵[tableCount*(charCount+1)*[byte/ushort/int]]
                        *_prefix++ = (byte)_charCount; //字符数量
                        stateCount = 0;
                        create(0, values.Length, 0);
                    }
                    else
                    {
                        _chars = new TmphFixedMap();
                        var value = values[0].Key;
                        fixed (char* valueFixed = value)
                        {
                            if (values[0].Key.Length <= 128)
                            {
                                Data = TmphUnmanaged.Get(sizeof(int) + sizeof(int) * 3 + 128 + 1, false);
                                *Data.Int = 1; //状态数量
                                _state = Data.Byte + sizeof(int);
                                *(int*)_state = sizeof(int) * 3; //前缀位置
                                *(int*)(_state + sizeof(int)) = 0; //状态位置
                                *(int*)(_state + sizeof(int) * 2) = values[0].Value; //名称索引
                                _prefix = Data.Byte + sizeof(int) * 4;
                                Unsafe.TmphString.WriteBytes(valueFixed, value.Length, _prefix);
                                *(_prefix + value.Length) = *(_prefix + 128) = 0;
                            }
                            else
                            {
                                Data = TmphUnmanaged.Get(sizeof(int) + sizeof(int) * 3 + 128 + 1 + value.Length + 1, true);
                                *Data.Int = 1; //状态数量
                                _state = Data.Byte + sizeof(int);
                                *(int*)_state = sizeof(int) * 3 + 128 + 1; //前缀位置
                                *(int*)(_state + sizeof(int)) = 0; //状态位置
                                *(int*)(_state + sizeof(int) * 2) = values[0].Value; //名称索引
                                Unsafe.TmphString.WriteBytes(valueFixed, value.Length,
                                    Data.Byte + sizeof(int) * 3 + 128 + 1);
                            }
                        }
                    }
                }

                /// <summary>
                ///     计算状态数量
                /// </summary>
                /// <param name="start">起始名称位置</param>
                /// <param name="end">结束名称位置</param>
                /// <param name="current"></param>
                private void count(int start, int end, int current)
                {
                    ++tableCount;
                    int index = start, prefixSize = 0;
                    var value = (char)0;
                    while (_values[start].Key.Length != current)
                    {
                        value = _values[start].Key[current];
                        while (++index != end && _values[index].Key[current] == value) ;
                        if (index != end) break;
                        ++prefixSize;
                        index = start;
                        ++current;
                    }
                    if (prefixSize != 0) this._prefixSize += prefixSize + 1;
                    do
                    {
                        var count = index - start;
                        if (count == 0) index = ++start;
                        else
                        {
                            if (value >= 128 || value == 0) TmphLog.Default.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                            _chars.Set(value);
                            if (count == 1)
                            {
                                ++stateCount;
                                prefixSize = _values[start].Key.Length - current - 1;
                                if (prefixSize != 0) this._prefixSize += prefixSize + 1;
                            }
                            else this.count(start, index, current + 1);
                        }
                        if (index == end) break;
                        value = _values[start = index].Key[current];
                        while (++index != end && _values[index].Key[current] == value) ;
                    } while (true);
                }

                /// <summary>
                ///     创建状态数据
                /// </summary>
                /// <param name="start">起始名称位置</param>
                /// <param name="end">结束名称位置</param>
                /// <param name="current"></param>
                private void create(int start, int end, int current)
                {
                    byte* prefix = this._prefix, table = this.table;
                    *(int*)(_state + sizeof(int)) = (int)(table - _state);
                    var index = start;
                    var value = (char)0;
                    if (_values[start].Key.Length == current) *(int*)(_state + sizeof(int) * 2) = _values[start].Value;
                    else
                    {
                        do
                        {
                            value = _values[index].Key[current];
                            while (++index != end && _values[index].Key[current] == value) ;
                            if (index != end)
                            {
                                *(int*)(_state + sizeof(int) * 2) = -1;
                                break;
                            }
                            *this._prefix++ = (byte)value;
                            if (_values[index = start].Key.Length == ++current)
                            {
                                *(int*)(_state + sizeof(int) * 2) = _values[index].Value;
                                break;
                            }
                        } while (true);
                    }
                    if (prefix == this._prefix) *(int*)_state = (int)(_charsAscii - _state);
                    else
                    {
                        *this._prefix++ = 0;
                        *(int*)_state = (int)(prefix - _state);
                    }
                    _state += sizeof(int) * 3;
                    ++stateCount;
                    if (_tableType == 0) this.table += _charCount + 1;
                    else if (_tableType == 1) this.table += (_charCount + 1) * sizeof(ushort);
                    else this.table += (_charCount + 1) * sizeof(int);
                    do
                    {
                        var count = index - start;
                        if (count == 0) index = ++start;
                        else
                        {
                            int charIndex = *(_charsAscii + value);
                            if (_tableType == 0) *(table + charIndex) = (byte)stateCount;
                            else if (_tableType == 1)
                                *(ushort*)(table + charIndex * sizeof(ushort)) = (ushort)stateCount;
                            else *(int*)(table + charIndex * sizeof(int)) = stateCount * 3 * sizeof(int);
                            if (count == 1)
                            {
                                var prefixSize = _values[start].Key.Length - current - 1;
                                if (prefixSize == 0) *(int*)_state = (int)(_charsAscii - _state);
                                else
                                {
                                    *(int*)_state = (int)(this._prefix - _state);
                                    fixed (char* charFixed = _values[start].Key)
                                    {
                                        Unsafe.TmphString.WriteBytes(charFixed + current + 1, prefixSize, this._prefix);
                                        *(this._prefix += prefixSize) = 0;
                                        ++this._prefix;
                                    }
                                }
                                *(int*)(_state + sizeof(int) * 2) = _values[start].Value;
                                ++stateCount;
                                _state += sizeof(int) * 3;
                            }
                            else create(start, index, current + 1);
                        }
                        if (index == end) break;
                        value = _values[start = index].Key[current];
                        while (++index != end && _values[index].Key[current] == value) ;
                    } while (true);
                }
            }

            /// <summary>
            ///     状态数据创建器
            /// </summary>
            private struct TmphByteArrayBuilder
            {
                /// <summary>
                ///     状态字符数量
                /// </summary>
                private readonly int charCount;

                /// <summary>
                ///     ASCII字符查找表
                /// </summary>
                private readonly byte* charsAscii;

                /// <summary>
                ///     状态数据
                /// </summary>
                public readonly TmphPointer Data;

                /// <summary>
                ///     查询矩阵单位尺寸类型
                /// </summary>
                private readonly int tableType;

                /// <summary>
                ///     状态集合
                /// </summary>
                private readonly TmphKeyValue<byte[], int>[] values;

                /// <summary>
                ///     状态字符集合
                /// </summary>
                private TmphFixedMap chars;

                /// <summary>
                ///     前缀集合
                /// </summary>
                private byte* prefix;

                /// <summary>
                ///     前缀数量
                /// </summary>
                private int prefixSize;

                /// <summary>
                ///     状态集合
                /// </summary>
                private byte* state;

                /// <summary>
                ///     状态数量
                /// </summary>
                private int stateCount;

                /// <summary>
                ///     状态矩阵
                /// </summary>
                private byte* table;

                /// <summary>
                ///     矩阵状态数量
                /// </summary>
                private int tableCount;

                /// <summary>
                ///     状态数据创建器
                /// </summary>
                /// <param name="values">状态集合</param>
                public TmphByteArrayBuilder(TmphKeyValue<byte[], int>[] values)
                {
                    this.values = values;
                    prefixSize = tableCount = stateCount = tableType = charCount = 0;
                    state = charsAscii = prefix = table = null;
                    if (values.Length > 1)
                    {
                        byte* chars = stackalloc byte[128 >> 3];
                        this.chars = new TmphFixedMap(chars, 128 >> 3, 0);
                        Data = new TmphPointer();
                        count(0, values.Length, 0);
                        for (byte* start = chars, end = chars + (128 >> 3); start != end; start += sizeof(int))
                        {
                            charCount += (*(uint*)start).bitCount();
                        }
                        var size = (1 + (stateCount += tableCount) * 3) * sizeof(int) + 128 + 4 +
                                   (prefixSize & (int.MaxValue - 3));
                        if (stateCount < 256) size += tableCount * (charCount + 1);
                        else if (stateCount < 65536)
                        {
                            size += tableCount * (charCount + 1) * sizeof(ushort);
                            tableType = 1;
                        }
                        else
                        {
                            size += tableCount * (charCount + 1) * sizeof(int);
                            tableType = 2;
                        }
                        Data = TmphUnmanaged.Get(size, true);
                        *Data.Int = stateCount; //状态数量[int]
                        state = Data.Byte + sizeof(int); //状态集合[stateCount*(前缀位置[int]+状态位置[int]+名称索引[int])]
                        charsAscii = state + (stateCount * 3) * sizeof(int); //ascii字符查找表[128*byte]
                        byte charIndex = 0;
                        for (byte index = 1; index != 128; ++index)
                        {
                            if (this.chars.Get(index)) *(charsAscii + index) = ++charIndex;
                        }
                        prefix = charsAscii + 128; //前缀集合
                        table = prefix + ((prefixSize & (int.MaxValue - 3)) + 4);
                        //状态矩阵[tableCount*(charCount+1)*[byte/ushort/int]]
                        *prefix++ = (byte)charCount; //字符数量
                        stateCount = 0;
                        create(0, values.Length, 0);
                    }
                    else
                    {
                        chars = new TmphFixedMap();
                        var value = values[0].Key;
                        fixed (byte* valueFixed = value)
                        {
                            if (values[0].Key.Length <= 128)
                            {
                                Data = TmphUnmanaged.Get(sizeof(int) + sizeof(int) * 3 + 128 + 1, false);
                                *Data.Int = 1; //状态数量
                                state = Data.Byte + sizeof(int);
                                *(int*)state = sizeof(int) * 3; //前缀位置
                                *(int*)(state + sizeof(int)) = 0; //状态位置
                                *(int*)(state + sizeof(int) * 2) = values[0].Value; //名称索引
                                prefix = Data.Byte + sizeof(int) * 4;
                                Unsafe.TmphMemory.Copy(valueFixed, prefix, value.Length);
                                *(prefix + value.Length) = *(prefix + 128) = 0;
                            }
                            else
                            {
                                Data = TmphUnmanaged.Get(sizeof(int) + sizeof(int) * 3 + 128 + 1 + value.Length + 1, true);
                                *Data.Int = 1; //状态数量
                                state = Data.Byte + sizeof(int);
                                *(int*)state = sizeof(int) * 3 + 128 + 1; //前缀位置
                                *(int*)(state + sizeof(int)) = 0; //状态位置
                                *(int*)(state + sizeof(int) * 2) = values[0].Value; //名称索引
                                Unsafe.TmphMemory.Copy(valueFixed, Data.Byte + sizeof(int) * 3 + 128 + 1, value.Length);
                            }
                        }
                    }
                }

                /// <summary>
                ///     计算状态数量
                /// </summary>
                /// <param name="start">起始名称位置</param>
                /// <param name="end">结束名称位置</param>
                /// <param name="current"></param>
                private void count(int start, int end, int current)
                {
                    ++tableCount;
                    int index = start, prefixSize = 0;
                    byte value = 0;
                    while (values[start].Key.Length != current)
                    {
                        value = values[start].Key[current];
                        while (++index != end && values[index].Key[current] == value) ;
                        if (index != end) break;
                        ++prefixSize;
                        index = start;
                        ++current;
                    }
                    if (prefixSize != 0) this.prefixSize += prefixSize + 1;
                    do
                    {
                        var count = index - start;
                        if (count == 0) index = ++start;
                        else
                        {
                            if (value >= 128 || value == 0) TmphLog.Default.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                            chars.Set(value);
                            if (count == 1)
                            {
                                ++stateCount;
                                prefixSize = values[start].Key.Length - current - 1;
                                if (prefixSize != 0) this.prefixSize += prefixSize + 1;
                            }
                            else this.count(start, index, current + 1);
                        }
                        if (index == end) break;
                        value = values[start = index].Key[current];
                        while (++index != end && values[index].Key[current] == value) ;
                    } while (true);
                }

                /// <summary>
                ///     创建状态数据
                /// </summary>
                /// <param name="start">起始名称位置</param>
                /// <param name="end">结束名称位置</param>
                /// <param name="current"></param>
                private void create(int start, int end, int current)
                {
                    byte* prefix = this.prefix, table = this.table;
                    *(int*)(state + sizeof(int)) = (int)(table - state);
                    var index = start;
                    byte value = 0;
                    if (values[start].Key.Length == current) *(int*)(state + sizeof(int) * 2) = values[start].Value;
                    else
                    {
                        do
                        {
                            value = values[index].Key[current];
                            while (++index != end && values[index].Key[current] == value) ;
                            if (index != end)
                            {
                                *(int*)(state + sizeof(int) * 2) = -1;
                                break;
                            }
                            *this.prefix++ = value;
                            if (values[index = start].Key.Length == ++current)
                            {
                                *(int*)(state + sizeof(int) * 2) = values[index].Value;
                                break;
                            }
                        } while (true);
                    }
                    if (prefix == this.prefix) *(int*)state = (int)(charsAscii - state);
                    else
                    {
                        *this.prefix++ = 0;
                        *(int*)state = (int)(prefix - state);
                    }
                    state += sizeof(int) * 3;
                    ++stateCount;
                    if (tableType == 0) this.table += charCount + 1;
                    else if (tableType == 1) this.table += (charCount + 1) * sizeof(ushort);
                    else this.table += (charCount + 1) * sizeof(int);
                    do
                    {
                        var count = index - start;
                        if (count == 0) index = ++start;
                        else
                        {
                            int charIndex = *(charsAscii + value);
                            if (tableType == 0) *(table + charIndex) = (byte)stateCount;
                            else if (tableType == 1)
                                *(ushort*)(table + charIndex * sizeof(ushort)) = (ushort)stateCount;
                            else *(int*)(table + charIndex * sizeof(int)) = stateCount * 3 * sizeof(int);
                            if (count == 1)
                            {
                                var prefixSize = values[start].Key.Length - current - 1;
                                if (prefixSize == 0) *(int*)state = (int)(charsAscii - state);
                                else
                                {
                                    *(int*)state = (int)(this.prefix - state);
                                    fixed (byte* charFixed = values[start].Key)
                                    {
                                        Unsafe.TmphMemory.Copy(charFixed + current + 1, this.prefix, prefixSize);
                                        *(this.prefix += prefixSize) = 0;
                                        ++this.prefix;
                                    }
                                }
                                *(int*)(state + sizeof(int) * 2) = values[start].Value;
                                ++stateCount;
                                state += sizeof(int) * 3;
                            }
                            else create(start, index, current + 1);
                        }
                        if (index == end) break;
                        value = values[start = index].Key[current];
                        while (++index != end && values[index].Key[current] == value) ;
                    } while (true);
                }
            }
        }

        /// <summary>
        ///     ASCII字节状态搜索
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        public sealed class TmphAscii<TValueType> : IDisposable
        {
            /// <summary>
            ///     状态搜索数据
            /// </summary>
            private TmphPointer data;

            /// <summary>
            ///     状态数据集合
            /// </summary>
            private TValueType[] values;

            /// <summary>
            ///     ASCII字节状态搜索
            /// </summary>
            /// <param name="states">状态集合</param>
            /// <param name="values">状态数据集合</param>
            public TmphAscii(string[] states, TValueType[] values)
            {
                if (states == null || values == null) TmphLog.Default.Throw(TmphLog.TmphExceptionType.Null);
                if (states.Length > values.Length) TmphLog.Default.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                this.values = values;
                data = TmphAscii.Create(states);
            }

            /// <summary>
            ///     ASCII字节状态搜索
            /// </summary>
            /// <param name="states">状态集合</param>
            /// <param name="values">状态数据集合</param>
            public TmphAscii(byte[][] states, TValueType[] values)
            {
                if (states == null || values == null) TmphLog.Default.Throw(TmphLog.TmphExceptionType.Null);
                if (states.Length > values.Length) TmphLog.Default.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                this.values = values;
                data = TmphAscii.Create(states);
            }

            /// <summary>
            ///     释放资源
            /// </summary>
            public void Dispose()
            {
                TmphUnmanaged.Free(ref data);
                values = null;
            }

            /// <summary>
            ///     获取状态数据
            /// </summary>
            /// <param name="state">查询状态</param>
            /// <param name="nullValue">默认空值</param>
            /// <returns>状态数据,失败返回默认空值</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public TValueType Get(byte[] state, TValueType nullValue = default(TValueType))
            {
                var index = new TmphAscii(data).Search(state);
                return index >= 0 ? values[index] : nullValue;
            }

            /// <summary>
            ///     获取状态数据
            /// </summary>
            /// <param name="state">查询状态</param>
            /// <param name="value">目标数据</param>
            /// <returns>是否存在匹配状态数据</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public bool Get(byte[] state, ref TValueType value)
            {
                var index = new TmphAscii(data).Search(state);
                if (index >= 0)
                {
                    value = values[index];
                    return true;
                }
                return false;
            }

            /// <summary>
            ///     获取状态数据
            /// </summary>
            /// <param name="state">查询状态</param>
            /// <param name="nullValue">默认空值</param>
            /// <returns>状态数据,失败返回默认空值</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public TValueType Get(TmphSubArray<byte> state, TValueType nullValue = default(TValueType))
            {
                var index = new TmphAscii(data).Search(state);
                return index >= 0 ? values[index] : nullValue;
            }

            /// <summary>
            ///     获取状态数据
            /// </summary>
            /// <param name="state">查询状态</param>
            /// <param name="value">目标数据</param>
            /// <returns>是否存在匹配状态数据</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public bool Get(TmphSubArray<byte> state, ref TValueType value)
            {
                var index = new TmphAscii(data).Search(state);
                if (index >= 0)
                {
                    value = values[index];
                    return true;
                }
                return false;
            }

            /// <summary>
            ///     获取状态数据
            /// </summary>
            /// <param name="state">查询状态</param>
            /// <param name="nullValue">默认空值</param>
            /// <returns>状态数据,失败返回默认空值</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public TValueType Get(string state, TValueType nullValue = default(TValueType))
            {
                var index = new TmphAscii(data).Search(state);
                return index >= 0 ? values[index] : nullValue;
            }

            /// <summary>
            ///     判断是否存在状态数据
            /// </summary>
            /// <param name="state">查询状态</param>
            /// <returns>是否存在状态数据</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public bool ContainsKey(string state)
            {
                return new TmphAscii(data).Search(state) >= 0;
            }

            /// <summary>
            ///     删除状态数据
            /// </summary>
            /// <param name="state">查询状态</param>
            /// <returns>是否存在状态数据</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public bool Remove(string state)
            {
                return new TmphAscii(data).Remove(state) >= 0;
            }

            /// <summary>
            ///     获取状态数据
            /// </summary>
            /// <param name="state">查询状态</param>
            /// <param name="nullValue">默认空值</param>
            /// <returns>状态数据,失败返回默认空值</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public TValueType Get(TmphSubString state, TValueType nullValue = default(TValueType))
            {
                var index = new TmphAscii(data).Search(state);
                return index >= 0 ? values[index] : nullValue;
            }

            /// <summary>
            ///     判断是否存在状态数据
            /// </summary>
            /// <param name="state">查询状态</param>
            /// <returns>是否存在状态数据</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public bool ContainsKey(TmphSubString state)
            {
                return new TmphAscii(data).Search(state) >= 0;
            }
        }

        /// <summary>
        ///     字节数组搜索器
        /// </summary>
        internal struct TmphByteArray
        {
            /// <summary>
            ///     字节查找表
            /// </summary>
            private readonly byte* bytes;

            /// <summary>
            ///     状态集合
            /// </summary>
            private readonly byte* state;

            /// <summary>
            ///     查询矩阵单位尺寸类型
            /// </summary>
            private readonly byte tableType;

            /// <summary>
            ///     当前状态
            /// </summary>
            private byte* currentState;

            /// <summary>
            ///     ASCII字节搜索器
            /// </summary>
            /// <param name="data">数据起始位置</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public TmphByteArray(TmphPointer data)
            {
                if (data.Data == null)
                {
                    state = bytes = currentState = null;
                    tableType = 0;
                }
                else
                {
                    var stateCount = *data.Int;
                    currentState = state = data.Byte + sizeof(int);
                    bytes = state + stateCount * 3 * sizeof(int);
                    if (stateCount < 256) tableType = 0;
                    else if (stateCount < 65536) tableType = 1;
                    else tableType = 2;
                }
            }

            /// <summary>
            ///     获取状态索引
            /// </summary>
            /// <param name="start">匹配起始位置</param>
            /// <param name="end">匹配结束位置</param>
            /// <returns>状态索引,失败返回-1</returns>
            public int Search(byte* start, byte* end)
            {
                if (state == null || start >= end) return -1;
                currentState = state;
                do
                {
                    var prefix = currentState + *(int*)currentState;
                    int prefixSize = *(ushort*)(prefix - sizeof(ushort));
                    if (prefixSize != 0)
                    {
                        for (var endPrefix = prefix + prefixSize; prefix != endPrefix; ++prefix, ++start)
                        {
                            if (start == end || *start != *prefix) return -1;
                        }
                    }
                    if (start == end) return *(int*)(currentState + sizeof(int) * 2);
                    if (*(int*)(currentState + sizeof(int)) == 0) return -1;
                    int index = *(bytes + *start);
                    var table = currentState + *(int*)(currentState + sizeof(int));
                    if (tableType == 0)
                    {
                        if ((index = *(table + index)) == 0) return -1;
                        currentState = state + index * 3 * sizeof(int);
                    }
                    else if (tableType == 1)
                    {
                        if ((index = *(ushort*)(table + index * sizeof(ushort))) == 0) return -1;
                        currentState = state + index * 3 * sizeof(int);
                    }
                    else
                    {
                        if ((index = *(int*)(table + index * sizeof(int))) == 0) return -1;
                        currentState = state + index;
                    }
                    ++start;
                } while (true);
            }

            /// <summary>
            ///     获取状态索引
            /// </summary>
            /// <param name="data">匹配状态</param>
            /// <returns>状态索引,失败返回-1</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public int Search(TmphSubArray<byte> data)
            {
                if (data.Count != 0)
                {
                    fixed (byte* dataFixed = data.array)
                    {
                        var start = dataFixed + data.StartIndex;
                        return Search(start, start + data.Count);
                    }
                }
                return -1;
            }

            /// <summary>
            ///     创建状态查找数据
            /// </summary>
            /// <param name="states">状态集合</param>
            /// <returns>状态查找数据</returns>
            public static TmphPointer Create(byte[][] states)
            {
                if (states.length() != 0)
                {
                    var index = 0;
                    var strings = new TmphKeyValue<byte[], int>[states.Length];
                    foreach (var name in states)
                    {
                        if (name.Length >= 65536) TmphLog.Default.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                        strings[index].Set(name, index);
                        ++index;
                    }
                    strings = strings.sort(CompareHanlde);
                    return new TmphBuilder(strings).Data;
                }
                return new TmphPointer();
            }

            /// <summary>
            ///     状态数据创建器
            /// </summary>
            private struct TmphBuilder
            {
                /// <summary>
                ///     ASCII字符查找表
                /// </summary>
                private readonly byte* bytes;

                /// <summary>
                ///     状态字符数量
                /// </summary>
                private readonly int charCount;

                /// <summary>
                ///     状态数据
                /// </summary>
                public readonly TmphPointer Data;

                /// <summary>
                ///     空前缀
                /// </summary>
                private readonly byte* nullPrefix;

                /// <summary>
                ///     查询矩阵单位尺寸类型
                /// </summary>
                private readonly int tableType;

                /// <summary>
                ///     状态集合
                /// </summary>
                private readonly TmphKeyValue<byte[], int>[] values;

                /// <summary>
                ///     状态字符集合
                /// </summary>
                private TmphFixedMap chars;

                /// <summary>
                ///     前缀集合
                /// </summary>
                private byte* prefix;

                /// <summary>
                ///     前缀数量
                /// </summary>
                private int prefixSize;

                /// <summary>
                ///     状态集合
                /// </summary>
                private byte* state;

                /// <summary>
                ///     状态数量
                /// </summary>
                private int stateCount;

                /// <summary>
                ///     状态矩阵
                /// </summary>
                private byte* table;

                /// <summary>
                ///     矩阵状态数量
                /// </summary>
                private int tableCount;

                /// <summary>
                ///     状态数据创建器
                /// </summary>
                /// <param name="values">状态集合</param>
                public TmphBuilder(TmphKeyValue<byte[], int>[] values)
                {
                    this.values = values;
                    prefixSize = tableCount = stateCount = tableType = charCount = 0;
                    state = bytes = nullPrefix = prefix = table = null;
                    if (values.Length > 1)
                    {
                        byte* chars = stackalloc byte[256 >> 3];
                        this.chars = new TmphFixedMap(chars, 256 >> 3, 0);
                        Data = new TmphPointer();
                        count(0, values.Length, 0);
                        for (byte* start = chars, end = chars + (256 >> 3); start != end; start += sizeof(int))
                        {
                            charCount += (*(uint*)start).bitCount();
                        }
                        var size = (1 + (stateCount += tableCount) * 3) * sizeof(int) + 256 + 4 +
                                   ((prefixSize + 3) & (int.MaxValue - 3));
                        if (stateCount < 256) size += tableCount * charCount;
                        else if (stateCount < 65536)
                        {
                            size += tableCount * charCount * sizeof(ushort);
                            tableType = 1;
                        }
                        else
                        {
                            size += tableCount * charCount * sizeof(int);
                            tableType = 2;
                        }
                        Data = TmphUnmanaged.Get(size, true);
                        *Data.Int = stateCount; //状态数量[int]
                        state = Data.Byte + sizeof(int); //状态集合[stateCount*(前缀位置[int]+状态位置[int]+名称索引[int])]
                        bytes = state + (stateCount * 3) * sizeof(int); //字节查找表[256*byte]
                        byte charIndex = 0;
                        for (var index = 0; index != 256; ++index)
                        {
                            if (this.chars.Get(index)) *(bytes + index) = charIndex++;
                        }
                        nullPrefix = bytes + 256; //空前缀
                        table = nullPrefix + (((prefixSize + 3) & (int.MaxValue - 3)) + 4);
                        //状态矩阵[tableCount*charCount*[byte/ushort/int]]
                        *(ushort*)nullPrefix = (ushort)charCount; //字符数量
                        prefix = nullPrefix + sizeof(int) + sizeof(ushort); //前缀集合
                        nullPrefix += sizeof(int);
                        stateCount = 0;
                        create(0, values.Length, 0);
                    }
                    else
                    {
                        chars = new TmphFixedMap();
                        var value = values[0].Key;
                        fixed (byte* valueFixed = value)
                        {
                            if (values[0].Key.Length <= 254)
                            {
                                Data = TmphUnmanaged.Get(sizeof(int) + sizeof(int) * 3 + 256 + 2, false);
                                *Data.Int = 1; //状态数量
                                state = Data.Byte + sizeof(int);
                                *(int*)state = sizeof(int) * 3 + sizeof(ushort); //前缀位置
                                *(int*)(state + sizeof(int)) = 0; //状态位置
                                *(int*)(state + sizeof(int) * 2) = values[0].Value; //名称索引
                                prefix = Data.Byte + sizeof(int) * 4;
                                *(ushort*)prefix = (ushort)value.Length;
                                Unsafe.TmphMemory.Copy(valueFixed, prefix + sizeof(ushort), value.Length);
                                *(ushort*)(prefix + 256) = 0;
                            }
                            else
                            {
                                Data = TmphUnmanaged.Get(sizeof(int) + sizeof(int) * 3 + 256 + 4 + 2 + value.Length, true);
                                *Data.Int = 1; //状态数量
                                state = Data.Byte + sizeof(int);
                                *(int*)state = sizeof(int) * 3 + 256 + 4 + sizeof(ushort); //前缀位置
                                *(int*)(state + sizeof(int)) = 0; //状态位置
                                *(int*)(state + sizeof(int) * 2) = values[0].Value; //名称索引
                                prefix = Data.Byte + sizeof(int) * 4 + 256 + 4;
                                *(ushort*)prefix = (ushort)value.Length;
                                Unsafe.TmphMemory.Copy(valueFixed, prefix + sizeof(ushort), value.Length);
                            }
                        }
                    }
                }

                /// <summary>
                ///     计算状态数量
                /// </summary>
                /// <param name="start">起始名称位置</param>
                /// <param name="end">结束名称位置</param>
                /// <param name="current"></param>
                private void count(int start, int end, int current)
                {
                    ++tableCount;
                    int index = start, prefixSize = 0;
                    byte value = 0;
                    while (values[start].Key.Length != current)
                    {
                        value = values[start].Key[current];
                        while (++index != end && values[index].Key[current] == value) ;
                        if (index != end) break;
                        ++prefixSize;
                        index = start;
                        ++current;
                    }
                    if (prefixSize != 0) this.prefixSize += (prefixSize + 3) & (int.MaxValue - 1);
                    do
                    {
                        var count = index - start;
                        if (count == 0) index = ++start;
                        else
                        {
                            chars.Set(value);
                            if (count == 1)
                            {
                                ++stateCount;
                                prefixSize = values[start].Key.Length - current - 1;
                                if (prefixSize != 0) this.prefixSize += (prefixSize + 3) & (int.MaxValue - 1);
                            }
                            else this.count(start, index, current + 1);
                        }
                        if (index == end) break;
                        value = values[start = index].Key[current];
                        while (++index != end && values[index].Key[current] == value) ;
                    } while (true);
                }

                /// <summary>
                ///     创建状态数据
                /// </summary>
                /// <param name="start">起始名称位置</param>
                /// <param name="end">结束名称位置</param>
                /// <param name="current"></param>
                private void create(int start, int end, int current)
                {
                    byte* prefix = this.prefix, table = this.table;
                    *(int*)(state + sizeof(int)) = (int)(table - state);
                    var index = start;
                    byte value = 0;
                    if (values[start].Key.Length == current) *(int*)(state + sizeof(int) * 2) = values[start].Value;
                    else
                    {
                        do
                        {
                            value = values[index].Key[current];
                            while (++index != end && values[index].Key[current] == value) ;
                            if (index != end)
                            {
                                *(int*)(state + sizeof(int) * 2) = -1;
                                break;
                            }
                            *this.prefix++ = value;
                            if (values[index = start].Key.Length == ++current)
                            {
                                *(int*)(state + sizeof(int) * 2) = values[index].Value;
                                break;
                            }
                        } while (true);
                    }
                    var prefixSize = (int)(this.prefix - prefix);
                    if (prefixSize == 0) *(int*)state = (int)(nullPrefix - state);
                    else
                    {
                        *(ushort*)(prefix - sizeof(ushort)) = (ushort)prefixSize;
                        *(int*)state = (int)(prefix - state);
                        this.prefix += sizeof(ushort) + (prefixSize & 1);
                    }
                    state += sizeof(int) * 3;
                    ++stateCount;
                    if (tableType == 0) this.table += charCount;
                    else if (tableType == 1) this.table += charCount * sizeof(ushort);
                    else this.table += charCount * sizeof(int);
                    do
                    {
                        var count = index - start;
                        if (count == 0) index = ++start;
                        else
                        {
                            int charIndex = *(bytes + value);
                            if (tableType == 0) *(table + charIndex) = (byte)stateCount;
                            else if (tableType == 1)
                                *(ushort*)(table + charIndex * sizeof(ushort)) = (ushort)stateCount;
                            else *(int*)(table + charIndex * sizeof(int)) = stateCount * 3 * sizeof(int);
                            if (count == 1)
                            {
                                prefixSize = values[start].Key.Length - current - 1;
                                if (prefixSize == 0) *(int*)state = (int)(nullPrefix - state);
                                else
                                {
                                    *(int*)state = (int)(this.prefix - state);
                                    *(ushort*)(this.prefix - sizeof(ushort)) = (ushort)prefixSize;
                                    fixed (byte* charFixed = values[start].Key)
                                    {
                                        Unsafe.TmphMemory.Copy(charFixed + current + 1, this.prefix, prefixSize);
                                        this.prefix += (prefixSize + 3) & (int.MaxValue - 1);
                                    }
                                }
                                *(int*)(state + sizeof(int) * 2) = values[start].Value;
                                ++stateCount;
                                state += sizeof(int) * 3;
                            }
                            else create(start, index, current + 1);
                        }
                        if (index == end) break;
                        value = values[start = index].Key[current];
                        while (++index != end && values[index].Key[current] == value) ;
                    } while (true);
                }
            }
        }

        /// <summary>
        ///     字节数组状态搜索
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        public sealed class TmphByteArray<TValueType> : IDisposable
        {
            /// <summary>
            ///     状态搜索数据
            /// </summary>
            private TmphPointer data;

            /// <summary>
            ///     状态数据集合
            /// </summary>
            private TValueType[] values;

            /// <summary>
            ///     字节数组状态搜索
            /// </summary>
            /// <param name="states">状态集合</param>
            /// <param name="values">状态数据集合</param>
            public TmphByteArray(byte[][] states, TValueType[] values)
            {
                if (states == null || values == null) TmphLog.Default.Throw(TmphLog.TmphExceptionType.Null);
                if (states.Length > values.Length) TmphLog.Default.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                this.values = values;
                data = TmphByteArray.Create(states);
            }

            /// <summary>
            ///     释放资源
            /// </summary>
            public void Dispose()
            {
                TmphUnmanaged.Free(ref data);
                values = null;
            }

            /// <summary>
            ///     获取状态数据
            /// </summary>
            /// <param name="state">查询状态</param>
            /// <param name="nullValue">默认空值</param>
            /// <returns>状态数据,失败返回默认空值</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public TValueType Get(TmphSubArray<byte> state, TValueType nullValue = default(TValueType))
            {
                var index = new TmphByteArray(data).Search(state);
                return index >= 0 ? values[index] : nullValue;
            }

            /// <summary>
            ///     获取状态数据
            /// </summary>
            /// <param name="state">查询状态</param>
            /// <param name="value">目标数据</param>
            /// <returns>是否存在匹配状态数据</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public bool Get(TmphSubArray<byte> state, ref TValueType value)
            {
                var index = new TmphByteArray(data).Search(state);
                if (index >= 0)
                {
                    value = values[index];
                    return true;
                }
                return false;
            }

            /// <summary>
            ///     获取状态数据
            /// </summary>
            /// <param name="state">查询状态</param>
            /// <param name="length">状态长度</param>
            /// <param name="nullValue">默认空值</param>
            /// <returns>状态数据,失败返回默认空值</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public TValueType Get(void* state, int length, TValueType nullValue = default(TValueType))
            {
                var index = new TmphByteArray(data).Search((byte*)state, (byte*)state + length);
                return index >= 0 ? values[index] : nullValue;
            }
        }

        /// <summary>
        ///     字符搜索器
        /// </summary>
        internal struct TmphChars
        {
            /// <summary>
            ///     特殊字符串查找表结束位置
            /// </summary>
            private readonly byte* charEnd;

            /// <summary>
            ///     特殊字符起始值
            /// </summary>
            private readonly int charIndex;

            /// <summary>
            ///     ASCII字符查找表
            /// </summary>
            private readonly byte* charsAscii;

            /// <summary>
            ///     特殊字符串查找表
            /// </summary>
            private readonly byte* charStart;

            /// <summary>
            ///     状态集合
            /// </summary>
            private readonly byte* state;

            /// <summary>
            ///     查询矩阵单位尺寸类型
            /// </summary>
            private readonly byte tableType;

            /// <summary>
            ///     当前状态
            /// </summary>
            private byte* currentState;

            /// <summary>
            ///     字符搜索器
            /// </summary>
            /// <param name="data">数据起始位置</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public TmphChars(TmphPointer data)
            {
                if (data.Data == null)
                {
                    state = charsAscii = charStart = charEnd = currentState = null;
                    charIndex = 0;
                    tableType = 0;
                }
                else
                {
                    var stateCount = *data.Int;
                    currentState = state = data.Byte + sizeof(int);
                    charsAscii = state + stateCount * 3 * sizeof(int);
                    charStart = charsAscii + 128 * sizeof(ushort);
                    charIndex = *(ushort*)charStart;
                    charStart += sizeof(ushort) * 2;
                    charEnd = charStart + *(ushort*)(charStart - sizeof(ushort)) * sizeof(ushort);
                    if (stateCount < 256) tableType = 0;
                    else if (stateCount < 65536) tableType = 1;
                    else tableType = 2;
                }
            }

            /// <summary>
            ///     获取状态索引
            /// </summary>
            /// <param name="start">匹配起始位置</param>
            /// <param name="end">匹配结束位置</param>
            /// <returns>状态索引,失败返回-1</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public int Search(char* start, char* end)
            {
                if (state == null || start >= end) return -1;
                currentState = state;
                do
                {
                    var prefix = (char*)(currentState + *(int*)currentState);
                    if (*prefix != 0)
                    {
                        if (start == end || *start != *prefix) return -1;
                        while (*++prefix != 0)
                        {
                            if (++start == end || *start != *prefix) return -1;
                        }
                        ++start;
                    }
                    if (start == end) return *(int*)(currentState + sizeof(int) * 2);
                    if (*(int*)(currentState + sizeof(int)) == 0) return -1;
                    var index = *start < 128 ? *(ushort*)(charsAscii + (*start << 1)) : getCharIndex(*start);
                    var table = currentState + *(int*)(currentState + sizeof(int));
                    if (tableType == 0)
                    {
                        if ((index = *(table + index)) == 0) return -1;
                        currentState = state + index * 3 * sizeof(int);
                    }
                    else if (tableType == 1)
                    {
                        if ((index = *(ushort*)(table + index * sizeof(ushort))) == 0) return -1;
                        currentState = state + index * 3 * sizeof(int);
                    }
                    else
                    {
                        if ((index = *(int*)(table + index * sizeof(int))) == 0) return -1;
                        currentState = state + index;
                    }
                    ++start;
                } while (true);
            }

            /// <summary>
            ///     获取状态索引
            /// </summary>
            /// <param name="value"></param>
            /// <returns>状态索引,失败返回-1</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            public int Search(string value)
            {
                if (value == null) return -1;
                fixed (char* valueFixed = value) return Search(valueFixed, valueFixed + value.Length);
            }

            /// <summary>
            ///     获取特殊字符索引值
            /// </summary>
            /// <param name="value">特殊字符</param>
            /// <returns>索引值,匹配失败返回0</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private int getCharIndex(char value)
            {
                var current = GetCharIndex((char*)charStart, (char*)charEnd, value);
                return current == null ? 0 : (charIndex + (int)(current - (char*)charStart));
            }

            /// <summary>
            ///     获取特殊字符索引值
            /// </summary>
            /// <param name="charStart">特殊字符串查找表</param>
            /// <param name="charEnd">特殊字符串查找表结束位置</param>
            /// <param name="value">特殊字符</param>
            /// <returns>特殊字符位置,匹配失败返回null</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal static char* GetCharIndex(char* charStart, char* charEnd, char value)
            {
                var current = charStart + ((int)(charEnd - charStart) >> 1);
                while (*current != value)
                {
                    if (value < *current)
                    {
                        if (current == charStart) return null;
                        charEnd = current;
                        current = charStart + ((int)(charEnd - charStart) >> 1);
                    }
                    else
                    {
                        if ((charStart = current + 1) == charEnd) return null;
                        current = charStart + ((int)(charEnd - charStart) >> 1);
                    }
                }
                return current;
            }

            /// <summary>
            ///     创建名称查找数据
            /// </summary>
            /// <param name="names">名称集合</param>
            /// <returns>名称查找数据</returns>
            public static TmphPointer Create(string[] names)
            {
                var index = 0;
                var strings = new TmphKeyValue<string, int>[names.Length];
                foreach (var name in names)
                {
                    strings[index].Set(name, index);
                    ++index;
                }
                strings = strings.sort(StringCompare);
                return new TmphBuilder(strings).Data;
            }

            /// <summary>
            ///     状态数据创建器
            /// </summary>
            private struct TmphBuilder
            {
                /// <summary>
                ///     特殊字符串查找表结束位置
                /// </summary>
                private readonly byte* charEnd;

                /// <summary>
                ///     状态字符集合
                /// </summary>
                private readonly TmphList<char> chars;

                /// <summary>
                ///     ASCII字符查找表
                /// </summary>
                private readonly byte* charsAscii;

                /// <summary>
                ///     特殊字符串查找表
                /// </summary>
                private readonly byte* charStart;

                /// <summary>
                ///     状态数据
                /// </summary>
                public readonly TmphPointer Data;

                /// <summary>
                ///     名称集合
                /// </summary>
                private readonly TmphKeyValue<string, int>[] names;

                /// <summary>
                ///     查询矩阵单位尺寸类型
                /// </summary>
                private readonly int tableType;

                /// <summary>
                ///     前缀集合
                /// </summary>
                private byte* prefix;

                /// <summary>
                ///     前缀数量
                /// </summary>
                private int prefixSize;

                /// <summary>
                ///     状态集合
                /// </summary>
                private byte* state;

                /// <summary>
                ///     状态数量
                /// </summary>
                private int stateCount;

                /// <summary>
                ///     状态矩阵
                /// </summary>
                private byte* table;

                /// <summary>
                ///     矩阵状态数量
                /// </summary>
                private int tableCount;

                /// <summary>
                ///     状态数据创建器
                /// </summary>
                /// <param name="names">名称集合</param>
                public TmphBuilder(TmphKeyValue<string, int>[] names)
                {
                    this.names = names;
                    prefixSize = tableCount = stateCount = tableType = 0;
                    state = charsAscii = charStart = charEnd = prefix = table = null;
                    if (names.Length > 1)
                    {
                        chars = new TmphList<char>();
                        Data = new TmphPointer();
                        count(0, names.Length, 0);
                        var charArray = chars.array;
                        int charCount, asciiCount;
                        Array.Sort(charArray, 0, chars.Count);
                        fixed (char* charFixed = charArray)
                        {
                            char* start = charFixed + 1, end = charFixed + chars.Count, write = start;
                            var value = *charFixed;
                            if (*(end - 1) < 128)
                            {
                                while (start != end)
                                {
                                    if (*start != value) *write++ = value = *start;
                                    ++start;
                                }
                                asciiCount = (int)(write - charFixed);
                                charCount = 0;
                            }
                            else
                            {
                                while (value < 128)
                                {
                                    while (*start == value) ++start;
                                    *write++ = value = *start++;
                                }
                                asciiCount = (int)(write - charFixed) - 1;
                                while (start != end)
                                {
                                    if (*start != value) *write++ = value = *start;
                                    ++start;
                                }
                                charCount = (int)(write - charFixed) - asciiCount;
                            }
                            chars.Unsafer.AddLength(asciiCount + charCount - chars.Count);
                            var size = (1 + (stateCount += tableCount) * 3) * sizeof(int) +
                                       (128 + 2 + charCount + prefixSize) * sizeof(ushort);
                            if (stateCount < 256) size += tableCount * (chars.Count + 1);
                            else if (stateCount < 65536)
                            {
                                size += tableCount * (chars.Count + 1) * sizeof(ushort);
                                tableType = 1;
                            }
                            else
                            {
                                size += tableCount * (chars.Count + 1) * sizeof(int);
                                tableType = 2;
                            }
                            Data = TmphUnmanaged.Get(size, true);
                            *Data.Int = stateCount; //状态数量[int]
                            state = Data.Byte + sizeof(int); //状态集合[stateCount*(前缀位置[int]+状态位置[int]+名称索引[int])]
                            charsAscii = state + (stateCount * 3) * sizeof(int); //ascii字符查找表[128*ushort]
                            charStart = charsAscii + 128 * sizeof(ushort);
                            *(ushort*)charStart = (ushort)(asciiCount + 1); //特殊字符起始值[ushort]
                            *(ushort*)(charStart + sizeof(ushort)) = (ushort)charCount; //特殊字符数量[ushort]
                            charStart += sizeof(ushort) * 2;
                            ushort charIndex = 0;
                            for (start = charFixed, end = charFixed + asciiCount; start != end; ++start)
                            {
                                *(ushort*)(charsAscii + (*start << 1)) = ++charIndex;
                            }
                            charEnd = charStart;
                            if (charCount != 0)
                            {
                                //特殊字符二分查找表[charCount*char]
                                Unsafe.TmphMemory.Copy((byte*)start, charStart, charCount << 1);
                                charEnd += charCount << 1;
                            }
                            prefix = charStart + charCount * sizeof(ushort); //前缀集合
                            table = prefix + prefixSize * sizeof(ushort);
                            //状态矩阵[tableCount*(chars.Count+1)*[byte/ushort/int]]
                        }
                        stateCount = 0;
                        create(0, names.Length, 0);
                    }
                    else
                    {
                        chars = null;
                        if (names.Length == 0) Data = new TmphPointer();
                        else if (names[0].Key.Length <= 128)
                        {
                            Data =
                                TmphUnmanaged.Get(
                                    sizeof(int) + sizeof(int) * 3 + 128 * sizeof(ushort) + 2 * sizeof(ushort), false);
                            *Data.Int = 1; //状态数量
                            state = Data.Byte + sizeof(int);
                            *(int*)state = sizeof(int) * 3; //前缀位置
                            *(int*)(state + sizeof(int)) = 0; //状态位置
                            *(int*)(state + sizeof(int) * 2) = names[0].Value; //名称索引
                            prefix = Data.Byte + sizeof(int) * 4;
                            Unsafe.TmphString.Copy(names[0].Key, prefix);
                            *(char*)(prefix + (names[0].Key.Length << 1)) = (char)0;
                            *(int*)(Data.Byte + sizeof(int) * 4 + 128 * sizeof(ushort)) = 0;
                        }
                        else
                        {
                            Data =
                                TmphUnmanaged.Get(
                                    sizeof(int) + sizeof(int) * 3 + 128 * sizeof(ushort) + 2 * sizeof(ushort) +
                                    names[0].Key.Length * sizeof(char) + sizeof(char), true);
                            *Data.Int = 1; //状态数量
                            state = Data.Byte + sizeof(int);
                            *(int*)state = sizeof(int) * 3 + 128 * sizeof(ushort) + 2 * sizeof(ushort); //前缀位置
                            *(int*)(state + sizeof(int)) = 0; //状态位置
                            *(int*)(state + sizeof(int) * 2) = names[0].Value; //名称索引
                            Unsafe.TmphString.Copy(names[0].Key, state + *(int*)state);
                        }
                    }
                }

                /// <summary>
                ///     计算状态数量
                /// </summary>
                /// <param name="start">起始名称位置</param>
                /// <param name="end">结束名称位置</param>
                /// <param name="current"></param>
                private void count(int start, int end, int current)
                {
                    ++tableCount;
                    int index = start, prefixSize = 0;
                    var value = (char)0;
                    while (names[start].Key.Length != current)
                    {
                        value = names[start].Key[current];
                        while (++index != end && names[index].Key[current] == value) ;
                        if (index != end) break;
                        ++prefixSize;
                        index = start;
                        ++current;
                    }
                    if (prefixSize != 0) this.prefixSize += prefixSize + 1;
                    do
                    {
                        var count = index - start;
                        if (count == 0) index = ++start;
                        else
                        {
                            if (value == 0) TmphLog.Default.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                            chars.Add(value);
                            if (count == 1)
                            {
                                ++stateCount;
                                prefixSize = names[start].Key.Length - current - 1;
                                if (prefixSize != 0) this.prefixSize += prefixSize + 1;
                            }
                            else this.count(start, index, current + 1);
                        }
                        if (index == end) break;
                        value = names[start = index].Key[current];
                        while (++index != end && names[index].Key[current] == value) ;
                    } while (true);
                }

                /// <summary>
                ///     创建状态数据
                /// </summary>
                /// <param name="start">起始名称位置</param>
                /// <param name="end">结束名称位置</param>
                /// <param name="current"></param>
                private void create(int start, int end, int current)
                {
                    byte* prefix = this.prefix, table = this.table;
                    *(int*)(state + sizeof(int)) = (int)(table - state);
                    var index = start;
                    var value = (char)0;
                    if (names[start].Key.Length == current) *(int*)(state + sizeof(int) * 2) = names[start].Value;
                    else
                    {
                        do
                        {
                            value = names[index].Key[current];
                            while (++index != end && names[index].Key[current] == value) ;
                            if (index != end)
                            {
                                *(int*)(state + sizeof(int) * 2) = -1;
                                break;
                            }
                            *(char*)this.prefix = value;
                            this.prefix += 2;
                            if (names[index = start].Key.Length == ++current)
                            {
                                *(int*)(state + sizeof(int) * 2) = names[index].Value;
                                break;
                            }
                        } while (true);
                    }
                    if (prefix == this.prefix) *(int*)state = (int)(charsAscii - state);
                    else
                    {
                        *(char*)this.prefix = (char)0;
                        *(int*)state = (int)(prefix - state);
                        this.prefix += 2;
                    }
                    state += sizeof(int) * 3;
                    ++stateCount;
                    if (tableType == 0) this.table += chars.Count + 1;
                    else if (tableType == 1) this.table += (chars.Count + 1) * sizeof(ushort);
                    else this.table += (chars.Count + 1) * sizeof(int);
                    do
                    {
                        var count = index - start;
                        if (count == 0) index = ++start;
                        else
                        {
                            int charIndex;
                            if (value < 128) charIndex = *(ushort*)(charsAscii + (value << 1));
                            else
                            {
                                char* charStart = (char*)this.charStart,
                                    charEnd = (char*)this.charEnd,
                                    charCurrent = charStart + ((int)(charEnd - charStart) >> 1);
                                while (*charCurrent != value)
                                {
                                    if (value < *charCurrent) charEnd = charCurrent;
                                    else charStart = charCurrent + 1;
                                    charCurrent = charStart + ((int)(charEnd - charStart) >> 1);
                                }
                                charIndex = *(ushort*)(this.charStart - sizeof(int)) +
                                            (int)(charCurrent - (char*)this.charStart);
                            }
                            if (tableType == 0) *(table + charIndex) = (byte)stateCount;
                            else if (tableType == 1)
                                *(ushort*)(table + charIndex * sizeof(ushort)) = (ushort)stateCount;
                            else *(int*)(table + charIndex * sizeof(int)) = stateCount * 3 * sizeof(int);
                            if (count == 1)
                            {
                                var prefixSize = names[start].Key.Length - current - 1;
                                if (prefixSize == 0) *(int*)state = (int)(charsAscii - state);
                                else
                                {
                                    *(int*)state = (int)(this.prefix - state);
                                    fixed (char* charFixed = names[start].Key)
                                    {
                                        Unsafe.TmphMemory.Copy(charFixed + current + 1, this.prefix, prefixSize <<= 1);
                                        *(char*)(this.prefix += prefixSize) = (char)0;
                                        this.prefix += sizeof(char);
                                    }
                                }
                                *(int*)(state + sizeof(int) * 2) = names[start].Value;
                                ++stateCount;
                                state += sizeof(int) * 3;
                            }
                            else create(start, index, current + 1);
                        }
                        if (index == end) break;
                        value = names[start = index].Key[current];
                        while (++index != end && names[index].Key[current] == value) ;
                    } while (true);
                }
            }
        }
    }
}