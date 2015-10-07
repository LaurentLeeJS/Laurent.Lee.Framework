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

using Laurent.Lee.CLB.Code;
using Laurent.Lee.CLB.Code.CSharp;
using Laurent.Lee.CLB.Config;
using Laurent.Lee.CLB.MemoryDataBase;
using Laurent.Lee.CLB.MemoryDataBase.Cache;
using Laurent.Lee.CLB.Reflection;
using Laurent.Lee.CLB.TcpClient;
using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using TmphAjax = Laurent.Lee.CLB.Web.TmphAjax;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    ///     内存数据库表格配置
    /// </summary>
    public class TmphMemoryDatabaseTable
    {
        /// <summary>
        ///     序列化类型
        /// </summary>
        public enum TmphSerializeType : byte
        {
            /// <summary>
            ///     二进制索引序列化
            /// </summary>
            Index,

            /// <summary>
            ///     JSON序列化
            /// </summary>
            Json,

            /// <summary>
            ///     二进制数据序列化
            /// </summary>
            Data
        }

        /// <summary>
        ///     内存数据库表格操作工具 字段名称
        /// </summary>
        public static readonly string MemoryDatabaseTableName = "MdbTable";

        /// <summary>
        ///     数据序列化
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        internal abstract unsafe class TmphSerializer
        {
            /// <summary>
            ///     序列化类型
            /// </summary>
            internal abstract TmphSerializeType Type { get; }

            /// <summary>
            ///     加载数据成员位图
            /// </summary>
            internal abstract TmphMemberMap LoadMemberMap { get; }

            /// <summary>
            ///     是否存在成员集合
            /// </summary>
            internal abstract bool IsDataMember { get; }

            /// <summary>
            ///     判断当前成员集合是否匹配
            /// </summary>
            internal abstract bool IsCurrentDataMembers { get; }

            /// <summary>
            ///     序列化成员集合
            /// </summary>
            /// <param name="stream"></param>
            internal abstract void Members(TmphUnmanagedStream stream);

            /// <summary>
            ///     准备加载数据
            /// </summary>
            internal abstract void ReadyLoad();

            /// <summary>
            ///     加载数据结束
            /// </summary>
            internal abstract void Loaded();

            /// <summary>
            ///     反序列化成员集合
            /// </summary>
            /// <param name="data"></param>
            /// <param name="size"></param>
            /// <returns></returns>
            internal abstract bool LoadMembers(byte* data, int size);

            /// <summary>
            ///     获取成员集合
            /// </summary>
            /// <param name="members">成员集合</param>
            /// <param name="serializeType">序列化类型</param>
            /// <returns></returns>
            protected static FDataMember[] getMembers(TmphSubArray<TmphMemberIndex> members, Type serializeType)
            {
                var dataMembers = new FDataMember[members.Count];
                var index = 0;
                foreach (var member in members.Sort((left, right) => left.Member.Name.CompareTo(right.Member.Name)))
                    dataMembers[index++] = new FDataMember(member, serializeType);
                return dataMembers;
            }

            /// <summary>
            ///     数据库成员信息
            /// </summary>
            protected struct FDataMember
            {
                /// <summary>
                ///     类型序号集合
                /// </summary>
                private static readonly Dictionary<Type, int> typeIndexs;

                /// <summary>
                ///     成员序号
                /// </summary>
                public int MemberIndex;

                /// <summary>
                ///     成员集合
                /// </summary>
                public FDataMember[] Members;

                /// <summary>
                ///     成员名称
                /// </summary>
                public string Name;

                /// <summary>
                ///     类型序号
                /// </summary>
                public int TypeIndex;

                /// <summary>
                ///     类型名称
                /// </summary>
                public string TypeName;

                ///// <summary>
                ///// 获取类型序号
                ///// </summary>
                ///// <param name="type">类型</param>
                ///// <returns>类型序号</returns>
                //public static int GetMemberTypeIndex(Type type)
                //{
                //    int index;
                //    return typeIndexs.TryGetValue(type, out index) ? index : 0;
                //}
                static FDataMember()
                {
                    typeIndexs = TmphDictionary.CreateOnly<Type, int>();
                    var index = 0;
                    typeIndexs.Add(typeof(bool), ++index);
                    typeIndexs.Add(typeof(bool?), ++index);
                    typeIndexs.Add(typeof(byte), ++index);
                    typeIndexs.Add(typeof(byte?), ++index);
                    typeIndexs.Add(typeof(sbyte), ++index);
                    typeIndexs.Add(typeof(sbyte?), ++index);
                    typeIndexs.Add(typeof(short), ++index);
                    typeIndexs.Add(typeof(short?), ++index);
                    typeIndexs.Add(typeof(ushort), ++index);
                    typeIndexs.Add(typeof(ushort?), ++index);
                    typeIndexs.Add(typeof(int), ++index);
                    typeIndexs.Add(typeof(int?), ++index);
                    typeIndexs.Add(typeof(uint), ++index);
                    typeIndexs.Add(typeof(uint?), ++index);
                    typeIndexs.Add(typeof(long), ++index);
                    typeIndexs.Add(typeof(long?), ++index);
                    typeIndexs.Add(typeof(ulong), ++index);
                    typeIndexs.Add(typeof(ulong?), ++index);
                    typeIndexs.Add(typeof(DateTime), ++index);
                    typeIndexs.Add(typeof(DateTime?), ++index);
                    typeIndexs.Add(typeof(float), ++index);
                    typeIndexs.Add(typeof(float?), ++index);
                    typeIndexs.Add(typeof(double), ++index);
                    typeIndexs.Add(typeof(double?), ++index);
                    typeIndexs.Add(typeof(decimal), ++index);
                    typeIndexs.Add(typeof(decimal?), ++index);
                    typeIndexs.Add(typeof(Guid), ++index);
                    typeIndexs.Add(typeof(Guid?), ++index);
                    typeIndexs.Add(typeof(char), ++index);
                    typeIndexs.Add(typeof(char?), ++index);
                    typeIndexs.Add(typeof(string), ++index);
                    typeIndexs.Add(typeof(bool[]), ++index);
                    typeIndexs.Add(typeof(bool?[]), ++index);
                    typeIndexs.Add(typeof(byte[]), ++index);
                    typeIndexs.Add(typeof(byte?[]), ++index);
                    typeIndexs.Add(typeof(sbyte[]), ++index);
                    typeIndexs.Add(typeof(sbyte?[]), ++index);
                    typeIndexs.Add(typeof(short[]), ++index);
                    typeIndexs.Add(typeof(short?[]), ++index);
                    typeIndexs.Add(typeof(ushort[]), ++index);
                    typeIndexs.Add(typeof(ushort?[]), ++index);
                    typeIndexs.Add(typeof(int[]), ++index);
                    typeIndexs.Add(typeof(int?[]), ++index);
                    typeIndexs.Add(typeof(uint[]), ++index);
                    typeIndexs.Add(typeof(uint?[]), ++index);
                    typeIndexs.Add(typeof(long[]), ++index);
                    typeIndexs.Add(typeof(long?[]), ++index);
                    typeIndexs.Add(typeof(ulong[]), ++index);
                    typeIndexs.Add(typeof(ulong?[]), ++index);
                    typeIndexs.Add(typeof(DateTime[]), ++index);
                    typeIndexs.Add(typeof(DateTime?[]), ++index);
                    typeIndexs.Add(typeof(float[]), ++index);
                    typeIndexs.Add(typeof(float?[]), ++index);
                    typeIndexs.Add(typeof(double[]), ++index);
                    typeIndexs.Add(typeof(double?[]), ++index);
                    typeIndexs.Add(typeof(decimal[]), ++index);
                    typeIndexs.Add(typeof(decimal?[]), ++index);
                    typeIndexs.Add(typeof(Guid[]), ++index);
                    typeIndexs.Add(typeof(Guid?[]), ++index);
                    typeIndexs.Add(typeof(char[]), ++index);
                    typeIndexs.Add(typeof(char?[]), ++index);
                    typeIndexs.Add(typeof(string[]), ++index);
                }

                /// <summary>
                ///     数据库成员信息
                /// </summary>
                /// <param name="member">成员信息</param>
                /// <param name="serializeType">序列化类型</param>
                public FDataMember(TmphMemberIndex member, Type serializeType)
                {
                    Name = member.Member.Name;
                    MemberIndex = member.MemberIndex;
                    if (typeIndexs.TryGetValue(member.Type, out TypeIndex))
                    {
                        TypeName = null;
                        Members = TmphNullValue<FDataMember>.Array;
                    }
                    else
                    {
                        TypeName = member.Type.fullName();
                        Members =
                            (FDataMember[])
                                serializeType.MakeGenericType(member.Type)
                                    .GetField("dataMembers",
                                        BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                                    .GetValue(null);
                    }
                }

                /// <summary>
                ///     判断是否相等
                /// </summary>
                /// <param name="other"></param>
                /// <returns></returns>
                private bool equals(FDataMember other, TmphList<FDataMember[]> history)
                {
                    if (((TypeIndex ^ other.TypeIndex) | (MemberIndex ^ other.MemberIndex)) == 0 && Name == other.Name &&
                        TypeName == other.TypeName)
                    {
                        return Members == null
                            ? other.Members == null
                            : other.Members != null && Equals(Members, other.Members, history);
                    }
                    return false;
                }

                /// <summary>
                ///     判断是否相等
                /// </summary>
                /// <param name="left"></param>
                /// <param name="right"></param>
                /// <param name="history"></param>
                /// <returns></returns>
                public static bool Equals(FDataMember[] left, FDataMember[] right, TmphList<FDataMember[]> history)
                {
                    if (history.IndexOf(left) != -1) return true;
                    history.Add(left);
                    if (left.Length == right.Length)
                    {
                        var index = 0;
                        foreach (var member in left)
                        {
                            if (!member.Equals(right[index++])) return false;
                        }
                        return true;
                    }
                    return false;
                }
            }
        }

        /// <summary>
        ///     数据序列化
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        internal abstract unsafe class TmphSerializer<TValueType> : TmphSerializer, IDisposable
        {
            /// <summary>
            ///     释放资源
            /// </summary>
            public virtual void Dispose()
            {
                Loaded();
            }

            /// <summary>
            ///     反序列化添加数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            internal abstract bool LoadInsert(TValueType value, byte* data, int size);

            /// <summary>
            ///     反序列化更新数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="data"></param>
            /// <param name="memberMap"></param>
            /// <returns></returns>
            internal abstract bool LoadUpdate(TValueType value, byte* data, int size);

            /// <summary>
            ///     反序列化删除数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            internal abstract bool LoadDelete<TKeyType>(ref TKeyType value, byte* data, int size);

            /// <summary>
            ///     添加数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="stream"></param>
            internal abstract void Insert(TValueType value, TmphUnmanagedStream stream);

            /// <summary>
            ///     更新数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="stream"></param>
            /// <param name="memberMap"></param>
            internal abstract void Update(TValueType value, TmphUnmanagedStream stream, TmphMemberMap memberMap);

            /// <summary>
            ///     删除数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="stream"></param>
            internal abstract void Delete<TKeyType>(TKeyType value, TmphUnmanagedStream stream);
        }

        /// <summary>
        ///     二进制数据序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        private abstract class TmphBinarySerializer<TValueType> : TmphSerializer<TValueType>
        {
            /// <summary>
            ///     当前成员集合
            /// </summary>
            protected FDataMember[] currentDataMembers;

            /// <summary>
            ///     Json解析配置参数
            /// </summary>
            protected TmphBinaryDeSerializer.TmphConfig deSerializerConfig;

            /// <summary>
            ///     是否存在成员集合
            /// </summary>
            internal override bool IsDataMember
            {
                get { return true; }
            }

            /// <summary>
            ///     加载数据成员位图
            /// </summary>
            internal override TmphMemberMap LoadMemberMap
            {
                get { return deSerializerConfig.MemberMap; }
            }

            /// <summary>
            ///     反序列化成员集合
            /// </summary>
            /// <param name="data"></param>
            /// <param name="size"></param>
            /// <returns></returns>
            internal override unsafe bool LoadMembers(byte* data, int size)
            {
                if (TmphDataDeSerializer.DeSerialize(data, size, ref currentDataMembers))
                {
                    return true;
                }
                return false;
            }

            /// <summary>
            ///     准备加载数据
            /// </summary>
            internal override void ReadyLoad()
            {
                deSerializerConfig = new TmphBinaryDeSerializer.TmphConfig
                {
                    IsLogError = false,
                    MemberMap = TmphMemberMap<TValueType>.New()
                };
            }

            /// <summary>
            ///     加载数据结束
            /// </summary>
            internal override void Loaded()
            {
                if (deSerializerConfig != null)
                {
                    deSerializerConfig.MemberMap.Dispose();
                    deSerializerConfig = null;
                }
            }

            /// <summary>
            ///     获取成员集合
            /// </summary>
            /// <param name="members">成员集合</param>
            /// <param name="memberMap"></param>
            protected static TmphSubArray<TmphMemberIndex> getMembers(TmphSubArray<TmphMemberIndex> members, TmphMemberMap memberMap)
            {
                if (members.Count != 0)
                {
                    var dataMembers = TmphSubArray<TmphMemberIndex>.Unsafe(members.array, 0, 0);
                    var modelMemberMap = TmphMemoryDatabaseModel<TValueType>.MemberMap;
                    var isAllMember = TmphMemoryDatabaseModel<TValueType>.IsAllMember;
                    foreach (var member in members)
                    {
                        if (modelMemberMap.IsMember(member.MemberIndex))
                        {
                            memberMap.SetMember(member.MemberIndex);
                            dataMembers.Add(member);
                        }
                        else isAllMember = 0;
                    }
                    if (isAllMember == 0) return dataMembers;
                    memberMap.Clear();
                }
                return members;
            }
        }

        /// <summary>
        ///     二进制索引序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        private sealed unsafe class TmphIndexSerializer<TValueType> : TmphBinarySerializer<TValueType>
        {
            /// <summary>
            ///     成员位图
            /// </summary>
            private static readonly TmphMemberMap memberMap = TmphMemberMap<TValueType>.Empty();

            /// <summary>
            ///     成员集合
            /// </summary>
            private static readonly FDataMember[] dataMembers;

            /// <summary>
            ///     序列化参数
            /// </summary>
            private readonly TmphBinarySerializer.TmphConfig serializeConfig = new TmphBinarySerializer.TmphConfig
            {
                IsMemberMapErrorLog = false
            };

            static TmphIndexSerializer()
            {
                dataMembers =
                    getMembers(getMembers(TmphIndexSerializer.TmphTypeSerializer<TValueType>.GetMembers(), memberMap),
                        typeof(TmphIndexSerializer<>));
            }

            /// <summary>
            ///     序列化类型
            /// </summary>
            internal override TmphSerializeType Type
            {
                get { return TmphSerializeType.Index; }
            }

            /// <summary>
            ///     判断当前成员集合是否匹配
            /// </summary>
            internal override bool IsCurrentDataMembers
            {
                get { return FDataMember.Equals(dataMembers, currentDataMembers, new TmphList<FDataMember[]>()); }
            }

            /// <summary>
            ///     反序列化添加数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            internal override bool LoadInsert(TValueType value, byte* data, int size)
            {
                return TmphIndexDeSerializer.DeSerialize(data, size, ref value, deSerializerConfig);
            }

            /// <summary>
            ///     反序列化更新数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            internal override bool LoadUpdate(TValueType value, byte* data, int size)
            {
                return TmphIndexDeSerializer.DeSerialize(data, size, ref value, deSerializerConfig);
            }

            /// <summary>
            ///     反序列化删除数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            internal override bool LoadDelete<TKeyType>(ref TKeyType value, byte* data, int size)
            {
                return TmphIndexDeSerializer.DeSerialize(data, size, ref value);
            }

            /// <summary>
            ///     序列化成员集合
            /// </summary>
            /// <param name="stream"></param>
            internal override void Members(TmphUnmanagedStream stream)
            {
                TmphDataSerializer.Serialize(dataMembers, stream);
            }

            /// <summary>
            ///     添加数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="stream"></param>
            internal override void Insert(TValueType value, TmphUnmanagedStream stream)
            {
                serializeConfig.MemberMap = memberMap.IsDefault ? null : memberMap;
                TmphIndexSerializer.Serialize(value, stream, serializeConfig);
            }

            /// <summary>
            ///     更新数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="stream"></param>
            /// <param name="memberMap"></param>
            internal override void Update(TValueType value, TmphUnmanagedStream stream, TmphMemberMap memberMap)
            {
                serializeConfig.MemberMap = memberMap;
                TmphIndexSerializer.Serialize(value, stream, serializeConfig);
            }

            /// <summary>
            ///     删除数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="stream"></param>
            internal override void Delete<TKeyType>(TKeyType value, TmphUnmanagedStream stream)
            {
                serializeConfig.MemberMap = null;
                TmphIndexSerializer.Serialize(value, stream, serializeConfig);
            }
        }

        /// <summary>
        ///     二进制数据序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        private sealed unsafe class TmphDataSerializer<TValueType> : TmphBinarySerializer<TValueType>
        {
            /// <summary>
            ///     成员位图
            /// </summary>
            private static readonly TmphMemberMap memberMap = TmphMemberMap<TValueType>.Empty();

            /// <summary>
            ///     成员集合
            /// </summary>
            private static readonly FDataMember[] dataMembers;

            /// <summary>
            ///     序列化参数
            /// </summary>
            private readonly TmphDataSerializer.TmphConfig serializeConfig = new TmphDataSerializer.TmphConfig
            {
                IsMemberMapErrorLog = false
            };

            static TmphDataSerializer()
            {
                dataMembers =
                    getMembers(getMembers(TmphDataSerializer.TmphTypeSerializer<TValueType>.GetMembers(), memberMap),
                        typeof(TmphDataSerializer<>));
            }

            /// <summary>
            ///     序列化类型
            /// </summary>
            internal override TmphSerializeType Type
            {
                get { return TmphSerializeType.Data; }
            }

            /// <summary>
            ///     判断当前成员集合是否匹配
            /// </summary>
            internal override bool IsCurrentDataMembers
            {
                get { return FDataMember.Equals(dataMembers, currentDataMembers, new TmphList<FDataMember[]>()); }
            }

            /// <summary>
            ///     反序列化添加数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            internal override bool LoadInsert(TValueType value, byte* data, int size)
            {
                return TmphDataDeSerializer.DeSerialize(data, size, ref value, deSerializerConfig);
            }

            /// <summary>
            ///     反序列化更新数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            internal override bool LoadUpdate(TValueType value, byte* data, int size)
            {
                return TmphDataDeSerializer.DeSerialize(data, size, ref value, deSerializerConfig);
            }

            /// <summary>
            ///     反序列化删除数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            internal override bool LoadDelete<TKeyType>(ref TKeyType value, byte* data, int size)
            {
                return TmphDataDeSerializer.DeSerialize(data, size, ref value);
            }

            /// <summary>
            ///     序列化成员集合
            /// </summary>
            /// <param name="stream"></param>
            internal override void Members(TmphUnmanagedStream stream)
            {
                TmphDataSerializer.Serialize(dataMembers, stream);
            }

            /// <summary>
            ///     添加数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="stream"></param>
            internal override void Insert(TValueType value, TmphUnmanagedStream stream)
            {
                serializeConfig.MemberMap = memberMap.IsDefault ? null : memberMap;
                TmphDataSerializer.Serialize(value, stream, serializeConfig);
            }

            /// <summary>
            ///     更新数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="stream"></param>
            /// <param name="memberMap"></param>
            internal override void Update(TValueType value, TmphUnmanagedStream stream, TmphMemberMap memberMap)
            {
                serializeConfig.MemberMap = memberMap;
                TmphDataSerializer.Serialize(value, stream, serializeConfig);
            }

            /// <summary>
            ///     删除数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="stream"></param>
            internal override void Delete<TKeyType>(TKeyType value, TmphUnmanagedStream stream)
            {
                serializeConfig.MemberMap = null;
                TmphDataSerializer.Serialize(value, stream, serializeConfig);
            }
        }

        /// <summary>
        ///     JSON数据序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        private sealed unsafe class TmphJsonSerializer<TValueType> : TmphSerializer<TValueType>
        {
            /// <summary>
            ///     成员位图
            /// </summary>
            private static readonly TmphMemberMap memberMap = TmphMemberMap<TValueType>.Empty();

            /// <summary>
            ///     JSON序列化流
            /// </summary>
            private readonly TmphCharStream jsonStream = new TmphCharStream(1 << 10);

            /// <summary>
            ///     序列化参数
            /// </summary>
            private readonly TmphJsonSerializer.TmphConfig serializeConfig = new TmphJsonSerializer.TmphConfig
            {
                IsMemberMapErrorLog = false,
                CheckLoopDepth = TmphAppSetting.JsonDepth
            };

            /// <summary>
            ///     Json解析配置参数
            /// </summary>
            private TmphJsonParser.TmphConfig parseConfig;

            static TmphJsonSerializer()
            {
                var members = TmphJsonParser.TmphTypeParser<TValueType>.GetMembers();
                if (members.Count != 0)
                {
                    byte* map = stackalloc byte[memberMap.Type.MemberMapSize];
                    var modelMemberMap = TmphMemoryDatabaseModel<TValueType>.MemberMap;
                    Unsafe.TmphMemory.Fill(map, 0UL, memberMap.Type.MemberMapSize >> 3);
                    foreach (var member in members)
                        map[member.MemberIndex >> 3] |= (byte)(1 << (member.MemberIndex & 7));
                    members = TmphJsonParser.TmphTypeParser<TValueType>.GetMembers();
                    foreach (var member in members)
                    {
                        if ((map[member.MemberIndex >> 3] & (1 << (member.MemberIndex & 7))) != 0)
                        {
                            if (member.IsField)
                            {
                                if (modelMemberMap.IsMember(member.MemberIndex))
                                    memberMap.SetMember(member.MemberIndex);
                            }
                            else
                            {
                                var attribute = member.GetAttribute<TmphDataMember>(true, true);
                                if (attribute == null || attribute.IsSetup) memberMap.SetMember(member.MemberIndex);
                            }
                        }
                    }
                }
            }

            /// <summary>
            ///     序列化类型
            /// </summary>
            internal override TmphSerializeType Type
            {
                get { return TmphSerializeType.Json; }
            }

            /// <summary>
            ///     是否存在成员集合
            /// </summary>
            internal override bool IsDataMember
            {
                get { return false; }
            }

            /// <summary>
            ///     判断当前成员集合是否匹配
            /// </summary>
            internal override bool IsCurrentDataMembers
            {
                get { return true; }
            }

            /// <summary>
            ///     加载数据成员位图
            /// </summary>
            internal override TmphMemberMap LoadMemberMap
            {
                get { return parseConfig.MemberMap; }
            }

            /// <summary>
            ///     反序列化成员集合
            /// </summary>
            /// <param name="data"></param>
            /// <param name="size"></param>
            /// <returns></returns>
            internal override bool LoadMembers(byte* data, int size)
            {
                TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
                return false;
            }

            /// <summary>
            ///     释放资源
            /// </summary>
            public override void Dispose()
            {
                base.Dispose();
                jsonStream.Dispose();
            }

            /// <summary>
            ///     准备加载数据
            /// </summary>
            internal override void ReadyLoad()
            {
                parseConfig = new TmphJsonParser.TmphConfig
                {
                    IsGetJson = false,
                    IsEndSpace = false,
                    MemberMap = TmphMemberMap<TValueType>.New()
                };
            }

            /// <summary>
            ///     加载数据结束
            /// </summary>
            internal override void Loaded()
            {
                if (parseConfig != null)
                {
                    parseConfig.MemberMap.Dispose();
                    parseConfig = null;
                }
            }

            /// <summary>
            ///     检测数据结束位置
            /// </summary>
            /// <param name="data"></param>
            /// <param name="size"></param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private void checkSize(byte* data, ref int size)
            {
                if (*(ushort*)(data + size - sizeof(ushort)) == 32) size -= sizeof(ushort);
            }

            /// <summary>
            ///     反序列化添加数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            internal override bool LoadInsert(TValueType value, byte* data, int size)
            {
                checkSize(data, ref size);
                return TmphJsonParser.Parse((char*)data, size >> 1, ref value);
            }

            /// <summary>
            ///     反序列化更新数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            internal override bool LoadUpdate(TValueType value, byte* data, int size)
            {
                checkSize(data, ref size);
                return TmphJsonParser.Parse((char*)data, size >> 1, ref value, parseConfig);
            }

            /// <summary>
            ///     反序列化删除数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            internal override bool LoadDelete<TKeyType>(ref TKeyType value, byte* data, int size)
            {
                checkSize(data, ref size);
                return TmphJsonParser.Parse((char*)data, size >> 1, ref value);
            }

            /// <summary>
            ///     序列化成员集合
            /// </summary>
            /// <param name="stream"></param>
            internal override void Members(TmphUnmanagedStream stream)
            {
                TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
            }

            /// <summary>
            ///     添加数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="stream"></param>
            internal override void Insert(TValueType value, TmphUnmanagedStream stream)
            {
                jsonStream.Clear();
                serializeConfig.MemberMap = memberMap;
                TmphJsonSerializer.ToJson(value, jsonStream, serializeConfig);
                TmphAjax.FormatJavascript(jsonStream, stream);
                if ((stream.Length & 2) != 0) stream.Write(' ');
            }

            /// <summary>
            ///     更新数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="stream"></param>
            /// <param name="memberMap"></param>
            internal override void Update(TValueType value, TmphUnmanagedStream stream, TmphMemberMap memberMap)
            {
                jsonStream.Clear();
                serializeConfig.MemberMap = memberMap;
                TmphJsonSerializer.ToJson(value, jsonStream, serializeConfig);
                TmphAjax.FormatJavascript(jsonStream, stream);
                if ((stream.Length & 2) != 0) stream.Write(' ');
            }

            /// <summary>
            ///     删除数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="stream"></param>
            internal override void Delete<TKeyType>(TKeyType value, TmphUnmanagedStream stream)
            {
                jsonStream.Clear();
                serializeConfig.MemberMap = null;
                TmphJsonSerializer.ToJson(value, jsonStream, serializeConfig);
                TmphAjax.FormatJavascript(jsonStream, stream);
                if ((stream.Length & 2) != 0) stream.Write(' ');
            }
        }

        /// <summary>
        ///     日志类型
        /// </summary>
        internal enum TmphLogType : byte
        {
            /// <summary>
            ///     未知
            /// </summary>
            Unknown,

            /// <summary>
            ///     添加对象
            /// </summary>
            Insert,

            /// <summary>
            ///     修改对象
            /// </summary>
            Update,

            /// <summary>
            ///     删除对象
            /// </summary>
            Delete,

            /// <summary>
            ///     成员变换
            /// </summary>
            MemberData
        }

        /// <summary>
        ///     内存数据库表格操作工具
        /// </summary>
        public abstract class TmphTable : IDisposable
        {
            /// <summary>
            ///     数据库日志文件最小刷新尺寸
            /// </summary>
            private readonly long minRefreshSize;

            /// <summary>
            ///     表格名称
            /// </summary>
            protected readonly string name;

            /// <summary>
            ///     成员集合日志字节数
            /// </summary>
            protected int dataMemberSize;

            /// <summary>
            ///     日志数据长度
            /// </summary>
            protected long dataSize;

            /// <summary>
            ///     是否已经释放资源
            /// </summary>
            protected int isDisposed;

            /// <summary>
            ///     序列化输出缓冲区
            /// </summary>
            protected TmphUnmanagedStream stream;

            /// <summary>
            ///     内存数据库表格操作工具
            /// </summary>
            /// <param name="name">表格名称</param>
            /// <param name="minRefreshSize">数据库日志文件最小刷新尺寸(单位:KB)</param>
            protected TmphTable(string name, int minRefreshSize)
            {
                this.name = name;
                this.minRefreshSize =
                    (long)
                        (minRefreshSize < TmphMemoryDatabase.DefaultMinRefreshSize
                            ? TmphMemoryDatabase.Default.MinRefreshSize
                            : minRefreshSize) << 10;
            }

            /// <summary>
            ///     是否已经释放资源
            /// </summary>
            public bool IsDisposed
            {
                get { return isDisposed != 0; }
            }

            /// <summary>
            ///     释放资源
            /// </summary>
            public void Dispose()
            {
                if (Interlocked.Increment(ref isDisposed) == 1) dispose();
            }

            /// <summary>
            ///     获取更新成员位图
            /// </summary>
            /// <param name="memberMap"></param>
            /// <returns></returns>
            protected abstract TmphMemberMap GetUpdateMemberMap(TmphMemberMap memberMap);

            /// <summary>
            ///     释放资源
            /// </summary>
            protected virtual void dispose()
            {
                CLB.TmphPub.Dispose(ref stream);
            }
        }

        /// <summary>
        ///     内存数据库表格操作工具
        /// </summary>
        /// <typeparam name="TModelType">模型类型</typeparam>
        public abstract class TmphTable<TModelType> : TmphTable
            where TModelType : class
        {
            /// <summary>
            ///     数据序列化
            /// </summary>
            internal TmphSerializer<TModelType> Serializer;

            /// <summary>
            ///     内存数据库表格操作工具
            /// </summary>
            /// <param name="serializeType">数据序列化类型</param>
            /// <param name="name">表格名称</param>
            /// <param name="minRefreshSize">数据库日志文件最小刷新尺寸(单位:KB)</param>
            protected TmphTable(TmphSerializeType serializeType, string name, int minRefreshSize)
                : base(name, minRefreshSize)
            {
                switch (serializeType)
                {
                    case TmphSerializeType.Index:
                        Serializer = new TmphIndexSerializer<TModelType>();
                        break;

                    case TmphSerializeType.Data:
                        Serializer = new TmphDataSerializer<TModelType>();
                        break;

                    case TmphSerializeType.Json:
                        Serializer = new TmphJsonSerializer<TModelType>();
                        break;
                }
            }

            /// <summary>
            ///     释放资源
            /// </summary>
            protected override void dispose()
            {
                base.dispose();
                CLB.TmphPub.Dispose(ref Serializer);
            }
        }

        /// <summary>
        ///     内存数据库表格操作工具
        /// </summary>
        /// <typeparam name="TValueType">表格类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        public abstract class TmphTable<TValueType, TModelType> : TmphTable<TModelType>
            where TValueType : class, TModelType
            where TModelType : class
        {
            /// <summary>
            ///     内存数据库表格操作工具
            /// </summary>
            /// <param name="serializeType">数据序列化类型</param>
            /// <param name="name">表格名称</param>
            /// <param name="minRefreshSize">数据库日志文件最小刷新尺寸(单位:KB)</param>
            protected TmphTable(TmphSerializeType serializeType, string name, int minRefreshSize)
                : base(serializeType, name, minRefreshSize)
            {
            }
        }

        /// <summary>
        ///     内存数据库表格操作工具
        /// </summary>
        /// <typeparam name="TValueType">表格类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <typeparam name="TKeyType">关键字类型</typeparam>
        public abstract class TmphTable<TValueType, TModelType, TKeyType> : TmphTable<TValueType, TModelType>
            where TValueType : class, TModelType
            where TModelType : class
            where TKeyType : IEquatable<TKeyType>
        {
            /// <summary>
            ///     获取关键字
            /// </summary>
            public readonly Func<TModelType, TKeyType> GetPrimaryKey;

            /// <summary>
            ///     设置关键字
            /// </summary>
            public readonly Action<TModelType, TKeyType> SetPrimaryKey;

            /// <summary>
            ///     自增数据加载基本缓存接口
            /// </summary>
            protected TmphILoadCache<TValueType, TModelType, TKeyType> cache;

            /// <summary>
            ///     内存数据库表格操作工具
            /// </summary>
            /// <param name="cache">自增数据加载基本缓存接口</param>
            /// <param name="serializeType">数据序列化类型</param>
            /// <param name="name">表格名称</param>
            /// <param name="minRefreshSize">数据库日志文件最小刷新尺寸(单位:KB)</param>
            protected TmphTable(TmphILoadCache<TValueType, TModelType, TKeyType> cache
                , TmphSerializeType serializeType, string name, int minRefreshSize
                , Func<TModelType, TKeyType> getKey, Action<TModelType, TKeyType> setKey)
                : base(serializeType, name ?? typeof(TValueType).onlyName(), minRefreshSize)
            {
                if (cache == null || Serializer == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                if (setKey == null || getKey == null)
                {
                    cache.Loaded(false);
                    TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                }
                this.cache = cache;
                SetPrimaryKey = setKey;
                GetPrimaryKey = getKey;
            }

            /// <summary>
            ///     自增数据加载基本缓存接口
            /// </summary>
            public TmphILoadCache<TValueType, TModelType, TKeyType> Cache
            {
                get { return cache; }
            }

            /// <summary>
            ///     设置文件头数据
            /// </summary>
            /// <param name="TmphBuffer"></param>
            /// <param name="bufferSize"></param>
            protected unsafe void headerData(byte* TmphBuffer, int bufferSize)
            {
                *(int*)TmphBuffer = TmphPub.PuzzleValue;
                *(int*)(TmphBuffer + sizeof(int)) = sizeof(int) * 4;
                *(int*)(TmphBuffer + sizeof(int) * 2) = bufferSize == 0
                    ? TmphMemoryDatabase.Default.PhysicalBufferSize
                    : bufferSize;
                *(int*)(TmphBuffer + sizeof(int) * 3) = (byte)Serializer.Type;
            }

            /// <summary>
            ///     加载数据
            /// </summary>
            /// <param name="data"></param>
            /// <returns></returns>
            protected unsafe int load(TmphSubArray<byte> data)
            {
                var count = data.Count;
                if (count >= sizeof(int))
                {
                    fixed (byte* dataFixed = data.array)
                    {
                        TValueType value;
                        var TmphBuffer = dataFixed + data.StartIndex;
                        do
                        {
                            if (*(int*)TmphBuffer <= sizeof(int) * 2 || (count -= *(int*)TmphBuffer) < 0) return 0;
                            switch (*(int*)(TmphBuffer + sizeof(int)))
                            {
                                case (byte)TmphLogType.Insert:
                                    if (
                                        !Serializer.LoadInsert(value = TmphConstructor<TValueType>.New(),
                                            TmphBuffer + sizeof(int) * 2, *(int*)TmphBuffer - sizeof(int) * 2))
                                        return 0;
                                    cache.LoadInsert(value, GetPrimaryKey(value), *(int*)TmphBuffer);
                                    dataSize += *(int*)TmphBuffer;
                                    break;

                                case (byte)TmphLogType.Update:
                                    if (
                                        !Serializer.LoadUpdate(value = TmphConstructor<TValueType>.New(),
                                            TmphBuffer + sizeof(int) * 2, *(int*)TmphBuffer - sizeof(int) * 2))
                                        return 0;
                                    cache.LoadUpdate(value, GetPrimaryKey(value), Serializer.LoadMemberMap);
                                    break;

                                case (byte)TmphLogType.Delete:
                                    var key = default(TKeyType);
                                    if (
                                        !Serializer.LoadDelete(ref key, TmphBuffer + sizeof(int) * 2,
                                            *(int*)TmphBuffer - sizeof(int) * 2))
                                        return 0;
                                    dataSize -= cache.LoadDelete(key);
                                    break;

                                case (byte)TmphLogType.MemberData:
                                    if (
                                        !Serializer.LoadMembers(TmphBuffer + sizeof(int) * 2, *(int*)TmphBuffer - sizeof(int) * 2))
                                        return 0;
                                    dataSize -= dataMemberSize;
                                    dataSize += (dataMemberSize = *(int*)TmphBuffer);
                                    break;

                                default:
                                    return 0;
                            }
                            TmphBuffer += *(int*)TmphBuffer;
                        } while (count != 0);
                    }
                    return 1;
                }
                return 0;
            }

            /// <summary>
            ///     释放资源
            /// </summary>
            protected override void dispose()
            {
                base.dispose();
                CLB.TmphPub.Dispose(ref cache);
            }
        }

        /// <summary>
        ///     内存数据库表格操作工具(本地)
        /// </summary>
        /// <typeparam name="TValueType">表格类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <typeparam name="TKeyType">关键字类型</typeparam>
        public abstract class TmphLocalTable<TValueType, TModelType, TKeyType> : TmphTable<TValueType, TModelType, TKeyType>
            where TValueType : class, TModelType
            where TModelType : class
            where TKeyType : IEquatable<TKeyType>
        {
            /// <summary>
            ///     数据库物理层
            /// </summary>
            internal TmphPhysical Physical;

            /// <summary>
            ///     数据库物理层访问锁
            /// </summary>
            protected int physicalLock;

            /// <summary>
            ///     内存数据库表格操作工具
            /// </summary>
            /// <param name="cache">自增数据加载基本缓存接口</param>
            /// <param name="serializeType">数据序列化类型</param>
            /// <param name="name">表格名称</param>
            /// <param name="minRefreshSize">数据库日志文件最小刷新尺寸(单位:KB)</param>
            /// <param name="bufferSize">缓冲区字节大小</param>
            protected TmphLocalTable(TmphILoadCache<TValueType, TModelType, TKeyType> cache
                , TmphSerializeType serializeType, string name, int minRefreshSize, int bufferSize
                , Func<TModelType, TKeyType> getKey, Action<TModelType, TKeyType> setKey)
                : base(cache, serializeType, name, minRefreshSize, getKey, setKey)
            {
                TmphThreadPool.TinyPool.Start(open, bufferSize);
            }

            /// <summary>
            ///     释放资源
            /// </summary>
            protected override void dispose()
            {
                base.dispose();
                CLB.TmphPub.Dispose(ref Physical);
            }

            /// <summary>
            ///     打开数据库文件
            /// </summary>
            /// <param name="bufferSize"></param>
            private unsafe void open(int bufferSize)
            {
                int isLoaded = 0, isPhysicalLoaded = 1;
                try
                {
                    if ((Physical = new TmphPhysical(name)).LastException == null)
                    {
                        if (Physical.IsLoader)
                        {
                            isPhysicalLoaded = 0;
                            var header = Physical.LoadHeader();
                            if (header.Count == sizeof(int) * 2)
                            {
                                fixed (byte* headerFixed = header.array)
                                {
                                    var TmphBuffer = headerFixed + header.StartIndex;
                                    if (((*(int*)TmphBuffer ^
                                          (bufferSize == 0 ? TmphMemoryDatabase.Default.PhysicalBufferSize : bufferSize)) |
                                         (*(int*)(TmphBuffer + sizeof(int)) ^ (byte)Serializer.Type)) == 0)
                                    {
                                        Serializer.ReadyLoad();
                                        try
                                        {
                                            do
                                            {
                                                var data = Physical.Load();
                                                if (data.array == null) break;
                                                if (data.Count == 0)
                                                {
                                                    if (Physical.Loaded(true))
                                                    {
                                                        isPhysicalLoaded = 1;
                                                        if (Serializer.IsCurrentDataMembers || members())
                                                        {
                                                            cache.Loaded(true);
                                                            isLoaded = 1;
                                                            return;
                                                        }
                                                    }
                                                    break;
                                                }
                                                if (load(data) == 0) break;
                                            } while (true);
                                        }
                                        finally
                                        {
                                            Serializer.Loaded();
                                        }
                                    }
                                }
                            }
                            TmphLog.Error.Add("数据库 " + name + " 加载失败", true, false);
                        }
                        else
                        {
                            byte* TmphBuffer = stackalloc byte[sizeof(int) * 4];
                            headerData(TmphBuffer, bufferSize);
                            if (Physical.Create(TmphBuffer, sizeof(int) * 4) && (!Serializer.IsDataMember || members()))
                            {
                                cache.Loaded(true);
                                isLoaded = 1;
                                return;
                            }
                            TmphLog.Error.Add("数据库 " + name + " 创建失败", true, false);
                        }
                    }
                    else TmphLog.Error.Add("数据库 " + name + " 打开失败", true, false);
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, "数据库 " + name + " 打开失败", false);
                }
                finally
                {
                    if (isPhysicalLoaded == 0) Physical.Loaded(false);
                    if (isLoaded == 0) Dispose();
                }
            }

            /// <summary>
            ///     成员变换
            /// </summary>
            /// <returns></returns>
            private unsafe bool members()
            {
                var data = TmphUnmanagedPool.StreamBuffers.Get();
                var stream = Interlocked.Exchange(ref this.stream, null);
                try
                {
                    if (stream == null) stream = new TmphUnmanagedStream(data.Byte, TmphUnmanagedPool.StreamBuffers.Size);
                    else stream.Reset(data.Byte, TmphUnmanagedPool.StreamBuffers.Size);
                    stream.Unsafer.AddLength(sizeof(int) * 2);
                    Serializer.Members(stream);
                    var TmphBuffer = stream.Data;
                    *(int*)(TmphBuffer + sizeof(int)) = (byte)TmphLogType.MemberData;
                    if (Physical.Append(TmphBuffer, *(int*)TmphBuffer = stream.Length) != 0)
                    {
                        dataSize -= dataMemberSize;
                        dataSize += (dataMemberSize = stream.Length);
                        return true;
                    }
                }
                finally
                {
                    if (isDisposed == 0)
                    {
                        if (stream != null) Interlocked.Exchange(ref this.stream, stream);
                    }
                    else CLB.TmphPub.Dispose(ref stream);
                    TmphUnmanagedPool.StreamBuffers.Push(ref data);
                }
                return false;
            }

            /// <summary>
            ///     添加数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="isCopy">是否复制缓存数据</param>
            /// <returns></returns>
            public abstract TValueType Insert(TValueType value, bool isCopy = true);

            /// <summary>
            ///     更新数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="memberMap"></param>
            /// <param name="isCopy">是否复制缓存数据</param>
            /// <returns></returns>
            public unsafe TValueType Update(TValueType value, TmphMemberMap memberMap)
            {
                if (value != null && memberMap != null && !memberMap.IsDefault)
                {
                    TmphUnmanagedStream stream = null;
                    var key = GetPrimaryKey(value);
                    var isError = 0;
                    TmphInterlocked.CompareSetSleep1(ref physicalLock);
                    try
                    {
                        if (cache.ContainsKey(key))
                        {
                            stream = Interlocked.Exchange(ref this.stream, null);
                            var TmphBuffer = Physical.LocalBuffer();
                            if (TmphBuffer == null)
                            {
                                value = null;
                                isError = 1;
                            }
                            else
                            {
                                fixed (byte* bufferFixed = TmphBuffer)
                                {
                                    if (stream == null)
                                        stream = new TmphUnmanagedStream(bufferFixed + Physical.BufferIndex,
                                            TmphBuffer.Length - Physical.BufferIndex);
                                    else
                                        stream.Reset(bufferFixed + Physical.BufferIndex,
                                            TmphBuffer.Length - Physical.BufferIndex);
                                    stream.Unsafer.AddLength(sizeof(int) * 2);
                                    using (var updateMemberMap = GetUpdateMemberMap(memberMap))
                                        Serializer.Update(value, stream, updateMemberMap);
                                    var data = stream.Data;
                                    *(int*)(data + sizeof(int)) = (byte)TmphLogType.Update;
                                    *(int*)data = stream.Length;
                                    isError = 1;
                                    if (data == bufferFixed + Physical.BufferIndex)
                                    {
                                        Physical.BufferIndex += stream.Length;
                                        if ((value = cache.Update(value, key, memberMap)) == null) isError = 0;
                                    }
                                    else if (Physical.Append(stream.Data, stream.Length) == 0) value = null;
                                    else if ((value = cache.Update(value, key, memberMap)) == null) isError = 0;
                                }
                            }
                        }
                        else value = null;
                    }
                    catch (Exception error)
                    {
                        value = null;
                        TmphLog.Error.Add(error, null, false);
                    }
                    finally
                    {
                        if (isDisposed == 0)
                        {
                            if (stream != null) Interlocked.Exchange(ref this.stream, stream);
                        }
                        else CLB.TmphPub.Dispose(ref stream);
                        physicalLock = 0;
                    }
                    if (value != null)
                    {
                        Physical.WaitBuffer();
                        return value;
                    }
                    if (isError != 0) Dispose();
                }
                return null;
            }

            /// <summary>
            ///     删除数据
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public TValueType Delete(TValueType value)
            {
                return value != null ? Delete(GetPrimaryKey(value)) : null;
            }

            /// <summary>
            ///     删除数据
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public unsafe TValueType Delete(TKeyType key)
            {
                TmphUnmanagedStream stream = null;
                TValueType value = null;
                int isError = 0, logSize;
                TmphInterlocked.CompareSetSleep1(ref physicalLock);
                try
                {
                    if (cache.ContainsKey(key))
                    {
                        stream = Interlocked.Exchange(ref this.stream, null);
                        var TmphBuffer = Physical.LocalBuffer();
                        if (TmphBuffer == null) isError = 1;
                        else
                        {
                            fixed (byte* bufferFixed = TmphBuffer)
                            {
                                if (stream == null)
                                    stream = new TmphUnmanagedStream(bufferFixed + Physical.BufferIndex,
                                        TmphBuffer.Length - Physical.BufferIndex);
                                else
                                    stream.Reset(bufferFixed + Physical.BufferIndex,
                                        TmphBuffer.Length - Physical.BufferIndex);
                                stream.Unsafer.AddLength(sizeof(int) * 2);
                                Serializer.Delete(key, stream);
                                var data = stream.Data;
                                *(int*)(data + sizeof(int)) = (byte)TmphLogType.Delete;
                                *(int*)data = stream.Length;
                                isError = 1;
                                if (data == bufferFixed + Physical.BufferIndex)
                                {
                                    Physical.BufferIndex += stream.Length;
                                    if ((value = cache.Delete(key, out logSize)) != null)
                                    {
                                        dataSize -= logSize;
                                        isError = 0;
                                    }
                                }
                                else if (Physical.Append(stream.Data, stream.Length) != 0 &&
                                         (value = cache.Delete(key, out logSize)) != null)
                                {
                                    dataSize -= logSize;
                                    isError = 0;
                                }
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                finally
                {
                    if (isDisposed == 0)
                    {
                        if (stream != null) Interlocked.Exchange(ref this.stream, stream);
                    }
                    else CLB.TmphPub.Dispose(ref stream);
                    physicalLock = 0;
                }
                if (value != null)
                {
                    Physical.WaitBuffer();
                    return value;
                }
                if (isError != 0) Dispose();
                return null;
            }

            /// <summary>
            ///     刷新写入文件缓存区
            /// </summary>
            /// <param name="isWriteFile">是否写入文件</param>
            /// <returns>是否操作成功</returns>
            public bool Flush(bool isWriteFile)
            {
                if (Physical.Flush() && Physical.FlushFile(isWriteFile))
                {
                    return true;
                }
                Dispose();
                return false;
            }
        }

        /// <summary>
        ///     内存数据库表格操作工具(远程)
        /// </summary>
        /// <typeparam name="TValueType">表格类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <typeparam name="TKeyType">关键字类型</typeparam>
        public abstract class TmphRemoteTable<TValueType, TModelType, TKeyType> : TmphTable<TValueType, TModelType, TKeyType>
            where TValueType : class, TModelType
            where TModelType : class
            where TKeyType : IEquatable<TKeyType>
        {
            /// <summary>
            ///     是否自动关系内存数据库客户端
            /// </summary>
            private readonly bool isCloseCient;

            /// <summary>
            ///     自动关闭客户端时是否等待数据库关闭
            /// </summary>
            private readonly bool isWaitClose;

            /// <summary>
            ///     最后一次添加数据操作返回值
            /// </summary>
            protected int appendValue;

            /// <summary>
            ///     缓存访问锁
            /// </summary>
            protected int cacheLock;

            /// <summary>
            ///     内存数据库客户端
            /// </summary>
            protected TmphMemoryDatabasePhysical client;

            /// <summary>
            ///     内存池
            /// </summary>
            protected TmphUnmanagedPool memoryPool;

            /// <summary>
            ///     数据库物理层唯一标识
            /// </summary>
            protected TmphPhysicalServer.TmphTimeIdentity physicalIdentity;

            /// <summary>
            ///     内存数据库表格操作工具
            /// </summary>
            /// <param name="TmphClient"></param>
            /// <param name="cache">自增数据加载基本缓存接口</param>
            /// <param name="serializeType">序列化类型</param>
            /// <param name="name">表格名称</param>
            /// <param name="minRefreshSize">数据库日志文件最小刷新尺寸(单位:KB)</param>
            /// <param name="bufferSize">缓冲区字节大小</param>
            /// <param name="isWaitClose">是否等待数据库关闭</param>
            /// <param name="isCloseCient">是否自动关系内存数据库客户端</param>
            protected TmphRemoteTable(TmphMemoryDatabasePhysical TmphClient, TmphILoadCache<TValueType, TModelType, TKeyType> cache
                , TmphSerializeType serializeType, string name, int minRefreshSize, int bufferSize
                , Func<TModelType, TKeyType> getKey, Action<TModelType, TKeyType> setKey, TmphUnmanagedPool memoryPool,
                bool isWaitClose, bool isCloseCient)
                : base(cache, serializeType, name, minRefreshSize, getKey, setKey)
            {
                if (TmphClient == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                this.client = TmphClient;
                this.isCloseCient = isCloseCient;
                this.isWaitClose = isWaitClose;
                this.memoryPool = memoryPool ?? TmphUnmanagedPool.TinyBuffers;
                new TmphLoader(this, bufferSize).Open();
            }

            /// <summary>
            ///     释放资源
            /// </summary>
            protected override void dispose()
            {
                base.dispose();
                if (isCloseCient)
                {
                    try
                    {
                        if (isWaitClose)
                        {
                            client.flush(physicalIdentity);
                            client.flushFile(physicalIdentity, false);
                        }
                        client.close(physicalIdentity);
                    }
                    catch (Exception error)
                    {
                        TmphLog.Default.Add(error, null, false);
                    }
                    CLB.TmphPub.Dispose(ref client);
                }
                TmphTypePool<TmphInsertWaiter>.Clear();
                TmphTypePool<TmphUpdateCallbacker>.Clear();
                TmphTypePool<TmphDeleteWaiter>.Clear();
                TmphTypePool<TmphInsertCallbacker>.Clear();
                TmphTypePool<TmphUpdateCallbacker>.Clear();
                TmphTypePool<TmphDeleteCallbacker>.Clear();
            }

            /// <summary>
            ///     添加数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="isCopy">是否复制缓存数据</param>
            /// <returns></returns>
            public abstract TValueType Insert(TValueType value, bool isCopy = true);

            /// <summary>
            ///     添加数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="key"></param>
            /// <param name="isCopy">是否复制缓存数据</param>
            /// <returns></returns>
            protected TValueType insert(TValueType value, TKeyType key, bool isCopy)
            {
                TmphInsertWaiter inserter = null;
                try
                {
                    if ((inserter = TmphInsertWaiter.Get(this, value, key, isCopy)) != null)
                    {
                        insert(value, inserter);
                        return inserter.Value;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Default.Add(error, null, false);
                }
                if (inserter != null) inserter.Cancel();
                return null;
            }

            /// <summary>
            ///     添加数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="callbacker"></param>
            private unsafe void insert(TValueType value, TmphCallbacker callbacker)
            {
                var stream = callbacker.Stream;
                stream.Unsafer.AddLength(sizeof(TmphPhysicalServer.TmphTimeIdentity) + sizeof(int) * 2);
                Serializer.Insert(value, stream);
                var data = stream.Data;
                *(TmphPhysicalServer.TmphTimeIdentity*)data = physicalIdentity;
                *(int*)(data + sizeof(TmphPhysicalServer.TmphTimeIdentity)) = stream.Length -
                                                                           sizeof(TmphPhysicalServer.TmphTimeIdentity);
                *(int*)(data + (sizeof(TmphPhysicalServer.TmphTimeIdentity) + sizeof(int))) = (byte)TmphLogType.Insert;
                client.append(stream, callbacker.Callback);
            }

            /// <summary>
            ///     添加数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="onInserted"></param>
            /// <param name="isCallbackTask">添加数据回调是否使用任务模式</param>
            /// <param name="isCopy">是否复制缓存数据</param>
            /// <returns></returns>
            public abstract void Insert(TValueType value, Action<TValueType> onInserted, bool isCallbackTask = true,
                bool isCopy = true);

            /// <summary>
            ///     添加数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="key"></param>
            /// <param name="onInserted"></param>
            /// <param name="isCallbackTask">添加数据回调是否使用任务模式</param>
            /// <param name="isCopy">是否复制缓存数据</param>
            /// <returns></returns>
            protected bool insert(TValueType value, TKeyType key, Action<TValueType> onInserted, bool isCallbackTask,
                bool isCopy)
            {
                TmphInsertCallbacker inserter = null;
                try
                {
                    if ((inserter = TmphInsertCallbacker.Get(this, value, key, onInserted, isCallbackTask, isCopy)) !=
                        null)
                    {
                        insert(value, inserter);
                        return true;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Default.Add(error, null, false);
                }
                if (inserter != null)
                {
                    inserter.Cancel();
                    return true;
                }
                return false;
            }

            /// <summary>
            ///     更新数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="memberMap"></param>
            /// <param name="callbacker"></param>
            private unsafe void update(TValueType value, TmphMemberMap memberMap, TmphCallbacker callbacker)
            {
                var stream = callbacker.Stream;
                stream.Unsafer.AddLength(sizeof(TmphPhysicalServer.TmphTimeIdentity) + sizeof(int) * 2);
                using (var updateMemberMap = GetUpdateMemberMap(memberMap))
                    Serializer.Update(value, stream, updateMemberMap);
                var data = stream.Data;
                *(TmphPhysicalServer.TmphTimeIdentity*)data = physicalIdentity;
                *(int*)(data + sizeof(TmphPhysicalServer.TmphTimeIdentity)) = stream.Length -
                                                                           sizeof(TmphPhysicalServer.TmphTimeIdentity);
                *(int*)(data + (sizeof(TmphPhysicalServer.TmphTimeIdentity) + sizeof(int))) = (byte)TmphLogType.Update;
                client.append(stream, callbacker.Callback);
            }

            /// <summary>
            ///     更新数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="memberMap"></param>
            public TValueType Update(TValueType value, TmphMemberMap memberMap)
            {
                if (value != null && memberMap != null && !memberMap.IsDefault)
                {
                    var key = GetPrimaryKey(value);
                    var isKey = true;
                    TmphInterlocked.CompareSetSleep0(ref cacheLock);
                    try
                    {
                        isKey = cache.ContainsKey(key);
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                    finally
                    {
                        cacheLock = 0;
                    }
                    if (isKey)
                    {
                        TmphUpdateWaiter updater = null;
                        try
                        {
                            if ((updater = TmphUpdateWaiter.Get(this, value, key, memberMap)) != null)
                            {
                                update(value, memberMap, updater);
                                return updater.Value;
                            }
                        }
                        catch (Exception error)
                        {
                            TmphLog.Default.Add(error, null, false);
                        }
                        if (updater != null) updater.Cancel();
                    }
                }
                return null;
            }

            /// <summary>
            ///     更新数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="memberMap"></param>
            /// <param name="onUpdated"></param>
            /// <param name="isCallbackTask"></param>
            public void Update(TValueType value, TmphMemberMap memberMap, Action<TValueType> onUpdated,
                bool isCallbackTask = true)
            {
                if (value != null && memberMap != null && !memberMap.IsDefault)
                {
                    var key = GetPrimaryKey(value);
                    var isKey = true;
                    TmphInterlocked.CompareSetSleep0(ref cacheLock);
                    try
                    {
                        isKey = cache.ContainsKey(key);
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                    finally
                    {
                        cacheLock = 0;
                    }
                    if (isKey)
                    {
                        TmphUpdateCallbacker updater = null;
                        try
                        {
                            if (
                                (updater =
                                    TmphUpdateCallbacker.Get(this, value, key, memberMap, onUpdated, isCallbackTask)) !=
                                null)
                            {
                                update(value, memberMap, updater);
                                return;
                            }
                        }
                        catch (Exception error)
                        {
                            TmphLog.Default.Add(error, null, false);
                        }
                        if (updater != null)
                        {
                            updater.Cancel();
                            return;
                        }
                    }
                }
                if (onUpdated != null)
                {
                    if (isCallbackTask) TmphTask.Tiny.Add(onUpdated, null);
                    else onUpdated(null);
                }
            }

            /// <summary>
            ///     删除数据
            /// </summary>
            /// <param name="value"></param>
            public TValueType Delete(TValueType value)
            {
                return value != null ? Delete(GetPrimaryKey(value)) : null;
            }

            /// <summary>
            ///     删除数据
            /// </summary>
            /// <param name="value"></param>
            public TValueType Delete(TKeyType key)
            {
                var isKey = true;
                TmphInterlocked.CompareSetSleep0(ref cacheLock);
                try
                {
                    isKey = cache.ContainsKey(key);
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                finally
                {
                    cacheLock = 0;
                }
                if (isKey)
                {
                    TmphDeleteWaiter deleter = null;
                    try
                    {
                        if ((deleter = TmphDeleteWaiter.Get(this, key)) != null)
                        {
                            delete(key, deleter);
                            return deleter.Value;
                        }
                    }
                    catch (Exception error)
                    {
                        TmphLog.Default.Add(error, null, false);
                    }
                    if (deleter != null) deleter.Cancel();
                }
                return null;
            }

            /// <summary>
            ///     删除数据
            /// </summary>
            /// <param name="key"></param>
            /// <param name="callbacker"></param>
            private unsafe void delete(TKeyType key, TmphCallbacker callbacker)
            {
                var stream = callbacker.Stream;
                stream.Unsafer.AddLength(sizeof(TmphPhysicalServer.TmphTimeIdentity) + sizeof(int) * 2);
                Serializer.Delete(key, stream);
                var data = stream.Data;
                *(TmphPhysicalServer.TmphTimeIdentity*)data = physicalIdentity;
                *(int*)(data + sizeof(TmphPhysicalServer.TmphTimeIdentity)) = stream.Length -
                                                                           sizeof(TmphPhysicalServer.TmphTimeIdentity);
                *(int*)(data + (sizeof(TmphPhysicalServer.TmphTimeIdentity) + sizeof(int))) = (byte)TmphLogType.Delete;
                client.append(stream, callbacker.Callback);
            }

            /// <summary>
            ///     删除数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="onDeleted"></param>
            /// <param name="isCallbackTask"></param>
            public void Delete(TValueType value, Action<TValueType> onDeleted, bool isCallbackTask = true)
            {
                if (value != null)
                {
                    Delete(GetPrimaryKey(value), onDeleted, isCallbackTask);
                }
                else if (onDeleted != null)
                {
                    if (isCallbackTask) TmphTask.Tiny.Add(onDeleted, null);
                    else onDeleted(null);
                }
            }

            /// <summary>
            ///     删除数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="onDeleted"></param>
            /// <param name="isCallbackTask"></param>
            public void Delete(TKeyType key, Action<TValueType> onDeleted, bool isCallbackTask = true)
            {
                var isKey = true;
                TmphInterlocked.CompareSetSleep0(ref cacheLock);
                try
                {
                    isKey = cache.ContainsKey(key);
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                finally
                {
                    cacheLock = 0;
                }
                if (isKey)
                {
                    TmphDeleteCallbacker deleter = null;
                    try
                    {
                        if ((deleter = TmphDeleteCallbacker.Get(this, key, onDeleted, isCallbackTask)) != null)
                        {
                            delete(key, deleter);
                            return;
                        }
                    }
                    catch (Exception error)
                    {
                        TmphLog.Default.Add(error, null, false);
                    }
                    if (deleter != null)
                    {
                        deleter.Cancel();
                        return;
                    }
                }
                if (onDeleted != null)
                {
                    if (isCallbackTask) TmphTask.Tiny.Add(onDeleted, null);
                    else onDeleted(null);
                }
            }

            /// <summary>
            ///     刷新写入文件缓存区
            /// </summary>
            /// <param name="isWriteFile">是否写入文件</param>
            /// <returns>是否操作成功</returns>
            public bool Flush(bool isWriteFile)
            {
                try
                {
                    if (client.flush(physicalIdentity) && client.flushFile(physicalIdentity, isWriteFile))
                    {
                        return true;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Default.Add(error, null, false);
                }
                Dispose();
                return false;
            }

            /// <summary>
            ///     数据加载
            /// </summary>
            private sealed class TmphLoader
            {
                /// <summary>
                ///     物理层缓冲区字节数
                /// </summary>
                private readonly int bufferSize;

                /// <summary>
                ///     内存数据库表格操作工具
                /// </summary>
                private readonly TmphRemoteTable<TValueType, TModelType, TKeyType> table;

                /// <summary>
                ///     数据是否加载成功
                /// </summary>
                private int isLoaded;

                /// <summary>
                ///     是否需要通知物理层加载失败
                /// </summary>
                private int isPhysicalLoaded = 1;

                /// <summary>
                ///     是否已经初始化序列化
                /// </summary>
                private int isSerializerLoaded;

                /// <summary>
                ///     加载数据
                /// </summary>
                private Action<TmphAsynchronousMethod.TmphReturnValue<TmphTcpBase.TmphSubByteArrayBuffer>> onLoadHandle;

                /// <summary>
                ///     数据加载
                /// </summary>
                /// <param name="table">内存数据库表格操作工具</param>
                public TmphLoader(TmphRemoteTable<TValueType, TModelType, TKeyType> table, int bufferSize)
                {
                    this.table = table;
                    this.bufferSize = bufferSize;
                }

                /// <summary>
                ///     打开数据库
                /// </summary>
                public void Open()
                {
                    try
                    {
                        table.client.open(table.name, onOpen);
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, "数据库 " + table.name + " 打开失败", false);
                        end();
                    }
                }

                /// <summary>
                ///     数据加载完毕
                /// </summary>
                private void end()
                {
                    if (isSerializerLoaded != 0 && table.Serializer != null) table.Serializer.Loaded();
                    if (isPhysicalLoaded == 0)
                    {
                        try
                        {
                            table.client.loaded(table.physicalIdentity, false);
                        }
                        catch (Exception error)
                        {
                            TmphLog.Default.Add(error, null, false);
                        }
                    }
                    if (isLoaded == 0) table.Dispose();
                }

                /// <summary>
                ///     打开数据库
                /// </summary>
                /// <param name="identity">数据库物理层初始化信息</param>
                private unsafe void onOpen(
                    TmphAsynchronousMethod.TmphReturnValue<TmphPhysicalServer.TmphPhysicalIdentity> identity)
                {
                    if (identity.IsReturn)
                    {
                        table.physicalIdentity = identity.Value.Identity;
                        if (identity.Value.IsOpen)
                        {
                            try
                            {
                                if (identity.Value.IsLoader)
                                {
                                    isPhysicalLoaded = 0;
                                    table.client.loadHeader(table.physicalIdentity, onLoadHeader);
                                    return;
                                }
                                byte* TmphBuffer = stackalloc byte[sizeof(TmphPhysicalServer.TmphTimeIdentity) + sizeof(int) * 4];
                                *(TmphPhysicalServer.TmphTimeIdentity*)TmphBuffer = table.physicalIdentity;
                                table.headerData(TmphBuffer + sizeof(TmphPhysicalServer.TmphTimeIdentity), bufferSize);
                                (table.stream =
                                    new TmphUnmanagedStream(TmphBuffer,
                                        sizeof(TmphPhysicalServer.TmphTimeIdentity) + sizeof(int) * 4)).Unsafer.AddLength(
                                            sizeof(TmphPhysicalServer.TmphTimeIdentity) + sizeof(int) * 4);
                                if (table.client.create(table.stream) && (!table.Serializer.IsDataMember || members()))
                                {
                                    table.cache.Loaded(true);
                                    isLoaded = 1;
                                    end();
                                    return;
                                }
                                TmphLog.Error.Add("数据库 " + table.name + " 创建失败", false, false);
                            }
                            catch (Exception error)
                            {
                                TmphLog.Error.Add(error,
                                    "数据库 " + table.name + (identity.Value.IsLoader ? " 加载失败" : " 创建失败"), false);
                            }
                            end();
                            return;
                        }
                    }
                    TmphLog.Error.Add("数据库 " + table.name + " 打开失败", false, false);
                    end();
                }

                /// <summary>
                ///     成员变换
                /// </summary>
                /// <returns></returns>
                private unsafe bool members()
                {
                    var data = TmphUnmanagedPool.StreamBuffers.Get();
                    var stream = Interlocked.Exchange(ref table.stream, null);
                    try
                    {
                        if (stream == null)
                            stream = new TmphUnmanagedStream(data.Byte, TmphUnmanagedPool.StreamBuffers.Size);
                        else stream.Reset(data.Byte, TmphUnmanagedPool.StreamBuffers.Size);
                        stream.Unsafer.AddLength(sizeof(TmphPhysicalServer.TmphTimeIdentity) + sizeof(int) * 2);
                        table.Serializer.Members(stream);
                        var TmphBuffer = stream.Data;
                        *(TmphPhysicalServer.TmphTimeIdentity*)TmphBuffer = table.physicalIdentity;
                        *(int*)(TmphBuffer + (sizeof(TmphPhysicalServer.TmphTimeIdentity) + sizeof(int))) =
                            (byte)TmphLogType.MemberData;
                        *(int*)(TmphBuffer + sizeof(TmphPhysicalServer.TmphTimeIdentity)) = stream.Length -
                                                                                     sizeof(
                                                                                         TmphPhysicalServer.TmphTimeIdentity);
                        int value = table.client.append(stream);
                        if (value != 0)
                        {
                            table.dataSize -= table.dataMemberSize;
                            table.dataSize += (table.dataMemberSize = stream.Length);
                            return true;
                        }
                    }
                    finally
                    {
                        if (table.isDisposed == 0)
                        {
                            if (stream != null) Interlocked.Exchange(ref table.stream, stream);
                        }
                        else CLB.TmphPub.Dispose(ref stream);
                        TmphUnmanagedPool.StreamBuffers.Push(ref data);
                    }
                    return false;
                }

                /// <summary>
                ///     加载文件头部数据
                /// </summary>
                /// <param name="data"></param>
                private unsafe void onLoadHeader(TmphAsynchronousMethod.TmphReturnValue<TmphTcpBase.TmphSubByteArrayBuffer> data)
                {
                    if (data.IsReturn)
                    {
                        var header = data.Value.TmphBuffer;
                        if (header.Count == sizeof(int) * 2)
                        {
                            fixed (byte* headerFixed = header.array)
                            {
                                var TmphBuffer = headerFixed + header.StartIndex;
                                if (((*(int*)TmphBuffer ^
                                      (bufferSize == 0 ? TmphMemoryDatabase.Default.PhysicalBufferSize : bufferSize)) |
                                     (*(int*)(TmphBuffer + sizeof(int)) ^ (byte)table.Serializer.Type)) == 0)
                                {
                                    try
                                    {
                                        table.Serializer.ReadyLoad();
                                        isSerializerLoaded = 1;
                                        load();
                                    }
                                    catch (Exception error)
                                    {
                                        TmphLog.Error.Add(error, "数据库 " + table.name + " 加载失败", false);
                                        TmphThreadPool.TinyPool.Start(end);
                                    }
                                    return;
                                }
                            }
                        }
                    }
                    TmphLog.Error.Add("数据库 " + table.name + " 加载失败", false, false);
                    TmphThreadPool.TinyPool.Start(end);
                }

                /// <summary>
                ///     加载数据
                /// </summary>
                private void load()
                {
                    try
                    {
                        if (onLoadHandle == null) onLoadHandle = onLoad;
                        table.client.load(table.physicalIdentity, onLoadHandle);
                        return;
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, "数据库 " + table.name + " 加载失败", false);
                    }
                    TmphThreadPool.TinyPool.Start(end);
                }

                /// <summary>
                ///     加载数据
                /// </summary>
                /// <param name="data"></param>
                private void onLoad(TmphAsynchronousMethod.TmphReturnValue<TmphTcpBase.TmphSubByteArrayBuffer> TmphBuffer)
                {
                    if (TmphBuffer.IsReturn)
                    {
                        var data = TmphBuffer.Value.TmphBuffer;
                        if (data.array != null)
                        {
                            try
                            {
                                if (data.Count == 0)
                                {
                                    TmphThreadPool.TinyPool.Start(loaded);
                                    return;
                                }
                                if (table.load(data) != 0)
                                {
                                    load();
                                    return;
                                }
                            }
                            catch (Exception error)
                            {
                                TmphLog.Error.Add(error, "数据库 " + table.name + " 加载失败", false);
                                TmphThreadPool.TinyPool.Start(end);
                                return;
                            }
                        }
                    }
                    TmphLog.Error.Add("数据库 " + table.name + " 加载失败", false, false);
                    TmphThreadPool.TinyPool.Start(end);
                }

                /// <summary>
                ///     数据加载完毕
                /// </summary>
                private void loaded()
                {
                    try
                    {
                        if (table.client.loaded(table.physicalIdentity, true))
                        {
                            isPhysicalLoaded = 1;
                            if (table.Serializer.IsCurrentDataMembers || members())
                            {
                                table.cache.Loaded(true);
                                isLoaded = 1;
                                return;
                            }
                        }
                        TmphLog.Error.Add("数据库 " + table.name + " 加载失败", false, false);
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, "数据库 " + table.name + " 加载失败", false);
                    }
                    finally
                    {
                        end();
                    }
                }
            }

            /// <summary>
            ///     回调操作
            /// </summary>
            private abstract unsafe class TmphCallbacker : IDisposable
            {
                /// <summary>
                ///     内存流
                /// </summary>
                internal readonly TmphUnmanagedStream Stream;

                /// <summary>
                ///     内存缓冲区
                /// </summary>
                protected TmphPointer TmphBuffer;

                /// <summary>
                ///     操作回调委托
                /// </summary>
                internal Action<TmphAsynchronousMethod.TmphReturnValue<int>> Callback;

                /// <summary>
                ///     关键字
                /// </summary>
                protected TKeyType key;

                /// <summary>
                ///     内存池
                /// </summary>
                protected TmphUnmanagedPool memoryPool;

                /// <summary>
                ///     内存数据库表格操作工具
                /// </summary>
                protected TmphRemoteTable<TValueType, TModelType, TKeyType> table;

                /// <summary>
                ///     添加数据
                /// </summary>
                protected TmphCallbacker(TmphUnmanagedPool memoryPool)
                {
                    TmphBuffer = (this.memoryPool = memoryPool).Get();
                    Stream = new TmphUnmanagedStream(TmphBuffer.Byte, memoryPool.Size);
                }

                /// <summary>
                ///     释放资源
                /// </summary>
                public virtual void Dispose()
                {
                    Stream.Dispose();
                    if (memoryPool != null)
                    {
                        memoryPool.Push(ref TmphBuffer);
                        memoryPool = null;
                    }
                }
            }

            /// <summary>
            ///     同步操作
            /// </summary>
            private abstract unsafe class TmphWaiter : TmphCallbacker
            {
                /// <summary>
                ///     添加数据返回值
                /// </summary>
                protected TmphAsynchronousMethod.TmphReturnValue<int> returnValue;

                /// <summary>
                ///     数据
                /// </summary>
                protected TValueType value;

                /// <summary>
                ///     同步等待事件
                /// </summary>
                protected EventWaitHandle waitHandle;

                /// <summary>
                ///     添加数据
                /// </summary>
                protected TmphWaiter(TmphUnmanagedPool memoryPool)
                    : base(memoryPool)
                {
                    waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                    Callback = onReturn;
                }

                /// <summary>
                ///     释放资源
                /// </summary>
                public override void Dispose()
                {
                    base.Dispose();
                    if (waitHandle != null)
                    {
                        waitHandle.Set();
                        waitHandle.Close();
                        waitHandle = null;
                    }
                }

                /// <summary>
                ///     添加数据回调委托
                /// </summary>
                /// <param name="returnValue"></param>
                private void onReturn(TmphAsynchronousMethod.TmphReturnValue<int> returnValue)
                {
                    if (memoryPool != null && Stream.Data != TmphBuffer.Byte)
                    {
                        memoryPool.Push(ref TmphBuffer);
                        memoryPool = null;
                    }
                    this.returnValue = returnValue;
                    waitHandle.Set();
                }
            }

            /// <summary>
            ///     添加数据
            /// </summary>
            private sealed unsafe class TmphInsertWaiter : TmphWaiter
            {
                /// <summary>
                ///     是否复制缓存数据
                /// </summary>
                private bool isCopy;

                /// <summary>
                ///     添加数据
                /// </summary>
                private TmphInsertWaiter(TmphUnmanagedPool memoryPool)
                    : base(memoryPool)
                {
                }

                /// <summary>
                ///     添加数据
                /// </summary>
                public TValueType Value
                {
                    get
                    {
                        waitHandle.WaitOne();
                        if (returnValue.IsReturn)
                        {
                            var table = this.table;
                            this.table = null;
                            if (returnValue.Value == 0)
                            {
                                this.value = null;
                                this.key = default(TKeyType);
                                TmphTypePool<TmphInsertWaiter>.Push(this);
                                table.Dispose();
                                return null;
                            }
                            var value = this.value;
                            var key = this.key;
                            var logSize = Stream.Length;
                            var isCopy = this.isCopy;
                            this.value = null;
                            this.key = default(TKeyType);
                            TmphTypePool<TmphInsertWaiter>.Push(this);
                            table.appendValue = returnValue.Value;
                            logSize -= sizeof(TmphPhysicalServer.TmphTimeIdentity);
                            TmphInterlocked.CompareSetSleep0(ref table.cacheLock);
                            try
                            {
                                table.dataSize += logSize;
                                value = table.cache.Insert(value, key, logSize, isCopy);
                            }
                            catch (Exception error)
                            {
                                value = null;
                                TmphLog.Default.Add(error, null, false);
                                table.Dispose();
                            }
                            finally
                            {
                                table.cacheLock = 0;
                            }
                            return value;
                        }
                        this.table = null;
                        this.value = null;
                        this.key = default(TKeyType);
                        TmphTypePool<TmphInsertWaiter>.Push(this);
                        return null;
                    }
                }

                /// <summary>
                ///     取消当前调用
                /// </summary>
                public void Cancel()
                {
                    if (memoryPool != null && Stream.Data != TmphBuffer.Byte)
                    {
                        memoryPool.Push(ref TmphBuffer);
                        memoryPool = null;
                    }
                    table = null;
                    value = null;
                    key = default(TKeyType);
                    TmphTypePool<TmphInsertWaiter>.Push(this);
                }

                /// <summary>
                ///     获取添加数据
                /// </summary>
                /// <param name="table"></param>
                /// <param name="value"></param>
                /// <param name="key"></param>
                /// <param name="isCopy"></param>
                /// <returns></returns>
                public static TmphInsertWaiter Get
                    (TmphRemoteTable<TValueType, TModelType, TKeyType> table, TValueType value, TKeyType key, bool isCopy)
                {
                    var inserter = TmphTypePool<TmphInsertWaiter>.Pop();
                    if (inserter == null)
                    {
                        try
                        {
                            inserter = new TmphInsertWaiter(table.memoryPool);
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                            return null;
                        }
                    }
                    inserter.table = table;
                    inserter.value = value;
                    inserter.key = key;
                    inserter.isCopy = isCopy;
                    inserter.Stream.Clear();
                    return inserter;
                }
            }

            /// <summary>
            ///     添加数据
            /// </summary>
            private sealed unsafe class TmphInsertCallbacker : TmphCallbacker
            {
                /// <summary>
                ///     添加数据回调委托
                /// </summary>
                private Action<TValueType> callback;

                /// <summary>
                ///     操作回调是否使用任务模式
                /// </summary>
                private bool isCallbackTask;

                /// <summary>
                ///     是否复制缓存数据
                /// </summary>
                private bool isCopy;

                /// <summary>
                ///     添加数据
                /// </summary>
                private TValueType value;

                /// <summary>
                ///     添加数据
                /// </summary>
                private TmphInsertCallbacker(TmphUnmanagedPool memoryPool)
                    : base(memoryPool)
                {
                    Callback = onReturn;
                }

                /// <summary>
                ///     添加数据回调委托
                /// </summary>
                /// <param name="returnValue"></param>
                private void onReturn(TmphAsynchronousMethod.TmphReturnValue<int> returnValue)
                {
                    if (memoryPool != null && Stream.Data != TmphBuffer.Byte)
                    {
                        memoryPool.Push(ref TmphBuffer);
                        memoryPool = null;
                    }
                    var callback = this.callback;
                    var isCallbackTask = this.isCallbackTask;
                    this.callback = null;
                    if (returnValue.IsReturn)
                    {
                        var table = this.table;
                        this.table = null;
                        if (returnValue.Value == 0)
                        {
                            value = null;
                            key = default(TKeyType);
                            TmphTypePool<TmphInsertCallbacker>.Push(this);
                            table.Dispose();
                            if (callback != null)
                            {
                                if (isCallbackTask) TmphTask.Tiny.Add(callback, null);
                                else callback(null);
                            }
                        }
                        else
                        {
                            var value = this.value;
                            var key = this.key;
                            var logSize = Stream.Length;
                            var isCopy = this.isCopy;
                            this.value = null;
                            this.key = default(TKeyType);
                            TmphTypePool<TmphInsertCallbacker>.Push(this);
                            table.appendValue = returnValue.Value;
                            logSize -= sizeof(TmphPhysicalServer.TmphTimeIdentity);
                            TmphInterlocked.CompareSetSleep0(ref table.cacheLock);
                            try
                            {
                                table.dataSize += logSize;
                                value = table.cache.Insert(value, key, logSize, isCopy);
                            }
                            catch (Exception error)
                            {
                                value = null;
                                TmphLog.Default.Add(error, null, false);
                                table.Dispose();
                            }
                            finally
                            {
                                table.cacheLock = 0;
                            }
                            if (callback != null)
                            {
                                if (isCallbackTask) TmphTask.Tiny.Add(callback, value);
                                else callback(value);
                            }
                        }
                    }
                    else
                    {
                        table = null;
                        value = null;
                        key = default(TKeyType);
                        TmphTypePool<TmphInsertCallbacker>.Push(this);
                        if (callback != null)
                        {
                            if (isCallbackTask) TmphTask.Tiny.Add(callback, null);
                            else callback(null);
                        }
                    }
                }

                /// <summary>
                ///     取消当前调用
                /// </summary>
                public void Cancel()
                {
                    if (memoryPool != null && Stream.Data != TmphBuffer.Byte)
                    {
                        memoryPool.Push(ref TmphBuffer);
                        memoryPool = null;
                    }
                    var callback = this.callback;
                    var isCallbackTask = this.isCallbackTask;
                    table = null;
                    value = null;
                    key = default(TKeyType);
                    this.callback = null;
                    TmphTypePool<TmphInsertCallbacker>.Push(this);
                    if (callback != null)
                    {
                        if (isCallbackTask) TmphTask.Tiny.Add(callback, null);
                        else callback(null);
                    }
                }

                /// <summary>
                ///     获取添加数据
                /// </summary>
                /// <param name="table"></param>
                /// <param name="value"></param>
                /// <param name="key"></param>
                /// <param name="onInserted"></param>
                /// <param name="isCallbackTask"></param>
                /// <param name="isCopy"></param>
                /// <returns></returns>
                public static TmphInsertCallbacker Get
                    (TmphRemoteTable<TValueType, TModelType, TKeyType> table, TValueType value, TKeyType key,
                        Action<TValueType> onInserted, bool isCallbackTask, bool isCopy)
                {
                    var inserter = TmphTypePool<TmphInsertCallbacker>.Pop();
                    if (inserter == null)
                    {
                        try
                        {
                            inserter = new TmphInsertCallbacker(table.memoryPool);
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                            return null;
                        }
                    }
                    inserter.table = table;
                    inserter.value = value;
                    inserter.key = key;
                    inserter.callback = onInserted;
                    inserter.isCallbackTask = isCallbackTask;
                    inserter.isCopy = isCopy;
                    inserter.Stream.Clear();
                    return inserter;
                }
            }

            /// <summary>
            ///     更新数据
            /// </summary>
            private sealed unsafe class TmphUpdateWaiter : TmphWaiter
            {
                /// <summary>
                ///     更新成员位图
                /// </summary>
                private TmphMemberMap memberMap;

                /// <summary>
                ///     添加数据
                /// </summary>
                private TmphUpdateWaiter(TmphUnmanagedPool memoryPool)
                    : base(memoryPool)
                {
                }

                /// <summary>
                ///     更新数据
                /// </summary>
                public TValueType Value
                {
                    get
                    {
                        waitHandle.WaitOne();
                        if (returnValue.IsReturn)
                        {
                            var table = this.table;
                            this.table = null;
                            if (returnValue.Value == 0)
                            {
                                this.value = null;
                                this.key = default(TKeyType);
                                this.memberMap = null;
                                TmphTypePool<TmphUpdateWaiter>.Push(this);
                                table.Dispose();
                                return null;
                            }
                            var value = this.value;
                            var key = this.key;
                            var memberMap = this.memberMap;
                            this.value = null;
                            this.key = default(TKeyType);
                            this.memberMap = null;
                            TmphTypePool<TmphUpdateWaiter>.Push(this);
                            table.appendValue = returnValue.Value;
                            TmphInterlocked.CompareSetSleep0(ref table.cacheLock);
                            try
                            {
                                value = table.cache.Update(value, key, memberMap);
                            }
                            catch (Exception error)
                            {
                                value = null;
                                TmphLog.Default.Add(error, null, false);
                                table.Dispose();
                            }
                            finally
                            {
                                table.cacheLock = 0;
                            }
                            return value;
                        }
                        this.table = null;
                        this.value = null;
                        this.key = default(TKeyType);
                        this.memberMap = null;
                        TmphTypePool<TmphUpdateWaiter>.Push(this);
                        return null;
                    }
                }

                /// <summary>
                ///     取消当前调用
                /// </summary>
                public void Cancel()
                {
                    if (memoryPool != null && Stream.Data != TmphBuffer.Byte)
                    {
                        memoryPool.Push(ref TmphBuffer);
                        memoryPool = null;
                    }
                    table = null;
                    value = null;
                    key = default(TKeyType);
                    memberMap = null;
                    TmphTypePool<TmphUpdateWaiter>.Push(this);
                }

                /// <summary>
                ///     获取添加数据
                /// </summary>
                /// <param name="table"></param>
                /// <param name="value"></param>
                /// <param name="key"></param>
                /// <param name="memberMap"></param>
                /// <returns></returns>
                public static TmphUpdateWaiter Get
                    (TmphRemoteTable<TValueType, TModelType, TKeyType> table, TValueType value, TKeyType key,
                        TmphMemberMap memberMap)
                {
                    var updater = TmphTypePool<TmphUpdateWaiter>.Pop();
                    if (updater == null)
                    {
                        try
                        {
                            updater = new TmphUpdateWaiter(table.memoryPool);
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                            return null;
                        }
                    }
                    updater.table = table;
                    updater.value = value;
                    updater.key = key;
                    updater.memberMap = memberMap;
                    updater.Stream.Clear();
                    return updater;
                }
            }

            /// <summary>
            ///     更新数据
            /// </summary>
            private sealed unsafe class TmphUpdateCallbacker : TmphCallbacker
            {
                /// <summary>
                ///     添加数据回调委托
                /// </summary>
                private Action<TValueType> callback;

                /// <summary>
                ///     操作回调是否使用任务模式
                /// </summary>
                private bool isCallbackTask;

                /// <summary>
                ///     更新成员位图
                /// </summary>
                private TmphMemberMap memberMap;

                /// <summary>
                ///     更新数据
                /// </summary>
                private TValueType value;

                /// <summary>
                ///     添加数据
                /// </summary>
                private TmphUpdateCallbacker(TmphUnmanagedPool memoryPool)
                    : base(memoryPool)
                {
                    Callback = onReturn;
                }

                /// <summary>
                ///     更新数据回调委托
                /// </summary>
                /// <param name="returnValue"></param>
                private void onReturn(TmphAsynchronousMethod.TmphReturnValue<int> returnValue)
                {
                    if (memoryPool != null && Stream.Data != TmphBuffer.Byte)
                    {
                        memoryPool.Push(ref TmphBuffer);
                        memoryPool = null;
                    }
                    var callback = this.callback;
                    var isCallbackTask = this.isCallbackTask;
                    this.callback = null;
                    if (returnValue.IsReturn)
                    {
                        var table = this.table;
                        this.table = null;
                        if (returnValue.Value == 0)
                        {
                            value = null;
                            key = default(TKeyType);
                            memberMap = null;
                            TmphTypePool<TmphUpdateCallbacker>.Push(this);
                            table.Dispose();
                            if (callback != null)
                            {
                                if (isCallbackTask) TmphTask.Tiny.Add(callback, null);
                                else callback(null);
                            }
                        }
                        else
                        {
                            var value = this.value;
                            var key = this.key;
                            var memberMap = this.memberMap;
                            this.value = null;
                            this.key = default(TKeyType);
                            this.memberMap = null;
                            TmphTypePool<TmphUpdateCallbacker>.Push(this);
                            table.appendValue = returnValue.Value;
                            TmphInterlocked.CompareSetSleep0(ref table.cacheLock);
                            try
                            {
                                value = table.cache.Update(value, key, memberMap);
                            }
                            catch (Exception error)
                            {
                                value = null;
                                TmphLog.Default.Add(error, null, false);
                                table.Dispose();
                            }
                            finally
                            {
                                table.cacheLock = 0;
                            }
                            if (callback != null)
                            {
                                if (isCallbackTask) TmphTask.Tiny.Add(callback, value);
                                else callback(value);
                            }
                        }
                    }
                    else
                    {
                        table = null;
                        value = null;
                        key = default(TKeyType);
                        memberMap = null;
                        TmphTypePool<TmphUpdateCallbacker>.Push(this);
                        if (callback != null)
                        {
                            if (isCallbackTask) TmphTask.Tiny.Add(callback, null);
                            else callback(null);
                        }
                    }
                }

                /// <summary>
                ///     取消当前调用
                /// </summary>
                public void Cancel()
                {
                    if (memoryPool != null && Stream.Data != TmphBuffer.Byte)
                    {
                        memoryPool.Push(ref TmphBuffer);
                        memoryPool = null;
                    }
                    var callback = this.callback;
                    var isCallbackTask = this.isCallbackTask;
                    table = null;
                    value = null;
                    key = default(TKeyType);
                    memberMap = null;
                    this.callback = null;
                    TmphTypePool<TmphUpdateCallbacker>.Push(this);
                    if (callback != null)
                    {
                        if (isCallbackTask) TmphTask.Tiny.Add(callback, null);
                        else callback(null);
                    }
                }

                /// <summary>
                ///     获取添加数据
                /// </summary>
                /// <param name="table"></param>
                /// <param name="value"></param>
                /// <param name="key"></param>
                /// <param name="memberMap"></param>
                /// <param name="onUpdated"></param>
                /// <param name="isCallbackTask"></param>
                /// <returns></returns>
                public static TmphUpdateCallbacker Get
                    (TmphRemoteTable<TValueType, TModelType, TKeyType> table, TValueType value, TKeyType key,
                        TmphMemberMap memberMap, Action<TValueType> onUpdated, bool isCallbackTask)
                {
                    var updater = TmphTypePool<TmphUpdateCallbacker>.Pop();
                    if (updater == null)
                    {
                        try
                        {
                            updater = new TmphUpdateCallbacker(table.memoryPool);
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                            return null;
                        }
                    }
                    updater.table = table;
                    updater.value = value;
                    updater.key = key;
                    updater.memberMap = memberMap;
                    updater.callback = onUpdated;
                    updater.isCallbackTask = isCallbackTask;
                    updater.Stream.Clear();
                    return updater;
                }
            }

            /// <summary>
            ///     删除数据
            /// </summary>
            private sealed unsafe class TmphDeleteWaiter : TmphWaiter
            {
                /// <summary>
                ///     删除数据
                /// </summary>
                private TmphDeleteWaiter()
                    : base(TmphUnmanagedPool.TinyBuffers)
                {
                }

                /// <summary>
                ///     删除数据
                /// </summary>
                public TValueType Value
                {
                    get
                    {
                        waitHandle.WaitOne();
                        if (returnValue.IsReturn)
                        {
                            var table = this.table;
                            this.table = null;
                            if (returnValue.Value == 0)
                            {
                                this.key = default(TKeyType);
                                TmphTypePool<TmphDeleteWaiter>.Push(this);
                                table.Dispose();
                                return null;
                            }
                            var key = this.key;
                            this.key = default(TKeyType);
                            TmphTypePool<TmphDeleteWaiter>.Push(this);
                            TValueType value = null;
                            int logSize;
                            table.appendValue = returnValue.Value;
                            TmphInterlocked.CompareSetSleep0(ref table.cacheLock);
                            try
                            {
                                value = table.cache.Delete(key, out logSize);
                                table.dataSize -= logSize;
                            }
                            catch (Exception error)
                            {
                                value = null;
                                TmphLog.Default.Add(error, null, false);
                                table.Dispose();
                            }
                            finally
                            {
                                table.cacheLock = 0;
                            }
                            return value;
                        }
                        this.table = null;
                        this.key = default(TKeyType);
                        TmphTypePool<TmphDeleteWaiter>.Push(this);
                        return null;
                    }
                }

                /// <summary>
                ///     取消当前调用
                /// </summary>
                public void Cancel()
                {
                    if (memoryPool != null && Stream.Data != TmphBuffer.Byte)
                    {
                        memoryPool.Push(ref TmphBuffer);
                        memoryPool = null;
                    }
                    table = null;
                    key = default(TKeyType);
                    TmphTypePool<TmphDeleteWaiter>.Push(this);
                }

                /// <summary>
                ///     获取添加数据
                /// </summary>
                /// <param name="table"></param>
                /// <param name="key"></param>
                /// <returns></returns>
                public static TmphDeleteWaiter Get
                    (TmphRemoteTable<TValueType, TModelType, TKeyType> table, TKeyType key)
                {
                    var deleter = TmphTypePool<TmphDeleteWaiter>.Pop();
                    if (deleter == null)
                    {
                        try
                        {
                            deleter = new TmphDeleteWaiter();
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                            return null;
                        }
                    }
                    deleter.table = table;
                    deleter.key = key;
                    deleter.Stream.Clear();
                    return deleter;
                }
            }

            /// <summary>
            ///     删除数据
            /// </summary>
            private sealed unsafe class TmphDeleteCallbacker : TmphCallbacker
            {
                /// <summary>
                ///     添加数据回调委托
                /// </summary>
                private Action<TValueType> callback;

                /// <summary>
                ///     操作回调是否使用任务模式
                /// </summary>
                private bool isCallbackTask;

                /// <summary>
                ///     删除数据
                /// </summary>
                private TmphDeleteCallbacker()
                    : base(TmphUnmanagedPool.TinyBuffers)
                {
                    Callback = onReturn;
                }

                /// <summary>
                ///     删除数据回调委托
                /// </summary>
                /// <param name="returnValue"></param>
                private void onReturn(TmphAsynchronousMethod.TmphReturnValue<int> returnValue)
                {
                    if (memoryPool != null && Stream.Data != TmphBuffer.Byte)
                    {
                        memoryPool.Push(ref TmphBuffer);
                        memoryPool = null;
                    }
                    var callback = this.callback;
                    var isCallbackTask = this.isCallbackTask;
                    this.callback = null;
                    if (returnValue.IsReturn)
                    {
                        var table = this.table;
                        this.table = null;
                        if (returnValue.Value == 0)
                        {
                            key = default(TKeyType);
                            TmphTypePool<TmphDeleteCallbacker>.Push(this);
                            table.Dispose();
                            if (callback != null)
                            {
                                if (isCallbackTask) TmphTask.Tiny.Add(callback, null);
                                else callback(null);
                            }
                        }
                        else
                        {
                            var key = this.key;
                            this.key = default(TKeyType);
                            TmphTypePool<TmphDeleteCallbacker>.Push(this);
                            TValueType value = null;
                            int logSize;
                            table.appendValue = returnValue.Value;
                            TmphInterlocked.CompareSetSleep0(ref table.cacheLock);
                            try
                            {
                                value = table.cache.Delete(key, out logSize);
                                table.dataSize -= logSize;
                            }
                            catch (Exception error)
                            {
                                value = null;
                                TmphLog.Default.Add(error, null, false);
                                table.Dispose();
                            }
                            finally
                            {
                                table.cacheLock = 0;
                            }
                            if (callback != null)
                            {
                                if (isCallbackTask) TmphTask.Tiny.Add(callback, value);
                                else callback(value);
                            }
                        }
                    }
                    else
                    {
                        table = null;
                        key = default(TKeyType);
                        TmphTypePool<TmphDeleteCallbacker>.Push(this);
                        if (callback != null)
                        {
                            if (isCallbackTask) TmphTask.Tiny.Add(callback, null);
                            else callback(null);
                        }
                    }
                }

                /// <summary>
                ///     取消当前调用
                /// </summary>
                public void Cancel()
                {
                    if (memoryPool != null && Stream.Data != TmphBuffer.Byte)
                    {
                        memoryPool.Push(ref TmphBuffer);
                        memoryPool = null;
                    }
                    var callback = this.callback;
                    var isCallbackTask = this.isCallbackTask;
                    table = null;
                    key = default(TKeyType);
                    this.callback = null;
                    TmphTypePool<TmphDeleteCallbacker>.Push(this);
                    if (callback != null)
                    {
                        if (isCallbackTask) TmphTask.Tiny.Add(callback, null);
                        else callback(null);
                    }
                }

                /// <summary>
                ///     获取添加数据
                /// </summary>
                /// <param name="table"></param>
                /// <param name="key"></param>
                /// <param name="onDeleted"></param>
                /// <param name="isCallbackTask"></param>
                /// <returns></returns>
                public static TmphDeleteCallbacker Get
                    (TmphRemoteTable<TValueType, TModelType, TKeyType> table, TKeyType key, Action<TValueType> onDeleted,
                        bool isCallbackTask)
                {
                    var deleter = TmphTypePool<TmphDeleteCallbacker>.Pop();
                    if (deleter == null)
                    {
                        try
                        {
                            deleter = new TmphDeleteCallbacker();
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                            return null;
                        }
                    }
                    deleter.table = table;
                    deleter.key = key;
                    deleter.callback = onDeleted;
                    deleter.isCallbackTask = isCallbackTask;
                    deleter.Stream.Clear();
                    return deleter;
                }
            }
        }
    }

    /// <summary>
    ///     内存数据库表格操作工具
    /// </summary>
    /// <typeparam name="TValueType">表格类型</typeparam>
    /// <typeparam name="TModelType">模型类型</typeparam>
    public class TmphMemoryDatabaseTable<TValueType, TModelType> :
        TmphMemoryDatabaseTable.TmphLocalTable<TValueType, TModelType, int>
        where TValueType : class, TModelType
        where TModelType : class
    {
        /// <summary>
        ///     自增数据加载基本缓存接口
        /// </summary>
        private new readonly TmphILoadIdentityCache<TValueType, TModelType> cache;

        /// <summary>
        ///     自增字段成员索引
        /// </summary>
        private readonly int identityMemberIndex;

        /// <summary>
        ///     内存数据库表格操作工具
        /// </summary>
        /// <param name="cache">自增数据加载基本缓存接口</param>
        /// <param name="serializeType">数据序类型</param>
        /// <param name="name">表格名称</param>
        /// <param name="minRefreshSize">数据库日志文件最小刷新尺寸(单位:KB)</param>
        /// <param name="bufferSize">缓冲区字节大小</param>
        public TmphMemoryDatabaseTable(TmphILoadIdentityCache<TValueType, TModelType> cache
            , TmphMemoryDatabaseTable.TmphSerializeType serializeType = TmphMemoryDatabaseTable.TmphSerializeType.Index
            , string name = null, int minRefreshSize = 0, int bufferSize = 0)
            : base(
                cache, serializeType, name, minRefreshSize, bufferSize, TmphMemoryDatabaseModel<TModelType>.GetIdentity,
                TmphMemoryDatabaseModel<TModelType>.SetIdentity)
        {
            this.cache = cache;
            identityMemberIndex = TmphMemoryDatabaseModel<TModelType>.Identity.MemberMapIndex;
        }

        /// <summary>
        ///     添加数据
        /// </summary>
        /// <param name="value"></param>
        /// <param name="isCopy">是否复制缓存数据</param>
        /// <returns></returns>
        public override unsafe TValueType Insert(TValueType value, bool isCopy = true)
        {
            if (value != null && GetPrimaryKey(value) == 0)
            {
                var isError = 0;
                TmphInterlocked.CompareSetSleep1(ref physicalLock);
                var stream = Interlocked.Exchange(ref this.stream, null);
                try
                {
                    var identity = cache.NextIdentity();
                    SetPrimaryKey(value, identity);
                    var TmphBuffer = Physical.LocalBuffer();
                    if (TmphBuffer == null)
                    {
                        value = null;
                        isError = 1;
                    }
                    else
                    {
                        fixed (byte* bufferFixed = TmphBuffer)
                        {
                            if (stream == null)
                                stream = new TmphUnmanagedStream(bufferFixed + Physical.BufferIndex,
                                    TmphBuffer.Length - Physical.BufferIndex);
                            else stream.Reset(bufferFixed + Physical.BufferIndex, TmphBuffer.Length - Physical.BufferIndex);
                            stream.Unsafer.AddLength(sizeof(int) * 2);
                            Serializer.Insert(value, stream);
                            var data = stream.Data;
                            *(int*)(data + sizeof(int)) = (byte)TmphMemoryDatabaseTable.TmphLogType.Insert;
                            *(int*)data = stream.Length;
                            isError = 1;
                            if (data == bufferFixed + Physical.BufferIndex)
                            {
                                Physical.BufferIndex += stream.Length;
                                if ((value = cache.Insert(value, identity, stream.Length, isCopy)) != null)
                                {
                                    dataSize += stream.Length;
                                    isError = 0;
                                }
                            }
                            else if (Physical.Append(stream.Data, stream.Length) == 0) value = null;
                            else if ((value = cache.Insert(value, identity, stream.Length, isCopy)) != null)
                            {
                                dataSize += stream.Length;
                                isError = 0;
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    value = null;
                    TmphLog.Error.Add(error, null, false);
                }
                finally
                {
                    if (isDisposed == 0)
                    {
                        if (stream != null) Interlocked.Exchange(ref this.stream, stream);
                    }
                    else CLB.TmphPub.Dispose(ref stream);
                    physicalLock = 0;
                }
                if (value != null)
                {
                    Physical.WaitBuffer();
                    return value;
                }
                if (isError != 0) Dispose();
            }
            return null;
        }

        /// <summary>
        ///     获取更新成员位图
        /// </summary>
        /// <param name="memberMap"></param>
        /// <returns></returns>
        protected override TmphMemberMap GetUpdateMemberMap(TmphMemberMap memberMap)
        {
            memberMap.ClearMember(identityMemberIndex);
            var updateMemberMap = memberMap.Copy();
            updateMemberMap.SetMember(identityMemberIndex);
            return updateMemberMap;
        }

        /// <summary>
        ///     内存数据库表格操作工具(远程)
        /// </summary>
        public sealed class TmphRemote : TmphMemoryDatabaseTable.TmphRemoteTable<TValueType, TModelType, int>
        {
            /// <summary>
            ///     自增数据加载基本缓存接口
            /// </summary>
            private new readonly TmphILoadIdentityCache<TValueType, TModelType> cache;

            /// <summary>
            ///     自增字段成员索引
            /// </summary>
            private readonly int identityMemberIndex;

            /// <summary>
            ///     内存数据库表格操作工具
            /// </summary>
            /// <param name="TmphClient"></param>
            /// <param name="cache">自增数据加载基本缓存接口</param>
            /// <param name="serializeType">数据序类型</param>
            /// <param name="name">表格名称</param>
            /// <param name="minRefreshSize">数据库日志文件最小刷新尺寸(单位:KB)</param>
            /// <param name="bufferSize">缓冲区字节大小</param>
            /// <param name="isWaitClose">是否等待数据库关闭</param>
            /// <param name="isCloseCient">是否自动关系内存数据库客户端</param>
            public TmphRemote(TmphMemoryDatabasePhysical TmphClient, TmphILoadIdentityCache<TValueType, TModelType> cache
                , TmphMemoryDatabaseTable.TmphSerializeType serializeType = TmphMemoryDatabaseTable.TmphSerializeType.Index
                , string name = null, int minRefreshSize = 0, int bufferSize = 0, TmphUnmanagedPool memoryPool = null,
                bool isWaitClose = false, bool isCloseCient = true)
                : base(
                    TmphClient, cache, serializeType, name, minRefreshSize, bufferSize,
                    TmphMemoryDatabaseModel<TModelType>.GetIdentity, TmphMemoryDatabaseModel<TModelType>.SetIdentity,
                    memoryPool, isWaitClose, isCloseCient)
            {
                this.cache = cache;
                identityMemberIndex = TmphMemoryDatabaseModel<TModelType>.Identity.MemberMapIndex;
            }

            /// <summary>
            ///     添加数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="onInserted"></param>
            /// <param name="isCallbackTask">添加数据回调是否使用任务模式</param>
            /// <param name="isCopy">是否复制缓存数据</param>
            /// <returns></returns>
            public override void Insert(TValueType value, Action<TValueType> onInserted, bool isCallbackTask = true,
                bool isCopy = true)
            {
                if (value != null && GetPrimaryKey(value) == 0)
                {
                    TmphInterlocked.CompareSetSleep0(ref cacheLock);
                    var identity = cache.NextIdentity();
                    cacheLock = 0;
                    SetPrimaryKey(value, identity);
                    if (insert(value, identity, onInserted, isCallbackTask, isCopy)) return;
                }
                if (onInserted != null)
                {
                    if (isCallbackTask) TmphTask.Tiny.Add(onInserted, null);
                    else onInserted(null);
                }
            }

            /// <summary>
            ///     添加数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="isCopy">是否复制缓存数据</param>
            /// <returns></returns>
            public override TValueType Insert(TValueType value, bool isCopy = true)
            {
                if (value != null && GetPrimaryKey(value) == 0)
                {
                    TmphInterlocked.CompareSetSleep0(ref cacheLock);
                    var identity = cache.NextIdentity();
                    cacheLock = 0;
                    SetPrimaryKey(value, identity);
                    return insert(value, identity, isCopy);
                }
                return null;
            }

            /// <summary>
            ///     获取更新成员位图
            /// </summary>
            /// <param name="memberMap"></param>
            /// <returns></returns>
            protected override TmphMemberMap GetUpdateMemberMap(TmphMemberMap memberMap)
            {
                memberMap.ClearMember(identityMemberIndex);
                var updateMemberMap = memberMap.Copy();
                updateMemberMap.SetMember(identityMemberIndex);
                return updateMemberMap;
            }
        }
    }

    /// <summary>
    ///     内存数据库表格操作工具
    /// </summary>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    public sealed class TmphMemoryDatabaseModelTable<TModelType> : TmphMemoryDatabaseTable<TModelType, TModelType>
        where TModelType : class
    {
        /// <summary>
        ///     内存数据库表格操作工具
        /// </summary>
        /// <param name="cache">自增数据加载基本缓存接口</param>
        /// <param name="serializeType">数据序类型</param>
        /// <param name="name">表格名称</param>
        /// <param name="minRefreshSize">数据库日志文件最小刷新尺寸(单位:KB)</param>
        /// <param name="bufferSize">缓冲区字节大小</param>
        public TmphMemoryDatabaseModelTable(TmphILoadIdentityCache<TModelType, TModelType> cache
            , TmphMemoryDatabaseTable.TmphSerializeType serializeType = TmphMemoryDatabaseTable.TmphSerializeType.Index
            , string name = null, int minRefreshSize = 0, int bufferSize = 0)
            : base(cache, serializeType, name, minRefreshSize, bufferSize)
        {
        }
    }

    /// <summary>
    ///     内存数据库表格操作工具
    /// </summary>
    /// <typeparam name="TValueType">表格类型</typeparam>
    /// <typeparam name="TModelType">模型类型</typeparam>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    public class TmphMemoryDatabaseTable<TValueType, TModelType, TKeyType> :
        TmphMemoryDatabaseTable.TmphLocalTable<TValueType, TModelType, TKeyType>
        where TValueType : class, TModelType
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        ///     内存数据库表格操作工具
        /// </summary>
        /// <param name="cache">自增数据加载基本缓存接口</param>
        /// <param name="serializeType">数据序类型</param>
        /// <param name="name">表格名称</param>
        /// <param name="minRefreshSize">数据库日志文件最小刷新尺寸(单位:KB)</param>
        /// <param name="bufferSize">缓冲区字节大小</param>
        public TmphMemoryDatabaseTable(TmphILoadCache<TValueType, TModelType, TKeyType> cache
            , TmphMemoryDatabaseTable.TmphSerializeType serializeType = TmphMemoryDatabaseTable.TmphSerializeType.Index
            , string name = null, int minRefreshSize = 0, int bufferSize = 0)
            : base(cache, serializeType, name, minRefreshSize, bufferSize
                ,
                TmphDatabaseModel<TModelType>.GetPrimaryKeyGetter<TKeyType>("GetMemoryDatabasePrimaryKey",
                    TmphMemoryDatabaseModel<TModelType>.PrimaryKeyFields)
                ,
                TmphDatabaseModel<TModelType>.GetPrimaryKeySetter<TKeyType>("SetMemoryDatabasePrimaryKey",
                    TmphMemoryDatabaseModel<TModelType>.PrimaryKeyFields))
        {
        }

        /// <summary>
        ///     添加数据
        /// </summary>
        /// <param name="value"></param>
        /// <param name="isCopy">是否复制缓存数据</param>
        /// <returns></returns>
        public override unsafe TValueType Insert(TValueType value, bool isCopy = true)
        {
            if (value != null)
            {
                TmphUnmanagedStream stream = null;
                var isError = 0;
                var key = GetPrimaryKey(value);
                TmphInterlocked.CompareSetSleep1(ref physicalLock);
                try
                {
                    if (cache.ContainsKey(key)) value = null;
                    else
                    {
                        stream = Interlocked.Exchange(ref this.stream, null);
                        var TmphBuffer = Physical.LocalBuffer();
                        if (TmphBuffer == null)
                        {
                            value = null;
                            isError = 1;
                        }
                        else
                        {
                            fixed (byte* bufferFixed = TmphBuffer)
                            {
                                if (stream == null)
                                    stream = new TmphUnmanagedStream(bufferFixed + Physical.BufferIndex,
                                        TmphBuffer.Length - Physical.BufferIndex);
                                else
                                    stream.Reset(bufferFixed + Physical.BufferIndex,
                                        TmphBuffer.Length - Physical.BufferIndex);
                                stream.Unsafer.AddLength(sizeof(int) * 2);
                                Serializer.Insert(value, stream);
                                var data = stream.Data;
                                *(int*)(data + sizeof(int)) = (byte)TmphMemoryDatabaseTable.TmphLogType.Insert;
                                *(int*)data = stream.Length;
                                isError = 1;
                                if (data == bufferFixed + Physical.BufferIndex)
                                {
                                    Physical.BufferIndex += stream.Length;
                                    if ((value = cache.Insert(value, key, stream.Length, isCopy)) != null)
                                    {
                                        dataSize += stream.Length;
                                        isError = 0;
                                    }
                                }
                                else if (Physical.Append(stream.Data, stream.Length) == 0) value = null;
                                else if ((value = cache.Insert(value, key, stream.Length, isCopy)) != null)
                                {
                                    dataSize += stream.Length;
                                    isError = 0;
                                }
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    value = null;
                    TmphLog.Error.Add(error, null, false);
                }
                finally
                {
                    if (isDisposed == 0)
                    {
                        if (stream != null) Interlocked.Exchange(ref this.stream, stream);
                    }
                    else CLB.TmphPub.Dispose(ref stream);
                    physicalLock = 0;
                }
                if (value != null)
                {
                    Physical.WaitBuffer();
                    return value;
                }
                if (isError != 0) Dispose();
            }
            return null;
        }

        /// <summary>
        ///     获取更新成员位图
        /// </summary>
        /// <param name="memberMap"></param>
        /// <returns></returns>
        protected override TmphMemberMap GetUpdateMemberMap(TmphMemberMap memberMap)
        {
            var updateMemberMap = memberMap.Copy();
            foreach (var primaryKey in TmphMemoryDatabaseModel<TModelType>.PrimaryKeys)
            {
                memberMap.ClearMember(primaryKey.MemberMapIndex);
                updateMemberMap.SetMember(primaryKey.MemberMapIndex);
            }
            return updateMemberMap;
        }

        /// <summary>
        ///     内存数据库表格操作工具(远程)
        /// </summary>
        public sealed class TmphRemote : TmphMemoryDatabaseTable.TmphRemoteTable<TValueType, TModelType, TKeyType>
        {
            /// <summary>
            ///     内存数据库表格操作工具
            /// </summary>
            /// <param name="TmphClient"></param>
            /// <param name="cache">自增数据加载基本缓存接口</param>
            /// <param name="serializeType">数据序类型</param>
            /// <param name="name">表格名称</param>
            /// <param name="minRefreshSize">数据库日志文件最小刷新尺寸(单位:KB)</param>
            /// <param name="bufferSize">缓冲区字节大小</param>
            /// <param name="isWaitClose">是否等待数据库关闭</param>
            /// <param name="isCloseCient">是否自动关系内存数据库客户端</param>
            public TmphRemote(TmphMemoryDatabasePhysical TmphClient, TmphILoadCache<TValueType, TModelType, TKeyType> cache
                , TmphMemoryDatabaseTable.TmphSerializeType serializeType = TmphMemoryDatabaseTable.TmphSerializeType.Index
                , string name = null, int minRefreshSize = 0, int bufferSize = 0, TmphUnmanagedPool memoryPool = null,
                bool isWaitClose = false, bool isCloseCient = true)
                : base(TmphClient, cache, serializeType, name, minRefreshSize, bufferSize
                    ,
                    TmphDatabaseModel<TModelType>.GetPrimaryKeyGetter<TKeyType>("GetMemoryDatabasePrimaryKey",
                        TmphMemoryDatabaseModel<TModelType>.PrimaryKeyFields)
                    ,
                    TmphDatabaseModel<TModelType>.GetPrimaryKeySetter<TKeyType>("SetMemoryDatabasePrimaryKey",
                        TmphMemoryDatabaseModel<TModelType>.PrimaryKeyFields)
                    , memoryPool, isWaitClose, isCloseCient)
            {
            }

            /// <summary>
            ///     添加数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="onInserted"></param>
            /// <param name="isCallbackTask">添加数据回调是否使用任务模式</param>
            /// <param name="isCopy">是否复制缓存数据</param>
            /// <returns></returns>
            public override void Insert(TValueType value, Action<TValueType> onInserted, bool isCallbackTask = true,
                bool isCopy = true)
            {
                if (value != null)
                {
                    var key = GetPrimaryKey(value);
                    var isKey = true;
                    TmphInterlocked.CompareSetSleep0(ref cacheLock);
                    try
                    {
                        isKey = cache.ContainsKey(key);
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                    finally
                    {
                        cacheLock = 0;
                    }
                    if (!isKey && insert(value, key, onInserted, isCallbackTask, isCopy)) return;
                }
                if (onInserted != null)
                {
                    if (isCallbackTask) TmphTask.Tiny.Add(onInserted, null);
                    else onInserted(null);
                }
            }

            /// <summary>
            ///     添加数据
            /// </summary>
            /// <param name="value"></param>
            /// <param name="isCopy">是否复制缓存数据</param>
            /// <returns></returns>
            public override TValueType Insert(TValueType value, bool isCopy = true)
            {
                if (value != null)
                {
                    var key = GetPrimaryKey(value);
                    var isKey = true;
                    TmphInterlocked.CompareSetSleep0(ref cacheLock);
                    try
                    {
                        isKey = cache.ContainsKey(key);
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                    finally
                    {
                        cacheLock = 0;
                    }
                    if (!isKey) return insert(value, key, isCopy);
                }
                return null;
            }

            /// <summary>
            ///     获取更新成员位图
            /// </summary>
            /// <param name="memberMap"></param>
            /// <returns></returns>
            protected override TmphMemberMap GetUpdateMemberMap(TmphMemberMap memberMap)
            {
                var updateMemberMap = memberMap.Copy();
                foreach (var primaryKey in TmphMemoryDatabaseModel<TModelType>.PrimaryKeys)
                {
                    memberMap.ClearMember(primaryKey.MemberMapIndex);
                    updateMemberMap.SetMember(primaryKey.MemberMapIndex);
                }
                return updateMemberMap;
            }
        }
    }

    /// <summary>
    ///     内存数据库表格操作工具
    /// </summary>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    public sealed class TmphMemoryDatabaseModelTable<TModelType, TKeyType> :
        TmphMemoryDatabaseTable<TModelType, TModelType, TKeyType>
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        ///     内存数据库表格操作工具
        /// </summary>
        /// <param name="cache">自增数据加载基本缓存接口</param>
        /// <param name="serializeType">数据序类型</param>
        /// <param name="name">表格名称</param>
        /// <param name="minRefreshSize">数据库日志文件最小刷新尺寸(单位:KB)</param>
        /// <param name="bufferSize">缓冲区字节大小</param>
        public TmphMemoryDatabaseModelTable(TmphILoadCache<TModelType, TModelType, TKeyType> cache
            , TmphMemoryDatabaseTable.TmphSerializeType serializeType = TmphMemoryDatabaseTable.TmphSerializeType.Index
            , string name = null, int minRefreshSize = 0, int bufferSize = 0)
            : base(cache, serializeType, name, minRefreshSize, bufferSize)
        {
        }
    }
}