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

using Laurent.Lee.CLB.Reflection;
using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Laurent.Lee.CLB.Code.CSharp
{
    /// <summary>
    ///     TCP调用配置
    /// </summary>
    public class TmphTcpCall : TmphTcpBase
    {
        /// <summary>
        ///     泛型类型函数调用缓存
        /// </summary>
        private static readonly Dictionary<Type, TmphKeyValue<Type, TmphStateSearcher.TmphAscii<MethodInfo>>>
            GenericTypeMethods =
                TmphDictionary.CreateOnly<Type, TmphKeyValue<Type, TmphStateSearcher.TmphAscii<MethodInfo>>>();

        /// <summary>
        ///     泛型类型函数调用缓存 访问锁
        /// </summary>
        private static int _genericTypeMethodLock;

        /// <summary>
        ///     泛型类型函数调用缓存 版本
        /// </summary>
        private static int _genericTypeMethodVersion;

        /// <summary>
        ///     成员选择类型
        /// </summary>
        public TmphMemberFilters Filter = TmphMemberFilters.NonPublicStatic;

        /// <summary>
        ///     是否支持抽象类
        /// </summary>
        public bool IsAbstract;

        /// <summary>
        ///     是否泛型方法服务器端代理,用于代码生成,请不要手动设置此属性,否则可能产生严重的安全问题
        /// </summary>
        public bool IsGenericTypeServerMethod;

        /// <summary>
        ///     是否TCP服务配置
        /// </summary>
        public bool IsServer;

        /// <summary>
        ///     成员选择类型
        /// </summary>
        public TmphMemberFilters MemberFilter
        {
            get { return Filter & TmphMemberFilters.Static; }
        }

        /// <summary>
        ///     获取TCP调用泛型函数集合
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <returns>TCP调用泛型函数集合</returns>
        public static Dictionary<TmphGenericMethod, MethodInfo> GetGenericMethods(Type type)
        {
            if (type != null)
            {
                var tcpCall = TmphTypeAttribute.GetAttribute<TmphTcpCall>(type, false, true);
                //cSharp.Default.IsInheritAttribute
                if (tcpCall != null && tcpCall.IsSetup)
                {
                    var values = TmphDictionary.Create<TmphGenericMethod, MethodInfo>();
                    foreach (
                        var method in
                            TmphMethodInfo.GetMethods<TmphTcpCall>(type, tcpCall.MemberFilter, false, tcpCall.IsAttribute,
                                tcpCall.IsBaseTypeAttribute, tcpCall.IsInheritAttribute))
                    {
                        if (method.Method.IsGenericMethod) values.Add(new TmphGenericMethod(method.Method), method.Method);
                    }
                    return values;
                }
            }
            return null;
        }

        /// <summary>
        ///     泛型类型函数调用
        /// </summary>
        /// <param name="remoteType">调用代理类型</param>
        /// <param name="methodName">调用函数名称</param>
        /// <param name="methodGenericTypes">方法泛型参数集合</param>
        /// <param name="parameters">调用参数</param>
        /// <returns>函数返回值</returns>
        public static object InvokeGenericTypeMethod(TmphRemoteType remoteType, string methodName,
            TmphRemoteType[] methodGenericTypes, params object[] parameters)
        {
            return
                GetGenericTypeMethod(remoteType, methodName)
                    .MakeGenericMethod(methodGenericTypes.getArray(value => value.Type))
                    .Invoke(null, parameters);
        }

        /// <summary>
        ///     泛型类型函数调用
        /// </summary>
        /// <param name="remoteType">调用代理类型</param>
        /// <param name="methodName">调用函数名称</param>
        /// <param name="parameters">调用参数</param>
        /// <returns>函数返回值</returns>
        public static object InvokeGenericTypeMethod(TmphRemoteType remoteType, string methodName,
            params object[] parameters)
        {
            return GetGenericTypeMethod(remoteType, methodName).Invoke(null, parameters);
        }

        /// <summary>
        ///     获取泛型类型函数信息
        /// </summary>
        /// <param name="remoteType">调用代理类型</param>
        /// <param name="methodName">调用函数名称</param>
        /// <returns>泛型类型函数信息</returns>
        private static MethodInfo GetGenericTypeMethod(TmphRemoteType remoteType, string methodName)
        {
            var type = remoteType.Type;
            if (type.DeclaringType != null && (type.Name == GenericTypeServerName && type.DeclaringType.IsGenericType))
            {
                var tcpCall = TmphTypeAttribute.GetAttribute<TmphTcpCall>(type, false, false);
                if (tcpCall != null && tcpCall.IsGenericTypeServerMethod)
                {
                    tcpCall = TmphTypeAttribute.GetAttribute<TmphTcpCall>(type.DeclaringType, false, true);
                    //cSharp.Default.IsInheritAttribute
                    if (tcpCall != null && tcpCall.IsSetup)
                    {
                        TmphKeyValue<Type, TmphStateSearcher.TmphAscii<MethodInfo>> methods;
                        var version = _genericTypeMethodVersion;
                        if (!GenericTypeMethods.TryGetValue(type, out methods) || methods.Key != type)
                        {
                            TmphInterlocked.CompareSetSleep(ref _genericTypeMethodLock);
                            try
                            {
                                if (version == _genericTypeMethodVersion ||
                                    !GenericTypeMethods.TryGetValue(type, out methods))
                                {
                                    var methodInfos = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
                                    methods = new TmphKeyValue<Type, TmphStateSearcher.TmphAscii<MethodInfo>>(type,
                                        new TmphStateSearcher.TmphAscii<MethodInfo>(
                                            methodInfos.getArray(value => value.Name),
                                            methodInfos));
                                    GenericTypeMethods.Add(type, methods);
                                    ++_genericTypeMethodVersion;
                                }
                            }
                            finally
                            {
                                _genericTypeMethodLock = 0;
                            }
                        }
                        return methods.Value.Get(methodName);
                    }
                }
            }
            TmphLog.Error.Throw(type.fullName() + " 不符合泛型类型服务器端调用");
            return null;
        }

        /// <summary>
        ///     泛型方法调用
        /// </summary>
        /// <param name="method">泛型方法信息</param>
        /// <param name="types">泛型参数类型集合</param>
        /// <param name="parameters">调用参数</param>
        /// <returns>返回值</returns>
        public static object InvokeGenericMethod(MethodInfo method, TmphRemoteType[] types, params object[] parameters)
        {
            if (method == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            Debug.Assert(method != null, "method != null");
            return method.MakeGenericMethod(types.getArray(value => value.Type)).Invoke(null, parameters);
        }
    }
}

namespace Laurent.Lee.CLB.Code
{
    /// <summary>
    ///     成员类型
    /// </summary>
    public partial class TmphMemberType
    {
        /// <summary>
        ///     泛型参数类型
        /// </summary>
        public TmphMemberType GenericParameterType
        {
            get { return Type.IsGenericParameter ? (TmphMemberType)typeof(object) : this; }
        }
    }
}