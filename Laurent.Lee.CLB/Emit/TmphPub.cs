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
using Laurent.Lee.CLB.Reflection;
using Laurent.Lee.CLB.Sql.Expression;
using Laurent.Lee.CLB.Threading;
using Laurent.Lee.CLB.Web;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Emit;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    ///     公共类型
    /// </summary>
    public static class TmphPub
    {
        /// <summary>
        ///     获取字段委托
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <typeparam name="fieldType"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public delegate fieldType TmphGetField<TValueType, fieldType>(ref TValueType value);

        /// <summary>
        ///     设置字段值委托
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <typeparam name="fieldType"></typeparam>
        /// <param name="value"></param>
        /// <param name="fieldValue"></param>
        public delegate void TmphSetField<TValueType, fieldType>(ref TValueType value, fieldType fieldValue);

        /// <summary>
        ///     LGD
        /// </summary>
        internal const int PuzzleValue = 0x10035113;

        /// <summary>
        ///     int引用参数类型
        /// </summary>
        internal static readonly Type RefIntType = typeof(int).MakeByRefType();

        /// <summary>
        ///     内存字符流写入字符串方法信息
        /// </summary>
        internal static readonly MethodInfo CharStreamWriteNotNullMethod =
            typeof(TmphCharStream).GetMethod("WriteNotNull", BindingFlags.Instance | BindingFlags.NonPublic, null,
                new[] { typeof(string) }, null);

        /// <summary>
        ///     内存字符流写入字符方法信息
        /// </summary>
        internal static readonly MethodInfo CharStreamWriteCharMethod = typeof(TmphCharStream).GetMethod("Write",
            BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(char) }, null);

        /// <summary>
        ///     字符流写入null方法信息
        /// </summary>
        internal static readonly MethodInfo CharStreamWriteNullMethod = typeof(TmphAjax).GetMethod("WriteNull",
            BindingFlags.Public | BindingFlags.Static);

        /// <summary>
        ///     内存流安全写入Int32方法信息
        /// </summary>
        internal static readonly MethodInfo UnmanagedStreamUnsafeWriteIntMethod =
            typeof(TmphUnmanagedStream).GetMethod("UnsafeWrite", BindingFlags.Instance | BindingFlags.NonPublic, null,
                new[] { typeof(int) }, null);

        /// <summary>
        ///     内存流写入Int32方法信息
        /// </summary>
        internal static readonly MethodInfo UnmanagedStreamWriteIntMethod = typeof(TmphUnmanagedStream).GetMethod(
            "Write", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(int) }, null);

        /// <summary>
        ///     判断成员位图是否匹配成员索引
        /// </summary>
        internal static readonly MethodInfo MemberMapIsMemberMethod = TmphMemberMap.IsMemberMethod;

        /// <summary>
        ///     可空类型是否为空判断函数信息集合
        /// </summary>
        private static TmphInterlocked.TmphDictionary<Type, MethodInfo> nullableHasValues =
            new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

        /// <summary>
        ///     可空类型获取数据函数信息集合
        /// </summary>
        private static TmphInterlocked.TmphDictionary<Type, MethodInfo> nullableValues =
            new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

        /// <summary>
        ///     SQL常量转换函数信息集合
        /// </summary>
        private static TmphInterlocked.TmphDictionary<Type, MethodInfo> sqlConverterMethods =
            new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

        /// <summary>
        ///     类型转换函数集合
        /// </summary>
        private static TmphInterlocked.TmphDictionary<TmphCastType, MethodInfo> castMethods =
            new TmphInterlocked.TmphDictionary<TmphCastType, MethodInfo>(TmphDictionary<TmphCastType>.Create<MethodInfo>());

        /// <summary>
        ///     判断数据是否为空
        /// </summary>
        internal static readonly MethodInfo DataReaderIsDBNullMethod = typeof(DbDataReader).GetMethod("IsDBNull",
            BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int) }, null);

        /// <summary>
        ///     基本类型设置函数
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> dataReaderMethods;

        /// <summary>
        ///     可空类型构造函数
        /// </summary>
        internal static readonly Dictionary<Type, ConstructorInfo> NullableConstructors;

        static TmphPub()
        {
            Type[] intType = { typeof(int) };
            dataReaderMethods = TmphDictionary.CreateOnly<Type, MethodInfo>();
            dataReaderMethods.Add(typeof(bool),
                typeof(DbDataReader).GetMethod("GetBoolean", BindingFlags.Public | BindingFlags.Instance, null, intType,
                    null));
            dataReaderMethods.Add(typeof(byte),
                typeof(DbDataReader).GetMethod("GetByte", BindingFlags.Public | BindingFlags.Instance, null, intType,
                    null));
            dataReaderMethods.Add(typeof(char),
                typeof(DbDataReader).GetMethod("GetChar", BindingFlags.Public | BindingFlags.Instance, null, intType,
                    null));
            dataReaderMethods.Add(typeof(DateTime),
                typeof(DbDataReader).GetMethod("GetDateTime", BindingFlags.Public | BindingFlags.Instance, null,
                    intType, null));
            dataReaderMethods.Add(typeof(decimal),
                typeof(DbDataReader).GetMethod("GetDecimal", BindingFlags.Public | BindingFlags.Instance, null, intType,
                    null));
            dataReaderMethods.Add(typeof(double),
                typeof(DbDataReader).GetMethod("GetDouble", BindingFlags.Public | BindingFlags.Instance, null, intType,
                    null));
            dataReaderMethods.Add(typeof(float),
                typeof(DbDataReader).GetMethod("GetFloat", BindingFlags.Public | BindingFlags.Instance, null, intType,
                    null));
            dataReaderMethods.Add(typeof(Guid),
                typeof(DbDataReader).GetMethod("GetGuid", BindingFlags.Public | BindingFlags.Instance, null, intType,
                    null));
            dataReaderMethods.Add(typeof(short),
                typeof(DbDataReader).GetMethod("GetInt16", BindingFlags.Public | BindingFlags.Instance, null, intType,
                    null));
            dataReaderMethods.Add(typeof(int),
                typeof(DbDataReader).GetMethod("GetInt32", BindingFlags.Public | BindingFlags.Instance, null, intType,
                    null));
            dataReaderMethods.Add(typeof(long),
                typeof(DbDataReader).GetMethod("GetInt64", BindingFlags.Public | BindingFlags.Instance, null, intType,
                    null));
            dataReaderMethods.Add(typeof(string),
                typeof(DbDataReader).GetMethod("GetString", BindingFlags.Public | BindingFlags.Instance, null, intType,
                    null));

            NullableConstructors = TmphDictionary.CreateOnly<Type, ConstructorInfo>();
            NullableConstructors.Add(typeof(bool),
                typeof(bool?).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(bool) },
                    null));
            NullableConstructors.Add(typeof(byte),
                typeof(byte?).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(byte) },
                    null));
            NullableConstructors.Add(typeof(char),
                typeof(char?).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(char) },
                    null));
            NullableConstructors.Add(typeof(DateTime),
                typeof(DateTime?).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(DateTime) }, null));
            NullableConstructors.Add(typeof(decimal),
                typeof(decimal?).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(decimal) }, null));
            NullableConstructors.Add(typeof(double),
                typeof(double?).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(double) }, null));
            NullableConstructors.Add(typeof(float),
                typeof(float?).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(float) },
                    null));
            NullableConstructors.Add(typeof(Guid),
                typeof(Guid?).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(Guid) },
                    null));
            NullableConstructors.Add(typeof(short),
                typeof(short?).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(short) },
                    null));
            NullableConstructors.Add(typeof(int),
                typeof(int?).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int) },
                    null));
            NullableConstructors.Add(typeof(long),
                typeof(long?).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(long) },
                    null));
        }

        /// <summary>
        ///     创建构造函数委托
        /// </summary>
        /// <param name="type"></param>
        /// <param name="TParameterType">参数类型</param>
        /// <returns>构造函数委托</returns>
        public static Delegate CreateConstructor(Type type, Type TParameterType)
        {
            var dynamicMethod = new DynamicMethod("constructor", type, new[] { TParameterType }, type, true);
            var generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg, 0);
            generator.Emit(OpCodes.Newobj,
                type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                    new[] { TParameterType }, null));
            generator.Emit(OpCodes.Ret);
            return dynamicMethod.CreateDelegate(typeof(Func<,>).MakeGenericType(TParameterType, type));
        }

        /// <summary>
        ///     获取数值转换委托调用函数信息
        /// </summary>
        /// <param name="type">数值类型</param>
        /// <returns>数值转换委托调用函数信息</returns>
        internal static MethodInfo GetNumberToCharStreamMethod(Type type)
        {
            return TmphNumberToCharStream.GetToStringMethod(type);
        }

        /// <summary>
        ///     获取可空类型是否为空判断函数信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns>可空类型是否为空判断函数信息</returns>
        internal static MethodInfo GetNullableHasValue(Type type)
        {
            MethodInfo method;
            if (nullableHasValues.TryGetValue(type, out method)) return method;
            method = type.GetProperty("HasValue", BindingFlags.Instance | BindingFlags.Public).GetGetMethod();
            nullableHasValues.Set(type, method);
            return method;
        }

        /// <summary>
        ///     获取可空类型获取数据函数信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns>可空类型获取数据函数信息</returns>
        internal static MethodInfo GetNullableValue(Type type)
        {
            MethodInfo method;
            if (nullableValues.TryGetValue(type, out method)) return method;
            method = type.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public).GetGetMethod();
            nullableValues.Set(type, method);
            return method;
        }

        /// <summary>
        ///     获取SQL常量转换函数信息
        /// </summary>
        /// <param name="type">数值类型</param>
        /// <returns>SQL常量转换函数信息</returns>
        internal static MethodInfo GetSqlConverterMethod(Type type)
        {
            MethodInfo method;
            if (sqlConverterMethods.TryGetValue(type, out method)) return method;
            method = typeof(TmphConstantConverter).GetMethod("convertConstant",
                BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(TmphCharStream), type }, null)
                     ?? TmphConstantConverter.ConvertConstantStringMethod.MakeGenericMethod(type);
            sqlConverterMethods.Set(type, method);
            return method;
        }

        /// <summary>
        ///     获取类型转换函数
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="toType"></param>
        /// <returns></returns>
        internal static MethodInfo GetCastMethod(Type fromType, Type toType)
        {
            if (fromType == toType) return null;
            if (fromType == typeof(int))
            {
                if (toType == typeof(uint)) return null;
            }
            else if (fromType == typeof(long))
            {
                if (toType == typeof(ulong)) return null;
            }
            else if (fromType == typeof(byte))
            {
                if (toType == typeof(sbyte)) return null;
            }
            else if (fromType == typeof(short))
            {
                if (toType == typeof(ushort)) return null;
            }
            var castType = new TmphCastType { FromType = fromType, ToType = toType };
            MethodInfo method;
            if (castMethods.TryGetValue(castType, out method)) return method;
            if (!toType.IsPrimitive)
            {
                Type[] castParameterTypes = { fromType };
                method = toType.GetMethod("op_Implicit",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, castParameterTypes, null)
                         ??
                         toType.GetMethod("op_Explicit",
                             BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null,
                             castParameterTypes, null);
            }
            if (method == null && !fromType.IsPrimitive)
            {
                foreach (
                    var methodInfo in
                        fromType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    if (methodInfo.ReturnType == toType &&
                        (methodInfo.Name == "op_Implicit" || methodInfo.Name == "op_Explicit") &&
                        methodInfo.GetParameters()[0].ParameterType == fromType)
                    {
                        method = methodInfo;
                        break;
                    }
                }
            }
            castMethods.Set(castType, method);
            return method;
        }

        /// <summary>
        ///     获取基本类型设置函数
        /// </summary>
        /// <param name="type">基本类型</param>
        /// <returns>设置函数</returns>
        internal static MethodInfo GetDataReaderMethod(Type type)
        {
            MethodInfo method;
            return dataReaderMethods.TryGetValue(type, out method) ? method : null;
        }

        /// <summary>
        ///     获取字段成员集合
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <typeparam name="memberAttribute"></typeparam>
        /// <param name="memberFilter"></param>
        /// <param name="isAllMember"></param>
        /// <returns>字段成员集合</returns>
        internal static TmphSubArray<FieldInfo> GetFields<TValueType, memberAttribute>(TmphMemberFilters memberFilter,
            bool isAllMember)
            where memberAttribute : TmphIgnoreMember
        {
            var fieldIndexs = TmphMemberIndexGroup<TValueType>.GetFields(memberFilter);
            var fields = new TmphSubArray<FieldInfo>(fieldIndexs.Length);
            foreach (var field in fieldIndexs)
            {
                var type = field.Member.FieldType;
                if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !field.IsIgnore)
                {
                    var attribute = field.GetAttribute<memberAttribute>(true, true);
                    if (isAllMember
                        ? (attribute == null || attribute.IsSetup)
                        : (attribute != null && attribute.IsSetup))
                    {
                        fields.Add(field.Member);
                    }
                }
            }
            return fields;
        }

        /// <summary>
        ///     获取字段成员集合
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="memberFilter"></param>
        /// <returns>字段成员集合</returns>
        internal static TmphKeyValue<FieldInfo, int>[] GetFieldIndexs<TValueType>(TmphMemberFilters memberFilter)
        {
            return TmphMemberIndexGroup<TValueType>.GetFields(memberFilter)
                .getArray(value => new TmphKeyValue<FieldInfo, int>(value.Member, value.MemberIndex));
        }

        /// <summary>
        ///     创建获取字段委托
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <typeparam name="fieldType"></typeparam>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static Func<TValueType, fieldType> GetField<TValueType, fieldType>(string fieldName)
        {
            var field = typeof(TValueType).GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null) TmphLog.Error.Throw(typeof(TValueType).fullName() + " 未找到字段成员 " + fieldName, true, false);
            return GetField<TValueType, fieldType>(field);
        }

        /// <summary>
        ///     创建获取字段委托
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <typeparam name="fieldType"></typeparam>
        /// <param name="field"></param>
        /// <returns></returns>
        public static Func<TValueType, fieldType> GetField<TValueType, fieldType>(FieldInfo field)
        {
            if (field.ReflectedType != typeof(TValueType) || !typeof(fieldType).IsAssignableFrom(field.FieldType))
                TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
            var dynamicMethod = new DynamicMethod("get_" + field.Name, typeof(fieldType), new[] { typeof(TValueType) },
                typeof(TValueType), true);
            var generator = dynamicMethod.GetILGenerator();
            if (typeof(TValueType).IsValueType) generator.Emit(OpCodes.Ldarga_S, 0);
            else generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, field);
            generator.Emit(OpCodes.Ret);
            return (Func<TValueType, fieldType>)dynamicMethod.CreateDelegate(typeof(Func<TValueType, fieldType>));
        }

        /// <summary>
        ///     创建获取字段委托
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <typeparam name="fieldType"></typeparam>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static TmphGetField<TValueType, fieldType> GetFieldStruct<TValueType, fieldType>(string fieldName)
            where TValueType : struct
        {
            var dynamicMethod = new DynamicMethod("getRef_" + fieldName, typeof(fieldType),
                new[] { typeof(TValueType).MakeByRefType() }, typeof(TValueType), true);
            var generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld,
                typeof(TValueType).GetField(fieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));
            generator.Emit(OpCodes.Ret);
            return
                (TmphGetField<TValueType, fieldType>)
                    dynamicMethod.CreateDelegate(typeof(TmphGetField<TValueType, fieldType>));
        }

        /// <summary>
        ///     创建设置字段委托
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="dynamicMethod"></param>
        /// <param name="fieldName"></param>
        private static void getSetField<TValueType>(DynamicMethod dynamicMethod, string fieldName)
        {
            var generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Stfld,
                typeof(TValueType).GetField(fieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));
            generator.Emit(OpCodes.Ret);
        }

        /// <summary>
        ///     创建设置字段委托
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <typeparam name="fieldType"></typeparam>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static Action<TValueType, fieldType> SetField<TValueType, fieldType>(string fieldName)
            where TValueType : class
        {
            var dynamicMethod = new DynamicMethod("set_" + fieldName, null,
                new[] { typeof(TValueType), typeof(fieldType) }, typeof(TValueType), true);
            getSetField<TValueType>(dynamicMethod, fieldName);
            return (Action<TValueType, fieldType>)dynamicMethod.CreateDelegate(typeof(Action<TValueType, fieldType>));
        }

        /// <summary>
        ///     创建设置字段委托
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <typeparam name="fieldType"></typeparam>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static TmphSetField<TValueType, fieldType> SetFieldStruct<TValueType, fieldType>(string fieldName)
            where TValueType : struct
        {
            var dynamicMethod = new DynamicMethod("set_" + fieldName, null,
                new[] { typeof(TValueType).MakeByRefType(), typeof(fieldType) }, typeof(TValueType), true);
            getSetField<TValueType>(dynamicMethod, fieldName);
            return
                (TmphSetField<TValueType, fieldType>)
                    dynamicMethod.CreateDelegate(typeof(TmphSetField<TValueType, fieldType>));
        }

        /// <summary>
        ///     获取静态属性值
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="assembly"></param>
        /// <param name="typeName"></param>
        /// <param name="name"></param>
        /// <param name="nonPublic"></param>
        /// <returns></returns>
        public static Func<TValueType> GetStaticProperty<TValueType>(Assembly assembly, string typeName, string name,
            bool nonPublic)
        {
            var type = assembly.GetType(typeName);
            var dynamicMethod = new DynamicMethod("get_" + name, typeof(TValueType), TmphNullValue<Type>.Array, type, true);
            var generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Call,
                type.GetProperty(name, BindingFlags.Static | (nonPublic ? BindingFlags.NonPublic : BindingFlags.Public))
                    .GetGetMethod(nonPublic));
            generator.Emit(OpCodes.Ret);
            return (Func<TValueType>)dynamicMethod.CreateDelegate(typeof(Func<TValueType>));
        }

        /// <summary>
        ///     获取静态属性值
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="assembly"></param>
        /// <param name="typeName"></param>
        /// <param name="name"></param>
        /// <param name="nonPublic"></param>
        /// <returns></returns>
        public static Func<object, TValueType> GetProperty<TValueType>(Assembly assembly, string typeName, string name,
            bool nonPublic)
        {
            var type = assembly.GetType(typeName);
            var dynamicMethod = new DynamicMethod("get_" + name, typeof(TValueType), new[] { typeof(object) }, type, true);
            var generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            var method =
                type.GetProperty(name,
                    BindingFlags.Instance | (nonPublic ? BindingFlags.NonPublic : BindingFlags.Public))
                    .GetGetMethod(nonPublic);
            generator.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);
            generator.Emit(OpCodes.Ret);
            return (Func<object, TValueType>)dynamicMethod.CreateDelegate(typeof(Func<object, TValueType>));
        }

        /// <summary>
        ///     创建函数委托
        /// </summary>
        /// <typeparam name="valueType1"></typeparam>
        /// <typeparam name="valueType2"></typeparam>
        /// <typeparam name="TReturnType"></typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Action<valueType1, valueType2> GetAction<valueType1, valueType2>(MethodInfo method)
        {
            var dynamicMethod = new DynamicMethod(method.Name, null, new[] { typeof(valueType1), typeof(valueType2) },
                method.DeclaringType, true);
            var generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);
            generator.Emit(OpCodes.Ret);
            return
                (Action<valueType1, valueType2>)dynamicMethod.CreateDelegate(typeof(Action<valueType1, valueType2>));
        }

        /// <summary>
        ///     创建函数委托
        /// </summary>
        /// <typeparam name="valueType1"></typeparam>
        /// <typeparam name="valueType2"></typeparam>
        /// <typeparam name="TReturnType"></typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Func<valueType1, valueType2, TReturnType> GetStaticFunc<valueType1, valueType2, TReturnType>(
            MethodInfo method)
        {
            var dynamicMethod = new DynamicMethod(method.Name, typeof(TReturnType),
                new[] { typeof(valueType1), typeof(valueType2) }, method.DeclaringType, true);
            var generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);
            generator.Emit(OpCodes.Ret);
            return
                (Func<valueType1, valueType2, TReturnType>)
                    dynamicMethod.CreateDelegate(typeof(Func<valueType1, valueType2, TReturnType>));
        }

        /// <summary>
        ///     转换类型
        /// </summary>
        private struct TmphCastType : IEquatable<TmphCastType>
        {
            /// <summary>
            ///     原始类型
            /// </summary>
            public Type FromType;

            /// <summary>
            ///     目标类型
            /// </summary>
            public Type ToType;

            /// <summary>
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public bool Equals(TmphCastType other)
            {
                return FromType == other.FromType && ToType == other.ToType;
            }

            /// <summary>
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return FromType.GetHashCode() ^ ToType.GetHashCode();
            }

            /// <summary>
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                return Equals((TmphCastType)obj);
            }
        }

        /// <summary>
        ///     枚举值解析
        /// </summary>
        /// <typeparam name="TValueType">枚举类型</typeparam>
        /// <typeparam name="intType">枚举值数字类型</typeparam>
        public static class TmphEnumCast<TValueType, intType>
        {
            /// <summary>
            ///     枚举转数字委托
            /// </summary>
            public static readonly Func<TValueType, intType> ToInt;

            /// <summary>
            ///     数字转枚举委托
            /// </summary>
            public static readonly Func<intType, TValueType> FromInt;

            static TmphEnumCast()
            {
                var dynamicMethod = new DynamicMethod("To" + typeof(intType).FullName, typeof(intType),
                    new[] { typeof(TValueType) }, typeof(TValueType), true);
                var generator = dynamicMethod.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ret);
                ToInt = (Func<TValueType, intType>)dynamicMethod.CreateDelegate(typeof(Func<TValueType, intType>));

                dynamicMethod = new DynamicMethod("From" + typeof(intType).FullName, typeof(TValueType),
                    new[] { typeof(intType) }, typeof(TValueType), true);
                generator = dynamicMethod.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ret);
                FromInt = (Func<intType, TValueType>)dynamicMethod.CreateDelegate(typeof(Func<intType, TValueType>));
            }
        }

        /// <summary>
        ///     集合构造函数
        /// </summary>
        /// <typeparam name="dictionaryType">集合类型</typeparam>
        /// <typeparam name="TKeyType">枚举值类型</typeparam>
        /// <typeparam name="TValueType">枚举值类型</typeparam>
        internal static class TmphDictionaryConstructor<dictionaryType, TKeyType, TValueType>
        {
            /// <summary>
            ///     构造函数
            /// </summary>
            public static readonly Func<IDictionary<TKeyType, TValueType>, dictionaryType> Constructor =
                (Func<IDictionary<TKeyType, TValueType>, dictionaryType>)
                    CreateConstructor(typeof(dictionaryType),
                        typeof(IDictionary<,>).MakeGenericType(typeof(TKeyType), typeof(TValueType)));
        }

        /// <summary>
        ///     集合构造函数
        /// </summary>
        /// <typeparam name="TValueType">集合类型</typeparam>
        /// <typeparam name="argumentType">枚举值类型</typeparam>
        internal static class TmphListConstructor<TValueType, argumentType>
        {
            /// <summary>
            ///     构造函数
            /// </summary>
            public static readonly Func<IList<argumentType>, TValueType> Constructor =
                (Func<IList<argumentType>, TValueType>)
                    CreateConstructor(typeof(TValueType), typeof(IList<>).MakeGenericType(typeof(argumentType)));
        }

        /// <summary>
        ///     集合构造函数
        /// </summary>
        /// <typeparam name="TValueType">集合类型</typeparam>
        /// <typeparam name="argumentType">枚举值类型</typeparam>
        internal static class TmphCollectionConstructor<TValueType, argumentType>
        {
            /// <summary>
            ///     构造函数
            /// </summary>
            public static readonly Func<ICollection<argumentType>, TValueType> Constructor =
                (Func<ICollection<argumentType>, TValueType>)
                    CreateConstructor(typeof(TValueType), typeof(ICollection<>).MakeGenericType(typeof(argumentType)));
        }

        /// <summary>
        ///     集合构造函数
        /// </summary>
        /// <typeparam name="TValueType">集合类型</typeparam>
        /// <typeparam name="argumentType">枚举值类型</typeparam>
        public static class TmphEnumerableConstructor<TValueType, argumentType>
        {
            /// <summary>
            ///     构造函数
            /// </summary>
            public static readonly Func<IEnumerable<argumentType>, TValueType> Constructor =
                (Func<IEnumerable<argumentType>, TValueType>)
                    CreateConstructor(typeof(TValueType), typeof(IEnumerable<>).MakeGenericType(typeof(argumentType)));
        }

        /// <summary>
        ///     集合构造函数
        /// </summary>
        /// <typeparam name="TValueType">集合类型</typeparam>
        /// <typeparam name="argumentType">枚举值类型</typeparam>
        internal static class TmphArrayConstructor<TValueType, argumentType>
        {
            /// <summary>
            ///     构造函数
            /// </summary>
            public static readonly Func<argumentType[], TValueType> Constructor =
                (Func<argumentType[], TValueType>)
                    CreateConstructor(typeof(TValueType), typeof(argumentType).MakeArrayType());
        }
    }
}