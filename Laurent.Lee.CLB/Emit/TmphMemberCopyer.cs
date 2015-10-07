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
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    ///     成员复制
    /// </summary>
    internal static class TmphMemberCopyer
    {
        /// <summary>
        ///     动态函数
        /// </summary>
        public struct TmphMemberDynamicMethod
        {
            /// <summary>
            ///     动态函数
            /// </summary>
            private readonly DynamicMethod dynamicMethod;

            /// <summary>
            /// </summary>
            private readonly ILGenerator generator;

            /// <summary>
            ///     是否值类型
            /// </summary>
            private readonly bool isValueType;

            /// <summary>
            ///     动态函数
            /// </summary>
            /// <param name="type"></param>
            /// <param name="dynamicMethod"></param>
            public TmphMemberDynamicMethod(Type type, DynamicMethod dynamicMethod)
            {
                this.dynamicMethod = dynamicMethod;
                generator = dynamicMethod.GetILGenerator();
                isValueType = type.IsValueType;
            }

            /// <summary>
            ///     添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void Push(TmphFieldIndex field)
            {
                generator.Emit(OpCodes.Ldarg_0);
                if (isValueType) generator.Emit(OpCodes.Ldarga_S, 1);
                else
                {
                    generator.Emit(OpCodes.Ldind_Ref);
                    generator.Emit(OpCodes.Ldarg_1);
                }
                generator.Emit(OpCodes.Ldfld, field.Member);
                generator.Emit(OpCodes.Stfld, field.Member);
            }

            /// <summary>
            ///     添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void PushMemberMap(TmphFieldIndex field)
            {
                var isMember = generator.DefineLabel();
                generator.Emit(OpCodes.Ldarg_2);
                generator.Emit(OpCodes.Ldc_I4, field.MemberIndex);
                generator.Emit(OpCodes.Callvirt, TmphPub.MemberMapIsMemberMethod);
                generator.Emit(OpCodes.Brfalse_S, isMember);
                Push(field);
                generator.MarkLabel(isMember);
            }

            /// <summary>
            ///     创建成员复制委托
            /// </summary>
            /// <returns>成员复制委托</returns>
            public Delegate Create<delegateType>()
            {
                generator.Emit(OpCodes.Ret);
                return dynamicMethod.CreateDelegate(typeof(delegateType));
            }
        }
    }

    /// <summary>
    ///     成员复制
    /// </summary>
    /// <typeparam name="TValueType">对象类型</typeparam>
    public static class TmphMemberCopyer<TValueType>
    {
        /// <summary>
        ///     是否采用值类型复制模式
        /// </summary>
        private static readonly bool isValueCopy;

        /// <summary>
        ///     默认成员复制委托
        /// </summary>
        private static readonly TmphCopyer defaultCopyer;

        /// <summary>
        ///     默认成员复制委托
        /// </summary>
        private static readonly TmphMemberMapCopyer defaultMemberCopyer;

        /// <summary>
        ///     对象浅复制
        /// </summary>
        private static readonly Func<TValueType, object> memberwiseClone;

        static TmphMemberCopyer()
        {
            Type type = typeof(TValueType), refType = type.MakeByRefType();
            if (!type.IsValueType)
                memberwiseClone =
                    (Func<TValueType, object>)
                        Delegate.CreateDelegate(typeof(Func<TValueType, object>),
                            typeof(TValueType).GetMethod("MemberwiseClone",
                                BindingFlags.Instance | BindingFlags.NonPublic));
            if (type.IsArray)
            {
                if (type.GetArrayRank() == 1)
                {
                    var elementType = type.GetElementType();
                    defaultCopyer =
                        (TmphCopyer)
                            Delegate.CreateDelegate(typeof(TmphCopyer),
                                elementType.GetMethod("copyArray", BindingFlags.Static | BindingFlags.NonPublic, null,
                                    new[] { refType, type }, null));
                    defaultMemberCopyer =
                        (TmphMemberMapCopyer)
                            Delegate.CreateDelegate(typeof(TmphMemberMapCopyer),
                                elementType.GetMethod("copyArray", BindingFlags.Static | BindingFlags.NonPublic, null,
                                    new[] { refType, type, typeof(TmphMemberMap) }, null));
                    return;
                }
                defaultCopyer = noCopy;
                defaultMemberCopyer = noCopy;
                return;
            }
            if (type.IsEnum || type.IsPointer || type.IsInterface)
            {
                isValueCopy = true;
                return;
            }
            foreach (var methodInfo in TmphAttributeMethod.GetStatic(type))
            {
                if (methodInfo.Method.ReturnType == typeof(void))
                {
                    var parameters = methodInfo.Method.GetParameters();
                    if (parameters.Length == 3 && parameters[0].ParameterType == refType &&
                        parameters[1].ParameterType == type && parameters[2].ParameterType == typeof(TmphMemberMap))
                    {
                        if (methodInfo.GetAttribute<TmphMemberCopy.TmphCustom>(true) != null)
                        {
                            defaultCopyer = customCopy;
                            defaultMemberCopyer =
                                (TmphMemberMapCopyer)
                                    Delegate.CreateDelegate(typeof(TmphMemberMapCopyer), methodInfo.Method);
                            return;
                        }
                    }
                }
            }
            var fields = TmphMemberIndexGroup<TValueType>.GetFields();
            if (fields.Length == 0)
            {
                defaultCopyer = noCopy;
                defaultMemberCopyer = noCopy;
                return;
            }
            var dynamicMethod = new TmphMemberCopyer.TmphMemberDynamicMethod(type,
                new DynamicMethod("memberCopyer", null, new[] { refType, type }, type, true));
            var memberMapDynamicMethod = new TmphMemberCopyer.TmphMemberDynamicMethod(type,
                new DynamicMethod("memberMapCopyer", null, new[] { refType, type, typeof(TmphMemberMap) }, type, true));
            if (type.IsValueType)
            {
                foreach (var field in fields)
                {
                    dynamicMethod.Push(field);
                    memberMapDynamicMethod.PushMemberMap(field);
                }
            }
            else
            {
                foreach (var field in fields)
                {
                    dynamicMethod.Push(field);
                    memberMapDynamicMethod.PushMemberMap(field);
                }
            }
            defaultCopyer = (TmphCopyer)dynamicMethod.Create<TmphCopyer>();
            defaultMemberCopyer = (TmphMemberMapCopyer)memberMapDynamicMethod.Create<TmphMemberMapCopyer>();
        }

        /// <summary>
        ///     对象成员复制
        /// </summary>
        /// <param name="value">目标对象</param>
        /// <param name="readValue">被复制对象</param>
        /// <param name="memberMap">成员位图</param>
        public static void Copy(ref TValueType value, TValueType readValue, TmphMemberMap memberMap = null)
        {
            if (isValueCopy) value = readValue;
            else if (memberMap == null || memberMap.IsDefault) defaultCopyer(ref value, readValue);
            else defaultMemberCopyer(ref value, readValue, memberMap);
        }

        /// <summary>
        ///     对象成员复制
        /// </summary>
        /// <param name="value">目标对象</param>
        /// <param name="readValue">被复制对象</param>
        /// <param name="memberMap">成员位图</param>
        public static void Copy(TValueType value, TValueType readValue, TmphMemberMap memberMap = null)
        {
            if (memberMap == null || memberMap.IsDefault) defaultCopyer(ref value, readValue);
            else defaultMemberCopyer(ref value, readValue, memberMap);
        }

        /// <summary>
        ///     数组复制
        /// </summary>
        /// <param name="value"></param>
        /// <param name="readValue"></param>
        private static void copyArray(ref TValueType[] value, TValueType[] readValue)
        {
            if (readValue != null)
            {
                if (readValue.Length == 0)
                {
                    if (value == null) value = TmphNullValue<TValueType>.Array;
                    return;
                }
                if (value == null || value.Length < readValue.Length) Array.Resize(ref value, readValue.Length);
                Array.Copy(readValue, 0, value, 0, readValue.Length);
            }
        }

        /// <summary>
        ///     数组复制
        /// </summary>
        /// <param name="value"></param>
        /// <param name="readValue"></param>
        /// <param name="memberMap"></param>
        private static void copyArray(ref TValueType[] value, TValueType[] readValue, TmphMemberMap memberMap)
        {
            copyArray(ref value, readValue);
        }

        /// <summary>
        ///     自定义复制函数
        /// </summary>
        /// <param name="value"></param>
        /// <param name="readValue"></param>
        private static void customCopy(ref TValueType value, TValueType readValue)
        {
            defaultMemberCopyer(ref value, readValue, null);
        }

        /// <summary>
        ///     没有复制字段
        /// </summary>
        /// <param name="value"></param>
        /// <param name="readValue"></param>
        private static void noCopy(ref TValueType value, TValueType readValue)
        {
        }

        /// <summary>
        ///     没有复制字段
        /// </summary>
        /// <param name="value"></param>
        /// <param name="readValue"></param>
        /// <param name="memberMap"></param>
        private static void noCopy(ref TValueType value, TValueType readValue, TmphMemberMap memberMap)
        {
        }

        /// <summary>
        ///     对象浅复制
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TValueType MemberwiseClone(TValueType value)
        {
            return memberwiseClone != null ? (TValueType)memberwiseClone(value) : value;
        }

        /// <summary>
        ///     成员复制委托
        /// </summary>
        /// <param name="value"></param>
        /// <param name="copyValue"></param>
        private delegate void TmphCopyer(ref TValueType value, TValueType copyValue);

        /// <summary>
        ///     成员复制委托
        /// </summary>
        /// <param name="value"></param>
        /// <param name="copyValue"></param>
        /// <param name="memberMap">成员位图</param>
        private delegate void TmphMemberMapCopyer(ref TValueType value, TValueType copyValue, TmphMemberMap memberMap);
    }
}