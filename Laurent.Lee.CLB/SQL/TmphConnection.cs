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
using Laurent.Lee.CLB.Emit;
using Laurent.Lee.CLB.Reflection;
using Laurent.Lee.CLB.Threading;
using System;
using System.Reflection;
using System.Threading;
using TmphSqlModel = Laurent.Lee.CLB.Code.CSharp.TmphSqlModel;

namespace Laurent.Lee.CLB.Sql
{
    /// <summary>
    ///     数据库连接信息
    /// </summary>
    public sealed unsafe class TmphConnection
    {
        /// <summary>
        ///     检测链接程序集名称集合
        /// </summary>
        private static readonly TmphUniqueHashSet<TmphAssemblyName> checkConnectionAssemblyNames =
            new TmphUniqueHashSet<TmphAssemblyName>(
                new TmphAssemblyName[] { "mscorlib", TmphPub.LaurentLeeFramework, "System", "Microsoft", "vshost" }, 7);

        /// <summary>
        ///     检测链接程序集名称分隔符位图
        /// </summary>
        private static readonly TmphPointer checkConnectionAssemblyNameMap;

        /// <summary>
        ///     检测程序集名称集合
        /// </summary>
        private static TmphInterlocked.TmphDictionary<TmphHashString, EventWaitHandle> checkAssemblyNames =
            new TmphInterlocked.TmphDictionary<TmphHashString, EventWaitHandle>(
                TmphDictionary.CreateHashString<EventWaitHandle>());

        /// <summary>
        ///     SQL客户端
        /// </summary>
        private TmphClient TmphClient;

        /// <summary>
        ///     连接字符串
        /// </summary>
        public string Connection;

        /// <summary>
        ///     数据库表格所有者
        /// </summary>
        public string Owner = "dbo";

        /// <summary>
        ///     SQL类型
        /// </summary>
        public TmphType Type;

        static TmphConnection()
        {
            if (TmphSql.Default.CheckConnection.Length != 0)
            {
                checkConnectionAssemblyNameMap =
                    new TmphString.TmphAsciiMap(TmphUnmanaged.Get(TmphString.TmphAsciiMap.MapBytes, true), ".,-", true).Pointer;
                AppDomain.CurrentDomain.AssemblyLoad += checkConnection;
                TmphThreadPool.TinyPool.FastStart(checkConnection, AppDomain.CurrentDomain.GetAssemblies(), null, null);
            }
        }

        /// <summary>
        ///     SQL客户端
        /// </summary>
        public TmphClient Client
        {
            get
            {
                if (TmphClient == null)
                {
                    TmphClient = (TmphClient)TmphEnum<TmphType, TmphTypeInfo>.Array(Type).ClientType
                        .GetConstructor(new[] { typeof(TmphConnection) }).Invoke(new object[] { this });
                }
                return TmphClient;
            }
        }

        /// <summary>
        ///     是否需要检测链接
        /// </summary>
        public static bool IsCheckConnection
        {
            get { return TmphSql.Default.CheckConnection.Length != 0; }
        }

        /// <summary>
        ///     根据连接类型获取连接信息
        /// </summary>
        /// <param name="type">连接类型</param>
        /// <returns>连接信息</returns>
        public static TmphConnection GetConnection(string type)
        {
            return Config.TmphPub.LoadConfig(new TmphConnection(), type);
        }

        /// <summary>
        ///     检测链接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void checkConnection(object sender, AssemblyLoadEventArgs args)
        {
            TmphThreadPool.TinyPool.FastStart(checkConnection, args.LoadedAssembly, null, null);
        }

        /// <summary>
        ///     检测链接
        /// </summary>
        /// <param name="assembly">程序集</param>
        private static void checkConnection(Assembly assembly)
        {
            bool isAssembly;
            var assemblyName = assembly.FullName;
            fixed (char* nameFixed = assemblyName)
            {
                var splitIndex = Unsafe.TmphString.FindAscii(nameFixed, nameFixed + assemblyName.Length,
                    checkConnectionAssemblyNameMap.Byte, ',');
                isAssembly = splitIndex == null ||
                             !checkConnectionAssemblyNames.Contains(assemblyName.Substring(0,
                                 (int)(splitIndex - nameFixed)));
            }
            if (isAssembly)
            {
                Type currentType = null;
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsClass && !type.IsAbstract && !type.IsGenericType && type.IsVisible)
                        {
                            var sqlTable = TmphTypeAttribute.GetAttribute<TmphSqlTable>(type, false, true);
                            if (sqlTable != null &&
                                Array.IndexOf(TmphSql.Default.CheckConnection, sqlTable.ConnectionType) != -1)
                            {
                                checkSqlTable(currentType = type, TmphDataModel.GetModelType<TmphSqlModel>(type) ?? type,
                                    sqlTable);
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, currentType.fullName(), true);
                }
                finally
                {
                    EventWaitHandle wait = null;
                    if (checkAssemblyNames.Set(assemblyName, null, out wait))
                    {
                        wait.Set();
                        wait.Close();
                    }
                }
            }
        }

        /// <summary>
        ///     等待检测链接
        /// </summary>
        /// <param name="type">表格绑定类型</param>
        public static void WaitCheckConnection(Type type)
        {
            if (type != null) WaitCheckConnection(type.Assembly);
        }

        /// <summary>
        ///     等待检测链接
        /// </summary>
        /// <param name="assembly">程序集</param>
        public static void WaitCheckConnection(Assembly assembly)
        {
            if (assembly != null)
            {
                EventWaitHandle wait = null;
                TmphHashString assemblyName = assembly.FullName;
                if (!checkAssemblyNames.TryGetValue(assemblyName, out wait))
                {
                    checkAssemblyNames.Set(assemblyName, wait = new EventWaitHandle(false, EventResetMode.ManualReset));
                }
                if (wait != null) wait.WaitOne();
            }
        }

        /// <summary>
        ///     检测链接
        /// </summary>
        /// <param name="assemblys">程序集集合</param>
        private static void checkConnection(Assembly[] assemblys)
        {
            foreach (var assembly in assemblys)
            {
                Thread.Sleep(0);
                checkConnection(assembly);
            }
        }

        /// <summary>
        ///     检测SQL表格
        /// </summary>
        /// <param name="table">表格绑定类型</param>
        /// <param name="sqlModelType">表格模型类型</param>
        /// <param name="sqlTable">数据库表格配置</param>
        private static void checkSqlTable(Type type, Type sqlModelType, TmphSqlTable sqlTable)
        {
            var sqlClient = GetConnection(sqlTable.ConnectionType).Client;
            var memberTable =
                (TmphTable)
                    typeof(TmphSqlModel<>).MakeGenericType(sqlModelType)
                        .GetMethod("GetTable", BindingFlags.Static | BindingFlags.NonPublic, null,
                            new[] { typeof(Type), typeof(TmphSqlTable) }, null)
                        .Invoke(null, new object[] { type, sqlTable });
            sqlClient.ToSqlColumn(memberTable);
            var table = sqlClient.GetTable(memberTable.Columns.Name, null);
            if (table == null)
            {
                if (!sqlClient.CreateTable(memberTable, null))
                    TmphLog.Error.Add("表格 " + memberTable.Columns.Name + " 创建失败", false, false);
            }
            else
            {
                var ignoreCase = TmphEnum<TmphType, TmphTypeInfo>.Array(sqlClient.Connection.Type).IgnoreCase;
                var names = ignoreCase
                    ? table.Columns.Columns.getArray(value => value.Name.ToLower())
                    : table.Columns.Columns.getArray(value => value.Name);
                using (var sqlColumnNames = new TmphStateSearcher.TmphAscii<TmphColumn>(names, table.Columns.Columns))
                {
                    TmphSubArray<TmphColumn> newColumns;
                    if (ignoreCase)
                        newColumns =
                            memberTable.Columns.Columns.getFind(
                                value => !sqlColumnNames.ContainsKey(value.Name.ToLower()));
                    else
                        newColumns =
                            memberTable.Columns.Columns.getFind(value => !sqlColumnNames.ContainsKey(value.Name));
                    if (newColumns.Count != 0)
                    {
                        if (sqlClient.IsAddField &&
                            sqlClient.AddFields(
                                new THColumnCollection { Name = memberTable.Columns.Name, Columns = newColumns.ToArray() },
                                null))
                        {
                            table.Columns.Columns = newColumns.Add(table.Columns.Columns).ToArray();
                        }
                        else
                        {
                            TmphLog.Error.Add(
                                "表格 " + memberTable.Columns.Name + " 字段添加失败 : " +
                                newColumns.JoinString(',', value => value.Name), false, false);
                        }
                    }
                    if (ignoreCase)
                        newColumns =
                            memberTable.Columns.Columns.getFind(
                                value => !value.IsMatch(sqlColumnNames.Get(value.Name.ToLower()), ignoreCase));
                    else
                        newColumns =
                            memberTable.Columns.Columns.getFind(
                                value => !value.IsMatch(sqlColumnNames.Get(value.Name), ignoreCase));
                    if (newColumns.Count() != 0)
                    {
                        TmphLog.Default.Add(
                            "表格 " + memberTable.Columns.Name + " 字段类型不匹配 : " +
                            newColumns.JoinString(',', value => value.Name), false, false);
                    }
                }
            }
        }

        /// <summary>
        ///     程序集名称唯一哈希
        /// </summary>
        private struct TmphAssemblyName : IEquatable<TmphAssemblyName>
        {
            /// <summary>
            ///     程序集名称
            /// </summary>
            public string Name;

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public bool Equals(TmphAssemblyName other)
            {
                return Name == other.Name;
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="name">程序集名称</param>
            /// <returns>程序集名称唯一哈希</returns>
            public static implicit operator TmphAssemblyName(string name)
            {
                return new TmphAssemblyName { Name = name };
            }

            /// <summary>
            ///     获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override int GetHashCode()
            {
                return (Name[0] >> 3) & 7;
            }

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((TmphAssemblyName)obj);
            }
        }
    }
}