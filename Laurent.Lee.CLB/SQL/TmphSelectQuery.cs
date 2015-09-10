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