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

using Laurent.Lee.CLB.Web;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Laurent.Lee.CLB.Sql.Expression
{
    /// <summary>
    ///     常量转换
    /// </summary>
    public class TmphConstantConverter
    {
        /// <summary>
        ///     常量转换字符串函数信息
        /// </summary>
        internal static readonly MethodInfo ConvertConstantStringMethod =
            typeof(TmphConstantConverter).GetMethod("convertConstantString",
                BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     常量转换
        /// </summary>
        internal static readonly TmphConstantConverter Default = new TmphConstantConverter();

        /// <summary>
        ///     常量转换处理集合
        /// </summary>
        protected Dictionary<Type, Action<TmphCharStream, object>> converters;

        /// <summary>
        ///     常量转换
        /// </summary>
        protected TmphConstantConverter()
        {
            converters = TmphDictionary.CreateOnly<Type, Action<TmphCharStream, object>>();
            converters.Add(typeof(bool), convertConstantBoolTo01);
            converters.Add(typeof(bool?), convertConstantBoolNullable);
            converters.Add(typeof(byte), convertConstantByte);
            converters.Add(typeof(byte?), convertConstantByteNullable);
            converters.Add(typeof(sbyte), convertConstantSByte);
            converters.Add(typeof(sbyte?), convertConstantSByteNullable);
            converters.Add(typeof(short), convertConstantShort);
            converters.Add(typeof(short?), convertConstantShortNullable);
            converters.Add(typeof(ushort), convertConstantUShort);
            converters.Add(typeof(ushort?), convertConstantUShortNullable);
            converters.Add(typeof(int), convertConstantInt);
            converters.Add(typeof(int?), convertConstantIntNullable);
            converters.Add(typeof(uint), convertConstantUInt);
            converters.Add(typeof(uint?), convertConstantUIntNullable);
            converters.Add(typeof(long), convertConstantLong);
            converters.Add(typeof(long?), convertConstantLongNullable);
            converters.Add(typeof(ulong), convertConstantULong);
            converters.Add(typeof(ulong?), convertConstantULongNullable);
            converters.Add(typeof(float), convertConstantFloat);
            converters.Add(typeof(float?), convertConstantFloatNullable);
            converters.Add(typeof(double), convertConstantDouble);
            converters.Add(typeof(double?), convertConstantDoubleNullable);
            converters.Add(typeof(decimal), convertConstantDecimal);
            converters.Add(typeof(decimal?), convertConstantDecimalNullable);
            converters.Add(typeof(DateTime), convertConstantDateTimeMillisecond);
            converters.Add(typeof(DateTime?), convertConstantDateTimeMillisecondNullable);
            converters.Add(typeof(string), convertConstantStringQuote);
        }

        /// <summary>
        ///     获取常量转换处理函数
        /// </summary>
        /// <param name="type">数据类型</param>
        /// <returns>失败返回null</returns>
        public Action<TmphCharStream, object> this[Type type]
        {
            get
            {
                Action<TmphCharStream, object> value;
                return converters.TryGetValue(type, out value) ? value : null;
            }
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantString<TValueType>(TmphCharStream sqlStream, TValueType value)
        {
            if (value == null) TmphAjax.WriteNull(sqlStream);
            else ConvertConstantStringQuote(sqlStream, value.ToString());
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, bool value)
        {
            sqlStream.Write(value ? '1' : '0');
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantBoolTo01(TmphCharStream sqlStream, object value)
        {
            convertConstant(sqlStream, (bool)value);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, bool? value)
        {
            if (value == null) TmphAjax.WriteNull(sqlStream);
            else sqlStream.Write((bool)value ? '1' : '0');
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantBoolNullable(TmphCharStream sqlStream, object value)
        {
            convertConstant(sqlStream, (bool?)value);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, byte value)
        {
            TmphNumber.ToString(value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantByte(TmphCharStream sqlStream, object value)
        {
            TmphNumber.ToString((byte)value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, byte? value)
        {
            if (value == null) TmphAjax.WriteNull(sqlStream);
            else TmphNumber.ToString((byte)value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantByteNullable(TmphCharStream sqlStream, object value)
        {
            convertConstant(sqlStream, (byte?)value);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, sbyte value)
        {
            TmphNumber.ToString(value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantSByte(TmphCharStream sqlStream, object value)
        {
            TmphNumber.ToString((sbyte)value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, sbyte? value)
        {
            if (value == null) TmphAjax.WriteNull(sqlStream);
            else TmphNumber.ToString((sbyte)value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantSByteNullable(TmphCharStream sqlStream, object value)
        {
            convertConstant(sqlStream, (sbyte?)value);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, short value)
        {
            TmphNumber.ToString(value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantShort(TmphCharStream sqlStream, object value)
        {
            TmphNumber.ToString((short)value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, short? value)
        {
            if (value == null) TmphAjax.WriteNull(sqlStream);
            else TmphNumber.ToString((short)value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantShortNullable(TmphCharStream sqlStream, object value)
        {
            convertConstant(sqlStream, (short?)value);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, ushort value)
        {
            TmphNumber.ToString(value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantUShort(TmphCharStream sqlStream, object value)
        {
            TmphNumber.ToString((ushort)value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, ushort? value)
        {
            if (value == null) TmphAjax.WriteNull(sqlStream);
            else TmphNumber.ToString((ushort)value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantUShortNullable(TmphCharStream sqlStream, object value)
        {
            convertConstant(sqlStream, (ushort?)value);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, int value)
        {
            TmphNumber.ToString(value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantInt(TmphCharStream sqlStream, object value)
        {
            TmphNumber.ToString((int)value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, int? value)
        {
            if (value == null) TmphAjax.WriteNull(sqlStream);
            else TmphNumber.ToString((int)value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantIntNullable(TmphCharStream sqlStream, object value)
        {
            convertConstant(sqlStream, (int?)value);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, uint value)
        {
            TmphNumber.ToString(value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantUInt(TmphCharStream sqlStream, object value)
        {
            TmphNumber.ToString((uint)value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, uint? value)
        {
            if (value == null) TmphAjax.WriteNull(sqlStream);
            else TmphNumber.ToString((uint)value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantUIntNullable(TmphCharStream sqlStream, object value)
        {
            convertConstant(sqlStream, (uint?)value);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, long value)
        {
            TmphNumber.ToString(value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantLong(TmphCharStream sqlStream, object value)
        {
            TmphNumber.ToString((long)value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, long? value)
        {
            if (value == null) TmphAjax.WriteNull(sqlStream);
            else TmphNumber.ToString((long)value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantLongNullable(TmphCharStream sqlStream, object value)
        {
            convertConstant(sqlStream, (long?)value);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, ulong value)
        {
            TmphNumber.ToString(value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantULong(TmphCharStream sqlStream, object value)
        {
            TmphNumber.ToString((ulong)value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, ulong? value)
        {
            if (value == null) TmphAjax.WriteNull(sqlStream);
            else TmphNumber.ToString((ulong)value, sqlStream);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantULongNullable(TmphCharStream sqlStream, object value)
        {
            convertConstant(sqlStream, (ulong?)value);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, float value)
        {
            sqlStream.WriteNotNull(value.ToString());
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantFloat(TmphCharStream sqlStream, object value)
        {
            sqlStream.WriteNotNull(((float)value).ToString());
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, float? value)
        {
            if (value == null) TmphAjax.WriteNull(sqlStream);
            else sqlStream.WriteNotNull(((float)value).ToString());
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantFloatNullable(TmphCharStream sqlStream, object value)
        {
            convertConstant(sqlStream, (float?)value);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, double value)
        {
            sqlStream.WriteNotNull(value.ToString());
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantDouble(TmphCharStream sqlStream, object value)
        {
            sqlStream.WriteNotNull(((double)value).ToString());
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, double? value)
        {
            if (value == null) TmphAjax.WriteNull(sqlStream);
            else sqlStream.WriteNotNull(((double)value).ToString());
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantDoubleNullable(TmphCharStream sqlStream, object value)
        {
            convertConstant(sqlStream, (double?)value);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, decimal value)
        {
            sqlStream.WriteNotNull(value.ToString());
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantDecimal(TmphCharStream sqlStream, object value)
        {
            sqlStream.WriteNotNull(((decimal)value).ToString());
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, decimal? value)
        {
            if (value == null) TmphAjax.WriteNull(sqlStream);
            else sqlStream.WriteNotNull(((decimal)value).ToString());
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantDecimalNullable(TmphCharStream sqlStream, object value)
        {
            convertConstant(sqlStream, (decimal?)value);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, DateTime value)
        {
            sqlStream.PrepLength(TmphDate.SqlMillisecondSize + 2);
            sqlStream.Unsafer.Write('\'');
            TmphDate.ToSqlMillisecond(value, sqlStream);
            sqlStream.Unsafer.Write('\'');
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantDateTimeMillisecond(TmphCharStream sqlStream, object value)
        {
            convertConstant(sqlStream, (DateTime)value);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, DateTime? value)
        {
            if (value == null) TmphAjax.WriteNull(sqlStream);
            else convertConstant(sqlStream, (DateTime)value);
        }

        /// <summary>
        ///     常量转换字符串
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantDateTimeMillisecondNullable(TmphCharStream sqlStream, object value)
        {
            convertConstant(sqlStream, (DateTime?)value);
        }

        /// <summary>
        ///     常量转换字符串(单引号变两个)
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstant(TmphCharStream sqlStream, string value)
        {
            if (value == null) TmphAjax.WriteNull(sqlStream);
            else ConvertConstantStringQuote(sqlStream, value);
        }

        /// <summary>
        ///     常量转换字符串(单引号变两个)
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        private void convertConstantStringQuote(TmphCharStream sqlStream, object value)
        {
            convertConstant(sqlStream, (string)value);
        }

        /// <summary>
        ///     SQL语句字符串格式化(单引号变两个)
        /// </summary>
        /// <param name="sqlStream">SQL字符流</param>
        /// <param name="value">常量</param>
        internal static unsafe void ConvertConstantStringQuote(TmphCharStream sqlStream, string value)
        {
            fixed (char* valueFixed = value)
            {
                var length = 0;
                for (char* start = valueFixed, end = valueFixed + value.Length; start != end; ++start)
                {
                    if (*start == '\'') ++length;
                }
                var unsafeStream = sqlStream.Unsafer;
                if (length == 0)
                {
                    sqlStream.PrepLength(value.Length + 2);
                    unsafeStream.Write('\'');
                    sqlStream.WriteNotNull(value);
                    unsafeStream.Write('\'');
                    return;
                }
                sqlStream.PrepLength((length += value.Length) + 2);
                unsafeStream.Write('\'');
                var write = (byte*)sqlStream.CurrentChar;
                for (char* start = valueFixed, end = valueFixed + value.Length; start != end; ++start)
                {
                    if (*start == '\'')
                    {
                        *(int*)write = ('\'' << 16) + '\'';
                        write += sizeof(int);
                    }
                    else
                    {
                        *(char*)write = *start;
                        write += sizeof(char);
                    }
                }
                unsafeStream.AddLength(length);
                unsafeStream.Write('\'');
            }
        }
    }
}