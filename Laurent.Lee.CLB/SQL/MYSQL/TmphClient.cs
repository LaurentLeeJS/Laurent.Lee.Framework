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
using Laurent.Lee.CLB.Sql.Expression;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq.Expressions;

namespace Laurent.Lee.CLB.Sql.MySql
{
    /// <summary>
    ///     MySql客户端
    /// </summary>
    public sealed class TmphClient : Sql.TmphClient
    {
        /// <summary>
        ///     最大字符串长度(最大65532字节)
        /// </summary>
        private const int maxStringSize = 65535;

        //grant usage on *.* to xxx_user@127.0.0.1 identified by 'xxx_pwd' with grant option;
        //flush privileges;
        //create database xxx;
        //grant all privileges on xxx.* to xxx_user@127.0.0.1 identified by 'xxx_pwd';
        //flush privileges;
        /// <summary>
        ///     MySql客户端
        /// </summary>
        /// <param name="connection">SQL连接信息</param>
        public TmphClient(TmphConnection connection) : base(connection)
        {
        }

        /// <summary>
        ///     是否支持DataTable导入
        /// </summary>
        protected override bool isImport
        {
            get { return false; }
        }

        /// <summary>
        ///     根据SQL连接类型获取SQL连接
        /// </summary>
        /// <param name="connection">SQL连接信息</param>
        /// <param name="isAsynchronous">是否异步连接(不支持)</param>
        /// <returns>SQL连接</returns>
        protected override DbConnection getConnection(bool isAsynchronous)
        {
            return new MySqlConnection(Connection.Connection);
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
            DbCommand command = new MySqlCommand(sql, (MySqlConnection)connection);
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
            return new MySqlDataAdapter((MySqlCommand)command);
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
                    var table = GetDataTable(GetCommand(connection, "select * from `" + tableName + "`"));
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
        ///     导入数据集合(不支持)
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="data">数据集合</param>
        /// <param name="batchSize">批处理数量</param>
        /// <param name="timeout">超时秒数</param>
        /// <returns>成功导入数量</returns>
        protected override int import(DbConnection connection, DataTable data, int batchSize, int timeout)
        {
            TmphLog.Error.Add("mysql 不支持批量导入操作", false, true);
            return 0;
        }

        /// <summary>
        ///     判断表格是否存在
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="tableName">表格名称</param>
        /// <returns>表格是否存在</returns>
        protected override bool isTable(DbConnection connection, string tableName)
        {
            using (var command = GetCommand(connection, "show tables;"))
            using (var reader = command.ExecuteReader(CommandBehavior.Default))
            {
                while (reader.Read()) if (tableName.EqualCase((string)reader[0])) return true;
            }
            return false;
        }

        /// <summary>
        ///     写入列信息
        /// </summary>
        /// <param name="sqlStream">SQL语句流</param>
        /// <param name="TmphColumn">列信息</param>
        /// <param name="isIdentity">是否自增列</param>
        private static void appendColumn(TmphCharStream sqlStream, TmphColumn TmphColumn)
        {
            sqlStream.Write('`');
            sqlStream.WriteNotNull(TmphColumn.Name);
            sqlStream.WriteNotNull("` ");
            if (TmphColumn.DbType == SqlDbType.Text || TmphColumn.DbType == SqlDbType.NText)
            {
                if (TmphColumn.Size <= 65535) sqlStream.WriteNotNull("TEXT");
                else if (TmphColumn.Size <= 16777215) sqlStream.WriteNotNull("MEDIUMTEXT");
                else sqlStream.WriteNotNull("LONGTEXT");
                sqlStream.WriteNotNull(TmphColumn.DbType == SqlDbType.NText ? " UNICODE" : " ASCII");
            }
            else
            {
                sqlStream.WriteNotNull(TmphColumn.DbType.getSqlTypeName());
                if (TmphColumn.DbType.isStringType())
                {
                    if (TmphColumn.Size != int.MaxValue)
                    {
                        sqlStream.Write('(');
                        sqlStream.WriteNotNull(TmphColumn.Size.toString());
                        sqlStream.Write(')');
                    }
                    sqlStream.WriteNotNull(TmphColumn.DbType == SqlDbType.NChar || TmphColumn.DbType == SqlDbType.NVarChar
                        ? " UNICODE"
                        : " ASCII");
                }
            }
            if (TmphColumn.DefaultValue != null)
            {
                sqlStream.WriteNotNull(" default ");
                sqlStream.WriteNotNull(TmphColumn.DefaultValue);
            }
            if (!TmphColumn.IsNull) sqlStream.WriteNotNull(" not null");
            if (TmphColumn.Remark.Length() != 0)
            {
                sqlStream.WriteNotNull(" comment '");
                TmphConstantConverter.Default[typeof(string)](sqlStream, TmphColumn.Remark);
                sqlStream.Write('\'');
            }
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
                    sqlStream.WriteNotNull("create table`");
                    sqlStream.WriteNotNull(tableName);
                    sqlStream.WriteNotNull("`(");
                    var isNext = false;
                    foreach (var TmphColumn in table.Columns.Columns)
                    {
                        if (isNext) sqlStream.Write(',');
                        appendColumn(sqlStream, TmphColumn);
                        isNext = true;
                    }
                    var primaryKey = table.PrimaryKey;
                    if (primaryKey != null && primaryKey.Columns.length() != 0)
                    {
                        isNext = false;
                        sqlStream.WriteNotNull(",primary key(");
                        foreach (var TmphColumn in primaryKey.Columns)
                        {
                            if (isNext) sqlStream.Write(',');
                            sqlStream.WriteNotNull(TmphColumn.Name);
                            isNext = true;
                        }
                        sqlStream.Write(')');
                    }
                    if (table.Indexs != null)
                    {
                        foreach (var columns in table.Indexs)
                        {
                            if (columns != null && columns.Columns.length() != 0)
                            {
                                if (columns.Type == THColumnCollection.TmphType.UniqueIndex)
                                    sqlStream.WriteNotNull(@"unique index ");
                                else sqlStream.WriteNotNull(@"
index ");
                                AppendIndexName(sqlStream, tableName, columns);
                                sqlStream.Write('(');
                                isNext = false;
                                foreach (var TmphColumn in columns.Columns)
                                {
                                    if (isNext) sqlStream.Write(',');
                                    sqlStream.Write('`');
                                    sqlStream.WriteNotNull(TmphColumn.Name);
                                    sqlStream.Write('`');
                                    isNext = true;
                                }
                                sqlStream.Write(')');
                            }
                        }
                    }
                    sqlStream.WriteNotNull(");");
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
                    if (size <= 0) size = int.MaxValue;
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
            return executeNonQuery(connection, "drop table `" + tableName + "`;") != ExecuteNonQueryError;
        }

        /// <summary>
        ///     创建索引
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="tableName">表格名称</param>
        /// <param name="THColumnCollection">索引列集合</param>
        internal override unsafe bool createIndex(DbConnection connection, string tableName,
            THColumnCollection THColumnCollection)
        {
            string sql;
            var TmphBuffer = SqlBuffers.Get();
            try
            {
                using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                {
                    sqlStream.WriteNotNull(@"create index`");
                    AppendIndexName(sqlStream, tableName, THColumnCollection);
                    sqlStream.WriteNotNull("`on`");
                    sqlStream.WriteNotNull(tableName);
                    sqlStream.WriteNotNull("`(");
                    var isNext = false;
                    foreach (var TmphColumn in THColumnCollection.Columns)
                    {
                        if (isNext) sqlStream.Write(',');
                        sqlStream.Write('`');
                        sqlStream.WriteNotNull(TmphColumn.Name);
                        sqlStream.Write('`');
                        isNext = true;
                    }
                    sqlStream.WriteNotNull(");");
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
        /// <param name="THColumnCollection">新增列集合</param>
        internal override unsafe bool addFields(DbConnection connection, THColumnCollection THColumnCollection)
        {
            string tableName = THColumnCollection.Name, sql;
            var TmphBuffer = SqlBuffers.Get();
            try
            {
                using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                {
                    var isUpdateValue = false;
                    foreach (var TmphColumn in THColumnCollection.Columns)
                    {
                        sqlStream.WriteNotNull(@"
alter table `");
                        sqlStream.WriteNotNull(tableName);
                        sqlStream.WriteNotNull(@"` add ");
                        if (!TmphColumn.IsNull && TmphColumn.DefaultValue == null)
                        {
                            TmphColumn.DefaultValue = TmphColumn.DbType.getDefaultValue();
                            if (TmphColumn.DefaultValue == null) TmphColumn.IsNull = true;
                        }
                        appendColumn(sqlStream, TmphColumn);
                        sqlStream.Write(';');
                        if (TmphColumn.UpdateValue != null) isUpdateValue = true;
                    }
                    if (isUpdateValue)
                    {
                        sqlStream.WriteNotNull(@"
update `");
                        sqlStream.WriteNotNull(tableName);
                        sqlStream.WriteNotNull("` set ");
                        foreach (var TmphColumn in THColumnCollection.Columns)
                        {
                            if (TmphColumn.UpdateValue != null)
                            {
                                if (!isUpdateValue) sqlStream.Write(',');
                                sqlStream.WriteNotNull(TmphColumn.Name);
                                sqlStream.Write('=');
                                sqlStream.WriteNotNull(TmphColumn.UpdateValue);
                                isUpdateValue = false;
                            }
                        }
                        sqlStream.Write(';');
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
            using (var command = GetCommand(connection, "show tables;"))
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
            if (isTable(connection, tableName))
            {
                using (var command = GetCommand(connection, @"describe `" + tableName + @"`;
show index from `" + tableName + @"`;"))
                using (var reader = command.ExecuteReader(CommandBehavior.Default))
                {
                    TmphColumn identity = null;
                    var columns = TmphDictionary.CreateHashString<TmphColumn>();
                    var primaryKeys = default(TmphSubArray<TmphColumn>);
                    Dictionary<TmphHashString, TmphList<indexColumn>> indexs = null;
                    while (reader.Read())
                    {
                        var key = (string)reader["Key"];
                        var defaultValue = reader["Default"];
                        var TmphColumn = new TmphColumn
                        {
                            Name = (string)reader["Field"],
                            DefaultValue = defaultValue == DBNull.Value ? null : (string)defaultValue,
                            IsNull = (string)reader["Null"] == "YES"
                        };
                        TmphColumn.DbType = TmphSqlDbType.FormatDbType((string)reader["Type"], out TmphColumn.Size);
                        columns.Add(TmphColumn.Name, TmphColumn);
                        if (key == "PRI") primaryKeys.Add(TmphColumn);
                    }
                    if (reader.NextResult())
                    {
                        indexs = TmphDictionary.CreateHashString<TmphList<indexColumn>>();
                        TmphList<indexColumn> indexColumns;
                        while (reader.Read())
                        {
                            var name = (string)reader["Key_name"];
                            var indexColumn = new indexColumn
                            {
                                Column = columns[(string)reader["Column_name"]],
                                Index = (int)(long)reader["Seq_in_index"],
                                IsNull = (string)reader["Null"] == "YES"
                            };
                            TmphHashString nameKey = name;
                            if (!indexs.TryGetValue(nameKey, out indexColumns))
                            {
                                indexs.Add(nameKey, indexColumns = new TmphList<indexColumn>());
                                indexColumns.Add(indexColumn);
                                indexColumn.Type = (long)reader["Non_unique"] == 0
                                    ? THColumnCollection.TmphType.UniqueIndex
                                    : THColumnCollection.TmphType.Index;
                            }
                            else indexColumns.Add(indexColumn);
                        }
                    }
                    return new TmphTable
                    {
                        Columns = new THColumnCollection
                        {
                            Name = tableName,
                            Columns = columns.Values.GetArray(),
                            Type = THColumnCollection.TmphType.None
                        },
                        Identity = identity,
                        PrimaryKey =
                            primaryKeys.Count == 0
                                ? null
                                : new THColumnCollection
                                {
                                    Type = THColumnCollection.TmphType.PrimaryKey,
                                    Columns = primaryKeys.ToArray()
                                },
                        Indexs = indexs.GetArray(index => new THColumnCollection
                        {
                            Name = index.Key.ToString(),
                            Type = index.Value[0].Type,
                            Columns =
                                index.Value.sort((left, right) => left.Index - right.Index)
                                    .getArray(TmphColumn => TmphColumn.Column)
                        })
                    };
                }
            }
            return null;
        }

        /// <summary>
        ///     插入数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="value">待插入数据</param>
        /// <param name="TmphMemberMap">目标成员位图</param>
        /// <returns>是否成功</returns>
        protected override unsafe bool insert<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap TmphMemberMap)
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
                        sqlStream.WriteNotNull("insert into`");
                        sqlStream.WriteNotNull(sqlTool.TableName);
                        sqlStream.WriteNotNull("`(");
                        TmphSqlModel<TModelType>.TmphInsert.GetColumnNames(sqlStream, TmphMemberMap);
                        sqlStream.WriteNotNull(")values(");
                        TmphSqlModel<TModelType>.TmphInsert.Insert(sqlStream, TmphMemberMap, value, Converter);
                        sqlStream.WriteNotNull(");");
                        using (var connection = GetConnection())
                        {
                            if (connection != null &&
                                executeNonQuery(connection, sqlStream.ToString()) != ExecuteNonQueryError)
                            {
                                sqlStream.Clear();
                                sqlStream.WriteNotNull("select ");
                                TmphSqlModel<TModelType>.GetNames(sqlStream, TmphSqlModel<TModelType>.MemberMap);
                                sqlStream.WriteNotNull(" from `");
                                sqlStream.WriteNotNull(sqlTool.TableName);
                                sqlStream.WriteNotNull("` where ");
                                sqlStream.WriteNotNull(TmphSqlModel<TModelType>.Identity.Field.Name);
                                sqlStream.Write('=');
                                TmphNumber.ToString(identity, sqlStream);
                                sqlStream.WriteNotNull(" limit 0,1;");
                                if (set<TValueType, TModelType>(connection, sqlStream.ToString(), value,
                                    TmphSqlModel<TModelType>.MemberMap))
                                {
                                    if (sqlTool.IsLockWrite) sqlTool.CallOnInsertedLock(value);
                                    return true;
                                }
                            }
                        }
                    }
                    else
                    {
                        sqlStream.WriteNotNull("insert into`");
                        sqlStream.WriteNotNull(sqlTool.TableName);
                        sqlStream.WriteNotNull("`(");
                        TmphSqlModel<TModelType>.TmphInsert.GetColumnNames(sqlStream, TmphMemberMap);
                        sqlStream.WriteNotNull(")values(");
                        TmphSqlModel<TModelType>.TmphInsert.Insert(sqlStream, TmphMemberMap, value, Converter);
                        sqlStream.WriteNotNull(");");
                        if (TmphSqlModel<TModelType>.PrimaryKeys.Length != 0)
                        {
                            using (var connection = GetConnection())
                            {
                                if (connection != null &&
                                    executeNonQuery(connection, sqlStream.ToString()) != ExecuteNonQueryError)
                                {
                                    sqlStream.Clear();
                                    sqlStream.WriteNotNull("select ");
                                    TmphSqlModel<TModelType>.GetNames(sqlStream, TmphSqlModel<TModelType>.MemberMap);
                                    sqlStream.WriteNotNull(" from `");
                                    sqlStream.WriteNotNull(sqlTool.TableName);
                                    sqlStream.WriteNotNull("` where ");
                                    TmphSqlModel<TModelType>.TmphPrimaryKeyWhere.Where(sqlStream, value, Converter);
                                    sqlStream.WriteNotNull(" limit 0,1;");
                                    if (set<TValueType, TModelType>(connection, sqlStream.ToString(), value,
                                        TmphSqlModel<TModelType>.MemberMap))
                                    {
                                        if (sqlTool.IsLockWrite) sqlTool.CallOnInsertedLock(value);
                                        return true;
                                    }
                                }
                            }
                        }
                        else if (ExecuteNonQuery(sqlStream.ToString(), null) > 0)
                        {
                            if (sqlTool.IsLockWrite) sqlTool.CallOnInsertedLock(value);
                            return true;
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
        /// <param name="TmphMemberMap">目标成员位图</param>
        /// <returns>更新前的数据对象,null表示失败</returns>
        protected override unsafe TValueType update<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap TmphMemberMap)
        {
            using (var selectMemberMap = sqlTool.GetSelectMemberMap(TmphMemberMap))
            {
                var TmphBuffer = SqlBuffers.Get();
                try
                {
                    using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                    {
                        sqlStream.WriteNotNull("select ");
                        TmphSqlModel<TModelType>.TmphInsert.GetColumnNames(sqlStream, selectMemberMap);
                        sqlStream.WriteNotNull(" from `");
                        sqlStream.WriteNotNull(sqlTool.TableName);
                        sqlStream.WriteNotNull("` where ");
                        TmphSqlModel<TModelType>.TmphPrimaryKeyWhere.Where(sqlStream, value, Converter);
                        sqlStream.WriteNotNull(" limit 0,1;");
                        var sql = sqlStream.ToString();
                        using (var connection = GetConnection())
                        {
                            var oldValue = TmphConstructor<TValueType>.New();
                            if (connection != null &&
                                set<TValueType, TModelType>(connection, sql, oldValue, selectMemberMap))
                            {
                                sqlStream.Clear();
                                sqlStream.WriteNotNull(" update `");
                                sqlStream.WriteNotNull(sqlTool.TableName);
                                sqlStream.WriteNotNull("` set ");
                                TmphSqlModel<TModelType>.TmphUpdate.Update(sqlStream, TmphMemberMap, value, Converter);
                                sqlStream.WriteNotNull(" where ");
                                TmphSqlModel<TModelType>.TmphPrimaryKeyWhere.Where(sqlStream, value, Converter);
                                sqlStream.Write(';');
                                if (executeNonQuery(connection, sqlStream.ToString()) != ExecuteNonQueryError &&
                                    set<TValueType, TModelType>(connection, sql, value, selectMemberMap))
                                {
                                    sqlTool.CallOnUpdatedLock(value, oldValue, TmphMemberMap);
                                    return oldValue;
                                }
                            }
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
        /// <param name="TmphMemberMap">目标成员位图</param>
        /// <returns>更新前的数据对象,null表示失败</returns>
        protected override unsafe TValueType update<TValueType, TModelType>(
            TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool
            , TValueType value, TmphSqlTable.TmphUpdateExpression sqlExpression, TmphMemberMap TmphMemberMap)
        {
            var TmphBuffer = SqlBuffers.Get();
            try
            {
                using (var selectMemberMap = sqlTool.GetSelectMemberMap(TmphMemberMap))
                using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                {
                    sqlStream.WriteNotNull("select ");
                    TmphSqlModel<TModelType>.GetNames(sqlStream, selectMemberMap);
                    sqlStream.WriteNotNull(" from `");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("` where ");
                    TmphSqlModel<TModelType>.TmphPrimaryKeyWhere.Where(sqlStream, value, Converter);
                    sqlStream.WriteNotNull(" limit 0,1;");
                    using (var connection = GetConnection())
                    {
                        var sql = sqlStream.ToString();
                        var oldValue = TmphConstructor<TValueType>.New();
                        if (connection != null && set<TValueType, TModelType>(connection, sql, oldValue, selectMemberMap))
                        {
                            sqlStream.Clear();
                            sqlStream.WriteNotNull(" update `");
                            sqlStream.WriteNotNull(sqlTool.TableName);
                            sqlStream.WriteNotNull("` set ");
                            sqlExpression.Update(sqlStream);
                            sqlStream.WriteNotNull(" where ");
                            TmphSqlModel<TModelType>.TmphPrimaryKeyWhere.Where(sqlStream, value, Converter);
                            sqlStream.Write(';');
                            if (executeNonQuery(connection, sqlStream.ToString()) != ExecuteNonQueryError)
                            {
                                if (set<TValueType, TModelType>(connection, sql, value, selectMemberMap))
                                {
                                    sqlTool.CallOnUpdatedLock(value, oldValue, TmphMemberMap);
                                    return oldValue;
                                }
                            }
                        }
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
        /// <param name="TmphMemberMap">目标成员位图</param>
        /// <returns>更新前的数据对象,null表示失败</returns>
        protected override unsafe TValueType updateByIdentity<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap TmphMemberMap)
        {
            using (var selectMemberMap = sqlTool.GetSelectMemberMap(TmphMemberMap))
            {
                var TmphBuffer = SqlBuffers.Get();
                try
                {
                    using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                    {
                        sqlStream.WriteNotNull("select ");
                        TmphSqlModel<TModelType>.TmphInsert.GetColumnNames(sqlStream, selectMemberMap);
                        sqlStream.WriteNotNull(" from `");
                        sqlStream.WriteNotNull(sqlTool.TableName);
                        sqlStream.WriteNotNull("` where ");
                        sqlStream.WriteNotNull(TmphSqlModel<TModelType>.Identity.Field.Name);
                        sqlStream.Write('=');
                        TmphNumber.ToString(TmphSqlModel<TModelType>.GetIdentity(value), sqlStream);
                        sqlStream.WriteNotNull(" limit 0,1;");
                        var sql = sqlStream.ToString();
                        using (var connection = GetConnection())
                        {
                            var oldValue = TmphConstructor<TValueType>.New();
                            if (connection != null &&
                                set<TValueType, TModelType>(connection, sql, oldValue, selectMemberMap))
                            {
                                sqlStream.Clear();
                                sqlStream.WriteNotNull(" update `");
                                sqlStream.WriteNotNull(sqlTool.TableName);
                                sqlStream.WriteNotNull("` set ");
                                TmphSqlModel<TModelType>.TmphUpdate.Update(sqlStream, TmphMemberMap, value, Converter);
                                sqlStream.WriteNotNull(" where ");
                                sqlStream.WriteNotNull(TmphSqlModel<TModelType>.Identity.Field.Name);
                                sqlStream.Write('=');
                                TmphNumber.ToString(TmphSqlModel<TModelType>.GetIdentity(value), sqlStream);
                                sqlStream.Write(';');
                                if (executeNonQuery(connection, sqlStream.ToString()) != ExecuteNonQueryError &&
                                    set<TValueType, TModelType>(connection, sql, value, selectMemberMap))
                                {
                                    sqlTool.CallOnUpdatedLock(value, oldValue, TmphMemberMap);
                                    return oldValue;
                                }
                            }
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
        /// <param name="TmphMemberMap">目标成员位图</param>
        /// <returns>更新前的数据对象,null表示失败</returns>
        protected override unsafe TValueType updateByIdentity<TValueType, TModelType>(
            TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool
            , TValueType value, TmphSqlTable.TmphUpdateExpression sqlExpression, TmphMemberMap TmphMemberMap)
        {
            var TmphBuffer = SqlBuffers.Get();
            try
            {
                using (var selectMemberMap = sqlTool.GetSelectMemberMap(TmphMemberMap))
                using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                {
                    sqlStream.WriteNotNull("select ");
                    TmphSqlModel<TModelType>.GetNames(sqlStream, selectMemberMap);
                    sqlStream.WriteNotNull(" from `");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("` where ");
                    sqlStream.WriteNotNull(TmphSqlModel<TModelType>.Identity.Field.Name);
                    sqlStream.Write('=');
                    TmphNumber.ToString(TmphSqlModel<TModelType>.GetIdentity(value), sqlStream);
                    sqlStream.WriteNotNull(" limit 0,1;");
                    using (var connection = GetConnection())
                    {
                        var sql = sqlStream.ToString();
                        var oldValue = TmphConstructor<TValueType>.New();
                        if (connection != null && set<TValueType, TModelType>(connection, sql, oldValue, selectMemberMap))
                        {
                            sqlStream.Clear();
                            sqlStream.WriteNotNull(" update `");
                            sqlStream.WriteNotNull(sqlTool.TableName);
                            sqlStream.WriteNotNull("` set ");
                            sqlExpression.Update(sqlStream);
                            sqlStream.WriteNotNull(" where ");
                            sqlStream.WriteNotNull(TmphSqlModel<TModelType>.Identity.Field.Name);
                            sqlStream.Write('=');
                            TmphNumber.ToString(TmphSqlModel<TModelType>.GetIdentity(value), sqlStream);
                            sqlStream.Write(';');
                            if (executeNonQuery(connection, sqlStream.ToString()) != ExecuteNonQueryError)
                            {
                                if (set<TValueType, TModelType>(connection, sql, value, selectMemberMap))
                                {
                                    sqlTool.CallOnUpdatedLock(value, oldValue, TmphMemberMap);
                                    return oldValue;
                                }
                            }
                        }
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
                    sqlStream.WriteNotNull("select ");
                    TmphSqlModel<TModelType>.GetNames(sqlStream, sqlTool.SelectMemberMap);
                    sqlStream.WriteNotNull(" from '");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("' where ");
                    TmphSqlModel<TModelType>.TmphPrimaryKeyWhere.Where(sqlStream, value, Converter);
                    sqlStream.WriteNotNull(" limit 0,1;");
                    using (var connection = GetConnection())
                    {
                        if (connection != null &&
                            set<TValueType, TModelType>(connection, sqlStream.ToString(), value, sqlTool.SelectMemberMap))
                        {
                            sqlStream.Clear();
                            sqlStream.WriteNotNull("delete from `");
                            sqlStream.WriteNotNull(sqlTool.TableName);
                            sqlStream.WriteNotNull("' where ");
                            TmphSqlModel<TModelType>.TmphPrimaryKeyWhere.Where(sqlStream, value, Converter);
                            sqlStream.Write(';');
                            if (executeNonQuery(connection, sqlStream.ToString()) != ExecuteNonQueryError)
                            {
                                sqlTool.CallOnDeletedLock(value);
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
                    sqlStream.WriteNotNull("select ");
                    TmphSqlModel<TModelType>.GetNames(sqlStream, sqlTool.SelectMemberMap);
                    sqlStream.WriteNotNull(" from '");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("' where ");
                    sqlStream.WriteNotNull(TmphSqlModel<TModelType>.Identity.Field.Name);
                    sqlStream.Write('=');
                    TmphNumber.ToString(TmphSqlModel<TModelType>.GetIdentity(value), sqlStream);
                    sqlStream.WriteNotNull(" limit 0,1;");
                    using (var connection = GetConnection())
                    {
                        if (connection != null &&
                            set<TValueType, TModelType>(connection, sqlStream.ToString(), value, sqlTool.SelectMemberMap))
                        {
                            sqlStream.Clear();
                            sqlStream.WriteNotNull("delete from `");
                            sqlStream.WriteNotNull(sqlTool.TableName);
                            sqlStream.WriteNotNull("' where ");
                            sqlStream.WriteNotNull(TmphSqlModel<TModelType>.Identity.Field.Name);
                            sqlStream.Write('=');
                            TmphNumber.ToString(TmphSqlModel<TModelType>.GetIdentity(value), sqlStream);
                            sqlStream.Write(';');
                            if (executeNonQuery(connection, sqlStream.ToString()) != ExecuteNonQueryError)
                            {
                                sqlTool.CallOnDeletedLock(value);
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
                    if (!sqlExpression.IsLogicConstantExpression)
                        return MsSql.TmphConverter.Convert(sqlExpression).Value ?? string.Empty;
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
                TmphLambdaExpression sqlExpression = Expression.TmphLambdaExpression.convert(expression);
                try
                {
                    if (!sqlExpression.IsLogicConstantExpression)
                        return MsSql.TmphConverter.Convert(sqlExpression, sqlStream);
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
                TmphLambdaExpression sqlExpression = Expression.TmphLambdaExpression.convert(expression);
                try
                {
                    return MsSql.TmphConverter.Convert(sqlExpression);
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
                    return MsSql.TmphConverter.Convert(sqlExpression, sqlStream);
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
                    sqlStream.WriteNotNull("select count(*)from`");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("`");
                    var isCreatedIndex = false;
                    if (TmphSelectQuery<TModelType>.WriteWhere(sqlTool, sqlStream, expression, ref isCreatedIndex))
                    {
                        sqlStream.Write(';');
                        sql = sqlStream.ToString();
                    }
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
                    sqlStream.WriteNotNull("select ");
                    sqlStream.WriteNotNull(memberName);
                    sqlStream.WriteNotNull(" from`");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.WriteNotNull("` ");
                    if (query == null) sql = sqlStream.ToString();
                    else if (query.WriteWhere(sqlTool, sqlStream))
                    {
                        query.WriteOrder(sqlTool, sqlStream);
                        sqlStream.WriteNotNull(" limit ");
                        TmphNumber.ToString(query.SkipCount, sqlStream);
                        sqlStream.WriteNotNull(",1;");
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
        /// <param name="TmphMemberMap">成员位图</param>
        /// <returns>对象集合</returns>
        internal override unsafe IEnumerable<TValueType> selectPushMemberMap<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TmphSelectQuery<TModelType> query, TmphMemberMap TmphMemberMap)
        {
            string sql = null;
            var TmphBuffer = SqlBuffers.Get();
            try
            {
                using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                {
                    sqlStream.WriteNotNull("select ");
                    TmphSqlModel<TModelType>.GetNames(sqlStream, TmphMemberMap);
                    sqlStream.WriteNotNull(" from `");
                    sqlStream.WriteNotNull(sqlTool.TableName);
                    sqlStream.Write('`');
                    if (query == null)
                    {
                        sqlStream.Write(';');
                        sql = sqlStream.ToString();
                    }
                    else
                    {
                        sqlStream.Write(' ');
                        if (query.WriteWhere(sqlTool, sqlStream))
                        {
                            query.WriteOrder(sqlTool, sqlStream);
                            if ((query.GetCount | query.SkipCount) != 0)
                            {
                                sqlStream.WriteNotNull(" limit ");
                                TmphNumber.ToString(query.SkipCount, sqlStream);
                                sqlStream.Write(',');
                                TmphNumber.ToString(query.GetCount, sqlStream);
                            }
                            sqlStream.Write(';');
                            sql = sqlStream.ToString();
                        }
                    }
                }
            }
            finally
            {
                SqlBuffers.Push(ref TmphBuffer);
            }
            return sql != null
                ? selectPushMemberMap<TValueType, TModelType>(sql, 0, TmphMemberMap)
                : TmphNullValue<TValueType>.Array;
        }

        /// <summary>
        ///     查询对象
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="value">匹配成员值</param>
        /// <param name="TmphMemberMap">成员位图</param>
        /// <returns>对象集合</returns>
        public override unsafe TValueType GetByIdentity<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap TmphMemberMap)
        {
            string sql;
            using (var selectMemberMap = TmphSqlModel<TModelType>.CopyMemberMap)
            {
                if (TmphMemberMap != null && !TmphMemberMap.IsDefault) selectMemberMap.And(TmphMemberMap);
                var TmphBuffer = SqlBuffers.Get();
                try
                {
                    using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                    {
                        sqlStream.WriteNotNull("select ");
                        TmphSqlModel<TModelType>.GetNames(sqlStream, selectMemberMap);
                        sqlStream.WriteNotNull(" from `");
                        sqlStream.WriteNotNull(sqlTool.TableName);
                        sqlStream.WriteNotNull("` where ");
                        sqlStream.WriteNotNull(TmphSqlModel<TModelType>.Identity.Field.Name);
                        sqlStream.Write('=');
                        TmphNumber.ToString(TmphSqlModel<TModelType>.GetIdentity(value), sqlStream);
                        sqlStream.WriteNotNull(" limit 0,1;");
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
        /// <param name="TmphMemberMap">成员位图</param>
        /// <returns>对象集合</returns>
        public override unsafe TValueType GetByPrimaryKey<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap TmphMemberMap)
        {
            string sql;
            using (var selectMemberMap = TmphSqlModel<TModelType>.CopyMemberMap)
            {
                if (TmphMemberMap != null && !TmphMemberMap.IsDefault) selectMemberMap.And(TmphMemberMap);
                var TmphBuffer = SqlBuffers.Get();
                try
                {
                    using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                    {
                        sqlStream.WriteNotNull("select ");
                        TmphSqlModel<TModelType>.GetNames(sqlStream, selectMemberMap);
                        sqlStream.WriteNotNull(" from `");
                        sqlStream.WriteNotNull(sqlTool.TableName);
                        sqlStream.WriteNotNull("` where ");
                        TmphSqlModel<TModelType>.TmphPrimaryKeyWhere.Where(sqlStream, value, Converter);
                        sqlStream.WriteNotNull(" limit 0,1;");
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
        ///     索引列
        /// </summary>
        private sealed class indexColumn
        {
            /// <summary>
            ///     数据列
            /// </summary>
            public TmphColumn Column;

            /// <summary>
            ///     列序号
            /// </summary>
            public int Index;

            /// <summary>
            ///     是否可空
            /// </summary>
            public bool IsNull;

            /// <summary>
            ///     是否不允许重复
            /// </summary>
            public THColumnCollection.TmphType Type;
        }
    }
}