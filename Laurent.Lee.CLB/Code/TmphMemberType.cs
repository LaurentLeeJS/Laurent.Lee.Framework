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

using Laurent.Lee.CLB.Reflection;
using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Laurent.Lee.CLB.Code
{
    /// <summary>
    ///     成员类型
    /// </summary>
    public sealed partial class TmphMemberType
    {
        /// <summary>
        ///     空类型
        /// </summary>
        internal static readonly TmphMemberType Null = new TmphMemberType(null);

        /// <summary>
        ///     成员类型隐式转换集合
        /// </summary>
        private static readonly Dictionary<Type, TmphMemberType> Types = TmphDictionary.CreateOnly<Type, TmphMemberType>();

        /// <summary>
        ///     隐式转换集合转换锁
        /// </summary>
        private static int _typeLock;

        /// <summary>
        ///     是否引用类型
        /// </summary>
        private readonly bool? _isNull;

        /// <summary>
        ///     自定义类型名称
        /// </summary>
        private readonly string _name;

        /// <summary>
        ///     可枚举泛型参数类型
        /// </summary>
        private TmphMemberType _enumerableArgumentType;

        /// <summary>
        ///     可枚举泛型类型
        /// </summary>
        private TmphMemberType _enumerableType;

        /// <summary>
        ///     类型全名
        /// </summary>
        private string _fullName;

        /// <summary>
        ///     泛型参数集合
        /// </summary>
        private TmphMemberType[] _genericParameters;

        ///// <summary>
        ///// 结构体非可空类型
        ///// </summary>
        //private string structType;
        ///// <summary>
        ///// 结构体非可空类型
        ///// </summary>
        //public string StructType
        //{
        //    get
        //    {
        //        if (structType == null)
        //        {
        //            Type type = Type.nullableType();
        //            structType = type == null ? fullName : type.fullName();
        //        }
        //        return structType;
        //    }
        //}
        /// <summary>
        ///     是否拥有静态转换函数
        /// </summary>
        private bool? _isTryParse;

        /// <summary>
        ///     可控类型的值类型
        /// </summary>
        private TmphMemberType _nullType;

        /// <summary>
        ///     SQL类型
        /// </summary>
        private Type _sqlType;

        /// <summary>
        ///     类型名称
        /// </summary>
        private string _typeName;

        /// <summary>
        ///     类型名称
        /// </summary>
        private string _typeOnlyName;

        /// <summary>
        ///     键值对键类型
        /// </summary>
        internal TmphMemberType keyValueKeyType;

        /// <summary>
        ///     键值对键类型
        /// </summary>
        internal TmphMemberType keyValueValueType;

        /// <summary>
        ///     键值对键类型
        /// </summary>
        internal TmphMemberType pairKeyType;

        /// <summary>
        ///     键值对值类型
        /// </summary>
        internal TmphMemberType pairValueType;

        /// <summary>
        ///     成员类型
        /// </summary>
        /// <param name="name">类型名称</param>
        /// <param name="isNull">是否引用类型</param>
        public TmphMemberType(string name, bool isNull)
        {
            _name = name;
            _isNull = isNull;
        }

        /// <summary>
        ///     成员类型
        /// </summary>
        /// <param name="type">类型</param>
        private TmphMemberType(Type type)
        {
            Type = type;
        }

        /// <summary>
        ///     成员类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="sqlType">SQL类型</param>
        internal TmphMemberType(Type type, Type sqlType)
            : this(type)
        {
            _sqlType = sqlType;
        }

        /// <summary>
        ///     类型
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        ///     类型名称
        /// </summary>
        public string TypeName
        {
            get
            {
                if (_typeName == null) _typeName = _name ?? (Type != null ? Type.name() : null);
                return _typeName;
            }
        }

        /// <summary>
        ///     类型名称
        /// </summary>
        public string TypeOnlyName
        {
            get
            {
                if (_typeOnlyName == null) _typeOnlyName = _name ?? (Type != null ? Type.onlyName() : null);
                return _typeOnlyName;
            }
        }

        /// <summary>
        ///     类型全名
        /// </summary>
        public string FullName
        {
            get
            {
                if (_fullName == null) _fullName = Type != null ? Type.fullName() : TypeName;
                return _fullName;
            }
        }

        /// <summary>
        ///     是否引用类型
        /// </summary>
        public bool IsNull
        {
            get { return _isNull == null ? Type == null || Type.isNull() : (bool)_isNull; }
        }

        /// <summary>
        ///     是否object
        /// </summary>
        internal bool IsObject
        {
            get { return Type == typeof(object); }
        }

        /// <summary>
        ///     是否字符串
        /// </summary>
        public bool IsString
        {
            get { return Type == typeof(string); }
        }

        /// <summary>
        ///     是否字符串
        /// </summary>
        public bool IsSubString
        {
            get { return Type == typeof(TmphSubString); }
        }

        /// <summary>
        ///     是否字符类型(包括可空类型)
        /// </summary>
        public bool IsChar
        {
            get { return Type == typeof(char) || Type == typeof(char?); }
        }

        /// <summary>
        ///     是否逻辑类型(包括可空类型)
        /// </summary>
        public bool IsBool
        {
            get { return Type == typeof(bool) || Type == typeof(bool?); }
        }

        /// <summary>
        ///     是否时间类型(包括可空类型)
        /// </summary>
        public bool IsDateTime
        {
            get { return Type == typeof(DateTime) || Type == typeof(DateTime?); }
        }

        /// <summary>
        ///     是否数字类型(包括可空类型)
        /// </summary>
        public bool IsDecimal
        {
            get { return Type == typeof(decimal) || Type == typeof(decimal?); }
        }

        /// <summary>
        ///     是否Guid类型(包括可空类型)
        /// </summary>
        public bool IsGuid
        {
            get { return Type == typeof(Guid) || Type == typeof(Guid?); }
        }

        /// <summary>
        ///     是否字节数组
        /// </summary>
        public bool IsByteArray
        {
            get { return Type == typeof(byte[]); }
        }

        /// <summary>
        ///     是否值类型(排除可空类型)
        /// </summary>
        public bool IsStruct
        {
            get { return Type.isStruct() && Type.nullableType() == null; }
        }

        /// <summary>
        ///     是否数组或者接口
        /// </summary>
        public bool IsArrayOrInterface
        {
            get { return Type.IsArray || Type.IsInterface; }
        }

        /// <summary>
        ///     是否字典
        /// </summary>
        public bool IsDictionary
        {
            get { return Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(Dictionary<,>); }
        }

        /// <summary>
        ///     是否流
        /// </summary>
        public bool IsStream
        {
            get { return Type == typeof(Stream); }
        }

        /// <summary>
        ///     数组构造信息
        /// </summary>
        internal ConstructorInfo ArrayConstructor { get; private set; }

        /// <summary>
        ///     列表数组构造信息
        /// </summary>
        internal ConstructorInfo ListConstructor { get; private set; }

        /// <summary>
        ///     集合构造信息
        /// </summary>
        internal ConstructorInfo CollectionConstructor { get; private set; }

        /// <summary>
        ///     可枚举泛型构造信息
        /// </summary>
        internal ConstructorInfo EnumerableConstructor { get; private set; }

        /// <summary>
        ///     枚举基类类型
        /// </summary>
        public TmphMemberType EnumUnderlyingType
        {
            get { return Type.GetEnumUnderlyingType(); }
        }

        /// <summary>
        ///     可枚举泛型类型
        /// </summary>
        public TmphMemberType EnumerableType
        {
            get
            {
                if (_enumerableType == null)
                {
                    if (!IsString)
                    {
                        var value = Type.getGenericInterface(typeof(IEnumerable<>));
                        if (value != null)
                        {
                            if (Type.IsInterface)
                            {
                                var interfaceType = Type.GetGenericTypeDefinition();
                                if (interfaceType == typeof(IEnumerable<>) || interfaceType == typeof(ICollection<>)
                                    || interfaceType == typeof(IList<>))
                                {
                                    _enumerableArgumentType = value.GetGenericArguments()[0];
                                    _enumerableType = value;
                                }
                            }
                            else if (Type.IsArray)
                            {
                                _enumerableArgumentType = value.GetGenericArguments()[0];
                                _enumerableType = value;
                            }
                            else
                            {
                                var enumerableArgumentType = value.GetGenericArguments()[0];
                                var parameters = new Type[1];
                                parameters[0] = enumerableArgumentType.MakeArrayType();
                                ArrayConstructor =
                                    Type.GetConstructor(
                                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                        parameters, null);
                                if (ArrayConstructor != null)
                                {
                                    _enumerableArgumentType = enumerableArgumentType;
                                    _enumerableType = value;
                                }
                                else
                                {
                                    parameters[0] = typeof(IList<>).MakeGenericType(enumerableArgumentType);
                                    ListConstructor =
                                        Type.GetConstructor(
                                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                            parameters, null);
                                    if (ListConstructor != null)
                                    {
                                        _enumerableArgumentType = enumerableArgumentType;
                                        _enumerableType = value;
                                    }
                                    else
                                    {
                                        parameters[0] = typeof(ICollection<>).MakeGenericType(enumerableArgumentType);
                                        CollectionConstructor =
                                            Type.GetConstructor(
                                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                                                null, parameters, null);
                                        if (CollectionConstructor != null)
                                        {
                                            _enumerableArgumentType = enumerableArgumentType;
                                            _enumerableType = value;
                                        }
                                        else
                                        {
                                            parameters[0] =
                                                typeof(IEnumerable<>).MakeGenericType(enumerableArgumentType);
                                            EnumerableConstructor =
                                                Type.GetConstructor(
                                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                                                    null, parameters, null);
                                            if (EnumerableConstructor != null)
                                            {
                                                _enumerableArgumentType = enumerableArgumentType;
                                                _enumerableType = value;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (_enumerableType == null) _enumerableType = Null;
                }
                return _enumerableType.Type != null ? _enumerableType : null;
            }
        }

        /// <summary>
        ///     是否可枚举类型
        /// </summary>
        public bool IsEnumerable
        {
            get { return EnumerableType != null; }
        }

        /// <summary>
        ///     可枚举泛型参数类型
        /// </summary>
        public TmphMemberType EnumerableArgumentType
        {
            get { return EnumerableType != null ? _enumerableArgumentType : null; }
        }

        /// <summary>
        ///     可控类型的值类型
        /// </summary>
        public TmphMemberType NullType
        {
            get
            {
                if (_nullType == null) _nullType = Type.nullableType();
                return _nullType.Type != null ? _nullType : null;
            }
        }

        /// <summary>
        ///     非可控类型为null
        /// </summary>
        public TmphMemberType NotNullType
        {
            get { return NullType != null ? _nullType : this; }
        }

        /// <summary>
        ///     非可控类型为null
        /// </summary>
        public string StructNotNullType
        {
            get
            {
                if (NotNullType.Type.IsEnum) return NotNullType.Type.GetEnumUnderlyingType().fullName();
                return NotNullType.FullName;
            }
        }

        /// <summary>
        ///     是否拥有静态转换函数
        /// </summary>
        internal bool IsTryParse
        {
            get
            {
                if (_isTryParse == null) _isTryParse = (Type.nullableType() ?? Type).getTryParse() != null;
                return (bool)_isTryParse;
            }
        }

        /// <summary>
        ///     键值对键类型
        /// </summary>
        public TmphMemberType PairKeyType
        {
            get
            {
                if (pairKeyType == null)
                {
                    if (Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                    {
                        pairKeyType = Type.GetGenericArguments()[0];
                    }
                    else pairKeyType = Null;
                }
                return pairKeyType.Type != null ? pairKeyType : null;
            }
        }

        /// <summary>
        ///     键值对值类型
        /// </summary>
        public TmphMemberType PairValueType
        {
            get
            {
                if (pairValueType == null)
                {
                    if (Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                    {
                        pairValueType = Type.GetGenericArguments()[1];
                    }
                    else pairValueType = Null;
                }
                return pairValueType.Type != null ? pairValueType : null;
            }
        }

        /// <summary>
        ///     键值对键类型
        /// </summary>
        public TmphMemberType KeyValueKeyType
        {
            get
            {
                if (keyValueKeyType == null)
                {
                    if (Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(TmphKeyValue<,>))
                    {
                        keyValueKeyType = Type.GetGenericArguments()[0];
                    }
                    else keyValueKeyType = Null;
                }
                return keyValueKeyType.Type != null ? keyValueKeyType : null;
            }
        }

        /// <summary>
        ///     键值对键类型
        /// </summary>
        public TmphMemberType KeyValueValueType
        {
            get
            {
                if (keyValueValueType == null)
                {
                    if (Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(TmphKeyValue<,>))
                    {
                        keyValueValueType = Type.GetGenericArguments()[1];
                    }
                    else keyValueValueType = Null;
                }
                return keyValueValueType.Type != null ? keyValueValueType : null;
            }
        }

        /// <summary>
        ///     泛型参数集合
        /// </summary>
        internal TmphMemberType[] GenericParameters
        {
            get
            {
                if (_genericParameters == null)
                {
                    _genericParameters = Type.IsGenericType
                        ? Type.GetGenericArguments().getArray(value => (TmphMemberType)value)
                        : TmphNullValue<TmphMemberType>.Array;
                }
                return _genericParameters;
            }
        }

        /// <summary>
        ///     泛型参数名称
        /// </summary>
        public string GenericParameterNames
        {
            get { return GenericParameters.joinString(',', value => value.FullName); }
        }

        /// <summary>
        ///     隐式转换
        /// </summary>
        /// <param name="value">成员类型</param>
        /// <returns>类型</returns>
        public static implicit operator Type(TmphMemberType value)
        {
            return value != null ? value.Type : null;
        }

        /// <summary>
        ///     隐式转换
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>成员类型</returns>
        public static implicit operator TmphMemberType(Type type)
        {
            if (type == null) return Null;
            TmphMemberType value;
            TmphInterlocked.CompareSetSleep(ref _typeLock);
            try
            {
                if (!Types.TryGetValue(type, out value)) Types.Add(type, value = new TmphMemberType(type));
            }
            finally
            {
                _typeLock = 0;
            }
            return value;
        }
    }
}