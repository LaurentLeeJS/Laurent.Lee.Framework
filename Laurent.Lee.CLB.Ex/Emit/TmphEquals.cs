using Laurent.Lee.CLB.Code;
using Laurent.Lee.CLB.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    /// 对象对比
    /// </summary>
    public static class TmphEquals
    {
        /// <summary>
        /// 动态函数
        /// </summary>
        public struct TmphMemberDynamicMethod
        {
            /// <summary>
            /// 动态函数
            /// </summary>
            private DynamicMethod dynamicMethod;

            /// <summary>
            ///
            /// </summary>
            private ILGenerator generator;

            /// <summary>
            /// 数据类型
            /// </summary>
            private Type type;

            /// <summary>
            /// 是否值类型
            /// </summary>
            private bool isValueType;

            /// <summary>
            ///
            /// </summary>
            private bool isMemberMap;

            /// <summary>
            /// 动态函数
            /// </summary>
            /// <param name="type"></param>
            public TmphMemberDynamicMethod(Type type, bool isMemberMap)
            {
                this.type = type;
                if (this.isMemberMap = isMemberMap) dynamicMethod = new DynamicMethod("memberMapEquals", typeof(bool), new Type[] { type, type, typeof(TmphMemberMap) }, type, true);
                else dynamicMethod = new DynamicMethod("equals", typeof(bool), new Type[] { type, type }, type, true);
                generator = dynamicMethod.GetILGenerator();
                if (!(isValueType = type.IsValueType))
                {
                    Label next = generator.DefineLabel();
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Call, TmphPubExtension.ReferenceEqualsMethod);
                    generator.Emit(OpCodes.Brfalse_S, next);
                    generator.Emit(OpCodes.Ldc_I4_1);
                    generator.Emit(OpCodes.Ret);
                    generator.MarkLabel(next);
                }
            }

            /// <summary>
            /// 添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void Push(FieldInfo field)
            {
                TmphKeyValue<FieldInfo, MethodInfo> fieldInvoke = getEqualsFieldInvoke(field.FieldType);
                Label next = generator.DefineLabel();
                generator.Emit(OpCodes.Ldsfld, fieldInvoke.Key);
                if (isValueType) generator.Emit(OpCodes.Ldarga_S, 0);
                else generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, field);
                if (isValueType) generator.Emit(OpCodes.Ldarga_S, 1);
                else generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldfld, field);
                generator.Emit(OpCodes.Callvirt, fieldInvoke.Value);
                generator.Emit(OpCodes.Brtrue_S, next);
                generator.Emit(OpCodes.Ldc_I4_0);
                generator.Emit(OpCodes.Ret);
                generator.MarkLabel(next);
            }

            /// <summary>
            /// 添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            /// <param name="memberIndex">字段信息</param>
            public void Push(FieldInfo field, int memberIndex)
            {
                Label next = generator.DefineLabel();
                generator.Emit(OpCodes.Ldarg_2);
                generator.Emit(OpCodes.Ldc_I4, memberIndex);
                generator.Emit(OpCodes.Callvirt, TmphPubExtension.MemberMapIsMemberMethod);
                generator.Emit(OpCodes.Brfalse_S, next);
                Push(field);
                generator.MarkLabel(next);
            }

            /// <summary>
            /// 基类调用
            /// </summary>
            public void Base()
            {
                if (!isValueType && (type = type.BaseType) != typeof(object))
                {
                    TmphKeyValue<FieldInfo, MethodInfo> fieldInvoke = getEqualsFieldInvoke(type);
                    generator.Emit(OpCodes.Ldsfld, fieldInvoke.Key);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Callvirt, fieldInvoke.Value);
                }
                else generator.Emit(OpCodes.Ldc_I4_1);
                generator.Emit(OpCodes.Ret);
            }

            /// <summary>
            /// 创建委托
            /// </summary>
            /// <returns>委托</returns>
            public Delegate Create<delegateType>()
            {
                if (isMemberMap)
                {
                    generator.Emit(OpCodes.Ldc_I4_1);
                    generator.Emit(OpCodes.Ret);
                }
                return dynamicMethod.CreateDelegate(typeof(delegateType));
            }
        }

        /// <summary>
        /// 类型比较字段与委托调用集合
        /// </summary>
        private static TmphInterlocked.TmphDictionary<Type, TmphKeyValue<FieldInfo, MethodInfo>> equalsFieldInvokes = new TmphInterlocked.TmphDictionary<Type, TmphKeyValue<FieldInfo, MethodInfo>>(Laurent.Lee.CLB.TmphDictionary.CreateOnly<Type, TmphKeyValue<FieldInfo, MethodInfo>>());

        /// <summary>
        /// 获取类型比较字段与委托调用
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static TmphKeyValue<FieldInfo, MethodInfo> getEqualsFieldInvoke(Type type)
        {
            TmphKeyValue<FieldInfo, MethodInfo> fieldInvoke;
            if (equalsFieldInvokes.TryGetValue(type, out fieldInvoke)) return fieldInvoke;
            fieldInvoke.Key = typeof(TmphEquals<>).MakeGenericType(type).GetField("Equals", BindingFlags.Static | BindingFlags.Public);
            fieldInvoke.Value = typeof(Func<,,>).MakeGenericType(type, type, typeof(bool)).GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { type, type }, null);
            equalsFieldInvokes.Set(type, fieldInvoke);
            return fieldInvoke;
        }

        /// <summary>
        /// 可空数据比较
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private static bool nullable<TValueType>(Nullable<TValueType> left, Nullable<TValueType> right) where TValueType : struct
        {
            if (left.HasValue) return right.HasValue && TmphEquals<TValueType>.Equals(left.Value, right.Value);
            return !right.HasValue;
        }

        /// <summary>
        /// 可空数据比较函数信息
        /// </summary>
        public static readonly MethodInfo NullableMethod = typeof(TmphEquals).GetMethod("nullable", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 结构体数据比较
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private static bool structIEquatable<TValueType>(TValueType left, TValueType right) where TValueType : struct, IEquatable<TValueType>
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 结构体数据比较函数信息
        /// </summary>
        public static readonly MethodInfo StructIEquatableMethod = typeof(TmphEquals).GetMethod("structIEquatable", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 引用对象比较
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private static bool classIEquatable<TValueType>(TValueType left, TValueType right) where TValueType : class, IEquatable<TValueType>
        {
            if (Object.ReferenceEquals(left, right)) return true;
            return left != null && right != null && left.Equals(right);
        }

        /// <summary>
        /// 引用对象比较函数信息
        /// </summary>
        public static readonly MethodInfo ClassIEquatableMethod = typeof(TmphEquals).GetMethod("classIEquatable", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 数组比较
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="leftArray"></param>
        /// <param name="rightArray"></param>
        /// <returns></returns>
        private static bool array<TValueType>(TValueType[] leftArray, TValueType[] rightArray)
        {
            if (Object.ReferenceEquals(leftArray, rightArray)) return true;
            if (leftArray != null && rightArray != null && leftArray.Length == rightArray.Length)
            {
                int index = 0;
                foreach (TValueType left in leftArray)
                {
                    if (!TmphEquals<TValueType>.Equals(left, rightArray[index++])) return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 数组比较函数信息
        /// </summary>
        public static readonly MethodInfo ArrayMethod = typeof(TmphEquals).GetMethod("array", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 数组比较
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="leftArray"></param>
        /// <param name="rightArray"></param>
        /// <returns></returns>
        private static bool TmphSubArray<TValueType>(TmphSubArray<TValueType> leftArray, TmphSubArray<TValueType> rightArray)
        {
            if (leftArray.Count == rightArray.Count)
            {
                for (int index = leftArray.Count; index != 0;)
                {
                    --index;
                    if (!TmphEquals<TValueType>.Equals(leftArray[index], rightArray[index])) return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 数组比较函数信息
        /// </summary>
        public static readonly MethodInfo SubArrayMethod = typeof(TmphEquals).GetMethod("TmphSubArray", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 数组比较
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="leftArray"></param>
        /// <param name="rightArray"></param>
        /// <returns></returns>
        private static bool list<TValueType>(TmphList<TValueType> leftArray, TmphList<TValueType> rightArray)
        {
            if (Object.ReferenceEquals(leftArray, rightArray)) return true;
            if (leftArray != null && rightArray != null && leftArray.Count == rightArray.Count)
            {
                for (int index = leftArray.Count; index != 0;)
                {
                    --index;
                    if (!TmphEquals<TValueType>.Equals(leftArray[index], rightArray[index])) return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 数组比较函数信息
        /// </summary>
        public static readonly MethodInfo ListMethod = typeof(TmphEquals).GetMethod("list", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 数组比较
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="leftArray"></param>
        /// <param name="rightArray"></param>
        /// <returns></returns>
        private static bool TmphCollection<TValueType, TArgumentType>(TValueType leftArray, TValueType rightArray) where TValueType : IEnumerable<TArgumentType>, ICollection
        {
            if (Object.ReferenceEquals(leftArray, rightArray)) return true;
            if (leftArray != null && rightArray != null && leftArray.Count == rightArray.Count)
            {
                IEnumerator<TArgumentType> right = rightArray.GetEnumerator();
                foreach (TArgumentType left in leftArray)
                {
                    if (!right.MoveNext() || !TmphEquals<TArgumentType>.Equals(left, right.Current)) return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 数组比较函数信息
        /// </summary>
        public static readonly MethodInfo CollectionMethod = typeof(TmphEquals).GetMethod("TmphCollection", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 字典比较
        /// </summary>
        /// <typeparam name="TKeyType"></typeparam>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="leftArray"></param>
        /// <param name="rightArray"></param>
        /// <returns></returns>
        private static bool dictionary<TDictionaryType, TKeyType, TValueType>(TDictionaryType leftArray, TDictionaryType rightArray) where TDictionaryType : IDictionary<TKeyType, TValueType>
        {
            if (Object.ReferenceEquals(leftArray, rightArray)) return true;
            if (leftArray != null && rightArray != null && leftArray.Count == rightArray.Count)
            {
                foreach (KeyValuePair<TKeyType, TValueType> left in leftArray)
                {
                    TValueType right;
                    if (!rightArray.TryGetValue(left.Key, out right) || !TmphEquals<TValueType>.Equals(left.Value, right))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 字典比较函数信息
        /// </summary>
        public static readonly MethodInfo DictionaryMethod = typeof(TmphEquals).GetMethod("dictionary", BindingFlags.Static | BindingFlags.NonPublic);
    }

    /// <summary>
    /// 对象对比
    /// </summary>
    /// <typeparam name="TValueType"></typeparam>
    public static class TmphEquals<TValueType>
    {
        /// <summary>
        /// 对象对比委托
        /// </summary>
        public static new readonly Func<TValueType, TValueType, bool> Equals;

        /// <summary>
        /// 对象对比委托
        /// </summary>
        public static readonly Func<TValueType, TValueType, TmphMemberMap, bool> MemberMapEquals;

        /// <summary>
        /// 枚举值比较
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private static bool enumByte(TValueType left, TValueType right)
        {
            return TmphPub.TmphEnumCast<TValueType, byte>.ToInt(left) == TmphPub.TmphEnumCast<TValueType, byte>.ToInt(right);
        }

        /// <summary>
        /// 枚举值比较
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private static bool enumSByte(TValueType left, TValueType right)
        {
            return TmphPub.TmphEnumCast<TValueType, sbyte>.ToInt(left) == TmphPub.TmphEnumCast<TValueType, sbyte>.ToInt(right);
        }

        /// <summary>
        /// 枚举值比较
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private static bool enumUShort(TValueType left, TValueType right)
        {
            return TmphPub.TmphEnumCast<TValueType, ushort>.ToInt(left) == TmphPub.TmphEnumCast<TValueType, ushort>.ToInt(right);
        }

        /// <summary>
        /// 枚举值比较
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private static bool enumShort(TValueType left, TValueType right)
        {
            return TmphPub.TmphEnumCast<TValueType, short>.ToInt(left) == TmphPub.TmphEnumCast<TValueType, short>.ToInt(right);
        }

        /// <summary>
        /// 枚举值比较
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private static bool enumUInt(TValueType left, TValueType right)
        {
            return TmphPub.TmphEnumCast<TValueType, uint>.ToInt(left) == TmphPub.TmphEnumCast<TValueType, uint>.ToInt(right);
        }

        /// <summary>
        /// 枚举值比较
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private static bool enumInt(TValueType left, TValueType right)
        {
            return TmphPub.TmphEnumCast<TValueType, int>.ToInt(left) == TmphPub.TmphEnumCast<TValueType, int>.ToInt(right);
        }

        /// <summary>
        /// 枚举值比较
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private static bool enumULong(TValueType left, TValueType right)
        {
            return TmphPub.TmphEnumCast<TValueType, ulong>.ToInt(left) == TmphPub.TmphEnumCast<TValueType, ulong>.ToInt(right);
        }

        /// <summary>
        /// 枚举值比较
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private static bool enumLong(TValueType left, TValueType right)
        {
            return TmphPub.TmphEnumCast<TValueType, long>.ToInt(left) == TmphPub.TmphEnumCast<TValueType, long>.ToInt(right);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private static bool unknown(TValueType left, TValueType right)
        {
            return false;
        }

        static TmphEquals()
        {
            Type type = typeof(TValueType);
            if (typeof(IEquatable<TValueType>).IsAssignableFrom(type))
            {
                Equals = (Func<TValueType, TValueType, bool>)Delegate.CreateDelegate(typeof(Func<TValueType, TValueType, bool>), (type.IsValueType ? TmphEquals.StructIEquatableMethod : TmphEquals.ClassIEquatableMethod).MakeGenericMethod(type));
                return;
            }
            if (type.IsArray)
            {
                if (type.GetArrayRank() == 1)
                {
                    Equals = (Func<TValueType, TValueType, bool>)Delegate.CreateDelegate(typeof(Func<TValueType, TValueType, bool>), TmphEquals.ArrayMethod.MakeGenericMethod(type.GetElementType()));
                }
                else Equals = unknown;
                return;
            }
            if (type.IsEnum)
            {
                Type TEnumType = System.Enum.GetUnderlyingType(type);
                if (TEnumType == typeof(uint)) Equals = enumUInt;
                else if (TEnumType == typeof(byte)) Equals = enumByte;
                else if (TEnumType == typeof(ulong)) Equals = enumULong;
                else if (TEnumType == typeof(ushort)) Equals = enumUShort;
                else if (TEnumType == typeof(long)) Equals = enumLong;
                else if (TEnumType == typeof(short)) Equals = enumShort;
                else if (TEnumType == typeof(sbyte)) Equals = enumSByte;
                else Equals = enumInt;
                return;
            }
            if (type.IsGenericType)
            {
                Type genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(Nullable<>))
                {
                    Equals = (Func<TValueType, TValueType, bool>)Delegate.CreateDelegate(typeof(Func<TValueType, TValueType, bool>), TmphEquals.NullableMethod.MakeGenericMethod(type.GetGenericArguments()));
                    return;
                }
                if (genericType == typeof(TmphSubArray<>))
                {
                    Equals = (Func<TValueType, TValueType, bool>)Delegate.CreateDelegate(typeof(Func<TValueType, TValueType, bool>), TmphEquals.SubArrayMethod.MakeGenericMethod(type.GetGenericArguments()));
                    return;
                }
                if (genericType == typeof(TmphList<>))
                {
                    Equals = (Func<TValueType, TValueType, bool>)Delegate.CreateDelegate(typeof(Func<TValueType, TValueType, bool>), TmphEquals.ListMethod.MakeGenericMethod(type.GetGenericArguments()));
                    return;
                }
                if (genericType == typeof(List<>) || genericType == typeof(HashSet<>) || genericType == typeof(Queue<>) || genericType == typeof(Stack<>) || genericType == typeof(SortedSet<>) || genericType == typeof(LinkedList<>))
                {
                    Equals = (Func<TValueType, TValueType, bool>)Delegate.CreateDelegate(typeof(Func<TValueType, TValueType, bool>), TmphEquals.CollectionMethod.MakeGenericMethod(type, type.GetGenericArguments()[0]));
                    return;
                }
                if (genericType == typeof(Dictionary<,>) || genericType == typeof(SortedDictionary<,>) || genericType == typeof(SortedList<,>))
                {
                    Type[] parameterTypes = type.GetGenericArguments();
                    Equals = (Func<TValueType, TValueType, bool>)Delegate.CreateDelegate(typeof(Func<TValueType, TValueType, bool>), TmphEquals.DictionaryMethod.MakeGenericMethod(type, parameterTypes[0], parameterTypes[1]));
                    return;
                }
            }
            if (type.IsPointer || type.IsInterface)
            {
                Equals = unknown;
                return;
            }
            TmphEquals.TmphMemberDynamicMethod dynamicMethod = new TmphEquals.TmphMemberDynamicMethod(type, false);
            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                dynamicMethod.Push(field);
            }
            dynamicMethod.Base();
            Equals = (Func<TValueType, TValueType, bool>)dynamicMethod.Create<Func<TValueType, TValueType, bool>>();

            dynamicMethod = new TmphEquals.TmphMemberDynamicMethod(type, true);
            foreach (TmphKeyValue<FieldInfo, int> field in TmphPubExtension.GetFieldIndexs<TValueType>(TmphMemberFilters.InstanceField))
            {
                dynamicMethod.Push(field.Key, field.Value);
            }
            MemberMapEquals = (Func<TValueType, TValueType, TmphMemberMap, bool>)dynamicMethod.Create<Func<TValueType, TValueType, TmphMemberMap, bool>>();
        }
    }
}