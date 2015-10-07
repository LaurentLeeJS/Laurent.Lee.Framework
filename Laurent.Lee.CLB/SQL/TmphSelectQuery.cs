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

using Laurent.Lee.CLB.Emit;
using System;
using System.Linq.Expressions;

namespace Laurent.Lee.CLB.Sql
{
    /// <summary>
    ///     查询信息
    /// </summary>
    /// <typeparam name="TValueType">对象类型</typeparam>
    public sealed class TmphSelectQuery<TValueType>
    {
        /// <summary>
        ///     获取记录数量,0表示不限
        /// </summary>
        public int GetCount;

        /// <summary>
        ///     是否已经创建查询索引
        /// </summary>
        private bool isCreatedIndex;

        /// <summary>
        ///     排序表达式集合,false为升序,true为降序
        /// </summary>
        public TmphKeyValue<LambdaExpression, bool>[] Orders;

        /// <summary>
        ///     跳过记录数量
        /// </summary>
        public int SkipCount;

        /// <summary>
        ///     排序字符串集合,false为升序,true为降序
        /// </summary>
        internal TmphKeyValue<string, bool>[] StringOrders;

        /// <summary>
        ///     查询条件表达式
        /// </summary>
        public Expression<Func<TValueType, bool>> Where;

        /// <summary>
        ///     查询条件表达式隐式转换为查询信息
        /// </summary>
        /// <param name="expression">查询条件表达式</param>
        /// <returns>查询信息</returns>
        public static implicit operator TmphSelectQuery<TValueType>(Expression<Func<TValueType, bool>> expression)
        {
            return expression == null ? null : new TmphSelectQuery<TValueType> { Where = expression };
        }

        /// <summary>
        ///     查询条件
        /// </summary>
        /// <param name="sqlTable">数据库表格操作工具</param>
        /// <param name="sqlStream">SQL表达式流</param>
        /// <returns></returns>
        internal bool WriteWhere(TmphSqlTable.TmphSqlTool sqlTable, TmphCharStream sqlStream)
        {
            return WriteWhere(sqlTable, sqlStream, Where, ref isCreatedIndex);
        }

        /// <summary>
        ///     查询条件
        /// </summary>
        /// <param name="sqlTable">数据库表格操作工具</param>
        /// <param name="sqlStream">SQL表达式流</param>
        /// <returns></returns>
        internal bool WriteWhereOnly(TmphSqlTable.TmphSqlTool sqlTable, TmphCharStream sqlStream)
        {
            if (Where == null) return true;
            var length = sqlStream.Length;
            var logicConstantWhere = false;
            var name = sqlTable.Client.GetWhere(Where, sqlStream, ref logicConstantWhere);
            if (length == sqlStream.Length) return logicConstantWhere;
            if (name != null)
            {
                isCreatedIndex = true;
                sqlTable.CreateIndex(name);
            }
            return true;
        }

        /// <summary>
        ///     排序字符串
        /// </summary>
        /// <param name="sqlTable">数据库表格操作工具</param>
        /// <param name="sqlStream">SQL表达式流</param>
        internal void WriteOrder(TmphSqlTable.TmphSqlTool sqlTable, TmphCharStream sqlStream)
        {
            if (Orders != null)
            {
                var isNext = 0;
                sqlStream.WriteNotNull(" order by ");
                foreach (var order in Orders)
                {
                    if (isNext == 0) isNext = 1;
                    else sqlStream.Write(',');
                    var name = sqlTable.Client.GetSql(order.Key, sqlStream);
                    if (order.Value) sqlStream.WriteNotNull(" desc");
                    if (!isCreatedIndex && name != null)
                    {
                        isCreatedIndex = true;
                        sqlTable.CreateIndex(name);
                    }
                }
            }
            else if (StringOrders != null)
            {
                var isNext = 0;
                sqlStream.WriteNotNull(" order by ");
                foreach (var order in StringOrders)
                {
                    if (isNext == 0) isNext = 1;
                    else sqlStream.Write(',');
                    sqlStream.WriteNotNull(order.Key);
                    if (order.Value) sqlStream.WriteNotNull(" desc");
                }
            }
        }

        /// <summary>
        ///     查询条件
        /// </summary>
        /// <param name="sqlTable">数据库表格操作工具</param>
        /// <param name="sqlStream">SQL表达式流</param>
        /// <param name="expression">查询条件表达式</param>
        /// <param name="isCreatedIndex">是否已经创建查询索引</param>
        /// <returns></returns>
        internal static unsafe bool WriteWhere(TmphSqlTable.TmphSqlTool sqlTable, TmphCharStream sqlStream,
            Expression<Func<TValueType, bool>> expression, ref bool isCreatedIndex)
        {
            if (expression == null) return true;
            sqlStream.PrepLength(6);
            sqlStream.Unsafer.AddLength(6);
            var length = sqlStream.Length;
            var logicConstantWhere = false;
            var name = sqlTable.Client.GetWhere(expression, sqlStream, ref logicConstantWhere);
            if (length == sqlStream.Length)
            {
                sqlStream.Unsafer.AddLength(-6);
                return logicConstantWhere;
            }
            if (name != null)
            {
                var where = (byte*)(sqlStream.Char + length);
                *(uint*)(where - sizeof(uint)) = 'e' + (' ' << 16);
                *(uint*)(where - sizeof(uint) * 2) = 'e' + ('r' << 16);
                *(uint*)(where - sizeof(uint) * 3) = 'w' + ('h' << 16);
                isCreatedIndex = true;
                sqlTable.CreateIndex(name);
            }
            return true;
        }
    }
}