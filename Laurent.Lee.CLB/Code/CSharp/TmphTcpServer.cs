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

using Laurent.Lee.CLB.Net.Tcp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using pub = Laurent.Lee.CLB.Config.TmphPub;

namespace Laurent.Lee.CLB.Code.CSharp
{
    /// <summary>
    ///     TCP服务调用配置,定义类必须实现Laurent.Lee.CLB.Code.CSharp.tcpServer.ITcpServer接口
    /// </summary>
    public class TmphTcpServer : TmphTcpBase
    {
        /// <summary>
        ///     客户端调用接口名称(服务配置)
        /// </summary>
        public string ClientInterfaceName;

        /// <summary>
        ///     客户端附加接口类型(服务配置)
        /// </summary>
        public Type ClientInterfaceType;

        /// <summary>
        ///     成员选择类型
        /// </summary>
        public TmphMemberFilters Filter = TmphMemberFilters.NonPublicInstance;

        /// <summary>
        ///     是否生成客户端调用接口(服务配置)
        /// </summary>
        public bool IsClientInterface;

        /// <summary>
        ///     是否生成负载均衡服务(服务配置)
        /// </summary>
        public bool IsLoadBalancing;

        /// <summary>
        ///     是否服务器端(服务配置)
        /// </summary>
        public bool IsServer;

        /// <summary>
        ///     负载均衡服务端检测间隔秒数
        /// </summary>
        public int LoadBalancingCheckSeconds;

        /// <summary>
        ///     负载均衡错误尝试次数
        /// </summary>
        public int LoadBalancingTryCount = 3;

        /// <summary>
        ///     自定义负载均衡类型(服务配置)
        /// </summary>
        public Type LoadBalancingType;

        /// <summary>
        ///     成员选择类型
        /// </summary>
        public TmphMemberFilters MemberFilter
        {
            get { return Filter & TmphMemberFilters.Instance; }
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
                var tcpServer = TmphTypeAttribute.GetAttribute<TmphTcpServer>(type, false, false);
                //cSharp.Default.IsInheritAttribute
                if (tcpServer != null && tcpServer.IsSetup)
                {
                    var values = TmphDictionary.Create<TmphGenericMethod, MethodInfo>();
                    var methods = TmphMethodInfo.GetMethods<TmphTcpServer>(type, tcpServer.MemberFilter, false,
                        tcpServer.IsAttribute, tcpServer.IsBaseTypeAttribute, tcpServer.IsInheritAttribute);
                    if (type.IsGenericType)
                    {
                        var definitionMethods = TmphMethodInfo.GetMethods<TmphTcpServer>(type.GetGenericTypeDefinition(),
                            tcpServer.MemberFilter, false, tcpServer.IsAttribute, tcpServer.IsBaseTypeAttribute,
                            tcpServer.IsInheritAttribute);
                        var index = 0;
                        foreach (var method in methods)
                        {
                            if (method.Method.IsGenericMethod)
                                values.Add(new TmphGenericMethod(definitionMethods[index].Method), method.Method);
                            ++index;
                        }
                    }
                    else
                    {
                        foreach (var method in methods)
                        {
                            if (method.Method.IsGenericMethod)
                                values.Add(new TmphGenericMethod(method.Method), method.Method);
                        }
                    }
                    return values;
                }
            }
            return null;
        }

        /// <summary>
        ///     泛型方法调用
        /// </summary>
        /// <param name="value">服务器端目标对象</param>
        /// <param name="method">泛型方法信息</param>
        /// <param name="types">泛型参数类型集合</param>
        /// <param name="parameters">调用参数</param>
        /// <returns>返回值</returns>
        public static object InvokeGenericMethod(object value, MethodInfo method, TmphRemoteType[] types,
            params object[] parameters)
        {
            if (method == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            Debug.Assert(method != null, "method != null");
            return method.MakeGenericMethod(types.getArray(type => type.Type)).Invoke(value, parameters);
        }

        /// <summary>
        ///     获取配置信息
        /// </summary>
        /// <param name="type">TCP服务器类型</param>
        /// <returns>TCP调用服务器端配置信息</returns>
        public static TmphTcpServer GetConfig(Type type)
        {
            var attribute = TmphTypeAttribute.GetAttribute<TmphTcpServer>(type, false, true);
            return attribute != null ? pub.LoadConfig(attribute, attribute.Service) : null;
        }

        /// <summary>
        ///     获取配置信息
        /// </summary>
        /// <param name="serviceName">TCP调用服务名称</param>
        /// <param name="type">TCP服务器类型</param>
        /// <returns>TCP调用服务器端配置信息</returns>
        public static TmphTcpServer GetConfig(string serviceName, Type type = null)
        {
            var attribute =
                pub.LoadConfig(
                    type != null
                        ? TmphTypeAttribute.GetAttribute<TmphTcpServer>(type, false, true) ?? new TmphTcpServer()
                        : new TmphTcpServer(), serviceName);
            if (attribute.Service == null) attribute.Service = serviceName;
            return attribute;
        }

        /// <summary>
        ///     获取配置信息
        /// </summary>
        /// <param name="serviceName">TCP调用服务名称</param>
        /// <param name="type">TCP服务器类型</param>
        /// <returns>TCP调用服务器端配置信息</returns>
        public static TmphTcpServer GetTcpCallConfig(string serviceName, Type type = null)
        {
            var attribute = new TmphTcpServer();
            if (type != null)
            {
                var tcpCall = TmphTypeAttribute.GetAttribute<TmphTcpCall>(type, false, true);
                if (tcpCall != null) attribute.CopyFrom(tcpCall);
            }
            attribute = pub.LoadConfig(attribute, serviceName);
            if (attribute.Service == null) attribute.Service = serviceName;
            return attribute;
        }

        /// <summary>
        ///     TCP服务接口
        /// </summary>
        public interface TmphITcpServer
        {
            /// <summary>
            ///     设置TCP服务端
            /// </summary>
            /// <param name="tcpServer">TCP服务端</param>
            void SetTcpServer(TmphServer tcpServer);
        }
    }
}