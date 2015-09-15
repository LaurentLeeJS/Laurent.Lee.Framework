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
using Laurent.Lee.CLB.Emit;
using Laurent.Lee.CLB.Sql.Expression;
using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Threading;
using TmphSqlModel = Laurent.Lee.CLB.Code.CSharp.TmphSqlModel;

namespace Laurent.Lee.CLB.Sql
{
    /// <summary>
    ///     SQL客户端操作
    /// </summary>
    public abstract class TmphClient
    {
        /// <summary>
        ///     执行错误返回值
        /// </summary>
        public const int ExecuteNonQueryError = int.MinValue;

        /// <summary>
        ///     SQL字符串缓冲区大小
        /// </summary>
        internal const int SqlBufferSize = 1 << 10;

        /// <summary>
        ///     SQL字符串缓冲区
        /// </summary>
        internal static readonly TmphUnmanagedPool SqlBuffers = TmphUnmanagedPool.GetPool(SqlBufferSize << 1);

        /// <summary>
        ///     对象转换成SQL字符流
        /// </summary>
        private static readonly TmphCharStream toStringStream = new TmphCharStream(TmphUnmanagedStreamBase.DefaultLength);

        /// <summary>
        ///     对象转换成SQL字符流访问锁
        /// </summary>
        private static int toStringLock;

        /// <summary>
        ///     SQL连接信息
        /// </summary>
        internal TmphConnection Connection;

        /// <summary>
        ///     SQL常量转换处理
        /// </summary>
        internal TmphConstantConverter Converter;

        /// <summary>
        ///     SQL客户端操作
        /// </summary>
        /// <param name="connection">SQL连接信息</param>
        protected TmphClient(TmphConnection connection)
        {
            Connection = connection;
            Converter = TmphEnum<TmphType, TmphTypeInfo>.Array(Connection.Type).Converter;
        }

        /// <summary>
        ///     是否支持DataTable导入
        /// </summary>
        protected virtual bool isImport
        {
            get { return false; }
        }

        /// <summary>
        ///     是否支持删除表格
        /// </summary>
        internal virtual bool IsDropTable
        {
            get { return true; }
        }

        /// <summary>
        ///     是否支持索引
        /// </summary>
        internal virtual bool IsIndex
        {
            get { return true; }
        }

        /// <summary>
        ///     是否支持新增列
        /// </summary>
        internal virtual bool IsAddField
        {
            get { return true; }
        }

        /// <summary>
        ///     根据SQL连接类型获取SQL连接
        /// </summary>
        /// <param name="isAsynchronous">是否异步连接</param>
        /// <returns>SQL连接</returns>
        protected abstract DbConnection getConnection(bool isAsynchronous);

        /// <summary>
        ///     根据SQL连接类型获取SQL连接
        /// </summary>
        /// <param name="isAsynchronous">是否异步连接</param>
        /// <returns>SQL连接</returns>
        public DbConnection GetConnection(bool isAsynchronous = false)
        {
            Exception openError = null;
            var connection = getConnection(isAsynchronous);
            try
            {
                connection.Open();
                return connection;
            }
            catch (Exception error)
            {
                connection.Dispose();
                openError = error;
            }
            TmphLog.Error.Add(openError, null, true);
            return null;
        }

        /// <summary>
        ///     获取SQL命令
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="type">SQL命令类型</param>
        /// <returns>SQL命令</returns>
        public abstract DbCommand GetCommand
            (DbConnection connection, string sql, SqlParameter[] parameters = null, CommandType type = CommandType.Text);

        /// <summary>
        ///     获取数据适配器
        /// </summary>
        /// <param name="command">SQL命令</param>
        /// <returns>数据适配器</returns>
        protected abstract DbDataAdapter getAdapter(DbCommand command);

        /// <summary>
        ///     获取数据集并关闭SQL命令
        /// </summary>
        /// <param name="command">SQL命令</param>
        /// <returns>数据集</returns>
        public DataSet GetDataSet(DbCommand command)
        {
            using (command)
            {
                var adapter = getAdapter(command);
                if (adapter != null)
                {
                    var data = new DataSet();
                    adapter.Fill(data);
                    return data;
                }
                return null;
            }
        }

        /// <summary>
        ///     获取数据表格并关闭SQL命令
        /// </summary>
        /// <param name="command">SQL命令</param>
        /// <returns>数据表格</returns>
        public DataTable GetDataTable(DbCommand command)
        {
            using (command)
            {
                var adapter = getAdapter(command);
                if (adapter != null)
                {
                    var table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
                return null;
            }
        }

        /// <summary>
        ///     获取数据表格
        /// </summary>
        /// <param name="tableName">表格名称</param>
        /// <returns>数据表格</returns>
        public abstract DataTable GetDataTable(string tableName);

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="command">SQL命令</param>
        /// <param name="errorValue">错误值</param>
        /// <returns>数据</returns>
        public TValueType GetValue<TValueType>(DbCommand command, TValueType errorValue)
        {
            var value = command.ExecuteScalar();
            return value != null && value != DBNull.Value ? (TValueType)value : errorValue;
        }

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="connection">SQL连接</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="errorValue">错误值</param>
        /// <returns>数据</returns>
        protected TValueType getValue<TValueType>(DbConnection connection, string sql, TValueType errorValue)
        {
            try
            {
                using (var command = GetCommand(connection, sql)) return GetValue(command, errorValue);
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, sql, false);
            }
            return errorValue;
        }

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="sql">SQL语句</param>
        /// <param name="errorValue">错误值</param>
        /// <param name="connection">SQL连接</param>
        /// <returns>数据</returns>
        public TValueType GetValue<TValueType>(string sql, TValueType errorValue, DbConnection connection)
        {
            if (sql.Length() != 0)
            {
                if (connection == null)
                {
                    using (connection = GetConnection())
                    {
                        if (connection != null) return getValue(connection, sql, errorValue);
                    }
                }
                else return getValue(connection, sql, errorValue);
            }
            return errorValue;
        }

        /// <summary>
        ///     执行SQL语句
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="sql">SQL语句</param>
        /// <returns>受影响的行数,错误返回ExecuteNonQueryError</returns>
        protected int executeNonQuery(DbConnection connection, string sql)
        {
            try
            {
                using (var command = GetCommand(connection, sql)) return command.ExecuteNonQuery();
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, sql, false);
            }
            return ExecuteNonQueryError;
        }

        /// <summary>
        ///     执行SQL语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="connection">SQL连接</param>
        /// <returns>受影响的行数,错误返回ExecuteNonQueryError</returns>
        public int ExecuteNonQuery(string sql, DbConnection connection)
        {
            if (sql.Length() != 0)
            {
                if (connection == null)
                {
                    using (connection = GetConnection())
                    {
                        if (connection != null) return executeNonQuery(connection, sql);
                    }
                }
                else return executeNonQuery(connection, sql);
            }
            return ExecuteNonQueryError;
        }

        /// <summary>
        ///     导入数据集合
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="data">数据集合</param>
        /// <param name="batchSize">批处理数量</param>
        /// <param name="timeout">超时秒数</param>
        /// <returns>成功导入数量</returns>
        protected abstract int import(DbConnection connection, DataTable data, int batchSize, int timeout);

        /// <summary>
        ///     导入数据集合
        /// </summary>
        /// <param name="data">数据集合</param>
        /// <param name="batchSize">批处理数量,0表示默认数量</param>
        /// <param name="timeout">超时秒数,0表示不设置超时</param>
        /// <param name="connection">SQL连接</param>
        /// <returns>成功导入数量</returns>
        public int Import(DataTable data, int batchSize = 0, int timeout = 0, DbConnection connection = null)
        {
            if (data != null && data.Rows.Count != 0)
            {
                if (connection == null)
                {
                    using (connection = GetConnection())
                    {
                        if (connection != null) return import(connection, data, batchSize, timeout);
                    }
                }
                else return import(connection, data, batchSize, timeout);
            }
            return 0;
        }

        /// <summary>
        ///     判断表格是否存在
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="tableName">表格名称</param>
        /// <returns>表格是否存在</returns>
        protected abstract bool isTable(DbConnection connection, string tableName);

        /// <summary>
        ///     判断表格是否存在
        /// </summary>
        /// <param name="tableName">表格名称</param>
        /// <param name="connection">SQL连接</param>
        /// <returns>表格是否存在</returns>
        public bool IsTable(string tableName, DbConnection connection)
        {
            if (tableName.Length() != 0)
            {
                if (connection == null)
                {
                    using (connection = GetConnection())
                    {
                        if (connection != null) return isTable(connection, tableName);
                    }
                }
                else return isTable(connection, tableName);
            }
            return false;
        }

        /// <summary>
        ///     写入索引名称
        /// </summary>
        /// <param name="sqlStream">SQL语句流</param>
        /// <param name="tableName">表格名称</param>
        /// <param name="columnCollection">索引列集合</param>
        internal static void AppendIndexName(TmphCharStream sqlStream, string tableName,
            THColumnCollection columnCollection)
        {
            if (columnCollection.Name.Length() == 0)
            {
                sqlStream.WriteNotNull("ix_");
                sqlStream.WriteNotNull(tableName);
                foreach (var column in columnCollection.Columns)
                {
                    sqlStream.Write('_');
                    sqlStream.WriteNotNull(column.Name);
                }
            }
            else sqlStream.WriteNotNull(columnCollection.Name);
        }

        /// <summary>
        ///     创建表格
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="table">表格信息</param>
        internal abstract bool createTable(DbConnection connection, TmphTable table);

        /// <summary>
        ///     创建表格
        /// </summary>
        /// <param name="table">表格信息</param>
        /// <param name="connection">SQL连接</param>
        internal bool CreateTable(TmphTable table, DbConnection connection)
        {
            if (table != null && table.Columns != null && table.Columns.Name.Length() != 0 &&
                table.Columns.Columns.length() != 0)
            {
                if (connection == null)
                {
                    using (connection = GetConnection())
                    {
                        if (connection != null) return createTable(connection, table);
                    }
                }
                else return createTable(connection, table);
            }
            return false;
        }

        /// <summary>
        ///     成员信息转换为数据列
        /// </summary>
        /// <param name="type">成员类型</param>
        /// <param name="sqlMember">SQL成员信息</param>
        /// <returns>数据列</returns>
        internal abstract TmphColumn getColumn(Type type, TmphDataMember sqlMember);

        /// <summary>
        ///     成员信息转换为数据列
        /// </summary>
        /// <param name="name">成员名称</param>
        /// <param name="type">成员类型</param>
        /// <param name="sqlMember">SQL成员信息</param>
        /// <returns>数据列</returns>
        internal TmphColumn GetColumn(string name, Type type, TmphDataMember sqlMember)
        {
            var column = TmphTypeAttribute.GetAttribute<TmphSqlColumn>(type, false, false) == null
                ? getColumn(type, sqlMember)
                : new TmphColumn { SqlColumnType = type };
            column.Name = name;
            return column;
        }

        /// <summary>
        ///     SQL列转换
        /// </summary>
        /// <param name="table">表格信息</param>
        internal void ToSqlColumn(TmphTable table)
        {
            if (table.Columns.Columns.any(column => column.SqlColumnType != null))
            {
                var sqlColumn = new FSqlColumnBuilder { Client = this };
                foreach (var column in table.Columns.Columns) sqlColumn.Append(column);
                table.Columns.Columns = sqlColumn.Columns.ToArray();
            }
        }

        /// <summary>
        ///     删除表格
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="tableName">表格名称</param>
        protected abstract bool dropTable(DbConnection connection, string tableName);

        /// <summary>
        ///     删除表格
        /// </summary>
        /// <param name="tableName">表格名称</param>
        /// <param name="connection">SQL连接</param>
        public bool DropTable(string tableName, DbConnection connection)
        {
            if (tableName.Length() != 0)
            {
                if (connection == null)
                {
                    using (connection = GetConnection())
                    {
                        if (connection != null) return dropTable(connection, tableName);
                    }
                }
                else return dropTable(connection, tableName);
            }
            return false;
        }

        /// <summary>
        ///     创建索引
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="tableName">表格名称</param>
        /// <param name="columnCollection">索引列集合</param>
        internal abstract bool createIndex(DbConnection connection, string tableName,
            THColumnCollection columnCollection);

        /// <summary>
        ///     创建索引
        /// </summary>
        /// <param name="tableName">表格名称</param>
        /// <param name="columnCollection">索引列集合</param>
        /// <param name="connection">SQL连接</param>
        internal bool CreateIndex(string tableName, THColumnCollection columnCollection, DbConnection connection)
        {
            if (tableName.Length() != 0 && columnCollection != null && columnCollection.Columns.length() != 0)
            {
                if (connection == null)
                {
                    using (connection = GetConnection())
                    {
                        if (connection != null) return createIndex(connection, tableName, columnCollection);
                    }
                }
                else return createIndex(connection, tableName, columnCollection);
            }
            return false;
        }

        /// <summary>
        ///     新增列集合
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="columnCollection">新增列集合</param>
        internal abstract bool addFields(DbConnection connection, THColumnCollection columnCollection);

        /// <summary>
        ///     新增列集合
        /// </summary>
        /// <param name="columnCollection">新增列集合</param>
        /// <param name="connection">SQL连接</param>
        internal bool AddFields(THColumnCollection columnCollection, DbConnection connection)
        {
            if (columnCollection != null && columnCollection.Columns.length() != 0 &&
                columnCollection.Name.Length() != 0)
            {
                if (connection == null)
                {
                    using (connection = GetConnection())
                    {
                        if (connection != null) return addFields(connection, columnCollection);
                    }
                }
                else return addFields(connection, columnCollection);
            }
            return false;
        }

        /// <summary>
        ///     获取表格名称集合
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <returns>表格名称集合</returns>
        protected abstract TmphSubArray<string> getTableNames(DbConnection connection);

        /// <summary>
        ///     获取表格名称集合
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <returns>表格名称集合</returns>
        public TmphSubArray<string> GetTableNames(DbConnection connection = null)
        {
            if (connection == null)
            {
                using (connection = GetConnection())
                {
                    if (connection != null) return getTableNames(connection);
                }
            }
            else return getTableNames(connection);
            return default(TmphSubArray<string>);
        }

        /// <summary>
        ///     根据表格名称获取表格信息
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="name">表格名称</param>
        /// <returns>表格信息</returns>
        internal abstract TmphTable getTable(DbConnection connection, string tableName);

        /// <summary>
        ///     根据表格名称获取表格信息
        /// </summary>
        /// <param name="connection">SQL连接</param>
        /// <param name="name">表格名称</param>
        /// <returns>表格信息</returns>
        internal TmphTable GetTable(string tableName, DbConnection connection = null)
        {
            if (tableName.Length() != 0)
            {
                if (connection == null)
                {
                    using (connection = GetConnection())
                    {
                        if (connection != null) return getTable(connection, tableName);
                    }
                }
                else return getTable(connection, tableName);
            }
            return null;
        }

        /// <summary>
        ///     对象转换成SQL字符串
        /// </summary>
        /// <param name="value">对象</param>
        /// <returns>SQL字符串</returns>
        public virtual string ToString(object value)
        {
            if (value != null)
            {
                string stringValue = null;
                var toString = TmphConstantConverter.Default[value.GetType()];
                var unsafeStream = toStringStream.Unsafer;
                if (toString == null)
                {
                    stringValue = value.ToString();
                    TmphInterlocked.NoCheckCompareSetSleep0(ref toStringLock);
                    try
                    {
                        unsafeStream.SetLength(0);
                        TmphConstantConverter.ConvertConstantStringQuote(toStringStream, stringValue);
                        stringValue = toStringStream.ToString();
                    }
                    finally
                    {
                        toStringLock = 0;
                    }
                }
                else
                {
                    TmphInterlocked.NoCheckCompareSetSleep0(ref toStringLock);
                    try
                    {
                        unsafeStream.SetLength(0);
                        toString(toStringStream, value);
                        stringValue = toStringStream.ToString();
                    }
                    finally
                    {
                        toStringLock = 0;
                    }
                }
                return stringValue;
            }
            return "null";
        }

        /// <summary>
        ///     执行SQL语句并更新成员
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sql">SQL语句</param>
        /// <param name="value">目标对象</param>
        /// <param name="oldValue">更新前的目标对象</param>
        /// <param name="memberMap">目标成员位图</param>
        /// <returns>更新是否成功</returns>
        protected bool set<TValueType, TModelType>(string sql, TValueType value, TValueType oldValue, TmphMemberMap memberMap)
            where TValueType : class, TModelType
            where TModelType : class
        {
            using (var connection = GetConnection())
            {
                if (connection != null)
                {
                    using (var command = GetCommand(connection, sql))
                    {
                        try
                        {
                            using (var reader = command.ExecuteReader(CommandBehavior.Default))
                            {
                                if (reader.Read())
                                {
                                    TmphSqlModel<TModelType>.TmphSet.Set(reader, oldValue, memberMap);
                                    if (reader.NextResult() && reader.Read())
                                    {
                                        TmphSqlModel<TModelType>.TmphSet.Set(reader, value, memberMap);
                                        return true;
                                    }
                                }
                            }
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, sql, false);
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        ///     执行SQL语句并更新成员
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="connection">SQL连接</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="value">目标对象</param>
        /// <param name="memberMap">目标成员位图</param>
        /// <returns>更新是否成功</returns>
        protected bool set<TValueType, TModelType>
            (DbConnection connection, string sql, TValueType value, TmphMemberMap memberMap)
            where TValueType : class, TModelType
            where TModelType : class
        {
            using (var command = GetCommand(connection, sql))
            {
                try
                {
                    using (var reader = command.ExecuteReader(CommandBehavior.Default))
                    {
                        if (reader.Read())
                        {
                            TmphSqlModel<TModelType>.TmphSet.Set(reader, value, memberMap);
                            return true;
                        }
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, sql, false);
                }
            }
            return false;
        }

        /// <summary>
        ///     执行SQL语句并更新成员
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sql">SQL语句</param>
        /// <param name="value">目标对象</param>
        /// <param name="memberMap">目标成员位图</param>
        /// <returns>更新是否成功</returns>
        protected bool set<TValueType, TModelType>
            (string sql, TValueType value, TmphMemberMap memberMap)
            where TValueType : class, TModelType
            where TModelType : class
        {
            using (var connection = GetConnection())
            {
                if (connection != null) return set<TValueType, TModelType>(connection, sql, value, memberMap);
            }
            return false;
        }

        /// <summary>
        ///     插入数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">表格模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="valuse">待插入数据集合</param>
        /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
        /// <returns>插入的数据集合,失败返回null</returns>
        public TValueType[] Insert<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType[] values, bool isIgnoreTransaction)
            where TValueType : class, TModelType
            where TModelType : class
        {
            if (isImport)
            {
                if (
                    values.count(
                        value => TmphSqlModel<TModelType>.TmphVerify.Verify(value, TmphMemberMap<TModelType>.Default, sqlTool)) ==
                    values.Length && sqlTool.CallOnInsert(values))
                {
                    if (TmphSqlModel<TModelType>.Identity != null)
                    {
                        Action<TValueType, long> identitySetter = TmphSqlModel<TModelType>.SetIdentity;
                        foreach (var value in values) identitySetter(value, sqlTool.NextIdentity);
                    }
                    var dataTable = sqlTool.GetDataTable(values);
                    if (!isIgnoreTransaction && sqlTool.IsInsertTransaction)
                    {
                        if (TmphDomainUnload.TransactionStart(false))
                        {
                            try
                            {
                                insertLock(sqlTool, values, dataTable);
                                return values;
                            }
                            finally
                            {
                                TmphDomainUnload.TransactionEnd();
                            }
                        }
                    }
                    else
                    {
                        insertLock(sqlTool, values, dataTable);
                        return values;
                    }
                }
            }
            else
            {
                var newValues = new TmphSubArray<TValueType>(values.Length);
                foreach (var value in values)
                {
                    if (Insert(sqlTool, value, null, false)) newValues.UnsafeAdd(value);
                }
                return newValues.ToArray();
            }
            return null;
        }

        /// <summary>
        ///     插入数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">表格模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="values">待插入数据集合</param>
        /// <param name="dataTable">待插入数据集合</param>
        private void insertLock<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType[] values, DataTable dataTable)
            where TValueType : class, TModelType
            where TModelType : class
        {
            if (sqlTool.IsLockWrite)
            {
                Monitor.Enter(sqlTool.Lock);
                try
                {
                    if (sqlTool.CallOnInsertLock(values)) insert(sqlTool, values, dataTable);
                }
                finally
                {
                    Monitor.Exit(sqlTool.Lock);
                }
            }
            else insert(sqlTool, values, dataTable);
            foreach (var value in values) sqlTool.CallOnInserted(value);
        }

        /// <summary>
        ///     插入数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">表格模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="values">待插入数据集合</param>
        /// <param name="dataTable">待插入数据集合</param>
        /// <returns>成功导入数量</returns>
        private int insert<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType[] values, DataTable dataTable)
            where TValueType : class, TModelType
            where TModelType : class
        {
            using (var connection = GetConnection())
            {
                if (connection != null)
                {
                    import(connection, dataTable, 0, 0);
                    if (sqlTool.IsLockWrite)
                    {
                        foreach (var value in values) sqlTool.CallOnInsertedLock(value);
                    }
                }
            }
            return 0;
        }

        /// <summary>
        ///     插入数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="value">待插入数据</param>
        /// <param name="memberMap">目标成员位图</param>
        /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
        /// <returns>是否成功</returns>
        public bool Insert<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap memberMap,
                bool isIgnoreTransaction)
            where TValueType : class, TModelType
            where TModelType : class
        {
            if (memberMap == null) memberMap = TmphMemberMap<TModelType>.Default;
            if (TmphSqlModel<TModelType>.TmphVerify.Verify(value, memberMap, sqlTool) && sqlTool.CallOnInsert(value))
            {
                if (!isIgnoreTransaction && sqlTool.IsInsertTransaction)
                {
                    if (TmphDomainUnload.TransactionStart(false))
                    {
                        try
                        {
                            return insertLock(sqlTool, value, memberMap);
                        }
                        finally
                        {
                            TmphDomainUnload.TransactionEnd();
                        }
                    }
                }
                else return insertLock(sqlTool, value, memberMap);
            }
            return false;
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
        private bool insertLock<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap memberMap)
            where TValueType : class, TModelType
            where TModelType : class
        {
            var isInsert = false;
            if (sqlTool.IsLockWrite)
            {
                Monitor.Enter(sqlTool.Lock);
                try
                {
                    if (sqlTool.CallOnInsertLock(value)) isInsert = insert(sqlTool, value, memberMap);
                }
                finally
                {
                    Monitor.Exit(sqlTool.Lock);
                }
            }
            else isInsert = insert(sqlTool, value, memberMap);
            if (isInsert) sqlTool.CallOnInserted(value);
            return isInsert;
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
        protected abstract bool insert<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap memberMap)
            where TValueType : class, TModelType
            where TModelType : class;

        /// <summary>
        ///     更新数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="value">待更新数据自增id标识</param>
        /// <param name="updateExpression">待更新数据</param>
        /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
        /// <returns>是否成功</returns>
        public bool Update<TValueType, TModelType>(TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value
            , TmphSqlTable.TmphUpdateExpression updateExpression, bool isIgnoreTransaction)
            where TValueType : class, TModelType
            where TModelType : class
        {
            using (var memberMap = updateExpression.CreateMemberMap<TModelType>())
            {
                if (!memberMap.IsDefault)
                {
                    if (TmphSqlModel<TModelType>.Identity != null)
                        memberMap.ClearMember(TmphSqlModel<TModelType>.Identity.MemberMapIndex);
                    if (sqlTool.CallOnUpdate(value, memberMap))
                    {
                        if (!isIgnoreTransaction && sqlTool.IsUpdateTransaction)
                        {
                            if (TmphDomainUnload.TransactionStart(false))
                            {
                                try
                                {
                                    return updateLock(sqlTool, value, updateExpression, memberMap);
                                }
                                finally
                                {
                                    TmphDomainUnload.TransactionEnd();
                                }
                            }
                        }
                        else return updateLock(sqlTool, value, updateExpression, memberMap);
                    }
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
        /// <param name="value">待更新数据自增id标识</param>
        /// <param name="sqlExpression">待更新数据</param>
        /// <param name="memberMap">目标成员位图</param>
        /// <returns>是否成功</returns>
        private bool updateLock<TValueType, TModelType>(TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool
            , TValueType value, TmphSqlTable.TmphUpdateExpression sqlExpression, TmphMemberMap memberMap)
            where TValueType : class, TModelType
            where TModelType : class
        {
            TValueType oldValue = null;
            if (sqlTool.IsLockWrite)
            {
                Monitor.Enter(sqlTool.Lock);
                try
                {
                    if (sqlTool.CallOnUpdateLock(value, memberMap))
                        oldValue = update(sqlTool, value, sqlExpression, memberMap);
                }
                finally
                {
                    Monitor.Exit(sqlTool.Lock);
                }
            }
            else oldValue = update(sqlTool, value, sqlExpression, memberMap);
            if (oldValue != null)
            {
                sqlTool.CallOnUpdated(value, oldValue, memberMap);
                return true;
            }
            return false;
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
        protected abstract TValueType update<TValueType, TModelType>(TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool
            , TValueType value, TmphSqlTable.TmphUpdateExpression sqlExpression, TmphMemberMap memberMap)
            where TValueType : class, TModelType
            where TModelType : class;

        /// <summary>
        ///     更新数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="value">待更新数据</param>
        /// <param name="memberMap">目标成员位图</param>
        /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
        /// <returns>是否成功</returns>
        public bool Update<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap memberMap,
                bool isIgnoreTransaction)
            where TValueType : class, TModelType
            where TModelType : class
        {
            using (var updateMemberMap = sqlTool.GetMemberMapClearIdentity(memberMap))
            {
                if (TmphSqlModel<TModelType>.TmphVerify.Verify(value, updateMemberMap, sqlTool) &&
                    sqlTool.CallOnUpdate(value, updateMemberMap))
                {
                    if (!isIgnoreTransaction && sqlTool.IsUpdateTransaction)
                    {
                        if (TmphDomainUnload.TransactionStart(false))
                        {
                            try
                            {
                                return updateLock(sqlTool, value, updateMemberMap);
                            }
                            finally
                            {
                                TmphDomainUnload.TransactionEnd();
                            }
                        }
                    }
                    else return updateLock(sqlTool, value, updateMemberMap);
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
        /// <returns>是否成功</returns>
        private bool updateLock<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap memberMap)
            where TValueType : class, TModelType
            where TModelType : class
        {
            TValueType oldValue = null;
            if (sqlTool.IsLockWrite)
            {
                Monitor.Enter(sqlTool.Lock);
                try
                {
                    if (sqlTool.CallOnUpdateLock(value, memberMap)) oldValue = update(sqlTool, value, memberMap);
                }
                finally
                {
                    Monitor.Exit(sqlTool.Lock);
                }
            }
            else oldValue = update(sqlTool, value, memberMap);
            if (oldValue != null)
            {
                sqlTool.CallOnUpdated(value, oldValue, memberMap);
                return true;
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
        protected abstract TValueType update<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap memberMap)
            where TValueType : class, TModelType
            where TModelType : class;

        /// <summary>
        ///     更新数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="value">待更新数据自增id标识</param>
        /// <param name="updateExpression">待更新数据</param>
        /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
        /// <returns>是否成功</returns>
        public bool UpdateByIdentity<TValueType, TModelType>(TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool,
            TValueType value
            , TmphSqlTable.TmphUpdateExpression updateExpression, bool isIgnoreTransaction)
            where TValueType : class, TModelType
            where TModelType : class
        {
            using (var memberMap = updateExpression.CreateMemberMap<TModelType>())
            {
                if (!memberMap.IsDefault)
                {
                    memberMap.ClearMember(TmphSqlModel<TModelType>.Identity.MemberMapIndex);
                    if (sqlTool.CallOnUpdate(value, memberMap))
                    {
                        if (!isIgnoreTransaction && sqlTool.IsUpdateTransaction)
                        {
                            if (TmphDomainUnload.TransactionStart(false))
                            {
                                try
                                {
                                    return updateByIdentityLock(sqlTool, value, updateExpression, memberMap);
                                }
                                finally
                                {
                                    TmphDomainUnload.TransactionEnd();
                                }
                            }
                        }
                        else return updateByIdentityLock(sqlTool, value, updateExpression, memberMap);
                    }
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
        /// <param name="value">待更新数据自增id标识</param>
        /// <param name="sqlExpression">待更新数据</param>
        /// <param name="memberMap">目标成员位图</param>
        /// <returns>是否成功</returns>
        private bool updateByIdentityLock<TValueType, TModelType>(TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool
            , TValueType value, TmphSqlTable.TmphUpdateExpression updateExpression, TmphMemberMap memberMap)
            where TValueType : class, TModelType
            where TModelType : class
        {
            TValueType oldValue = null;
            if (sqlTool.IsLockWrite)
            {
                Monitor.Enter(sqlTool.Lock);
                try
                {
                    if (sqlTool.CallOnUpdateLock(value, memberMap))
                        oldValue = updateByIdentity(sqlTool, value, updateExpression, memberMap);
                }
                finally
                {
                    Monitor.Exit(sqlTool.Lock);
                }
            }
            else oldValue = updateByIdentity(sqlTool, value, updateExpression, memberMap);
            if (oldValue != null)
            {
                sqlTool.CallOnUpdated(value, oldValue, memberMap);
                return true;
            }
            return false;
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
        protected abstract TValueType updateByIdentity<TValueType, TModelType>(
            TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool
            , TValueType value, TmphSqlTable.TmphUpdateExpression sqlExpression, TmphMemberMap memberMap)
            where TValueType : class, TModelType
            where TModelType : class;

        /// <summary>
        ///     更新数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="value">待更新数据</param>
        /// <param name="memberMap">目标成员位图</param>
        /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
        /// <returns>是否成功</returns>
        public bool UpdateByIdentity<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap memberMap,
                bool isIgnoreTransaction)
            where TValueType : class, TModelType
            where TModelType : class
        {
            using (var updateMemberMap = sqlTool.GetMemberMapClearIdentity(memberMap))
            {
                if (TmphSqlModel<TModelType>.TmphVerify.Verify(value, updateMemberMap, sqlTool) &&
                    sqlTool.CallOnUpdate(value, updateMemberMap))
                {
                    if (!isIgnoreTransaction && sqlTool.IsUpdateTransaction)
                    {
                        if (TmphDomainUnload.TransactionStart(false))
                        {
                            try
                            {
                                return updateByIdentityLock(sqlTool, value, updateMemberMap);
                            }
                            finally
                            {
                                TmphDomainUnload.TransactionEnd();
                            }
                        }
                    }
                    else return updateByIdentityLock(sqlTool, value, updateMemberMap);
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
        /// <returns>是否成功</returns>
        private bool updateByIdentityLock<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap memberMap)
            where TValueType : class, TModelType
            where TModelType : class
        {
            TValueType oldValue = null;
            if (sqlTool.IsLockWrite)
            {
                Monitor.Enter(sqlTool.Lock);
                try
                {
                    if (sqlTool.CallOnUpdateLock(value, memberMap))
                        oldValue = updateByIdentity(sqlTool, value, memberMap);
                }
                finally
                {
                    Monitor.Exit(sqlTool.Lock);
                }
            }
            else oldValue = updateByIdentity(sqlTool, value, memberMap);
            if (oldValue != null)
            {
                sqlTool.CallOnUpdated(value, oldValue, memberMap);
                return true;
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
        protected abstract TValueType updateByIdentity<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap memberMap)
            where TValueType : class, TModelType
            where TModelType : class;

        /// <summary>
        ///     删除数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="value">待删除数据</param>
        /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
        /// <returns>是否成功</returns>
        public bool Delete<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, bool isIgnoreTransaction)
            where TValueType : class, TModelType
            where TModelType : class
        {
            if (sqlTool.CallOnDelete(value))
            {
                if (!isIgnoreTransaction && sqlTool.IsDeleteTransaction)
                {
                    if (TmphDomainUnload.TransactionStart(false))
                    {
                        try
                        {
                            return deleteLock(sqlTool, value);
                        }
                        finally
                        {
                            TmphDomainUnload.TransactionEnd();
                        }
                    }
                }
                else return deleteLock(sqlTool, value);
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
        private bool deleteLock<TValueType, TModelType>(TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool,
            TValueType value)
            where TValueType : class, TModelType
            where TModelType : class
        {
            var isDelete = false;
            if (sqlTool.IsLockWrite)
            {
                Monitor.Enter(sqlTool.Lock);
                try
                {
                    if (sqlTool.CallOnDeleteLock(value)) isDelete = delete(sqlTool, value);
                }
                finally
                {
                    Monitor.Exit(sqlTool.Lock);
                }
            }
            else isDelete = delete(sqlTool, value);
            if (isDelete)
            {
                sqlTool.CallOnDeleted(value);
                return true;
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
        protected abstract bool delete<TValueType, TModelType>(TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool,
            TValueType value)
            where TValueType : class, TModelType
            where TModelType : class;

        /// <summary>
        ///     删除数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="value">待删除数据</param>
        /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
        /// <returns>是否成功</returns>
        public bool DeleteByIdentity<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, bool isIgnoreTransaction)
            where TValueType : class, TModelType
            where TModelType : class
        {
            if (sqlTool.CallOnDelete(value))
            {
                if (!isIgnoreTransaction && sqlTool.IsDeleteTransaction)
                {
                    if (TmphDomainUnload.TransactionStart(false))
                    {
                        try
                        {
                            return deleteByIdentityLock(sqlTool, value);
                        }
                        finally
                        {
                            TmphDomainUnload.TransactionEnd();
                        }
                    }
                }
                else return deleteByIdentityLock(sqlTool, value);
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
        private bool deleteByIdentityLock<TValueType, TModelType>(TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool,
            TValueType value)
            where TValueType : class, TModelType
            where TModelType : class
        {
            var isDelete = false;
            if (sqlTool.IsLockWrite)
            {
                Monitor.Enter(sqlTool.Lock);
                try
                {
                    if (sqlTool.CallOnDeleteLock(value)) isDelete = deleteByIdentity(sqlTool, value);
                }
                finally
                {
                    Monitor.Exit(sqlTool.Lock);
                }
            }
            else isDelete = deleteByIdentity(sqlTool, value);
            if (isDelete)
            {
                sqlTool.CallOnDeleted(value);
                return true;
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
        protected abstract bool deleteByIdentity<TValueType, TModelType>(
            TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value)
            where TValueType : class, TModelType
            where TModelType : class;

        /// <summary>
        ///     委托关联表达式转SQL表达式
        /// </summary>
        /// <param name="expression">委托关联表达式</param>
        /// <param name="logicConstantWhere">逻辑常量值</param>
        /// <returns>SQL表达式(null表示常量条件)</returns>
        public abstract string GetWhere(LambdaExpression expression, ref bool logicConstantWhere);

        /// <summary>
        ///     委托关联表达式转SQL表达式
        /// </summary>
        /// <param name="expression">委托关联表达式</param>
        /// <param name="sqlStream">SQL表达式流</param>
        /// <param name="logicConstantWhere">逻辑常量值</param>
        /// <returns>参数成员名称</returns>
        internal abstract string GetWhere(LambdaExpression expression, TmphCharStream sqlStream,
            ref bool logicConstantWhere);

        /// <summary>
        ///     委托关联表达式转SQL表达式
        /// </summary>
        /// <param name="expression">委托关联表达式</param>
        /// <returns>参数成员名称+SQL表达式</returns>
        public abstract TmphKeyValue<string, string> GetSql(LambdaExpression expression);

        /// <summary>
        ///     委托关联表达式转SQL表达式
        /// </summary>
        /// <param name="expression">委托关联表达式</param>
        /// <param name="sqlStream">SQL表达式流</param>
        /// <returns>参数成员名称</returns>
        internal abstract string GetSql(LambdaExpression expression, TmphCharStream sqlStream);

        /// <summary>
        ///     委托关联表达式转SQL列
        /// </summary>
        /// <param name="expression">委托关联表达式</param>
        /// <returns>SQL列</returns>
        public object GetSqlColumn(LambdaExpression expression)
        {
            if (expression != null)
            {
                var sqlExpression = TmphLambdaExpression.convert(expression);
                try
                {
                    var body = sqlExpression.Body;
                    if (body.IsConstant) return ((TmphConstantExpression)body).Value;
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
        public abstract int Count<TValueType, TModelType>(TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool,
            Expression<Func<TModelType, bool>> expression)
            where TValueType : class, TModelType
            where TModelType : class;

        /// <summary>
        ///     执行SQL语句并返回数据集合
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sql">SQL语句</param>
        /// <param name="skipCount">跳过记录数</param>
        /// <param name="memberMap">成员位图</param>
        /// <returns>数据集合</returns>
        protected IEnumerable<TValueType> selectPushMemberMap<TValueType, TModelType>(string sql, int skipCount,
            TmphMemberMap memberMap)
            where TValueType : class, TModelType
            where TModelType : class
        {
            using (memberMap)
            using (var connection = GetConnection())
            using (var command = GetCommand(connection, sql))
            {
                DbDataReader reader = null;
                try
                {
                    reader = command.ExecuteReader(CommandBehavior.Default);
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, sql, false);
                }
                if (reader != null)
                {
                    using (reader)
                    {
                        while (skipCount != 0 && reader.Read()) --skipCount;
                        if (skipCount == 0)
                        {
                            while (reader.Read())
                            {
                                var value = TmphConstructor<TValueType>.New();
                                TmphSqlModel<TModelType>.TmphSet.Set(reader, value, memberMap);
                                yield return value;
                            }
                        }
                    }
                }
            }
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
        internal TReturnType GetValue<TValueType, TModelType, TReturnType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TmphSelectQuery<TModelType> query, string memberName,
                TReturnType errorValue)
            where TValueType : class, TModelType
            where TModelType : class
        {
            return getValue(sqlTool, query, memberName, errorValue);
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
        internal abstract TReturnType getValue<TValueType, TModelType, TReturnType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TmphSelectQuery<TModelType> query, string memberName,
                TReturnType errorValue)
            where TValueType : class, TModelType
            where TModelType : class;

        /// <summary>
        ///     查询对象集合
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="query">查询信息</param>
        /// <param name="memberMap">成员位图</param>
        /// <returns>对象集合</returns>
        public IEnumerable<TValueType> Select<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TmphSelectQuery<TModelType> query, TmphMemberMap memberMap)
            where TValueType : class, TModelType
            where TModelType : class
        {
            var selectMemberMap = TmphSqlModel<TModelType>.CopyMemberMap;
            if (memberMap != null && !memberMap.IsDefault) selectMemberMap.And(memberMap);
            return selectPushMemberMap(sqlTool, query, selectMemberMap);
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
        internal abstract IEnumerable<TValueType> selectPushMemberMap<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TmphSelectQuery<TModelType> query, TmphMemberMap memberMap)
            where TValueType : class, TModelType
            where TModelType : class;

        /// <summary>
        ///     查询对象
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="value">匹配成员值</param>
        /// <param name="memberMap">成员位图</param>
        /// <returns>对象集合</returns>
        public abstract TValueType GetByIdentity<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap memberMap)
            where TValueType : class, TModelType
            where TModelType : class;

        /// <summary>
        ///     查询对象
        /// </summary>
        /// <typeparam name="TValueType">对象类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="value">匹配成员值</param>
        /// <param name="memberMap">成员位图</param>
        /// <returns>对象集合</returns>
        public abstract TValueType GetByPrimaryKey<TValueType, TModelType>
            (TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, TValueType value, TmphMemberMap memberMap)
            where TValueType : class, TModelType
            where TModelType : class;

        /// <summary>
        ///     SQL列转换
        /// </summary>
        internal struct FSqlColumnBuilder
        {
            /// <summary>
            ///     SQL列转换类型集合
            /// </summary>
            private static readonly Dictionary<Type, TmphColumn[]> sqlColumns =
                TmphDictionary.CreateOnly<Type, TmphColumn[]>();

            /// <summary>
            ///     SQL列转换类型集合访问锁
            /// </summary>
            private static int sqlColumnLock;

            /// <summary>
            ///     SQL客户端操作
            /// </summary>
            public TmphClient Client;

            /// <summary>
            ///     数据列集合
            /// </summary>
            public TmphSubArray<TmphColumn> Columns;

            /// <summary>
            ///     SQL列转换
            /// </summary>
            /// <param name="column">数据列</param>
            public void Append(TmphColumn column)
            {
                if (column.SqlColumnType == null) Columns.Add(column);
                else
                {
                    foreach (var sqlColumn in get(column.SqlColumnType))
                    {
                        var copyColumn = TmphMemberCopyer<TmphColumn>.MemberwiseClone(sqlColumn);
                        copyColumn.Name = column.Name + "_" + copyColumn.Name;
                        Columns.Add(copyColumn);
                    }
                }
            }

            /// <summary>
            ///     添加SQL列类型
            /// </summary>
            /// <param name="type">SQL列类型</param>
            private void append(Type type)
            {
                foreach (
                    var member in
                        TmphSqlModel.GetMemberIndexs(type, TmphTypeAttribute.GetAttribute<TmphSqlColumn>(type, false, false)))
                {
                    var column = Client.GetColumn(member.Key.Member.Name, member.Key.Type, member.Value);
                    if (column.SqlColumnType == null) Columns.Add(column);
                    else
                    {
                        foreach (var sqlColumn in getNoLock(column.SqlColumnType))
                        {
                            var copyColumn = TmphMemberCopyer<TmphColumn>.MemberwiseClone(sqlColumn);
                            copyColumn.Name = column.Name + "_" + copyColumn.Name;
                            Columns.Add(copyColumn);
                        }
                    }
                }
            }

            /// <summary>
            ///     获取SQL列转换集合
            /// </summary>
            /// <param name="type">SQL列类型</param>
            /// <returns>SQL列转换集合</returns>
            private TmphColumn[] getNoLock(Type type)
            {
                TmphColumn[] columns;
                if (!sqlColumns.TryGetValue(type, out columns))
                {
                    var index = Columns.Count;
                    append(type);
                    sqlColumns.Add(type,
                        columns = TmphSubArray<TmphColumn>.Unsafe(Columns.array, index, Columns.Count - index).GetArray());
                    Columns.UnsafeSetLength(index);
                }
                return columns;
            }

            /// <summary>
            ///     获取SQL列转换集合
            /// </summary>
            /// <param name="type">SQL列类型</param>
            /// <returns>SQL列转换集合</returns>
            private TmphColumn[] get(Type type)
            {
                TmphColumn[] columns;
                TmphInterlocked.NoCheckCompareSetSleep0(ref sqlColumnLock);
                try
                {
                    columns = getNoLock(type);
                }
                finally
                {
                    sqlColumnLock = 0;
                }
                return columns;
            }

            /// <summary>
            ///     获取SQL列转换集合
            /// </summary>
            /// <param name="type">SQL列类型</param>
            /// <param name="TmphClient">SQL客户端操作</param>
            /// <returns>SQL列转换集合</returns>
            public static TmphColumn[] Get(Type type, TmphClient TmphClient)
            {
                TmphColumn[] columns;
                TmphInterlocked.NoCheckCompareSetSleep0(ref sqlColumnLock);
                try
                {
                    if (!sqlColumns.TryGetValue(type, out columns))
                    {
                        var sqlColumn = new FSqlColumnBuilder { Client = TmphClient };
                        sqlColumn.append(type);
                        sqlColumns.Add(type, columns = sqlColumn.Columns.ToArray());
                    }
                }
                finally
                {
                    sqlColumnLock = 0;
                }
                return columns;
            }
        }
    }
}