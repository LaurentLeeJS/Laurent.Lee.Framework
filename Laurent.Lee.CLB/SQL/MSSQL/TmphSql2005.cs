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

using Laurent.Lee.CLB.Code;
using Laurent.Lee.CLB.Emit;
using System.Collections.Generic;

namespace Laurent.Lee.CLB.Sql.MsSql
{
    /// <summary>
    ///     SQL Server2005/2008/2012客户端
    /// </summary>
    public class TmphSql2005 : TmphSql2000
    {
        /// <summary>
        ///     排序名称
        /// </summary>
        private const string orderOverName = "_ROW_";

        /// <summary>
        ///     SQL Server2005客户端
        /// </summary>
        /// <param name="connection">SQL连接信息</param>
        public TmphSql2005(TmphConnection connection) : base(connection)
        {
        }

        /// <summary>
        ///     最大字符串长度
        /// </summary>
        protected override string maxString
        {
            get { return "max"; }
        }

        /// <summary>
        ///     获取表格名称的SQL语句
        /// </summary>
        protected override string GetTableNameSql
        {
            get { return "select name from sysobjects where objectproperty(id,'IsUserTable')=1"; }
        }

        /// <summary>
        ///     根据表格名称获取表格信息的SQL语句
        /// </summary>
        /// <param name="tableName">表格名称</param>
        /// <returns>表格信息的SQL语句</returns>
        protected override string GetTableSql(string tableName)
        {
            return @"declare @id int
set @id=object_id(N'[dbo].[" + tableName + @"]')
if(select top 1 id from sysobjects where id=@id and objectproperty(id,N'IsUserTable')=1)is not null begin
 select columnproperty(id,name,'IsIdentity')as isidentity,id,xusertype,name,length,isnullable,colid,isnull((select top 1 text from syscomments where id=syscolumns.cdefault and colid=1),'')as defaultValue
  ,isnull((select value from ::fn_listextendedproperty(null,'user','dbo','table','" + tableName +
                   @"','column',syscolumns.name)as property where property.name='MS_Description'),'')as remark
  from syscolumns where id=@id order by colid
 if @@rowcount<>0 begin
  select a.indid,a.colid,b.name,(case when b.status=2 then 'UQ' else(select top 1 xtype from sysobjects where name=b.name)end)as type from sysindexkeys a left join sysindexes b on a.id=b.id and a.indid=b.indid where a.id=@id order by a.indid,a.keyno
 end
end";
            //备注
            //"select top 1 value from ::fn_listextendedproperty(null,'user','dbo','table','" + tableName + "','column','" + reader["name"].ToString() + "')as property where property.name='MS_Description'"
        }

        /// <summary>
        ///     查询对象集合
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="query">查询信息</param>
        /// <param name="memberMap">成员位图</param>
        /// <returns>对象集合</returns>
        internal override IEnumerable<TValueType> selectPushMemberMap<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TmphSelectQuery<TModelType> query, TmphMemberMap memberMap)
        {
            if (query != null && query.SkipCount != 0 && query.Orders.length() != 0)
            {
                if (TmphSqlModel<TModelType>.PrimaryKeys.Length == 1)
                {
                    return selectKeysPushMemberMap(sqlTool, query, TmphSqlModel<TModelType>.PrimaryKeys[0].Field.Name,
                        memberMap);
                }
                if (TmphSqlModel<TModelType>.Identity != null)
                {
                    return selectKeysPushMemberMap(sqlTool, query, TmphSqlModel<TModelType>.Identity.Field.Name, memberMap);
                }
                return selectRowsPushMemberMap(sqlTool, query, memberMap);
            }
            return selectNoOrderPushMemberMap(sqlTool, query, memberMap);
        }

        /// <summary>
        ///     查询对象集合
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        /// <typeparam name="TModelType">成员位图类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="query">查询信息</param>
        /// <param name="keyName">关键之名称</param>
        /// <param name="memberMap">成员位图</param>
        /// <returns>对象集合</returns>
        private unsafe IEnumerable<TValueType> selectKeysPushMemberMap<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TmphSelectQuery<TModelType> query, string keyName,
                TmphMemberMap memberMap)
            where TValueType : class, TModelType
            where TModelType : class
        {
            string sql = null;
            var TmphBuffer = SqlBuffers.Get();
            try
            {
                using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                {
                    sqlStream.WriteNotNull("select ");
                    TmphSqlModel<TModelType>.GetNames(sqlStream, memberMap);
                    sqlStream.WriteNotNull(" from[");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("]with(nolock)where ");
                    sqlStream.WriteNotNull(keyName);
                    sqlStream.WriteNotNull(" in(select ");
                    sqlStream.WriteNotNull(keyName);
                    sqlStream.WriteNotNull(" from(select ");
                    sqlStream.WriteNotNull(keyName);
                    sqlStream.WriteNotNull(",row_number()over(");
                    var startIndex = sqlStream.Length;
                    query.WriteOrder(sqlTool, sqlStream);
                    var count = sqlStream.Length - startIndex;
                    sqlStream.WriteNotNull(")as ");
                    sqlStream.WriteNotNull(orderOverName);
                    sqlStream.WriteNotNull(" from[");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("]with(nolock)");
                    if (query.WriteWhereOnly(sqlTool, sqlStream))
                    {
                        sqlStream.WriteNotNull(")as T where ");
                        sqlStream.WriteNotNull(orderOverName);
                        sqlStream.WriteNotNull(" between ");
                        TmphNumber.ToString(query.SkipCount, sqlStream);
                        sqlStream.WriteNotNull(" and ");
                        TmphNumber.ToString(query.SkipCount + query.GetCount - 1, sqlStream);
                        sqlStream.Write(')');
                        if (count != 0) sqlStream.Write(sqlStream.Char + startIndex, count);
                        sql = sqlStream.ToString();
                    }
                }
            }
            finally
            {
                SqlBuffers.Push(ref TmphBuffer);
            }
            return sql != null
                ? selectPushMemberMap<TValueType, TModelType>(sql, 0, memberMap)
                : TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     查询对象集合
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="query">查询信息</param>
        /// <param name="memberMap">成员位图</param>
        /// <returns>对象集合</returns>
        private unsafe IEnumerable<TValueType> selectRowsPushMemberMap<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TmphSelectQuery<TModelType> query, TmphMemberMap memberMap)
            where TValueType : class, TModelType
            where TModelType : class
        {
            string sql = null;
            var TmphBuffer = SqlBuffers.Get();
            try
            {
                using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                {
                    sqlStream.WriteNotNull("select * from(select ");
                    TmphSqlModel<TModelType>.GetNames(sqlStream, memberMap);
                    sqlStream.WriteNotNull(",row_number()over(");
                    query.WriteOrder(sqlTool, sqlStream);
                    sqlStream.WriteNotNull(")as ");
                    sqlStream.WriteNotNull(orderOverName);
                    sqlStream.WriteNotNull(" from[");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("]with(nolock)");
                    if (query.WriteWhereOnly(sqlTool, sqlStream))
                    {
                        sqlStream.WriteNotNull(")as T where ");
                        sqlStream.WriteNotNull(orderOverName);
                        sqlStream.WriteNotNull(" between ");
                        TmphNumber.ToString(query.SkipCount, sqlStream);
                        sqlStream.WriteNotNull(" and ");
                        TmphNumber.ToString(query.SkipCount + query.GetCount - 1, sqlStream);
                        sql = sqlStream.ToString();
                    }
                }
            }
            finally
            {
                SqlBuffers.Push(ref TmphBuffer);
            }
            return sql != null
                ? selectPushMemberMap<TValueType, TModelType>(sql, 0, memberMap)
                : TmphNullValue<TValueType>.Array;
        }
    }
}