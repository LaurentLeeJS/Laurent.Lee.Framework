using System;

namespace Laurent.Lee.CLB.Algorithm
{
    /// <summary>
    ///     并查集聚类
    /// </summary>
    public static class TmphGroup
    {
        /// <summary>
        ///     数据分组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="values">数据组合集合</param>
        /// <returns>数据分组集合</returns>
        public static unsafe TmphGroupResult<TValueType> Groups<TValueType>(TmphData<TValueType>[] values)
            where TValueType : IEquatable<TValueType>
        {
            if (values.length() == 0) return default(TmphGroupResult<TValueType>);
            var bufferLength = values.Length * 8 + 3;
            var buffer = TmphMemoryPool.StreamBuffers.Get(bufferLength * sizeof(int));
            try
            {
                fixed (byte* bufferFixed = buffer)
                {
                    #region 数字化

                    int* indexFixed = (int*)bufferFixed, numberFixed = (int*)bufferFixed;
                    var numberHash = TmphDictionary<TValueType>.Create<int>(values.Length);
                    int indexCount = 0, sum;
                    foreach (var value in values)
                    {
                        if (!numberHash.TryGetValue(value.Value1, out *numberFixed))
                            numberHash.Add(value.Value1, *numberFixed = indexCount++);
                        ++numberFixed;
                        if (!numberHash.TryGetValue(value.Value2, out *numberFixed))
                            numberHash.Add(value.Value2, *numberFixed = indexCount++);
                        ++numberFixed;
                    }
                    numberHash = null;

                    #endregion 数字化

                    #region 数量统计

                    var countFixed = numberFixed + (values.Length << 1);
                    Array.Clear(buffer, (int)((byte*)countFixed - bufferFixed), indexCount * sizeof(int));
                    for (var start = indexFixed; start != numberFixed; ++countFixed[*start++])
                    {
                    }

                    #endregion 数量统计

                    #region 桶排序建图

                    var groupFixed = countFixed + indexCount;
                    sum = *countFixed;
                    for (var start = countFixed; ++start != groupFixed; *start = (sum += *start))
                    {
                    }
                    *groupFixed++ = sum;
                    for (var start = indexFixed; start != numberFixed; ++start)
                    {
                        int index1 = *start, index2 = *++start;
                        numberFixed[--countFixed[index1]] = index2;
                        numberFixed[--countFixed[index2]] = index1;
                    }

                    #endregion 桶排序建图

                    #region 深搜分组统计

                    int* groupEnd = groupFixed + indexCount, bufferEnd = indexFixed + bufferLength;
                    Array.Clear(buffer, (int)((byte*)groupFixed - bufferFixed), indexCount * sizeof(int));
                    var groupCount = 1;
                    for (int* group = groupFixed, searchFixed = bufferEnd; group != groupEnd; ++group)
                    {
                        if (*group == 0)
                        {
                            *--searchFixed = (int)(group - groupFixed);
                            *group = groupCount;
                            do
                            {
                                var index = *searchFixed++;
                                for (
                                    int* start = numberFixed + countFixed[index],
                                        end = numberFixed + countFixed[index + 1];
                                    start != end;
                                    ++start)
                                {
                                    if (groupFixed[index = *start] == 0)
                                    {
                                        *--searchFixed = index;
                                        groupFixed[index] = groupCount;
                                    }
                                }
                            } while (searchFixed != bufferEnd);
                            ++groupCount;
                        }
                    }

                    #endregion 深搜分组统计

                    #region 反数字化

                    Array.Clear(buffer, (int)((byte*)(countFixed--) - bufferFixed), (groupCount - 1) * sizeof(int));
                    for (var start = indexFixed; start != numberFixed; start += 2)
                        ++countFixed[*start = groupFixed[*start]];
                    var result = new TmphGroupResult<TValueType>
                    {
                        Values = new TmphData<TValueType>[values.Length],
                        Indexs = new int[groupCount]
                    };
                    fixed (int* resultFixed = result.Indexs)
                    {
                        for (sum = 0, groupFixed = countFixed + groupCount, groupEnd = resultFixed;
                            ++countFixed != groupFixed;
                            *groupEnd++ = (sum += *countFixed))
                        {
                        }
                        groupFixed = indexFixed;
                        countFixed = resultFixed - 1;
                        *groupEnd = values.Length;
                        foreach (var value in values)
                        {
                            result.Values[--countFixed[*groupFixed]] = value;
                            groupFixed += 2;
                        }
                    }
                    return result;

                    #endregion 反数字化
                }
            }
            finally
            {
                TmphMemoryPool.StreamBuffers.Push(ref buffer);
            }
        }

        /// <summary>
        ///     数据分组
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="values">数据组合集合</param>
        /// <returns>数据分组集合</returns>
        public static unsafe TmphResult<TValueType> Values<TValueType>(TmphData<TValueType>[] values)
            where TValueType : IEquatable<TValueType>
        {
            if (values.length() == 0) return default(TmphResult<TValueType>);
            var buffer = TmphMemoryPool.StreamBuffers.Get((values.Length * 7 + ((values.Length + 7) >> 1)) * sizeof(int));
            try
            {
                fixed (byte* bufferFixed = buffer)
                {
                    #region 数字化

                    int* indexFixed = (int*)bufferFixed, numberFixed = (int*)bufferFixed;
                    var numberHash = TmphDictionary<TValueType>.Create<int>(values.Length);
                    var indexValues = new TValueType[values.Length << 1];
                    int indexCount = 0, sum;
                    foreach (var value in values)
                    {
                        if (!numberHash.TryGetValue(value.Value1, out *numberFixed))
                        {
                            indexValues[indexCount] = value.Value1;
                            numberHash.Add(value.Value1, *numberFixed = indexCount++);
                        }
                        ++numberFixed;
                        if (!numberHash.TryGetValue(value.Value2, out *numberFixed))
                        {
                            indexValues[indexCount] = value.Value2;
                            numberHash.Add(value.Value2, *numberFixed = indexCount++);
                        }
                        ++numberFixed;
                    }
                    numberHash = null;

                    #endregion 数字化

                    #region 数量统计

                    var countFixed = numberFixed + (values.Length << 1);
                    Array.Clear(buffer, (int)((byte*)countFixed - bufferFixed), indexCount * sizeof(int));
                    for (var start = indexFixed; start != numberFixed; ++countFixed[*start++])
                    {
                    }

                    #endregion 数量统计

                    #region 桶排序建图

                    var groupFixed = (byte*)(countFixed + indexCount);
                    sum = *countFixed;
                    for (var start = countFixed; ++start != groupFixed; *start = (sum += *start))
                    {
                    }
                    *(int*)groupFixed = sum;
                    groupFixed += sizeof(int);
                    for (var start = indexFixed; start != numberFixed; ++start)
                    {
                        int index1 = *start, index2 = *++start;
                        numberFixed[--countFixed[index1]] = index2;
                        numberFixed[--countFixed[index2]] = index1;
                    }

                    #endregion 桶排序建图

                    #region 深搜分组统计

                    int* groupIndexFixed = (int*)(groupFixed + ((indexCount + 3) & (int.MaxValue - 3))),
                        groupIndexEnd = groupIndexFixed,
                        searchFixed = indexFixed;
                    Array.Clear(buffer, (int)(groupFixed - bufferFixed), ((indexCount + 3) >> 2) << 2);
                    var result = new TmphResult<TValueType> { Values = new TValueType[indexCount] };
                    var resultIndex = 0;
                    for (byte* group = groupFixed, groupEnd = groupFixed + indexCount; group != groupEnd; ++group)
                    {
                        if (*group == 0)
                        {
                            var index = (int)(group - groupFixed);
                            *groupIndexEnd++ = resultIndex;
                            *group = 1;
                            *searchFixed++ = index;
                            result.Values[resultIndex++] = indexValues[index];
                            do
                            {
                                for (
                                    int* start = numberFixed + countFixed[index = *--searchFixed],
                                        end = numberFixed + countFixed[index + 1];
                                    start != end;
                                    ++start)
                                {
                                    if (groupFixed[index = *start] == 0)
                                    {
                                        result.Values[resultIndex++] = indexValues[index];
                                        *searchFixed++ = index;
                                        groupFixed[index] = 1;
                                    }
                                }
                            } while (searchFixed != indexFixed);
                        }
                    }
                    indexValues = null;
                    var groupCount = (int)(groupIndexEnd - groupIndexFixed);
                    fixed (int* resultFixed = result.Indexs = new int[groupCount + 1])
                    {
                        Unsafe.TmphMemory.Copy(groupIndexFixed, resultFixed, groupCount << 2);
                        resultFixed[groupCount] = indexCount;
                    }
                    return result;

                    #endregion 深搜分组统计
                }
            }
            finally
            {
                TmphMemoryPool.StreamBuffers.Push(ref buffer);
            }
        }

        /// <summary>
        ///     数据组合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        public struct TmphData<TValueType> where TValueType : IEquatable<TValueType>
        {
            /// <summary>
            ///     数据值
            /// </summary>
            public TValueType Value1;

            /// <summary>
            ///     数据值
            /// </summary>
            public TValueType Value2;
        }

        /// <summary>
        ///     数据组合分组结果
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        public struct TmphGroupResult<TValueType> where TValueType : IEquatable<TValueType>
        {
            /// <summary>
            ///     数据组合分组索引集合
            /// </summary>
            public int[] Indexs;

            /// <summary>
            ///     数据组合集合
            /// </summary>
            public TmphData<TValueType>[] Values;
        }

        /// <summary>
        ///     数据分组结果
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        public struct TmphResult<TValueType> where TValueType : IEquatable<TValueType>
        {
            /// <summary>
            ///     数据分组索引集合
            /// </summary>
            public int[] Indexs;

            /// <summary>
            ///     数据集合
            /// </summary>
            public TValueType[] Values;

            /// <summary>
            ///     分组数量
            /// </summary>
            public int Count
            {
                get { return Indexs != null ? Indexs.Length - 1 : 0; }
            }

            /// <summary>
            ///     获取数据分组
            /// </summary>
            /// <param name="index">数据分组索引</param>
            /// <returns>数据分组</returns>
            public TValueType[] GetArray(int index)
            {
                if ((uint)index < Indexs.Length - 1)
                {
                    int startIndex = Indexs[index], length = Indexs[index + 1] - startIndex;
                    var values = new TValueType[length];
                    Array.Copy(Values, startIndex, values, 0, length);
                    return values;
                }
                return TmphNullValue<TValueType>.Array;
            }
        }
    }
}