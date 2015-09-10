using Laurent.Lee.CLB.Code;
using Laurent.Lee.CLB.Emit;
using Laurent.Lee.CLB.Web;
using System;
using System.Linq.Expressions;
using TmphDataSerialize = Laurent.Lee.CLB.Code.CSharp.TmphDataSerialize;

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     C#代码生成扩展
    /// </summary>
    public static class TmphCsharp
    {
        /// <summary>
        ///     连接字符串集合
        /// </summary>
        /// <param name="array">字符串集合</param>
        /// <param name="join">字符连接</param>
        /// <param name="isNull"></param>
        /// <returns>连接后的字符串</returns>
        public static string JoinString<TValueType>(this TValueType[] array, char join, bool isNull = false)
        {
            return TmphNumberToCharStream<TValueType>.JoinString(array, join, isNull);
        }

        /// <summary>
        ///     连接字符串集合
        /// </summary>
        /// <param name="array">字符串集合</param>
        /// <param name="join">字符连接</param>
        /// <param name="isNull"></param>
        /// <returns>连接后的字符串</returns>
        public static string JoinString<TValueType>(this TmphSubArray<TValueType> array, char join, bool isNull = false)
        {
            return TmphNumberToCharStream<TValueType>.JoinString(array, join, isNull);
        }

        /// <summary>
        ///     连接字符串集合
        /// </summary>
        /// <param name="array">字符串集合</param>
        /// <param name="stream">字符流</param>
        /// <param name="join">字符连接</param>
        /// <param name="isNull"></param>
        /// <returns>连接后的字符串</returns>
        public static void JoinString(this int[] array, TmphCharStream stream, char join, bool isNull = false)
        {
            if (array.length() == 0)
            {
                if (isNull) TmphAjax.WriteNull(stream);
                return;
            }
            TmphNumberToCharStream<int>.NumberJoinChar(stream, array, 0, array.Length, join, isNull);
        }

        /// <summary>
        ///     连接字符串集合
        /// </summary>
        /// <param name="array">字符串集合</param>
        /// <param name="stream">字符流</param>
        /// <param name="join">字符连接</param>
        /// <param name="isNull"></param>
        /// <returns>连接后的字符串</returns>
        public static void JoinString(this TmphSubArray<int> array, TmphCharStream stream, char join, bool isNull = false)
        {
            if (array.Count == 0)
            {
                if (isNull) TmphAjax.WriteNull(stream);
                return;
            }
            TmphNumberToCharStream<int>.NumberJoinChar(stream, array.array, array.StartIndex, array.Count, join, isNull);
        }

        /// <summary>
        ///     创建成员位图
        /// </summary>
        /// <typeparam name="TValueType">表格模型类型</typeparam>
        /// <param name="sqlTable"></param>
        /// <returns></returns>
        public static TmphMemberMap<TValueType>.TmphBuilder CreateMemberMap<TValueType>(
            this TmphSqlTable.TmphSqlTool<TValueType> sqlTable)
            where TValueType : class
        {
            return new TmphMemberMap<TValueType>.TmphBuilder(sqlTable != null);
        }

        /// <summary>
        ///     判断成员索引是否有效
        /// </summary>
        /// <typeparam name="TReturnType"></typeparam>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="sqlTable"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public static TmphMemberMap.TmphMemberIndex CreateMemberIndex<TValueType, TReturnType>(
            this TmphSqlTable.TmphSqlTool<TValueType> sqlTable, Expression<Func<TValueType, TReturnType>> member)
            where TValueType : class
        {
            return TmphMemberMap<TValueType>.CreateMemberIndex(member);
        }

        /// <summary>
        ///     创建成员位图
        /// </summary>
        /// <typeparam name="TValueType">表格模型类型</typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static TmphMemberMap<TValueType>.TmphBuilder CreateMemberMap<TValueType>(
            this TmphMemoryDatabaseTable.TmphTable<TValueType> table)
            where TValueType : class
        {
            return new TmphMemberMap<TValueType>.TmphBuilder(table != null);
        }

        /// <summary>
        ///     对象成员复制
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <typeparam name="TBaseType"></typeparam>
        /// <param name="value"></param>
        /// <param name="memberMap"></param>
        /// <returns></returns>
        public static TValueType Copy<TValueType, TBaseType>(this TBaseType value, TmphMemberMap memberMap = null)
            where TValueType : class, TBaseType
        {
            if (value == null) return null;
            var newValue = TmphConstructor<TValueType>.New();
            TmphMemberCopyer<TBaseType>.Copy(newValue, value, memberMap);
            return newValue;
        }

        /// <summary>
        ///     对象成员复制
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        /// <param name="memberMap"></param>
        /// <returns></returns>
        public static TValueType Copy<TValueType>(this TValueType value, TmphMemberMap memberMap = null)
            where TValueType : class
        {
            if (value == null) return null;
            var newValue = TmphConstructor<TValueType>.New();
            TmphMemberCopyer<TValueType>.Copy(newValue, value, memberMap);
            return newValue;
        }

        /// <summary>
        ///     对象转换JSON字符串
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="jsonStream">Json输出缓冲区</param>
        /// <param name="TmphConfig">配置参数</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static void ToJson<TValueType>(this TValueType value, TmphCharStream jsonStream,
            TmphJsonSerializer.TmphConfig TmphConfig = null)
        {
            TmphJsonSerializer.ToJson(value, jsonStream, TmphConfig);
        }

        /// <summary>
        ///     对象转换JSON字符串
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="TmphConfig">配置参数</param>
        /// <returns>Json字符串</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static string ToJson<TValueType>(this TValueType value, TmphJsonSerializer.TmphConfig TmphConfig = null)
        {
            return TmphJsonSerializer.ToJson(value, TmphConfig);
        }

        /// <summary>
        ///     Json解析
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="value">目标数据</param>
        /// <param name="json">Json字符串</param>
        /// <param name="TmphConfig">配置参数</param>
        /// <returns>是否解析成功</returns>
        public static bool FromJson<TValueType>(this TValueType value, TmphSubString json,
            TmphJsonParser.TmphConfig TmphConfig = null)
        {
            return TmphJsonParser.Parse(json, ref value, TmphConfig);
        }

        /// <summary>
        ///     序列化
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="stream">序列化输出缓冲区</param>
        /// <param name="TmphConfig">配置参数</param>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static void Serialize<TValueType>(this TValueType value, TmphUnmanagedStream stream,
            TmphDataSerializer.TmphConfig TmphConfig = null) where TValueType : TmphDataSerialize.ISerialize
        {
            TmphDataSerializer.CodeSerialize(value, stream, TmphConfig);
        }

        /// <summary>
        ///     序列化
        /// </summary>
        /// <typeparam name="TValueType">目标数据类型</typeparam>
        /// <param name="value">数据对象</param>
        /// <param name="TmphConfig">配置参数</param>
        /// <returns>序列化数据</returns>
        [TmphMethodImpl(TmphMethodImplOptions.AggressiveInlining)]
        public static byte[] Serialize<TValueType>(this TValueType value, TmphDataSerializer.TmphConfig TmphConfig = null)
            where TValueType : TmphDataSerialize.ISerialize
        {
            return TmphDataSerializer.CodeSerialize(value, TmphConfig);
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        /// <param name="data"></param>
        /// <param name="TmphConfig"></param>
        /// <returns>是否成功</returns>
        public static bool DeSerialize<TValueType>(this TValueType value, byte[] data,
            TmphBinaryDeSerializer.TmphConfig TmphConfig = null) where TValueType : TmphDataSerialize.ISerialize
        {
            return TmphDataDeSerializer.CodeDeSerialize(data, ref value, TmphConfig);
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="value"></param>
        /// <param name="data"></param>
        /// <param name="TmphConfig"></param>
        /// <returns>是否成功</returns>
        public static bool DeSerialize<TValueType>(this TValueType value, TmphSubArray<byte> data,
            TmphBinaryDeSerializer.TmphConfig TmphConfig = null) where TValueType : class, TmphDataSerialize.ISerialize
        {
            return TmphDataDeSerializer.CodeDeSerialize(data, ref value, TmphConfig);
        }
    }
}