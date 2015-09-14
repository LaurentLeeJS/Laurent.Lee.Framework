using Laurent.Lee.CLB.Code;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    /// 随机对象生成
    /// </summary>
    public static class TmphRandom
    {
        /// <summary>
        /// 最大随机数组尺寸
        /// </summary>
        private const uint maxSize = (1 << 4) - 1;

        /// <summary>
        /// 随机对象成员配置
        /// </summary>
        public sealed class TmphMember : TmphIgnoreMember
        {
        }

        /// <summary>
        /// 随机对象生成配置
        /// </summary>
        public sealed class TmphConfig
        {
            /// <summary>
            /// 时间是否精确到秒
            /// </summary>
            public bool IsSecondDateTime;

            /// <summary>
            /// 浮点数是否转换成字符串
            /// </summary>
            public bool IsParseFloat;

            /// <summary>
            /// 是否生成字符0
            /// </summary>
            public bool IsNullChar = true;

            /// <summary>
            /// 历史对象集合
            /// </summary>
            public Dictionary<Type, TmphList<object>> History;

            /// <summary>
            /// 获取历史对象
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            public object TryGetValue(Type type)
            {
                if (History != null && CLB.TmphRandom.Default.NextBit() == 0)
                {
                    TmphList<object> objects;
                    if (History.TryGetValue(type, out objects)) return objects.Unsafer.Array[CLB.TmphRandom.Default.Next(objects.Count)];
                }
                return null;
            }

            /// <summary>
            /// 保存历史对象
            /// </summary>
            /// <typeparam name="TValueType"></typeparam>
            /// <param name="value"></param>
            /// <returns></returns>
            public TValueType SaveHistory<TValueType>(TValueType value)
            {
                if (History != null && value != null)
                {
                    TmphList<object> objects;
                    if (!History.TryGetValue(typeof(TValueType), out objects)) History.Add(typeof(TValueType), objects = new TmphList<object>());
                    objects.Add(value);
                }
                return value;
            }
        }

        /// <summary>
        /// 默认随机对象生成配置
        /// </summary>
        public static readonly TmphConfig DefaultConfig = new TmphConfig { History = TmphDictionary.CreateAny<Type, TmphList<object>>() };

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
            /// 动态函数
            /// </summary>
            /// <param name="type"></param>
            public TmphMemberDynamicMethod(Type type)
            {
                dynamicMethod = new DynamicMethod("random", null, new Type[] { type.MakeByRefType(), typeof(TmphConfig) }, this.type = type, true);
                generator = dynamicMethod.GetILGenerator();
                isValueType = type.IsValueType;
            }

            /// <summary>
            /// 添加字段
            /// </summary>
            /// <param name="field">字段信息</param>
            public void Push(FieldInfo field)
            {
                generator.Emit(OpCodes.Ldarg_0);
                if (!isValueType) generator.Emit(OpCodes.Ldind_Ref);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Call, CreateMethod.MakeGenericMethod(field.FieldType));
                generator.Emit(OpCodes.Stfld, field);
            }

            /// <summary>
            /// 基类调用
            /// </summary>
            public void Base()
            {
                if (!isValueType && (type = type.BaseType) != typeof(object))
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Call, createMemberMethod.MakeGenericMethod(type));
                }
            }

            /// <summary>
            /// 创建委托
            /// </summary>
            /// <returns>委托</returns>
            public Delegate Create<delegateType>()
            {
                generator.Emit(OpCodes.Ret);
                return dynamicMethod.CreateDelegate(typeof(delegateType));
            }
        }

        /// <summary>
        /// 基本类型随机数创建函数
        /// </summary>
        private sealed class TmphCreateMethod : Attribute { }

        /// <summary>
        /// 基本类型随机数创建函数
        /// </summary>
        private sealed class TmphCreateConfigMethod : Attribute { }

        /// <summary>
        /// 基本类型随机数创建函数
        /// </summary>
        private sealed class TmphCreateConfigNullMethod : Attribute { }

        /// <summary>
        /// 创建随机数
        /// </summary>
        /// <returns></returns>
        [TmphCreateMethod]
        private static bool createBool()
        {
            return CLB.TmphRandom.Default.NextBit() != 0;
        }

        /// <summary>
        /// 创建随机数
        /// </summary>
        /// <returns></returns>
        [TmphCreateMethod]
        public static byte CreateByte()
        {
            return CLB.TmphRandom.Default.NextByte();
        }

        /// <summary>
        /// 创建随机数
        /// </summary>
        /// <returns></returns>
        [TmphCreateMethod]
        public static sbyte CreateSByte()
        {
            return (sbyte)CLB.TmphRandom.Default.NextByte();
        }

        /// <summary>
        /// 创建随机数
        /// </summary>
        /// <returns></returns>
        [TmphCreateMethod]
        public static short CreateShort()
        {
            return (short)CLB.TmphRandom.Default.NextUShort();
        }

        /// <summary>
        /// 创建随机数
        /// </summary>
        /// <returns></returns>
        [TmphCreateMethod]
        public static ushort CreateUShort()
        {
            return CLB.TmphRandom.Default.NextUShort();
        }

        /// <summary>
        /// 创建随机数
        /// </summary>
        /// <returns></returns>
        [TmphCreateMethod]
        public static int CreateInt()
        {
            return CLB.TmphRandom.Default.Next();
        }

        /// <summary>
        /// 创建随机数
        /// </summary>
        /// <returns></returns>
        [TmphCreateMethod]
        public static uint CreateUInt()
        {
            return (uint)CLB.TmphRandom.Default.Next();
        }

        /// <summary>
        /// 创建随机数
        /// </summary>
        /// <returns></returns>
        [TmphCreateMethod]
        public static long CreateLong()
        {
            return (long)CLB.TmphRandom.Default.NextULong();
        }

        /// <summary>
        /// 创建随机数
        /// </summary>
        /// <returns></returns>
        [TmphCreateMethod]
        public static ulong CreateULong()
        {
            return CLB.TmphRandom.Default.NextULong();
        }

        /// <summary>
        /// 随机数除数
        /// </summary>
        private static readonly decimal decimalDiv = 100;

        /// <summary>
        /// 创建随机数
        /// </summary>
        /// <returns></returns>
        [TmphCreateMethod]
        private static decimal createDecimal()
        {
            return (decimal)(long)CLB.TmphRandom.Default.NextULong() / decimalDiv;
        }

        /// <summary>
        /// 创建随机数
        /// </summary>
        /// <returns></returns>
        [TmphCreateMethod]
        private static Guid createGuid()
        {
            return Guid.NewGuid();
        }

        /// <summary>
        /// 创建随机字符
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        [TmphCreateConfigMethod]
        [TmphCreateConfigNullMethod]
        private static char createChar(TmphConfig config)
        {
            if (config.IsNullChar) return (char)CLB.TmphRandom.Default.NextUShort();
            char value = (char)CLB.TmphRandom.Default.NextUShort();
            return value == 0 ? char.MaxValue : value;
        }

        /// <summary>
        /// 创建随机字符串
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        [TmphCreateConfigMethod]
        private static string createString(TmphConfig config)
        {
            object historyValue = config.TryGetValue(typeof(string));
            if (historyValue != null) return (string)historyValue;
            return config.SaveHistory(new string(createArray<char>(config)));
        }

        /// <summary>
        /// 创建随机字符串
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        [TmphCreateConfigNullMethod]
        private static string createStringNull(TmphConfig config)
        {
            object historyValue = config.TryGetValue(typeof(string));
            if (historyValue != null) return (string)historyValue;
            char[] value = createArrayNull<char>(config);
            return config.SaveHistory(value == null ? null : new string(value));
        }

        /// <summary>
        /// 创建随机字符串
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        [TmphCreateConfigMethod]
        [TmphCreateConfigNullMethod]
        private static TmphSubString createSubString(TmphConfig config)
        {
            return new TmphSubString(createStringNull(config));
        }

        /// <summary>
        /// 创建随机数
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        [TmphCreateConfigMethod]
        [TmphCreateConfigNullMethod]
        private unsafe static float createFloat(TmphConfig config)
        {
            if (config.IsParseFloat)
            {
                return float.Parse(CLB.TmphRandom.Default.NextFloat().ToString());
            }
            return CLB.TmphRandom.Default.NextFloat();
        }

        /// <summary>
        /// 创建随机数
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        [TmphCreateConfigMethod]
        [TmphCreateConfigNullMethod]
        private unsafe static double createDouble(TmphConfig config)
        {
            if (config.IsParseFloat)
            {
                return double.Parse(CLB.TmphRandom.Default.NextDouble().ToString());
            }
            return CLB.TmphRandom.Default.NextDouble();
        }

        /// <summary>
        /// 创建随机时间
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        [TmphCreateConfigMethod]
        [TmphCreateConfigNullMethod]
        private static DateTime createDateTime(TmphConfig config)
        {
            if (config.IsSecondDateTime)
            {
                return new DateTime((long)(CLB.TmphRandom.Default.NextULong() % (ulong)DateTime.MaxValue.Ticks) / TmphDate.SecondTicks * TmphDate.SecondTicks);
            }
            return new DateTime((long)(CLB.TmphRandom.Default.NextULong() % (ulong)DateTime.MaxValue.Ticks));
        }

        /// <summary>
        /// 创建随机数组
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        private static TValueType[] createArray<TValueType>(TmphConfig config)
        {
            object historyValue = config.TryGetValue(typeof(TValueType[]));
            if (historyValue != null) return (TValueType[])historyValue;
            uint length = (uint)CLB.TmphRandom.Default.NextByte() & maxSize;
            if (length > 0)
            {
                TValueType[] value = config.SaveHistory(new TValueType[--length]);
                while (length != 0) value[--length] = TmphRandom<TValueType>.CreateNull(config);
                return value;
            }
            return null;
        }

        /// <summary>
        /// 创建随机数组函数信息
        /// </summary>
        public static readonly MethodInfo CreateArrayMethod = typeof(TmphRandom).GetMethod("createArray", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 创建随机数组
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        private static TValueType[] createArrayNull<TValueType>(TmphConfig config)
        {
            object historyValue = config.TryGetValue(typeof(TValueType[]));
            if (historyValue != null) return (TValueType[])historyValue;
            uint length = (uint)CLB.TmphRandom.Default.NextByte() & maxSize;
            TValueType[] value = config.SaveHistory(new TValueType[length]);
            while (length != 0) value[--length] = TmphRandom<TValueType>.CreateNull(config);
            return value;
        }

        /// <summary>
        /// 创建随机数组函数信息
        /// </summary>
        public static readonly MethodInfo CreateArrayNullMethod = typeof(TmphRandom).GetMethod("createArrayNull", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 创建可空随机对象
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        private static Nullable<TValueType> createNullable<TValueType>(TmphConfig config) where TValueType : struct
        {
            if (createBool()) return TmphRandom<TValueType>.CreateNotNull(config);
            return new Nullable<TValueType>();
        }

        /// <summary>
        /// 创建可空随机对象函数信息
        /// </summary>
        public static readonly MethodInfo CreateNullableMethod = typeof(TmphRandom).GetMethod("createNullable", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 创建随机数组
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        private static TmphSubArray<TValueType> createSubArray<TValueType>(TmphConfig config)
        {
            return new TmphSubArray<TValueType>(createArray<TValueType>(config));
        }

        /// <summary>
        /// 创建随机数组函数信息
        /// </summary>
        public static readonly MethodInfo CreateSubArrayMethod = typeof(TmphRandom).GetMethod("createSubArray", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 创建随机数组
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        private static TmphList<TValueType> createList<TValueType>(TmphConfig config)
        {
            object historyValue = config.TryGetValue(typeof(TmphList<TValueType>));
            if (historyValue != null) return (TmphList<TValueType>)historyValue;
            TmphList<TValueType> value = config.SaveHistory(new TmphList<TValueType>());
            value.Add(createArray<TValueType>(config));
            return value;
        }

        /// <summary>
        /// 创建随机数组函数信息
        /// </summary>
        public static readonly MethodInfo CreateListMethod = typeof(TmphRandom).GetMethod("createList", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 创建随机数组
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        private static TmphList<TValueType> createListNull<TValueType>(TmphConfig config)
        {
            object historyValue = config.TryGetValue(typeof(TmphList<TValueType>));
            if (historyValue != null) return (TmphList<TValueType>)historyValue;
            TValueType[] array = createArrayNull<TValueType>(config);
            return array == null ? null : config.SaveHistory(new TmphList<TValueType>(array, true));
        }

        /// <summary>
        /// 创建随机数组函数信息
        /// </summary>
        public static readonly MethodInfo CreateListNullMethod = typeof(TmphRandom).GetMethod("createListNull", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 创建随机数组
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        private static List<TValueType> createSystemList<TValueType>(TmphConfig config)
        {
            object historyValue = config.TryGetValue(typeof(List<TValueType>));
            if (historyValue != null) return (List<TValueType>)historyValue;
            List<TValueType> value = config.SaveHistory(new List<TValueType>());
            value.AddRange(createArray<TValueType>(config));
            return value;
        }

        /// <summary>
        /// 创建随机数组函数信息
        /// </summary>
        public static readonly MethodInfo CreateSystemListMethod = typeof(TmphRandom).GetMethod("createSystemList", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 创建随机数组
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        private static TValueType createEnumerableConstructorNull<TValueType, TArgumentType>(TmphConfig config)
        {
            object historyValue = config.TryGetValue(typeof(TValueType));
            if (historyValue != null)
                return (TValueType)historyValue;
            TValueType[] array = createArrayNull<TValueType>(config);
            return array == null ? default(TValueType) : config.SaveHistory(TmphPub.TmphEnumerableConstructor<TValueType, TArgumentType>.Constructor(createArray<TArgumentType>(config)));
        }

        /// <summary>
        /// 创建随机数组函数信息
        /// </summary>
        public static readonly MethodInfo CreateEnumerableConstructorNullMethod = typeof(TmphRandom).GetMethod("createEnumerableConstructorNull", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 创建随机数组
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        private static TValueType createEnumerableConstructor<TValueType, TArgumentType>(TmphConfig config)
        {
            object historyValue = config.TryGetValue(typeof(TValueType));
            if (historyValue != null) return (TValueType)historyValue;
            return config.SaveHistory(TmphPub.TmphEnumerableConstructor<TValueType, TArgumentType>.Constructor(createArray<TArgumentType>(config)));
        }

        /// <summary>
        /// 创建随机数组函数信息
        /// </summary>
        public static readonly MethodInfo CreateEnumerableConstructorMethod = typeof(TmphRandom).GetMethod("createEnumerableConstructor", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 创建随机数组
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        private static List<TValueType> createSystemListNull<TValueType>(TmphConfig config)
        {
            object historyValue = config.TryGetValue(typeof(List<TValueType>));
            if (historyValue != null) return (List<TValueType>)historyValue;
            TValueType[] array = createArrayNull<TValueType>(config);
            return array == null ? null : config.SaveHistory(new List<TValueType>(array));
        }

        /// <summary>
        /// 创建随机数组函数信息
        /// </summary>
        public static readonly MethodInfo CreateSystemListNullMethod = typeof(TmphRandom).GetMethod("createSystemListNull", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 创建随机数组
        /// </summary>
        /// <typeparam name="TKeyType"></typeparam>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        private static Dictionary<TKeyType, TValueType> createDictionary<TKeyType, TValueType>(TmphConfig config)
        {
            object historyValue = config.TryGetValue(typeof(Dictionary<TKeyType, TValueType>));
            if (historyValue != null) return (Dictionary<TKeyType, TValueType>)historyValue;
            uint length = (uint)CLB.TmphRandom.Default.NextByte() & maxSize;
            Dictionary<TKeyType, TValueType> values = config.SaveHistory(TmphDictionary.CreateAny<TKeyType, TValueType>((int)length));
            while (length-- != 0)
            {
                TKeyType key = TmphRandom<TKeyType>.CreateNotNull(config);
                TValueType value;
                if (!values.TryGetValue(key, out value)) values.Add(key, TmphRandom<TValueType>.CreateNull(config));
            }
            return values;
        }

        /// <summary>
        /// 创建随机数组函数信息
        /// </summary>
        public static readonly MethodInfo CreateDictionaryMethod = typeof(TmphRandom).GetMethod("createDictionary", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 创建随机数组
        /// </summary>
        /// <typeparam name="TKeyType"></typeparam>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        private static Dictionary<TKeyType, TValueType> createDictionaryNull<TKeyType, TValueType>(TmphConfig config)
        {
            object historyValue = config.TryGetValue(typeof(Dictionary<TKeyType, TValueType>));
            if (historyValue != null) return (Dictionary<TKeyType, TValueType>)historyValue;
            uint length = (uint)CLB.TmphRandom.Default.NextByte() & maxSize;
            if (length > 0)
            {
                Dictionary<TKeyType, TValueType> values = config.SaveHistory(TmphDictionary.CreateAny<TKeyType, TValueType>((int)length));
                while (--length != 0)
                {
                    TKeyType key = TmphRandom<TKeyType>.CreateNotNull(config);
                    TValueType value;
                    if (!values.TryGetValue(key, out value)) values.Add(key, TmphRandom<TValueType>.CreateNull(config));
                }
                return values;
            }
            return null;
        }

        /// <summary>
        /// 创建随机数组函数信息
        /// </summary>
        public static readonly MethodInfo CreateDictionaryNullMethod = typeof(TmphRandom).GetMethod("createDictionaryNull", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 创建随机数组
        /// </summary>
        /// <typeparam name="TKeyType"></typeparam>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        private static SortedList<TKeyType, TValueType> createSortedList<TKeyType, TValueType>(TmphConfig config)
        {
            object historyValue = config.TryGetValue(typeof(Dictionary<TKeyType, TValueType>));
            if (historyValue != null) return (SortedList<TKeyType, TValueType>)historyValue;
            uint length = (uint)CLB.TmphRandom.Default.NextByte() & maxSize;
            SortedList<TKeyType, TValueType> values = config.SaveHistory(new SortedList<TKeyType, TValueType>((int)length));
            while (length-- != 0)
            {
                TKeyType key = TmphRandom<TKeyType>.CreateNotNull(config);
                TValueType value;
                if (!values.TryGetValue(key, out value)) values.Add(key, TmphRandom<TValueType>.CreateNull(config));
            }
            return values;
        }

        /// <summary>
        /// 创建随机数组函数信息
        /// </summary>
        public static readonly MethodInfo CreateSortedListMethod = typeof(TmphRandom).GetMethod("createSortedList", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 创建随机数组
        /// </summary>
        /// <typeparam name="TKeyType"></typeparam>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        private static SortedList<TKeyType, TValueType> createSortedListNull<TKeyType, TValueType>(TmphConfig config)
        {
            object historyValue = config.TryGetValue(typeof(Dictionary<TKeyType, TValueType>));
            if (historyValue != null) return (SortedList<TKeyType, TValueType>)historyValue;
            uint length = (uint)CLB.TmphRandom.Default.NextByte() & maxSize;
            if (length > 0)
            {
                SortedList<TKeyType, TValueType> values = config.SaveHistory(new SortedList<TKeyType, TValueType>((int)length));
                while (--length != 0)
                {
                    TKeyType key = TmphRandom<TKeyType>.CreateNotNull(config);
                    TValueType value;
                    if (!values.TryGetValue(key, out value)) values.Add(key, TmphRandom<TValueType>.CreateNull(config));
                }
                return values;
            }
            return null;
        }

        /// <summary>
        /// 创建随机数组函数信息
        /// </summary>
        public static readonly MethodInfo CreateSortedListNullMethod = typeof(TmphRandom).GetMethod("createSortedListNull", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 创建随机数组
        /// </summary>
        /// <typeparam name="TKeyType"></typeparam>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        private static SortedDictionary<TKeyType, TValueType> createSortedDictionary<TKeyType, TValueType>(TmphConfig config)
        {
            object historyValue = config.TryGetValue(typeof(Dictionary<TKeyType, TValueType>));
            if (historyValue != null) return (SortedDictionary<TKeyType, TValueType>)historyValue;
            uint length = (uint)CLB.TmphRandom.Default.NextByte() & maxSize;
            SortedDictionary<TKeyType, TValueType> values = config.SaveHistory(new SortedDictionary<TKeyType, TValueType>());
            while (length-- != 0)
            {
                TKeyType key = TmphRandom<TKeyType>.CreateNotNull(config);
                TValueType value;
                if (!values.TryGetValue(key, out value)) values.Add(key, TmphRandom<TValueType>.CreateNull(config));
            }
            return values;
        }

        /// <summary>
        /// 创建随机数组函数信息
        /// </summary>
        public static readonly MethodInfo CreateSortedDictionaryMethod = typeof(TmphRandom).GetMethod("createSortedDictionary", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 创建随机数组
        /// </summary>
        /// <typeparam name="TKeyType"></typeparam>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        private static SortedDictionary<TKeyType, TValueType> createSortedDictionaryNull<TKeyType, TValueType>(TmphConfig config)
        {
            object historyValue = config.TryGetValue(typeof(Dictionary<TKeyType, TValueType>));
            if (historyValue != null) return (SortedDictionary<TKeyType, TValueType>)historyValue;
            uint length = (uint)CLB.TmphRandom.Default.NextByte() & maxSize;
            if (length > 0)
            {
                SortedDictionary<TKeyType, TValueType> values = config.SaveHistory(new SortedDictionary<TKeyType, TValueType>());
                while (--length != 0)
                {
                    TKeyType key = TmphRandom<TKeyType>.CreateNotNull(config);
                    TValueType value;
                    if (!values.TryGetValue(key, out value)) values.Add(key, TmphRandom<TValueType>.CreateNull(config));
                }
                return values;
            }
            return null;
        }

        /// <summary>
        /// 创建随机数组函数信息
        /// </summary>
        public static readonly MethodInfo CreateSortedDictionaryNullMethod = typeof(TmphRandom).GetMethod("createSortedDictionaryNull", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 创建随机对象
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        private static TValueType create<TValueType>(TmphConfig config)
        {
            return TmphRandom<TValueType>.CreateNull(config);
        }

        /// <summary>
        /// 创建随机对象函数信息
        /// </summary>
        public static readonly MethodInfo CreateMethod = typeof(TmphRandom).GetMethod("create", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 创建随机成员对象
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        /// <param name="config"></param>
        private static void createMember<TValueType>(ref TValueType value, TmphConfig config)
        {
            TmphRandom<TValueType>.MemberCreator(ref value, config);
        }

        /// <summary>
        /// 创建随机对象函数信息
        /// </summary>
        private static readonly MethodInfo createMemberMethod = typeof(TmphRandom).GetMethod("createMember", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 基本类型随机数创建函数信息集合
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> createMethods;

        /// <summary>
        /// 获取基本类型随机数创建函数信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns>基本类型随机数创建函数信息</returns>
        public static MethodInfo GetMethod(Type type)
        {
            MethodInfo method;
            if (createMethods.TryGetValue(type, out method))
            {
                createMethods.Remove(type);
                return method;
            }
            return null;
        }

        /// <summary>
        /// 基本类型随机数创建函数信息集合
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> createConfigMethods;

        /// <summary>
        /// 获取基本类型随机数创建函数信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns>基本类型随机数创建函数信息</returns>
        public static MethodInfo GetConfigMethod(Type type)
        {
            MethodInfo method;
            if (createConfigMethods.TryGetValue(type, out method))
            {
                createConfigMethods.Remove(type);
                return method;
            }
            return null;
        }

        /// <summary>
        /// 基本类型随机数创建函数信息集合
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> createConfigNullMethods;

        /// <summary>
        /// 获取基本类型随机数创建函数信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns>基本类型随机数创建函数信息</returns>
        public static MethodInfo GetConfigNullMethod(Type type)
        {
            MethodInfo method;
            if (createConfigNullMethods.TryGetValue(type, out method))
            {
                createConfigNullMethods.Remove(type);
                return method;
            }
            return null;
        }

        static TmphRandom()
        {
            createMethods = TmphDictionary.CreateOnly<Type, MethodInfo>();
            createConfigMethods = TmphDictionary.CreateOnly<Type, MethodInfo>();
            createConfigNullMethods = TmphDictionary.CreateOnly<Type, MethodInfo>();
            foreach (MethodInfo method in typeof(TmphRandom).GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
            {
                if (method.GetCustomAttribute<TmphCreateMethod>() != null)
                {
                    createMethods.Add(method.ReturnType, method);
                }
                else
                {
                    if (method.GetCustomAttribute<TmphCreateConfigMethod>() != null)
                    {
                        createConfigMethods.Add(method.ReturnType, method);
                    }
                    if (method.GetCustomAttribute<TmphCreateConfigNullMethod>() != null)
                    {
                        createConfigNullMethods.Add(method.ReturnType, method);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 随机对象生成
    /// </summary>
    /// <typeparam name="TValueType"></typeparam>
    public static class TmphRandom<TValueType>
    {
        /// <summary>
        /// 创建随机对象
        /// </summary>
        /// <param name="value"></param>
        /// <param name="config"></param>
        public delegate void TmphCreator(ref TValueType value, TmphRandom.TmphConfig config);

        /// <summary>
        /// 基本类型随机数创建函数
        /// </summary>
        private static readonly Func<TValueType> defaultCreator;

        /// <summary>
        /// 随机对象创建函数
        /// </summary>
        private static readonly Func<TmphRandom.TmphConfig, TValueType> configNullCreator;

        /// <summary>
        /// 随机对象创建函数
        /// </summary>
        private static readonly Func<TmphRandom.TmphConfig, TValueType> configCreator;

        /// <summary>
        /// 随机对象创建函数
        /// </summary>
        public static readonly TmphCreator MemberCreator;

        /// <summary>
        /// 是否值类型
        /// </summary>
        private static readonly bool isValueType;

        /// <summary>
        /// 创建随机对象
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TValueType CreateNull(TmphRandom.TmphConfig config)
        {
            if (defaultCreator != null) return defaultCreator();
            if (configNullCreator != null) return configNullCreator(config);
            if (TmphConstructor<TValueType>.New == null) return default(TValueType);
            if (isValueType)
            {
                TValueType value = TmphConstructor<TValueType>.New();
                MemberCreator(ref value, config);
                return value;
            }
            else
            {
                object historyValue = config.TryGetValue(typeof(TValueType));
                if (historyValue != null) return (TValueType)historyValue;
                if (CLB.TmphRandom.Default.NextBit() == 0) return default(TValueType);
                TValueType value = config.SaveHistory(TmphConstructor<TValueType>.New());
                MemberCreator(ref value, config);
                return value;
            }
        }

        /// <summary>
        /// 创建随机对象
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TValueType CreateNotNull(TmphRandom.TmphConfig config)
        {
            if (defaultCreator != null) return defaultCreator();
            if (configNullCreator != null) return configNullCreator(config);
            if (TmphConstructor<TValueType>.New == null) return default(TValueType);
            if (isValueType)
            {
                TValueType value = TmphConstructor<TValueType>.New();
                MemberCreator(ref value, config);
                return value;
            }
            else
            {
                object historyValue = config.TryGetValue(typeof(TValueType));
                if (historyValue != null) return (TValueType)historyValue;
                TValueType value = config.SaveHistory(TmphConstructor<TValueType>.New());
                MemberCreator(ref value, config);
                return value;
            }
        }

        /// <summary>
        /// 创建随机对象
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TValueType Create(TmphRandom.TmphConfig config = null)
        {
            if (defaultCreator != null) return defaultCreator();
            if (configCreator != null)
            {
                if (config == null) config = TmphRandom.DefaultConfig;
                if (config.History == null) return configCreator(config);
                try
                {
                    return configCreator(config);
                }
                finally { config.History.Clear(); }
            }
            if (TmphConstructor<TValueType>.New == null) return default(TValueType);
            TValueType value = TmphConstructor<TValueType>.New();
            if (config == null) config = TmphRandom.DefaultConfig;
            if (config.History == null) MemberCreator(ref value, config);
            else
            {
                try
                {
                    MemberCreator(ref value, config);
                }
                finally { config.History.Clear(); }
            }
            return value;
        }

        /// <summary>
        /// 创建随机枚举值
        /// </summary>
        /// <returns></returns>
        private static TValueType enumByte()
        {
            return TmphPub.TmphEnumCast<TValueType, byte>.FromInt(TmphRandom.CreateByte());
        }

        /// <summary>
        /// 创建随机枚举值
        /// </summary>
        /// <returns></returns>
        private static TValueType enumSByte()
        {
            return TmphPub.TmphEnumCast<TValueType, sbyte>.FromInt(TmphRandom.CreateSByte());
        }

        /// <summary>
        /// 创建随机枚举值
        /// </summary>
        /// <returns></returns>
        private static TValueType enumShort()
        {
            return TmphPub.TmphEnumCast<TValueType, short>.FromInt(TmphRandom.CreateShort());
        }

        /// <summary>
        /// 创建随机枚举值
        /// </summary>
        /// <returns></returns>
        private static TValueType enumUShort()
        {
            return TmphPub.TmphEnumCast<TValueType, ushort>.FromInt(TmphRandom.CreateUShort());
        }

        /// <summary>
        /// 创建随机枚举值
        /// </summary>
        /// <returns></returns>
        private static TValueType enumInt()
        {
            return TmphPub.TmphEnumCast<TValueType, int>.FromInt(TmphRandom.CreateInt());
        }

        /// <summary>
        /// 创建随机枚举值
        /// </summary>
        /// <returns></returns>
        private static TValueType enumUInt()
        {
            return TmphPub.TmphEnumCast<TValueType, uint>.FromInt(TmphRandom.CreateUInt());
        }

        /// <summary>
        /// 创建随机枚举值
        /// </summary>
        /// <returns></returns>
        private static TValueType enumLong()
        {
            return TmphPub.TmphEnumCast<TValueType, long>.FromInt(TmphRandom.CreateLong());
        }

        /// <summary>
        /// 创建随机枚举值
        /// </summary>
        /// <returns></returns>
        private static TValueType enumULong()
        {
            return TmphPub.TmphEnumCast<TValueType, ulong>.FromInt(TmphRandom.CreateULong());
        }

        static TmphRandom()
        {
            Type type = typeof(TValueType);
            MethodInfo method = TmphRandom.GetMethod(type);
            if (method != null)
            {
                defaultCreator = (Func<TValueType>)Delegate.CreateDelegate(typeof(Func<TValueType>), method);
                return;
            }
            if ((method = TmphRandom.GetConfigMethod(type)) != null)
            {
                configCreator = (Func<TmphRandom.TmphConfig, TValueType>)Delegate.CreateDelegate(typeof(Func<TmphRandom.TmphConfig, TValueType>), method);
                configNullCreator = (Func<TmphRandom.TmphConfig, TValueType>)Delegate.CreateDelegate(typeof(Func<TmphRandom.TmphConfig, TValueType>), TmphRandom.GetConfigNullMethod(type));
                return;
            }
            if (type.IsArray)
            {
                if (type.GetArrayRank() == 1)
                {
                    configNullCreator = configCreator = (Func<TmphRandom.TmphConfig, TValueType>)Delegate.CreateDelegate(typeof(Func<TmphRandom.TmphConfig, TValueType>), TmphRandom.CreateArrayMethod.MakeGenericMethod(type.GetElementType()));
                }
                return;
            }
            if (type.IsEnum)
            {
                Type TEnumType = System.Enum.GetUnderlyingType(type);
                if (TEnumType == typeof(uint)) defaultCreator = enumUInt;
                else if (TEnumType == typeof(byte)) defaultCreator = enumByte;
                else if (TEnumType == typeof(ulong)) defaultCreator = enumULong;
                else if (TEnumType == typeof(ushort)) defaultCreator = enumUShort;
                else if (TEnumType == typeof(long)) defaultCreator = enumLong;
                else if (TEnumType == typeof(short)) defaultCreator = enumShort;
                else if (TEnumType == typeof(sbyte)) defaultCreator = enumSByte;
                else defaultCreator = enumInt;
                return;
            }
            if (type.IsGenericType)
            {
                Type genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(Nullable<>))
                {
                    configNullCreator = configCreator = (Func<TmphRandom.TmphConfig, TValueType>)Delegate.CreateDelegate(typeof(Func<TmphRandom.TmphConfig, TValueType>), TmphRandom.CreateNullableMethod.MakeGenericMethod(type.GetGenericArguments()));
                    return;
                }
                if (genericType == typeof(TmphSubArray<>))
                {
                    configNullCreator = configCreator = (Func<TmphRandom.TmphConfig, TValueType>)Delegate.CreateDelegate(typeof(Func<TmphRandom.TmphConfig, TValueType>), TmphRandom.CreateSubArrayMethod.MakeGenericMethod(type.GetGenericArguments()));
                    return;
                }
                if (genericType == typeof(TmphList<>))
                {
                    Type[] parameterTypes = type.GetGenericArguments();
                    configNullCreator = (Func<TmphRandom.TmphConfig, TValueType>)Delegate.CreateDelegate(typeof(Func<TmphRandom.TmphConfig, TValueType>), TmphRandom.CreateListNullMethod.MakeGenericMethod(parameterTypes));
                    configCreator = (Func<TmphRandom.TmphConfig, TValueType>)Delegate.CreateDelegate(typeof(Func<TmphRandom.TmphConfig, TValueType>), TmphRandom.CreateListMethod.MakeGenericMethod(parameterTypes));
                    return;
                }
                if (genericType == typeof(List<>))
                {
                    Type[] parameterTypes = type.GetGenericArguments();
                    configNullCreator = (Func<TmphRandom.TmphConfig, TValueType>)Delegate.CreateDelegate(typeof(Func<TmphRandom.TmphConfig, TValueType>), TmphRandom.CreateSystemListNullMethod.MakeGenericMethod(parameterTypes));
                    configCreator = (Func<TmphRandom.TmphConfig, TValueType>)Delegate.CreateDelegate(typeof(Func<TmphRandom.TmphConfig, TValueType>), TmphRandom.CreateSystemListMethod.MakeGenericMethod(parameterTypes));
                    return;
                }
                if (genericType == typeof(HashSet<>) || genericType == typeof(Queue<>) || genericType == typeof(Stack<>) || genericType == typeof(SortedSet<>) || genericType == typeof(LinkedList<>))
                {
                    Type[] parameterTypes = new Type[] { type, type.GetGenericArguments()[0] };
                    configNullCreator = (Func<TmphRandom.TmphConfig, TValueType>)Delegate.CreateDelegate(typeof(Func<TmphRandom.TmphConfig, TValueType>), TmphRandom.CreateEnumerableConstructorNullMethod.MakeGenericMethod(parameterTypes));
                    configCreator = (Func<TmphRandom.TmphConfig, TValueType>)Delegate.CreateDelegate(typeof(Func<TmphRandom.TmphConfig, TValueType>), TmphRandom.CreateEnumerableConstructorMethod.MakeGenericMethod(parameterTypes));
                    return;
                }
                if (genericType == typeof(Dictionary<,>))
                {
                    Type[] parameterTypes = type.GetGenericArguments();
                    configNullCreator = (Func<TmphRandom.TmphConfig, TValueType>)Delegate.CreateDelegate(typeof(Func<TmphRandom.TmphConfig, TValueType>), TmphRandom.CreateDictionaryNullMethod.MakeGenericMethod(parameterTypes));
                    configCreator = (Func<TmphRandom.TmphConfig, TValueType>)Delegate.CreateDelegate(typeof(Func<TmphRandom.TmphConfig, TValueType>), TmphRandom.CreateDictionaryMethod.MakeGenericMethod(parameterTypes));
                    return;
                }
                if (genericType == typeof(SortedDictionary<,>))
                {
                    Type[] parameterTypes = type.GetGenericArguments();
                    configNullCreator = (Func<TmphRandom.TmphConfig, TValueType>)Delegate.CreateDelegate(typeof(Func<TmphRandom.TmphConfig, TValueType>), TmphRandom.CreateSortedDictionaryNullMethod.MakeGenericMethod(parameterTypes));
                    configCreator = (Func<TmphRandom.TmphConfig, TValueType>)Delegate.CreateDelegate(typeof(Func<TmphRandom.TmphConfig, TValueType>), TmphRandom.CreateSortedDictionaryMethod.MakeGenericMethod(parameterTypes));
                    return;
                }
                if (genericType == typeof(SortedList<,>))
                {
                    Type[] parameterTypes = type.GetGenericArguments();
                    configNullCreator = (Func<TmphRandom.TmphConfig, TValueType>)Delegate.CreateDelegate(typeof(Func<TmphRandom.TmphConfig, TValueType>), TmphRandom.CreateSortedListNullMethod.MakeGenericMethod(parameterTypes));
                    configCreator = (Func<TmphRandom.TmphConfig, TValueType>)Delegate.CreateDelegate(typeof(Func<TmphRandom.TmphConfig, TValueType>), TmphRandom.CreateSortedListMethod.MakeGenericMethod(parameterTypes));
                    return;
                }
            }
            if (type.IsPointer || type.IsInterface) return;
            isValueType = type.IsValueType;
            TmphRandom.TmphMemberDynamicMethod dynamicMethod = new TmphRandom.TmphMemberDynamicMethod(type);
            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                dynamicMethod.Push(field);
            }
            dynamicMethod.Base();
            MemberCreator = (TmphCreator)dynamicMethod.Create<TmphCreator>();
        }
    }
}