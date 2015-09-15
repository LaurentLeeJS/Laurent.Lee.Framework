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
using Laurent.Lee.CLB.Reflection;
using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TmphAjax = Laurent.Lee.CLB.Web.TmphAjax;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    ///     JSON序列化
    /// </summary>
    public sealed unsafe class TmphJsonSerializer
    {
        /// <summary>
        ///     警告提示状态
        /// </summary>
        public enum TmphWarning : byte
        {
            /// <summary>
            ///     正常
            /// </summary>
            None,

            /// <summary>
            ///     缺少循环引用设置函数名称
            /// </summary>
            LessSetLoop,

            /// <summary>
            ///     缺少循环引用获取函数名称
            /// </summary>
            LessGetLoop,

            /// <summary>
            ///     成员位图类型不匹配
            /// </summary>
            MemberMap
        }

        /// <summary>
        ///     值类型对象转换函数信息
        /// </summary>
        private static readonly MethodInfo nullableToJsonMethod = typeof(TmphJsonSerializer).GetMethod("nullableToJson",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     字典转换函数信息
        /// </summary>
        private static readonly MethodInfo keyValuePairToJsonMethod =
            typeof(TmphJsonSerializer).GetMethod("keyValuePairToJson", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     字典转换函数信息
        /// </summary>
        private static readonly MethodInfo classToJsonMethod = typeof(TmphJsonSerializer).GetMethod("classToJson",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     字典转换函数信息
        /// </summary>
        private static readonly MethodInfo memberToJsonMethod = typeof(TmphJsonSerializer).GetMethod("memberToJson",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     字典转换函数信息
        /// </summary>
        private static readonly MethodInfo nullableMemberToJsonMethod =
            typeof(TmphJsonSerializer).GetMethod("nullableMemberToJson", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     字典转换函数信息
        /// </summary>
        private static readonly MethodInfo enumToStringMethod = typeof(TmphJsonSerializer).GetMethod("enumToString",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     字典转换函数信息
        /// </summary>
        private static readonly MethodInfo toJsonObjectMethod = typeof(TmphJsonSerializer).GetMethod("toJsonObject",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     字典转换函数信息
        /// </summary>
        private static readonly MethodInfo arrayMethod = typeof(TmphJsonSerializer).GetMethod("array",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     字典转换函数信息
        /// </summary>
        private static readonly MethodInfo structEnumerableMethod =
            typeof(TmphJsonSerializer).GetMethod("structEnumerable", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     字典转换函数信息
        /// </summary>
        private static readonly MethodInfo enumerableMethod = typeof(TmphJsonSerializer).GetMethod("enumerable",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     字典转换函数信息
        /// </summary>
        private static readonly MethodInfo dictionaryMethod = typeof(TmphJsonSerializer).GetMethod("dictionary",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     字符串字典转换函数信息
        /// </summary>
        private static readonly MethodInfo stringDictionaryMethod =
            typeof(TmphJsonSerializer).GetMethod("stringDictionary", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     基类转换函数信息
        /// </summary>
        private static readonly MethodInfo baseToJsonMethod = typeof(TmphJsonSerializer).GetMethod("baseToJson",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     公共默认配置参数
        /// </summary>
        private static readonly TmphConfig defaultConfig = new TmphConfig { CheckLoopDepth = TmphAppSetting.JsonDepth };

        /// <summary>
        ///     未知类型对象转换JSON字符串
        /// </summary>
        private static TmphInterlocked.TmphDictionary<Type, Func<object, TmphConfig, string>> objectToJsons =
            new TmphInterlocked.TmphDictionary<Type, Func<object, TmphConfig, string>>(
                TmphDictionary.CreateOnly<Type, Func<object, TmphConfig, string>>());

        /// <summary>
        ///     未知类型对象转换JSON字符串函数信息
        /// </summary>
        private static readonly MethodInfo objectToJsonMethod = typeof(TmphJsonSerializer).GetMethod("objectToJson",
            BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(object), typeof(TmphConfig) }, null);

        /// <summary>
        ///     获取Json字符串输出缓冲区属性方法信息
        /// </summary>
        private static readonly FieldInfo jsonStreamField = typeof(TmphJsonSerializer).GetField("JsonStream",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     基本类型转换函数
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> toJsonMethods;

        /// <summary>
        ///     Json字符串输出缓冲区
        /// </summary>
        internal readonly TmphCharStream JsonStream = new TmphCharStream((char*)TmphPub.PuzzleValue, 1);

        /// <summary>
        ///     循环检测深度
        /// </summary>
        private int checkLoopDepth;

        /// <summary>
        ///     祖先节点集合
        /// </summary>
        private object[] forefather;

        /// <summary>
        ///     祖先节点数量
        /// </summary>
        private int forefatherCount;

        /// <summary>
        ///     是否调用循环引用处理函数
        /// </summary>
        private bool isLoopObject;

        /// <summary>
        ///     对象编号
        /// </summary>
        private Dictionary<TmphObjectReference, string> objectIndexs;

        /// <summary>
        ///     配置参数
        /// </summary>
        private TmphConfig toJsonConfig;

        static TmphJsonSerializer()
        {
            toJsonMethods = TmphDictionary.CreateOnly<Type, MethodInfo>();
            foreach (var method in typeof(TmphJsonSerializer).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (method.CustomAttribute<TmphToJsonMethod>() != null)
                {
                    toJsonMethods.Add(method.GetParameters()[0].ParameterType, method);
                }
            }
        }

        /// <summary>
        ///     对象转换JSON字符串
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="TmphConfig">配置参数</param>
        /// <returns>Json字符串</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private string toJson<TValueType>(TValueType value, TmphConfig TmphConfig)
        {
            toJsonConfig = TmphConfig;
            var TmphBuffer = TmphUnmanagedPool.StreamBuffers.Get();
            try
            {
                JsonStream.Reset((byte*)TmphBuffer.Char, TmphUnmanagedPool.StreamBuffers.Size);
                using (JsonStream)
                {
                    toJson(value);
                    return TmphAjax.FormatJavascript(JsonStream);
                }
            }
            finally
            {
                TmphUnmanagedPool.StreamBuffers.Push(ref TmphBuffer);
            }
        }

        /// <summary>
        ///     对象转换JSON字符串
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="jsonStream">Json输出缓冲区</param>
        /// <param name="TmphConfig">配置参数</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson<TValueType>(TValueType value, TmphCharStream jsonStream, TmphConfig TmphConfig)
        {
            toJsonConfig = TmphConfig;
            JsonStream.From(jsonStream);
            try
            {
                toJson(value);
            }
            finally
            {
                jsonStream.From(JsonStream);
            }
        }

        /// <summary>
        ///     对象转换JSON字符串
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="TmphConfig">配置参数</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson<TValueType>(TValueType value)
        {
            if (toJsonConfig.GetLoopObject == null || toJsonConfig.SetLoopObject == null)
            {
                if (toJsonConfig.GetLoopObject != null) toJsonConfig.Warning = TmphWarning.LessSetLoop;
                else if (toJsonConfig.SetLoopObject != null) toJsonConfig.Warning = TmphWarning.LessGetLoop;
                else toJsonConfig.Warning = TmphWarning.None;
                isLoopObject = false;
                if (toJsonConfig.CheckLoopDepth <= 0)
                {
                    checkLoopDepth = 0;
                    if (forefather == null) forefather = new object[sizeof(int)];
                }
                else checkLoopDepth = toJsonConfig.CheckLoopDepth;
            }
            else
            {
                isLoopObject = true;
                if (objectIndexs == null) objectIndexs = TmphDictionary<TmphObjectReference>.Create<string>();
                checkLoopDepth = toJsonConfig.CheckLoopDepth <= 0 ? TmphAppSetting.JsonDepth : toJsonConfig.CheckLoopDepth;
            }
            TmphTypeToJsoner<TValueType>.ToJson(this, value);
        }

        /// <summary>
        ///     进入对象节点
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <returns>是否继续处理对象</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private bool push<TValueType>(TValueType value)
        {
            if (checkLoopDepth == 0)
            {
                if (forefatherCount != 0)
                {
                    var count = forefatherCount;
                    object objectValue = value;
                    foreach (var arrayValue in forefather)
                    {
                        if (arrayValue == objectValue)
                        {
                            TmphAjax.WriteObject(JsonStream);
                            return false;
                        }
                        if (--count == 0) break;
                    }
                }
                if (forefatherCount == forefather.Length)
                {
                    var newValues = new object[forefatherCount << 1];
                    forefather.CopyTo(newValues, 0);
                    forefather = newValues;
                }
                forefather[forefatherCount++] = value;
            }
            else
            {
                if (--checkLoopDepth == 0) TmphLog.Default.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                if (isLoopObject)
                {
                    string index;
                    if (objectIndexs.TryGetValue(new TmphObjectReference { Value = value }, out index))
                    {
                        JsonStream.PrepLength(toJsonConfig.GetLoopObject.Length + index.Length + 2);
                        JsonStream.WriteNotNull(toJsonConfig.GetLoopObject);
                        JsonStream.Unsafer.Write('(');
                        JsonStream.WriteNotNull(index);
                        JsonStream.Unsafer.Write(')');
                        return false;
                    }
                    objectIndexs.Add(new TmphObjectReference { Value = value }, index = objectIndexs.Count.toString());
                    JsonStream.PrepLength(toJsonConfig.SetLoopObject.Length + index.Length + 4);
                    JsonStream.WriteNotNull(toJsonConfig.SetLoopObject);
                    JsonStream.Unsafer.Write('(');
                    JsonStream.WriteNotNull(index);
                    JsonStream.Unsafer.Write(',');
                }
            }
            return true;
        }

        /// <summary>
        ///     进入对象节点
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <returns>是否继续处理对象</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private bool pushArray<TValueType>(TValueType value)
        {
            if (checkLoopDepth == 0)
            {
                if (forefatherCount != 0)
                {
                    var count = forefatherCount;
                    object objectValue = value;
                    foreach (var arrayValue in forefather)
                    {
                        if (arrayValue == objectValue)
                        {
                            TmphAjax.WriteObject(JsonStream);
                            return false;
                        }
                        if (--count == 0) break;
                    }
                }
                if (forefatherCount == forefather.Length)
                {
                    var newValues = new object[forefatherCount << 1];
                    forefather.CopyTo(newValues, 0);
                    forefather = newValues;
                }
                forefather[forefatherCount++] = value;
            }
            else
            {
                if (--checkLoopDepth == 0) TmphLog.Default.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                if (isLoopObject)
                {
                    string index;
                    if (objectIndexs.TryGetValue(new TmphObjectReference { Value = value }, out index))
                    {
                        JsonStream.PrepLength(toJsonConfig.GetLoopObject.Length + index.Length + 5);
                        JsonStream.WriteNotNull(toJsonConfig.GetLoopObject);
                        JsonStream.Unsafer.Write('(');
                        JsonStream.WriteNotNull(index);
                        JsonStream.WriteNotNull(",[])");
                        return false;
                    }
                    objectIndexs.Add(new TmphObjectReference { Value = value }, index = objectIndexs.Count.toString());
                    JsonStream.PrepLength(toJsonConfig.SetLoopObject.Length + index.Length + 4);
                    JsonStream.WriteNotNull(toJsonConfig.SetLoopObject);
                    JsonStream.Unsafer.Write('(');
                    JsonStream.WriteNotNull(index);
                    JsonStream.Unsafer.Write(',');
                }
            }
            return true;
        }

        /// <summary>
        ///     退出对象节点
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void pop()
        {
            if (checkLoopDepth == 0) forefather[--forefatherCount] = null;
            else
            {
                ++checkLoopDepth;
                if (isLoopObject) JsonStream.Write(')');
            }
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        private void free()
        {
            toJsonConfig = null;
            if (objectIndexs != null) objectIndexs.Clear();
            if (forefatherCount != 0)
            {
                Array.Clear(forefather, 0, forefatherCount);
                forefatherCount = 0;
            }
            TmphTypePool<TmphJsonSerializer>.Push(this);
        }

        /// <summary>
        ///     逻辑值转换
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(bool value)
        {
            if (value)
            {
                JsonStream.PrepLength(4);
                var data = (byte*)JsonStream.CurrentChar;
                *(char*)data = 't';
                *(char*)(data + sizeof(char)) = 'r';
                *(char*)(data + sizeof(char) * 2) = 'u';
                *(char*)(data + sizeof(char) * 3) = 'e';
                JsonStream.Unsafer.AddLength(4);
            }
            else
            {
                JsonStream.PrepLength(5);
                var data = (byte*)JsonStream.CurrentChar;
                *(char*)data = 'f';
                *(char*)(data + sizeof(char)) = 'a';
                *(char*)(data + sizeof(char) * 2) = 'l';
                *(char*)(data + sizeof(char) * 3) = 's';
                *(char*)(data + sizeof(char) * 4) = 'e';
                JsonStream.Unsafer.AddLength(5);
            }
        }

        /// <summary>
        ///     逻辑值转换
        /// </summary>
        /// <param name="value">逻辑值</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(bool? value)
        {
            if (value.HasValue) toJson((bool)value);
            else TmphAjax.WriteNull(JsonStream);
        }

        /// <summary>
        ///     数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(byte value)
        {
            TmphAjax.ToString(value, JsonStream);
        }

        /// <summary>
        ///     数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(byte? value)
        {
            if (value.HasValue) TmphAjax.ToString((byte)value, JsonStream);
            else TmphAjax.WriteNull(JsonStream);
        }

        /// <summary>
        ///     数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(sbyte value)
        {
            TmphAjax.ToString(value, JsonStream);
        }

        /// <summary>
        ///     数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(sbyte? value)
        {
            if (value.HasValue) TmphAjax.ToString((sbyte)value, JsonStream);
            else TmphAjax.WriteNull(JsonStream);
        }

        /// <summary>
        ///     数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(short value)
        {
            TmphAjax.ToString(value, JsonStream);
        }

        /// <summary>
        ///     数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(short? value)
        {
            if (value.HasValue) TmphAjax.ToString((short)value, JsonStream);
            else TmphAjax.WriteNull(JsonStream);
        }

        /// <summary>
        ///     数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(ushort value)
        {
            TmphAjax.ToString(value, JsonStream);
        }

        /// <summary>
        ///     数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(ushort? value)
        {
            if (value.HasValue) TmphAjax.ToString((ushort)value, JsonStream);
            else TmphAjax.WriteNull(JsonStream);
        }

        /// <summary>
        ///     数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(int value)
        {
            TmphAjax.ToString(value, JsonStream);
        }

        /// <summary>
        ///     数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(int? value)
        {
            if (value.HasValue) TmphAjax.ToString((int)value, JsonStream);
            else TmphAjax.WriteNull(JsonStream);
        }

        /// <summary>
        ///     数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(uint value)
        {
            TmphAjax.ToString(value, JsonStream);
        }

        /// <summary>
        ///     数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(uint? value)
        {
            if (value.HasValue) TmphAjax.ToString((uint)value, JsonStream);
            else TmphAjax.WriteNull(JsonStream);
        }

        /// <summary>
        ///     数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(long value)
        {
            TmphAjax.ToString(value, JsonStream, toJsonConfig.IsMaxNumberToString);
        }

        /// <summary>
        ///     数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(long? value)
        {
            if (value.HasValue) TmphAjax.ToString((long)value, JsonStream, toJsonConfig.IsMaxNumberToString);
            else TmphAjax.WriteNull(JsonStream);
        }

        /// <summary>
        ///     数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(ulong value)
        {
            TmphAjax.ToString(value, JsonStream, toJsonConfig.IsMaxNumberToString);
        }

        /// <summary>
        ///     数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(ulong? value)
        {
            if (value.HasValue) TmphAjax.ToString((ulong)value, JsonStream, toJsonConfig.IsMaxNumberToString);
            else TmphAjax.WriteNull(JsonStream);
        }

        /// <summary>
        ///     数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(float value)
        {
            if (float.IsNaN(value)) TmphAjax.WriteNaN(JsonStream);
            else JsonStream.WriteNotNull(value.ToString());
        }

        /// <summary>
        ///     数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(float? value)
        {
            if (value.HasValue) toJson(value.Value);
            else TmphAjax.WriteNull(JsonStream);
        }

        /// <summary>
        ///     数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(double value)
        {
            if (double.IsNaN(value)) TmphAjax.WriteNaN(JsonStream);
            else JsonStream.WriteNotNull(value.ToString());
        }

        /// <summary>
        ///     数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(double? value)
        {
            if (value.HasValue) toJson(value.Value);
            else TmphAjax.WriteNull(JsonStream);
        }

        /// <summary>
        ///     数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(decimal value)
        {
            JsonStream.WriteNotNull(value.ToString());
        }

        /// <summary>
        ///     数字转换
        /// </summary>
        /// <param name="value">数字</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(decimal? value)
        {
            if (value.HasValue) JsonStream.WriteNotNull(((decimal)value).ToString());
            else TmphAjax.WriteNull(JsonStream);
        }

        /// <summary>
        ///     字符转换
        /// </summary>
        /// <param name="value">字符</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(char value)
        {
            JsonStream.PrepLength(3);
            var data = (byte*)JsonStream.CurrentChar;
            *(char*)data = TmphAjax.Quote;
            *(char*)(data + sizeof(char)) = value == TmphAjax.Quote ? ' ' : value;
            *(char*)(data + sizeof(char) * 2) = TmphAjax.Quote;
            JsonStream.Unsafer.AddLength(3);
        }

        /// <summary>
        ///     字符转换
        /// </summary>
        /// <param name="value">字符</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(char? value)
        {
            if (value.HasValue) toJson((char)value);
            else TmphAjax.WriteNull(JsonStream);
        }

        /// <summary>
        ///     时间转换
        /// </summary>
        /// <param name="value">时间</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(DateTime value)
        {
            if (toJsonConfig.IsDateTimeMinNull) TmphAjax.ToString(value, JsonStream);
            else if (toJsonConfig.IsDateTimeToString)
            {
                JsonStream.PrepLength(TmphDate.SqlMillisecondSize + 2);
                JsonStream.Unsafer.Write('"');
                TmphDate.ToSqlMillisecond(value, JsonStream);
                JsonStream.Unsafer.Write('"');
            }
            else TmphAjax.ToStringNotNull(value, JsonStream);
        }

        /// <summary>
        ///     时间转换
        /// </summary>
        /// <param name="value">时间</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(DateTime? value)
        {
            if (value.HasValue) toJson((DateTime)value);
            else TmphAjax.WriteNull(JsonStream);
        }

        /// <summary>
        ///     Guid转换
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(Guid value)
        {
            TmphAjax.ToString(value, JsonStream);
        }

        /// <summary>
        ///     Guid转换
        /// </summary>
        /// <param name="value">Guid</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(Guid? value)
        {
            if (value.HasValue) toJson((Guid)value);
            else TmphAjax.WriteNull(JsonStream);
        }

        /// <summary>
        ///     字符串转换
        /// </summary>
        /// <param name="value">字符串</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(string value)
        {
            if (value == null) TmphAjax.WriteNull(JsonStream);
            else
            {
                fixed (char* valueFixed = value) toJson(valueFixed, value.Length);
            }
        }

        /// <summary>
        ///     字符串转换
        /// </summary>
        /// <param name="value">字符串</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(TmphSubString value)
        {
            if (value.value == null) TmphAjax.WriteNull(JsonStream);
            else
            {
                fixed (char* valueFixed = value.value) toJson(valueFixed + value.StartIndex, value.Length);
            }
        }

        /// <summary>
        ///     字符串转换
        /// </summary>
        /// <param name="value">JSON节点</param>
        [TmphToJsonMethod]
        private void toJson(TmphJsonNode value)
        {
            switch (value.Type)
            {
                case TmphJsonNode.TmphType.Null:
                    TmphAjax.WriteNull(JsonStream);
                    return;

                case TmphJsonNode.TmphType.Dictionary:
                    var dictionary = value.Dictionary;
                    JsonStream.Write('{');
                    if (dictionary.Count != 0)
                    {
                        var count = dictionary.Count;
                        foreach (var keyValue in dictionary.array)
                        {
                            if (count != dictionary.Count) JsonStream.Write(',');
                            toJson(keyValue.Key);
                            JsonStream.Write(':');
                            toJson(keyValue.Value);
                            if (--count == 0) break;
                        }
                    }
                    JsonStream.Write('}');
                    return;

                case TmphJsonNode.TmphType.List:
                    var list = value.List;
                    JsonStream.Write('[');
                    if (list.Count != 0)
                    {
                        var count = list.Count;
                        foreach (var node in list.array)
                        {
                            if (count != list.Count) JsonStream.Write(',');
                            toJson(node);
                            if (--count == 0) break;
                        }
                    }
                    JsonStream.Write(']');
                    return;

                case TmphJsonNode.TmphType.String:
                    var subString = value.String;
                    fixed (char* valueFixed = subString.value)
                        toJson(valueFixed + subString.StartIndex, subString.Length);
                    return;

                case TmphJsonNode.TmphType.QuoteString:
                    JsonStream.PrepLength(value.String.Length + 2);
                    JsonStream.Unsafer.Write((char)value.Int64);
                    JsonStream.Write(value.String);
                    JsonStream.Unsafer.Write((char)value.Int64);
                    return;

                case TmphJsonNode.TmphType.NumberString:
                    if ((int)value.Int64 == 0) JsonStream.Write(value.String);
                    else
                    {
                        JsonStream.PrepLength(value.String.Length + 2);
                        JsonStream.Unsafer.Write((char)value.Int64);
                        JsonStream.Write(value.String);
                        JsonStream.Unsafer.Write((char)value.Int64);
                    }
                    return;

                case TmphJsonNode.TmphType.Bool:
                    toJson((byte)value.Int64 != 0);
                    return;

                case TmphJsonNode.TmphType.DateTimeTick:
                    toJson(new DateTime(value.Int64));
                    return;

                case TmphJsonNode.TmphType.NaN:
                    TmphAjax.WriteNaN(JsonStream);
                    return;
            }
        }

        /// <summary>
        ///     字符串转换
        /// </summary>
        /// <param name="value">字符串</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(object value)
        {
            if (value == null) TmphAjax.WriteNull(JsonStream);
            else if (toJsonConfig.IsObject)
            {
                var type = value.GetType();
                if (type == typeof(object)) TmphAjax.WriteObject(JsonStream);
                else TmphTypeToJsoner.GetObjectToJsoner(type)(this, value);
            }
            else TmphAjax.WriteObject(JsonStream);
        }

        /// <summary>
        ///     字符串转换
        /// </summary>
        /// <param name="start">起始位置</param>
        /// <param name="length">字符串长度</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(char* start, int length)
        {
            JsonStream.PrepLength(length + 2);
            var data = JsonStream.CurrentChar;
            *data = TmphAjax.Quote;
            for (var end = start + length; start != end; ++start) *++data = *start == TmphAjax.Quote ? ' ' : *start;
            *(data + 1) = TmphAjax.Quote;
            JsonStream.Unsafer.AddLength(length + 2);
        }

        /// <summary>
        ///     类型转换
        /// </summary>
        /// <param name="type">类型</param>
        [TmphToJsonMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void toJson(Type type)
        {
            if (type == null) TmphAjax.WriteNull(JsonStream);
            else TmphTypeToJsoner<TmphRemoteType>.ToJson(this, new TmphRemoteType(type));
        }

        /// <summary>
        ///     值类型对象转换JSON字符串
        /// </summary>
        /// <param name="toJsoner">对象转换JSON字符串</param>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void nullableToJson<TValueType>(TmphJsonSerializer toJsoner, TValueType? value)
            where TValueType : struct
        {
            if (value.HasValue) TmphTypeToJsoner<TValueType>.StructToJson(toJsoner, value.Value);
            else TmphAjax.WriteNull(toJsoner.JsonStream);
        }

        /// <summary>
        ///     字典转换JSON字符串
        /// </summary>
        /// <param name="toJsoner">对象转换JSON字符串</param>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void keyValuePairToJson<keyValue, TValueType>(TmphJsonSerializer toJsoner,
            KeyValuePair<keyValue, TValueType> value)
        {
            TmphTypeToJsoner<keyValue>.KeyValuePair(toJsoner, value);
        }

        /// <summary>
        ///     引用类型对象转换JSON字符串
        /// </summary>
        /// <param name="toJsoner">对象转换JSON字符串</param>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void classToJson<TValueType>(TmphJsonSerializer toJsoner, TValueType value)
        {
            if (value == null) TmphAjax.WriteNull(toJsoner.JsonStream);
            else TmphTypeToJsoner<TValueType>.ClassToJson(toJsoner, value);
        }

        /// <summary>
        ///     值类型对象转换JSON字符串
        /// </summary>
        /// <param name="toJsoner">对象转换JSON字符串</param>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void memberToJson<TValueType>(TmphJsonSerializer toJsoner, TValueType value)
        {
            TmphTypeToJsoner<TValueType>.MemberToJson(toJsoner, value);
        }

        /// <summary>
        ///     值类型对象转换JSON字符串
        /// </summary>
        /// <param name="toJsoner">对象转换JSON字符串</param>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void nullableMemberToJson<TValueType>(TmphJsonSerializer toJsoner, TValueType? value)
            where TValueType : struct
        {
            if (value.HasValue) TmphTypeToJsoner<TValueType>.MemberToJson(toJsoner, value.Value);
            else TmphAjax.WriteNull(toJsoner.JsonStream);
        }

        /// <summary>
        ///     字符串转换
        /// </summary>
        /// <param name="toJsoner">对象转换JSON字符串</param>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void enumToString<TValueType>(TmphJsonSerializer toJsoner, TValueType value)
        {
            var stringValue = value.ToString();
            var charValue = stringValue[0];
            if ((uint)(charValue - '1') < 10 || charValue == '-') toJsoner.JsonStream.WriteNotNull(stringValue);
            else TmphAjax.ToString(stringValue, toJsoner.JsonStream);
        }

        /// <summary>
        ///     object转换JSON字符串
        /// </summary>
        /// <param name="toJsoner">对象转换JSON字符串</param>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void toJsonObject<TValueType>(TmphJsonSerializer toJsoner, object value)
        {
            TmphTypeToJsoner<TValueType>.ToJson(toJsoner, (TValueType)value);
        }

        /// <summary>
        ///     数组转换
        /// </summary>
        /// <param name="array">数组对象</param>
        private void array<TValueType>(TValueType[] array)
        {
            if (array == null) TmphAjax.WriteNull(JsonStream);
            else if (push(array))
            {
                TmphTypeToJsoner<TValueType>.Array(this, array);
                pop();
            }
        }

        /// <summary>
        ///     枚举集合转换
        /// </summary>
        /// <param name="values">枚举集合</param>
        private static void structEnumerable<TValueType, elementType>(TmphJsonSerializer serializer, TValueType value)
            where TValueType : IEnumerable<elementType>
        {
            TmphTypeToJsoner<elementType>.Enumerable(serializer, value);
        }

        /// <summary>
        ///     枚举集合转换
        /// </summary>
        /// <param name="values">枚举集合</param>
        private void enumerable<TValueType, elementType>(TValueType value) where TValueType : IEnumerable<elementType>
        {
            if (value == null) TmphAjax.WriteNull(JsonStream);
            else if (pushArray(value))
            {
                TmphTypeToJsoner<elementType>.Enumerable(this, value);
                pop();
            }
        }

        /// <summary>
        ///     字典转换
        /// </summary>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void dictionary<TValueType, dictionaryValueType>(Dictionary<TValueType, dictionaryValueType> dictionary)
        {
            if (dictionary == null) TmphAjax.WriteNull(JsonStream);
            else if (push(dictionary))
            {
                TmphTypeToJsoner<TValueType>.Dictionary(this, dictionary);
                pop();
            }
        }

        /// <summary>
        ///     字典转换
        /// </summary>
        /// <param name="dictionary">字典</param>
        private void stringDictionary<TValueType>(Dictionary<string, TValueType> dictionary)
        {
            if (dictionary == null) TmphAjax.WriteNull(JsonStream);
            else if (push(dictionary))
            {
                if (toJsonConfig.IsStringDictionaryToObject)
                    TmphTypeToJsoner<TValueType>.StringDictionary(this, dictionary);
                else TmphTypeToJsoner<string>.Dictionary(this, dictionary);
                pop();
            }
        }

        /// <summary>
        ///     基类转换
        /// </summary>
        /// <param name="toJsoner">对象转换JSON字符串</param>
        /// <param name="value">数据对象</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void baseToJson<TValueType, childType>(TmphJsonSerializer toJsoner, childType value)
            where childType : TValueType
        {
            TmphTypeToJsoner<TValueType>.ClassToJson(toJsoner, value);
        }

        /// <summary>
        ///     序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        /// <param name="jsonStream"></param>
        /// <param name="stream"></param>
        /// <param name="TmphConfig"></param>
        internal static void Serialize<TValueType>(TValueType value, TmphCharStream jsonStream, TmphUnmanagedStream stream,
            TmphConfig TmphConfig)
        {
            ToJson(value, jsonStream, TmphConfig);
            stream.PrepLength(sizeof(int) + (jsonStream.Length << 1));
            stream.Unsafer.AddLength(sizeof(int));
            var index = stream.Length;
            TmphAjax.FormatJavascript(jsonStream, stream);
            var length = stream.Length - index;
            *(int*)(stream.Data + index - sizeof(int)) = length;
            if ((length & 2) != 0) stream.Write(' ');
        }

        /// <summary>
        ///     对象转换JSON字符串
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="jsonStream">Json输出缓冲区</param>
        /// <param name="TmphConfig">配置参数</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static void ToJson<TValueType>(TValueType value, TmphCharStream jsonStream, TmphConfig TmphConfig = null)
        {
            if (jsonStream == null) TmphLog.Default.Throw(TmphLog.TmphExceptionType.Null);
            var toJsoner = TmphTypePool<TmphJsonSerializer>.Pop() ?? new TmphJsonSerializer();
            try
            {
                toJsoner.toJson(value, jsonStream, TmphConfig ?? defaultConfig);
            }
            finally
            {
                toJsoner.free();
            }
        }

        /// <summary>
        ///     对象转换JSON字符串
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="TmphConfig">配置参数</param>
        /// <returns>Json字符串</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static string ToJson<TValueType>(TValueType value, TmphConfig TmphConfig = null)
        {
            var toJsoner = TmphTypePool<TmphJsonSerializer>.Pop() ?? new TmphJsonSerializer();
            try
            {
                return toJsoner.toJson(value, TmphConfig ?? defaultConfig);
            }
            finally
            {
                toJsoner.free();
            }
        }

        /// <summary>
        ///     未知类型对象转换JSON字符串
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="TmphConfig">配置参数</param>
        /// <returns>Json字符串</returns>
        private static string objectToJson<TValueType>(object value, TmphConfig TmphConfig)
        {
            var toJsoner = TmphTypePool<TmphJsonSerializer>.Pop() ?? new TmphJsonSerializer();
            try
            {
                return toJsoner.toJson((TValueType)value, TmphConfig ?? defaultConfig);
            }
            finally
            {
                toJsoner.free();
            }
        }

        /// <summary>
        ///     未知类型对象转换JSON字符串
        /// </summary>
        /// <param name="value">数据对象</param>
        /// <param name="TmphConfig">配置参数</param>
        /// <returns>Json字符串</returns>
        public static string ObjectToJson(object value, TmphConfig TmphConfig = null)
        {
            if (value == null) return TmphAjax.Null;
            var type = value.GetType();
            Func<object, TmphConfig, string> toJson;
            if (!objectToJsons.TryGetValue(type, out toJson))
            {
                objectToJsons.Set(type,
                    toJson =
                        (Func<object, TmphConfig, string>)
                            Delegate.CreateDelegate(typeof(Func<object, TmphConfig, string>),
                                objectToJsonMethod.MakeGenericMethod(type)));
            }
            return toJson(value, TmphConfig);
        }

        /// <summary>
        ///     获取基本类型转换函数
        /// </summary>
        /// <param name="type">基本类型</param>
        /// <returns>转换函数</returns>
        private static MethodInfo getToJsonMethod(Type type)
        {
            MethodInfo method;
            return toJsonMethods.TryGetValue(type, out method) ? method : null;
        }

        /// <summary>
        ///     配置参数
        /// </summary>
        public sealed class TmphConfig
        {
            /// <summary>
            ///     循环引用检测深度,0表示实时检测
            /// </summary>
            public int CheckLoopDepth;

            /// <summary>
            ///     循环引用获取函数名称
            /// </summary>
            public string GetLoopObject;

            /// <summary>
            ///     最小时间是否输出为null
            /// </summary>
            public bool IsDateTimeMinNull = true;

            /// <summary>
            ///     时间是否转换成字符串
            /// </summary>
            public bool IsDateTimeToString;

            /// <summary>
            ///     Dictionary是否转换成对象模式输出
            /// </summary>
            public bool IsDictionaryToObject;

            /// <summary>
            ///     超出最大有效精度的long/ulong是否转换成字符串
            /// </summary>
            public bool IsMaxNumberToString;

            /// <summary>
            ///     成员位图类型不匹配是否输出错误信息
            /// </summary>
            public bool IsMemberMapErrorLog = true;

            /// <summary>
            ///     成员位图类型不匹配时是否使用默认输出
            /// </summary>
            public bool IsMemberMapErrorToDefault = true;

            /// <summary>
            ///     是否将object转换成真实类型输出
            /// </summary>
            public bool IsObject;

            /// <summary>
            ///     Dictionary[string,]是否转换成对象输出
            /// </summary>
            public bool IsStringDictionaryToObject = true;

            /// <summary>
            ///     是否输出客户端视图绑定类型
            /// </summary>
            public bool IsViewClientType;

            /// <summary>
            ///     成员位图
            /// </summary>
            public TmphMemberMap MemberMap;

            /// <summary>
            ///     循环引用设置函数名称
            /// </summary>
            public string SetLoopObject;

            /// <summary>
            ///     警告提示状态
            /// </summary>
            public TmphWarning Warning { get; internal set; }
        }

        /// <summary>
        ///     基本转换类型
        /// </summary>
        private sealed class TmphToJsonMethod : Attribute
        {
        }

        /// <summary>
        ///     对象转换JSON字符串静态信息
        /// </summary>
        private static class TmphTypeToJsoner
        {
            /// <summary>
            ///     object转换调用委托信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, Action<TmphJsonSerializer, object>> objectToJsoners =
                new TmphInterlocked.TmphDictionary<Type, Action<TmphJsonSerializer, object>>(
                    TmphDictionary.CreateOnly<Type, Action<TmphJsonSerializer, object>>());

            /// <summary>
            ///     数组转换调用函数信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, MethodInfo> arrayToJsoners =
                new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

            /// <summary>
            ///     枚举集合转换调用函数信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, MethodInfo> enumerableToJsoners =
                new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

            /// <summary>
            ///     字典转换调用函数信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, MethodInfo> dictionaryToJsoners =
                new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

            /// <summary>
            ///     枚举转换调用函数信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, MethodInfo> enumToJsoners =
                new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

            /// <summary>
            ///     未知类型转换调用函数信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, MethodInfo> typeToJsoners =
                new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

            /// <summary>
            ///     自定义转换调用函数信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, MethodInfo> customToJsoners =
                new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

            /// <summary>
            ///     获取字段成员集合
            /// </summary>
            /// <param name="type"></param>
            /// <param name="typeAttribute">类型配置</param>
            /// <returns>字段成员集合</returns>
            public static TmphSubArray<TmphFieldIndex> GetFields(TmphFieldIndex[] fields, TmphJsonSerialize typeAttribute)
            {
                var values = new TmphSubArray<TmphFieldIndex>(fields.Length);
                foreach (var field in fields)
                {
                    var type = field.Member.FieldType;
                    if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !field.IsIgnore)
                    {
                        var attribute = field.GetAttribute<TmphJsonSerialize.TmphMember>(true, true);
                        if (typeAttribute.IsAllMember
                            ? (attribute == null || attribute.IsSetup)
                            : (attribute != null && attribute.IsSetup))
                            values.Add(field);
                    }
                }
                return values;
            }

            /// <summary>
            ///     获取属性成员集合
            /// </summary>
            /// <param name="type"></param>
            /// <param name="typeAttribute">类型配置</param>
            /// <param name="fields">字段成员集合</param>
            /// <returns>属性成员集合</returns>
            public static TmphSubArray<TmphKeyValue<TmphPropertyIndex, MethodInfo>> GetProperties(TmphPropertyIndex[] properties,
                TmphJsonSerialize typeAttribute)
            {
                var values = new TmphSubArray<TmphKeyValue<TmphPropertyIndex, MethodInfo>>(properties.Length);
                foreach (var property in properties)
                {
                    if (property.Member.CanRead)
                    {
                        var type = property.Member.PropertyType;
                        if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !property.IsIgnore)
                        {
                            var attribute = property.GetAttribute<TmphJsonSerialize.TmphMember>(true, true);
                            if (typeAttribute.IsAllMember
                                ? (attribute == null || attribute.IsSetup)
                                : (attribute != null && attribute.IsSetup))
                            {
                                var method = property.Member.GetGetMethod(true);
                                if (method != null && method.GetParameters().Length == 0)
                                    values.Add(new TmphKeyValue<TmphPropertyIndex, MethodInfo>(property, method));
                            }
                        }
                    }
                }
                return values;
            }

            /// <summary>
            ///     获取字段成员集合
            /// </summary>
            /// <param name="type"></param>
            /// <returns>字段成员集合</returns>
            public static TmphSubArray<TmphMemberIndex> GetMembers(TmphFieldIndex[] fieldIndexs, TmphPropertyIndex[] properties,
                TmphJsonSerialize typeAttribute)
            {
                var members = new TmphSubArray<TmphMemberIndex>(fieldIndexs.Length + properties.Length);
                foreach (var field in fieldIndexs)
                {
                    var type = field.Member.FieldType;
                    if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !field.IsIgnore)
                    {
                        var attribute = field.GetAttribute<TmphJsonSerialize.TmphMember>(true, true);
                        if (typeAttribute.IsAllMember
                            ? (attribute == null || attribute.IsSetup)
                            : (attribute != null && attribute.IsSetup))
                            members.Add(field);
                    }
                }
                foreach (var property in properties)
                {
                    if (property.Member.CanRead && property.Member.CanWrite)
                    {
                        var type = property.Member.PropertyType;
                        if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !property.IsIgnore)
                        {
                            var attribute = property.GetAttribute<TmphJsonSerialize.TmphMember>(true, true);
                            if (typeAttribute.IsAllMember
                                ? (attribute == null || attribute.IsSetup)
                                : (attribute != null && attribute.IsSetup))
                            {
                                var method = property.Member.GetGetMethod(true);
                                if (method != null && method.GetParameters().Length == 0) members.Add(property);
                            }
                        }
                    }
                }
                return members;
            }

            /// <summary>
            ///     获取成员转换函数信息
            /// </summary>
            /// <param name="type">成员类型</param>
            /// <returns>成员转换函数信息</returns>
            private static MethodInfo getMemberMethodInfo(Type type)
            {
                var methodInfo = getToJsonMethod(type);
                if (methodInfo != null) return methodInfo;
                if (type.IsArray) return GetArrayToJsoner(type.GetElementType());
                if (type.IsEnum) return GetEnumToJsoner(type);
                if (type.IsGenericType)
                {
                    var genericType = type.GetGenericTypeDefinition();
                    if (genericType == typeof(Dictionary<,>)) return GetDictionaryToJsoner(type);
                    if (genericType == typeof(Nullable<>))
                        return nullableToJsonMethod.MakeGenericMethod(type.GetGenericArguments());
                    if (genericType == typeof(KeyValuePair<,>))
                        return keyValuePairToJsonMethod.MakeGenericMethod(type.GetGenericArguments());
                }
                if ((methodInfo = GetCustomToJsoner(type)) != null) return methodInfo;
                if ((methodInfo = GetIEnumerableToJsoner(type)) != null) return methodInfo;
                return GetTypeToJsoner(type);
            }

            /// <summary>
            ///     获取object转换调用委托信息
            /// </summary>
            /// <param name="type">真实类型</param>
            /// <returns>object转换调用委托信息</returns>
            public static Action<TmphJsonSerializer, object> GetObjectToJsoner(Type type)
            {
                Action<TmphJsonSerializer, object> method;
                if (objectToJsoners.TryGetValue(type, out method)) return method;
                method =
                    (Action<TmphJsonSerializer, object>)
                        Delegate.CreateDelegate(typeof(Action<TmphJsonSerializer, object>),
                            toJsonObjectMethod.MakeGenericMethod(type));
                objectToJsoners.Set(type, method);
                return method;
            }

            /// <summary>
            ///     获取数组转换委托调用函数信息
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>数组转换委托调用函数信息</returns>
            public static MethodInfo GetArrayToJsoner(Type type)
            {
                MethodInfo method;
                if (arrayToJsoners.TryGetValue(type, out method)) return method;
                arrayToJsoners.Set(type, method = arrayMethod.MakeGenericMethod(type));
                return method;
            }

            /// <summary>
            ///     获取枚举集合转换委托调用函数信息
            /// </summary>
            /// <param name="type">枚举类型</param>
            /// <returns>枚举集合转换委托调用函数信息</returns>
            public static MethodInfo GetIEnumerableToJsoner(Type type)
            {
                MethodInfo method;
                if (enumerableToJsoners.TryGetValue(type, out method)) return method;
                foreach (var interfaceType in type.GetInterfaces())
                {
                    if (interfaceType.IsGenericType)
                    {
                        var genericType = interfaceType.GetGenericTypeDefinition();
                        if (genericType == typeof(IEnumerable<>))
                        {
                            var parameters = interfaceType.GetGenericArguments();
                            var argumentType = parameters[0];
                            parameters[0] = typeof(IList<>).MakeGenericType(argumentType);
                            var constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    parameters, null);
                            if (constructorInfo != null)
                            {
                                method =
                                    (type.IsValueType ? structEnumerableMethod : enumerableMethod).MakeGenericMethod(
                                        type, argumentType);
                                break;
                            }
                            parameters[0] = typeof(ICollection<>).MakeGenericType(argumentType);
                            constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    parameters, null);
                            if (constructorInfo != null)
                            {
                                method =
                                    (type.IsValueType ? structEnumerableMethod : enumerableMethod).MakeGenericMethod(
                                        type, argumentType);
                                break;
                            }
                            parameters[0] = typeof(IEnumerable<>).MakeGenericType(argumentType);
                            constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    parameters, null);
                            if (constructorInfo != null)
                            {
                                method =
                                    (type.IsValueType ? structEnumerableMethod : enumerableMethod).MakeGenericMethod(
                                        type, argumentType);
                                break;
                            }
                            parameters[0] = argumentType.MakeArrayType();
                            constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    parameters, null);
                            if (constructorInfo != null)
                            {
                                method =
                                    (type.IsValueType ? structEnumerableMethod : enumerableMethod).MakeGenericMethod(
                                        type, argumentType);
                                break;
                            }
                        }
                        else if (genericType == typeof(IDictionary<,>))
                        {
                            var constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    new[] { interfaceType }, null);
                            if (constructorInfo != null)
                            {
                                method =
                                    (type.IsValueType ? structEnumerableMethod : enumerableMethod).MakeGenericMethod(
                                        type,
                                        typeof(KeyValuePair<,>).MakeGenericType(interfaceType.GetGenericArguments()));
                                break;
                            }
                        }
                    }
                }
                enumerableToJsoners.Set(type, method);
                return method;
            }

            /// <summary>
            ///     获取字典转换委托调用函数信息
            /// </summary>
            /// <param name="type">枚举类型</param>
            /// <returns>字典转换委托调用函数信息</returns>
            public static MethodInfo GetDictionaryToJsoner(Type type)
            {
                MethodInfo method;
                if (dictionaryToJsoners.TryGetValue(type, out method)) return method;
                var types = type.GetGenericArguments();
                if (types[0] == typeof(string)) method = stringDictionaryMethod.MakeGenericMethod(types[1]);
                else method = dictionaryMethod.MakeGenericMethod(types);
                dictionaryToJsoners.Set(type, method);
                return method;
            }

            /// <summary>
            ///     获取枚举转换委托调用函数信息
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>枚举转换委托调用函数信息</returns>
            public static MethodInfo GetEnumToJsoner(Type type)
            {
                MethodInfo method;
                if (enumToJsoners.TryGetValue(type, out method)) return method;
                enumToJsoners.Set(type, method = enumToStringMethod.MakeGenericMethod(type));
                return method;
            }

            /// <summary>
            ///     未知类型枚举转换委托调用函数信息
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>未知类型转换委托调用函数信息</returns>
            public static MethodInfo GetTypeToJsoner(Type type)
            {
                MethodInfo method;
                if (typeToJsoners.TryGetValue(type, out method)) return method;
                if (type.IsValueType)
                {
                    var nullType = type.nullableType();
                    if (nullType == null) method = memberToJsonMethod.MakeGenericMethod(type);
                    else method = nullableMemberToJsonMethod.MakeGenericMethod(nullType);
                }
                else method = classToJsonMethod.MakeGenericMethod(type);
                typeToJsoners.Set(type, method);
                return method;
            }

            /// <summary>
            ///     自定义枚举转换委托调用函数信息
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>自定义转换委托调用函数信息</returns>
            public static MethodInfo GetCustomToJsoner(Type type)
            {
                MethodInfo method;
                if (customToJsoners.TryGetValue(type, out method)) return method;
                foreach (var methodInfo in TmphAttributeMethod.GetStatic(type))
                {
                    if (methodInfo.Method.ReturnType == typeof(void))
                    {
                        var parameters = methodInfo.Method.GetParameters();
                        if (parameters.Length == 2 && parameters[0].ParameterType == typeof(TmphJsonSerializer) &&
                            parameters[1].ParameterType == type)
                        {
                            if (methodInfo.GetAttribute<TmphJsonSerialize.TmphCustom>(true) != null)
                            {
                                method = methodInfo.Method;
                                break;
                            }
                        }
                    }
                }
                customToJsoners.Set(type, method);
                return method;
            }

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
                ///     是否第一个字段
                /// </summary>
                private byte isFirstMember;

                /// <summary>
                ///     动态函数
                /// </summary>
                /// <param name="type"></param>
                /// <param name="name">成员类型</param>
                public TmphMemberDynamicMethod(Type type)
                {
                    dynamicMethod = new DynamicMethod("jsonSerializer", null, new[] { typeof(TmphJsonSerializer), type },
                        type, true);
                    generator = dynamicMethod.GetILGenerator();
                    generator.DeclareLocal(typeof(TmphCharStream));

                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldfld, jsonStreamField);
                    generator.Emit(OpCodes.Stloc_0);

                    isFirstMember = 1;
                    isValueType = type.IsValueType;
                }

                /// <summary>
                ///     添加成员
                /// </summary>
                /// <param name="name">成员名称</param>
                private void push(string name)
                {
                    if (isFirstMember == 0)
                    {
                        generator.Emit(OpCodes.Ldloc_0);
                        generator.Emit(OpCodes.Ldstr, "," + TmphAjax.QuoteString + name + TmphAjax.QuoteString + ":");
                        generator.Emit(OpCodes.Call, TmphPub.CharStreamWriteNotNullMethod);
                    }
                    else
                    {
                        generator.Emit(OpCodes.Ldloc_0);
                        generator.Emit(OpCodes.Ldstr, TmphAjax.QuoteString + name + TmphAjax.QuoteString + ":");
                        generator.Emit(OpCodes.Call, TmphPub.CharStreamWriteNotNullMethod);

                        isFirstMember = 0;
                    }
                    generator.Emit(OpCodes.Ldarg_0);
                    if (isValueType) generator.Emit(OpCodes.Ldarga_S, 1);
                    else generator.Emit(OpCodes.Ldarg_1);
                }

                /// <summary>
                ///     添加字段
                /// </summary>
                /// <param name="field">字段信息</param>
                public void Push(TmphFieldIndex field)
                {
                    push(field.Member.Name);
                    generator.Emit(OpCodes.Ldfld, field.Member);
                    var method = getMemberMethodInfo(field.Member.FieldType);
                    generator.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);
                }

                /// <summary>
                ///     添加属性
                /// </summary>
                /// <param name="property">属性信息</param>
                /// <param name="method">函数信息</param>
                public void Push(TmphPropertyIndex property, MethodInfo method)
                {
                    push(property.Member.Name);
                    generator.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);
                    method = getMemberMethodInfo(property.Member.PropertyType);
                    generator.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);
                }

                /// <summary>
                ///     创建成员转换委托
                /// </summary>
                /// <returns>成员转换委托</returns>
                public Delegate Create<delegateType>()
                {
                    generator.Emit(OpCodes.Ret);
                    return dynamicMethod.CreateDelegate(typeof(delegateType));
                }
            }

            /// <summary>
            ///     动态函数
            /// </summary>
            public struct TmphMemberMapDynamicMethod
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
                /// <param name="name">成员类型</param>
                public TmphMemberMapDynamicMethod(Type type)
                {
                    dynamicMethod = new DynamicMethod("jsonMemberMapSerializer", null,
                        new[] { typeof(TmphMemberMap), typeof(TmphJsonSerializer), type, typeof(TmphCharStream) }, type, true);
                    generator = dynamicMethod.GetILGenerator();

                    generator.DeclareLocal(typeof(int));
                    generator.Emit(OpCodes.Ldc_I4_0);
                    generator.Emit(OpCodes.Stloc_0);

                    isValueType = type.IsValueType;
                }

                /// <summary>
                ///     添加成员
                /// </summary>
                /// <param name="name">成员名称</param>
                private void push(string name, int memberIndex, Label end)
                {
                    Label next = generator.DefineLabel(), value = generator.DefineLabel();
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4, memberIndex);
                    generator.Emit(OpCodes.Callvirt, TmphPub.MemberMapIsMemberMethod);
                    generator.Emit(OpCodes.Brfalse_S, end);

                    generator.Emit(OpCodes.Ldloc_0);
                    generator.Emit(OpCodes.Brtrue_S, next);

                    generator.Emit(OpCodes.Ldc_I4_1);
                    generator.Emit(OpCodes.Stloc_0);
                    generator.Emit(OpCodes.Ldarg_3);
                    generator.Emit(OpCodes.Ldstr, TmphAjax.QuoteString + name + TmphAjax.QuoteString + ":");
                    generator.Emit(OpCodes.Call, TmphPub.CharStreamWriteNotNullMethod);
                    generator.Emit(OpCodes.Br_S, value);

                    generator.MarkLabel(next);
                    generator.Emit(OpCodes.Ldarg_3);
                    generator.Emit(OpCodes.Ldstr, "," + TmphAjax.QuoteString + name + TmphAjax.QuoteString + ":");
                    generator.Emit(OpCodes.Call, TmphPub.CharStreamWriteNotNullMethod);

                    generator.MarkLabel(value);
                    generator.Emit(OpCodes.Ldarg_1);
                    if (isValueType) generator.Emit(OpCodes.Ldarga_S, 2);
                    else generator.Emit(OpCodes.Ldarg_2);
                }

                /// <summary>
                ///     添加字段
                /// </summary>
                /// <param name="field">字段信息</param>
                public void Push(TmphFieldIndex field)
                {
                    var end = generator.DefineLabel();
                    push(field.Member.Name, field.MemberIndex, end);
                    generator.Emit(OpCodes.Ldfld, field.Member);
                    var method = getMemberMethodInfo(field.Member.FieldType);
                    generator.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);
                    generator.MarkLabel(end);
                }

                /// <summary>
                ///     添加属性
                /// </summary>
                /// <param name="property">属性信息</param>
                /// <param name="method">函数信息</param>
                public void Push(TmphPropertyIndex property, MethodInfo method)
                {
                    var end = generator.DefineLabel();
                    push(property.Member.Name, property.MemberIndex, end);
                    generator.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);
                    method = getMemberMethodInfo(property.Member.PropertyType);
                    generator.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);
                    generator.MarkLabel(end);
                }

                /// <summary>
                ///     创建成员转换委托
                /// </summary>
                /// <returns>成员转换委托</returns>
                public Delegate Create<delegateType>()
                {
                    generator.Emit(OpCodes.Ret);
                    return dynamicMethod.CreateDelegate(typeof(delegateType));
                }
            }
        }

        /// <summary>
        ///     对象转换JSON字符串
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        internal static class TmphTypeToJsoner<TValueType>
        {
            /// <summary>
            ///     成员转换
            /// </summary>
            private static readonly Action<TmphJsonSerializer, TValueType> memberToJsoner;

            /// <summary>
            ///     成员转换
            /// </summary>
            private static readonly Action<TmphMemberMap, TmphJsonSerializer, TValueType, TmphCharStream> memberMapToJsoner;

            /// <summary>
            ///     转换委托
            /// </summary>
            private static readonly Action<TmphJsonSerializer, TValueType> defaultToJsoner;

            /// <summary>
            ///     JSON序列化类型配置
            /// </summary>
            private static readonly TmphJsonSerialize attribute;

            /// <summary>
            ///     客户端视图类型名称
            /// </summary>
            private static readonly string viewClientTypeName;

            /// <summary>
            ///     是否值类型
            /// </summary>
            private static readonly bool isValueType;

            static TmphTypeToJsoner()
            {
                var type = typeof(TValueType);
                var methodInfo = getToJsonMethod(type);
                if (methodInfo != null)
                {
                    var dynamicMethod = new DynamicMethod("toJsoner", typeof(void),
                        new[] { typeof(TmphJsonSerializer), type }, true);
                    dynamicMethod.InitLocals = true;
                    var generator = dynamicMethod.GetILGenerator();
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Call, methodInfo);
                    generator.Emit(OpCodes.Ret);
                    defaultToJsoner =
                        (Action<TmphJsonSerializer, TValueType>)
                            dynamicMethod.CreateDelegate(typeof(Action<TmphJsonSerializer, TValueType>));
                    isValueType = true;
                    return;
                }
                if (type.IsArray)
                {
                    if (type.GetArrayRank() == 1)
                        defaultToJsoner =
                            (Action<TmphJsonSerializer, TValueType>)
                                Delegate.CreateDelegate(typeof(Action<TmphJsonSerializer, TValueType>),
                                    TmphTypeToJsoner.GetArrayToJsoner(type.GetElementType()));
                    else defaultToJsoner = arrayManyRank;
                    isValueType = true;
                    return;
                }
                if (type.IsEnum)
                {
                    defaultToJsoner = enumToString;
                    isValueType = true;
                    return;
                }
                if (type.IsPointer)
                {
                    defaultToJsoner = toNull;
                    isValueType = true;
                    return;
                }
                if (type.IsGenericType)
                {
                    var genericType = type.GetGenericTypeDefinition();
                    if (genericType == typeof(Dictionary<,>))
                    {
                        defaultToJsoner =
                            (Action<TmphJsonSerializer, TValueType>)
                                Delegate.CreateDelegate(typeof(Action<TmphJsonSerializer, TValueType>),
                                    TmphTypeToJsoner.GetDictionaryToJsoner(type));
                        isValueType = true;
                        return;
                    }
                    if (genericType == typeof(Nullable<>))
                    {
                        defaultToJsoner =
                            (Action<TmphJsonSerializer, TValueType>)
                                Delegate.CreateDelegate(typeof(Action<TmphJsonSerializer, TValueType>),
                                    nullableToJsonMethod.MakeGenericMethod(type.GetGenericArguments()));
                        isValueType = true;
                        return;
                    }
                    if (genericType == typeof(KeyValuePair<,>))
                    {
                        defaultToJsoner =
                            (Action<TmphJsonSerializer, TValueType>)
                                Delegate.CreateDelegate(typeof(Action<TmphJsonSerializer, TValueType>),
                                    keyValuePairToJsonMethod.MakeGenericMethod(type.GetGenericArguments()));
                        isValueType = true;
                        return;
                    }
                }
                if ((methodInfo = TmphTypeToJsoner.GetCustomToJsoner(type)) != null
                    || (methodInfo = TmphTypeToJsoner.GetIEnumerableToJsoner(type)) != null)
                {
                    defaultToJsoner =
                        (Action<TmphJsonSerializer, TValueType>)
                            Delegate.CreateDelegate(typeof(Action<TmphJsonSerializer, TValueType>), methodInfo);
                    isValueType = true;
                }
                else
                {
                    Type TAttributeType;
                    attribute = type.customAttribute<TmphJsonSerialize>(out TAttributeType, true) ??
                                TmphJsonSerialize.AllMember;
                    if (type.IsValueType) isValueType = true;
                    else if (attribute != TmphJsonSerialize.AllMember && TAttributeType != type)
                    {
                        for (var baseType = type.BaseType; baseType != typeof(object); baseType = baseType.BaseType)
                        {
                            var baseAttribute = TmphTypeAttribute.GetAttribute<TmphJsonSerialize>(baseType, false, true);
                            if (baseAttribute != null)
                            {
                                if (baseAttribute.IsBaseType)
                                {
                                    methodInfo = baseToJsonMethod.MakeGenericMethod(baseType, type);
                                    defaultToJsoner =
                                        (Action<TmphJsonSerializer, TValueType>)
                                            Delegate.CreateDelegate(typeof(Action<TmphJsonSerializer, TValueType>),
                                                methodInfo);
                                    return;
                                }
                                break;
                            }
                        }
                    }
                    var TClientType = TmphTypeAttribute.GetAttribute<TmphWebView.TmphClientType>(type, true, true);
                    if (TClientType != null)
                    {
                        if (TClientType.MemberName == null) viewClientTypeName = "new " + TClientType.Name + "({";
                        else viewClientTypeName = TClientType.Name + ".Get({";
                    }
                    var dynamicMethod = new TmphTypeToJsoner.TmphMemberDynamicMethod(type);
                    var memberMapDynamicMethod = new TmphTypeToJsoner.TmphMemberMapDynamicMethod(type);
                    var fields =
                        TmphTypeToJsoner.GetFields(TmphMemberIndexGroup<TValueType>.GetFields(attribute.MemberFilter),
                            attribute);
                    foreach (var member in fields)
                    {
                        dynamicMethod.Push(member);
                        memberMapDynamicMethod.Push(member);
                    }
                    var properties =
                        TmphTypeToJsoner.GetProperties(
                            TmphMemberIndexGroup<TValueType>.GetProperties(attribute.MemberFilter), attribute);
                    foreach (var member in properties)
                    {
                        dynamicMethod.Push(member.Key, member.Value);
                        memberMapDynamicMethod.Push(member.Key, member.Value);
                    }
                    memberToJsoner =
                        (Action<TmphJsonSerializer, TValueType>)
                            dynamicMethod.Create<Action<TmphJsonSerializer, TValueType>>();
                    memberMapToJsoner =
                        (Action<TmphMemberMap, TmphJsonSerializer, TValueType, TmphCharStream>)
                            memberMapDynamicMethod
                                .Create<Action<TmphMemberMap, TmphJsonSerializer, TValueType, TmphCharStream>>();
                }
            }

            /// <summary>
            ///     对象转换JSON字符串
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal static void ToJson(TmphJsonSerializer toJsoner, TValueType value)
            {
                if (isValueType) StructToJson(toJsoner, value);
                else toJson(toJsoner, value);
            }

            /// <summary>
            ///     对象转换JSON字符串
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal static void StructToJson(TmphJsonSerializer toJsoner, TValueType value)
            {
                if (defaultToJsoner == null) MemberToJson(toJsoner, value);
                else defaultToJsoner(toJsoner, value);
            }

            /// <summary>
            ///     引用类型对象转换JSON字符串
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal static void ClassToJson(TmphJsonSerializer toJsoner, TValueType value)
            {
                if (defaultToJsoner == null)
                {
                    if (toJsoner.push(value))
                    {
                        MemberToJson(toJsoner, value);
                        toJsoner.pop();
                    }
                }
                else defaultToJsoner(toJsoner, value);
            }

            /// <summary>
            ///     引用类型对象转换JSON字符串
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void toJson(TmphJsonSerializer toJsoner, TValueType value)
            {
                if (value == null) TmphAjax.WriteNull(toJsoner.JsonStream);
                else ClassToJson(toJsoner, value);
            }

            /// <summary>
            ///     值类型对象转换JSON字符串
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal static void MemberToJson(TmphJsonSerializer toJsoner, TValueType value)
            {
                var jsonStream = toJsoner.JsonStream;
                var TmphConfig = toJsoner.toJsonConfig;
                byte isView;
                if (viewClientTypeName != null && TmphConfig.IsViewClientType)
                {
                    jsonStream.WriteNotNull(viewClientTypeName);
                    isView = 1;
                }
                else
                {
                    jsonStream.PrepLength(2);
                    jsonStream.Unsafer.Write('{');
                    isView = 0;
                }
                var memberMap = TmphConfig.MemberMap;
                if (memberMap == null) memberToJsoner(toJsoner, value);
                else if (memberMap.Type == TmphMemberMap<TValueType>.Type)
                {
                    TmphConfig.MemberMap = null;
                    try
                    {
                        memberMapToJsoner(memberMap, toJsoner, value, toJsoner.JsonStream);
                    }
                    finally
                    {
                        TmphConfig.MemberMap = memberMap;
                    }
                }
                else
                {
                    TmphConfig.Warning = TmphWarning.MemberMap;
                    if (TmphConfig.IsMemberMapErrorLog) TmphLog.Error.Add("Json序列化成员位图类型匹配失败", true, true);
                    if (TmphConfig.IsMemberMapErrorToDefault) memberToJsoner(toJsoner, value);
                }
                if (isView == 0) jsonStream.Write('}');
                else
                {
                    jsonStream.PrepLength(2);
                    jsonStream.Unsafer.Write('}');
                    jsonStream.Unsafer.Write(')');
                }
            }

            /// <summary>
            ///     数组转换
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="array">数组对象</param>
            internal static void Array(TmphJsonSerializer toJsoner, TValueType[] array)
            {
                var jsonStream = toJsoner.JsonStream;
                jsonStream.Write('[');
                byte isFirst = 1;
                if (isValueType)
                {
                    foreach (var value in array)
                    {
                        if (isFirst == 0) jsonStream.Write(',');
                        StructToJson(toJsoner, value);
                        isFirst = 0;
                    }
                }
                else
                {
                    foreach (var value in array)
                    {
                        if (isFirst == 0) jsonStream.Write(',');
                        toJson(toJsoner, value);
                        isFirst = 0;
                    }
                }
                jsonStream.Write(']');
            }

            /// <summary>
            ///     枚举集合转换
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="values">枚举集合</param>
            internal static void Enumerable(TmphJsonSerializer toJsoner, IEnumerable<TValueType> values)
            {
                var jsonStream = toJsoner.JsonStream;
                jsonStream.Write('[');
                byte isFirst = 1;
                if (isValueType)
                {
                    foreach (var value in values)
                    {
                        if (isFirst == 0) jsonStream.Write(',');
                        StructToJson(toJsoner, value);
                        isFirst = 0;
                    }
                }
                else
                {
                    foreach (var value in values)
                    {
                        if (isFirst == 0) jsonStream.Write(',');
                        toJson(toJsoner, value);
                        isFirst = 0;
                    }
                }
                jsonStream.Write(']');
            }

            /// <summary>
            ///     字典转换
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="value">数据对象</param>
            internal static void Dictionary<dictionaryValueType>(TmphJsonSerializer toJsoner,
                Dictionary<TValueType, dictionaryValueType> dictionary)
            {
                var jsonStream = toJsoner.JsonStream;
                byte isFirst = 1;
                if (toJsoner.toJsonConfig.IsDictionaryToObject)
                {
                    jsonStream.Write('{');
                    foreach (var value in dictionary)
                    {
                        if (isFirst == 0) jsonStream.Write(',');
                        ToJson(toJsoner, value.Key);
                        jsonStream.Write(':');
                        TmphTypeToJsoner<dictionaryValueType>.ToJson(toJsoner, value.Value);
                        isFirst = 0;
                    }
                    jsonStream.Write('}');
                }
                else
                {
                    jsonStream.Write('[');
                    foreach (var value in dictionary)
                    {
                        if (isFirst == 0) jsonStream.Write(',');
                        KeyValuePair(toJsoner, value);
                        isFirst = 0;
                    }
                    jsonStream.Write(']');
                }
            }

            /// <summary>
            ///     字典转换
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="dictionary">字典</param>
            internal static void StringDictionary(TmphJsonSerializer toJsoner, Dictionary<string, TValueType> dictionary)
            {
                var jsonStream = toJsoner.JsonStream;
                jsonStream.Write('{');
                byte isFirst = 1;
                if (isValueType)
                {
                    foreach (var value in dictionary)
                    {
                        if (isFirst == 0) jsonStream.Write(',');
                        TmphAjax.ToString(value.Key, jsonStream);
                        jsonStream.Write(':');
                        StructToJson(toJsoner, value.Value);
                        isFirst = 0;
                    }
                }
                else
                {
                    foreach (var value in dictionary)
                    {
                        if (isFirst == 0) jsonStream.Write(',');
                        TmphAjax.ToString(value.Key, jsonStream);
                        jsonStream.Write(':');
                        toJson(toJsoner, value.Value);
                        isFirst = 0;
                    }
                }
                jsonStream.Write('}');
            }

            /// <summary>
            ///     字典转换
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="value">数据对象</param>
            internal static void KeyValuePair<dictionaryValueType>(TmphJsonSerializer toJsoner,
                KeyValuePair<TValueType, dictionaryValueType> value)
            {
                var jsonStream = toJsoner.JsonStream;
                jsonStream.PrepLength(21);
                var data = (byte*)jsonStream.CurrentChar;
                *(char*)data = '{';
                *(char*)(data + sizeof(char)) = TmphAjax.Quote;
                *(char*)(data + sizeof(char) * 2) = 'K';
                *(char*)(data + sizeof(char) * 3) = 'e';
                *(char*)(data + sizeof(char) * 4) = 'y';
                *(char*)(data + sizeof(char) * 5) = TmphAjax.Quote;
                *(char*)(data + sizeof(char) * 6) = ':';
                jsonStream.Unsafer.AddLength(7);
                ToJson(toJsoner, value.Key);
                jsonStream.PrepLength(12);
                data = (byte*)jsonStream.CurrentChar;
                *(char*)data = ',';
                *(char*)(data + sizeof(char)) = TmphAjax.Quote;
                *(char*)(data + sizeof(char) * 2) = 'V';
                *(char*)(data + sizeof(char) * 3) = 'a';
                *(char*)(data + sizeof(char) * 4) = 'l';
                *(char*)(data + sizeof(char) * 5) = 'u';
                *(char*)(data + sizeof(char) * 6) = 'e';
                *(char*)(data + sizeof(char) * 7) = TmphAjax.Quote;
                *(char*)(data + sizeof(char) * 8) = ':';
                jsonStream.Unsafer.AddLength(9);
                TmphTypeToJsoner<dictionaryValueType>.ToJson(toJsoner, value.Value);
                jsonStream.Write('}');
            }

            /// <summary>
            ///     不支持多维数组
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void arrayManyRank(TmphJsonSerializer toJsoner, TValueType value)
            {
                TmphAjax.WriteArray(toJsoner.JsonStream);
            }

            /// <summary>
            ///     不支持对象转换null
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void toNull(TmphJsonSerializer toJsoner, TValueType value)
            {
                TmphAjax.WriteNull(toJsoner.JsonStream);
            }

            /// <summary>
            ///     枚举转换字符串
            /// </summary>
            /// <param name="toJsoner">对象转换JSON字符串</param>
            /// <param name="value">数据对象</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void enumToString(TmphJsonSerializer toJsoner, TValueType value)
            {
                TmphJsonSerializer.enumToString(toJsoner, value);
            }

            /// <summary>
            ///     获取字段成员集合
            /// </summary>
            /// <returns>字段成员集合</returns>
            public static TmphSubArray<TmphMemberIndex> GetMembers()
            {
                if (memberToJsoner == null) return default(TmphSubArray<TmphMemberIndex>);
                return TmphTypeToJsoner.GetMembers(TmphMemberIndexGroup<TValueType>.GetFields(attribute.MemberFilter),
                    TmphMemberIndexGroup<TValueType>.GetProperties(attribute.MemberFilter), attribute);
            }
        }
    }
}