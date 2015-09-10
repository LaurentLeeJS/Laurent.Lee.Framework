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
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq.Expressions;

namespace Laurent.Lee.CLB.Sql.Excel
{
    /// <summary>
    ///     Excel客户端(不做持久化设计,仅用于数据导入导出)
    /// </summary>
    public sealed class TmphClient : Sql.TmphClient
    {
        /// <summary>
        ///     表格名称
        /// </summary>
        private const string schemaTableName = "Table_Name";

        /// <summary>
        ///     Excel客户端
        /// </summary>
        /// <param name="connection">SQL连接信息</param>
        public TmphClient(Sql.TmphConnection connection) : base(connection)
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
        ///     是否支持删除表格
        /// </summary>
        internal override bool IsDropTable
        {
            get { return false; }
        }

        /// <summary>
        ///     是否支持索引
        /// </summary>
        internal override bool IsIndex
        {
            get { return false; }
        }

        /// <summary>
        ///     是否支持新增列
        /// </summary>
        internal override bool IsAddField
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
            return new OleDbConnection(Connection.Connection);
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
            DbCommand command = new OleDbCommand(sql, (OleDbConnection)connection);
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
            return new OleDbDataAdapter((OleDbCommand)command);
        }

        /// <summary>
        ///     获取数据表格
        /// </summary>
        /// <param name="tableName">表格名称</param>
        /// <param name="isName">是否处理表格名称</param>
        /// <returns>数据表格</returns>
        private DataTable getDataTable(string tableName, bool isName)
        {
            if (tableName != null && tableName.Length != 0)
            {
                using (var connection = getConnection(false))
                {
                    var table = GetDataTable(GetCommand(connection, "select * from [" + tableName + "]"));
                    if (table != null)
                    {
                        if (isName)
                            table.TableName = tableName[tableName.Length - 1] == '$'
                                ? tableName.Substring(0, tableName.Length - 1)
                                : tableName;
                        return table;
                    }
                }
            }
            return null;
        }

        /// <summary>
        ///     获取数据表格
        /// </summary>
        /// <param name="tableName">表格名称</param>
        /// <returns>数据表格</returns>
        public override DataTable GetDataTable(string tableName)
        {
            return getDataTable(tableName, true);
        }

        /// <summary>
        ///     导入数据集合
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="data">数据集合</param>
        /// <returns>成功导入数量</returns>
        private int import(DbConnection connection, DataTable data)
        {
            using (
                var adapter = new OleDbDataAdapter("select * from [" + data.TableName + "]",
                    (OleDbConnection)connection))
            using (var command = new OleDbCommandBuilder(adapter))
            using (var dataSet = new DataSet())
            {
                dataSet.Tables.Add(data);
                adapter.Update(dataSet, data.TableName);
                return data.Rows.Count;
            }
        }

        /// <summary>
        ///     导入数据集合
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="data">数据集合</param>
        /// <param name="batchSize">批处理数量,不支持</param>
        /// <param name="timeout">超时秒数,不支持</param>
        /// <returns>成功导入数量</returns>
        protected override int import(DbConnection connection, DataTable data, int batchSize, int timeout)
        {
            return import(connection, data);
        }

        /// <summary>
        ///     判断表格是否存在
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="tableName">表格名称</param>
        /// <returns>表格是否存在</returns>
        protected override bool isTable(DbConnection connection, string tableName)
        {
            using (var table = ((OleDbConnection)connection).GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null))
            {
                foreach (DataRow row in table.Rows)
                {
                    if (row[schemaTableName].ToString() == tableName) return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     创建表格
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="table">表格信息</param>
        internal override unsafe bool createTable(DbConnection connection, TmphTable table)
        {
            var name = table.Columns.Name;
            if (connection != null && name != null && name.Length != 0 && table.Columns != null &&
                table.Columns.Columns.Length != 0)
            {
                var TmphBuffer = SqlBuffers.Get();
                try
                {
                    using (var sqlStream = new TmphCharStream(TmphBuffer.Char, SqlBufferSize))
                    {
                        sqlStream.WriteNotNull("create table ");
                        sqlStream.WriteNotNull(name);
                        sqlStream.WriteNotNull(" (");
                        var isNext = false;
                        foreach (var column in table.Columns.Columns)
                        {
                            if (isNext) sqlStream.Write(',');
                            sqlStream.WriteNotNull(column.Name);
                            sqlStream.Write(' ');
                            sqlStream.Write(column.DbType.getSqlTypeName());
                            isNext = true;
                        }
                        sqlStream.Write(')');
                        return executeNonQuery(connection, sqlStream.ToString()) >= 0;
                    }
                }
                finally
                {
                    SqlBuffers.Push(ref TmphBuffer);
                }
            }
            return false;
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
            var size = sqlMember.MaxStringLength;
            TmphMemberType memberType = sqlMember.DataType != null ? sqlMember.DataType : type;
            if (memberType.IsString)
            {
                if (size > 0) sqlType = SqlDbType.NVarChar;
                else
                {
                    sqlType = SqlDbType.NText;
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
        ///     删除表格(不支持)
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="tableName">表格名称</param>
        protected override bool dropTable(DbConnection connection, string tableName)
        {
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
            return false;
        }

        /// <summary>
        ///     创建索引(不支持)
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="tableName">表格名称</param>
        /// <param name="columnCollection">索引列集合</param>
        internal override bool createIndex(DbConnection connection, string tableName,
            THColumnCollection columnCollection)
        {
            TmphLog.Error.Add("Excel 表格 " + tableName + " 不支持索引", false, false);
            return true;
        }

        /// <summary>
        ///     新增列集合(不支持)
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="columnCollection">新增列集合</param>
        internal override bool addFields(DbConnection connection, THColumnCollection columnCollection)
        {
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
            return false;
        }

        /// <summary>
        ///     获取表格名称集合
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <returns>表格名称集合</returns>
        protected override TmphSubArray<string> getTableNames(DbConnection connection)
        {
            using (var table = ((OleDbConnection)connection).GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null))
            {
                var rows = table.Rows;
                var names = new TmphSubArray<string>(rows.Count);
                foreach (DataRow row in rows) names.UnsafeAdd(row[schemaTableName].ToString());
                return names;
            }
        }

        /// <summary>
        ///     根据表格名称获取表格信息
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="name">表格名称</param>
        /// <returns>表格信息</returns>
        internal override TmphTable getTable(DbConnection connection, string tableName)
        {
            using (var command = GetCommand(connection, "select top 1 * from [" + tableName + "]"))
            using (var dataSet = GetDataSet(command))
            {
                var table = dataSet.Tables[0];
                TmphColumn identity = null;
                var columns = new TmphSubArray<TmphColumn>(table.Columns.Count);
                foreach (DataColumn dataColumn in table.Columns)
                {
                    var column = new TmphColumn
                    {
                        Name = dataColumn.ColumnName,
                        DbType = dataColumn.DataType.formCSharpType(),
                        Size = dataColumn.MaxLength,
                        DefaultValue = dataColumn.DefaultValue == null ? null : dataColumn.DefaultValue.ToString(),
                        IsNull = dataColumn.AllowDBNull
                    };
                    if (dataColumn.AutoIncrement) identity = column;
                    columns.UnsafeAdd(column);
                }
                return new TmphTable
                {
                    Columns = new THColumnCollection
                    {
                        Name = tableName,
                        Columns = columns.array,
                        Type = THColumnCollection.TmphType.None
                    },
                    Identity = identity
                };
            }
        }

        /// <summary>
        ///     插入数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="value">待插入数据</param>
        /// <param name="memberMap">目标成员位图</param>
        /// <returns>是否成功</returns>
        protected override bool insert<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap memberMap)
        {
            using (var connection = GetConnection())
            {
                if (connection != null)
                {
                    import(connection, sqlTool.GetDataTable(new[] { value }));
                    return true;
                }
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
        protected override TValueType update<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap memberMap)
        {
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
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
        protected override TValueType update<TValueType, TModelType>(TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool
            , TValueType value, TmphSqlTable.TmphUpdateExpression sqlExpression, TmphMemberMap memberMap)
        {
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
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
        protected override TValueType updateByIdentity<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap memberMap)
        {
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
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
        protected override TValueType updateByIdentity<TValueType, TModelType>(
            TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool
            , TValueType value, TmphSqlTable.TmphUpdateExpression sqlExpression, TmphMemberMap memberMap)
        {
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
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
        protected override bool delete<TValueType, TModelType>(TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool,
            TValueType value)
        {
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
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
        protected override bool deleteByIdentity<TValueType, TModelType>(
            TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value)
        {
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
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
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
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
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
            return null;
        }

        /// <summary>
        ///     委托关联表达式转SQL表达式
        /// </summary>
        /// <param name="expression">委托关联表达式</param>
        /// <returns>参数成员名称+SQL表达式</returns>
        public override TmphKeyValue<string, string> GetSql(LambdaExpression expression)
        {
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
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
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
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
        public override int Count<TValueType, TModelType>(TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool,
            Expression<Func<TModelType, bool>> expression)
        {
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
            return 0;
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
        internal override TReturnType getValue<TValueType, TModelType, TReturnType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TmphSelectQuery<TModelType> query, string memberName,
                TReturnType errorValue)
        {
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
            return errorValue;
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
            memberMap.Dispose();
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
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
        public override TValueType GetByIdentity<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap memberMap)
        {
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
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
        public override TValueType GetByPrimaryKey<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap memberMap)
        {
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
            return null;
        }
    }
}