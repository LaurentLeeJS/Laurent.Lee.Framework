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
using Laurent.Lee.CLB.Threading;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using pub = Laurent.Lee.CLB.Config.TmphPub;

namespace Laurent.Lee.CLB.Code
{
    /// <summary>
    ///     成员位图
    /// </summary>
    public abstract unsafe class TmphMemberMap : IEquatable<TmphMemberMap>, IDisposable
    {
        /// <summary>
        ///     判断成员位图是否匹配成员索引
        /// </summary>
        internal static readonly MethodInfo IsMemberMethod = typeof(TmphMemberMap).GetMethod("IsMember",
            BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(int) }, null);

        /// <summary>
        ///     成员位图
        /// </summary>
        /// <param name="type">成员位图类型信息</param>
        internal TmphMemberMap(TmphType type)
        {
            Type = type;
        }

        /// <summary>
        ///     成员位图类型信息
        /// </summary>
        internal TmphType Type { get; private set; }

        /// <summary>
        ///     是否默认全部成员有效
        /// </summary>
        public abstract bool IsDefault { get; }

        /// <summary>
        ///     释放资源
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        ///     比较是否相等
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract bool Equals(TmphMemberMap other);

        /// <summary>
        ///     添加所有成员
        /// </summary>
        internal abstract void Full();

        /// <summary>
        ///     清空所有成员
        /// </summary>
        internal abstract void Empty();

        /// <summary>
        ///     字段成员序列化
        /// </summary>
        /// <param name="stream"></param>
        internal abstract void FieldSerialize(TmphUnmanagedStream stream);

        /// <summary>
        ///     字段成员反序列化
        /// </summary>
        /// <param name="deSerializer"></param>
        internal abstract void FieldDeSerialize(TmphBinaryDeSerializer deSerializer);

        /// <summary>
        ///     设置成员索引,忽略默认成员
        /// </summary>
        /// <param name="memberIndex">成员索引</param>
        internal abstract void SetMember(int memberIndex);

        /// <summary>
        ///     设置成员索引,忽略默认成员
        /// </summary>
        /// <param name="memberName">成员名称</param>
        /// <returns>是否成功</returns>
        internal bool SetMember(string memberName)
        {
            var index = Type.GetMemberIndex(memberName);
            if (index >= 0)
            {
                SetMember(index);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     设置成员索引,忽略默认成员
        /// </summary>
        /// <param name="member">成员</param>
        /// <returns>是否成功</returns>
        internal bool SetMember(LambdaExpression member)
        {
            return SetMember(GetFieldName(member));
        }

        /// <summary>
        ///     清除所有成员
        /// </summary>
        internal abstract void Clear();

        /// <summary>
        ///     清除成员索引,忽略默认成员
        /// </summary>
        /// <param name="memberIndex">成员索引</param>
        internal abstract void ClearMember(int memberIndex);

        ///// <summary>
        ///// 清除成员索引,忽略默认成员
        ///// </summary>
        ///// <param name="memberName">成员名称</param>
        //public void ClearMember(string memberName)
        //{
        //    int index = Type.GetMemberIndex(memberName);
        //    if (index >= 0) ClearMember(index);
        //}
        /// <summary>
        ///     判断成员索引是否有效
        /// </summary>
        /// <param name="memberIndex">成员索引</param>
        /// <returns>成员索引是否有效</returns>
        internal abstract bool IsMember(int memberIndex);

        ///// <summary>
        ///// 判断成员索引是否有效
        ///// </summary>
        ///// <param name="memberName">成员名称</param>
        ///// <returns>成员索引是否有效</returns>
        //public bool IsMember(string memberName)
        //{
        //    int index = Type.GetMemberIndex(memberName);
        //    return index >= 0 && IsMember(index);
        //}
        /// <summary>
        ///     成员位图
        /// </summary>
        /// <returns>成员位图</returns>
        public abstract TmphMemberMap Copy();

        /// <summary>
        ///     成员交集运算
        /// </summary>
        /// <param name="memberMap">成员位图</param>
        public abstract void And(TmphMemberMap memberMap);

        /// <summary>
        ///     成员异或运算,忽略默认成员
        /// </summary>
        /// <param name="memberMap">成员位图</param>
        public abstract void Xor(TmphMemberMap memberMap);

        /// <summary>
        ///     成员并集运算,忽略默认成员
        /// </summary>
        /// <param name="memberMap">成员位图</param>
        public abstract void Or(TmphMemberMap memberMap);

        /// <summary>
        ///     获取成员名称
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        internal static string GetFieldName(LambdaExpression member)
        {
            var expression = member.Body;
            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var field = ((MemberExpression)expression).Member as FieldInfo;
                if (field != null) return field.Name;
            }
            return null;
        }

        /// <summary>
        ///     成员位图类型信息
        /// </summary>
        internal sealed class TmphType
        {
            /// <summary>
            ///     成员位图内存池
            /// </summary>
            internal TmphPoint.THPool Pool;

            /// <summary>
            ///     成员位图类型信息
            /// </summary>
            /// <param name="members">成员索引集合</param>
            /// <param name="fieldCount">字段成员数量</param>
            public TmphType(Code.TmphMemberIndex[] members, int fieldCount)
            {
                FieldCount = fieldCount;
                if ((MemberCount = members.Length) < 64) MemberMapSize = MemberCount < 32 ? 4 : 8;
                else MemberMapSize = ((MemberCount + 63) >> 6) << 3;
                NameIndexSearcher = TmphStateSearcher.TmphChars.Create(members.getArray(value => value.Member.Name));
                if (MemberCount >= 64)
                {
                    Pool = TmphPoint.THPool.GetPool(MemberMapSize);
                    FieldSerializeSize = ((fieldCount + 31) >> 5) << 2;
                }
            }

            /// <summary>
            ///     名称索引查找数据
            /// </summary>
            public TmphPointer NameIndexSearcher { get; private set; }

            /// <summary>
            ///     成员数量
            /// </summary>
            public int MemberCount { get; private set; }

            /// <summary>
            ///     字段成员数量
            /// </summary>
            public int FieldCount { get; private set; }

            /// <summary>
            ///     成员位图字节数量
            /// </summary>
            public int MemberMapSize { get; private set; }

            /// <summary>
            ///     字段成员位图序列化字节数量
            /// </summary>
            public int FieldSerializeSize { get; private set; }

            /// <summary>
            ///     获取成员索引
            /// </summary>
            /// <param name="name">成员名称</param>
            /// <returns>成员索引,失败返回-1</returns>
            public int GetMemberIndex(string name)
            {
                return name != null ? new TmphStateSearcher.TmphChars(NameIndexSearcher).Search(name) : -1;
            }
        }

        /// <summary>
        ///     成员位图
        /// </summary>
        internal sealed class TmphValue : TmphMemberMap
        {
            /// <summary>
            ///     成员位图
            /// </summary>
            private ulong _map;

            /// <summary>
            ///     成员位图
            /// </summary>
            /// <param name="type">成员位图类型信息</param>
            public TmphValue(TmphType type) : base(type)
            {
            }

            /// <summary>
            ///     是否默认全部成员有效
            /// </summary>
            public override bool IsDefault
            {
                get { return _map == 0; }
            }

            /// <summary>
            ///     比较是否相等
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public override bool Equals(TmphMemberMap other)
            {
                var value = other as TmphValue;
                return value != null && _map == value._map;
            }

            /// <summary>
            ///     添加所有成员
            /// </summary>
            internal override void Full()
            {
                _map = ulong.MaxValue;
            }

            /// <summary>
            ///     清空所有成员
            /// </summary>
            internal override void Empty()
            {
                _map = 0x8000000000000000UL;
            }

            /// <summary>
            ///     序列化
            /// </summary>
            /// <param name="stream"></param>
            internal override void FieldSerialize(TmphUnmanagedStream stream)
            {
                if (_map == 0) stream.Write(0);
                else
                {
                    stream.PrepLength(sizeof(int) + sizeof(ulong));
                    var data = stream.CurrentData;
                    *(int*)data = Type.FieldCount;
                    *(ulong*)(data + sizeof(int)) = _map;
                    stream.Unsafer.AddLength(sizeof(int) + sizeof(ulong));
                    stream.PrepLength();
                }
            }

            /// <summary>
            ///     字段成员反序列化
            /// </summary>
            /// <param name="deSerializer"></param>
            internal override void FieldDeSerialize(TmphBinaryDeSerializer deSerializer)
            {
                _map = deSerializer.DeSerializeMemberMap(Type.FieldCount);
            }

            /// <summary>
            ///     设置成员索引,忽略默认成员
            /// </summary>
            /// <param name="memberIndex">成员索引</param>
            internal override void SetMember(int memberIndex)
            {
                _map |= (1UL << memberIndex) | 0x8000000000000000UL;
            }

            /// <summary>
            ///     清除所有成员
            /// </summary>
            internal override void Clear()
            {
                _map = 0;
            }

            /// <summary>
            ///     清除成员索引,忽略默认成员
            /// </summary>
            /// <param name="memberIndex">成员索引</param>
            internal override void ClearMember(int memberIndex)
            {
                _map &= (1UL << memberIndex) ^ ulong.MaxValue;
            }

            /// <summary>
            ///     判断成员索引是否有效
            /// </summary>
            /// <param name="memberIndex">成员索引</param>
            /// <returns>成员索引是否有效</returns>
            internal override bool IsMember(int memberIndex)
            {
                return _map == 0 || (_map & (1UL << memberIndex)) != 0;
            }

            /// <summary>
            ///     成员位图
            /// </summary>
            /// <returns>成员位图</returns>
            public override TmphMemberMap Copy()
            {
                var value = new TmphValue(Type);
                value._map = _map;
                return value;
            }

            /// <summary>
            ///     成员交集运算
            /// </summary>
            /// <param name="memberMap">成员位图</param>
            public override void And(TmphMemberMap memberMap)
            {
                if (memberMap == null || memberMap.IsDefault) return;
                if (Type != memberMap.Type) TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
                if (_map == 0) _map = ((TmphValue)memberMap)._map;
                else _map &= ((TmphValue)memberMap)._map;
            }

            /// <summary>
            ///     成员异或运算,忽略默认成员
            /// </summary>
            /// <param name="memberMap">成员位图</param>
            public override void Xor(TmphMemberMap memberMap)
            {
                if (memberMap == null || memberMap.IsDefault || _map == 0) return;
                if (Type != memberMap.Type) TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
                _map ^= ((TmphValue)memberMap)._map;
                _map |= 0x8000000000000000UL;
            }

            /// <summary>
            ///     成员并集运算,忽略默认成员
            /// </summary>
            /// <param name="memberMap">成员位图</param>
            public override void Or(TmphMemberMap memberMap)
            {
                if (_map != 0)
                {
                    var map = ((TmphValue)memberMap)._map;
                    if (map == 0) _map = 0;
                    else _map |= map;
                }
            }
        }

        /// <summary>
        ///     指针成员位图
        /// </summary>
        internal sealed class TmphPoint : TmphMemberMap
        {
            /// <summary>
            ///     成员位图
            /// </summary>
            private byte* _map;

            /// <summary>
            ///     成员位图
            /// </summary>
            /// <param name="type">成员位图类型信息</param>
            public TmphPoint(TmphType type) : base(type)
            {
            }

            /// <summary>
            ///     是否默认全部成员有效
            /// </summary>
            public override bool IsDefault
            {
                get { return _map == null; }
            }

            /// <summary>
            ///     比较是否相等
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public override bool Equals(TmphMemberMap other)
            {
                var value = other as TmphPoint;
                if (value != null)
                {
                    if (_map == null) return value._map == null;
                    if (value._map != null)
                    {
                        byte* write = _map, end = _map + Type.MemberMapSize, read = value._map;
                        var bits = *(ulong*)write ^ *(ulong*)read;
                        while ((write += sizeof(ulong)) != end)
                            bits |= *(ulong*)write ^ *(ulong*)(read += sizeof(ulong));
                        return bits == 0;
                    }
                }
                return false;
            }

            /// <summary>
            ///     添加所有成员
            /// </summary>
            internal override void Full()
            {
                if (_map == null) _map = Type.Pool.Get();
                byte* write = _map, end = _map + Type.MemberMapSize;
                do
                {
                    *(ulong*)write = ulong.MaxValue;
                } while ((write += sizeof(ulong)) != end);
            }

            /// <summary>
            ///     清空所有成员
            /// </summary>
            internal override void Empty()
            {
                if (_map == null) _map = Type.Pool.GetClear();
                else
                {
                    byte* write = _map, end = _map + Type.MemberMapSize;
                    do
                    {
                        *(ulong*)write = 0;
                    } while ((write += sizeof(ulong)) != end);
                }
            }

            /// <summary>
            ///     序列化
            /// </summary>
            /// <param name="stream"></param>
            internal override void FieldSerialize(TmphUnmanagedStream stream)
            {
                if (_map == null) stream.Write(0);
                else
                {
                    stream.PrepLength(Type.FieldSerializeSize + sizeof(int));
                    byte* data = stream.CurrentData, read = _map;
                    *(int*)data = Type.FieldCount;
                    data += sizeof(int);
                    for (var end = _map + (Type.FieldSerializeSize & (int.MaxValue - sizeof(ulong) + 1));
                        read != end;
                        read += sizeof(ulong), data += sizeof(ulong))
                        *(ulong*)data = *(ulong*)read;
                    if ((Type.FieldSerializeSize & sizeof(int)) != 0) *(uint*)data = *(uint*)read;
                    stream.Unsafer.AddLength(Type.FieldSerializeSize + sizeof(int));
                    stream.PrepLength();
                }
            }

            /// <summary>
            ///     字段成员反序列化
            /// </summary>
            /// <param name="deSerializer"></param>
            internal override void FieldDeSerialize(TmphBinaryDeSerializer deSerializer)
            {
                if (_map == null) _map = Type.Pool.Get();
                deSerializer.DeSerializeMemberMap(_map, Type.FieldCount, Type.FieldSerializeSize);
            }

            /// <summary>
            ///     设置成员索引,忽略默认成员
            /// </summary>
            /// <param name="memberIndex">成员索引</param>
            internal override void SetMember(int memberIndex)
            {
                if (_map == null) _map = Type.Pool.GetClear();
                _map[memberIndex >> 3] |= (byte)(1 << (memberIndex & 7));
            }

            /// <summary>
            ///     清除所有成员
            /// </summary>
            internal override void Clear()
            {
                Dispose();
            }

            /// <summary>
            ///     清除成员索引,忽略默认成员
            /// </summary>
            /// <param name="memberIndex">成员索引</param>
            internal override void ClearMember(int memberIndex)
            {
                if (_map != null) _map[memberIndex >> 3] &= (byte)(byte.MaxValue ^ (1 << (memberIndex & 7)));
            }

            /// <summary>
            ///     判断成员索引是否有效
            /// </summary>
            /// <param name="memberIndex">成员索引</param>
            /// <returns>成员索引是否有效</returns>
            internal override bool IsMember(int memberIndex)
            {
                return _map == null || (_map[memberIndex >> 3] & (1 << (memberIndex & 7))) != 0;
            }

            /// <summary>
            ///     成员位图
            /// </summary>
            /// <returns>成员位图</returns>
            public override TmphMemberMap Copy()
            {
                var value = new TmphPoint(Type);
                if (_map != null)
                    Unsafe.TmphMemory.Copy(_map, value._map = Type.Pool.Get(), Type.MemberMapSize);
                return value;
            }

            /// <summary>
            ///     成员交集运算
            /// </summary>
            /// <param name="memberMap">成员位图</param>
            public override void And(TmphMemberMap memberMap)
            {
                if (memberMap == null || memberMap.IsDefault) return;
                if (Type != memberMap.Type) TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
                if (_map == null)
                    Unsafe.TmphMemory.Copy(((TmphPoint)memberMap)._map, _map = Type.Pool.Get(),
                        Type.MemberMapSize);
                else
                {
                    byte* write = _map, end = _map + Type.MemberMapSize, read = ((TmphPoint)memberMap)._map;
                    *(ulong*)write &= *(ulong*)read;
                    while ((write += sizeof(ulong)) != end) *(ulong*)write &= *(ulong*)(read += sizeof(ulong));
                }
            }

            /// <summary>
            ///     成员异或运算,忽略默认成员
            /// </summary>
            /// <param name="memberMap">成员位图</param>
            public override void Xor(TmphMemberMap memberMap)
            {
                if (memberMap == null || memberMap.IsDefault || _map == null) return;
                if (Type != memberMap.Type) TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
                byte* write = _map, end = _map + Type.MemberMapSize, read = ((TmphPoint)memberMap)._map;
                *(ulong*)write ^= *(ulong*)read;
                while ((write += sizeof(ulong)) != end) *(ulong*)write ^= *(ulong*)(read += sizeof(ulong));
            }

            /// <summary>
            ///     成员并集运算,忽略默认成员
            /// </summary>
            /// <param name="memberMap">成员位图</param>
            public override void Or(TmphMemberMap memberMap)
            {
                if (_map == null) return;
                if (memberMap == null || memberMap.IsDefault)
                {
                    Type.Pool.Push(_map);
                    _map = null;
                    return;
                }
                if (Type != memberMap.Type) TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
                byte* write = _map, end = _map + Type.MemberMapSize, read = ((TmphPoint)memberMap)._map;
                *(ulong*)write |= *(ulong*)read;
                while ((write += sizeof(ulong)) != end) *(ulong*)write |= *(ulong*)(read += sizeof(ulong));
            }

            /// <summary>
            ///     释放资源
            /// </summary>
            public override void Dispose()
            {
                if (_map != null)
                {
                    Type.Pool.Push(_map);
                    _map = null;
                }
            }

            /// <summary>
            ///     成员位图内存池
            /// </summary>
            internal sealed class THPool
            {
                /// <summary>
                ///     成员位图内存池集合
                /// </summary>
                private static readonly THPool[] Pools;

                /// <summary>
                ///     成员位图内存池集合访问锁
                /// </summary>
                private static int _poolLock;

                /// <summary>
                ///     内存申请数量
                /// </summary>
                private static int _memoryCount;

                /// <summary>
                ///     成员位图内存池字节大小
                /// </summary>
                private static readonly int MemorySize = pub.Default.MemberMapPoolSize;

                /// <summary>
                ///     成员位图内存池起始位置
                /// </summary>
                private static byte* _memoryStart;

                /// <summary>
                ///     成员位图内存池结束位置
                /// </summary>
                private static byte* _memoryEnd;

                /// <summary>
                ///     成员位图内存池访问锁
                /// </summary>
                private static int _memoryLock;

                /// <summary>
                ///     成员位图内存池访问锁
                /// </summary>
                private static readonly object CreateLock = new object();

                /// <summary>
                ///     成员位图字节数量
                /// </summary>
                private readonly int _size;

                /// <summary>
                ///     空闲内存地址
                /// </summary>
                private byte* _free;

                /// <summary>
                ///     空闲内存地址访问锁
                /// </summary>
                private int _freeLock;

                static THPool()
                {
                    var count = pub.Default.MaxMemberMapCount;
                    if ((count >> 3) >= pub.Default.MemberMapPoolSize)
                        TmphLog.Error.Add("成员位图支持数量过大 " + count.toString());
                    Pools = new THPool[count >> 6];
                }

                /// <summary>
                ///     成员位图内存池
                /// </summary>
                /// <param name="size">成员位图字节数量</param>
                private THPool(int size)
                {
                    _size = size;
                }

                /// <summary>
                ///     获取成员位图
                /// </summary>
                /// <returns>成员位图</returns>
                public byte* Get()
                {
                    byte* value;
                    TmphInterlocked.NoCheckCompareSetSleep0(ref _freeLock);
                    if (_free != null)
                    {
                        value = _free;
                        //free = (byte*)*(ulong*)free;
                        _free = *(byte**)_free;
                        _freeLock = 0;
                        return value;
                    }
                    _freeLock = 0;
                    TmphInterlocked.NoCheckCompareSetSleep0(ref _memoryLock);
                    var size = (int)(_memoryEnd - _memoryStart);
                    if (size >= _size)
                    {
                        value = _memoryStart;
                        _memoryStart += _size;
                        _memoryLock = 0;
                        return value;
                    }
                    _memoryLock = 0;
                    Monitor.Enter(CreateLock);
                    TmphInterlocked.NoCheckCompareSetSleep0(ref _memoryLock);
                    if (((int)(_memoryEnd - _memoryStart)) >= _size)
                    {
                        value = _memoryStart;
                        _memoryStart += _size;
                        _memoryLock = 0;
                        Monitor.Exit(CreateLock);
                        return value;
                    }
                    _memoryLock = 0;
                    try
                    {
                        var start = TmphUnmanaged.Get(MemorySize, false).Byte;
                        TmphInterlocked.NoCheckCompareSetSleep0(ref _memoryLock);
                        value = _memoryStart = start;
                        _memoryEnd = start + MemorySize;
                        _memoryStart += _size;
                        _memoryLock = 0;
                    }
                    finally
                    {
                        Monitor.Exit(CreateLock);
                    }
                    Interlocked.Increment(ref _memoryCount);
                    return value;
                }

                /// <summary>
                ///     获取成员位图
                /// </summary>
                /// <returns>成员位图</returns>
                public byte* GetClear()
                {
                    byte* value = Get(), write = value + _size;
                    do
                    {
                        *(ulong*)(write -= sizeof(ulong)) = 0;
                    } while (write != value);
                    return value;
                }

                /// <summary>
                ///     成员位图入池
                /// </summary>
                /// <param name="map">成员位图</param>
                public void Push(byte* map)
                {
                    //*(ulong*)map = (ulong)free;
                    *(byte**)map = _free;
                    _free = map;
                }

                /// <summary>
                ///     获取成员位图内存池
                /// </summary>
                /// <param name="size">成员位图字节数量</param>
                /// <returns></returns>
                public static THPool GetPool(int size)
                {
                    var index = size >> 3;
                    if (index < Pools.Length)
                    {
                        var pool = Pools[index];
                        if (pool != null) return pool;
                        TmphInterlocked.NoCheckCompareSetSleep0(ref _poolLock);
                        if ((pool = Pools[index]) == null)
                        {
                            try
                            {
                                Pools[index] = pool = new THPool(size);
                            }
                            finally
                            {
                                _poolLock = 0;
                            }
                            return pool;
                        }
                        _poolLock = 0;
                        return pool;
                    }
                    return null;
                }
            }
        }

        /// <summary>
        ///     成员索引
        /// </summary>
        public sealed class TmphMemberIndex
        {
            /// <summary>
            ///     成员索引
            /// </summary>
            private readonly int _index;

            /// <summary>
            ///     成员索引
            /// </summary>
            /// <param name="index">成员索引</param>
            internal TmphMemberIndex(int index)
            {
                _index = index;
            }

            /// <summary>
            ///     判断成员索引是否有效
            /// </summary>
            /// <param name="memberMap"></param>
            /// <returns>成员索引是否有效</returns>
            public bool IsMember(TmphMemberMap memberMap)
            {
                return memberMap != null && memberMap.IsMember(_index);
            }

            /// <summary>
            ///     清除成员索引,忽略默认成员
            /// </summary>
            /// <param name="memberMap"></param>
            public void ClearMember(TmphMemberMap memberMap)
            {
                if (memberMap != null) memberMap.ClearMember(_index);
            }

            /// <summary>
            ///     设置成员索引,忽略默认成员
            /// </summary>
            /// <param name="memberMap"></param>
            internal void SetMember(TmphMemberMap memberMap)
            {
                if (memberMap != null) memberMap.SetMember(_index);
            }
        }
    }

    /// <summary>
    ///     成员位图
    /// </summary>
    /// <typeparam name="TValueType">对象类型</typeparam>
    public static class TmphMemberMap<TValueType>
    {
        /// <summary>
        ///     成员位图类型信息
        /// </summary>
        internal static readonly TmphMemberMap.TmphType Type =
            new TmphMemberMap.TmphType(TmphMemberIndexGroup<TValueType>.GetAllMembers(),
                TmphMemberIndexGroup<TValueType>.FieldCount);

        /// <summary>
        ///     默认成员位图
        /// </summary>
        internal static readonly TmphMemberMap Default = New();

        /// <summary>
        ///     默认成员位图
        /// </summary>
        /// <returns></returns>
        public static TmphMemberMap New()
        {
            return Type.MemberCount < 64 ? (TmphMemberMap)new TmphMemberMap.TmphValue(Type) : new TmphMemberMap.TmphPoint(Type);
        }

        /// <summary>
        ///     所有成员位图
        /// </summary>
        /// <returns></returns>
        public static TmphMemberMap Full()
        {
            var value = Type.MemberCount < 64
                ? (TmphMemberMap)new TmphMemberMap.TmphValue(Type)
                : new TmphMemberMap.TmphPoint(Type);
            value.Full();
            return value;
        }

        /// <summary>
        ///     空成员位图
        /// </summary>
        /// <returns></returns>
        public static TmphMemberMap Empty()
        {
            var value = Type.MemberCount < 64
                ? (TmphMemberMap)new TmphMemberMap.TmphValue(Type)
                : new TmphMemberMap.TmphPoint(Type);
            value.Empty();
            return value;
        }

        /// <summary>
        ///     创建成员索引
        /// </summary>
        /// <typeparam name="TReturnType"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public static TmphMemberMap.TmphMemberIndex CreateMemberIndex<TReturnType>(
            Expression<Func<TValueType, TReturnType>> member)
        {
            var index = Type.GetMemberIndex(TmphMemberMap.GetFieldName(member));
            return index >= 0 ? new TmphMemberMap.TmphMemberIndex(index) : null;
        }

        /// <summary>
        ///     创建成员位图
        /// </summary>
        public struct TmphBuilder
        {
            /// <summary>
            ///     成员位图
            /// </summary>
            private readonly TmphMemberMap _memberMap;

            /// <summary>
            ///     创建成员位图
            /// </summary>
            /// <param name="isNew">是否创建成员</param>
            internal TmphBuilder(bool isNew)
            {
                _memberMap = isNew ? New() : null;
            }

            /// <summary>
            ///     成员位图
            /// </summary>
            /// <param name="value">创建成员位图</param>
            /// <returns>成员位图</returns>
            public static implicit operator TmphMemberMap(TmphBuilder value)
            {
                return value._memberMap;
            }

            /// <summary>
            ///     添加成员
            /// </summary>
            /// <typeparam name="TReturnType"></typeparam>
            /// <param name="member"></param>
            /// <returns></returns>
            public TmphBuilder Append<TReturnType>(Expression<Func<TValueType, TReturnType>> member)
            {
                if (_memberMap != null) _memberMap.SetMember(member);
                return this;
            }
        }
    }
}