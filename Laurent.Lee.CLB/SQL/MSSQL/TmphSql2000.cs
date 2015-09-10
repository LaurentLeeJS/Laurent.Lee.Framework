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
using Laurent.Lee.CLB.Config;
using Laurent.Lee.CLB.Emit;
using Laurent.Lee.CLB.Sql.Expression;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq.Expressions;

namespace Laurent.Lee.CLB.Sql.MsSql
{
    /// <summary>
    ///     SQL Server2000客户端
    /// </summary>
    public class TmphSql2000 : TmphClient
    {
        /// <summary>
        ///     最大字符串长度
        /// </summary>
        private const int maxStringSize = 4000;

        ///// <summary>
        ///// 最大参数数量
        ///// </summary>
        //public const int MaxParameterCount = 2100 - 3;
        /// <summary>
        ///     SQL Server2000客户端
        /// </summary>
        /// <param name="connection">SQL连接信息</param>
        public TmphSql2000(TmphConnection connection) : base(connection)
        {
        }

        /// <summary>
        ///     是否支持DataTable导入
        /// </summary>
        protected override bool isImport
        {
            get { return true; }
        }

        /// <summary>
        ///     最大字符串长度
        /// </summary>
        protected virtual string maxString
        {
            get { return "max"; }
        }

        /// <summary>
        ///     获取表格名称的SQL语句
        /// </summary>
        protected virtual string GetTableNameSql
        {
            get
            {
                return
                    "select name from sysobjects where(status&0xe0000000)=0x60000000 and objectproperty(id,'IsUserTable')=1";
            }
        }

        /// <summary>
        ///     根据SQL连接类型获取SQL连接
        /// </summary>
        /// <param name="connection">SQL连接信息</param>
        /// <param name="isAsynchronous">是否异步连接</param>
        /// <returns>SQL连接</returns>
        protected override DbConnection getConnection(bool isAsynchronous)
        {
            return
                new SqlConnection(isAsynchronous
                    ? Connection.Connection + ";Asynchronous Processing=true"
                    : Connection.Connection);
        }

        /// <summary>
        ///     获取SQL命令
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="type">SQL命令类型</param>
        /// <returns>SQL命令</returns>
        public override DbCommand GetCommand
            (DbConnection connection, string sql, SqlParameter[] parameters = null, CommandType type = CommandType.Text)
        {
            DbCommand command = new SqlCommand(sql, (SqlConnection)connection);
            command.CommandType = type;
            if (parameters != null) command.Parameters.AddRange(parameters);
            return command;
        }

        /// <summary>
        ///     获取数据适配器
        /// </summary>
        /// <param name="command">SQL命令</param>
        /// <returns>数据适配器</returns>
        protected override DbDataAdapter getAdapter(DbCommand command)
        {
            return new SqlDataAdapter((SqlCommand)command);
        }

        /// <summary>
        ///     获取数据表格
        /// </summary>
        /// <param name="tableName">表格名称</param>
        /// <returns>数据表格</returns>
        public override DataTable GetDataTable(string tableName)
        {
            if (tableName != null && tableName.Length != 0)
            {
                using (var connection = getConnection(false))
                {
                    var table = GetDataTable(GetCommand(connection, "select * from [" + tableName + "]"));
                    if (table != null)
                    {
                        table.TableName = tableName;
                        return table;
                    }
                }
            }
            return null;
        }

        /// <summary>
        ///     导入数据集合
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="data">数据集合</param>
        /// <param name="batchSize">批处理数量</param>
        /// <param name="timeout">超时秒数</param>
        /// <returns>成功导入数量</returns>
        protected override int import(DbConnection connection, DataTable data, int batchSize, int timeout)
        {
            using (var copy = new SqlBulkCopy((SqlConnection)connection))
            {
                var count = data.Rows.Count;
                if (batchSize <= 0) batchSize = TmphSql.Default.ImportBatchSize;
                copy.BulkCopyTimeout = timeout == 0 ? (count / batchSize) + 1 : timeout;
                copy.BatchSize = batchSize;
                copy.DestinationTableName = data.TableName;
                foreach (DataColumn column in data.Columns)
                    copy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                copy.WriteToServer(data);
                return count;
            }
        }

        /// <summary>
        ///     判断表格是否存在
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="tableName">表格名称</param>
        /// <returns>表格是否存在</returns>
        protected override bool isTable(DbConnection connection, string tableName)
        {
            var sql = "select top 1 id from dbo.sysobjects where id=object_id(N'[" + Connection.Owner + "].[" +
                      tableName + "]')and objectproperty(id,N'IsUserTable')=1";
            using (var command = GetCommand(connection, sql)) return GetValue(command, 0) != 0;
        }

        /// <summary>
        ///     创建表格
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="table">表格信息</param>
        internal override unsafe bool createTable(DbConnection connection, TmphTable table)
        {
            string tableName = table.Columns.Name, sql;
            var TmphBuffer = SqlBuffers.Get();
            try
            {
                using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                {
                    sqlStream.WriteNotNull("create table[");
                    sqlStream.WriteNotNull(Connection.Owner);
                    sqlStream.WriteNotNull("].[");
                    sqlStream.WriteNotNull(tableName);
                    sqlStream.WriteNotNull("](");
                    bool isTextImage = false, isNext = false;
                    foreach (var column in table.Columns.Columns)
                    {
                        if (isNext) sqlStream.Write(',');
                        appendColumn(sqlStream, column);
                        if (!isTextImage) isTextImage = column.DbType.isTextImageType();
                        isNext = true;
                    }
                    var primaryKey = table.PrimaryKey;
                    if (primaryKey != null && primaryKey.Columns.length() != 0)
                    {
                        isNext = false;
                        sqlStream.WriteNotNull(",primary key(");
                        foreach (var column in primaryKey.Columns)
                        {
                            if (isNext) sqlStream.Write(',');
                            sqlStream.WriteNotNull(column.Name);
                            isNext = true;
                        }
                        sqlStream.Write(')');
                    }
                    sqlStream.WriteNotNull(")on[primary]");
                    if (isTextImage) sqlStream.WriteNotNull(" textimage_on[primary]");
                    foreach (var column in table.Columns.Columns)
                    {
                        if (column.Remark.Length() != 0)
                        {
                            sqlStream.WriteNotNull(@"
exec dbo.sp_addextendedproperty @name=N'MS_Description',@value=N");
                            TmphConstantConverter.ConvertConstantStringQuote(sqlStream, column.Remark);
                            sqlStream.WriteNotNull(",@level0type=N'USER',@level0name=N'");
                            sqlStream.WriteNotNull(Connection.Owner);
                            sqlStream.WriteNotNull("',@level1type=N'TABLE',@level1name=N'");
                            sqlStream.WriteNotNull(tableName);
                            sqlStream.WriteNotNull("', @level2type=N'COLUMN',@level2name=N'");
                            sqlStream.WriteNotNull(column.Name);
                            sqlStream.Write('\'');
                        }
                    }
                    if (table.Indexs != null)
                    {
                        foreach (var columns in table.Indexs)
                        {
                            if (columns != null && columns.Columns.length() != 0)
                            {
                                sqlStream.WriteNotNull(@"
create");
                                if (columns.Type == THColumnCollection.TmphType.UniqueIndex)
                                    sqlStream.WriteNotNull(" unique");
                                sqlStream.WriteNotNull(" index[");
                                AppendIndexName(sqlStream, tableName, columns);
                                sqlStream.WriteNotNull("]on[");
                                sqlStream.WriteNotNull(Connection.Owner);
                                sqlStream.WriteNotNull("].[");
                                sqlStream.WriteNotNull(tableName);
                                sqlStream.WriteNotNull("](");
                                isNext = false;
                                foreach (var column in columns.Columns)
                                {
                                    if (isNext) sqlStream.Write(',');
                                    sqlStream.Write('[');
                                    sqlStream.WriteNotNull(column.Name);
                                    sqlStream.Write(']');
                                    isNext = true;
                                }
                                sqlStream.WriteNotNull(")on[primary]");
                            }
                        }
                    }
                    sql = sqlStream.ToString();
                }
            }
            finally
            {
                SqlBuffers.Push(ref TmphBuffer);
            }
            return executeNonQuery(connection, sql) != ExecuteNonQueryError;
        }

        /// <summary>
        ///     成员信息转换为数据列
        /// </summary>
        /// <param name="type">成员类型</param>
        /// <param name="sqlMember">SQL成员信息</param>
        /// <returns>数据列</returns>
        internal override TmphColumn getColumn(Type type, TmphDataMember sqlMember)
        {
            var sqlType = SqlDbType.NVarChar;
            var size = maxStringSize;
            TmphMemberType memberType = sqlMember.DataType != null ? sqlMember.DataType : type;
            if (memberType.IsString)
            {
                if (sqlMember.MaxStringLength > 0 && sqlMember.MaxStringLength <= maxStringSize)
                {
                    if (sqlMember.IsFixedLength) sqlType = sqlMember.IsAscii ? SqlDbType.Char : SqlDbType.NChar;
                    else sqlType = sqlMember.IsAscii ? SqlDbType.VarChar : SqlDbType.NVarChar;
                    size = sqlMember.MaxStringLength <= maxStringSize ? sqlMember.MaxStringLength : maxStringSize;
                }
                else if (!sqlMember.IsFixedLength && sqlMember.MaxStringLength == -1)
                {
                    sqlType = sqlMember.IsAscii ? SqlDbType.VarChar : SqlDbType.NVarChar;
                    size = sqlMember.MaxStringLength <= maxStringSize ? sqlMember.MaxStringLength : maxStringSize;
                }
                else
                {
                    sqlType = sqlMember.IsAscii ? SqlDbType.Text : SqlDbType.NText;
                    size = int.MaxValue;
                }
            }
            else
            {
                sqlType = memberType.Type.formCSharpType();
                size = sqlType.getSize();
            }
            return new TmphColumn
            {
                DbType = sqlType,
                Size = size,
                IsNull =
                    (sqlMember.IsDefaultMember && !memberType.IsString ? ((TmphMemberType)type).IsNull : sqlMember.IsNull),
                DefaultValue = sqlMember.DefaultValue,
                UpdateValue = sqlMember.UpdateValue
            };
        }

        /// <summary>
        ///     删除表格
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="tableName">表格名称</param>
        protected override bool dropTable(DbConnection connection, string tableName)
        {
            return executeNonQuery(connection, "drop table[" + Connection.Owner + "].[" + tableName + "]") !=
                   ExecuteNonQueryError;
        }

        /// <summary>
        ///     写入列信息
        /// </summary>
        /// <param name="sqlStream">SQL语句流</param>
        /// <param name="column">列信息</param>
        /// <param name="isIdentity">是否自增列</param>
        private void appendColumn(TmphCharStream sqlStream, TmphColumn column)
        {
            sqlStream.Write('[');
            sqlStream.WriteNotNull(column.Name);
            sqlStream.Write(']');
            sqlStream.WriteNotNull(column.DbType.ToString());
            //if (isIdentity) sqlStream.Write(" identity(1,1)not");
            //else
            //{
            if (column.DbType.isStringType() && column.Size != int.MaxValue)
            {
                sqlStream.Write('(');
                sqlStream.WriteNotNull(column.Size == -1 ? maxString : column.Size.toString());
                sqlStream.Write(')');
            }
            if (column.DefaultValue != null)
            {
                sqlStream.WriteNotNull(" default ");
                sqlStream.WriteNotNull(column.DefaultValue);
            }
            if (!column.IsNull) sqlStream.WriteNotNull(" not");
            //}
            sqlStream.WriteNotNull(" null");
        }

        /// <summary>
        ///     创建索引
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="tableName">表格名称</param>
        /// <param name="columnCollection">索引列集合</param>
        internal override unsafe bool createIndex(DbConnection connection, string tableName,
            THColumnCollection columnCollection)
        {
            string sql;
            var TmphBuffer = SqlBuffers.Get();
            try
            {
                using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                {
                    sqlStream.WriteNotNull(@"
create index[");
                    AppendIndexName(sqlStream, tableName, columnCollection);
                    sqlStream.WriteNotNull("]on[");
                    sqlStream.WriteNotNull(Connection.Owner);
                    sqlStream.WriteNotNull("].[");
                    sqlStream.WriteNotNull(tableName);
                    sqlStream.WriteNotNull("](");
                    var isNext = false;
                    foreach (var column in columnCollection.Columns)
                    {
                        if (isNext) sqlStream.Write(',');
                        sqlStream.Write('[');
                        sqlStream.WriteNotNull(column.Name);
                        sqlStream.Write(']');
                        isNext = true;
                    }
                    sqlStream.WriteNotNull(")on[primary]");
                    sql = sqlStream.ToString();
                }
            }
            finally
            {
                SqlBuffers.Push(ref TmphBuffer);
            }
            return executeNonQuery(connection, sql) != ExecuteNonQueryError;
        }

        /// <summary>
        ///     新增列集合
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="columnCollection">新增列集合</param>
        internal override unsafe bool addFields(DbConnection connection, THColumnCollection columnCollection)
        {
            string tableName = columnCollection.Name, sql;
            var TmphBuffer = SqlBuffers.Get();
            try
            {
                using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                {
                    var isUpdateValue = false;
                    foreach (var column in columnCollection.Columns)
                    {
                        sqlStream.WriteNotNull(@"
alter table [");
                        sqlStream.WriteNotNull(Connection.Owner);
                        sqlStream.WriteNotNull("].[");
                        sqlStream.WriteNotNull(tableName);
                        sqlStream.WriteNotNull(@"]add ");
                        if (!column.IsNull && column.DefaultValue == null)
                        {
                            column.DefaultValue = column.DbType.getDefaultValue();
                            if (column.DefaultValue == null) column.IsNull = true;
                        }
                        appendColumn(sqlStream, column);
                        if (column.UpdateValue != null) isUpdateValue = true;
                    }
                    if (isUpdateValue)
                    {
                        sqlStream.WriteNotNull(@"
update[");
                        sqlStream.WriteNotNull(tableName);
                        sqlStream.WriteNotNull("]set ");
                        foreach (var column in columnCollection.Columns)
                        {
                            if (column.UpdateValue != null)
                            {
                                if (!isUpdateValue) sqlStream.Write(',');
                                sqlStream.WriteNotNull(column.Name);
                                sqlStream.Write('=');
                                sqlStream.WriteNotNull(column.UpdateValue);
                                isUpdateValue = false;
                            }
                        }
                        sqlStream.WriteNotNull(" from[" + tableName + "]with(nolock)");
                    }
                    sql = sqlStream.ToString();
                }
            }
            finally
            {
                SqlBuffers.Push(ref TmphBuffer);
            }
            return executeNonQuery(connection, sql) != ExecuteNonQueryError;
        }

        /// <summary>
        ///     获取表格名称集合
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <returns>表格名称集合</returns>
        protected override TmphSubArray<string> getTableNames(DbConnection connection)
        {
            var value = new TmphSubArray<string>();
            using (var command = GetCommand(connection, GetTableNameSql))
            using (var reader = command.ExecuteReader(CommandBehavior.Default))
            {
                while (reader.Read()) value.Add((string)reader[0]);
            }
            return value;
        }

        /// <summary>
        ///     根据表格名称获取表格信息
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="name">表格名称</param>
        /// <returns>表格信息</returns>
        internal override TmphTable getTable(DbConnection connection, string tableName)
        {
            using (var command = GetCommand(connection, GetTableSql(tableName)))
            using (var reader = command.ExecuteReader(CommandBehavior.Default))
            {
                TmphColumn identity = null;
                var columns = TmphDictionary.CreateShort<TmphColumn>();
                while (reader.Read())
                {
                    var type = TmphSqlDbType.GetType((short)reader["xusertype"]);
                    int size = (short)reader["length"];
                    if (type == SqlDbType.NChar || type == SqlDbType.NVarChar) size >>= 1;
                    else if (type == SqlDbType.Text || type == SqlDbType.NText) size = int.MaxValue;
                    var column = new TmphColumn
                    {
                        Name = reader["name"].ToString(),
                        DbType = type,
                        Size = size,
                        DefaultValue = formatDefaultValue(reader["defaultValue"]),
                        Remark = reader["remark"].ToString(),
                        //GetColumnRemark(table, connection, name),
                        IsNull = (int)reader["isnullable"] == 1
                    };
                    columns.Add((short)reader["colid"], column);
                    if ((int)reader["isidentity"] == 1) identity = column;
                }
                var columnCollections = default(TmphSubArray<THColumnCollection>);
                if (reader.NextResult())
                {
                    short indexId = -1;
                    string indexName = null;
                    var columnType = THColumnCollection.TmphType.Index;
                    var columnId = default(TmphSubArray<short>);
                    while (reader.Read())
                    {
                        if (indexId != (short)reader["indid"])
                        {
                            if (indexId != -1)
                            {
                                var indexs = columnId.GetArray(columnIndex => columns[columnIndex]);
                                columnCollections.Add(new THColumnCollection
                                {
                                    Type = columnType,
                                    Name = indexName,
                                    Columns = indexs
                                });
                            }
                            columnId.Empty();
                            indexId = (short)reader["indid"];
                            indexName = reader["name"].ToString();
                            var type = reader["type"].ToString();
                            if (type == "PK") columnType = THColumnCollection.TmphType.PrimaryKey;
                            else if (type == "UQ") columnType = THColumnCollection.TmphType.UniqueIndex;
                            else columnType = THColumnCollection.TmphType.Index;
                        }
                        columnId.Add((short)reader["colid"]);
                    }
                    if (indexId != -1)
                    {
                        columnCollections.Add(new THColumnCollection
                        {
                            Type = columnType,
                            Name = indexName,
                            Columns = columnId.GetArray(columnIndex => columns[columnIndex])
                        });
                    }
                }
                if (columns.Count != 0)
                {
                    var primaryKey =
                        columnCollections.FirstOrDefault(
                            columnCollection => columnCollection.Type == THColumnCollection.TmphType.PrimaryKey);
                    return new TmphTable
                    {
                        Columns = new THColumnCollection
                        {
                            Name = tableName,
                            Columns = columns.Values.GetArray(),
                            Type = THColumnCollection.TmphType.None
                        },
                        Identity = identity,
                        PrimaryKey = primaryKey,
                        Indexs =
                            columnCollections.GetFindArray(
                                columnCollection => columnCollection.Type != THColumnCollection.TmphType.PrimaryKey)
                    };
                }
                return null;
            }
        }

        /// <summary>
        ///     根据表格名称获取表格信息的SQL语句
        /// </summary>
        /// <param name="tableName">表格名称</param>
        /// <returns>表格信息的SQL语句</returns>
        protected virtual string GetTableSql(string tableName)
        {
            return @"declare @id int
set @id=object_id(N'[dbo].[" + tableName + @"]')
if(select top 1 id from sysobjects where id=@id and objectproperty(id,N'IsUserTable')=1)is not null begin
 select columnproperty(id,name,'IsIdentity')as isidentity,id,xusertype,name,length,isnullable,colid,isnull((select top 1 text from syscomments where id=syscolumns.cdefault and colid=1),'')as defaultValue,isnull((select top 1 cast(value as varchar(256))from sysproperties where id=syscolumns.id and smallid=syscolumns.colid),'')as remark from syscolumns where id=@id order by colid
 if @@rowcount<>0 begin
  select a.indid,a.colid,b.name,(case when b.status=2 then 'UQ' else(select top 1 xtype from sysobjects where name=b.name)end)as type from sysindexkeys a left join sysindexes b on a.id=b.id and a.indid=b.indid where a.id=@id order by a.indid,a.keyno
 end
end";
        }

        /// <summary>
        ///     删除默认值左右括号()
        /// </summary>
        /// <param name="defaultValue">默认值</param>
        /// <returns>默认值</returns>
        protected static string formatDefaultValue(object defaultValue)
        {
            if (defaultValue != null)
            {
                var value = defaultValue.ToString();
                if (value.Length != 0)
                {
                    int valueIndex = 0, index = 0;
                    var valueIndexs = new int[value.Length];
                    for (var length = value.Length; index != length; ++index)
                    {
                        if (value[index] == '(') ++valueIndex;
                        else if (value[index] == ')') valueIndexs[--valueIndex] = index;
                    }
                    index = 0;
                    for (var length = value.Length - 1; valueIndexs[index] == length && value[index] == '('; --length)
                        ++index;
                    value = value.Substring(index, value.Length - (index << 1));
                }
                return value;
            }
            return null;
        }

        ///// <summary>
        ///// like转义字符位图
        ///// </summary>
        //private static readonly String.asciiMap likeMap = new String.asciiMap(@"[]*_%", true);
        ///// <summary>
        ///// like转义
        ///// </summary>
        //private struct toLiker
        //{
        //    /// <summary>
        //    /// 源字符串
        //    /// </summary>
        //    public string Value;
        //    /// <summary>
        //    /// 字符串like转义
        //    /// </summary>
        //    /// <param name="map">转义索引位图</param>
        //    /// <returns>转义后的字符串</returns>
        //    public unsafe string Get(fixedMap map)
        //    {
        //        int count = 0;
        //        String.asciiMap likeMap = sql2000.likeMap;
        //        fixed (char* valueFixed = Value)
        //        {
        //            for (char* start = valueFixed, end = valueFixed + Value.Length; start != end; ++start)
        //            {
        //                if (likeMap.Get(*start))
        //                {
        //                    map.Set((int)(start - valueFixed));
        //                    ++count;
        //                }
        //            }
        //            if (count != 0)
        //            {
        //                string newValue = Laurent.Lee.CLB.String.FastAllocateString(Value.Length + (count << 1));
        //                fixed (char* newValueFixed = newValue)
        //                {
        //                    char* write = newValueFixed, read = valueFixed;
        //                    for (int index = 0; index != Value.Length; ++index)
        //                    {
        //                        if (map.Get(index))
        //                        {
        //                            *write++ = '[';
        //                            *write++ = *read++;
        //                            *write++ = ']';
        //                        }
        //                        else *write++ = *read++;
        //                    }
        //                }
        //                return newValue;
        //            }
        //        }
        //        return Value;
        //    }
        //}
        ///// <summary>
        ///// 字符串like转义
        ///// </summary>
        ///// <param name="value">字符串</param>
        ///// <returns>转义后的字符串</returns>
        //public unsafe string ToLike(string value)
        //{
        //    return value.length() != 0 ? fixedMap.GetMap<string>(value.Length, new toLiker { Value = value }.Get) : value;
        //}
        /// <summary>
        ///     插入数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="value">待插入数据</param>
        /// <param name="memberMap">目标成员位图</param>
        /// <returns>是否成功</returns>
        protected override unsafe bool insert<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap memberMap)
        {
            var TmphBuffer = SqlBuffers.Get();
            try
            {
                using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                {
                    if (TmphSqlModel<TModelType>.Identity != null)
                    {
                        var identity = sqlTool.NextIdentity;
                        TmphSqlModel<TModelType>.SetIdentity(value, identity);
                        sqlStream.WriteNotNull("insert into[");
                        sqlStream.WriteNotNull(sqlTool.TableName);
                        sqlStream.WriteNotNull("](");
                        TmphSqlModel<TModelType>.TmphInsert.GetColumnNames(sqlStream, memberMap);
                        sqlStream.WriteNotNull(")values(");
                        TmphSqlModel<TModelType>.TmphInsert.Insert(sqlStream, memberMap, value, Converter);
                        sqlStream.WriteNotNull(@")
if @@ROWCOUNT<>0 begin
 select top 1 ");
                        TmphSqlModel<TModelType>.GetNames(sqlStream, TmphSqlModel<TModelType>.MemberMap);
                        sqlStream.WriteNotNull(" from[");
                        sqlStream.WriteNotNull(sqlTool.TableName);
                        sqlStream.WriteNotNull("]with(nolock)where ");
                        sqlStream.WriteNotNull(TmphSqlModel<TModelType>.Identity.Field.Name);
                        sqlStream.Write('=');
                        TmphNumber.ToString(identity, sqlStream);
                        sqlStream.WriteNotNull(@"
end");
                        if (set<TValueType, TModelType>(sqlStream.ToString(), value, TmphSqlModel<TModelType>.MemberMap))
                        {
                            if (sqlTool.IsLockWrite) sqlTool.CallOnInsertedLock(value);
                            return true;
                        }
                    }
                    else
                    {
                        sqlStream.WriteNotNull("insert into[");
                        sqlStream.WriteNotNull(sqlTool.TableName);
                        sqlStream.WriteNotNull("](");
                        TmphSqlModel<TModelType>.TmphInsert.GetColumnNames(sqlStream, memberMap);
                        sqlStream.WriteNotNull(")values(");
                        TmphSqlModel<TModelType>.TmphInsert.Insert(sqlStream, memberMap, value, Converter);
                        if (TmphSqlModel<TModelType>.PrimaryKeys.Length != 0)
                        {
                            sqlStream.WriteNotNull(@")
if @@ROWCOUNT<>0 begin
 select top 1 ");
                            TmphSqlModel<TModelType>.GetNames(sqlStream, TmphSqlModel<TModelType>.MemberMap);
                            sqlStream.WriteNotNull(" from[");
                            sqlStream.WriteNotNull(sqlTool.TableName);
                            sqlStream.WriteNotNull("]with(nolock)where ");
                            TmphSqlModel<TModelType>.TmphPrimaryKeyWhere.Where(sqlStream, value, Converter);
                            sqlStream.WriteNotNull(@"
end");
                            if (set<TValueType, TModelType>(sqlStream.ToString(), value, TmphSqlModel<TModelType>.MemberMap))
                            {
                                if (sqlTool.IsLockWrite) sqlTool.CallOnInsertedLock(value);
                                return true;
                            }
                        }
                        else
                        {
                            sqlStream.Write(')');
                            if (ExecuteNonQuery(sqlStream.ToString(), null) > 0)
                            {
                                if (sqlTool.IsLockWrite) sqlTool.CallOnInsertedLock(value);
                                return true;
                            }
                        }
                    }
                }
            }
            finally
            {
                SqlBuffers.Push(ref TmphBuffer);
            }
            return false;
        }

        /// <summary>
        ///     更新数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="value">待更新数据</param>
        /// <param name="memberMap">目标成员位图</param>
        /// <returns>更新前的数据对象,null表示失败</returns>
        protected override unsafe TValueType update<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap memberMap)
        {
            using (var selectMemberMap = sqlTool.GetSelectMemberMap(memberMap))
            {
                var TmphBuffer = SqlBuffers.Get();
                try
                {
                    using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                    {
                        sqlStream.WriteNotNull("select top 1 ");
                        TmphSqlModel<TModelType>.TmphInsert.GetColumnNames(sqlStream, selectMemberMap);
                        sqlStream.WriteNotNull(" from[");
                        sqlStream.WriteNotNull(sqlTool.TableName);
                        sqlStream.WriteNotNull("]with(nolock)where ");
                        TmphSqlModel<TModelType>.TmphPrimaryKeyWhere.Where(sqlStream, value, Converter);
                        var sql = sqlStream.ToString();
                        sqlStream.WriteNotNull(@"
if @@ROWCOUNT<>0 begin
 update[");
                        sqlStream.WriteNotNull(sqlTool.TableName);
                        sqlStream.WriteNotNull("]set ");
                        TmphSqlModel<TModelType>.TmphUpdate.Update(sqlStream, memberMap, value, Converter);
                        sqlStream.WriteNotNull(" from[");
                        sqlStream.WriteNotNull(sqlTool.TableName);
                        sqlStream.WriteNotNull("]with(nolock)where ");
                        TmphSqlModel<TModelType>.TmphPrimaryKeyWhere.Where(sqlStream, value, Converter);
                        sqlStream.WriteNotNull(@"
 ");
                        sqlStream.WriteNotNull(sql);
                        sqlStream.WriteNotNull(@"
end");
                        var oldValue = TmphConstructor<TValueType>.New();
                        if (set<TValueType, TModelType>(sqlStream.ToString(), value, oldValue, selectMemberMap))
                        {
                            sqlTool.CallOnUpdatedLock(value, oldValue, memberMap);
                            return oldValue;
                        }
                    }
                }
                finally
                {
                    SqlBuffers.Push(ref TmphBuffer);
                }
            }
            return null;
        }

        /// <summary>
        ///     更新数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="value">待更新数据自增id标识</param>
        /// <param name="sqlExpression">待更新数据</param>
        /// <param name="memberMap">目标成员位图</param>
        /// <returns>更新前的数据对象,null表示失败</returns>
        protected override unsafe TValueType update<TValueType, TModelType>(
            TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool
            , TValueType value, TmphSqlTable.TmphUpdateExpression sqlExpression, TmphMemberMap memberMap)
        {
            var TmphBuffer = SqlBuffers.Get();
            try
            {
                using (var selectMemberMap = sqlTool.GetSelectMemberMap(memberMap))
                using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                {
                    sqlStream.WriteNotNull("select top 1 ");
                    TmphSqlModel<TModelType>.GetNames(sqlStream, selectMemberMap);
                    sqlStream.WriteNotNull(" from[");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("]with(nolock)where ");
                    TmphSqlModel<TModelType>.TmphPrimaryKeyWhere.Where(sqlStream, value, Converter);
                    var sql = sqlStream.ToString();
                    sqlStream.WriteNotNull(@"
if @@ROWCOUNT<>0 begin
 update[");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("]set ");
                    sqlExpression.Update(sqlStream);
                    sqlStream.WriteNotNull(" from[");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("]with(nolock)where ");
                    TmphSqlModel<TModelType>.TmphPrimaryKeyWhere.Where(sqlStream, value, Converter);
                    sqlStream.WriteNotNull(@"
 ");
                    sqlStream.WriteNotNull(sql);
                    sqlStream.WriteNotNull(@"
end");
                    var oldValue = TmphConstructor<TValueType>.New();
                    if (set<TValueType, TModelType>(sqlStream.ToString(), value, oldValue, selectMemberMap))
                    {
                        sqlTool.CallOnUpdatedLock(value, oldValue, memberMap);
                        return oldValue;
                    }
                }
            }
            finally
            {
                SqlBuffers.Push(ref TmphBuffer);
            }
            return null;
        }

        /// <summary>
        ///     更新数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="value">待更新数据</param>
        /// <param name="memberMap">目标成员位图</param>
        /// <returns>更新前的数据对象,null表示失败</returns>
        protected override unsafe TValueType updateByIdentity<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap memberMap)
        {
            using (var selectMemberMap = sqlTool.GetSelectMemberMap(memberMap))
            {
                var TmphBuffer = SqlBuffers.Get();
                try
                {
                    using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                    {
                        sqlStream.WriteNotNull("select top 1 ");
                        TmphSqlModel<TModelType>.TmphInsert.GetColumnNames(sqlStream, selectMemberMap);
                        sqlStream.WriteNotNull(" from[");
                        sqlStream.WriteNotNull(sqlTool.TableName);
                        sqlStream.WriteNotNull("]with(nolock)where ");
                        sqlStream.WriteNotNull(TmphSqlModel<TModelType>.Identity.Field.Name);
                        sqlStream.Write('=');
                        TmphNumber.ToString(TmphSqlModel<TModelType>.GetIdentity(value), sqlStream);
                        var sql = sqlStream.ToString();
                        sqlStream.WriteNotNull(@"if @@ROWCOUNT<>0 begin update[");
                        sqlStream.WriteNotNull(sqlTool.TableName);
                        sqlStream.WriteNotNull("]set ");
                        TmphSqlModel<TModelType>.TmphUpdate.Update(sqlStream, memberMap, value, Converter);
                        sqlStream.WriteNotNull(" from[");
                        sqlStream.WriteNotNull(sqlTool.TableName);
                        sqlStream.WriteNotNull("]with(nolock)where ");
                        sqlStream.WriteNotNull(TmphSqlModel<TModelType>.Identity.Field.Name);
                        sqlStream.Write('=');
                        TmphNumber.ToString(TmphSqlModel<TModelType>.GetIdentity(value), sqlStream);
                        sqlStream.WriteNotNull(@" ");
                        sqlStream.WriteNotNull(sql);
                        sqlStream.WriteNotNull(@"end");
                        var oldValue = TmphConstructor<TValueType>.New();
                        if (set<TValueType, TModelType>(sqlStream.ToString(), value, oldValue, selectMemberMap))
                        {
                            sqlTool.CallOnUpdatedLock(value, oldValue, memberMap);
                            return oldValue;
                        }
                    }
                }
                finally
                {
                    SqlBuffers.Push(ref TmphBuffer);
                }
            }
            return null;
        }

        /// <summary>
        ///     更新数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="value">待更新数据自增id标识</param>
        /// <param name="sqlExpression">待更新数据</param>
        /// <param name="memberMap">目标成员位图</param>
        /// <returns>更新前的数据对象,null表示失败</returns>
        protected override unsafe TValueType updateByIdentity<TValueType, TModelType>(
            TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool
            , TValueType value, TmphSqlTable.TmphUpdateExpression sqlExpression, TmphMemberMap memberMap)
        {
            var TmphBuffer = SqlBuffers.Get();
            try
            {
                using (var selectMemberMap = sqlTool.GetSelectMemberMap(memberMap))
                using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                {
                    sqlStream.WriteNotNull("select top 1 ");
                    TmphSqlModel<TModelType>.GetNames(sqlStream, selectMemberMap);
                    sqlStream.WriteNotNull(" from[");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("]with(nolock)where ");
                    sqlStream.WriteNotNull(TmphSqlModel<TModelType>.Identity.Field.Name);
                    sqlStream.Write('=');
                    TmphNumber.ToString(TmphSqlModel<TModelType>.GetIdentity(value), sqlStream);
                    var sql = sqlStream.ToString();
                    sqlStream.WriteNotNull(@"
if @@ROWCOUNT<>0 begin
 update[");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("]set ");
                    sqlExpression.Update(sqlStream);
                    sqlStream.WriteNotNull(" from[");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("]with(nolock)where ");
                    sqlStream.WriteNotNull(TmphSqlModel<TModelType>.Identity.Field.Name);
                    sqlStream.Write('=');
                    TmphNumber.ToString(TmphSqlModel<TModelType>.GetIdentity(value), sqlStream);
                    sqlStream.WriteNotNull(@"
 ");
                    sqlStream.WriteNotNull(sql);
                    sqlStream.WriteNotNull(@"
end");
                    var oldValue = TmphConstructor<TValueType>.New();
                    if (set<TValueType, TModelType>(sqlStream.ToString(), value, oldValue, selectMemberMap))
                    {
                        sqlTool.CallOnUpdatedLock(value, oldValue, memberMap);
                        return oldValue;
                    }
                }
            }
            finally
            {
                SqlBuffers.Push(ref TmphBuffer);
            }
            return null;
        }

        /// <summary>
        ///     删除数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="value">待删除数据</param>
        /// <returns>是否成功</returns>
        protected override unsafe bool delete<TValueType, TModelType>(TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool,
            TValueType value)
        {
            var TmphBuffer = SqlBuffers.Get();
            try
            {
                using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                {
                    sqlStream.WriteNotNull("select top 1 ");
                    TmphSqlModel<TModelType>.GetNames(sqlStream, sqlTool.SelectMemberMap);
                    sqlStream.WriteNotNull(" from[");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("]with(nolock)where ");
                    TmphSqlModel<TModelType>.TmphPrimaryKeyWhere.Where(sqlStream, value, Converter);
                    sqlStream.WriteNotNull(@"
if @@ROWCOUNT<>0 begin
 delete[");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("]from[");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("]with(nolock)where ");
                    TmphSqlModel<TModelType>.TmphPrimaryKeyWhere.Where(sqlStream, value, Converter);
                    sqlStream.WriteNotNull(@"
end");
                    if (set<TValueType, TModelType>(sqlStream.ToString(), value, sqlTool.SelectMemberMap))
                    {
                        sqlTool.CallOnDeletedLock(value);
                        return true;
                    }
                }
            }
            finally
            {
                SqlBuffers.Push(ref TmphBuffer);
            }
            return false;
        }

        /// <summary>
        ///     删除数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="value">待删除数据</param>
        /// <returns>是否成功</returns>
        protected override unsafe bool deleteByIdentity<TValueType, TModelType>(
            TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value)
        {
            var TmphBuffer = SqlBuffers.Get();
            try
            {
                using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                {
                    sqlStream.WriteNotNull("select top 1 ");
                    TmphSqlModel<TModelType>.GetNames(sqlStream, sqlTool.SelectMemberMap);
                    sqlStream.WriteNotNull(" from[");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("]with(nolock)where ");
                    sqlStream.WriteNotNull(TmphSqlModel<TModelType>.Identity.Field.Name);
                    sqlStream.Write('=');
                    TmphNumber.ToString(TmphSqlModel<TModelType>.GetIdentity(value), sqlStream);
                    sqlStream.WriteNotNull(@"
if @@ROWCOUNT<>0 begin
 delete[");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("]from[");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("]with(nolock)where ");
                    sqlStream.WriteNotNull(TmphSqlModel<TModelType>.Identity.Field.Name);
                    sqlStream.Write('=');
                    TmphNumber.ToString(TmphSqlModel<TModelType>.GetIdentity(value), sqlStream);
                    sqlStream.WriteNotNull(@"
end");
                    if (set<TValueType, TModelType>(sqlStream.ToString(), value, sqlTool.SelectMemberMap))
                    {
                        sqlTool.CallOnDeletedLock(value);
                        return true;
                    }
                }
            }
            finally
            {
                SqlBuffers.Push(ref TmphBuffer);
            }
            return false;
        }

        /// <summary>
        ///     委托关联表达式转SQL表达式
        /// </summary>
        /// <param name="expression">委托关联表达式</param>
        /// <param name="logicConstantWhere">逻辑常量值</param>
        /// <returns>SQL表达式(null表示常量条件)</returns>
        public override string GetWhere(LambdaExpression expression, ref bool logicConstantWhere)
        {
            if (expression != null)
            {
                var sqlExpression = TmphLambdaExpression.convert(expression);
                try
                {
                    if (!sqlExpression.IsLogicConstantExpression) return TmphConverter.Convert(sqlExpression).Value;
                    logicConstantWhere = sqlExpression.LogicConstantValue;
                }
                finally
                {
                    sqlExpression.PushPool();
                }
            }
            else logicConstantWhere = true;
            return null;
        }

        /// <summary>
        ///     委托关联表达式转SQL表达式
        /// </summary>
        /// <param name="expression">委托关联表达式</param>
        /// <param name="sqlStream">SQL表达式流</param>
        /// <param name="logicConstantWhere">逻辑常量值</param>
        /// <returns>参数成员名称</returns>
        internal override string GetWhere(LambdaExpression expression, TmphCharStream sqlStream,
            ref bool logicConstantWhere)
        {
            if (expression != null)
            {
                var sqlExpression = TmphLambdaExpression.convert(expression);
                try
                {
                    if (!sqlExpression.IsLogicConstantExpression) return TmphConverter.Convert(sqlExpression, sqlStream);
                    logicConstantWhere = sqlExpression.LogicConstantValue;
                }
                finally
                {
                    sqlExpression.PushPool();
                }
            }
            else logicConstantWhere = true;
            return null;
        }

        /// <summary>
        ///     委托关联表达式转SQL表达式
        /// </summary>
        /// <param name="expression">委托关联表达式</param>
        /// <returns>参数成员名称+SQL表达式</returns>
        public override TmphKeyValue<string, string> GetSql(LambdaExpression expression)
        {
            if (expression != null)
            {
                var sqlExpression = TmphLambdaExpression.convert(expression);
                try
                {
                    return TmphConverter.Convert(sqlExpression);
                }
                finally
                {
                    sqlExpression.PushPool();
                }
            }
            return default(TmphKeyValue<string, string>);
        }

        /// <summary>
        ///     委托关联表达式转SQL表达式
        /// </summary>
        /// <param name="expression">委托关联表达式</param>
        /// <param name="sqlStream">SQL表达式流</param>
        /// <returns>参数成员名称</returns>
        internal override string GetSql(LambdaExpression expression, TmphCharStream sqlStream)
        {
            if (expression != null)
            {
                var sqlExpression = TmphLambdaExpression.convert(expression);
                try
                {
                    return TmphConverter.Convert(sqlExpression, sqlStream);
                }
                finally
                {
                    sqlExpression.PushPool();
                }
            }
            return null;
        }

        /// <summary>
        ///     获取记录数
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="expression">查询表达式</param>
        /// <returns>记录数,失败返回-1</returns>
        public override unsafe int Count<TValueType, TModelType>(TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool,
            Expression<Func<TModelType, bool>> expression)
        {
            string sql = null;
            var TmphBuffer = SqlBuffers.Get();
            try
            {
                using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                {
                    sqlStream.WriteNotNull("select count(*)from[");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("]with(nolock)");
                    var isCreatedIndex = false;
                    if (TmphSelectQuery<TModelType>.WriteWhere(sqlTool, sqlStream, expression, ref isCreatedIndex))
                        sql = sqlStream.ToString();
                }
            }
            finally
            {
                SqlBuffers.Push(ref TmphBuffer);
            }
            return sql != null ? GetValue(sql, -1, null) : -1;
        }

        /// <summary>
        ///     查询单值数据
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <typeparam name="TReturnType">返回值类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="query">查询信息</param>
        /// <param name="memberName">成员名称</param>
        /// <param name="errorValue">错误值</param>
        /// <returns>对象集合</returns>
        internal override unsafe TReturnType getValue<TValueType, TModelType, TReturnType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TmphSelectQuery<TModelType> query, string memberName,
                TReturnType errorValue)
        {
            string sql = null;
            var TmphBuffer = SqlBuffers.Get();
            try
            {
                using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                {
                    sqlStream.WriteNotNull("select top 1 ");
                    sqlStream.WriteNotNull(memberName);
                    sqlStream.WriteNotNull(" from[");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("]with(nolock)");
                    if (query == null) sql = sqlStream.ToString();
                    else if (query.WriteWhere(sqlTool, sqlStream))
                    {
                        query.WriteOrder(sqlTool, sqlStream);
                        sql = sqlStream.ToString();
                    }
                }
            }
            finally
            {
                SqlBuffers.Push(ref TmphBuffer);
            }
            return sql != null ? GetValue(sql, errorValue, null) : errorValue;
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
            }
            return selectNoOrderPushMemberMap(sqlTool, query, memberMap);
        }

        /// <summary>
        ///     查询对象集合
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        /// <typeparam name="memberType">成员位图类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="query">查询信息</param>
        /// <param name="memberMap">成员位图</param>
        /// <returns>对象集合</returns>
        internal unsafe IEnumerable<TValueType> selectNoOrderPushMemberMap<TValueType, TModelType>
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
                    sqlStream.WriteNotNull("select ");
                    if (query != null)
                    {
                        var count = query.SkipCount + query.GetCount;
                        if (count != 0)
                        {
                            sqlStream.WriteNotNull("top ");
                            TmphNumber.ToString(count, sqlStream);
                            sqlStream.Write(' ');
                        }
                    }
                    TmphSqlModel<TModelType>.GetNames(sqlStream, memberMap);
                    sqlStream.WriteNotNull(" from [");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("]with(nolock)");
                    if (query == null) sql = sqlStream.ToString();
                    else if (query.WriteWhere(sqlTool, sqlStream))
                    {
                        query.WriteOrder(sqlTool, sqlStream);
                        sql = sqlStream.ToString();
                    }
                }
            }
            finally
            {
                SqlBuffers.Push(ref TmphBuffer);
            }
            return sql != null
                ? selectPushMemberMap<TValueType, TModelType>(sql, query == null ? 0 : query.SkipCount, memberMap)
                : TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     查询对象集合
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
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
                    sqlStream.WriteNotNull(" in(select top ");
                    TmphNumber.ToString(query.GetCount, sqlStream);
                    sqlStream.Write(' ');
                    sqlStream.WriteNotNull(keyName);
                    sqlStream.WriteNotNull(" from[");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("]with(nolock)where ");
                    sqlStream.Write('(');
                    var startIndex = sqlStream.Length;
                    if (query.WriteWhereOnly(sqlTool, sqlStream))
                    {
                        var count = sqlStream.Length - startIndex;
                        if (count == 0) sqlStream.Unsafer.AddLength(-1);
                        else sqlStream.Write(")and ");
                        sqlStream.WriteNotNull(keyName);
                        sqlStream.WriteNotNull(" not in(select top ");
                        TmphNumber.ToString(query.SkipCount, sqlStream);
                        sqlStream.Write(' ');
                        sqlStream.WriteNotNull(keyName);
                        sqlStream.WriteNotNull(" from[");
                        sqlStream.WriteNotNull(sqlTool.TableName);
                        sqlStream.WriteNotNull("]with(nolock)");
                        if (count != 0)
                        {
                            sqlStream.WriteNotNull("where ");
                            sqlStream.Write(sqlStream.Char + startIndex, count);
                        }

                        startIndex = sqlStream.Length;
                        query.WriteOrder(sqlTool, sqlStream);
                        count = sqlStream.Length - startIndex;
                        sqlStream.Write(')');
                        if (count != 0) sqlStream.Write(sqlStream.Char + startIndex, count);
                        sqlStream.Write(')');
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
        ///     查询对象
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="value">匹配成员值</param>
        /// <param name="memberMap">成员位图</param>
        /// <returns>对象集合</returns>
        public override unsafe TValueType GetByIdentity<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap memberMap)
        {
            string sql;
            using (var selectMemberMap = TmphSqlModel<TModelType>.CopyMemberMap)
            {
                if (memberMap != null && !memberMap.IsDefault) selectMemberMap.And(memberMap);
                var TmphBuffer = SqlBuffers.Get();
                try
                {
                    using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                    {
                        sqlStream.WriteNotNull("select top 1 ");
                        TmphSqlModel<TModelType>.GetNames(sqlStream, selectMemberMap);
                        sqlStream.WriteNotNull(" from[");
                        sqlStream.WriteNotNull(sqlTool.TableName);
                        sqlStream.WriteNotNull("]with(nolock)where ");
                        sqlStream.WriteNotNull(TmphSqlModel<TModelType>.Identity.Field.Name);
                        sqlStream.Write('=');
                        TmphNumber.ToString(TmphSqlModel<TModelType>.GetIdentity(value), sqlStream);
                        sql = sqlStream.ToString();
                    }
                }
                finally
                {
                    SqlBuffers.Push(ref TmphBuffer);
                }
                if (set<TValueType, TModelType>(sql, value, selectMemberMap)) return value;
            }
            return null;
        }

        /// <summary>
        ///     查询对象
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="value">匹配成员值</param>
        /// <param name="memberMap">成员位图</param>
        /// <returns>对象集合</returns>
        public override unsafe TValueType GetByPrimaryKey<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap memberMap)
        {
            string sql;
            using (var selectMemberMap = TmphSqlModel<TModelType>.CopyMemberMap)
            {
                if (memberMap != null && !memberMap.IsDefault) selectMemberMap.And(memberMap);
                var TmphBuffer = SqlBuffers.Get();
                try
                {
                    using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                    {
                        sqlStream.WriteNotNull("select top 1 ");
                        TmphSqlModel<TModelType>.GetNames(sqlStream, selectMemberMap);
                        sqlStream.WriteNotNull(" from[");
                        sqlStream.WriteNotNull(sqlTool.TableName);
                        sqlStream.WriteNotNull("]with(nolock)where ");
                        TmphSqlModel<TModelType>.TmphPrimaryKeyWhere.Where(sqlStream, value, Converter);
                        sql = sqlStream.ToString();
                    }
                }
                finally
                {
                    SqlBuffers.Push(ref TmphBuffer);
                }
                if (set<TValueType, TModelType>(sql, value, selectMemberMap)) return value;
            }
            return null;
        }
    }
}