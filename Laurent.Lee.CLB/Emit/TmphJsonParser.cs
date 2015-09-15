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
using Laurent.Lee.CLB.Config;
using Laurent.Lee.CLB.Reflection;
using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    ///     Json解析器
    /// </summary>
    public sealed unsafe class TmphJsonParser
    {
        /// <summary>
        ///     解析状态
        /// </summary>
        public enum TmphParseState : byte
        {
            /// <summary>
            ///     成功
            /// </summary>
            Success,

            /// <summary>
            ///     成员位图类型错误
            /// </summary>
            MemberMap,

            /// <summary>
            ///     Json字符串参数为空
            /// </summary>
            NullJson,

            /// <summary>
            ///     解析目标对象参数为空
            /// </summary>
            NullValue,

            /// <summary>
            ///     非正常意外结束
            /// </summary>
            CrashEnd,

            /// <summary>
            ///     未能识别的注释
            /// </summary>
            UnknownNote,

            /// <summary>
            ///     /**/注释缺少回合
            /// </summary>
            NoteNotRound,

            /// <summary>
            ///     null值解析失败
            /// </summary>
            NotNull,

            /// <summary>
            ///     逻辑值解析错误
            /// </summary>
            NotBool,

            /// <summary>
            ///     非数字解析错误
            /// </summary>
            NotNumber,

            /// <summary>
            ///     16进制数字解析错误
            /// </summary>
            NotHex,

            /// <summary>
            ///     字符解析错误
            /// </summary>
            NotChar,

            /// <summary>
            ///     字符串解析失败
            /// </summary>
            NotString,

            /// <summary>
            ///     字符串被换行截断
            /// </summary>
            StringEnter,

            /// <summary>
            ///     时间解析错误
            /// </summary>
            NotDateTime,

            /// <summary>
            ///     Guid解析错误
            /// </summary>
            NotGuid,

            /// <summary>
            ///     不支持多维数组
            /// </summary>
            ArrayManyRank,

            /// <summary>
            ///     数组解析错误
            /// </summary>
            NotArray,

            /// <summary>
            ///     数组数据解析错误
            /// </summary>
            NotArrayValue,

            ///// <summary>
            ///// 不支持指针
            ///// </summary>
            //Pointer,
            /// <summary>
            ///     找不到构造函数
            /// </summary>
            NoConstructor,

            /// <summary>
            ///     非枚举字符
            /// </summary>
            NotEnumChar,

            /// <summary>
            ///     没有找到匹配的枚举值
            /// </summary>
            NoFoundEnumValue,

            /// <summary>
            ///     对象解析错误
            /// </summary>
            NotObject,

            /// <summary>
            ///     没有找到成员名称
            /// </summary>
            NotFoundName,

            /// <summary>
            ///     没有找到冒号
            /// </summary>
            NotFoundColon,

            /// <summary>
            ///     忽略值解析错误
            /// </summary>
            UnknownValue,

            /// <summary>
            ///     字典解析错误
            /// </summary>
            NotDictionary,

            /// <summary>
            ///     类型解析错误
            /// </summary>
            ErrorType
        }

        /// <summary>
        ///     空格字符位图尺寸
        /// </summary>
        internal const int SpaceMapSize = 128 + 64;

        /// <summary>
        ///     数字字符位图尺寸
        /// </summary>
        internal const int NumberMapSize = 128;

        /// <summary>
        ///     键值字符位图尺寸
        /// </summary>
        internal const int NameMapSize = 128;

        /// <summary>
        ///     转义字符集合尺寸
        /// </summary>
        private const int escapeCharSize = 128;

        /// <summary>
        ///     引用类型对象解析函数信息
        /// </summary>
        private static readonly MethodInfo typeParseMethod = typeof(TmphJsonParser).GetMethod("typeParse",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     集合构造解析函数信息
        /// </summary>
        private static readonly MethodInfo structParseMethod = typeof(TmphJsonParser).GetMethod("structParse",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     集合构造解析函数信息
        /// </summary>
        private static readonly MethodInfo nullableParseMethod = typeof(TmphJsonParser).GetMethod("nullableParse",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合构造解析函数信息
        /// </summary>
        private static readonly MethodInfo nullableEnumParseMethod = typeof(TmphJsonParser).GetMethod(
            "nullableEnumParse", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     值类型对象解析函数信息
        /// </summary>
        private static readonly MethodInfo keyValuePairParseMethod = typeof(TmphJsonParser).GetMethod(
            "keyValuePairParse", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     集合构造解析函数信息
        /// </summary>
        private static readonly MethodInfo nullableMemberParseMethod =
            typeof(TmphJsonParser).GetMethod("nullableMemberParse", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     基类转换函数信息
        /// </summary>
        private static readonly MethodInfo baseParseMethod = typeof(TmphJsonParser).GetMethod("baseParse",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     找不到构造函数解析函数信息
        /// </summary>
        private static readonly MethodInfo checkNoConstructorMethod =
            typeof(TmphJsonParser).GetMethod("checkNoConstructor", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumByteMethod = typeof(TmphJsonParser).GetMethod("enumByte",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteMethod = typeof(TmphJsonParser).GetMethod("enumSByte",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumShortMethod = typeof(TmphJsonParser).GetMethod("enumShort",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortMethod = typeof(TmphJsonParser).GetMethod("enumUShort",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumIntMethod = typeof(TmphJsonParser).GetMethod("enumInt",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntMethod = typeof(TmphJsonParser).GetMethod("enumUInt",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumLongMethod = typeof(TmphJsonParser).GetMethod("enumLong",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumULongMethod = typeof(TmphJsonParser).GetMethod("enumULong",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumByteFlagsMethod = typeof(TmphJsonParser).GetMethod("enumByteFlags",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumSByteFlagsMethod = typeof(TmphJsonParser).GetMethod("enumSByteFlags",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumShortFlagsMethod = typeof(TmphJsonParser).GetMethod("enumShortFlags",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumUShortFlagsMethod = typeof(TmphJsonParser).GetMethod("enumUShortFlags",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumIntFlagsMethod = typeof(TmphJsonParser).GetMethod("enumIntFlags",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumUIntFlagsMethod = typeof(TmphJsonParser).GetMethod("enumUIntFlags",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumLongFlagsMethod = typeof(TmphJsonParser).GetMethod("enumLongFlags",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     枚举值解析函数信息
        /// </summary>
        private static readonly MethodInfo enumULongFlagsMethod = typeof(TmphJsonParser).GetMethod("enumULongFlags",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     数组解析函数信息
        /// </summary>
        private static readonly MethodInfo arrayMethod = typeof(TmphJsonParser).GetMethod("array",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     字典解析函数信息
        /// </summary>
        private static readonly MethodInfo dictionaryMethod = typeof(TmphJsonParser).GetMethod("dictionary",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     集合构造解析函数信息
        /// </summary>
        private static readonly MethodInfo dictionaryConstructorMethod =
            typeof(TmphJsonParser).GetMethod("dictionaryConstructor", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     集合构造解析函数信息
        /// </summary>
        private static readonly MethodInfo listConstructorMethod = typeof(TmphJsonParser).GetMethod("listConstructor",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     集合构造解析函数信息
        /// </summary>
        private static readonly MethodInfo collectionConstructorMethod =
            typeof(TmphJsonParser).GetMethod("collectionConstructor", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     集合构造解析函数信息
        /// </summary>
        private static readonly MethodInfo enumerableConstructorMethod =
            typeof(TmphJsonParser).GetMethod("enumerableConstructor", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     数组构造解析函数信息
        /// </summary>
        private static readonly MethodInfo arrayConstructorMethod = typeof(TmphJsonParser).GetMethod("arrayConstructor",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        ///     公共默认配置参数
        /// </summary>
        private static readonly TmphConfig defaultConfig = new TmphConfig { IsGetJson = false };

        /// <summary>
        ///     Json解析
        /// </summary>
        private static TmphInterlocked.TmphDictionary<Type, Func<TmphSubString, TmphConfig, object>> parseTypes =
            new TmphInterlocked.TmphDictionary<Type, Func<TmphSubString, TmphConfig, object>>(
                TmphDictionary.CreateOnly<Type, Func<TmphSubString, TmphConfig, object>>());

        /// <summary>
        ///     Json解析函数信息
        /// </summary>
        private static readonly MethodInfo parseTypeMethod = typeof(TmphJsonParser).GetMethod("parseType",
            BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(TmphSubString), typeof(TmphConfig) }, null);

        /// <summary>
        ///     基本类型解析函数
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> parseMethods;

        /// <summary>
        ///     空格字符位图
        /// </summary>
        private static readonly TmphFixedMap spaceMap;

        /// <summary>
        ///     数字字符位图
        /// </summary>
        private static readonly TmphFixedMap numberMap;

        /// <summary>
        ///     键值字符位图
        /// </summary>
        private static readonly TmphFixedMap nameMap;

        /// <summary>
        ///     键值开始字符位图
        /// </summary>
        private static readonly TmphFixedMap nameStartMap;

        /// <summary>
        ///     转义字符集合
        /// </summary>
        private static readonly TmphPointer escapeCharData;

        /// <summary>
        ///     当前解析位置
        /// </summary>
        internal char* Current;

        /// <summary>
        ///     解析结束位置
        /// </summary>
        private char* end;

        /// <summary>
        ///     最后一个字符
        /// </summary>
        private char endChar;

        /// <summary>
        ///     是否以10进制数字字符结束
        /// </summary>
        private bool isEndDigital;

        /// <summary>
        ///     是否以16进制数字字符结束
        /// </summary>
        private bool isEndHex;

        /// <summary>
        ///     是否以数字字符结束
        /// </summary>
        private bool isEndNumber;

        /// <summary>
        ///     是否以空格字符结束
        /// </summary>
        private bool isEndSpace;

        /// <summary>
        ///     Json字符串
        /// </summary>
        private string json;

        /// <summary>
        ///     Json字符串起始位置
        /// </summary>
        private char* jsonFixed;

        /// <summary>
        ///     配置参数
        /// </summary>
        private TmphConfig parseConfig;

        /// <summary>
        ///     当前字符串引号
        /// </summary>
        private char quote;

        /// <summary>
        ///     解析状态
        /// </summary>
        private TmphParseState state;

        static TmphJsonParser()
        {
            var dataIndex = 0;
            var datas = TmphUnmanaged.Get(true, SpaceMapSize >> 3, NumberMapSize >> 3, NameMapSize >> 3, NameMapSize >> 3,
                escapeCharSize * sizeof(char));
            spaceMap = new TmphFixedMap(datas[dataIndex++]);
            numberMap = new TmphFixedMap(datas[dataIndex++]);
            nameMap = new TmphFixedMap(datas[dataIndex++]);
            nameStartMap = new TmphFixedMap(datas[dataIndex++]);
            escapeCharData = datas[dataIndex++];

            foreach (var value in " \r\n\t") spaceMap.Set(value);
            spaceMap.Set(160);

            numberMap.Set('0', 10);
            //numberMap.Set('a');
            numberMap.Set('e');
            numberMap.Set('E');
            //numberMap.Set('N');
            numberMap.Set('+');
            numberMap.Set('-');
            numberMap.Set('.');

            nameMap.Set('0', 10);
            nameMap.Set('A', 26);
            nameMap.Set('a', 26);
            nameMap.Set('_');

            nameStartMap.Set('A', 26);
            nameStartMap.Set('a', 26);
            nameStartMap.Set('_');
            nameStartMap.Set('\'');
            nameStartMap.Set('"');

            var escapeCharDataChar = escapeCharData.Char;
            for (var value = 0; value != escapeCharSize; ++value) escapeCharDataChar[value] = (char)value;
            escapeCharDataChar['0'] = (char)0;
            escapeCharDataChar['B'] = escapeCharDataChar['b'] = '\b';
            escapeCharDataChar['F'] = escapeCharDataChar['f'] = '\f';
            escapeCharDataChar['N'] = escapeCharDataChar['n'] = '\n';
            escapeCharDataChar['R'] = escapeCharDataChar['r'] = '\r';
            escapeCharDataChar['T'] = escapeCharDataChar['t'] = '\t';
            escapeCharDataChar['V'] = escapeCharDataChar['v'] = '\v';

            parseMethods = TmphDictionary.CreateOnly<Type, MethodInfo>();
            foreach (var method in typeof(TmphJsonParser).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (method.CustomAttribute<TmphParseMethod>() != null)
                {
                    parseMethods.Add(method.GetParameters()[0].ParameterType.GetElementType(), method);
                }
            }
        }

        /// <summary>
        ///     Json解析器
        /// </summary>
        private TmphJsonParser()
        {
        }

        /// <summary>
        ///     二进制缓冲区
        /// </summary>
        internal byte[] TmphBuffer { get; private set; }

        /// <summary>
        ///     解析结束位置
        /// </summary>
        internal char* End
        {
            get { return end; }
        }

        /// <summary>
        ///     Json解析
        /// </summary>
        /// <typeparam name="TValueType">目标类型</typeparam>
        /// <param name="json">Json字符串</param>
        /// <param name="value">目标数据</param>
        /// <param name="TmphConfig">配置参数</param>
        /// <returns>解析状态</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private TmphParseState parse<TValueType>(TmphSubString json, ref TValueType value, TmphConfig TmphConfig)
        {
            fixed (char* jsonFixed = (this.json = json.value))
            {
                Current = (this.jsonFixed = jsonFixed) + json.StartIndex;
                parseConfig = TmphConfig;
                endChar = *((end = Current + json.Length) - 1);
                var state = parse(ref value);
                if (state != TmphParseState.Success && TmphConfig.IsGetJson) TmphConfig.Json = json;
                return state;
            }
        }

        /// <summary>
        ///     Json解析
        /// </summary>
        /// <typeparam name="TValueType">目标类型</typeparam>
        /// <param name="json">Json字符串</param>
        /// <param name="length">Json长度</param>
        /// <param name="value">目标数据</param>
        /// <param name="TmphConfig">配置参数</param>
        /// <returns>解析状态</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private TmphParseState parse<TValueType>(char* json, int length, ref TValueType value, TmphConfig TmphConfig, byte[] TmphBuffer)
        {
            parseConfig = TmphConfig;
            TmphBuffer = TmphBuffer;
            endChar = *((end = (jsonFixed = Current = json) + length) - 1);
            return parse(ref value);
        }

        /// <summary>
        ///     Json解析
        /// </summary>
        /// <typeparam name="TValueType">目标类型</typeparam>
        /// <param name="value">目标数据</param>
        /// <returns>解析状态</returns>
        private TmphParseState parse<TValueType>(ref TValueType value)
        {
            isEndSpace = endChar < SpaceMapSize && spaceMap.Get(endChar);
            isEndDigital = (uint)(endChar - '0') < 10;
            isEndHex = isEndDigital || (uint)((endChar | 0x20) - 'a') < 6;
            isEndNumber = isEndHex || (endChar < NumberMapSize && numberMap.Get(endChar));
            state = TmphParseState.Success;
            TmphTypeParser<TValueType>.Parse(this, ref value);
            if (state == TmphParseState.Success)
            {
                if (Current == end || !parseConfig.IsEndSpace) return parseConfig.State = TmphParseState.Success;
                space();
                if (state == TmphParseState.Success)
                {
                    if (Current == end) return parseConfig.State = TmphParseState.Success;
                    state = TmphParseState.CrashEnd;
                }
            }
            if (parseConfig.IsGetJson)
            {
                parseConfig.State = state;
                parseConfig.CurrentIndex = (int)(Current - jsonFixed);
            }
            return state;
        }

        /// <summary>
        ///     释放Json解析器
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void free()
        {
            json = null;
            parseConfig = null;
            TmphBuffer = null;
            TmphTypePool<TmphJsonParser>.Push(this);
        }

        /// <summary>
        ///     设置错误解析状态
        /// </summary>
        /// <param name="state"></param>
        internal void Error(TmphParseState state)
        {
            this.state = state;
        }

        /// <summary>
        ///     扫描空格字符
        /// </summary>
        private void space()
        {
            SPACE:
            if (isEndSpace)
            {
                while (Current != end && *Current < SpaceMapSize && spaceMap.Get(*Current)) ++Current;
            }
            else
            {
                while (*Current < SpaceMapSize && spaceMap.Get(*Current)) ++Current;
            }
            if (Current == end || *Current != '/') return;
            if (++Current == end)
            {
                state = TmphParseState.UnknownNote;
                return;
            }
            if (*Current == '/')
            {
                if (endChar == '\n')
                {
                    while (*++Current != '\n') ;
                    ++Current;
                }
                else
                {
                    do
                    {
                        if (++Current == end) return;
                    } while (*Current != '\n');
                }
                goto SPACE;
            }
            if (*Current != '*')
            {
                state = TmphParseState.UnknownNote;
                return;
            }
            if (++Current == end)
            {
                state = TmphParseState.NoteNotRound;
                return;
            }
            if (endChar == '*')
            {
                do
                {
                    while (*Current != '*') ++Current;
                    if (++Current == end)
                    {
                        state = TmphParseState.NoteNotRound;
                        return;
                    }
                    if (*Current == '/')
                    {
                        ++Current;
                        goto SPACE;
                    }
                } while (true);
            }
            if (endChar == '/')
            {
                do
                {
                    if (++Current == end)
                    {
                        state = TmphParseState.NoteNotRound;
                        return;
                    }
                    while (*Current != '/') ++Current;
                    if (*(Current - 1) == '*')
                    {
                        ++Current;
                        goto SPACE;
                    }
                    if (++Current == end)
                    {
                        state = TmphParseState.NoteNotRound;
                        return;
                    }
                } while (true);
            }
            do
            {
                while (*Current != '*')
                {
                    if (++Current == end)
                    {
                        state = TmphParseState.NoteNotRound;
                        return;
                    }
                }
                if (++Current == end)
                {
                    state = TmphParseState.NoteNotRound;
                    return;
                }
                if (*Current == '/')
                {
                    if (++Current == end) return;
                    goto SPACE;
                }
            } while (true);
        }

        /// <summary>
        ///     是否null
        /// </summary>
        /// <returns>是否null</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private bool isNull()
        {
            if (*Current == 'n')
            {
                if ((int)(end - Current) < 4 ||
                    ((*(Current + 1) ^ 'u') | (*(int*)(Current + 2) ^ ('l' + ('l' << 16)))) != 0)
                {
                    state = TmphParseState.NotNull;
                    return false;
                }
                Current += 4;
                return true;
            }
            return false;
        }

        /// <summary>
        ///     是否非数字NaN
        /// </summary>
        /// <returns>是否非数字NaN</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private bool isNaN()
        {
            if (*Current == 'N')
            {
                if ((int)(end - Current) < 3 || (*(int*)(Current + 1) ^ ('a' + ('N' << 16))) != 0)
                {
                    state = TmphParseState.NotNumber;
                    return false;
                }
                Current += 3;
                return true;
            }
            return false;
        }

        /// <summary>
        ///     解析10进制数字
        /// </summary>
        /// <param name="value">第一位数字</param>
        /// <returns>数字</returns>
        private uint parseUInt32(uint value)
        {
            uint number;
            if (isEndDigital)
            {
                do
                {
                    if ((number = (uint)(*Current - '0')) > 9) return value;
                    value *= 10;
                    value += number;
                    if (++Current == end) return value;
                } while (true);
            }
            while ((number = (uint)(*Current - '0')) < 10)
            {
                value *= 10;
                ++Current;
                value += (byte)number;
            }
            return value;
        }

        /// <summary>
        ///     解析16进制数字
        /// </summary>
        /// <param name="value">数值</param>
        private void parseHex32(ref uint value)
        {
            var number = (uint)(*Current - '0');
            if (number > 9)
            {
                if ((number = (number - ('A' - '0')) & 0xffdfU) > 5)
                {
                    state = TmphParseState.NotHex;
                    return;
                }
                number += 10;
            }
            value = number;
            if (++Current == end) return;
            if (isEndHex)
            {
                do
                {
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        if ((number = (number - ('A' - '0')) & 0xffdfU) > 5) return;
                        number += 10;
                    }
                    value <<= 4;
                    value += number;
                } while (++Current != end);
                return;
            }
            do
            {
                if ((number = (uint)(*Current - '0')) > 9)
                {
                    if ((number = (number - ('A' - '0')) & 0xffdfU) > 5) return;
                    number += 10;
                }
                value <<= 4;
                ++Current;
                value += number;
            } while (true);
        }

        /// <summary>
        ///     逻辑值解析
        /// </summary>
        /// <param name="value">数据</param>
        /// <returns>解析状态</returns>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal void Parse(ref bool value)
        {
            if (*Current != 'f') goto NOTFALSE;
            FALSE:
            if (((int)(end - Current)) < 5
                || ((*(int*)(Current + 1) ^ ('a' + ('l' << 16))) | (*(int*)(Current + 3) ^ ('s' + ('e' << 16)))) != 0)
            {
                state = TmphParseState.NotBool;
                return;
            }
            Current += 5;
            value = false;
            return;
            NOTFALSE:
            if (*Current != 't') goto NOTTRUE;
            TRUE:
            if ((int)(end - Current) < 4 ||
                ((*(Current + 1) ^ 'r') | (*(int*)(Current + 2) ^ ('u' + ('e' << 16)))) != 0)
            {
                state = TmphParseState.NotBool;
                return;
            }
            Current += 4;
            value = true;
            return;
            NOTTRUE:
            var number = (uint)(*Current - '0');
            if (number < 2)
            {
                ++Current;
                value = number != 0;
                return;
            }
            space();
            if (state != TmphParseState.Success) return;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            if (*Current == 'f') goto FALSE;
            if (*Current == 't') goto TRUE;
            if ((number = (uint)(*Current - '0')) < 2)
            {
                ++Current;
                value = number != 0;
                return;
            }
            if (*Current == '"' || *Current == '\'')
            {
                quote = *Current;
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                if (*Current == 'f')
                {
                    if (((int)(end - Current)) < 5
                        ||
                        ((*(int*)(Current + 1) ^ ('a' + ('l' << 16))) | (*(int*)(Current + 3) ^ ('s' + ('e' << 16)))) !=
                        0)
                    {
                        state = TmphParseState.NotBool;
                        return;
                    }
                    Current += 5;
                    value = false;
                }
                else if (*Current == 't')
                {
                    if ((int)(end - Current) < 4 ||
                        ((*(Current + 1) ^ 'r') | (*(int*)(Current + 2) ^ ('u' + ('e' << 16)))) != 0)
                    {
                        state = TmphParseState.NotBool;
                        return;
                    }
                    Current += 4;
                    value = true;
                }
                else if ((number = (uint)(*Current - '0')) < 2)
                {
                    ++Current;
                    value = number != 0;
                }
                else state = TmphParseState.NotBool;
                if (state == TmphParseState.Success)
                {
                    if (Current == end) state = TmphParseState.CrashEnd;
                    else if (*Current == quote) ++Current;
                    else state = TmphParseState.NotBool;
                }
                return;
            }
            state = TmphParseState.NotBool;
        }

        /// <summary>
        ///     逻辑值解析
        /// </summary>
        /// <param name="value">数据</param>
        /// <returns>解析状态</returns>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void parse(ref bool? value)
        {
            if (*Current != 'f') goto NOTFALSE;
            FALSE:
            if (((int)(end - Current)) < 5
                || ((*(int*)(Current + 1) ^ ('a' + ('l' << 16))) | (*(int*)(Current + 3) ^ ('s' + ('e' << 16)))) != 0)
            {
                state = TmphParseState.NotBool;
                return;
            }
            Current += 5;
            value = false;
            return;
            NOTFALSE:
            if (*Current != 't') goto NOTTRUE;
            TRUE:
            if ((int)(end - Current) < 4 ||
                ((*(Current + 1) ^ 'r') | (*(int*)(Current + 2) ^ ('u' + ('e' << 16)))) != 0)
            {
                state = TmphParseState.NotBool;
                return;
            }
            Current += 4;
            value = true;
            return;
            NOTTRUE:
            var number = (uint)(*Current - '0');
            if (number < 2)
            {
                ++Current;
                value = number != 0;
                return;
            }
            if (isNull())
            {
                value = null;
                return;
            }
            space();
            if (state != TmphParseState.Success) return;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            if (*Current == 'f') goto FALSE;
            if (*Current == 't') goto TRUE;
            if ((number = (uint)(*Current - '0')) < 2)
            {
                ++Current;
                value = number != 0;
                return;
            }
            if (isNull())
            {
                value = null;
                return;
            }
            if (*Current == '"' || *Current == '\'')
            {
                quote = *Current;
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                if (*Current == 'f')
                {
                    if (((int)(end - Current)) < 5
                        ||
                        ((*(int*)(Current + 1) ^ ('a' + ('l' << 16))) | (*(int*)(Current + 3) ^ ('s' + ('e' << 16)))) !=
                        0)
                    {
                        state = TmphParseState.NotBool;
                        return;
                    }
                    Current += 5;
                    value = false;
                }
                else if (*Current == 't')
                {
                    if ((int)(end - Current) < 4 ||
                        ((*(Current + 1) ^ 'r') | (*(int*)(Current + 2) ^ ('u' + ('e' << 16)))) != 0)
                    {
                        state = TmphParseState.NotBool;
                        return;
                    }
                    Current += 4;
                    value = true;
                }
                else if ((number = (uint)(*Current - '0')) < 2)
                {
                    ++Current;
                    value = number != 0;
                }
                else state = TmphParseState.NotBool;
                if (state == TmphParseState.Success)
                {
                    if (Current == end) state = TmphParseState.CrashEnd;
                    else if (*Current == quote) ++Current;
                    else state = TmphParseState.NotBool;
                }
                return;
            }
            state = TmphParseState.NotBool;
        }

        /// <summary>
        ///     数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal void Parse(ref byte value)
        {
            var number = (uint)(*Current - '0');
            if (number > 9)
            {
                space();
                if (state != TmphParseState.Success) return;
                if (Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                if ((number = (uint)(*Current - '0')) > 9)
                {
                    if (*Current == '"' || *Current == '\'')
                    {
                        quote = *Current;
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if (number == 0)
                        {
                            if (*Current == quote)
                            {
                                value = 0;
                                ++Current;
                                return;
                            }
                            if (*Current != 'x')
                            {
                                state = TmphParseState.NotNumber;
                                return;
                            }
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            parseHex32(ref number);
                            value = (byte)number;
                        }
                        else value = (byte)parseUInt32(number);
                        if (state == TmphParseState.Success)
                        {
                            if (Current == end) state = TmphParseState.CrashEnd;
                            else if (*Current == quote) ++Current;
                            else state = TmphParseState.NotNumber;
                        }
                        return;
                    }
                    state = TmphParseState.NotNumber;
                    return;
                }
            }
            if (++Current == end)
            {
                value = (byte)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = (byte)number;
                return;
            }
            value = (byte)parseUInt32(number);
        }

        /// <summary>
        ///     数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void parse(ref byte? value)
        {
            var number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (isNull())
                {
                    value = null;
                    return;
                }
                space();
                if (state != TmphParseState.Success) return;
                if (Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                if ((number = (uint)(*Current - '0')) > 9)
                {
                    if (isNull())
                    {
                        value = null;
                        return;
                    }
                    if (*Current == '"' || *Current == '\'')
                    {
                        quote = *Current;
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if (number == 0)
                        {
                            if (*Current == quote)
                            {
                                value = 0;
                                ++Current;
                                return;
                            }
                            if (*Current != 'x')
                            {
                                state = TmphParseState.NotNumber;
                                return;
                            }
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            parseHex32(ref number);
                            value = (byte)number;
                        }
                        else value = (byte)parseUInt32(number);
                        if (state == TmphParseState.Success)
                        {
                            if (Current == end) state = TmphParseState.CrashEnd;
                            else if (*Current == quote) ++Current;
                            else state = TmphParseState.NotNumber;
                        }
                        return;
                    }
                    state = TmphParseState.NotNumber;
                    return;
                }
            }
            if (++Current == end)
            {
                value = (byte)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = (byte)number;
                return;
            }
            value = (byte)parseUInt32(number);
        }

        /// <summary>
        ///     数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal void Parse(ref sbyte value)
        {
            var sign = 0;
            var number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (*Current == '-')
                {
                    if (++Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        state = TmphParseState.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                else
                {
                    space();
                    if (state != TmphParseState.Success) return;
                    if (Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        if (*Current == '"' || *Current == '\'')
                        {
                            quote = *Current;
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            if ((number = (uint)(*Current - '0')) > 9)
                            {
                                if (*Current != '-')
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = TmphParseState.CrashEnd;
                                    return;
                                }
                                if ((number = (uint)(*Current - '0')) > 9)
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                sign = 1;
                            }
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            if (number == 0)
                            {
                                if (*Current == quote)
                                {
                                    value = 0;
                                    ++Current;
                                    return;
                                }
                                if (*Current != 'x')
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = TmphParseState.CrashEnd;
                                    return;
                                }
                                parseHex32(ref number);
                                value = sign == 0 ? (sbyte)(byte)number : (sbyte)-(int)number;
                            }
                            else
                                value = sign == 0
                                    ? (sbyte)(byte)parseUInt32(number)
                                    : (sbyte)-(int)parseUInt32(number);
                            if (state == TmphParseState.Success)
                            {
                                if (Current == end) state = TmphParseState.CrashEnd;
                                else if (*Current == quote) ++Current;
                                else state = TmphParseState.NotNumber;
                            }
                            return;
                        }
                        if (*Current != '-')
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        sign = 1;
                    }
                }
            }
            if (++Current == end)
            {
                value = sign == 0 ? (sbyte)(byte)number : (sbyte)-(int)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = sign == 0 ? (sbyte)(byte)number : (sbyte)-(int)number;
                return;
            }
            value = sign == 0 ? (sbyte)(byte)parseUInt32(number) : (sbyte)-(int)parseUInt32(number);
        }

        /// <summary>
        ///     数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void parse(ref sbyte? value)
        {
            var sign = 0;
            var number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (isNull())
                {
                    value = null;
                    return;
                }
                if (*Current == '-')
                {
                    if (++Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        state = TmphParseState.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                else
                {
                    space();
                    if (state != TmphParseState.Success) return;
                    if (Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        if (isNull())
                        {
                            value = null;
                            return;
                        }
                        if (*Current == '"' || *Current == '\'')
                        {
                            quote = *Current;
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            if ((number = (uint)(*Current - '0')) > 9)
                            {
                                if (*Current != '-')
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = TmphParseState.CrashEnd;
                                    return;
                                }
                                if ((number = (uint)(*Current - '0')) > 9)
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                sign = 1;
                            }
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            if (number == 0)
                            {
                                if (*Current == quote)
                                {
                                    value = 0;
                                    ++Current;
                                    return;
                                }
                                if (*Current != 'x')
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = TmphParseState.CrashEnd;
                                    return;
                                }
                                parseHex32(ref number);
                                value = sign == 0 ? (sbyte)(byte)number : (sbyte)-(int)number;
                            }
                            else
                                value = sign == 0
                                    ? (sbyte)(byte)parseUInt32(number)
                                    : (sbyte)-(int)parseUInt32(number);
                            if (state == TmphParseState.Success)
                            {
                                if (Current == end) state = TmphParseState.CrashEnd;
                                else if (*Current == quote) ++Current;
                                else state = TmphParseState.NotNumber;
                            }
                            return;
                        }
                        if (*Current != '-')
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        sign = 1;
                    }
                }
            }
            if (++Current == end)
            {
                value = sign == 0 ? (sbyte)(byte)number : (sbyte)-(int)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = sign == 0 ? (sbyte)(byte)number : (sbyte)-(int)number;
                return;
            }
            value = sign == 0 ? (sbyte)(byte)parseUInt32(number) : (sbyte)-(int)parseUInt32(number);
        }

        /// <summary>
        ///     数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal void Parse(ref ushort value)
        {
            var number = (uint)(*Current - '0');
            if (number > 9)
            {
                space();
                if (state != TmphParseState.Success) return;
                if (Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                if ((number = (uint)(*Current - '0')) > 9)
                {
                    if (*Current == '"' || *Current == '\'')
                    {
                        quote = *Current;
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if (number == 0)
                        {
                            if (*Current == quote)
                            {
                                value = 0;
                                ++Current;
                                return;
                            }
                            if (*Current != 'x')
                            {
                                state = TmphParseState.NotNumber;
                                return;
                            }
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            parseHex32(ref number);
                            value = (ushort)number;
                        }
                        else value = (ushort)parseUInt32(number);
                        if (state == TmphParseState.Success)
                        {
                            if (Current == end) state = TmphParseState.CrashEnd;
                            else if (*Current == quote) ++Current;
                            else state = TmphParseState.NotNumber;
                        }
                        return;
                    }
                    state = TmphParseState.NotNumber;
                    return;
                }
            }
            if (++Current == end)
            {
                value = (ushort)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = (ushort)number;
                return;
            }
            value = (ushort)parseUInt32(number);
        }

        /// <summary>
        ///     数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void parse(ref ushort? value)
        {
            var number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (isNull())
                {
                    value = null;
                    return;
                }
                space();
                if (state != TmphParseState.Success) return;
                if (Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                if ((number = (uint)(*Current - '0')) > 9)
                {
                    if (isNull())
                    {
                        value = null;
                        return;
                    }
                    if (*Current == '"' || *Current == '\'')
                    {
                        quote = *Current;
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if (number == 0)
                        {
                            if (*Current == quote)
                            {
                                value = 0;
                                ++Current;
                                return;
                            }
                            if (*Current != 'x')
                            {
                                state = TmphParseState.NotNumber;
                                return;
                            }
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            parseHex32(ref number);
                            value = (ushort)number;
                        }
                        else value = (ushort)parseUInt32(number);
                        if (state == TmphParseState.Success)
                        {
                            if (Current == end) state = TmphParseState.CrashEnd;
                            else if (*Current == quote) ++Current;
                            else state = TmphParseState.NotNumber;
                        }
                        return;
                    }
                    state = TmphParseState.NotNumber;
                    return;
                }
            }
            if (++Current == end)
            {
                value = (ushort)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = (ushort)number;
                return;
            }
            value = (ushort)parseUInt32(number);
        }

        /// <summary>
        ///     数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal void Parse(ref short value)
        {
            var sign = 0;
            var number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (*Current == '-')
                {
                    if (++Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        state = TmphParseState.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                else
                {
                    space();
                    if (state != TmphParseState.Success) return;
                    if (Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        if (*Current == '"' || *Current == '\'')
                        {
                            quote = *Current;
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            if ((number = (uint)(*Current - '0')) > 9)
                            {
                                if (*Current != '-')
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = TmphParseState.CrashEnd;
                                    return;
                                }
                                if ((number = (uint)(*Current - '0')) > 9)
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                sign = 1;
                            }
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            if (number == 0)
                            {
                                if (*Current == quote)
                                {
                                    value = 0;
                                    ++Current;
                                    return;
                                }
                                if (*Current != 'x')
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = TmphParseState.CrashEnd;
                                    return;
                                }
                                parseHex32(ref number);
                                value = sign == 0 ? (short)(ushort)number : (short)-(int)number;
                            }
                            else
                                value = sign == 0
                                    ? (short)(ushort)parseUInt32(number)
                                    : (short)-(int)parseUInt32(number);
                            if (state == TmphParseState.Success)
                            {
                                if (Current == end) state = TmphParseState.CrashEnd;
                                else if (*Current == quote) ++Current;
                                else state = TmphParseState.NotNumber;
                            }
                            return;
                        }
                        if (*Current != '-')
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        sign = 1;
                    }
                }
            }
            if (++Current == end)
            {
                value = sign == 0 ? (short)(ushort)number : (short)-(int)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = sign == 0 ? (short)(ushort)number : (short)-(int)number;
                return;
            }
            value = sign == 0 ? (short)(ushort)parseUInt32(number) : (short)-(int)parseUInt32(number);
        }

        /// <summary>
        ///     数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void parse(ref short? value)
        {
            var sign = 0;
            var number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (isNull())
                {
                    value = null;
                    return;
                }
                if (*Current == '-')
                {
                    if (++Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        state = TmphParseState.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                else
                {
                    space();
                    if (state != TmphParseState.Success) return;
                    if (Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        if (isNull())
                        {
                            value = null;
                            return;
                        }
                        if (*Current == '"' || *Current == '\'')
                        {
                            quote = *Current;
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            if ((number = (uint)(*Current - '0')) > 9)
                            {
                                if (*Current != '-')
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = TmphParseState.CrashEnd;
                                    return;
                                }
                                if ((number = (uint)(*Current - '0')) > 9)
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                sign = 1;
                            }
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            if (number == 0)
                            {
                                if (*Current == quote)
                                {
                                    value = 0;
                                    ++Current;
                                    return;
                                }
                                if (*Current != 'x')
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = TmphParseState.CrashEnd;
                                    return;
                                }
                                parseHex32(ref number);
                                value = sign == 0 ? (short)(ushort)number : (short)-(int)number;
                            }
                            else
                                value = sign == 0
                                    ? (short)(ushort)parseUInt32(number)
                                    : (short)-(int)parseUInt32(number);
                            if (state == TmphParseState.Success)
                            {
                                if (Current == end) state = TmphParseState.CrashEnd;
                                else if (*Current == quote) ++Current;
                                else state = TmphParseState.NotNumber;
                            }
                            return;
                        }
                        if (*Current != '-')
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        sign = 1;
                    }
                }
            }
            if (++Current == end)
            {
                value = sign == 0 ? (short)(ushort)number : (short)-(int)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = sign == 0 ? (short)(ushort)number : (short)-(int)number;
                return;
            }
            value = sign == 0 ? (short)(ushort)parseUInt32(number) : (short)-(int)parseUInt32(number);
        }

        /// <summary>
        ///     数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal void Parse(ref uint value)
        {
            var number = (uint)(*Current - '0');
            if (number > 9)
            {
                space();
                if (state != TmphParseState.Success) return;
                if (Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                if ((number = (uint)(*Current - '0')) > 9)
                {
                    if (*Current == '"' || *Current == '\'')
                    {
                        quote = *Current;
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if (number == 0)
                        {
                            if (*Current == quote)
                            {
                                value = 0;
                                ++Current;
                                return;
                            }
                            if (*Current != 'x')
                            {
                                state = TmphParseState.NotNumber;
                                return;
                            }
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            parseHex32(ref number);
                            value = number;
                        }
                        else value = parseUInt32(number);
                        if (state == TmphParseState.Success)
                        {
                            if (Current == end) state = TmphParseState.CrashEnd;
                            else if (*Current == quote) ++Current;
                            else state = TmphParseState.NotNumber;
                        }
                        return;
                    }
                    state = TmphParseState.NotNumber;
                    return;
                }
            }
            if (++Current == end)
            {
                value = number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = number;
                return;
            }
            value = parseUInt32(number);
        }

        /// <summary>
        ///     数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void parse(ref uint? value)
        {
            var number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (isNull())
                {
                    value = null;
                    return;
                }
                space();
                if (state != TmphParseState.Success) return;
                if (Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                if ((number = (uint)(*Current - '0')) > 9)
                {
                    if (isNull())
                    {
                        value = null;
                        return;
                    }
                    if (*Current == '"' || *Current == '\'')
                    {
                        quote = *Current;
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if (number == 0)
                        {
                            if (*Current == quote)
                            {
                                value = 0;
                                ++Current;
                                return;
                            }
                            if (*Current != 'x')
                            {
                                state = TmphParseState.NotNumber;
                                return;
                            }
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            parseHex32(ref number);
                            value = number;
                        }
                        else value = parseUInt32(number);
                        if (state == TmphParseState.Success)
                        {
                            if (Current == end) state = TmphParseState.CrashEnd;
                            else if (*Current == quote) ++Current;
                            else state = TmphParseState.NotNumber;
                        }
                        return;
                    }
                    state = TmphParseState.NotNumber;
                    return;
                }
            }
            if (++Current == end)
            {
                value = number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = number;
                return;
            }
            value = parseUInt32(number);
        }

        /// <summary>
        ///     数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal void Parse(ref int value)
        {
            var sign = 0;
            var number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (*Current == '-')
                {
                    if (++Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        state = TmphParseState.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                else
                {
                    space();
                    if (state != TmphParseState.Success) return;
                    if (Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        if (*Current == '"' || *Current == '\'')
                        {
                            quote = *Current;
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            if ((number = (uint)(*Current - '0')) > 9)
                            {
                                if (*Current != '-')
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = TmphParseState.CrashEnd;
                                    return;
                                }
                                if ((number = (uint)(*Current - '0')) > 9)
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                sign = 1;
                            }
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            if (number == 0)
                            {
                                if (*Current == quote)
                                {
                                    value = 0;
                                    ++Current;
                                    return;
                                }
                                if (*Current != 'x')
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = TmphParseState.CrashEnd;
                                    return;
                                }
                                parseHex32(ref number);
                                value = sign == 0 ? (int)number : -(int)number;
                            }
                            else value = sign == 0 ? (int)parseUInt32(number) : -(int)parseUInt32(number);
                            if (state == TmphParseState.Success)
                            {
                                if (Current == end) state = TmphParseState.CrashEnd;
                                else if (*Current == quote) ++Current;
                                else state = TmphParseState.NotNumber;
                            }
                            return;
                        }
                        if (*Current != '-')
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        sign = 1;
                    }
                }
            }
            if (++Current == end)
            {
                value = sign == 0 ? (int)number : -(int)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = sign == 0 ? (int)number : -(int)number;
                return;
            }
            value = sign == 0 ? (int)parseUInt32(number) : -(int)parseUInt32(number);
        }

        /// <summary>
        ///     数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void parse(ref int? value)
        {
            var sign = 0;
            var number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (isNull())
                {
                    value = null;
                    return;
                }
                if (*Current == '-')
                {
                    if (++Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        state = TmphParseState.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                else
                {
                    space();
                    if (state != TmphParseState.Success) return;
                    if (Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        if (isNull())
                        {
                            value = null;
                            return;
                        }
                        if (*Current == '"' || *Current == '\'')
                        {
                            quote = *Current;
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            if ((number = (uint)(*Current - '0')) > 9)
                            {
                                if (*Current != '-')
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = TmphParseState.CrashEnd;
                                    return;
                                }
                                if ((number = (uint)(*Current - '0')) > 9)
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                sign = 1;
                            }
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            if (number == 0)
                            {
                                if (*Current == quote)
                                {
                                    value = 0;
                                    ++Current;
                                    return;
                                }
                                if (*Current != 'x')
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = TmphParseState.CrashEnd;
                                    return;
                                }
                                parseHex32(ref number);
                                value = sign == 0 ? (int)number : -(int)number;
                            }
                            else value = sign == 0 ? (int)parseUInt32(number) : -(int)parseUInt32(number);
                            if (state == TmphParseState.Success)
                            {
                                if (Current == end) state = TmphParseState.CrashEnd;
                                else if (*Current == quote) ++Current;
                                else state = TmphParseState.NotNumber;
                            }
                            return;
                        }
                        if (*Current != '-')
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        sign = 1;
                    }
                }
            }
            if (++Current == end)
            {
                value = sign == 0 ? (int)number : -(int)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                parseHex32(ref number);
                value = sign == 0 ? (int)number : -(int)number;
                return;
            }
            value = sign == 0 ? (int)parseUInt32(number) : -(int)parseUInt32(number);
        }

        /// <summary>
        ///     解析10进制数字
        /// </summary>
        /// <param name="value">第一位数字</param>
        /// <returns>数字</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private ulong parseUInt64(uint value)
        {
            var end32 = Current + 8;
            if (end32 > end) end32 = end;
            uint number;
            do
            {
                if ((number = (uint)(*Current - '0')) > 9) return value;
                value *= 10;
                value += number;
            } while (++Current != end32);
            if (Current == end) return value;
            ulong value64 = value;
            if (isEndDigital)
            {
                do
                {
                    if ((number = (uint)(*Current - '0')) > 9) return value64;
                    value64 *= 10;
                    value64 += number;
                    if (++Current == end) return value64;
                } while (true);
            }
            while ((number = (uint)(*Current - '0')) < 10)
            {
                value64 *= 10;
                ++Current;
                value64 += (byte)number;
            }
            return value64;
        }

        /// <summary>
        ///     解析16进制数字
        /// </summary>
        /// <returns>数字</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private ulong parseHex64()
        {
            var number = (uint)(*Current - '0');
            if (number > 9)
            {
                if ((number = (number - ('A' - '0')) & 0xffdfU) > 5)
                {
                    state = TmphParseState.NotHex;
                    return 0;
                }
                number += 10;
            }
            if (++Current == end) return number;
            var high = number;
            var end32 = Current + 7;
            if (end32 > end) end32 = end;
            do
            {
                if ((number = (uint)(*Current - '0')) > 9)
                {
                    if ((number = (number - ('A' - '0')) & 0xffdfU) > 5) return high;
                    number += 10;
                }
                high <<= 4;
                high += number;
            } while (++Current != end32);
            if (Current == end) return high;
            var start = Current;
            ulong low = number;
            if (isEndHex)
            {
                do
                {
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        if ((number = (number - ('A' - '0')) & 0xffdfU) > 5)
                        {
                            return low | (ulong)high << ((int)((byte*)Current - (byte*)start) << 1);
                        }
                        number += 10;
                    }
                    low <<= 4;
                    low += number;
                } while (++Current != end);
                return low | (ulong)high << ((int)((byte*)Current - (byte*)start) << 1);
            }
            do
            {
                if ((number = (uint)(*Current - '0')) > 9)
                {
                    if ((number = (number - ('A' - '0')) & 0xffdfU) > 5)
                    {
                        return low | (ulong)high << ((int)((byte*)Current - (byte*)start) << 1);
                    }
                    number += 10;
                }
                low <<= 4;
                ++Current;
                low += number;
            } while (true);
        }

        /// <summary>
        ///     数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal void Parse(ref ulong value)
        {
            var number = (uint)(*Current - '0');
            if (number > 9)
            {
                space();
                if (state != TmphParseState.Success) return;
                if (Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                if ((number = (uint)(*Current - '0')) > 9)
                {
                    if (*Current == '"' || *Current == '\'')
                    {
                        quote = *Current;
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if (number == 0)
                        {
                            if (*Current == quote)
                            {
                                value = 0;
                                ++Current;
                                return;
                            }
                            if (*Current != 'x')
                            {
                                state = TmphParseState.NotNumber;
                                return;
                            }
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            value = parseHex64();
                        }
                        else value = parseUInt64(number);
                        if (state == TmphParseState.Success)
                        {
                            if (Current == end) state = TmphParseState.CrashEnd;
                            else if (*Current == quote) ++Current;
                            else state = TmphParseState.NotNumber;
                        }
                        return;
                    }
                    state = TmphParseState.NotNumber;
                    return;
                }
            }
            if (++Current == end)
            {
                value = number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                value = parseHex64();
                return;
            }
            value = parseUInt64(number);
        }

        /// <summary>
        ///     数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void parse(ref ulong? value)
        {
            var number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (isNull())
                {
                    value = null;
                    return;
                }
                space();
                if (state != TmphParseState.Success) return;
                if (Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                if ((number = (uint)(*Current - '0')) > 9)
                {
                    if (isNull())
                    {
                        value = null;
                        return;
                    }
                    if (*Current == '"' || *Current == '\'')
                    {
                        quote = *Current;
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if (number == 0)
                        {
                            if (*Current == quote)
                            {
                                value = 0;
                                ++Current;
                                return;
                            }
                            if (*Current != 'x')
                            {
                                state = TmphParseState.NotNumber;
                                return;
                            }
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            value = parseHex64();
                        }
                        else value = parseUInt64(number);
                        if (state == TmphParseState.Success)
                        {
                            if (Current == end) state = TmphParseState.CrashEnd;
                            else if (*Current == quote) ++Current;
                            else state = TmphParseState.NotNumber;
                        }
                        return;
                    }
                    state = TmphParseState.NotNumber;
                    return;
                }
            }
            if (++Current == end)
            {
                value = number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                value = parseHex64();
                return;
            }
            value = parseUInt64(number);
        }

        /// <summary>
        ///     数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal void Parse(ref long value)
        {
            var sign = 0;
            var number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (*Current == '-')
                {
                    if (++Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        state = TmphParseState.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                else
                {
                    space();
                    if (state != TmphParseState.Success) return;
                    if (Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        if (*Current == '"' || *Current == '\'')
                        {
                            quote = *Current;
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            if ((number = (uint)(*Current - '0')) > 9)
                            {
                                if (*Current != '-')
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = TmphParseState.CrashEnd;
                                    return;
                                }
                                if ((number = (uint)(*Current - '0')) > 9)
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                sign = 1;
                            }
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            if (number == 0)
                            {
                                if (*Current == quote)
                                {
                                    value = 0;
                                    ++Current;
                                    return;
                                }
                                if (*Current != 'x')
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = TmphParseState.CrashEnd;
                                    return;
                                }
                                value = (long)parseHex64();
                            }
                            else value = (long)parseUInt64(number);
                            if (state == TmphParseState.Success)
                            {
                                if (Current == end) state = TmphParseState.CrashEnd;
                                else if (*Current == quote)
                                {
                                    if (sign != 0) value = -value;
                                    ++Current;
                                }
                                else state = TmphParseState.NotNumber;
                            }
                            return;
                        }
                        if (*Current != '-')
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        sign = 1;
                    }
                }
            }
            if (++Current == end)
            {
                value = sign == 0 ? (int)number : -(long)(int)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                value = (long)parseHex64();
                if (sign != 0) value = -value;
                return;
            }
            value = (long)parseUInt64(number);
            if (sign != 0) value = -value;
        }

        /// <summary>
        ///     数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void parse(ref long? value)
        {
            var sign = 0;
            var number = (uint)(*Current - '0');
            if (number > 9)
            {
                if (isNull())
                {
                    value = null;
                    return;
                }
                if (*Current == '-')
                {
                    if (++Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        state = TmphParseState.NotNumber;
                        return;
                    }
                    sign = 1;
                }
                else
                {
                    space();
                    if (state != TmphParseState.Success) return;
                    if (Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    if ((number = (uint)(*Current - '0')) > 9)
                    {
                        if (isNull())
                        {
                            value = null;
                            return;
                        }
                        if (*Current == '"' || *Current == '\'')
                        {
                            quote = *Current;
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            if ((number = (uint)(*Current - '0')) > 9)
                            {
                                if (*Current != '-')
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = TmphParseState.CrashEnd;
                                    return;
                                }
                                if ((number = (uint)(*Current - '0')) > 9)
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                sign = 1;
                            }
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            if (number == 0)
                            {
                                if (*Current == quote)
                                {
                                    value = 0;
                                    ++Current;
                                    return;
                                }
                                if (*Current != 'x')
                                {
                                    state = TmphParseState.NotNumber;
                                    return;
                                }
                                if (++Current == end)
                                {
                                    state = TmphParseState.CrashEnd;
                                    return;
                                }
                                value = (long)parseHex64();
                            }
                            else value = (long)parseUInt64(number);
                            if (state == TmphParseState.Success)
                            {
                                if (Current == end) state = TmphParseState.CrashEnd;
                                else if (*Current == quote)
                                {
                                    if (sign != 0) value = -value;
                                    ++Current;
                                }
                                else state = TmphParseState.NotNumber;
                            }
                            return;
                        }
                        if (*Current != '-')
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if ((number = (uint)(*Current - '0')) > 9)
                        {
                            state = TmphParseState.NotNumber;
                            return;
                        }
                        sign = 1;
                    }
                }
            }
            if (++Current == end)
            {
                value = sign == 0 ? (int)number : -(long)(int)number;
                return;
            }
            if (number == 0)
            {
                if (*Current != 'x')
                {
                    value = 0;
                    return;
                }
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                var hexValue = (long)parseHex64();
                value = sign == 0 ? hexValue : -hexValue;
                return;
            }
            var value64 = (long)parseUInt64(number);
            value = sign == 0 ? value64 : -value64;
        }

        /// <summary>
        ///     查找数字结束位置
        /// </summary>
        /// <returns>数字结束位置,失败返回null</returns>
        private char* searchNumber()
        {
            if (*Current >= NumberMapSize || !numberMap.Get(*Current))
            {
                space();
                if (state != TmphParseState.Success) return null;
                if (Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return null;
                }
                if (*Current == '"' || *Current == '\'')
                {
                    quote = *Current;
                    if (++Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return null;
                    }
                    var stringEnd = Current;
                    if (endChar == quote)
                    {
                        while (*stringEnd != quote) ++stringEnd;
                    }
                    else
                    {
                        while (*stringEnd != quote)
                        {
                            if (++stringEnd == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return null;
                            }
                        }
                    }
                    return stringEnd;
                }
                if (*Current >= NumberMapSize || !numberMap.Get(*Current))
                {
                    if (isNaN()) return jsonFixed;
                    state = TmphParseState.NotNumber;
                    return null;
                }
            }
            var numberEnd = Current;
            if (isEndNumber)
            {
                while (++numberEnd != end && *numberEnd < NumberMapSize && numberMap.Get(*numberEnd)) ;
            }
            else
            {
                while (*++numberEnd < NumberMapSize && numberMap.Get(*numberEnd)) ;
            }
            quote = (char)0;
            return numberEnd;
        }

        /// <summary>
        ///     查找数字结束位置
        /// </summary>
        /// <returns>数字结束位置,失败返回null</returns>
        private char* searchNumberNull()
        {
            if (*Current >= NumberMapSize || !numberMap.Get(*Current))
            {
                if (isNull()) return jsonFixed;
                space();
                if (state != TmphParseState.Success) return null;
                if (Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return null;
                }
                if (*Current == '"' || *Current == '\'')
                {
                    quote = *Current;
                    if (++Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return null;
                    }
                    var stringEnd = Current;
                    if (endChar == quote)
                    {
                        while (*stringEnd != quote) ++stringEnd;
                    }
                    else
                    {
                        while (*stringEnd != quote)
                        {
                            if (++stringEnd == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return null;
                            }
                        }
                    }
                    return stringEnd;
                }
                if (*Current >= NumberMapSize || !numberMap.Get(*Current))
                {
                    if (isNull() || isNaN()) return jsonFixed;
                    state = TmphParseState.NotNumber;
                    return null;
                }
            }
            var numberEnd = Current;
            if (isEndNumber)
            {
                while (++numberEnd != end && *numberEnd < NumberMapSize && numberMap.Get(*numberEnd)) ;
            }
            else
            {
                while (*++numberEnd < NumberMapSize && numberMap.Get(*numberEnd)) ;
            }
            quote = (char)0;
            return numberEnd;
        }

        /// <summary>
        ///     数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal void Parse(ref float value)
        {
            var end = searchNumber();
            if (end != null)
            {
                if (end == jsonFixed) value = float.NaN;
                else
                {
                    var number = json == null
                        ? new string(Current, 0, (int)(end - Current))
                        : json.Substring((int)(Current - jsonFixed), (int)(end - Current));
                    if (float.TryParse(number, out value))
                    {
                        Current = end;
                        if (quote != 0) ++Current;
                    }
                    else state = TmphParseState.NotNumber;
                }
            }
        }

        /// <summary>
        ///     数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void parse(ref float? value)
        {
            var end = searchNumberNull();
            if (end != null)
            {
                if (end == jsonFixed)
                {
                    if (*(Current - 1) == 'l') value = null;
                    else value = float.NaN;
                }
                else
                {
                    var number = json == null
                        ? new string(Current, 0, (int)(end - Current))
                        : json.Substring((int)(Current - jsonFixed), (int)(end - Current));
                    float parseValue;
                    if (float.TryParse(number, out parseValue))
                    {
                        Current = end;
                        value = parseValue;
                        if (quote != 0) ++Current;
                    }
                    else state = TmphParseState.NotNumber;
                }
            }
        }

        /// <summary>
        ///     数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal void Parse(ref double value)
        {
            var end = searchNumber();
            if (end != null)
            {
                if (end == jsonFixed) value = double.NaN;
                else
                {
                    var number = json == null
                        ? new string(Current, 0, (int)(end - Current))
                        : json.Substring((int)(Current - jsonFixed), (int)(end - Current));
                    if (double.TryParse(number, out value))
                    {
                        Current = end;
                        if (quote != 0) ++Current;
                    }
                    else state = TmphParseState.NotNumber;
                }
            }
        }

        /// <summary>
        ///     数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void parse(ref double? value)
        {
            var end = searchNumberNull();
            if (end != null)
            {
                if (end == jsonFixed)
                {
                    if (*(Current - 1) == 'l') value = null;
                    else value = double.NaN;
                }
                else
                {
                    var number = json == null
                        ? new string(Current, 0, (int)(end - Current))
                        : json.Substring((int)(Current - jsonFixed), (int)(end - Current));
                    double parseValue;
                    if (double.TryParse(number, out parseValue))
                    {
                        Current = end;
                        value = parseValue;
                        if (quote != 0) ++Current;
                    }
                    else state = TmphParseState.NotNumber;
                }
            }
        }

        /// <summary>
        ///     数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal void Parse(ref decimal value)
        {
            var end = searchNumber();
            if (end != null)
            {
                if (end == jsonFixed) state = TmphParseState.NotNumber;
                else
                {
                    var number = json == null
                        ? new string(Current, 0, (int)(end - Current))
                        : json.Substring((int)(Current - jsonFixed), (int)(end - Current));
                    if (decimal.TryParse(number, out value))
                    {
                        Current = end;
                        if (quote != 0) ++Current;
                    }
                    else state = TmphParseState.NotNumber;
                }
            }
        }

        /// <summary>
        ///     数字解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void parse(ref decimal? value)
        {
            var end = searchNumberNull();
            if (end != null)
            {
                if (end == jsonFixed)
                {
                    if (*(Current - 1) == 'l') value = null;
                    else state = TmphParseState.NotNumber;
                }
                else
                {
                    var number = json == null
                        ? new string(Current, 0, (int)(end - Current))
                        : json.Substring((int)(Current - jsonFixed), (int)(end - Current));
                    decimal parseValue;
                    if (decimal.TryParse(number, out parseValue))
                    {
                        Current = end;
                        value = parseValue;
                        if (quote != 0) ++Current;
                    }
                    else state = TmphParseState.NotNumber;
                }
            }
        }

        /// <summary>
        ///     字符解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal void Parse(ref char value)
        {
            if (*Current != '"') goto NOTQUOTES;
            QUOTES:
            if (++Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            if ((value = *Current) == '\\')
            {
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                value = *Current == 'n' ? '\n' : (*Current == 'r' ? '\r' : *Current);
            }
            if (++Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            if (*Current == '"')
            {
                ++Current;
                return;
            }
            state = TmphParseState.NotChar;
            return;
            NOTQUOTES:
            if (*Current != '\'') goto NOTQUOTE;
            QUOTE:
            if (++Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            if ((value = *Current) == '\\')
            {
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                value = *Current == 'n' ? '\n' : (*Current == 'r' ? '\r' : *Current);
            }
            if (++Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            if (*Current == '\'')
            {
                ++Current;
                return;
            }
            NOTQUOTE:
            space();
            if (state != TmphParseState.Success) return;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            if (*Current == '"') goto QUOTES;
            if (*Current == '\'') goto QUOTE;
            state = TmphParseState.NotChar;
        }

        /// <summary>
        ///     字符解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void parse(ref char? value)
        {
            if (*Current != '"') goto NOTQUOTES;
            QUOTES:
            if (++Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            if (*Current == '\\')
            {
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                value = *Current == 'n' ? '\n' : (*Current == 'r' ? '\r' : *Current);
            }
            else value = *Current;
            if (++Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            if (*Current == '"')
            {
                ++Current;
                return;
            }
            state = TmphParseState.NotChar;
            return;
            NOTQUOTES:
            if (*Current != '\'') goto NOTQUOTE;
            QUOTE:
            if (++Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            if (*Current == '\\')
            {
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                value = *Current == 'n' ? '\n' : (*Current == 'r' ? '\r' : *Current);
            }
            else value = *Current;
            if (++Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            if (*Current == '\'')
            {
                ++Current;
                return;
            }
            NOTQUOTE:
            if (isNull())
            {
                value = null;
                return;
            }
            space();
            if (state != TmphParseState.Success) return;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            if (*Current == '"') goto QUOTES;
            if (*Current == '\'') goto QUOTE;
            if (isNull())
            {
                value = null;
                return;
            }
            state = TmphParseState.NotChar;
        }

        /// <summary>
        ///     时间解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal void Parse(ref DateTime value)
        {
            if (*Current != 'n') goto NOTDATETIME;
            DATETIME:
            var count = (int)(end - Current);
            if (count > 9 && *(int*)(Current + 1) == ('e' + ('w' << 16)))
            {
                if (((*(int*)(Current + 3) ^ (' ' + ('D' << 16))) | (*(int*)(Current + 5) ^ ('a' + ('t' << 16))) |
                     (*(int*)(Current + 7) ^ ('e' + ('(' << 16)))) == 0)
                {
                    long millisecond = 0;
                    Current += 9;
                    Parse(ref millisecond);
                    if (state != TmphParseState.Success) return;
                    if (Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    if (*Current == ')')
                    {
                        value = TmphAppSetting.JavascriptMinTime.AddTicks(millisecond * TmphDate.MillisecondTicks);
                        ++Current;
                        return;
                    }
                }
            }
            else if (count >= 4 && ((*(Current + 1) ^ 'u') | (*(int*)(Current + 2) ^ ('l' + ('l' << 16)))) == 0)
            {
                value = DateTime.MinValue;
                Current += 4;
                return;
            }
            state = TmphParseState.NotDateTime;
            return;
            NOTDATETIME:
            if (*Current == '\'' || *Current == '"') goto STRING;
            space();
            if (state != TmphParseState.Success) return;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            if (*Current == 'n') goto DATETIME;
            if (*Current != '\'' && *Current != '"')
            {
                state = TmphParseState.NotDateTime;
                return;
            }
            STRING:
            var timeString = parseString();
            if (timeString != null)
            {
                DateTime parseTime;
                if (DateTime.TryParse(timeString, out parseTime))
                {
                    value = parseTime;
                    return;
                }
            }
            state = TmphParseState.NotDateTime;
        }

        /// <summary>
        ///     时间解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void parse(ref DateTime? value)
        {
            if (*Current != 'n') goto NOTDATETIME;
            DATETIME:
            var count = (int)(end - Current);
            if (count > 9 && *(int*)(Current + 1) == ('e' + ('w' << 16)))
            {
                if (((*(int*)(Current + 3) ^ (' ' + ('D' << 16))) | (*(int*)(Current + 5) ^ ('a' + ('t' << 16))) |
                     (*(int*)(Current + 7) ^ ('e' + ('(' << 16)))) == 0)
                {
                    long millisecond = 0;
                    Current += 9;
                    Parse(ref millisecond);
                    if (state != TmphParseState.Success) return;
                    if (Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    if (*Current == ')')
                    {
                        value = TmphAppSetting.JavascriptMinTime.AddTicks(millisecond * TmphDate.MillisecondTicks);
                        ++Current;
                        return;
                    }
                }
            }
            else if (count >= 4 && ((*(Current + 1) ^ 'u') | (*(int*)(Current + 2) ^ ('l' + ('l' << 16)))) == 0)
            {
                value = null;
                Current += 4;
                return;
            }
            state = TmphParseState.NotDateTime;
            return;
            NOTDATETIME:
            if (*Current == '\'' || *Current == '"') goto STRING;
            space();
            if (state != TmphParseState.Success) return;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            if (*Current == 'n') goto DATETIME;
            if (*Current != '\'' && *Current != '"')
            {
                state = TmphParseState.NotDateTime;
                return;
            }
            STRING:
            var timeString = parseString();
            if (timeString != null)
            {
                DateTime parseTime;
                if (DateTime.TryParse(timeString, out parseTime))
                {
                    value = parseTime;
                    return;
                }
            }
            state = TmphParseState.NotDateTime;
        }

        /// <summary>
        ///     Guid解析
        /// </summary>
        /// <param name="value">数据</param>
        private void parse(ref Guid value)
        {
            if (end - Current < 38)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            quote = *Current;
            var guid = new TmphGuid();
            guid.Byte3 = (byte)parseHex2();
            guid.Byte2 = (byte)parseHex2();
            guid.Byte1 = (byte)parseHex2();
            guid.Byte0 = (byte)parseHex2();
            if (*++Current != '-')
            {
                state = TmphParseState.NotGuid;
                return;
            }
            guid.Byte45 = (ushort)parseHex4();
            if (*++Current != '-')
            {
                state = TmphParseState.NotGuid;
                return;
            }
            guid.Byte67 = (ushort)parseHex4();
            if (*++Current != '-')
            {
                state = TmphParseState.NotGuid;
                return;
            }
            guid.Byte8 = (byte)parseHex2();
            guid.Byte9 = (byte)parseHex2();
            if (*++Current != '-')
            {
                state = TmphParseState.NotGuid;
                return;
            }
            guid.Byte10 = (byte)parseHex2();
            guid.Byte11 = (byte)parseHex2();
            guid.Byte12 = (byte)parseHex2();
            guid.Byte13 = (byte)parseHex2();
            guid.Byte14 = (byte)parseHex2();
            guid.Byte15 = (byte)parseHex2();
            if (*++Current == quote)
            {
                value = guid.Value;
                ++Current;
                return;
            }
            state = TmphParseState.NotGuid;
        }

        /// <summary>
        ///     Guid解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal void Parse(ref Guid value)
        {
            if (*Current == '\'' || *Current == '"')
            {
                parse(ref value);
                return;
            }
            space();
            if (state != TmphParseState.Success) return;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            if (*Current == '\'' || *Current == '"')
            {
                parse(ref value);
                return;
            }
            state = TmphParseState.NotGuid;
        }

        /// <summary>
        ///     Guid解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void parse(ref Guid? value)
        {
            if (*Current == '\'' || *Current == '"')
            {
                var guid = new Guid();
                parse(ref guid);
                value = guid;
                return;
            }
            if (isNull())
            {
                value = null;
                return;
            }
            space();
            if (state != TmphParseState.Success) return;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            if (*Current == '\'' || *Current == '"')
            {
                var guid = new Guid();
                parse(ref guid);
                value = guid;
                return;
            }
            if (isNull())
            {
                value = null;
                return;
            }
            state = TmphParseState.NotGuid;
        }

        /// <summary>
        ///     查找字符串中的转义符
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void searchEscape()
        {
            if (endChar == quote)
            {
                while (*Current != quote && *Current != '\\')
                {
                    if (*Current == '\n')
                    {
                        state = TmphParseState.StringEnter;
                        return;
                    }
                    ++Current;
                }
            }
            else
            {
                while (*Current != quote && *Current != '\\')
                {
                    if (*Current == '\n')
                    {
                        state = TmphParseState.StringEnter;
                        return;
                    }
                    if (++Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                }
            }
        }

        /// <summary>
        ///     解析16进制字符
        /// </summary>
        /// <returns>字符</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private uint parseHex4()
        {
            uint code = (uint)(*++Current - '0'), number = (uint)(*++Current - '0');
            if (code > 9) code = ((code - ('A' - '0')) & 0xffdfU) + 10;
            if (number > 9) number = ((number - ('A' - '0')) & 0xffdfU) + 10;
            code <<= 12;
            code += (number << 8);
            if ((number = (uint)(*++Current - '0')) > 9) number = ((number - ('A' - '0')) & 0xffdfU) + 10;
            code += (number << 4);
            number = (uint)(*++Current - '0');
            return code + (number > 9 ? (((number - ('A' - '0')) & 0xffdfU) + 10) : number);
        }

        /// <summary>
        ///     解析16进制字符
        /// </summary>
        /// <returns>字符</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private uint parseHex2()
        {
            uint code = (uint)(*++Current - '0'), number = (uint)(*++Current - '0');
            if (code > 9) code = ((code - ('A' - '0')) & 0xffdfU) + 10;
            return (number > 9 ? (((number - ('A' - '0')) & 0xffdfU) + 10) : number) + (code << 4);
        }

        /// <summary>
        ///     字符串转义解析
        /// </summary>
        /// <returns>写入结束位置</returns>
        private char* parseEscape()
        {
            var write = Current;
            do
            {
                if (*++Current == 'u')
                {
                    if ((int)(end - Current) < 5)
                    {
                        state = TmphParseState.CrashEnd;
                        return null;
                    }
                    *write++ = (char)parseHex4();
                }
                else if (*Current == 'x')
                {
                    if ((int)(end - Current) < 3)
                    {
                        state = TmphParseState.CrashEnd;
                        return null;
                    }
                    *write++ = (char)parseHex2();
                }
                else
                {
                    if (Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return null;
                    }
                    *write++ = *Current < escapeCharSize ? escapeCharData.Char[*Current] : *Current;
                }
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return null;
                }
                if (endChar == quote)
                {
                    while (*Current != quote && *Current != '\\')
                    {
                        if (*Current == '\n')
                        {
                            state = TmphParseState.StringEnter;
                            return null;
                        }
                        *write++ = *Current++;
                    }
                }
                else
                {
                    while (*Current != quote && *Current != '\\')
                    {
                        if (*Current == '\n')
                        {
                            state = TmphParseState.StringEnter;
                            return null;
                        }
                        *write++ = *Current++;
                        if (Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return null;
                        }
                    }
                }
                if (*Current == quote)
                {
                    ++Current;
                    return write;
                }
            } while (true);
        }

        /// <summary>
        ///     获取转义后的字符串长度
        /// </summary>
        /// <returns>字符串长度</returns>
        private int parseEscapeSize()
        {
            var start = Current;
            var length = 0;
            do
            {
                if (*++Current == 'u')
                {
                    if ((int)(end - Current) < 5)
                    {
                        state = TmphParseState.CrashEnd;
                        return 0;
                    }
                    length += 5;
                    Current += 5;
                }
                else if (*Current == 'x')
                {
                    if ((int)(end - Current) < 3)
                    {
                        state = TmphParseState.CrashEnd;
                        return 0;
                    }
                    length += 3;
                    Current += 3;
                }
                else
                {
                    if (Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return 0;
                    }
                    ++length;
                    ++Current;
                }
                if (Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return 0;
                }
                if (endChar == quote)
                {
                    while (*Current != quote && *Current != '\\')
                    {
                        if (*Current == '\n')
                        {
                            state = TmphParseState.StringEnter;
                            return 0;
                        }
                        ++Current;
                    }
                }
                else
                {
                    while (*Current != quote && *Current != '\\')
                    {
                        if (*Current == '\n')
                        {
                            state = TmphParseState.StringEnter;
                            return 0;
                        }
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return 0;
                        }
                    }
                }
                if (*Current == quote)
                {
                    length = (int)(Current - start) - length;
                    Current = start;
                    return length;
                }
            } while (true);
        }

        /// <summary>
        ///     字符串转义解析
        /// </summary>
        /// <param name="write">当前写入位置</param>
        private void parseEscapeUnsafe(char* write)
        {
            do
            {
                if (*++Current == 'u') *write++ = (char)parseHex4();
                else if (*Current == 'x') *write++ = (char)parseHex2();
                else *write++ = *Current < escapeCharSize ? escapeCharData.Char[*Current] : *Current;
                while (*++Current != quote && *Current != '\\') *write++ = *Current;
                if (*Current == quote)
                {
                    ++Current;
                    return;
                }
            } while (true);
        }

        /// <summary>
        ///     字符串解析
        /// </summary>
        /// <returns>字符串,失败返回null</returns>
        private string parseString()
        {
            quote = *Current;
            if (++Current == end)
            {
                state = TmphParseState.CrashEnd;
                return null;
            }
            var start = Current;
            searchEscape();
            if (state != TmphParseState.Success) return null;
            if (*Current == quote) return new string(start, 0, (int)(Current++ - start));
            if (parseConfig.IsTempString)
            {
                var writeEnd = parseEscape();
                return writeEnd != null ? new string(start, 0, (int)(writeEnd - start)) : null;
            }
            return parseEscape(start);
        }

        /// <summary>
        ///     字符串解析
        /// </summary>
        /// <param name="start"></param>
        /// <returns>字符串,失败返回null</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private string parseEscape(char* start)
        {
            var size = parseEscapeSize();
            if (size != 0)
            {
                var left = (int)(Current - start);
                var value = TmphString.FastAllocateString(left + size);
                fixed (char* valueFixed = value)
                {
                    Unsafe.TmphMemory.Copy((void*)start, valueFixed, left << 1);
                    parseEscapeUnsafe(valueFixed + left);
                    return value;
                }
            }
            return null;
        }

        /// <summary>
        ///     查找枚举数字
        /// </summary>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private bool isEnumNumber()
        {
            if ((uint)(*Current - '0') < 10) return true;
            space();
            if (state != TmphParseState.Success) return false;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return false;
            }
            return (uint)(*Current - '0') < 10;
        }

        /// <summary>
        ///     查找枚举数字
        /// </summary>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private bool isEnumNumberFlag()
        {
            if ((uint)(*Current - '0') < 10 || *Current == '-') return true;
            space();
            if (state != TmphParseState.Success) return false;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return false;
            }
            return (uint)(*Current - '0') < 10 || *Current == '-';
        }

        /// <summary>
        ///     查找字符串引号并返回第一个字符
        /// </summary>
        /// <returns>第一个字符,0表示null</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private char searchQuote()
        {
            if (*Current == '\'' || *Current == '"')
            {
                quote = *Current;
                return nextStringChar();
            }
            if (isNull()) return quote = (char)0;
            space();
            if (++Current == end)
            {
                state = TmphParseState.CrashEnd;
                return (char)0;
            }
            if (*Current == '\'' || *Current == '"')
            {
                quote = *Current;
                return nextStringChar();
            }
            if (isNull()) return quote = (char)0;
            state = TmphParseState.NotString;
            return (char)0;
        }

        /// <summary>
        ///     读取下一个字符
        /// </summary>
        /// <returns>字符,结束或者错误返回0</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private char nextStringChar()
        {
            if (++Current == end)
            {
                state = TmphParseState.CrashEnd;
                return (char)0;
            }
            if (*Current == quote)
            {
                ++Current;
                return quote = (char)0;
            }
            if (*Current == '\\')
            {
                if (*++Current == 'u')
                {
                    if ((int)(end - Current) < 5)
                    {
                        state = TmphParseState.CrashEnd;
                        return (char)0;
                    }
                    return (char)parseHex4();
                }
                if (*Current == 'x')
                {
                    if ((int)(end - Current) < 3)
                    {
                        state = TmphParseState.CrashEnd;
                        return (char)0;
                    }
                    return (char)parseHex2();
                }
                if (Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return (char)0;
                }
                return *Current < escapeCharSize ? escapeCharData.Char[*Current] : *Current;
            }
            if (*Current == '\n')
            {
                state = TmphParseState.StringEnter;
                return (char)0;
            }
            return *Current;
        }

        ///// <summary>
        ///// 判断是否字符串结束引号
        ///// </summary>
        ///// <returns>是否字符串结束</returns>
        //[TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        //private bool isNextStringQuote()
        //{
        //    if (++current == end)
        //    {
        //        state = parseState.CrashEnd;
        //        return false;
        //    }
        //    if (*current == quote)
        //    {
        //        ++current;
        //        quote = (char)0;
        //        return true;
        //    }
        //    if (*current == '\\')
        //    {
        //        if (*++current == 'u')
        //        {
        //            if ((int)(end - current) < 5) state = parseState.CrashEnd;
        //            else current += 4;
        //        }
        //        else if (*current == 'x')
        //        {
        //            if ((int)(end - current) < 3) state = parseState.CrashEnd;
        //            else current += 2;
        //        }
        //        else if (current == end) state = parseState.CrashEnd;
        //        else ++current;
        //    }
        //    else if (*current == '\n') state = parseState.StringEnter;
        //    return false;
        //}
        /// <summary>
        ///     查找字符串直到结束
        /// </summary>
        private void searchStringEnd()
        {
            if (quote != 0 && state == TmphParseState.Success)
            {
                ++Current;
                do
                {
                    if (Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    if (endChar == quote)
                    {
                        while (*Current != quote && *Current != '\\')
                        {
                            if (*Current == '\n')
                            {
                                state = TmphParseState.StringEnter;
                                return;
                            }
                            ++Current;
                        }
                    }
                    else
                    {
                        while (*Current != quote && *Current != '\\')
                        {
                            if (*Current == '\n')
                            {
                                state = TmphParseState.StringEnter;
                                return;
                            }
                            if (++Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                        }
                    }
                    if (*Current == quote)
                    {
                        ++Current;
                        return;
                    }
                    if (*++Current == 'u')
                    {
                        if ((int)(end - Current) < 5)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        Current += 5;
                    }
                    else if (*Current == 'x')
                    {
                        if ((int)(end - Current) < 3)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        Current += 3;
                    }
                    else
                    {
                        if (Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        ++Current;
                    }
                } while (true);
            }
        }

        /// <summary>
        ///     字符串解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal void Parse(ref string value)
        {
            if (*Current == '\'' || *Current == '"')
            {
                value = parseString();
                return;
            }
            if (isNull())
            {
                value = null;
                return;
            }
            space();
            if (state != TmphParseState.Success) return;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            if (*Current == '\'' || *Current == '"')
            {
                value = parseString();
                return;
            }
            if (isNull())
            {
                value = null;
                return;
            }
            state = TmphParseState.NotString;
        }

        /// <summary>
        ///     字符串解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void parse(ref TmphSubString value)
        {
            if (*Current == '\'' || *Current == '"') goto STRING;
            if (isNull())
            {
                value.Null();
                return;
            }
            space();
            if (state != TmphParseState.Success) return;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            if (*Current != '\'' && *Current != '"')
            {
                if (isNull())
                {
                    value.Null();
                    return;
                }
                state = TmphParseState.NotString;
                return;
            }
            STRING:
            quote = *Current;
            if (++Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            var start = Current;
            searchEscape();
            if (state != TmphParseState.Success) return;
            if (*Current == quote)
            {
                if (json == null) value = new string(start, 0, (int)(Current++ - start));
                else value.UnsafeSet(json, (int)(start - jsonFixed), (int)(Current++ - start));
                return;
            }
            if (parseConfig.IsTempString && json != null)
            {
                var writeEnd = parseEscape();
                if (writeEnd != null) value.UnsafeSet(json, (int)(start - jsonFixed), (int)(writeEnd - start));
            }
            else
            {
                var newValue = parseEscape(start);
                if (newValue != null) value.UnsafeSet(newValue, 0, newValue.Length);
            }
        }

        /// <summary>
        ///     JSON节点解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        private void parse(ref TmphJsonNode value)
        {
            space();
            if (state != TmphParseState.Success) return;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            switch (*Current)
            {
                case '"':
                case '\'':
                    parseStringNode(ref value);
                    return;

                case '{':
                    var dictionary = parseDictionaryNode();
                    if (state == TmphParseState.Success) value.SetDictionary(dictionary);
                    return;

                case '[':
                    var list = parseListNode();
                    if (state == TmphParseState.Success) value.SetList(list);
                    {
                        value.Type = TmphJsonNode.TmphType.List;
                    }
                    return;

                case 'n':
                    if (*(Current + 1) == 'u')
                    {
                        if ((int)(end - Current) < 4 || (*(int*)(Current + 2) ^ ('l' + ('l' << 16))) != 0)
                            state = TmphParseState.NotNull;
                        else
                        {
                            value.Type = TmphJsonNode.TmphType.Null;
                            Current += 4;
                        }
                        return;
                    }
                    if ((int)(end - Current) > 9 && *(int*)(Current + 1) == ('e' + ('w' << 16))
                        &&
                        ((*(int*)(Current + 3) ^ (' ' + ('D' << 16))) | (*(int*)(Current + 5) ^ ('a' + ('t' << 16))) |
                         (*(int*)(Current + 7) ^ ('e' + ('(' << 16)))) == 0)
                    {
                        long millisecond = 0;
                        Current += 9;
                        Parse(ref millisecond);
                        if (state != TmphParseState.Success) return;
                        if (Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                        if (*Current == ')')
                        {
                            value.Int64 = TmphAppSetting.JavascriptMinTime.Ticks + millisecond * TmphDate.MillisecondTicks;
                            value.Type = TmphJsonNode.TmphType.DateTimeTick;
                            ++Current;
                            return;
                        }
                    }
                    break;

                case 't':
                    if ((int)(end - Current) < 4 ||
                        ((*(Current + 1) ^ 'r') | (*(int*)(Current + 2) ^ ('u' + ('e' << 16)))) != 0)
                        state = TmphParseState.NotBool;
                    else
                    {
                        Current += 4;
                        value.Int64 = 1;
                        value.Type = TmphJsonNode.TmphType.Bool;
                    }
                    return;

                case 'f':
                    if (((int)(end - Current)) < 5
                        ||
                        ((*(int*)(Current + 1) ^ ('a' + ('l' << 16))) | (*(int*)(Current + 3) ^ ('s' + ('e' << 16)))) !=
                        0)
                        state = TmphParseState.NotBool;
                    else
                    {
                        Current += 5;
                        value.Int64 = 0;
                        value.Type = TmphJsonNode.TmphType.Bool;
                    }
                    return;

                default:
                    var numberEnd = searchNumber();
                    if (numberEnd != null)
                    {
                        if (numberEnd == jsonFixed) value.Type = TmphJsonNode.TmphType.NaN;
                        else
                        {
                            if (json == null) value.String = new string(Current, 0, (int)(numberEnd - Current));
                            else value.String.UnsafeSet(json, (int)(Current - jsonFixed), (int)(numberEnd - Current));
                            Current = numberEnd;
                            if (quote != 0) ++Current;
                            value.SetNumberString(quote);
                        }
                        return;
                    }
                    break;
            }
            state = TmphParseState.UnknownValue;
        }

        /// <summary>
        ///     解析字符串节点
        /// </summary>
        /// <param name="value"></param>
        private void parseStringNode(ref TmphJsonNode value)
        {
            quote = *Current;
            if (++Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            var start = Current;
            searchEscape();
            if (state != TmphParseState.Success) return;
            if (*Current == quote)
            {
                if (json == null) value.String = new string(start, 0, (int)(Current++ - start));
                else value.String.UnsafeSet(json, (int)(start - jsonFixed), (int)(Current++ - start));
                value.Type = TmphJsonNode.TmphType.String;
                return;
            }
            if (json != null)
            {
                var escapeStart = Current;
                searchEscapeEnd();
                if (state == TmphParseState.Success)
                {
                    value.String.UnsafeSet(json, (int)(start - jsonFixed), (int)(Current - start));
                    value.SetQuoteString((int)(escapeStart - start), quote, parseConfig.IsTempString);
                    ++Current;
                }
            }
            else
            {
                var newValue = parseEscape(start);
                if (newValue != null)
                {
                    value.String.UnsafeSet(newValue, 0, newValue.Length);
                    value.Type = TmphJsonNode.TmphType.String;
                }
            }
        }

        /// <summary>
        ///     查找转义字符串结束位置
        /// </summary>
        private void searchEscapeEnd()
        {
            do
            {
                if (*++Current == 'u')
                {
                    if ((int)(end - Current) < 5)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    Current += 5;
                }
                else if (*Current == 'x')
                {
                    if ((int)(end - Current) < 3)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    Current += 3;
                }
                else
                {
                    if (Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    ++Current;
                }
                if (Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                if (endChar == quote)
                {
                    while (*Current != quote && *Current != '\\')
                    {
                        if (*Current == '\n')
                        {
                            state = TmphParseState.StringEnter;
                            return;
                        }
                        ++Current;
                    }
                }
                else
                {
                    while (*Current != quote && *Current != '\\')
                    {
                        if (*Current == '\n')
                        {
                            state = TmphParseState.StringEnter;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                    }
                }
                if (*Current == quote) return;
            } while (true);
        }

        /// <summary>
        ///     字符串转义解析
        /// </summary>
        /// <param name="value"></param>
        /// <param name="escapeIndex">未解析字符串起始位置</param>
        /// <param name="quote">字符串引号</param>
        /// <returns></returns>
        private TmphSubString parseQuoteString(TmphSubString value, int escapeIndex, char quote, int isTempString)
        {
            fixed (char* jsonFixed = value.value)
            {
                var start = jsonFixed + value.StartIndex;
                end = start + value.Length;
                this.quote = quote;
                Current = start + escapeIndex;
                endChar = *(end - 1);
                if (isTempString == 0)
                {
                    var newValue = parseEscape(start);
                    if (newValue != null) return newValue;
                }
                else
                {
                    var writeEnd = parseEscape();
                    if (writeEnd != null)
                        return TmphSubString.Unsafe(value.value, (int)(start - jsonFixed), (int)(writeEnd - start));
                }
            }
            return default(TmphSubString);
        }

        /// <summary>
        ///     解析列表节点
        /// </summary>
        /// <returns></returns>
        private TmphSubArray<TmphJsonNode> parseListNode()
        {
            if (++Current == end)
            {
                state = TmphParseState.CrashEnd;
                return default(TmphSubArray<TmphJsonNode>);
            }
            var value = default(TmphSubArray<TmphJsonNode>);
            while (isNextArrayValue())
            {
                var node = default(TmphJsonNode);
                parse(ref node);
                if (state != TmphParseState.Success) return default(TmphSubArray<TmphJsonNode>);
                value.Add(node);
            }
            return value;
        }

        /// <summary>
        ///     解析字典节点
        /// </summary>
        /// <returns></returns>
        private TmphSubArray<TmphKeyValue<TmphJsonNode, TmphJsonNode>> parseDictionaryNode()
        {
            if (++Current == end)
            {
                state = TmphParseState.CrashEnd;
                return default(TmphSubArray<TmphKeyValue<TmphJsonNode, TmphJsonNode>>);
            }
            var value = default(TmphSubArray<TmphKeyValue<TmphJsonNode, TmphJsonNode>>);
            if (isFirstObject())
            {
                do
                {
                    var name = default(TmphJsonNode);
                    if (*Current == '"' || *Current == '\'') parseStringNode(ref name);
                    else
                    {
                        var nameStart = Current;
                        searchNameEnd();
                        if (json == null) name.String = new string(nameStart, 0, (int)(Current - nameStart));
                        else name.String.UnsafeSet(json, (int)(nameStart - jsonFixed), (int)(Current - nameStart));
                        name.Type = TmphJsonNode.TmphType.String;
                    }
                    if (state != TmphParseState.Success || searchColon() == 0)
                        return default(TmphSubArray<TmphKeyValue<TmphJsonNode, TmphJsonNode>>);
                    var node = default(TmphJsonNode);
                    parse(ref node);
                    if (state != TmphParseState.Success) return default(TmphSubArray<TmphKeyValue<TmphJsonNode, TmphJsonNode>>);
                    value.Add(new TmphKeyValue<TmphJsonNode, TmphJsonNode>(name, node));
                } while (isNextObject());
            }
            return value;
        }

        /// <summary>
        ///     对象解析
        /// </summary>
        /// <param name="value">数据</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void parse(ref object value)
        {
            if (isNull())
            {
                value = null;
                return;
            }
            space();
            if (state != TmphParseState.Success) return;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            if (isNull())
            {
                value = null;
                return;
            }
            ignore();
            if (state == TmphParseState.Success) value = new object();
        }

        /// <summary>
        ///     类型解析
        /// </summary>
        /// <param name="type">类型</param>
        [TmphParseMethod]
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal void Parse(ref Type type)
        {
            if (*Current == '{')
            {
                var remoteType = default(TmphRemoteType);
                TmphTypeParser<TmphRemoteType>.Parse(this, ref remoteType);
                if (!remoteType.TryGet(out type)) state = TmphParseState.ErrorType;
                return;
            }
            if (isNull())
            {
                type = null;
                return;
            }
            space();
            if (*Current == '{')
            {
                var remoteType = default(TmphRemoteType);
                TmphTypeParser<TmphRemoteType>.Parse(this, ref remoteType);
                if (!remoteType.TryGet(out type)) state = TmphParseState.ErrorType;
                return;
            }
            if (isNull())
            {
                type = null;
                return;
            }
            state = TmphParseState.ErrorType;
        }

        /// <summary>
        ///     查找枚举引号并返回第一个字符
        /// </summary>
        /// <returns>第一个字符,0表示null</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private char searchEnumQuote()
        {
            if (*Current == '\'' || *Current == '"')
            {
                quote = *Current;
                return nextEnumChar();
            }
            if (isNull()) return quote = (char)0;
            space();
            if (++Current == end)
            {
                state = TmphParseState.CrashEnd;
                return (char)0;
            }
            if (*Current == '\'' || *Current == '"')
            {
                quote = *Current;
                return nextEnumChar();
            }
            if (isNull()) return quote = (char)0;
            state = TmphParseState.NotEnumChar;
            return (char)0;
        }

        /// <summary>
        ///     获取下一个枚举字符
        /// </summary>
        /// <returns>下一个枚举字符,0表示null</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private char nextEnumChar()
        {
            if (++Current == end)
            {
                state = TmphParseState.CrashEnd;
                return (char)0;
            }
            if (*Current == quote)
            {
                ++Current;
                return quote = (char)0;
            }
            if (*Current == '\\' || *Current == '\n')
            {
                state = TmphParseState.NotEnumChar;
                return (char)0;
            }
            return *Current;
        }

        /// <summary>
        ///     查找下一个枚举字符
        /// </summary>
        /// <returns>下一个枚举字符,0表示null</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private char searchNextEnum()
        {
            do
            {
                if (*Current == ',')
                {
                    do
                    {
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return (char)0;
                        }
                    } while (*Current == ' ');
                    if (*Current == quote)
                    {
                        ++Current;
                        return quote = (char)0;
                    }
                    if (*Current == '\\' || *Current == '\n')
                    {
                        state = TmphParseState.NotEnumChar;
                        return (char)0;
                    }
                    return *Current;
                }
                if (*Current == quote)
                {
                    ++Current;
                    return quote = (char)0;
                }
                if (*Current == '\\' || *Current == '\n')
                {
                    state = TmphParseState.NotEnumChar;
                    return (char)0;
                }
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return (char)0;
                }
            } while (true);
        }

        /// <summary>
        ///     查找数组起始位置
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="value">目标数组</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void searchArray<TValueType>(ref TValueType[] value)
        {
            if (*Current == '[')
            {
                ++Current;
                value = TmphNullValue<TValueType>.Array;
                return;
            }
            if (isNull())
            {
                value = null;
                return;
            }
            space();
            if (state != TmphParseState.Success) return;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            if (*Current == '[')
            {
                ++Current;
                value = TmphNullValue<TValueType>.Array;
                return;
            }
            state = TmphParseState.CrashEnd;
        }

        /// <summary>
        ///     是否存在下一个数组数据
        /// </summary>
        /// <returns>是否存在下一个数组数据</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private bool isFirstArrayValue()
        {
            if (*Current == ']')
            {
                ++Current;
                return false;
            }
            space();
            if (state != TmphParseState.Success) return false;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return false;
            }
            if (*Current == ']')
            {
                ++Current;
                return false;
            }
            return true;
        }

        /// <summary>
        ///     是否存在下一个数组数据
        /// </summary>
        /// <returns>是否存在下一个数组数据</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private bool isNextArrayValue()
        {
            if (*Current == ',')
            {
                ++Current;
                return true;
            }
            if (*Current == ']')
            {
                ++Current;
                return false;
            }
            space();
            if (state != TmphParseState.Success) return false;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return false;
            }
            if (*Current == ',')
            {
                ++Current;
                return true;
            }
            if (*Current == ']')
            {
                ++Current;
                return false;
            }
            state = TmphParseState.NotArrayValue;
            return false;
        }

        /// <summary>
        ///     自定义构造函数数据解析
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void noConstructor<TValueType>(ref TValueType value)
        {
            if (value == null)
            {
                var newValue = parseConfig.Constructor(typeof(TValueType));
                if (newValue == null)
                {
                    state = TmphParseState.NoConstructor;
                    return;
                }
                value = (TValueType)newValue;
            }
            TmphTypeParser<TValueType>.ParseClass(this, ref value);
        }

        /// <summary>
        ///     查找对象起始位置
        /// </summary>
        /// <returns>是否查找到</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private bool searchObject()
        {
            if (*Current == '{')
            {
                ++Current;
                return true;
            }
            if (isNull()) return false;
            space();
            if (state != TmphParseState.Success) return false;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return false;
            }
            if (*Current == '{')
            {
                ++Current;
                return true;
            }
            if (isNull()) return false;
            state = TmphParseState.NotObject;
            return false;
        }

        /// <summary>
        ///     判断是否存在第一个成员
        /// </summary>
        /// <returns>是否存在第一个成员</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private bool isFirstObject()
        {
            if (*Current < NameMapSize && nameStartMap.Get(*Current)) return true;
            if (*Current == '}')
            {
                ++Current;
                return false;
            }
            space();
            if (state != TmphParseState.Success) return false;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return false;
            }
            if (*Current < NameMapSize && nameStartMap.Get(*Current)) return true;
            if (*Current == '}')
            {
                ++Current;
                return false;
            }
            state = TmphParseState.NotFoundName;
            return false;
        }

        /// <summary>
        ///     判断是否存在下一个成员
        /// </summary>
        /// <returns>是否存在下一个成员</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private bool isNextObject()
        {
            if (*Current != ',') goto NOTNEXT;
            NEXT:
            if (++Current == end)
            {
                state = TmphParseState.CrashEnd;
                return false;
            }
            if (*Current < NameMapSize && nameStartMap.Get(*Current)) return true;
            space();
            if (state != TmphParseState.Success) return false;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return false;
            }
            if (*Current < NameMapSize && nameStartMap.Get(*Current)) return true;
            state = TmphParseState.NotFoundName;
            return false;
            NOTNEXT:
            if (*Current == '}')
            {
                ++Current;
                return false;
            }
            space();
            if (state != TmphParseState.Success) return false;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return false;
            }
            if (*Current == ',') goto NEXT;
            if (*Current == '}')
            {
                ++Current;
                return false;
            }
            state = TmphParseState.NotObject;
            return false;
        }

        /// <summary>
        ///     查找冒号
        /// </summary>
        /// <returns>是否找到</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private byte searchColon()
        {
            if (*Current == ':')
            {
                ++Current;
                return 1;
            }
            space();
            if (state != TmphParseState.Success) return 0;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return 0;
            }
            if (*Current == ':')
            {
                ++Current;
                return 1;
            }
            state = TmphParseState.NotFoundColon;
            return 0;
        }

        /// <summary>
        ///     获取成员名称第一个字符
        /// </summary>
        /// <returns>第一个字符,0表示失败</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private char getFirstName()
        {
            if (*Current == '\'' || *Current == '"')
            {
                quote = *Current;
                return nextStringChar();
            }
            quote = (char)0;
            return *Current;
        }

        /// <summary>
        ///     获取成员名称下一个字符
        /// </summary>
        /// <returns>第一个字符,0表示失败</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private char getNextName()
        {
            if (++Current == end)
            {
                state = TmphParseState.CrashEnd;
                return (char)0;
            }
            return *Current < NameMapSize && nameMap.Get(*Current) ? *Current : (char)0;
        }

        /// <summary>
        ///     查找名称直到结束
        /// </summary>
        private void searchNameEnd()
        {
            if (state == TmphParseState.Success)
            {
                while (++Current != end && *Current < NameMapSize && nameMap.Get(*Current)) ;
                if (Current == end)
                {
                    state = TmphParseState.CrashEnd;
                }
            }
        }

        /// <summary>
        ///     忽略对象
        /// </summary>
        private void ignore()
        {
            space();
            if (state != TmphParseState.Success) return;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            switch (*Current)
            {
                case '"':
                case '\'':
                    ignoreString();
                    return;

                case '{':
                    ignoreObject();
                    return;

                case '[':
                    ignoreArray();
                    return;

                case 'n':
                    if ((int)(end - Current) >= 4)
                    {
                        if (*(Current + 1) == 'u')
                        {
                            if (*(int*)(Current + 2) == ('l') + (('l') << 16))
                            {
                                Current += 4;
                                return;
                            }
                        }
                        else if ((int)(end - Current) > 9 &&
                                 ((*(int*)(Current + 1) ^ ('e' + ('w' << 16))) |
                                  (*(int*)(Current + 3) ^ (' ' + ('D' << 16))) |
                                  (*(int*)(Current + 5) ^ ('a' + ('t' << 16))) |
                                  (*(int*)(Current + 7) ^ ('e' + ('(' << 16)))) == 0)
                        {
                            Current += 9;
                            ignoreNumber();
                            if (state != TmphParseState.Success) return;
                            if (Current == end)
                            {
                                state = TmphParseState.CrashEnd;
                                return;
                            }
                            if (*Current == ')')
                            {
                                ++Current;
                                return;
                            }
                        }
                    }
                    break;

                case 't':
                    if ((int)(end - Current) >= 4 &&
                        ((*(Current + 1) ^ 'r') | (*(int*)(Current + 2) ^ ('u' + ('e' << 16)))) == 0)
                    {
                        Current += 4;
                        return;
                    }
                    break;

                case 'f':
                    if (((int)(end - Current)) >= 5 &&
                        ((*(int*)(Current + 1) ^ ('a' + ('l' << 16))) | (*(int*)(Current + 3) ^ ('s' + ('e' << 16)))) ==
                        0)
                    {
                        Current += 5;
                        return;
                    }
                    break;

                default:
                    ignoreNumber();
                    return;
            }
            state = TmphParseState.UnknownValue;
        }

        /// <summary>
        ///     忽略字符串
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void ignoreString()
        {
            quote = *Current;
            if (++Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            var start = Current;
            searchEscape();
            if (state != TmphParseState.Success) return;
            if (*Current == quote)
            {
                ++Current;
                return;
            }
            do
            {
                if (*++Current == 'u')
                {
                    if ((int)(end - Current) < 5)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    Current += 5;
                }
                else if (*Current == 'x')
                {
                    if ((int)(end - Current) < 3)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    Current += 3;
                }
                else
                {
                    if (Current == end)
                    {
                        state = TmphParseState.CrashEnd;
                        return;
                    }
                    ++Current;
                }
                if (Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
                if (endChar == quote)
                {
                    while (*Current != quote && *Current != '\\')
                    {
                        if (*Current == '\n')
                        {
                            state = TmphParseState.StringEnter;
                            return;
                        }
                        ++Current;
                    }
                }
                else
                {
                    while (*Current != quote && *Current != '\\')
                    {
                        if (*Current == '\n')
                        {
                            state = TmphParseState.StringEnter;
                            return;
                        }
                        if (++Current == end)
                        {
                            state = TmphParseState.CrashEnd;
                            return;
                        }
                    }
                }
                if (*Current == quote)
                {
                    ++Current;
                    return;
                }
            } while (true);
        }

        /// <summary>
        ///     忽略对象
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void ignoreObject()
        {
            if (++Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            if (isFirstObject())
            {
                if (*Current == '\'' || *Current == '"') ignoreString();
                else ignoreName();
                if (state != TmphParseState.Success || searchColon() == 0) return;
                ignore();
                while (state == TmphParseState.Success && isNextObject())
                {
                    if (*Current == '\'' || *Current == '"') ignoreString();
                    else ignoreName();
                    if (state != TmphParseState.Success || searchColon() == 0) return;
                    ignore();
                }
            }
        }

        /// <summary>
        ///     忽略成员名称
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void ignoreName()
        {
            do
            {
                if (++Current == end)
                {
                    state = TmphParseState.CrashEnd;
                    return;
                }
            } while (*Current < NameMapSize && nameMap.Get(*Current));
        }

        /// <summary>
        ///     忽略数组
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void ignoreArray()
        {
            if (++Current == end)
            {
                state = TmphParseState.CrashEnd;
                return;
            }
            if (isFirstArrayValue())
            {
                do
                {
                    ignore();
                } while (isNextArrayValue());
            }
        }

        /// <summary>
        ///     忽略数字
        /// </summary>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void ignoreNumber()
        {
            if (*Current < NumberMapSize && numberMap.Get(*Current))
            {
                while (++Current != end && *Current < NumberMapSize && numberMap.Get(*Current)) ;
                return;
            }
            state = TmphParseState.NotNumber;
        }

        /// <summary>
        ///     查找字典起始位置
        /// </summary>
        /// <returns>是否查找到</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private byte searchDictionary()
        {
            if (*Current == '{')
            {
                ++Current;
                return 1;
            }
            if (*Current == '[')
            {
                ++Current;
                return 2;
            }
            if (isNull()) return 0;
            space();
            if (state != TmphParseState.Success) return 0;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return 0;
            }
            if (*Current == '{')
            {
                ++Current;
                return 1;
            }
            if (*Current == '[')
            {
                ++Current;
                return 2;
            }
            if (isNull()) return 0;
            state = TmphParseState.NotObject;
            return 0;
        }

        /// <summary>
        ///     对象是否结束
        /// </summary>
        /// <returns>对象是否结束</returns>
        private byte isDictionaryObjectEnd()
        {
            if (*Current == '}')
            {
                ++Current;
                return 1;
            }
            space();
            if (state != TmphParseState.Success) return 1;
            if (Current == end)
            {
                state = TmphParseState.CrashEnd;
                return 1;
            }
            if (*Current == '}')
            {
                ++Current;
                return 1;
            }
            return 0;
        }

        ///// <summary>
        ///// TCP调用名称解析
        ///// </summary>
        ///// <param name="name">名称</param>
        ///// <returns>是否成功</returns>
        //internal bool TcpParameterName(string name)
        //{
        //    if (end - current > name.Length + 5 && *(int*)current == '{' + ('"' << 16))
        //    {
        //        fixed (char* nameFixed = name)
        //        {
        //            if (Laurent.Lee.CLB.Unsafe.THMemory.Equal((byte*)(current + 2), nameFixed, name.Length << 1))
        //            {
        //                if (*(int*)(current += name.Length + 2) == '"' + (':' << 16))
        //                {
        //                    current += 2;
        //                    return true;
        //                }
        //            }
        //        }
        //    }
        //    return false;
        //}
        ///// <summary>
        ///// TCP调用对象结束
        ///// </summary>
        //internal void TcpObjectEnd()
        //{
        //    if (state == parseState.Success)
        //    {
        //        if (*current == '}') ++current;
        //        else state = parseState.CrashEnd;
        //    }
        //}

        /// <summary>
        ///     引用类型对象解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void typeParse<TValueType>(TmphJsonParser parser, ref TValueType value)
        {
            TmphTypeParser<TValueType>.ParseClass(parser, ref value);
        }

        /// <summary>
        ///     值类型对象解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void structParse<TValueType>(TmphJsonParser parser, ref TValueType value) where TValueType : struct
        {
            if (parser.searchObject()) TmphTypeParser<TValueType>.ParseMembers(parser, ref value);
            else value = default(TValueType);
        }

        /// <summary>
        ///     值类型对象解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void nullableParse<TValueType>(ref TValueType? value) where TValueType : struct
        {
            if (searchObject())
            {
                --Current;
                var newValue = value.HasValue ? value.Value : default(TValueType);
                TmphTypeParser<TValueType>.Parse(this, ref newValue);
                value = newValue;
            }
            else value = null;
        }

        /// <summary>
        ///     是否null
        /// </summary>
        /// <returns>是否null</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private bool tryNull()
        {
            if (isNull()) return true;
            space();
            return isNull();
        }

        /// <summary>
        ///     值类型对象解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void nullableEnumParse<TValueType>(TmphJsonParser parser, ref TValueType? value)
            where TValueType : struct
        {
            if (parser.tryNull()) value = null;
            else
            {
                var newValue = value.HasValue ? value.Value : default(TValueType);
                TmphTypeParser<TValueType>.DefaultParser(parser, ref newValue);
                value = newValue;
            }
        }

        /// <summary>
        ///     值类型对象解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void keyValuePairParse<TKeyType, TValueType>(TmphJsonParser parser,
            ref KeyValuePair<TKeyType, TValueType> value)
        {
            if (parser.searchObject())
            {
                var keyValue = new TmphKeyValue<TKeyType, TValueType>();
                TmphTypeParser<TmphKeyValue<TKeyType, TValueType>>.ParseMembers(parser, ref keyValue);
                value = new KeyValuePair<TKeyType, TValueType>(keyValue.Key, keyValue.Value);
            }
            else value = new KeyValuePair<TKeyType, TValueType>();
        }

        /// <summary>
        ///     值类型对象解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void nullableMemberParse<TValueType>(TmphJsonParser parser, ref TValueType? value)
            where TValueType : struct
        {
            if (parser.searchObject())
            {
                var newValue = value.HasValue ? value.Value : default(TValueType);
                TmphTypeParser<TValueType>.ParseMembers(parser, ref newValue);
                value = newValue;
            }
            else value = null;
        }

        /// <summary>
        ///     基类转换
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void baseParse<TValueType, childType>(TmphJsonParser parser, ref childType value)
            where childType : TValueType
        {
            TValueType newValue = value;
            if (value == null)
            {
                if (parser.searchObject())
                {
                    newValue = TmphConstructor<childType>.New();
                    TmphTypeParser<TValueType>.ParseMembers(parser, ref newValue);
                    value = (childType)newValue;
                }
            }
            else TmphTypeParser<TValueType>.ParseClass(parser, ref newValue);
        }

        /// <summary>
        ///     找不到构造函数
        /// </summary>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void checkNoConstructor<TValueType>(ref TValueType value)
        {
            if (value == null)
            {
                var constructor = parseConfig.Constructor;
                if (constructor == null)
                {
                    state = TmphParseState.NoConstructor;
                    return;
                }
            }
            noConstructor(ref value);
        }

        /// <summary>
        ///     枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void enumByte<TValueType>(TmphJsonParser parser, ref TValueType value)
        {
            TmphTypeParser<TValueType>.TmphEnumByte.Parse(parser, ref value);
        }

        /// <summary>
        ///     枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void enumSByte<TValueType>(TmphJsonParser parser, ref TValueType value)
        {
            TmphTypeParser<TValueType>.TmphEnumSByte.Parse(parser, ref value);
        }

        /// <summary>
        ///     枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void enumShort<TValueType>(TmphJsonParser parser, ref TValueType value)
        {
            TmphTypeParser<TValueType>.TmphEnumShort.Parse(parser, ref value);
        }

        /// <summary>
        ///     枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void enumUShort<TValueType>(TmphJsonParser parser, ref TValueType value)
        {
            TmphTypeParser<TValueType>.TmphEnumUShort.Parse(parser, ref value);
        }

        /// <summary>
        ///     枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void enumInt<TValueType>(TmphJsonParser parser, ref TValueType value)
        {
            TmphTypeParser<TValueType>.TmphEnumInt.Parse(parser, ref value);
        }

        /// <summary>
        ///     枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void enumUInt<TValueType>(TmphJsonParser parser, ref TValueType value)
        {
            TmphTypeParser<TValueType>.TmphEnumUInt.Parse(parser, ref value);
        }

        /// <summary>
        ///     枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void enumLong<TValueType>(TmphJsonParser parser, ref TValueType value)
        {
            TmphTypeParser<TValueType>.TmphEnumLong.Parse(parser, ref value);
        }

        /// <summary>
        ///     枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void enumULong<TValueType>(TmphJsonParser parser, ref TValueType value)
        {
            TmphTypeParser<TValueType>.TmphEnumULong.Parse(parser, ref value);
        }

        /// <summary>
        ///     枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void enumByteFlags<TValueType>(TmphJsonParser parser, ref TValueType value)
        {
            TmphTypeParser<TValueType>.TmphEnumByte.ParseFlags(parser, ref value);
        }

        /// <summary>
        ///     枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void enumSByteFlags<TValueType>(TmphJsonParser parser, ref TValueType value)
        {
            TmphTypeParser<TValueType>.TmphEnumSByte.ParseFlags(parser, ref value);
        }

        /// <summary>
        ///     枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void enumShortFlags<TValueType>(TmphJsonParser parser, ref TValueType value)
        {
            TmphTypeParser<TValueType>.TmphEnumShort.ParseFlags(parser, ref value);
        }

        /// <summary>
        ///     枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void enumUShortFlags<TValueType>(TmphJsonParser parser, ref TValueType value)
        {
            TmphTypeParser<TValueType>.TmphEnumUShort.ParseFlags(parser, ref value);
        }

        /// <summary>
        ///     枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void enumIntFlags<TValueType>(TmphJsonParser parser, ref TValueType value)
        {
            TmphTypeParser<TValueType>.TmphEnumInt.ParseFlags(parser, ref value);
        }

        /// <summary>
        ///     枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void enumUIntFlags<TValueType>(TmphJsonParser parser, ref TValueType value)
        {
            TmphTypeParser<TValueType>.TmphEnumUInt.ParseFlags(parser, ref value);
        }

        /// <summary>
        ///     枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void enumLongFlags<TValueType>(TmphJsonParser parser, ref TValueType value)
        {
            TmphTypeParser<TValueType>.TmphEnumLong.ParseFlags(parser, ref value);
        }

        /// <summary>
        ///     枚举值解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void enumULongFlags<TValueType>(TmphJsonParser parser, ref TValueType value)
        {
            TmphTypeParser<TValueType>.TmphEnumULong.ParseFlags(parser, ref value);
        }

        /// <summary>
        ///     数组解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void array<TValueType>(TmphJsonParser parser, ref TValueType[] values)
        {
            TmphTypeParser<TValueType>.Array(parser, ref values);
        }

        /// <summary>
        ///     字典解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="dictionary">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void dictionary<TValueType, dictionaryValueType>(
            ref Dictionary<TValueType, dictionaryValueType> dictionary)
        {
            var type = searchDictionary();
            if (type == 0) dictionary = null;
            else
            {
                dictionary = TmphDictionary.CreateAny<TValueType, dictionaryValueType>();
                if (type == 1)
                {
                    if (isDictionaryObjectEnd() == 0)
                    {
                        var key = default(TValueType);
                        var value = default(dictionaryValueType);
                        do
                        {
                            TmphTypeParser<TValueType>.Parse(this, ref key);
                            if (state != TmphParseState.Success || searchColon() == 0) return;
                            TmphTypeParser<dictionaryValueType>.Parse(this, ref value);
                            if (state != TmphParseState.Success) return;
                            dictionary.Add(key, value);
                        } while (isNextObject());
                    }
                }
                else if (isFirstArrayValue())
                {
                    var value = default(TmphKeyValue<TValueType, dictionaryValueType>);
                    do
                    {
                        TmphTypeParser<TmphKeyValue<TValueType, dictionaryValueType>>.ParseValue(this, ref value);
                        if (state != TmphParseState.Success) return;
                        dictionary.Add(value.Key, value.Value);
                    } while (isNextArrayValue());
                }
            }
        }

        /// <summary>
        ///     集合构造函数解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void dictionaryConstructor<dictionaryType, TKeyType, TValueType>(TmphJsonParser parser,
            ref dictionaryType value)
        {
            KeyValuePair<TKeyType, TValueType>[] values = null;
            var count = TmphTypeParser<KeyValuePair<TKeyType, TValueType>>.ArrayIndex(parser, ref values);
            if (count == -1) value = default(dictionaryType);
            else
            {
                var dictionary = TmphDictionary.CreateAny<TKeyType, TValueType>(count);
                if (count != 0)
                {
                    foreach (var keyValue in values)
                    {
                        dictionary.Add(keyValue.Key, keyValue.Value);
                        if (--count == 0) break;
                    }
                }
                value = TmphPub.TmphDictionaryConstructor<dictionaryType, TKeyType, TValueType>.Constructor(dictionary);
            }
        }

        /// <summary>
        ///     集合构造函数解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void listConstructor<TValueType, argumentType>(TmphJsonParser parser, ref TValueType value)
        {
            argumentType[] values = null;
            var count = TmphTypeParser<argumentType>.ArrayIndex(parser, ref values);
            if (count == -1) value = default(TValueType);
            else
                value =
                    TmphPub.TmphListConstructor<TValueType, argumentType>.Constructor(TmphSubArray<argumentType>.Unsafe(values,
                        0, count));
        }

        /// <summary>
        ///     集合构造函数解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void collectionConstructor<TValueType, argumentType>(TmphJsonParser parser, ref TValueType value)
        {
            argumentType[] values = null;
            var count = TmphTypeParser<argumentType>.ArrayIndex(parser, ref values);
            if (count == -1) value = default(TValueType);
            else
                value =
                    value =
                        TmphPub.TmphCollectionConstructor<TValueType, argumentType>.Constructor(
                            TmphSubArray<argumentType>.Unsafe(values, 0, count));
        }

        /// <summary>
        ///     集合构造函数解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void enumerableConstructor<TValueType, argumentType>(TmphJsonParser parser, ref TValueType value)
        {
            argumentType[] values = null;
            var count = TmphTypeParser<argumentType>.ArrayIndex(parser, ref values);
            if (count == -1) value = default(TValueType);
            else
                value =
                    TmphPub.TmphEnumerableConstructor<TValueType, argumentType>.Constructor(
                        TmphSubArray<argumentType>.Unsafe(values, 0, count));
        }

        /// <summary>
        ///     集合构造函数解析
        /// </summary>
        /// <param name="parser">Json解析器</param>
        /// <param name="value">目标数据</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private static void arrayConstructor<TValueType, argumentType>(TmphJsonParser parser, ref TValueType value)
        {
            argumentType[] values = null;
            TmphTypeParser<argumentType>.Array(parser, ref values);
            if (parser.state == TmphParseState.Success)
            {
                if (values == null) value = default(TValueType);
                else value = TmphPub.TmphArrayConstructor<TValueType, argumentType>.Constructor(values);
            }
        }

        /// <summary>
        ///     Json解析
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="json">Json字符串</param>
        /// <param name="TmphConfig">配置参数</param>
        /// <returns>目标数据</returns>
        public static TValueType Parse<TValueType>(TmphSubString json, TmphConfig TmphConfig = null)
        {
            if (json.Length == 0)
            {
                if (TmphConfig != null) TmphConfig.State = TmphParseState.NullJson;
                return default(TValueType);
            }
            var value = default(TValueType);
            var parser = TmphTypePool<TmphJsonParser>.Pop() ?? new TmphJsonParser();
            try
            {
                return parser.parse(json, ref value, TmphConfig ?? defaultConfig) == TmphParseState.Success
                    ? value
                    : default(TValueType);
            }
            finally
            {
                parser.free();
            }
        }

        /// <summary>
        ///     Json解析
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="json">Json字符串</param>
        /// <param name="value">目标数据</param>
        /// <param name="TmphConfig">配置参数</param>
        /// <returns>是否解析成功</returns>
        public static bool Parse<TValueType>(TmphSubString json, ref TValueType value, TmphConfig TmphConfig = null)
        {
            if (json.Length == 0)
            {
                if (TmphConfig != null) TmphConfig.State = TmphParseState.NullJson;
                return false;
            }
            var parser = TmphTypePool<TmphJsonParser>.Pop() ?? new TmphJsonParser();
            try
            {
                return parser.parse(json, ref value, TmphConfig ?? defaultConfig) == TmphParseState.Success;
            }
            finally
            {
                parser.free();
            }
        }

        /// <summary>
        ///     Json解析
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="json">Json字符串</param>
        /// <param name="length">Json长度</param>
        /// <param name="value">目标数据</param>
        /// <param name="TmphConfig">配置参数</param>
        /// <returns>是否解析成功</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static bool Parse<TValueType>(char* json, int length, ref TValueType value, TmphConfig TmphConfig = null,
            byte[] TmphBuffer = null)
        {
            var parser = TmphTypePool<TmphJsonParser>.Pop() ?? new TmphJsonParser();
            try
            {
                return parser.parse(json, length, ref value, TmphConfig ?? defaultConfig, TmphBuffer) == TmphParseState.Success;
            }
            finally
            {
                parser.free();
            }
        }

        /// <summary>
        ///     Json解析
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="json">Json字符串</param>
        /// <param name="TmphConfig">配置参数</param>
        /// <returns>目标数据</returns>
        private static object parseType<TValueType>(TmphSubString json, TmphConfig TmphConfig)
        {
            return Parse<TValueType>(json, TmphConfig);
        }

        /// <summary>
        ///     Json解析
        /// </summary>
        /// <param name="type">目标数据类型</param>
        /// <param name="json">Json字符串</param>
        /// <param name="TmphConfig">配置参数</param>
        /// <returns>目标数据</returns>
        public static object ParseType(Type type, TmphSubString json, TmphConfig TmphConfig = null)
        {
            if (type == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            Func<TmphSubString, TmphConfig, object> parse;
            if (!parseTypes.TryGetValue(type, out parse))
            {
                parse =
                    (Func<TmphSubString, TmphConfig, object>)
                        Delegate.CreateDelegate(typeof(Func<TmphSubString, TmphConfig, object>),
                            parseTypeMethod.MakeGenericMethod(type));
                parseTypes.Set(type, parse);
            }
            return parse(json, TmphConfig);
        }

        /// <summary>
        ///     字符串转义解析
        /// </summary>
        /// <param name="value"></param>
        /// <param name="escapeIndex">未解析字符串起始位置</param>
        /// <param name="quote">字符串引号</param>
        /// <returns></returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        internal static TmphSubString ParseQuoteString(TmphSubString value, int escapeIndex, char quote, int isTempString)
        {
            var parser = TmphTypePool<TmphJsonParser>.Pop() ?? new TmphJsonParser();
            try
            {
                return parser.parseQuoteString(value, escapeIndex, quote, isTempString);
            }
            finally
            {
                parser.free();
            }
        }

        /// <summary>
        ///     获取基本类型解析函数
        /// </summary>
        /// <param name="type">基本类型</param>
        /// <returns>解析函数</returns>
        private static MethodInfo getParseMethod(Type type)
        {
            MethodInfo method;
            return parseMethods.TryGetValue(type, out method) ? method : null;
        }

        /// <summary>
        ///     配置参数
        /// </summary>
        public sealed class TmphConfig
        {
            /// <summary>
            ///     自定义构造函数
            /// </summary>
            public Func<Type, object> Constructor;

            /// <summary>
            ///     当前解析位置
            /// </summary>
            internal int CurrentIndex;

            /// <summary>
            ///     对象解析结束后是否检测最后的空格符
            /// </summary>
            public bool IsEndSpace = true;

            /// <summary>
            ///     是否获取Json字符串与当前解析位置信息
            /// </summary>
            public bool IsGetJson = true;

            /// <summary>
            ///     是否强制匹配枚举值
            /// </summary>
            public bool IsMatchEnum;

            /// <summary>
            ///     是否临时字符串(可修改)
            /// </summary>
            public bool IsTempString;

            /// <summary>
            ///     Json字符串
            /// </summary>
            internal TmphSubString Json;

            /// <summary>
            ///     成员选择
            /// </summary>
            public TmphMemberFilters MemberFilter = TmphMemberFilters.PublicInstance;

            /// <summary>
            ///     成员位图
            /// </summary>
            public TmphMemberMap MemberMap;

            /// <summary>
            ///     解析状态
            /// </summary>
            public TmphParseState State { get; internal set; }
        }

        /// <summary>
        ///     名称状态查找器
        /// </summary>
        internal struct TmphStateSearcher
        {
            /// <summary>
            ///     泛型定义类型成员名称查找数据
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, TmphPointer> genericDefinitionMemberSearchers =
                new TmphInterlocked.TmphDictionary<Type, TmphPointer>(TmphDictionary.CreateOnly<Type, TmphPointer>());

            /// <summary>
            ///     泛型定义类型成员名称查找数据创建锁
            /// </summary>
            private static readonly object genericDefinitionMemberSearcherCreateLock = new object();

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
            ///     Json解析器
            /// </summary>
            private readonly TmphJsonParser parser;

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
            ///     名称查找器
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="data">数据起始位置</param>
            internal TmphStateSearcher(TmphJsonParser parser, TmphPointer data)
            {
                this.parser = parser;
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
            ///     获取特殊字符索引值
            /// </summary>
            /// <param name="value">特殊字符</param>
            /// <returns>索引值,匹配失败返回0</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private int getCharIndex(char value)
            {
                var current = CLB.TmphStateSearcher.TmphChars.GetCharIndex((char*)charStart, (char*)charEnd, value);
                return current == null ? 0 : (charIndex + (int)(current - (char*)charStart));
                //char* charStart = (char*)this.charStart, charEnd = (char*)this.charEnd, current = charStart + ((int)(charEnd - charStart) >> 1);
                //while (*current != value)
                //{
                //    if (value < *current)
                //    {
                //        if (current == charStart) return 0;
                //        charEnd = current;
                //        current = charStart + ((int)(charEnd - charStart) >> 1);
                //    }
                //    else
                //    {
                //        if ((charStart = current + 1) == charEnd) return 0;
                //        current = charStart + ((int)(charEnd - charStart) >> 1);
                //    }
                //}
                //return charIndex + (int)(current - (char*)this.charStart);
            }

            /// <summary>
            ///     根据字符串查找目标索引
            /// </summary>
            /// <returns>目标索引,null返回-1</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal int SearchString()
            {
                var value = parser.searchQuote();
                if (parser.state != TmphParseState.Success || state == null) return -1;
                currentState = state;
                return searchString(value);
            }

            /// <summary>
            ///     获取名称索引
            /// </summary>
            /// <param name="isQuote">名称是否带引号</param>
            /// <returns>名称索引,失败返回-1</returns>
            internal int SearchName(ref bool isQuote)
            {
                var value = parser.getFirstName();
                if (state == null) return -1;
                if (parser.quote != 0)
                {
                    isQuote = true;
                    currentState = state;
                    return searchString(value);
                }
                if (parser.state != TmphParseState.Success) return -1;
                isQuote = false;
                currentState = state;
                do
                {
                    var prefix = (char*)(currentState + *(int*)currentState);
                    if (*prefix != 0)
                    {
                        if (value != *prefix) return -1;
                        while (*++prefix != 0)
                        {
                            if (parser.getNextName() != *prefix) return -1;
                        }
                        value = parser.getNextName();
                    }
                    if (value == 0)
                        return parser.state == TmphParseState.Success ? *(int*)(currentState + sizeof(int) * 2) : -1;
                    if (*(int*)(currentState + sizeof(int)) == 0) return -1;
                    var index = value < 128 ? *(ushort*)(charsAscii + (value << 1)) : getCharIndex(value);
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
                    value = parser.getNextName();
                } while (true);
            }

            /// <summary>
            ///     根据字符串查找目标索引
            /// </summary>
            /// <param name="value">第一个字符</param>
            /// <returns>目标索引,null返回-1</returns>
            internal int searchString(char value)
            {
                do
                {
                    var prefix = (char*)(currentState + *(int*)currentState);
                    if (*prefix != 0)
                    {
                        if (value != *prefix) return -1;
                        while (*++prefix != 0)
                        {
                            if (parser.nextStringChar() != *prefix) return -1;
                        }
                        value = parser.nextStringChar();
                    }
                    if (value == 0)
                        return parser.state == TmphParseState.Success ? *(int*)(currentState + sizeof(int) * 2) : -1;
                    if (*(int*)(currentState + sizeof(int)) == 0) return -1;
                    var index = value < 128 ? *(ushort*)(charsAscii + (value << 1)) : getCharIndex(value);
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
                    value = parser.nextStringChar();
                } while (true);
            }

            /// <summary>
            ///     根据枚举字符串查找目标索引
            /// </summary>
            /// <returns>目标索引,null返回-1</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal int SearchFlagEnum()
            {
                var value = parser.searchEnumQuote();
                if (state == null) return -1;
                currentState = state;
                return flagEnum(value);
            }

            /// <summary>
            ///     根据枚举字符串查找目标索引
            /// </summary>
            /// <returns>目标索引,null返回-1</returns>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal int NextFlagEnum()
            {
                var value = parser.searchNextEnum();
                if (state == null) return -1;
                currentState = state;
                return flagEnum(value);
            }

            /// <summary>
            ///     根据枚举字符串查找目标索引
            /// </summary>
            /// <param name="value">当前字符</param>
            /// <returns>目标索引,null返回-1</returns>
            private int flagEnum(char value)
            {
                do
                {
                    var prefix = (char*)(currentState + *(int*)currentState);
                    if (*prefix != 0)
                    {
                        if (value != *prefix) return -1;
                        while (*++prefix != 0)
                        {
                            if (parser.nextEnumChar() != *prefix) return -1;
                        }
                        value = parser.nextEnumChar();
                    }
                    if (value == 0 || value == ',')
                        return parser.state == TmphParseState.Success ? *(int*)(currentState + sizeof(int) * 2) : -1;
                    if (*(int*)(currentState + sizeof(int)) == 0) return -1;
                    var index = value < 128 ? *(ushort*)(charsAscii + (value << 1)) : getCharIndex(value);
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
                    value = parser.nextEnumChar();
                } while (true);
            }

            /// <summary>
            ///     获取泛型定义成员名称查找数据
            /// </summary>
            /// <param name="type">泛型定义类型</param>
            /// <param name="names">成员名称集合</param>
            /// <returns>泛型定义成员名称查找数据</returns>
            internal static TmphPointer GetGenericDefinitionMember(Type type, string[] names)
            {
                TmphPointer data;
                if (genericDefinitionMemberSearchers.TryGetValue(type = type.GetGenericTypeDefinition(), out data))
                    return data;
                Monitor.Enter(genericDefinitionMemberSearcherCreateLock);
                if (genericDefinitionMemberSearchers.TryGetValue(type, out data))
                {
                    Monitor.Exit(genericDefinitionMemberSearcherCreateLock);
                    return data;
                }
                try
                {
                    genericDefinitionMemberSearchers.Set(type, data = CLB.TmphStateSearcher.TmphChars.Create(names));
                }
                finally
                {
                    Monitor.Exit(genericDefinitionMemberSearcherCreateLock);
                }
                return data;
            }
        }

        /// <summary>
        ///     类型解析器静态信息
        /// </summary>
        internal static class TmphTypeParser
        {
            /// <summary>
            ///     数组解析调用函数信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, MethodInfo> arrayParsers =
                new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

            /// <summary>
            ///     缺少构造函数解析调用函数信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, MethodInfo> noConstructorParsers =
                new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

            /// <summary>
            ///     获取枚举构造调用函数信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, MethodInfo> enumerableConstructorParsers =
                new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

            /// <summary>
            ///     枚举解析调用函数信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, MethodInfo> enumParsers =
                new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

            /// <summary>
            ///     值类型解析调用函数信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, MethodInfo> valueTypeParsers =
                new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

            /// <summary>
            ///     引用类型解析调用函数信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, MethodInfo> typeParsers =
                new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

            /// <summary>
            ///     字典解析调用函数信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, MethodInfo> dictionaryParsers =
                new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

            /// <summary>
            ///     自定义解析调用函数信息集合
            /// </summary>
            private static TmphInterlocked.TmphDictionary<Type, MethodInfo> customParsers =
                new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

            /// <summary>
            ///     获取字段成员集合
            /// </summary>
            /// <param name="type"></param>
            /// <param name="typeAttribute">类型配置</param>
            /// <returns>字段成员集合</returns>
            public static TmphSubArray<TmphFieldIndex> GetFields(TmphFieldIndex[] fields, TmphJsonParse typeAttribute,
                ref TmphFieldIndex defaultMember)
            {
                var values = new TmphSubArray<TmphFieldIndex>(fields.Length);
                foreach (var field in fields)
                {
                    var type = field.Member.FieldType;
                    if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !field.IsIgnore)
                    {
                        var attribute = field.GetAttribute<TmphJsonParse.TmphMember>(true, true);
                        if (typeAttribute.IsAllMember
                            ? (attribute == null || attribute.IsSetup)
                            : (attribute != null && attribute.IsSetup))
                        {
                            if (attribute != null && attribute.IsDefault) defaultMember = field;
                            values.Add(field);
                        }
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
                TmphJsonParse typeAttribute)
            {
                var values = new TmphSubArray<TmphKeyValue<TmphPropertyIndex, MethodInfo>>(properties.Length);
                foreach (var property in properties)
                {
                    if (property.Member.CanWrite)
                    {
                        var type = property.Member.PropertyType;
                        if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !property.IsIgnore)
                        {
                            var attribute = property.GetAttribute<TmphJsonParse.TmphMember>(true, true);
                            if (typeAttribute.IsAllMember
                                ? (attribute == null || attribute.IsSetup)
                                : (attribute != null && attribute.IsSetup))
                            {
                                var method = property.Member.GetSetMethod(true);
                                if (method != null && method.GetParameters().Length == 1)
                                {
                                    values.Add(new TmphKeyValue<TmphPropertyIndex, MethodInfo>(property, method));
                                }
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
                TmphJsonParse typeAttribute)
            {
                var members = new TmphSubArray<TmphMemberIndex>(fieldIndexs.Length + properties.Length);
                foreach (var field in fieldIndexs)
                {
                    var type = field.Member.FieldType;
                    if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !field.IsIgnore)
                    {
                        var attribute = field.GetAttribute<TmphJsonParse.TmphMember>(true, true);
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
                            var attribute = property.GetAttribute<TmphJsonParse.TmphMember>(true, true);
                            if (typeAttribute.IsAllMember
                                ? (attribute == null || attribute.IsSetup)
                                : (attribute != null && attribute.IsSetup))
                            {
                                var method = property.Member.GetSetMethod(true);
                                if (method != null && method.GetParameters().Length == 1) members.Add(property);
                            }
                        }
                    }
                }
                return members;
            }

            /// <summary>
            ///     创建解析委托函数
            /// </summary>
            /// <param name="type"></param>
            /// <param name="field"></param>
            /// <returns>解析委托函数</returns>
            public static DynamicMethod CreateDynamicMethod(Type type, FieldInfo field)
            {
                var dynamicMethod = new DynamicMethod("jsonParser" + field.Name, null,
                    new[] { typeof(TmphJsonParser), type.MakeByRefType() }, type, true);
                var generator = dynamicMethod.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_1);
                if (!type.IsValueType) generator.Emit(OpCodes.Ldind_Ref);
                generator.Emit(OpCodes.Ldflda, field);
                var methodInfo = getMemberMethodInfo(field.FieldType);
                generator.Emit(methodInfo.IsFinal || !methodInfo.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, methodInfo);
                generator.Emit(OpCodes.Ret);
                return dynamicMethod;
            }

            /// <summary>
            ///     创建解析委托函数
            /// </summary>
            /// <param name="type"></param>
            /// <param name="property"></param>
            /// <param name="propertyMethod"></param>
            /// <returns>解析委托函数</returns>
            public static DynamicMethod CreateDynamicMethod(Type type, PropertyInfo property, MethodInfo propertyMethod)
            {
                var dynamicMethod = new DynamicMethod("jsonParser" + property.Name, null,
                    new[] { typeof(TmphJsonParser), type.MakeByRefType() }, type, true);
                var generator = dynamicMethod.GetILGenerator();
                var memberType = property.PropertyType;
                var loadMember = generator.DeclareLocal(memberType);
                var methodInfo = getMemberMethodInfo(memberType);
                if (!memberType.IsValueType)
                {
                    generator.Emit(OpCodes.Ldloca_S, loadMember);
                    generator.Emit(OpCodes.Initobj, memberType);
                }
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldloca_S, loadMember);
                generator.Emit(methodInfo.IsFinal || !methodInfo.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, methodInfo);

                generator.Emit(OpCodes.Ldarg_1);
                if (!type.IsValueType) generator.Emit(OpCodes.Ldind_Ref);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(propertyMethod.IsFinal || !propertyMethod.IsVirtual ? OpCodes.Call : OpCodes.Callvirt,
                    propertyMethod);
                generator.Emit(OpCodes.Ret);
                return dynamicMethod;
            }

            /// <summary>
            ///     获取成员转换函数信息
            /// </summary>
            /// <param name="type">成员类型</param>
            /// <returns>成员转换函数信息</returns>
            private static MethodInfo getMemberMethodInfo(Type type)
            {
                var methodInfo = getParseMethod(type);
                if (methodInfo != null) return methodInfo;
                if (type.IsArray) return GetArrayParser(type.GetElementType());
                if (type.IsEnum) return GetEnumParser(type);
                if (type.IsGenericType)
                {
                    var genericType = type.GetGenericTypeDefinition();
                    if (genericType == typeof(Dictionary<,>)) return GetDictionaryParser(type);
                    if (genericType == typeof(Nullable<>))
                    {
                        var parameterTypes = type.GetGenericArguments();
                        return
                            (parameterTypes[0].IsEnum ? nullableEnumParseMethod : nullableParseMethod).MakeGenericMethod
                                (parameterTypes);
                    }
                    if (genericType == typeof(KeyValuePair<,>))
                        return keyValuePairParseMethod.MakeGenericMethod(type.GetGenericArguments());
                }
                if ((methodInfo = GetCustomParser(type)) != null) return methodInfo;
                if (type.IsAbstract || type.IsInterface) return GetNoConstructorParser(type);
                if ((methodInfo = GetIEnumerableConstructorParser(type)) != null) return methodInfo;
                if (type.IsValueType) return GetValueTypeParser(type);
                return GetTypeParser(type);
            }

            /// <summary>
            ///     获取数组解析委托调用函数信息
            /// </summary>
            /// <param name="type">数组类型</param>
            /// <returns>数组解析委托调用函数信息</returns>
            public static MethodInfo GetArrayParser(Type type)
            {
                MethodInfo method;
                if (arrayParsers.TryGetValue(type, out method)) return method;
                arrayParsers.Set(type, method = arrayMethod.MakeGenericMethod(type));
                return method;
            }

            /// <summary>
            ///     获取缺少构造函数委托调用函数信息
            /// </summary>
            /// <param name="type">数据类型</param>
            /// <returns>缺少构造函数委托调用函数信息</returns>
            public static MethodInfo GetNoConstructorParser(Type type)
            {
                MethodInfo method;
                if (noConstructorParsers.TryGetValue(type, out method)) return method;
                method = checkNoConstructorMethod.MakeGenericMethod(type);
                noConstructorParsers.Set(type, method);
                return method;
            }

            /// <summary>
            ///     获取枚举构造调用函数信息
            /// </summary>
            /// <param name="type">集合类型</param>
            /// <returns>枚举构造调用函数信息</returns>
            public static MethodInfo GetIEnumerableConstructorParser(Type type)
            {
                MethodInfo method;
                if (enumerableConstructorParsers.TryGetValue(type, out method)) return method;
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
                                method = listConstructorMethod.MakeGenericMethod(type, argumentType);
                                break;
                            }
                            parameters[0] = typeof(ICollection<>).MakeGenericType(argumentType);
                            constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    parameters, null);
                            if (constructorInfo != null)
                            {
                                method = collectionConstructorMethod.MakeGenericMethod(type, argumentType);
                                break;
                            }
                            parameters[0] = typeof(IEnumerable<>).MakeGenericType(argumentType);
                            constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    parameters, null);
                            if (constructorInfo != null)
                            {
                                method = enumerableConstructorMethod.MakeGenericMethod(type, argumentType);
                                break;
                            }
                            parameters[0] = argumentType.MakeArrayType();
                            constructorInfo =
                                type.GetConstructor(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                                    parameters, null);
                            if (constructorInfo != null)
                            {
                                method = arrayConstructorMethod.MakeGenericMethod(type, argumentType);
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
                                var parameters = interfaceType.GetGenericArguments();
                                method = dictionaryConstructorMethod.MakeGenericMethod(type, parameters[0],
                                    parameters[1]);
                                break;
                            }
                        }
                    }
                }
                enumerableConstructorParsers.Set(type, method);
                return method;
            }

            /// <summary>
            ///     获取枚举解析调用函数信息
            /// </summary>
            /// <param name="type">枚举类型</param>
            /// <returns>枚举解析调用函数信息</returns>
            public static MethodInfo GetEnumParser(Type type)
            {
                MethodInfo method;
                if (enumParsers.TryGetValue(type, out method)) return method;
                var TEnumType = Enum.GetUnderlyingType(type);
                if (TmphTypeAttribute.GetAttribute<FlagsAttribute>(type, false, false) == null)
                {
                    if (TEnumType == typeof(uint)) method = enumUIntMethod.MakeGenericMethod(type);
                    else if (TEnumType == typeof(byte)) method = enumByteMethod.MakeGenericMethod(type);
                    else if (TEnumType == typeof(ulong)) method = enumULongMethod.MakeGenericMethod(type);
                    else if (TEnumType == typeof(ushort)) method = enumUShortMethod.MakeGenericMethod(type);
                    else if (TEnumType == typeof(long)) method = enumLongMethod.MakeGenericMethod(type);
                    else if (TEnumType == typeof(short)) method = enumShortMethod.MakeGenericMethod(type);
                    else if (TEnumType == typeof(sbyte)) method = enumSByteMethod.MakeGenericMethod(type);
                    else method = enumIntMethod.MakeGenericMethod(type);
                }
                else
                {
                    if (TEnumType == typeof(uint)) method = enumUIntFlagsMethod.MakeGenericMethod(type);
                    else if (TEnumType == typeof(byte)) method = enumByteFlagsMethod.MakeGenericMethod(type);
                    else if (TEnumType == typeof(ulong)) method = enumULongFlagsMethod.MakeGenericMethod(type);
                    else if (TEnumType == typeof(ushort)) method = enumUShortFlagsMethod.MakeGenericMethod(type);
                    else if (TEnumType == typeof(long)) method = enumLongFlagsMethod.MakeGenericMethod(type);
                    else if (TEnumType == typeof(short)) method = enumShortFlagsMethod.MakeGenericMethod(type);
                    else if (TEnumType == typeof(sbyte)) method = enumSByteFlagsMethod.MakeGenericMethod(type);
                    else method = enumIntFlagsMethod.MakeGenericMethod(type);
                }
                enumParsers.Set(type, method);
                return method;
            }

            /// <summary>
            ///     获取值类型解析调用函数信息
            /// </summary>
            /// <param name="type">数据类型</param>
            /// <returns>值类型解析调用函数信息</returns>
            public static MethodInfo GetValueTypeParser(Type type)
            {
                MethodInfo method;
                if (valueTypeParsers.TryGetValue(type, out method)) return method;
                var nullType = type.nullableType();
                if (nullType == null) method = structParseMethod.MakeGenericMethod(type);
                else method = nullableMemberParseMethod.MakeGenericMethod(nullType);
                valueTypeParsers.Set(type, method);
                return method;
            }

            /// <summary>
            ///     获取引用类型解析调用函数信息
            /// </summary>
            /// <param name="type">数据类型</param>
            /// <returns>引用类型解析调用函数信息</returns>
            public static MethodInfo GetTypeParser(Type type)
            {
                MethodInfo method;
                if (typeParsers.TryGetValue(type, out method)) return method;
                if (
                    type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                        TmphNullValue<Type>.Array, null) == null)
                    method = checkNoConstructorMethod.MakeGenericMethod(type);
                else method = typeParseMethod.MakeGenericMethod(type);
                typeParsers.Set(type, method);
                return method;
            }

            /// <summary>
            ///     获取字典解析调用函数信息
            /// </summary>
            /// <param name="type">数据类型</param>
            /// <returns>字典解析调用函数信息</returns>
            public static MethodInfo GetDictionaryParser(Type type)
            {
                MethodInfo method;
                if (dictionaryParsers.TryGetValue(type, out method)) return method;
                method = dictionaryMethod.MakeGenericMethod(type.GetGenericArguments());
                dictionaryParsers.Set(type, method);
                return method;
            }

            /// <summary>
            ///     自定义解析委托调用函数信息
            /// </summary>
            /// <param name="type">数据类型</param>
            /// <returns>自定义解析委托调用函数信息</returns>
            public static MethodInfo GetCustomParser(Type type)
            {
                MethodInfo method;
                if (customParsers.TryGetValue(type, out method)) return method;
                var refType = type.MakeByRefType();
                foreach (var methodInfo in TmphAttributeMethod.GetStatic(type))
                {
                    if (methodInfo.Method.ReturnType == typeof(void))
                    {
                        var parameters = methodInfo.Method.GetParameters();
                        if (parameters.Length == 2 && parameters[0].ParameterType == typeof(TmphJsonParser) &&
                            parameters[1].ParameterType == refType)
                        {
                            if (methodInfo.GetAttribute<TmphJsonParse.TmphCustom>(true) != null)
                            {
                                method = methodInfo.Method;
                                break;
                            }
                        }
                    }
                }
                customParsers.Set(type, method);
                return method;
            }
        }

        /// <summary>
        ///     类型解析器
        /// </summary>
        /// <typeparam name="TValueType">目标类型</typeparam>
        internal static class TmphTypeParser<TValueType>
        {
            /// <summary>
            ///     Json解析类型配置
            /// </summary>
            private static readonly TmphJsonParse attribute;

            /// <summary>
            ///     解析委托
            /// </summary>
            internal static readonly TmphTryParse DefaultParser;

            /// <summary>
            ///     是否值类型
            /// </summary>
            private static readonly bool isValueType;

            /// <summary>
            ///     成员解析器集合
            /// </summary>
            private static TmphTryParseFilter[] memberParsers;

            /// <summary>
            ///     成员名称查找数据
            /// </summary>
            private static TmphPointer memberSearcher;

            static TmphTypeParser()
            {
                var type = typeof(TValueType);
                var methodInfo = getParseMethod(type);
                if (methodInfo != null)
                {
                    var dynamicMethod = new DynamicMethod("jsonParser", typeof(void),
                        new[] { typeof(TmphJsonParser), type.MakeByRefType() }, true);
                    dynamicMethod.InitLocals = true;
                    var generator = dynamicMethod.GetILGenerator();
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Call, methodInfo);
                    generator.Emit(OpCodes.Ret);
                    DefaultParser = (TmphTryParse)dynamicMethod.CreateDelegate(typeof(TmphTryParse));
                    return;
                }
                if (type.IsArray)
                {
                    if (type.GetArrayRank() == 1)
                        DefaultParser =
                            (TmphTryParse)
                                Delegate.CreateDelegate(typeof(TmphTryParse),
                                    TmphTypeParser.GetArrayParser(type.GetElementType()));
                    else DefaultParser = arrayManyRank;
                    return;
                }
                if (type.IsEnum)
                {
                    var TEnumType = Enum.GetUnderlyingType(type);
                    if (TmphTypeAttribute.GetAttribute<FlagsAttribute>(type, false, false) == null)
                    {
                        if (TEnumType == typeof(uint)) DefaultParser = TmphEnumUInt.Parse;
                        else if (TEnumType == typeof(byte)) DefaultParser = TmphEnumByte.Parse;
                        else if (TEnumType == typeof(ulong)) DefaultParser = TmphEnumULong.Parse;
                        else if (TEnumType == typeof(ushort)) DefaultParser = TmphEnumUShort.Parse;
                        else if (TEnumType == typeof(long)) DefaultParser = TmphEnumLong.Parse;
                        else if (TEnumType == typeof(short)) DefaultParser = TmphEnumShort.Parse;
                        else if (TEnumType == typeof(sbyte)) DefaultParser = TmphEnumSByte.Parse;
                        else DefaultParser = TmphEnumInt.Parse;
                    }
                    else
                    {
                        if (TEnumType == typeof(uint)) DefaultParser = TmphEnumUInt.ParseFlags;
                        else if (TEnumType == typeof(byte)) DefaultParser = TmphEnumByte.ParseFlags;
                        else if (TEnumType == typeof(ulong)) DefaultParser = TmphEnumULong.ParseFlags;
                        else if (TEnumType == typeof(ushort)) DefaultParser = TmphEnumUShort.ParseFlags;
                        else if (TEnumType == typeof(long)) DefaultParser = TmphEnumLong.ParseFlags;
                        else if (TEnumType == typeof(short)) DefaultParser = TmphEnumShort.ParseFlags;
                        else if (TEnumType == typeof(sbyte)) DefaultParser = TmphEnumSByte.ParseFlags;
                        else DefaultParser = TmphEnumInt.ParseFlags;
                    }
                    return;
                }
                if (type.IsPointer)
                {
                    DefaultParser = ignore;
                    return;
                }
                if (type.IsGenericType)
                {
                    var genericType = type.GetGenericTypeDefinition();
                    if (genericType == typeof(Dictionary<,>))
                    {
                        DefaultParser =
                            (TmphTryParse)
                                Delegate.CreateDelegate(typeof(TmphTryParse), TmphTypeParser.GetDictionaryParser(type));
                        return;
                    }
                    if (genericType == typeof(Nullable<>))
                    {
                        var parameterTypes = type.GetGenericArguments();
                        DefaultParser =
                            (TmphTryParse)
                                Delegate.CreateDelegate(typeof(TmphTryParse),
                                    (parameterTypes[0].IsEnum ? nullableEnumParseMethod : nullableParseMethod)
                                        .MakeGenericMethod(parameterTypes));
                        return;
                    }
                    if (genericType == typeof(KeyValuePair<,>))
                    {
                        DefaultParser =
                            (TmphTryParse)
                                Delegate.CreateDelegate(typeof(TmphTryParse),
                                    keyValuePairParseMethod.MakeGenericMethod(type.GetGenericArguments()));
                        isValueType = true;
                        return;
                    }
                }
                if ((methodInfo = TmphTypeParser.GetCustomParser(type)) != null)
                {
                    DefaultParser = (TmphTryParse)Delegate.CreateDelegate(typeof(TmphTryParse), methodInfo);
                }
                else
                {
                    Type TAttributeType;
                    attribute = type.customAttribute<TmphJsonParse>(out TAttributeType, true) ?? TmphJsonParse.AllMember;
                    if (type.IsAbstract || type.IsInterface) DefaultParser = noConstructor;
                    else if ((methodInfo = TmphTypeParser.GetIEnumerableConstructorParser(type)) != null)
                    {
                        DefaultParser = (TmphTryParse)Delegate.CreateDelegate(typeof(TmphTryParse), methodInfo);
                    }
                    else if (TmphConstructor<TValueType>.New == null) DefaultParser = noConstructor;
                    else
                    {
                        if (type.IsValueType) isValueType = true;
                        else if (attribute != TmphJsonParse.AllMember && TAttributeType != type)
                        {
                            for (var baseType = type.BaseType;
                                baseType != typeof(object);
                                baseType = baseType.BaseType)
                            {
                                var baseAttribute = TmphTypeAttribute.GetAttribute<TmphJsonParse>(baseType, false, true);
                                if (baseAttribute != null)
                                {
                                    if (baseAttribute.IsBaseType)
                                    {
                                        methodInfo = baseParseMethod.MakeGenericMethod(baseType, type);
                                        DefaultParser =
                                            (TmphTryParse)Delegate.CreateDelegate(typeof(TmphTryParse), methodInfo);
                                        return;
                                    }
                                    break;
                                }
                            }
                        }
                        createMembers();
                    }
                }
            }

            /// <summary>
            ///     引用类型对象解析
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">目标数据</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal static void Parse(TmphJsonParser parser, ref TValueType value)
            {
                if (DefaultParser == null)
                {
                    if (isValueType) ParseValue(parser, ref value);
                    else parseClass(parser, ref value);
                }
                else DefaultParser(parser, ref value);
            }

            /// <summary>
            ///     值类型对象解析
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">目标数据</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal static void ParseValue(TmphJsonParser parser, ref TValueType value)
            {
                if (parser.searchObject()) ParseMembers(parser, ref value);
                else value = default(TValueType);
            }

            /// <summary>
            ///     引用类型对象解析
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">目标数据</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal static void ParseClass(TmphJsonParser parser, ref TValueType value)
            {
                if (DefaultParser == null) parseClass(parser, ref value);
                else DefaultParser(parser, ref value);
            }

            /// <summary>
            ///     引用类型对象解析
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">目标数据</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal static void parseClass(TmphJsonParser parser, ref TValueType value)
            {
                if (parser.searchObject())
                {
                    if (value == null) value = TmphConstructor<TValueType>.New();
                    ParseMembers(parser, ref value);
                }
                else value = default(TValueType);
            }

            /// <summary>
            ///     数据成员解析
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">目标数据</param>
            internal static void ParseMembers(TmphJsonParser parser, ref TValueType value)
            {
                var searcher = new TmphStateSearcher(parser, memberSearcher);
                if (parser.isFirstObject())
                {
                    var TmphConfig = parser.parseConfig;
                    var memberMap = TmphConfig.MemberMap;
                    var isQuote = false;
                    var index = searcher.SearchName(ref isQuote);
                    if (memberMap == null)
                    {
                        if (index != -1)
                        {
                            if (parser.searchColon() == 0) return;
                            memberParsers[index].Call(parser, ref value);
                        }
                        else
                        {
                            if (isQuote) parser.searchStringEnd();
                            else parser.searchNameEnd();
                            if (parser.state != TmphParseState.Success || parser.searchColon() == 0) return;
                            parser.ignore();
                        }
                        while (parser.state == TmphParseState.Success && parser.isNextObject())
                        {
                            if ((index = searcher.SearchName(ref isQuote)) != -1)
                            {
                                if (parser.searchColon() == 0) return;
                                memberParsers[index].Call(parser, ref value);
                            }
                            else
                            {
                                if (isQuote) parser.searchStringEnd();
                                else parser.searchNameEnd();
                                if (parser.state != TmphParseState.Success || parser.searchColon() == 0) return;
                                parser.ignore();
                            }
                        }
                    }
                    else if (memberMap.Type == TmphMemberMap<TValueType>.Type)
                    {
                        try
                        {
                            memberMap.Empty();
                            TmphConfig.MemberMap = null;
                            if (index != -1)
                            {
                                if (parser.searchColon() == 0) return;
                                memberParsers[index].Call(parser, memberMap, ref value);
                            }
                            else
                            {
                                if (isQuote) parser.searchStringEnd();
                                else parser.searchNameEnd();
                                if (parser.state != TmphParseState.Success || parser.searchColon() == 0) return;
                                parser.ignore();
                            }
                            while (parser.state == TmphParseState.Success && parser.isNextObject())
                            {
                                if ((index = searcher.SearchName(ref isQuote)) != -1)
                                {
                                    if (parser.searchColon() == 0) return;
                                    memberParsers[index].Call(parser, memberMap, ref value);
                                }
                                else
                                {
                                    if (isQuote) parser.searchStringEnd();
                                    else parser.searchNameEnd();
                                    if (parser.state != TmphParseState.Success || parser.searchColon() == 0) return;
                                    parser.ignore();
                                }
                            }
                        }
                        finally
                        {
                            TmphConfig.MemberMap = memberMap;
                        }
                    }
                    else parser.state = TmphParseState.MemberMap;
                }
            }

            /// <summary>
            ///     不支持多维数组
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">目标数据</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void arrayManyRank(TmphJsonParser parser, ref TValueType value)
            {
                parser.state = TmphParseState.ArrayManyRank;
            }

            /// <summary>
            ///     数组解析
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">目标数据</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            internal static void Array(TmphJsonParser parser, ref TValueType[] values)
            {
                var count = ArrayIndex(parser, ref values);
                if (count != -1 && count != values.Length) System.Array.Resize(ref values, count);
            }

            /// <summary>
            ///     数组解析
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">目标数据</param>
            /// <returns>数据数量,-1表示失败</returns>
            internal static int ArrayIndex(TmphJsonParser parser, ref TValueType[] values)
            {
                parser.searchArray(ref values);
                if (parser.state != TmphParseState.Success || values == null) return -1;
                var index = 0;
                if (parser.isFirstArrayValue())
                {
                    do
                    {
                        if (index == values.Length)
                        {
                            var value = default(TValueType);
                            Parse(parser, ref value);
                            if (parser.state != TmphParseState.Success) return -1;
                            var newValues = new TValueType[index == 0 ? sizeof(int) : (index << 1)];
                            values.CopyTo(newValues, 0);
                            newValues[index++] = value;
                            values = newValues;
                        }
                        else
                        {
                            Parse(parser, ref values[index]);
                            if (parser.state != TmphParseState.Success) return -1;
                            ++index;
                        }
                    } while (parser.isNextArrayValue());
                }
                return parser.state == TmphParseState.Success ? index : -1;
            }

            /// <summary>
            ///     忽略数据
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">目标数据</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void ignore(TmphJsonParser parser, ref TValueType value)
            {
                parser.ignore();
            }

            /// <summary>
            ///     找不到构造函数
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">目标数据</param>
            [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
            private static void noConstructor(TmphJsonParser parser, ref TValueType value)
            {
                parser.checkNoConstructor(ref value);
            }

            /// <summary>
            ///     获取字段成员集合
            /// </summary>
            /// <returns>字段成员集合</returns>
            public static TmphSubArray<TmphMemberIndex> GetMembers()
            {
                if (memberParsers == null) return default(TmphSubArray<TmphMemberIndex>);
                return TmphTypeParser.GetMembers(TmphMemberIndexGroup<TValueType>.GetFields(attribute.MemberFilter),
                    TmphMemberIndexGroup<TValueType>.GetProperties(attribute.MemberFilter), attribute);
            }

            /// <summary>
            ///     创建成员解析集合
            /// </summary>
            private static void createMembers()
            {
                var type = typeof(TValueType);
                TmphFieldIndex defaultMember = null;
                var fields = TmphTypeParser.GetFields(TmphMemberIndexGroup<TValueType>.GetFields(attribute.MemberFilter),
                    attribute, ref defaultMember);
                var properties =
                    TmphTypeParser.GetProperties(TmphMemberIndexGroup<TValueType>.GetProperties(attribute.MemberFilter),
                        attribute);
                var parsers = new TmphTryParseFilter[fields.Count + properties.Count + (defaultMember == null ? 0 : 1)];
                var memberMapType = TmphMemberMap<TValueType>.Type;
                var names = new string[parsers.Length];
                var index = 0;
                foreach (var member in fields)
                {
                    var tryParse = parsers[index] = new TmphTryParseFilter
                    {
                        TryParse =
                            (TmphTryParse)
                                TmphTypeParser.CreateDynamicMethod(type, member.Member)
                                    .CreateDelegate(typeof(TmphTryParse)),
                        MemberMapIndex = member.MemberIndex,
                        Filter =
                            member.Member.IsPublic
                                ? TmphMemberFilters.PublicInstanceField
                                : TmphMemberFilters.NonPublicInstanceField
                    };
                    names[index++] = member.Member.Name;
                    if (member == defaultMember)
                    {
                        parsers[index] = tryParse;
                        names[index++] = string.Empty;
                    }
                }
                foreach (var member in properties)
                {
                    parsers[index] = new TmphTryParseFilter
                    {
                        TryParse =
                            (TmphTryParse)
                                TmphTypeParser.CreateDynamicMethod(type, member.Key.Member, member.Value)
                                    .CreateDelegate(typeof(TmphTryParse)),
                        MemberMapIndex = member.Key.MemberIndex,
                        Filter =
                            member.Value.IsPublic
                                ? TmphMemberFilters.PublicInstanceProperty
                                : TmphMemberFilters.NonPublicInstanceProperty
                    };
                    names[index++] = member.Key.Member.Name;
                }
                if (type.IsGenericType) memberSearcher = TmphStateSearcher.GetGenericDefinitionMember(type, names);
                else memberSearcher = CLB.TmphStateSearcher.TmphChars.Create(names);
                memberParsers = parsers;
            }

            /// <summary>
            ///     枚举值解析
            /// </summary>
            internal class TmphEnumBase
            {
                /// <summary>
                ///     枚举值集合
                /// </summary>
                protected static readonly TValueType[] enumValues;

                /// <summary>
                ///     枚举名称查找数据
                /// </summary>
                protected static readonly TmphPointer enumSearcher;

                static TmphEnumBase()
                {
                    var values =
                        ((TValueType[])Enum.GetValues(typeof(TValueType))).getDictionary(
                            value => (TmphHashString)value.ToString());
                    enumValues = values.GetArray(value => value.Value);
                    enumSearcher = CLB.TmphStateSearcher.TmphChars.Create(values.GetArray((KeyValuePair<TmphHashString, TValueType> value) => value.Key.ToString()));
                }

                /// <summary>
                ///     枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
                protected static void parse(TmphJsonParser parser, ref TValueType value)
                {
                    var index = new TmphStateSearcher(parser, enumSearcher).SearchString();
                    if (index != -1) value = enumValues[index];
                    else if (parser.parseConfig.IsMatchEnum) parser.state = TmphParseState.NoFoundEnumValue;
                    else parser.searchStringEnd();
                }

                /// <summary>
                ///     枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="index">第一个枚举索引</param>
                /// <param name="nextIndex">第二个枚举索引</param>
                protected static void getIndex(TmphJsonParser parser, ref TValueType value, out int index,
                    ref int nextIndex)
                {
                    var searcher = new TmphStateSearcher(parser, enumSearcher);
                    if ((index = searcher.SearchFlagEnum()) == -1)
                    {
                        if (parser.parseConfig.IsMatchEnum)
                        {
                            parser.state = TmphParseState.NoFoundEnumValue;
                            return;
                        }
                        do
                        {
                            if (parser.state != TmphParseState.Success || parser.quote == 0) return;
                        } while ((index = searcher.NextFlagEnum()) == -1);
                    }
                    do
                    {
                        if (parser.quote == 0)
                        {
                            value = enumValues[index];
                            return;
                        }
                        if ((nextIndex = searcher.NextFlagEnum()) != -1) break;
                        if (parser.state != TmphParseState.Success) return;
                    } while (true);
                }
            }

            /// <summary>
            ///     枚举值解析
            /// </summary>
            internal sealed class TmphEnumByte : TmphEnumBase
            {
                /// <summary>
                ///     枚举值集合
                /// </summary>
                private static readonly TmphPointer enumInts;

                static TmphEnumByte()
                {
                    enumInts = TmphUnmanaged.Get(enumValues.Length * sizeof(byte), false);
                    var data = enumInts.Byte;
                    foreach (var value in enumValues) *data++ = TmphPub.TmphEnumCast<TValueType, byte>.ToInt(value);
                }

                /// <summary>
                ///     枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
                public static void Parse(TmphJsonParser parser, ref TValueType value)
                {
                    if (parser.isEnumNumber())
                    {
                        byte intValue = 0;
                        parser.Parse(ref intValue);
                        value = TmphPub.TmphEnumCast<TValueType, byte>.FromInt(intValue);
                    }
                    else parse(parser, ref value);
                }

                /// <summary>
                ///     枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
                public static void ParseFlags(TmphJsonParser parser, ref TValueType value)
                {
                    if (parser.isEnumNumber())
                    {
                        byte intValue = 0;
                        parser.Parse(ref intValue);
                        value = TmphPub.TmphEnumCast<TValueType, byte>.FromInt(intValue);
                    }
                    else
                    {
                        int index, nextIndex = -1;
                        getIndex(parser, ref value, out index, ref nextIndex);
                        if (nextIndex != -1)
                        {
                            var searcher = new TmphStateSearcher(parser, enumSearcher);
                            var intValue = enumInts.Byte[index];
                            intValue |= enumInts.Byte[nextIndex];
                            while (parser.quote != 0)
                            {
                                index = searcher.NextFlagEnum();
                                if (parser.state != TmphParseState.Success) return;
                                if (index != -1) intValue |= enumInts.Byte[index];
                            }
                            value = TmphPub.TmphEnumCast<TValueType, byte>.FromInt(intValue);
                        }
                    }
                }
            }

            /// <summary>
            ///     枚举值解析
            /// </summary>
            internal sealed class TmphEnumSByte : TmphEnumBase
            {
                /// <summary>
                ///     枚举值集合
                /// </summary>
                private static readonly TmphPointer enumInts;

                static TmphEnumSByte()
                {
                    enumInts = TmphUnmanaged.Get(enumValues.Length * sizeof(sbyte), false);
                    var data = enumInts.SByte;
                    foreach (var value in enumValues) *data++ = TmphPub.TmphEnumCast<TValueType, sbyte>.ToInt(value);
                }

                /// <summary>
                ///     枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
                public static void Parse(TmphJsonParser parser, ref TValueType value)
                {
                    if (parser.isEnumNumberFlag())
                    {
                        sbyte intValue = 0;
                        parser.Parse(ref intValue);
                        value = TmphPub.TmphEnumCast<TValueType, sbyte>.FromInt(intValue);
                    }
                    else parse(parser, ref value);
                }

                /// <summary>
                ///     枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
                public static void ParseFlags(TmphJsonParser parser, ref TValueType value)
                {
                    if (parser.isEnumNumberFlag())
                    {
                        sbyte intValue = 0;
                        parser.Parse(ref intValue);
                        value = TmphPub.TmphEnumCast<TValueType, sbyte>.FromInt(intValue);
                    }
                    else
                    {
                        int index, nextIndex = -1;
                        getIndex(parser, ref value, out index, ref nextIndex);
                        if (nextIndex != -1)
                        {
                            var searcher = new TmphStateSearcher(parser, enumSearcher);
                            var intValue = enumInts.SByte[index];
                            intValue |= enumInts.SByte[nextIndex];
                            while (parser.quote != 0)
                            {
                                index = searcher.NextFlagEnum();
                                if (parser.state != TmphParseState.Success) return;
                                if (index != -1) intValue |= enumInts.SByte[index];
                            }
                            value = TmphPub.TmphEnumCast<TValueType, sbyte>.FromInt(intValue);
                        }
                    }
                }
            }

            /// <summary>
            ///     枚举值解析
            /// </summary>
            internal sealed class TmphEnumShort : TmphEnumBase
            {
                /// <summary>
                ///     枚举值集合
                /// </summary>
                private static readonly TmphPointer enumInts;

                static TmphEnumShort()
                {
                    enumInts = TmphUnmanaged.Get(enumValues.Length * sizeof(short), false);
                    var data = enumInts.Short;
                    foreach (var value in enumValues) *data++ = TmphPub.TmphEnumCast<TValueType, short>.ToInt(value);
                }

                /// <summary>
                ///     枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
                public static void Parse(TmphJsonParser parser, ref TValueType value)
                {
                    if (parser.isEnumNumberFlag())
                    {
                        short intValue = 0;
                        parser.Parse(ref intValue);
                        value = TmphPub.TmphEnumCast<TValueType, short>.FromInt(intValue);
                    }
                    else parse(parser, ref value);
                }

                /// <summary>
                ///     枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
                public static void ParseFlags(TmphJsonParser parser, ref TValueType value)
                {
                    if (parser.isEnumNumberFlag())
                    {
                        short intValue = 0;
                        parser.Parse(ref intValue);
                        value = TmphPub.TmphEnumCast<TValueType, short>.FromInt(intValue);
                    }
                    else
                    {
                        int index, nextIndex = -1;
                        getIndex(parser, ref value, out index, ref nextIndex);
                        if (nextIndex != -1)
                        {
                            var searcher = new TmphStateSearcher(parser, enumSearcher);
                            var intValue = enumInts.Short[index];
                            intValue |= enumInts.Short[nextIndex];
                            while (parser.quote != 0)
                            {
                                index = searcher.NextFlagEnum();
                                if (parser.state != TmphParseState.Success) return;
                                if (index != -1) intValue |= enumInts.Short[index];
                            }
                            value = TmphPub.TmphEnumCast<TValueType, short>.FromInt(intValue);
                        }
                    }
                }
            }

            /// <summary>
            ///     枚举值解析
            /// </summary>
            internal sealed class TmphEnumUShort : TmphEnumBase
            {
                /// <summary>
                ///     枚举值集合
                /// </summary>
                private static readonly TmphPointer enumInts;

                static TmphEnumUShort()
                {
                    enumInts = TmphUnmanaged.Get(enumValues.Length * sizeof(ushort), false);
                    var data = enumInts.UShort;
                    foreach (var value in enumValues) *data++ = TmphPub.TmphEnumCast<TValueType, ushort>.ToInt(value);
                }

                /// <summary>
                ///     枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
                public static void Parse(TmphJsonParser parser, ref TValueType value)
                {
                    if (parser.isEnumNumber())
                    {
                        ushort intValue = 0;
                        parser.Parse(ref intValue);
                        value = TmphPub.TmphEnumCast<TValueType, ushort>.FromInt(intValue);
                    }
                    else parse(parser, ref value);
                }

                /// <summary>
                ///     枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
                public static void ParseFlags(TmphJsonParser parser, ref TValueType value)
                {
                    if (parser.isEnumNumber())
                    {
                        ushort intValue = 0;
                        parser.Parse(ref intValue);
                        value = TmphPub.TmphEnumCast<TValueType, ushort>.FromInt(intValue);
                    }
                    else
                    {
                        int index, nextIndex = -1;
                        getIndex(parser, ref value, out index, ref nextIndex);
                        if (nextIndex != -1)
                        {
                            var searcher = new TmphStateSearcher(parser, enumSearcher);
                            var intValue = enumInts.UShort[index];
                            intValue |= enumInts.UShort[nextIndex];
                            while (parser.quote != 0)
                            {
                                index = searcher.NextFlagEnum();
                                if (parser.state != TmphParseState.Success) return;
                                if (index != -1) intValue |= enumInts.UShort[index];
                            }
                            value = TmphPub.TmphEnumCast<TValueType, ushort>.FromInt(intValue);
                        }
                    }
                }
            }

            /// <summary>
            ///     枚举值解析
            /// </summary>
            internal sealed class TmphEnumInt : TmphEnumBase
            {
                /// <summary>
                ///     枚举值集合
                /// </summary>
                private static readonly TmphPointer enumInts;

                static TmphEnumInt()
                {
                    enumInts = TmphUnmanaged.Get(enumValues.Length * sizeof(int), false);
                    var data = enumInts.Int;
                    foreach (var value in enumValues) *data++ = TmphPub.TmphEnumCast<TValueType, int>.ToInt(value);
                }

                /// <summary>
                ///     枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
                public static void Parse(TmphJsonParser parser, ref TValueType value)
                {
                    if (parser.isEnumNumberFlag())
                    {
                        var intValue = 0;
                        parser.Parse(ref intValue);
                        value = TmphPub.TmphEnumCast<TValueType, int>.FromInt(intValue);
                    }
                    else parse(parser, ref value);
                }

                /// <summary>
                ///     枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
                public static void ParseFlags(TmphJsonParser parser, ref TValueType value)
                {
                    if (parser.isEnumNumberFlag())
                    {
                        var intValue = 0;
                        parser.Parse(ref intValue);
                        value = TmphPub.TmphEnumCast<TValueType, int>.FromInt(intValue);
                    }
                    else
                    {
                        int index, nextIndex = -1;
                        getIndex(parser, ref value, out index, ref nextIndex);
                        if (nextIndex != -1)
                        {
                            var searcher = new TmphStateSearcher(parser, enumSearcher);
                            var intValue = enumInts.Int[index];
                            intValue |= enumInts.Int[nextIndex];
                            while (parser.quote != 0)
                            {
                                index = searcher.NextFlagEnum();
                                if (parser.state != TmphParseState.Success) return;
                                if (index != -1) intValue |= enumInts.Int[index];
                            }
                            value = TmphPub.TmphEnumCast<TValueType, int>.FromInt(intValue);
                        }
                    }
                }
            }

            /// <summary>
            ///     枚举值解析
            /// </summary>
            internal sealed class TmphEnumUInt : TmphEnumBase
            {
                /// <summary>
                ///     枚举值集合
                /// </summary>
                private static readonly TmphPointer enumInts;

                static TmphEnumUInt()
                {
                    enumInts = TmphUnmanaged.Get(enumValues.Length * sizeof(uint), false);
                    var data = enumInts.UInt;
                    foreach (var value in enumValues) *data++ = TmphPub.TmphEnumCast<TValueType, uint>.ToInt(value);
                }

                /// <summary>
                ///     枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
                public static void Parse(TmphJsonParser parser, ref TValueType value)
                {
                    if (parser.isEnumNumber())
                    {
                        uint intValue = 0;
                        parser.Parse(ref intValue);
                        value = TmphPub.TmphEnumCast<TValueType, uint>.FromInt(intValue);
                    }
                    else parse(parser, ref value);
                }

                /// <summary>
                ///     枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
                public static void ParseFlags(TmphJsonParser parser, ref TValueType value)
                {
                    if (parser.isEnumNumber())
                    {
                        uint intValue = 0;
                        parser.Parse(ref intValue);
                        value = TmphPub.TmphEnumCast<TValueType, uint>.FromInt(intValue);
                    }
                    else
                    {
                        int index, nextIndex = -1;
                        getIndex(parser, ref value, out index, ref nextIndex);
                        if (nextIndex != -1)
                        {
                            var searcher = new TmphStateSearcher(parser, enumSearcher);
                            var intValue = enumInts.UInt[index];
                            intValue |= enumInts.UInt[nextIndex];
                            while (parser.quote != 0)
                            {
                                index = searcher.NextFlagEnum();
                                if (parser.state != TmphParseState.Success) return;
                                if (index != -1) intValue |= enumInts.UInt[index];
                            }
                            value = TmphPub.TmphEnumCast<TValueType, uint>.FromInt(intValue);
                        }
                    }
                }
            }

            /// <summary>
            ///     枚举值解析
            /// </summary>
            internal sealed class TmphEnumLong : TmphEnumBase
            {
                /// <summary>
                ///     枚举值集合
                /// </summary>
                private static readonly TmphPointer enumInts;

                static TmphEnumLong()
                {
                    enumInts = TmphUnmanaged.Get(enumValues.Length * sizeof(long), false);
                    var data = enumInts.Long;
                    foreach (var value in enumValues) *data++ = TmphPub.TmphEnumCast<TValueType, long>.ToInt(value);
                }

                /// <summary>
                ///     枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
                public static void Parse(TmphJsonParser parser, ref TValueType value)
                {
                    if (parser.isEnumNumberFlag())
                    {
                        long intValue = 0;
                        parser.Parse(ref intValue);
                        value = TmphPub.TmphEnumCast<TValueType, long>.FromInt(intValue);
                    }
                    else parse(parser, ref value);
                }

                /// <summary>
                ///     枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
                public static void ParseFlags(TmphJsonParser parser, ref TValueType value)
                {
                    if (parser.isEnumNumberFlag())
                    {
                        long intValue = 0;
                        parser.Parse(ref intValue);
                        value = TmphPub.TmphEnumCast<TValueType, long>.FromInt(intValue);
                    }
                    else
                    {
                        int index, nextIndex = -1;
                        getIndex(parser, ref value, out index, ref nextIndex);
                        if (nextIndex != -1)
                        {
                            var searcher = new TmphStateSearcher(parser, enumSearcher);
                            var intValue = enumInts.Long[index];
                            intValue |= enumInts.Long[nextIndex];
                            while (parser.quote != 0)
                            {
                                index = searcher.NextFlagEnum();
                                if (parser.state != TmphParseState.Success) return;
                                if (index != -1) intValue |= enumInts.Long[index];
                            }
                            value = TmphPub.TmphEnumCast<TValueType, long>.FromInt(intValue);
                        }
                    }
                }
            }

            /// <summary>
            ///     枚举值解析
            /// </summary>
            internal sealed class TmphEnumULong : TmphEnumBase
            {
                /// <summary>
                ///     枚举值集合
                /// </summary>
                private static readonly TmphPointer enumInts;

                static TmphEnumULong()
                {
                    enumInts = TmphUnmanaged.Get(enumValues.Length * sizeof(ulong), false);
                    var data = enumInts.ULong;
                    foreach (var value in enumValues) *data++ = TmphPub.TmphEnumCast<TValueType, ulong>.ToInt(value);
                }

                /// <summary>
                ///     枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
                public static void Parse(TmphJsonParser parser, ref TValueType value)
                {
                    if (parser.isEnumNumber())
                    {
                        ulong intValue = 0;
                        parser.Parse(ref intValue);
                        value = TmphPub.TmphEnumCast<TValueType, ulong>.FromInt(intValue);
                    }
                    else parse(parser, ref value);
                }

                /// <summary>
                ///     枚举值解析
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
                public static void ParseFlags(TmphJsonParser parser, ref TValueType value)
                {
                    if (parser.isEnumNumber())
                    {
                        ulong intValue = 0;
                        parser.Parse(ref intValue);
                        value = TmphPub.TmphEnumCast<TValueType, ulong>.FromInt(intValue);
                    }
                    else
                    {
                        int index, nextIndex = -1;
                        getIndex(parser, ref value, out index, ref nextIndex);
                        if (nextIndex != -1)
                        {
                            var searcher = new TmphStateSearcher(parser, enumSearcher);
                            var intValue = enumInts.ULong[index];
                            intValue |= enumInts.ULong[nextIndex];
                            while (parser.quote != 0)
                            {
                                index = searcher.NextFlagEnum();
                                if (parser.state != TmphParseState.Success) return;
                                if (index != -1) intValue |= enumInts.ULong[index];
                            }
                            value = TmphPub.TmphEnumCast<TValueType, ulong>.FromInt(intValue);
                        }
                    }
                }
            }

            /// <summary>
            ///     解析委托
            /// </summary>
            /// <param name="parser">Json解析器</param>
            /// <param name="value">目标数据</param>
            internal delegate void TmphTryParse(TmphJsonParser parser, ref TValueType value);

            /// <summary>
            ///     成员解析器过滤
            /// </summary>
            private struct TmphTryParseFilter
            {
                /// <summary>
                ///     成员选择
                /// </summary>
                public TmphMemberFilters Filter;

                /// <summary>
                ///     成员位图索引
                /// </summary>
                public int MemberMapIndex;

                /// <summary>
                ///     成员解析器
                /// </summary>
                public TmphTryParse TryParse;

                /// <summary>
                ///     成员解析器
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="value">目标数据</param>
                [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
                public void Call(TmphJsonParser parser, ref TValueType value)
                {
                    if ((parser.parseConfig.MemberFilter & Filter) == Filter) TryParse(parser, ref value);
                    else parser.ignore();
                }

                /// <summary>
                ///     成员解析器
                /// </summary>
                /// <param name="parser">Json解析器</param>
                /// <param name="memberMap">成员位图</param>
                /// <param name="value">目标数据</param>
                [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
                public void Call(TmphJsonParser parser, TmphMemberMap memberMap, ref TValueType value)
                {
                    if ((parser.parseConfig.MemberFilter & Filter) == Filter)
                    {
                        TryParse(parser, ref value);
                        memberMap.SetMember(MemberMapIndex);
                    }
                    else parser.ignore();
                }
            }
        }

        /// <summary>
        ///     解析类型
        /// </summary>
        private sealed class TmphParseMethod : Attribute
        {
        }
    }
}