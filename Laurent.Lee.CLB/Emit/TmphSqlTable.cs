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
using Laurent.Lee.CLB.Code.CSharp;
using Laurent.Lee.CLB.Config;
using Laurent.Lee.CLB.Reflection;
using Laurent.Lee.CLB.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    ///     数据库表格配置
    /// </summary>
    public class TmphSqlTable : Attribute
    {
        /// <summary>
        ///     SQL表格操作工具 字段名称
        /// </summary>
        public const string SqlTableName = "SqlTable";

        /// <summary>
        ///     链接类型
        /// </summary>
        public string ConnectionName;

        /// <summary>
        ///     是否自动获取自增标识
        /// </summary>
        public bool IsLoadIdentity = true;

        /// <summary>
        ///     写操作是否加锁
        /// </summary>
        public bool IsLockWrite = true;

        /// <summary>
        ///     表格名称
        /// </summary>
        public string TableName;

        /// <summary>
        ///     链接类型
        /// </summary>
        public virtual string ConnectionType
        {
            get { return ConnectionName; }
        }

        /// <summary>
        ///     获取表格名称
        /// </summary>
        /// <param name="type">表格绑定类型</param>
        /// <returns>表格名称</returns>
        internal unsafe string GetTableName(Type type)
        {
            if (TableName != null) return TableName;
            string name = null;
            if (TmphSql.Default.TableNamePrefixs.Length != 0)
            {
                name = type.fullName();
                foreach (var perfix in TmphSql.Default.TableNamePrefixs)
                {
                    if (name.Length > perfix.Length && name.StartsWith(perfix, StringComparison.Ordinal) &&
                        name[perfix.Length] == '.')
                    {
                        return name.Substring(perfix.Length + 1);
                    }
                }
            }
            var depth = TmphSql.Default.TableNameDepth;
            if (depth <= 0) return type.Name;
            if (name == null) name = type.fullName();
            fixed (char* nameFixed = name)
            {
                char* start = nameFixed, end = nameFixed + name.Length;
                do
                {
                    while (start != end && *start != '.') ++start;
                    if (start == end) return type.Name;
                    ++start;
                } while (--depth != 0);
                var index = (int)(start - nameFixed);
                while (start != end)
                {
                    if (*start == '.') *start = '_';
                    ++start;
                }
                return name.Substring(index);
            }
        }

        /// <summary>
        ///     取消确认
        /// </summary>
        public sealed class TmphCancel
        {
            /// <summary>
            ///     是否取消
            /// </summary>
            private bool isCancel;

            /// <summary>
            ///     是否取消
            /// </summary>
            public bool IsCancel
            {
                get { return isCancel; }
                set { if (value) isCancel = true; }
            }
        }

        /// <summary>
        ///     更新数据表达式
        /// </summary>
        /// <typeparam name="TModelType"></typeparam>
        public struct TmphUpdateExpression
        {
            /// <summary>
            ///     SQL表达式集合
            /// </summary>
            private TmphSubArray<TmphKeyValue<string, string>> values;

            /// <summary>
            ///     SQL表达式数量
            /// </summary>
            internal int Count
            {
                get { return values.Count; }
            }

            /// <summary>
            ///     添加SQL表达式
            /// </summary>
            /// <param name="value"></param>
            internal void Add(TmphKeyValue<string, string> value)
            {
                values.Add(value);
            }

            /// <summary>
            ///     更新数据成员位图
            /// </summary>
            /// <typeparam name="TModelType"></typeparam>
            /// <returns></returns>
            internal TmphMemberMap CreateMemberMap<TModelType>()
            {
                var memberMap = TmphMemberMap<TModelType>.New();
                foreach (var value in values.array)
                {
                    if (value.Key == null) break;
                    if (!memberMap.SetMember(value.Key))
                    {
                        memberMap.Dispose();
                        TmphLog.Error.Throw(typeof(TModelType).fullName() + " 找不到SQL字段 " + value.Key, false, true);
                    }
                }
                return memberMap;
            }

            /// <summary>
            ///     数据更新SQL流
            /// </summary>
            /// <param name="sqlStream"></param>
            internal void Update(TmphCharStream sqlStream)
            {
                var isNext = 0;
                foreach (var value in values.array)
                {
                    if (value.Key == null) break;
                    if (isNext == 0) isNext = 1;
                    else sqlStream.Write(',');
                    sqlStream.WriteNotNull(value.Key);
                    sqlStream.Write('=');
                    sqlStream.WriteNotNull(value.Value);
                }
            }
        }

        /// <summary>
        ///     数据库表格操作工具
        /// </summary>
        public abstract class TmphSqlTool : IDisposable
        {
            /// <summary>
            ///     数据库字符串验证函数
            /// </summary>
            internal static readonly MethodInfo StringVerifyMethod = typeof(TmphSqlTool).GetMethod("StringVerify",
                BindingFlags.Instance | BindingFlags.NonPublic, null,
                new[] { typeof(string), typeof(string), typeof(int), typeof(bool), typeof(bool) }, null);

            /// <summary>
            ///     数据库字段空值验证
            /// </summary>
            internal static readonly MethodInfo NullVerifyMethod = typeof(TmphSqlTool).GetMethod("NullVerify",
                BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(string) }, null);

            /// <summary>
            ///     缓存加载等待
            /// </summary>
            public readonly EventWaitHandle LoadWait;

            /// <summary>
            ///     SQL访问锁
            /// </summary>
            public readonly object Lock = new object();

            /// <summary>
            ///     数据库表格配置
            /// </summary>
            private readonly TmphSqlTable sqlTable;

            /// <summary>
            ///     自增ID生成器
            /// </summary>
            protected long identity64;

            /// <summary>
            ///     成员名称是否忽略大小写
            /// </summary>
            protected bool ignoreCase;

            /// <summary>
            ///     数据库表格是否加载成功
            /// </summary>
            protected bool isTable;

            /// <summary>
            ///     待创建一级索引的成员名称集合
            /// </summary>
            protected TmphStateSearcher.TmphAscii<string> noIndexMemberNames;

            /// <summary>
            ///     数据库表格操作工具
            /// </summary>
            /// <param name="sqlTable">数据库表格配置</param>
            /// <param name="TmphClient">SQL操作客户端</param>
            /// <param name="tableName">表格名称</param>
            protected TmphSqlTool(TmphSqlTable sqlTable, TmphClient TmphClient, string tableName)
            {
                this.sqlTable = sqlTable;
                Client = TmphClient;
                TableName = tableName;
                ignoreCase = TmphEnum<Sql.TmphType, TmphTypeInfo>.Array(TmphClient.Connection.Type).IgnoreCase;
                LoadWait = new EventWaitHandle(false, EventResetMode.ManualReset);
            }

            /// <summary>
            ///     当前自增ID
            /// </summary>
            public long Identity64
            {
                get { return identity64; }
                set { if (!sqlTable.IsLoadIdentity) identity64 = value; }
            }

            /// <summary>
            ///     自增ID
            /// </summary>
            internal long NextIdentity
            {
                get { return Interlocked.Increment(ref identity64); }
            }

            /// <summary>
            ///     SQL操作客户端
            /// </summary>
            internal TmphClient Client { get; private set; }

            /// <summary>
            ///     表格名称
            /// </summary>
            internal string TableName { get; private set; }

            /// <summary>
            ///     数据库表格是否加载成功
            /// </summary>
            public bool IsTable
            {
                get { return isTable; }
            }

            /// <summary>
            ///     是否锁定更新操作
            /// </summary>
            internal bool IsLockWrite
            {
                get { return sqlTable.IsLockWrite; }
            }

            /// <summary>
            ///     释放资源
            /// </summary>
            public virtual void Dispose()
            {
                CLB.TmphPub.Dispose(ref noIndexMemberNames);
            }

            /// <summary>
            ///     创建索引
            /// </summary>
            /// <param name="name">列名称</param>
            internal void CreateIndex(string name)
            {
                createIndex(name, false);
            }

            /// <summary>
            ///     创建索引
            /// </summary>
            /// <param name="name">列名称</param>
            /// <param name="isUnique">是否唯一值</param>
            protected void createIndex(string name, bool isUnique)
            {
                if (ignoreCase) name = name.ToLower();
                if (noIndexMemberNames.ContainsKey(name))
                {
                    var isIndex = false;
                    Exception exception = null;
                    Monitor.Enter(Lock);
                    try
                    {
                        if (noIndexMemberNames.Remove(name))
                        {
                            isIndex = true;
                            if (Client.CreateIndex(TableName, new THColumnCollection
                            {
                                Columns = new[] { new TmphColumn { Name = name } },
                                Type = isUnique ? THColumnCollection.TmphType.UniqueIndex : THColumnCollection.TmphType.Index
                            }, null))
                            {
                                return;
                            }
                        }
                    }
                    catch (Exception error)
                    {
                        exception = error;
                    }
                    finally
                    {
                        Monitor.Exit(Lock);
                    }
                    if (isIndex) TmphLog.Error.Add(exception, "索引 " + TableName + "." + name + " 创建失败", false);
                }
            }

            /// <summary>
            ///     字符串验证
            /// </summary>
            /// <param name="memberName">成员名称</param>
            /// <param name="value">成员值</param>
            /// <param name="length">最大长度</param>
            /// <param name="isAscii">是否ASCII</param>
            /// <param name="isNull">是否可以为null</param>
            /// <returns>字符串是否通过默认验证</returns>
            internal unsafe bool StringVerify(string memberName, string value, int length, bool isAscii, bool isNull)
            {
                if (!isNull && value == null)
                {
                    NullVerify(memberName);
                    return false;
                }
                if (length != 0)
                {
                    if (isAscii)
                    {
                        var nextLength = length - value.Length;
                        if (nextLength >= 0 && value.Length() > (length >> 1))
                        {
                            fixed (char* valueFixed = value)
                            {
                                for (char* start = valueFixed, end = valueFixed + value.Length; start != end; ++start)
                                {
                                    if ((*start & 0xff00) != 0 && --nextLength < 0) break;
                                }
                            }
                        }
                        if (nextLength < 0)
                        {
                            TmphLog.Error.Add(TableName + "." + memberName + " 超长 " + length.toString(), true, true);
                            return false;
                        }
                    }
                    else
                    {
                        if (value.Length() > length)
                        {
                            TmphLog.Error.Add(
                                TableName + "." + memberName + " 超长 " + value.Length.toString() + " > " +
                                length.toString(), true, false);
                            return false;
                        }
                    }
                }
                return true;
            }

            /// <summary>
            ///     成员值不能为null
            /// </summary>
            /// <param name="memberName">成员名称</param>
            internal void NullVerify(string memberName)
            {
                TmphLog.Error.Add(TableName + "." + memberName + " 不能为null", true, true);
            }
        }

        /// <summary>
        ///     数据库表格操作工具
        /// </summary>
        /// <typeparam name="TModelType">模型类型</typeparam>
        public abstract class TmphSqlTool<TModelType> : TmphSqlTool
            where TModelType : class
        {
            /// <summary>
            ///     更新查询SQL数据成员
            /// </summary>
            protected TmphMemberMap selectMemberMap = TmphMemberMap<TModelType>.New();

            /// <summary>
            ///     数据库表格操作工具
            /// </summary>
            /// <param name="sqlTable">数据库表格配置</param>
            /// <param name="TmphClient">SQL操作客户端</param>
            /// <param name="tableName">表格名称</param>
            protected TmphSqlTool(TmphSqlTable sqlTable, TmphClient TmphClient, string tableName)
                : base(sqlTable, TmphClient, tableName)
            {
            }

            /// <summary>
            ///     更新查询SQL数据成员
            /// </summary>
            internal TmphMemberMap SelectMemberMap
            {
                get { return selectMemberMap; }
            }

            /// <summary>
            ///     自增字段名称
            /// </summary>
            public string IdentityName
            {
                get
                {
                    var identity = TmphSqlModel<TModelType>.Identity;
                    return identity != null ? identity.Field.Name : null;
                }
            }

            /// <summary>
            ///     获取成员位图
            /// </summary>
            /// <param name="memberMap">成员位图</param>
            /// <returns>成员位图</returns>
            internal TmphMemberMap GetMemberMapClearIdentity(TmphMemberMap memberMap)
            {
                var value = TmphSqlModel<TModelType>.CopyMemberMap;
                if (memberMap != null && !memberMap.IsDefault) value.And(memberMap);
                if (TmphSqlModel<TModelType>.Identity != null)
                    value.ClearMember(TmphSqlModel<TModelType>.Identity.MemberMapIndex);
                return value;
            }

            /// <summary>
            ///     获取更新查询SQL数据成员
            /// </summary>
            /// <param name="memberMap">查询SQL数据成员</param>
            /// <returns>更新查询SQL数据成员</returns>
            internal TmphMemberMap GetSelectMemberMap(TmphMemberMap memberMap)
            {
                var value = selectMemberMap.Copy();
                value.Or(memberMap);
                return value;
            }

            /// <summary>
            ///     设置更新查询SQL数据成员
            /// </summary>
            /// <param name="memberIndex">数据成员索引</param>
            public void SetSelectMember<TReturnType>(Expression<Func<TModelType, TReturnType>> member)
            {
                selectMemberMap.SetMember(member);
            }

            /// <summary>
            ///     数据更新SQL表达式
            /// </summary>
            /// <param name="TReturnType">表达式类型</param>
            /// <param name="expression">成员表达式</param>
            /// <returns>数据更新SQL表达式</returns>
            public TmphUpdateExpression UpdateExpression<TReturnType>(Expression<Func<TModelType, TReturnType>> expression)
            {
                var value = new TmphUpdateExpression();
                AddUpdateExpression(ref value, expression);
                return value;
            }

            /// <summary>
            ///     数据更新SQL表达式
            /// </summary>
            /// <param name="TReturnType">表达式类型</param>
            /// <param name="expression">成员表达式</param>
            /// <returns>数据更新SQL表达式</returns>
            public TmphUpdateExpression UpdateExpression<TReturnType>(Expression<Func<TModelType, TReturnType>> expression,
                TReturnType updateValue)
            {
                var value = new TmphUpdateExpression();
                AddUpdateExpression(ref value, expression, updateValue);
                return value;
            }

            /// <summary>
            ///     数据更新SQL表达式
            /// </summary>
            /// <typeparam name="TReturnType"></typeparam>
            /// <param name="value"></param>
            /// <param name="expression"></param>
            public void AddUpdateExpression<TReturnType>(ref TmphUpdateExpression value,
                Expression<Func<TModelType, TReturnType>> expression)
            {
                if (expression != null)
                {
                    var sql = Client.GetSql(expression);
                    if (sql.Key == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                    value.Add(sql);
                }
            }

            /// <summary>
            ///     数据更新SQL表达式
            /// </summary>
            /// <typeparam name="TReturnType"></typeparam>
            /// <param name="value"></param>
            /// <param name="expression"></param>
            public void AddUpdateExpression<TReturnType>(ref TmphUpdateExpression value,
                Expression<Func<TModelType, TReturnType>> expression, TReturnType updateValue)
            {
                if (expression != null)
                {
                    var sql = Client.GetSql(expression);
                    if (sql.Key == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                    value.Add(new TmphKeyValue<string, string>(sql.Key, Client.ToString(updateValue)));
                }
            }

            /// <summary>
            ///     数据更新SQL表达式
            /// </summary>
            /// <typeparam name="TReturnType"></typeparam>
            /// <param name="value"></param>
            /// <param name="expression"></param>
            public void AddUpdateExpression(ref TmphUpdateExpression value, Expression<Action<TModelType>> expression)
            {
                if (expression != null)
                {
                    var sql = Client.GetSql(expression);
                    if (sql.Key == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                    value.Add(sql);
                }
            }

            /// <summary>
            ///     数据更新SQL表达式
            /// </summary>
            /// <param name="TReturnType">表达式类型</param>
            /// <param name="expressions">成员表达式集合</param>
            /// <returns>数据更新SQL表达式</returns>
            public TmphUpdateExpression UpdateExpression<TReturnType>(
                params Expression<Func<TModelType, TReturnType>>[] expressions)
            {
                var value = new TmphUpdateExpression();
                AddUpdateExpression(ref value, expressions);
                return value;
            }

            /// <summary>
            ///     数据更新SQL表达式
            /// </summary>
            /// <typeparam name="TReturnType"></typeparam>
            /// <param name="value"></param>
            /// <param name="expressions"></param>
            public void AddUpdateExpression<TReturnType>(ref TmphUpdateExpression value,
                params Expression<Func<TModelType, TReturnType>>[] expressions)
            {
                foreach (var expression in expressions) AddUpdateExpression(ref value, expression);
            }

            /// <summary>
            ///     释放资源
            /// </summary>
            public override void Dispose()
            {
                base.Dispose();
                CLB.TmphPub.Dispose(ref selectMemberMap);
            }
        }

        /// <summary>
        ///     数据库表格操作工具
        /// </summary>
        /// <typeparam name="TValueType">表格类型</typeparam>
        /// <typeparam name="TModelType">模型类型</typeparam>
        public abstract class TmphSqlTool<TValueType, TModelType> : TmphSqlTool<TModelType>
            where TValueType : class, TModelType
            where TModelType : class
        {
            /// <summary>
            ///     数据库表格操作工具
            /// </summary>
            /// <param name="sqlTable">数据库表格配置</param>
            /// <param name="TmphClient">SQL操作客户端</param>
            /// <param name="tableName">表格名称</param>
            protected TmphSqlTool(TmphSqlTable sqlTable, TmphClient TmphClient, string tableName)
                : base(sqlTable, TmphClient, tableName)
            {
                TmphConnection.WaitCheckConnection(typeof(TValueType));
                try
                {
                    var table = TmphClient.GetTable(tableName, null);
                    if (table == null)
                    {
                        var sqlModelType = TmphDataModel.GetModelType<Code.CSharp.TmphSqlModel>(typeof(TValueType)) ??
                                           typeof(TValueType);
                        var memberTable = TmphSqlModel<TModelType>.GetTable(typeof(TValueType), sqlTable);
                        TmphClient.ToSqlColumn(memberTable);
                        if (TmphClient.CreateTable(memberTable, null)) table = memberTable;
                    }
                    var names = ignoreCase
                        ? table.Columns.Columns.getArray(value => value.Name.ToLower())
                        : table.Columns.Columns.getArray(value => value.Name);
                    noIndexMemberNames = new TmphStateSearcher.TmphAscii<string>(names, names);
                    if (table.Indexs != null)
                    {
                        foreach (var column in table.Indexs)
                            noIndexMemberNames.Remove(ignoreCase
                                ? column.Columns[0].Name.ToLower()
                                : column.Columns[0].Name);
                    }
                    if (table.PrimaryKey != null)
                        noIndexMemberNames.Remove(ignoreCase
                            ? table.PrimaryKey.Columns[0].Name.ToLower()
                            : table.PrimaryKey.Columns[0].Name);
                    isTable = true;
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, tableName, false);
                }
                if (IsTable)
                {
                    if (TmphSqlModel<TModelType>.Identity != null)
                    {
                        var identityName = TmphSqlModel<TModelType>.Identity.Field.Name;
                        if (TmphClient.IsIndex) createIndex(identityName, true);
                        selectMemberMap.SetMember(identityName);
                        if (sqlTable.IsLoadIdentity)
                        {
                            var identityConvertible = TmphClient.getValue<TValueType, TModelType, IConvertible>(this,
                                new TmphSelectQuery<TModelType>
                                {
                                    StringOrders = new[] { new TmphKeyValue<string, bool>(identityName, true) }
                                },
                                identityName, null);
                            identity64 = identityConvertible == null ? 0 : identityConvertible.ToInt64(null);
                        }
                    }
                    foreach (var field in TmphSqlModel<TModelType>.PrimaryKeys)
                    {
                        selectMemberMap.SetMember(field.MemberMapIndex);
                    }
                }
            }

            /// <summary>
            ///     添加数据是否启用应用程序事务
            /// </summary>
            internal bool IsInsertTransaction
            {
                get { return OnInsertedLock != null || OnInserted != null; }
            }

            /// <summary>
            ///     添加数据是否启用应用程序事务
            /// </summary>
            internal bool IsUpdateTransaction
            {
                get { return OnUpdatedLock != null || OnUpdated != null; }
            }

            /// <summary>
            ///     删除数据是否启用应用程序事务
            /// </summary>
            internal bool IsDeleteTransaction
            {
                get { return OnDeletedLock != null || OnDeleted != null; }
            }

            /// <summary>
            ///     添加数据之前的验证事件
            /// </summary>
            public event Action<TValueType, TmphCancel> OnInsert;

            /// <summary>
            ///     添加数据之前的验证事件
            /// </summary>
            /// <param name="value">待插入数据</param>
            /// <returns>是否可插入数据库</returns>
            internal bool CallOnInsert(TValueType value)
            {
                if (OnInsert != null)
                {
                    var cancel = new TmphCancel();
                    OnInsert(value, cancel);
                    return !cancel.IsCancel;
                }
                return true;
            }

            /// <summary>
            ///     添加数据之前的验证事件
            /// </summary>
            /// <param name="values">待插入数据集合</param>
            /// <returns>是否可插入数据库</returns>
            internal bool CallOnInsert(TValueType[] values)
            {
                if (OnInsert != null)
                {
                    var cancel = new TmphCancel();
                    foreach (var value in values)
                    {
                        OnInsert(value, cancel);
                        if (cancel.IsCancel) return false;
                    }
                }
                return true;
            }

            /// <summary>
            ///     添加数据之前的验证事件
            /// </summary>
            public event Action<TValueType, TmphCancel> OnInsertLock;

            /// <summary>
            ///     添加数据之前的验证事件
            /// </summary>
            /// <param name="value">待插入数据</param>
            /// <returns>是否可插入数据库</returns>
            internal bool CallOnInsertLock(TValueType value)
            {
                if (OnInsertLock != null)
                {
                    var cancel = new TmphCancel();
                    OnInsertLock(value, cancel);
                    return !cancel.IsCancel;
                }
                return true;
            }

            /// <summary>
            ///     添加数据之前的验证事件
            /// </summary>
            /// <param name="values">待插入数据集合</param>
            /// <returns>是否可插入数据库</returns>
            internal bool CallOnInsertLock(TValueType[] values)
            {
                if (OnInsertLock != null)
                {
                    var cancel = new TmphCancel();
                    foreach (var value in values)
                    {
                        OnInsertLock(value, cancel);
                        if (cancel.IsCancel) return false;
                    }
                }
                return true;
            }

            /// <summary>
            ///     添加数据之后的处理事件
            /// </summary>
            public event Action<TValueType> OnInsertedLock;

            /// <summary>
            ///     添加数据之后的处理事件
            /// </summary>
            /// <param name="value">被插入的数据</param>
            internal void CallOnInsertedLock(TValueType value)
            {
                if (OnInsertedLock != null) OnInsertedLock(value);
            }

            /// <summary>
            ///     添加数据之后的处理事件
            /// </summary>
            public event Action<TValueType> OnInserted;

            /// <summary>
            ///     添加数据之后的处理事件
            /// </summary>
            /// <param name="value">被插入的数据</param>
            internal void CallOnInserted(TValueType value)
            {
                if (OnInserted != null) OnInserted(value);
            }

            /// <summary>
            ///     更新数据之前的验证事件
            /// </summary>
            public event Action<TValueType, TmphMemberMap, TmphCancel> OnUpdate;

            /// <summary>
            ///     更新数据之前的验证事件
            /// </summary>
            /// <param name="value">待更新数据</param>
            /// <param name="memberMap">更新成员位图</param>
            /// <returns>是否可更新数据库</returns>
            internal bool CallOnUpdate(TValueType value, TmphMemberMap memberMap)
            {
                if (OnUpdate != null)
                {
                    var cancel = new TmphCancel();
                    OnUpdate(value, memberMap, cancel);
                    return !cancel.IsCancel;
                }
                return true;
            }

            /// <summary>
            ///     更新数据之前的验证事件
            /// </summary>
            public event Action<TValueType, TmphMemberMap, TmphCancel> OnUpdateLock;

            /// <summary>
            ///     更新数据之前的验证事件
            /// </summary>
            /// <param name="value">待更新数据</param>
            /// <param name="memberMap">更新成员位图</param>
            /// <returns>是否可更新数据库</returns>
            internal bool CallOnUpdateLock(TValueType value, TmphMemberMap memberMap)
            {
                if (OnUpdateLock != null)
                {
                    var cancel = new TmphCancel();
                    OnUpdateLock(value, memberMap, cancel);
                    return !cancel.IsCancel;
                }
                return true;
            }

            /// <summary>
            ///     更新数据之后的处理事件
            /// </summary>
            public event Action<TValueType, TValueType, TmphMemberMap> OnUpdatedLock;

            /// <summary>
            ///     更新数据之后的处理事件
            /// </summary>
            /// <param name="value">更新后的数据</param>
            /// <param name="oldValue">更新前的数据</param>
            /// <param name="memberMap">更新成员位图</param>
            internal void CallOnUpdatedLock(TValueType value, TValueType oldValue, TmphMemberMap memberMap)
            {
                if (OnUpdatedLock != null) OnUpdatedLock(value, oldValue, memberMap);
            }

            /// <summary>
            ///     更新数据之后的处理事件
            /// </summary>
            public event Action<TValueType, TValueType, TmphMemberMap> OnUpdated;

            /// <summary>
            ///     更新数据之后的处理事件
            /// </summary>
            /// <param name="value">更新后的数据</param>
            /// <param name="oldValue">更新前的数据</param>
            /// <param name="memberMap">更新成员位图</param>
            internal void CallOnUpdated(TValueType value, TValueType oldValue, TmphMemberMap memberMap)
            {
                if (OnUpdated != null) OnUpdated(value, oldValue, memberMap);
            }

            /// <summary>
            ///     删除数据之前的验证事件
            /// </summary>
            public event Action<TValueType, TmphCancel> OnDelete;

            /// <summary>
            ///     删除数据之前的验证事件
            /// </summary>
            /// <param name="value">待删除数据</param>
            /// <returns>是否可删除数据</returns>
            internal bool CallOnDelete(TValueType value)
            {
                if (OnDelete != null)
                {
                    var cancel = new TmphCancel();
                    OnDelete(value, cancel);
                    return !cancel.IsCancel;
                }
                return true;
            }

            /// <summary>
            ///     删除数据之前的验证事件
            /// </summary>
            public event Action<TValueType, TmphCancel> OnDeleteLock;

            /// <summary>
            ///     删除数据之前的验证事件
            /// </summary>
            /// <param name="value">待删除数据</param>
            /// <returns>是否可删除数据</returns>
            internal bool CallOnDeleteLock(TValueType value)
            {
                if (OnDeleteLock != null)
                {
                    var cancel = new TmphCancel();
                    OnDeleteLock(value, cancel);
                    return !cancel.IsCancel;
                }
                return true;
            }

            /// <summary>
            ///     删除数据之后的处理事件
            /// </summary>
            public event Action<TValueType> OnDeletedLock;

            /// <summary>
            ///     删除数据之后的处理事件
            /// </summary>
            /// <param name="value">被删除的数据</param>
            internal void CallOnDeletedLock(TValueType value)
            {
                if (OnDeletedLock != null) OnDeletedLock(value);
            }

            /// <summary>
            ///     删除数据之后的处理事件
            /// </summary>
            public event Action<TValueType> OnDeleted;

            /// <summary>
            ///     删除数据之后的处理事件
            /// </summary>
            /// <param name="value">被删除的数据</param>
            internal void CallOnDeleted(TValueType value)
            {
                if (OnDeleted != null) OnDeleted(value);
            }

            /// <summary>
            ///     数据集合转DataTable
            /// </summary>
            /// <typeparam name="TValueType">数据类型</typeparam>
            /// <param name="values">数据集合</param>
            /// <returns>数据集合</returns>
            internal DataTable GetDataTable(TValueType[] values)
            {
                var dataTable = new DataTable(TableName);
                foreach (var column in TmphSqlModel<TModelType>.TmphToArray.DataColumns)
                    dataTable.Columns.Add(new DataColumn(column.Key, column.Value));
                foreach (var value in values)
                {
                    var memberValues = new object[dataTable.Columns.Count];
                    var index = 0;
                    TmphSqlModel<TModelType>.TmphToArray.ToArray(value, memberValues, ref index);
                    dataTable.Rows.Add(memberValues);
                }
                return dataTable;
            }

            /// <summary>
            ///     查询数据集合
            /// </summary>
            /// <param name="expression">查询条件表达式</param>
            /// <param name="memberMap">成员位图</param>
            /// <returns>数据集合</returns>
            public IEnumerable<TValueType> Where(Expression<Func<TModelType, bool>> expression = null,
                TmphMemberMap memberMap = null)
            {
                return Client.Select(this, expression, memberMap);
            }

            /// <summary>
            ///     查询数据集合
            /// </summary>
            /// <param name="query">查询信息</param>
            /// <param name="memberMap">成员位图</param>
            /// <returns>数据集合</returns>
            public IEnumerable<TValueType> Where(TmphSelectQuery<TModelType> query, TmphMemberMap memberMap = null)
            {
                return Client.Select(this, query, memberMap);
            }

            /// <summary>
            ///     查询数据集合
            /// </summary>
            /// <param name="expression">查询条件表达式</param>
            /// <returns>数据集合,失败返回-1</returns>
            public int Count(Expression<Func<TModelType, bool>> expression = null)
            {
                return Client.Count(this, expression);
            }

            /// <summary>
            ///     添加到数据库
            /// </summary>
            /// <param name="value">待添加数据</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <param name="memberMap">成员位图</param>
            /// <returns>添加是否成功</returns>
            public bool Insert(TValueType value, bool isIgnoreTransaction = false, TmphMemberMap memberMap = null)
            {
                return Client.Insert(this, value, memberMap, isIgnoreTransaction);
            }

            /// <summary>
            ///     添加到数据库
            /// </summary>
            /// <param name="values">数据集合</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>添加是否成功</returns>
            public bool Insert(TValueType[] values, bool isIgnoreTransaction = false)
            {
                return values.length() != 0 && Client.Insert(this, values, isIgnoreTransaction) != null;
            }

            /// <summary>
            ///     根据自增id获取数据对象
            /// </summary>
            /// <typeparam name="TValueType">数据类型</typeparam>
            /// <param name="value">关键字数据对象</param>
            /// <param name="memberMap">成员位图</param>
            /// <returns>数据对象</returns>
            public TValueType GetByIdentity(TValueType value, TmphMemberMap memberMap = null)
            {
                if (TmphSqlModel<TModelType>.Identity == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
                return Client.GetByIdentity(this, value, memberMap);
            }

            /// <summary>
            ///     获取数据库记录
            /// </summary>
            /// <param name="identity">自增id</param>
            /// <param name="memberMap">成员位图</param>
            /// <returns>数据对象</returns>
            public TValueType GetByIdentity(long identity, TmphMemberMap memberMap = null)
            {
                var value = TmphConstructor<TValueType>.New();
                TmphSqlModel<TModelType>.SetIdentity(value, identity);
                return GetByIdentity(value, memberMap);
            }

            /// <summary>
            ///     获取数据库记录
            /// </summary>
            /// <param name="identity">自增id</param>
            /// <param name="memberMap">成员位图</param>
            /// <returns>数据对象</returns>
            public TValueType GetByIdentity(int identity, TmphMemberMap memberMap = null)
            {
                return GetByIdentity((long)identity, memberMap);
            }

            /// <summary>
            ///     根据关键字获取数据对象
            /// </summary>
            /// <typeparam name="TValueType">数据类型</typeparam>
            /// <param name="value">关键字数据对象</param>
            /// <param name="memberMap">成员位图</param>
            /// <returns>数据对象</returns>
            public TValueType GetByPrimaryKey(TValueType value, TmphMemberMap memberMap = null)
            {
                if (TmphSqlModel<TModelType>.PrimaryKeys.Length == 0)
                    TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
                return Client.GetByPrimaryKey(this, value, memberMap);
            }

            /// <summary>
            ///     修改数据库记录
            /// </summary>
            /// <param name="value">待修改数据</param>
            /// <param name="memberMap">成员位图</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>是否修改成功</returns>
            public bool UpdateByIdentity(TValueType value, TmphMemberMap memberMap = null, bool isIgnoreTransaction = false)
            {
                if (TmphSqlModel<TModelType>.Identity == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
                return Client.UpdateByIdentity(this, value, memberMap, isIgnoreTransaction);
            }

            /// <summary>
            ///     修改数据库记录
            /// </summary>
            /// <param name="value">待修改数据</param>
            /// <param name="memberMap">成员位图</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>是否修改成功</returns>
            public bool UpdateByPrimaryKey(TValueType value, TmphMemberMap memberMap = null,
                bool isIgnoreTransaction = false)
            {
                if (TmphSqlModel<TModelType>.PrimaryKeys.Length == 0)
                    TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
                return Client.Update(this, value, memberMap, isIgnoreTransaction);
            }

            /// <summary>
            ///     删除数据库记录
            /// </summary>
            /// <param name="value">待删除数据</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>是否成功</returns>
            public bool DeleteByIdentity(TValueType value, bool isIgnoreTransaction = false)
            {
                if (TmphSqlModel<TModelType>.Identity == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
                return Client.DeleteByIdentity(this, value, isIgnoreTransaction);
            }

            /// <summary>
            ///     删除数据库记录
            /// </summary>
            /// <param name="identity"></param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>是否成功</returns>
            public bool DeleteByIdentity(long identity, bool isIgnoreTransaction = false)
            {
                if (TmphSqlModel<TModelType>.Identity == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
                var value = TmphConstructor<TValueType>.New();
                TmphSqlModel<TModelType>.SetIdentity(value, identity);
                return Client.DeleteByIdentity(this, value, isIgnoreTransaction);
            }

            /// <summary>
            ///     删除数据库记录
            /// </summary>
            /// <param name="identity"></param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>是否成功</returns>
            public bool DeleteByIdentity(int identity, bool isIgnoreTransaction = false)
            {
                return DeleteByIdentity((long)identity, isIgnoreTransaction);
            }

            /// <summary>
            ///     删除数据库记录
            /// </summary>
            /// <param name="value">待删除数据</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>是否成功</returns>
            public bool DeleteByPrimaryKey(TValueType value, bool isIgnoreTransaction = false)
            {
                if (TmphSqlModel<TModelType>.PrimaryKeys.Length == 0)
                    TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
                return Client.Delete(this, value, isIgnoreTransaction);
            }

            /// <summary>
            ///     修改数据库记录
            /// </summary>
            /// <param name="identity">自增id</param>
            /// <param name="sqlExpression">SQL表达式</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public TValueType UpdateByIdentity(long identity, TmphUpdateExpression updateExpression,
                bool isIgnoreTransaction = false)
            {
                if (updateExpression.Count != 0)
                {
                    var value = TmphConstructor<TValueType>.New();
                    TmphSqlModel<TModelType>.SetIdentity(value, identity);
                    if (Client.UpdateByIdentity(this, value, updateExpression, isIgnoreTransaction)) return value;
                }
                return null;
            }

            /// <summary>
            ///     修改数据库记录
            /// </summary>
            /// <param name="identity">自增id</param>
            /// <param name="sqlExpression">SQL表达式</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public TValueType UpdateByIdentity(int identity, TmphUpdateExpression updateExpression,
                bool isIgnoreTransaction = false)
            {
                return UpdateByIdentity((long)identity, updateExpression, isIgnoreTransaction);
            }

            /// <summary>
            ///     修改数据库记录
            /// </summary>
            /// <param name="identity">自增id</param>
            /// <param name="expression">SQL表达式</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public TValueType UpdateByIdentity<TReturnType>(long identity,
                Expression<Func<TModelType, TReturnType>> expression, bool isIgnoreTransaction = false)
            {
                return expression != null
                    ? UpdateByIdentity(identity, UpdateExpression(expression), isIgnoreTransaction)
                    : null;
            }

            /// <summary>
            ///     修改数据库记录
            /// </summary>
            /// <param name="identity">自增id</param>
            /// <param name="expression">SQL表达式</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public TValueType UpdateByIdentity<TReturnType>(long identity,
                Expression<Func<TModelType, TReturnType>> expression, TReturnType returnValue,
                bool isIgnoreTransaction = false)
            {
                return expression != null
                    ? UpdateByIdentity(identity, UpdateExpression(expression, returnValue), isIgnoreTransaction)
                    : null;
            }

            /// <summary>
            ///     修改数据库记录
            /// </summary>
            /// <param name="identity">自增id</param>
            /// <param name="expression">SQL表达式</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public TValueType UpdateByIdentity<TReturnType>(int identity,
                Expression<Func<TModelType, TReturnType>> expression, bool isIgnoreTransaction = false)
            {
                return UpdateByIdentity((long)identity, expression, isIgnoreTransaction);
            }

            /// <summary>
            ///     修改数据库记录
            /// </summary>
            /// <param name="identity">自增id</param>
            /// <param name="expression">SQL表达式</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public TValueType UpdateByIdentity<TReturnType>(int identity,
                Expression<Func<TModelType, TReturnType>> expression, TReturnType returnValue,
                bool isIgnoreTransaction = false)
            {
                return UpdateByIdentity((long)identity, expression, returnValue, isIgnoreTransaction);
            }

            /// <summary>
            ///     修改数据库记录
            /// </summary>
            /// <param name="identity">自增id</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <param name="expressions">SQL表达式</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public TValueType UpdateByIdentity<TReturnType>(long identity, bool isIgnoreTransaction,
                params Expression<Func<TModelType, TReturnType>>[] expressions)
            {
                return expressions.Length != 0
                    ? UpdateByIdentity(identity, UpdateExpression(expressions), isIgnoreTransaction)
                    : null;
            }

            /// <summary>
            ///     修改数据库记录
            /// </summary>
            /// <param name="identity">自增id</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <param name="expressions">SQL表达式</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public TValueType UpdateByIdentity<TReturnType>(int identity, bool isIgnoreTransaction,
                params Expression<Func<TModelType, TReturnType>>[] expressions)
            {
                return UpdateByIdentity((long)identity, isIgnoreTransaction, expressions);
            }

            /// <summary>
            ///     修改数据库记录
            /// </summary>
            /// <param name="primaryKey">SQL关键字</param>
            /// <param name="updateExpression">SQL表达式</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public TValueType UpdateByPrimaryKey(TValueType value, TmphUpdateExpression updateExpression,
                bool isIgnoreTransaction = false)
            {
                if (updateExpression.Count != 0)
                {
                    if (Client.Update(this, value, updateExpression, isIgnoreTransaction)) return value;
                }
                return null;
            }

            /// <summary>
            ///     修改数据库记录
            /// </summary>
            /// <param name="primaryKey">SQL关键字</param>
            /// <param name="updateExpression">SQL表达式</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public TValueType UpdateByPrimaryKey<TReturnType>(TValueType value,
                Expression<Func<TModelType, TReturnType>> expression, bool isIgnoreTransaction = false)
            {
                return expression != null
                    ? UpdateByPrimaryKey(value, UpdateExpression(expression), isIgnoreTransaction)
                    : null;
            }

            /// <summary>
            ///     修改数据库记录
            /// </summary>
            /// <param name="primaryKey">SQL关键字</param>
            /// <param name="updateExpression">SQL表达式</param>
            /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public TValueType UpdateByPrimaryKey<TReturnType>(TValueType value,
                Expression<Func<TModelType, TReturnType>> expression, TReturnType returnValue,
                bool isIgnoreTransaction = false)
            {
                return expression != null
                    ? UpdateByPrimaryKey(value, UpdateExpression(expression, returnValue), isIgnoreTransaction)
                    : null;
            }

            /// <summary>
            ///     修改数据库记录
            /// </summary>
            /// <param name="primaryKey">SQL关键字</param>
            /// <param name="updateExpression">SQL表达式</param>
            /// <param name="expressions">是否忽略应用程序事务</param>
            /// <returns>修改后的数据,失败返回null</returns>
            public TValueType UpdateByPrimaryKey<TReturnType>(TValueType value, bool isIgnoreTransaction,
                params Expression<Func<TModelType, TReturnType>>[] expressions)
            {
                return expressions.Length != 0
                    ? UpdateByPrimaryKey(value, UpdateExpression(expressions), isIgnoreTransaction)
                    : null;
            }
        }

        /// <summary>
        ///     JSON操作客户端
        /// </summary>
        public sealed class TmphJsonTool : IDisposable
        {
            /// <summary>
            ///     JSON解析配置参数
            /// </summary>
            private static readonly TmphJsonParser.TmphConfig jsonConfig = new TmphJsonParser.TmphConfig
            {
                IsGetJson = false,
                IsTempString = true
            };

            /// <summary>
            ///     JSON解析配置参数访问锁
            /// </summary>
            private static readonly object jsonConfigLock = new object();

            /// <summary>
            ///     数据库表格操作工具集合
            /// </summary>
            private readonly ISqlTable[] sqlTables;

            /// <summary>
            ///     类型名称索引查找数据
            /// </summary>
            private TmphPointer searcher;

            /// <summary>
            ///     JSON操作客户端
            /// </summary>
            /// <param name="assembly">数据库表格相关程序集</param>
            public TmphJsonTool(Assembly assembly)
            {
                if (assembly == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                var sqlTables = default(TmphSubArray<TmphKeyValue<string, ISqlTable>>);
                foreach (var type in assembly.GetTypes())
                {
                    var attribute = TmphTypeAttribute.GetAttribute<TmphSqlTable>(type, false, true);
                    if (attribute != null &&
                        Array.IndexOf(TmphSql.Default.CheckConnection, attribute.ConnectionType) != -1)
                    {
                        var sqlTable = typeof(sqlTable<,>).MakeGenericType(type,
                            TmphDataModel.GetModelType<Code.CSharp.TmphSqlModel>(type) ?? type)
                            .GetMethod("get", BindingFlags.Static | BindingFlags.NonPublic)
                            .Invoke(null, null);
                        if (sqlTable != null)
                            sqlTables.Add(new TmphKeyValue<string, ISqlTable>(type.fullName(), (ISqlTable)sqlTable));
                    }
                }
                if (sqlTables.Count != 0)
                {
                    sqlTables.Sort((left, right) => left.Key.CompareTo(right.Key));
                    this.sqlTables = sqlTables.GetArray(value => value.Value);
                    searcher = TmphStateSearcher.TmphChars.Create(Names = sqlTables.GetArray(value => value.Key));
                }
            }

            /// <summary>
            ///     类型名称集合
            /// </summary>
            public string[] Names { get; private set; }

            /// <summary>
            ///     释放资源
            /// </summary>
            public void Dispose()
            {
                TmphUnmanaged.Free(ref searcher);
            }

            /// <summary>
            ///     获取数据库表格操作工具
            /// </summary>
            /// <param name="type"></param>
            /// <returns>数据库表格操作工具</returns>
            private ISqlTable get(string type)
            {
                var index = new TmphStateSearcher.TmphChars(searcher).Search(type);
                return index >= 0 ? sqlTables[index] : null;
            }

            /// <summary>
            ///     获取字段名称集合
            /// </summary>
            /// <param name="type"></param>
            /// <param name="identityName">自增字段名称</param>
            /// <param name="primaryKeyNames">关键字名称集合</param>
            /// <returns>字段名称集合</returns>
            public string[] GetFields(string type, ref string identityName, ref string[] primaryKeyNames)
            {
                var sqlTable = get(type);
                if (sqlTable != null)
                {
                    identityName = sqlTable.IdentityName;
                    primaryKeyNames = sqlTable.PrimaryKeyNames;
                    return sqlTable.FieldNames;
                }
                return null;
            }

            /// <summary>
            ///     根据JSON查询数据
            /// </summary>
            /// <param name="type"></param>
            /// <param name="json"></param>
            /// <returns>查询数据的JSON字符串</returns>
            public TmphKeyValue<string, string>[] Query(string type, string json)
            {
                var sqlTable = get(type);
                return sqlTable != null ? sqlTable.Query(json) : null;
            }

            /// <summary>
            ///     根据JSON更新数据
            /// </summary>
            /// <param name="type"></param>
            /// <param name="json"></param>
            /// <param name="values"></param>
            /// <returns>更新是否成功</returns>
            public bool Update(string type, string json, TmphKeyValue<string, string>[] values)
            {
                var sqlTable = get(type);
                return sqlTable != null && sqlTable.Update(json, values);
            }

            /// <summary>
            ///     根据JSON更新数据
            /// </summary>
            /// <param name="type"></param>
            /// <param name="json"></param>
            /// <returns>更新是否成功</returns>
            public bool Delete(string type, string json)
            {
                var sqlTable = get(type);
                return sqlTable != null && sqlTable.Delete(json);
            }

            /// <summary>
            ///     数据库表格操作工具
            /// </summary>
            private interface ISqlTable
            {
                /// <summary>
                ///     字段名称集合
                /// </summary>
                string[] FieldNames { get; }

                /// <summary>
                ///     自增字段名称
                /// </summary>
                string IdentityName { get; }

                /// <summary>
                ///     关键字名称集合
                /// </summary>
                string[] PrimaryKeyNames { get; }

                /// <summary>
                ///     根据JSON查询数据
                /// </summary>
                /// <param name="json"></param>
                /// <returns>查询数据的JSON字符串</returns>
                TmphKeyValue<string, string>[] Query(string json);

                /// <summary>
                ///     根据JSON更新数据
                /// </summary>
                /// <param name="json"></param>
                /// <param name="values"></param>
                /// <returns>更新是否成功</returns>
                bool Update(string json, TmphKeyValue<string, string>[] values);

                /// <summary>
                ///     根据JSON更新数据
                /// </summary>
                /// <param name="json"></param>
                /// <returns>更新是否成功</returns>
                bool Delete(string json);
            }

            /// <summary>
            ///     数据库表格操作工具
            /// </summary>
            /// <typeparam name="TValueType"></typeparam>
            /// <typeparam name="TModelType"></typeparam>
            private sealed class sqlTable<TValueType, TModelType> : ISqlTable
                where TValueType : class, TModelType
                where TModelType : class
            {
                /// <summary>
                ///     JSON解析成员位图
                /// </summary>
                private static readonly TmphMemberMap jsonMemberMap = TmphMemberMap<TModelType>.New();

                /// <summary>
                ///     JSON解析成员位图参数访问锁
                /// </summary>
                private static readonly object jsonMemberMapLock = new object();

                /// <summary>
                ///     成员名称索引查找数据
                /// </summary>
                private static readonly TmphPointer searcher =
                    TmphStateSearcher.TmphChars.Create(TmphSqlModel<TModelType>.Fields.getArray(value => value.Field.Name));

                /// <summary>
                ///     数据库表格操作工具
                /// </summary>
                private readonly TmphSqlTool<TValueType, TModelType> sqlTool;

                /// <summary>
                ///     数据库表格操作工具
                /// </summary>
                /// <param name="sqlTool">数据库表格操作工具</param>
                private sqlTable(TmphSqlTool<TValueType, TModelType> sqlTool)
                {
                    this.sqlTool = sqlTool;
                }

                /// <summary>
                ///     字段名称集合
                /// </summary>
                public string[] FieldNames
                {
                    get { return TmphSqlModel<TModelType>.Fields.getArray(value => value.Field.Name); }
                }

                /// <summary>
                ///     自增字段名称
                /// </summary>
                public string IdentityName
                {
                    get
                    {
                        return TmphSqlModel<TModelType>.Identity == null ? null : TmphSqlModel<TModelType>.Identity.Field.Name;
                    }
                }

                /// <summary>
                ///     关键字名称集合
                /// </summary>
                public string[] PrimaryKeyNames
                {
                    get { return TmphSqlModel<TModelType>.PrimaryKeys.getArray(value => value.Field.Name); }
                }

                /// <summary>
                ///     根据JSON查询数据
                /// </summary>
                /// <param name="json"></param>
                /// <returns>查询数据的JSON字符串</returns>
                public TmphKeyValue<string, string>[] Query(string json)
                {
                    var value = TmphConstructor<TValueType>.New();
                    Monitor.Enter(jsonMemberMapLock);
                    try
                    {
                        if (parseJson(value, json))
                        {
                            if (TmphSqlModel<TModelType>.Identity != null &&
                                jsonMemberMap.IsMember(TmphSqlModel<TModelType>.Identity.MemberMapIndex))
                            {
                                value = sqlTool.GetByIdentity(value);
                            }
                            else if (TmphSqlModel<TModelType>.PrimaryKeys.Length != 0)
                                value = sqlTool.GetByPrimaryKey(value);
                            else value = null;
                        }
                        else value = null;
                    }
                    finally
                    {
                        jsonMemberMap.Clear();
                        Monitor.Exit(jsonMemberMapLock);
                    }
                    if (value != null)
                    {
                        return
                            TmphSqlModel<TModelType>.Fields.getArray(
                                field =>
                                    new TmphKeyValue<string, string>(field.Field.Name,
                                        TmphJsonSerializer.ObjectToJson(field.Field.GetValue(value))));
                    }
                    return null;
                }

                /// <summary>
                ///     根据JSON更新数据
                /// </summary>
                /// <param name="json"></param>
                /// <param name="values"></param>
                /// <returns>更新是否成功</returns>
                public bool Update(string json, TmphKeyValue<string, string>[] values)
                {
                    var value = TmphConstructor<TValueType>.New();
                    var isUpdate = 0;
                    Monitor.Enter(jsonMemberMapLock);
                    try
                    {
                        if (parseJson(value, json))
                        {
                            if (TmphSqlModel<TModelType>.Identity != null &&
                                jsonMemberMap.IsMember(TmphSqlModel<TModelType>.Identity.MemberMapIndex))
                                isUpdate = 1;
                            else if (TmphSqlModel<TModelType>.PrimaryKeys.Length != 0) isUpdate = 2;
                            if (isUpdate != 0)
                            {
                                var nameSearcher = new TmphStateSearcher.TmphChars(searcher);
                                foreach (var nameValue in values)
                                {
                                    var index = nameSearcher.Search(nameValue.Key);
                                    if (index != -1)
                                    {
                                        var field = TmphSqlModel<TModelType>.Fields[index];
                                        field.Field.SetValue(value,
                                            TmphJsonParser.ParseType(field.Field.FieldType, nameValue.Value));
                                        jsonMemberMap.SetMember(field.MemberMapIndex);
                                    }
                                }
                                return isUpdate == 1
                                    ? sqlTool.UpdateByIdentity(value, jsonMemberMap)
                                    : sqlTool.UpdateByPrimaryKey(value, jsonMemberMap);
                            }
                        }
                    }
                    finally
                    {
                        jsonMemberMap.Clear();
                        Monitor.Exit(jsonMemberMapLock);
                    }
                    return false;
                }

                /// <summary>
                ///     根据JSON更新数据
                /// </summary>
                /// <param name="json"></param>
                /// <returns>更新是否成功</returns>
                public bool Delete(string json)
                {
                    var value = TmphConstructor<TValueType>.New();
                    bool isIdentity = false, isPrimaryKey = false;
                    Monitor.Enter(jsonMemberMapLock);
                    try
                    {
                        if (parseJson(value, json))
                        {
                            if (TmphSqlModel<TModelType>.Identity != null &&
                                jsonMemberMap.IsMember(TmphSqlModel<TModelType>.Identity.MemberMapIndex))
                            {
                                isIdentity = true;
                            }
                            else if (TmphSqlModel<TModelType>.PrimaryKeys.Length != 0) isPrimaryKey = true;
                        }
                    }
                    finally
                    {
                        jsonMemberMap.Clear();
                        Monitor.Exit(jsonMemberMapLock);
                    }
                    if (isIdentity) return sqlTool.DeleteByIdentity(value);
                    if (isPrimaryKey) return sqlTool.DeleteByPrimaryKey(value);
                    return false;
                }

                /// <summary>
                ///     JSON解析
                /// </summary>
                /// <param name="value"></param>
                /// <param name="json"></param>
                /// <returns></returns>
                private bool parseJson(TModelType value, string json)
                {
                    Monitor.Enter(jsonConfig);
                    try
                    {
                        jsonConfig.MemberMap = jsonMemberMap;
                        return TmphJsonParser.Parse(json, ref value, jsonConfig);
                    }
                    finally
                    {
                        Monitor.Exit(jsonConfig);
                    }
                }

                /// <summary>
                ///     获取数据库表格操作工具
                /// </summary>
                /// <returns>数据库表格操作工具</returns>
                private static ISqlTable get()
                {
                    if (TmphSqlModel<TModelType>.Identity != null || TmphSqlModel<TModelType>.PrimaryKeys.Length != 0)
                    {
                        var field = typeof(TValueType).GetField(SqlTableName,
                            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.FlattenHierarchy);
                        if (field != null)
                        {
                            var sqlTable = field.GetValue(null) as TmphSqlTool<TValueType, TModelType>;
                            if (sqlTable != null) return new sqlTable<TValueType, TModelType>(sqlTable);
                        }
                    }
                    return null;
                }
            }
        }
    }

    /// <summary>
    ///     数据库表格操作工具
    /// </summary>
    /// <typeparam name="TValueType">表格类型</typeparam>
    /// <typeparam name="TModelType">模型类型</typeparam>
    public class TmphSqlTable<TValueType, TModelType> : TmphSqlTable.TmphSqlTool<TValueType, TModelType>
        where TValueType : class, TModelType
        where TModelType : class
    {
        /// <summary>
        ///     数据库表格操作工具
        /// </summary>
        /// <param name="sqlTable">数据库表格配置</param>
        /// <param name="TmphClient">SQL操作客户端</param>
        /// <param name="tableName">表格名称</param>
        protected TmphSqlTable(TmphSqlTable sqlTable, TmphClient TmphClient, string tableName)
            : base(sqlTable, TmphClient, tableName)
        {
        }

        /// <summary>
        ///     获取数据库表格操作工具
        /// </summary>
        /// <returns>数据库表格操作工具</returns>
        public static TmphSqlTable<TValueType, TModelType> Get()
        {
            var type = typeof(TValueType);
            var sqlTable = TmphTypeAttribute.GetAttribute<TmphSqlTable>(type, false, true);
            if (sqlTable != null && Array.IndexOf(TmphSql.Default.CheckConnection, sqlTable.ConnectionType) != -1)
            {
                return new TmphSqlTable<TValueType, TModelType>(sqlTable,
                    TmphConnection.GetConnection(sqlTable.ConnectionType).Client, sqlTable.GetTableName(type));
            }
            return null;
        }
    }

    /// <summary>
    ///     数据库表格操作工具
    /// </summary>
    /// <typeparam name="TModelType">模型类型</typeparam>
    public sealed class TmphSqlModelTable<TModelType> : TmphSqlTable<TModelType, TModelType>
        where TModelType : class
    {
        /// <summary>
        ///     数据库表格操作工具
        /// </summary>
        /// <param name="sqlTable">数据库表格配置</param>
        /// <param name="TmphClient">SQL操作客户端</param>
        /// <param name="tableName">表格名称</param>
        private TmphSqlModelTable(TmphSqlTable sqlTable, TmphClient TmphClient, string tableName)
            : base(sqlTable, TmphClient, tableName)
        {
        }

        /// <summary>
        ///     获取数据库表格操作工具
        /// </summary>
        /// <returns>数据库表格操作工具</returns>
        public new static TmphSqlModelTable<TModelType> Get()
        {
            var type = typeof(TModelType);
            var sqlTable = TmphTypeAttribute.GetAttribute<TmphSqlTable>(type, false, true);
            if (sqlTable != null && Array.IndexOf(TmphSql.Default.CheckConnection, sqlTable.ConnectionType) != -1)
            {
                return new TmphSqlModelTable<TModelType>(sqlTable,
                    TmphConnection.GetConnection(sqlTable.ConnectionType).Client, sqlTable.GetTableName(type));
            }
            return null;
        }
    }

    /// <summary>
    ///     数据库表格操作工具
    /// </summary>
    /// <typeparam name="TValueType">表格类型</typeparam>
    /// <typeparam name="TModelType">模型类型</typeparam>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    public class TmphSqlTable<TValueType, TModelType, TKeyType> : TmphSqlTable.TmphSqlTool<TValueType, TModelType>
        where TValueType : class, TModelType
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        ///     设置关键字
        /// </summary>
        private readonly Action<TModelType, TKeyType> setPrimaryKey;

        /// <summary>
        ///     数据库表格操作工具
        /// </summary>
        /// <param name="sqlTable">数据库表格配置</param>
        /// <param name="TmphClient">SQL操作客户端</param>
        /// <param name="tableName">表格名称</param>
        protected TmphSqlTable(TmphSqlTable sqlTable, TmphClient TmphClient, string tableName)
            : base(sqlTable, TmphClient, tableName)
        {
            var primaryKeys = TmphSqlModel<TModelType>.PrimaryKeys.getArray(value => value.Field);
            GetPrimaryKey = TmphDatabaseModel<TModelType>.GetPrimaryKeyGetter<TKeyType>("GetSqlPrimaryKey", primaryKeys);
            setPrimaryKey = TmphDatabaseModel<TModelType>.GetPrimaryKeySetter<TKeyType>("SetSqlPrimaryKey", primaryKeys);
        }

        /// <summary>
        ///     获取关键字
        /// </summary>
        public Func<TModelType, TKeyType> GetPrimaryKey { get; private set; }

        /// <summary>
        ///     获取数据库表格操作工具
        /// </summary>
        /// <returns>数据库表格操作工具</returns>
        public static TmphSqlTable<TValueType, TModelType, TKeyType> Get()
        {
            var type = typeof(TValueType);
            var sqlTable = TmphTypeAttribute.GetAttribute<TmphSqlTable>(type, false, true);
            if (sqlTable != null && Array.IndexOf(TmphSql.Default.CheckConnection, sqlTable.ConnectionType) != -1)
            {
                return new TmphSqlTable<TValueType, TModelType, TKeyType>(sqlTable,
                    TmphConnection.GetConnection(sqlTable.ConnectionType).Client, sqlTable.GetTableName(type));
            }
            return null;
        }

        /// <summary>
        ///     根据关键字获取数据对象
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="key">关键字</param>
        /// <param name="memberMap">成员位图</param>
        /// <returns>数据对象</returns>
        public TValueType GetByPrimaryKey(TKeyType key, TmphMemberMap memberMap = null)
        {
            var value = TmphConstructor<TValueType>.New();
            setPrimaryKey(value, key);
            return GetByPrimaryKey(value, memberMap);
        }

        /// <summary>
        ///     修改数据库记录
        /// </summary>
        /// <param name="primaryKey">SQL关键字</param>
        /// <param name="updateExpression">SQL表达式</param>
        /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
        /// <returns>修改后的数据,失败返回null</returns>
        public TValueType Update(TKeyType key, TmphSqlTable.TmphUpdateExpression updateExpression,
            bool isIgnoreTransaction = false)
        {
            if (updateExpression.Count != 0)
            {
                var value = TmphConstructor<TValueType>.New();
                setPrimaryKey(value, key);
                return UpdateByPrimaryKey(value, updateExpression, isIgnoreTransaction);
            }
            return null;
        }

        /// <summary>
        ///     修改数据库记录
        /// </summary>
        /// <param name="primaryKey">SQL关键字</param>
        /// <param name="expression">SQL表达式</param>
        /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
        /// <returns>修改后的数据,失败返回null</returns>
        public TValueType Update<TReturnType>(TKeyType key, Expression<Func<TModelType, TReturnType>> expression,
            bool isIgnoreTransaction = false)
        {
            return expression != null ? Update(key, UpdateExpression(expression), isIgnoreTransaction) : null;
        }

        /// <summary>
        ///     修改数据库记录
        /// </summary>
        /// <param name="primaryKey">SQL关键字</param>
        /// <param name="expression">SQL表达式</param>
        /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
        /// <returns>修改后的数据,失败返回null</returns>
        public TValueType Update<TReturnType>(TKeyType key, Expression<Func<TModelType, TReturnType>> expression,
            TReturnType returnValue, bool isIgnoreTransaction = false)
        {
            return expression != null
                ? Update(key, UpdateExpression(expression, returnValue), isIgnoreTransaction)
                : null;
        }

        /// <summary>
        ///     修改数据库记录
        /// </summary>
        /// <param name="primaryKey">SQL关键字</param>
        /// <param name="expressions">SQL表达式</param>
        /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
        /// <returns>修改后的数据,失败返回null</returns>
        public TValueType Update<TReturnType>(TKeyType key, bool isIgnoreTransaction = false,
            params Expression<Func<TModelType, TReturnType>>[] expressions)
        {
            return expressions.Length != 0 ? Update(key, UpdateExpression(expressions), isIgnoreTransaction) : null;
        }

        /// <summary>
        ///     修改数据库记录
        /// </summary>
        /// <param name="primaryKey">SQL关键字</param>
        /// <param name="updateExpression">SQL表达式</param>
        /// <param name="isIgnoreTransaction">是否忽略应用程序事务</param>
        /// <returns>修改后的数据,失败返回null</returns>
        public bool Delete(TKeyType key, bool isIgnoreTransaction = false)
        {
            var value = TmphConstructor<TValueType>.New();
            setPrimaryKey(value, key);
            return DeleteByPrimaryKey(value, isIgnoreTransaction);
        }
    }

    /// <summary>
    ///     数据库表格操作工具
    /// </summary>
    /// <typeparam name="TModelType">模型类型</typeparam>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    public sealed class TmphSqlModelTable<TModelType, TKeyType> : TmphSqlTable<TModelType, TModelType, TKeyType>
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        ///     数据库表格操作工具
        /// </summary>
        /// <param name="sqlTable">数据库表格配置</param>
        /// <param name="TmphClient">SQL操作客户端</param>
        /// <param name="tableName">表格名称</param>
        private TmphSqlModelTable(TmphSqlTable sqlTable, TmphClient TmphClient, string tableName)
            : base(sqlTable, TmphClient, tableName)
        {
        }

        /// <summary>
        ///     获取数据库表格操作工具
        /// </summary>
        /// <returns>数据库表格操作工具</returns>
        public new static TmphSqlModelTable<TModelType, TKeyType> Get()
        {
            var type = typeof(TModelType);
            var sqlTable = TmphTypeAttribute.GetAttribute<TmphSqlTable>(type, false, true);
            if (sqlTable != null && Array.IndexOf(TmphSql.Default.CheckConnection, sqlTable.ConnectionType) != -1)
            {
                return new TmphSqlModelTable<TModelType, TKeyType>(sqlTable,
                    TmphConnection.GetConnection(sqlTable.ConnectionType).Client, sqlTable.GetTableName(type));
            }
            return null;
        }
    }
}