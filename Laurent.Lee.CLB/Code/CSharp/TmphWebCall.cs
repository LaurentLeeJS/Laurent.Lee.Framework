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
using Laurent.Lee.CLB.Net.Tcp.Http;
using System.Text;

namespace Laurent.Lee.CLB.Code.CSharp
{
    /// <summary>
    ///     web调用配置
    /// </summary>
    public sealed class TmphWebCall : TmphWebPage
    {
        /// <summary>
        ///     调用全名
        /// </summary>
        public string FullName;

        /// <summary>
        ///     成员是否匹配自定义属性类型
        /// </summary>
        public bool IsAttribute = true;

        /// <summary>
        ///     是否搜索父类自定义属性
        /// </summary>
        public bool IsBaseTypeAttribute;

        /// <summary>
        ///     成员匹配自定义属性是否可继承
        /// </summary>
        public bool IsInheritAttribute;

        /// <summary>
        ///     是否仅支持POST请求(类型有效)
        /// </summary>
        public bool IsOnlyPost;

        /// <summary>
        ///     web调用接口
        /// </summary>
        public interface IWebCall : IWebPage
        {
            /// <summary>
            ///     HTTP请求表单设置
            /// </summary>
            TmphRequestForm RequestForm { set; }

            /// <summary>
            ///     解析web调用参数
            /// </summary>
            /// <typeparam name="TParameterType">web调用参数类型</typeparam>
            /// <param name="parameter">web调用参数</param>
            /// <returns>是否成功</returns>
            bool ParseParameter<TParameterType>(ref TParameterType parameter)
                where TParameterType : struct;
        }

        /// <summary>
        ///     web调用池
        /// </summary>
        public abstract class TmphCallPool
        {
            /// <summary>
            ///     web调用
            /// </summary>
            public abstract bool Call();
        }

        /// <summary>
        ///     web调用池
        /// </summary>
        /// <typeparam name="TCallType">web调用类型</typeparam>
        /// <typeparam name="TWebType">web调用实例类型</typeparam>
        public abstract class TmphCallPool<TCallType, TWebType> : TmphCallPool
            where TCallType : TmphCallPool<TCallType, TWebType>
            where TWebType : IWebCall
        {
            /// <summary>
            ///     web调用
            /// </summary>
            public TWebType WebCall;
        }

        /// <summary>
        ///     web调用池
        /// </summary>
        /// <typeparam name="TCallType">web调用类型</typeparam>
        /// <typeparam name="TWebType">web调用实例类型</typeparam>
        /// <typeparam name="TParameterType">web调用参数类型</typeparam>
        public abstract class TmphCallPool<TCallType, TWebType, TParameterType> : TmphCallPool<TCallType, TWebType>
            where TCallType : TmphCallPool<TCallType, TWebType, TParameterType>
            where TWebType : IWebCall
            where TParameterType : struct
        {
            /// <summary>
            ///     web调用参数
            /// </summary>
            public TParameterType Parameter;
        }

        /// <summary>
        ///     web调用
        /// </summary>
        public abstract class TmphCall : TmphPage
        {
            /// <summary>
            ///     HTTP请求表单设置
            /// </summary>
            public TmphRequestForm RequestForm
            {
                set
                {
                    if (Form == null) Form = value;
                    else TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
                }
            }

            /// <summary>
            ///     解析web调用参数
            /// </summary>
            /// <typeparam name="TParameterType">web调用参数类型</typeparam>
            /// <param name="parameter">web调用参数</param>
            /// <returns>是否成功</returns>
            public bool ParseParameter<TParameterType>(ref TParameterType parameter)
                where TParameterType : struct
            {
                if (Form != null && Form.Json != null)
                {
                    if (Form.Json.Length != 0
                        && !TmphJsonParser.Parse(Form.Json, ref parameter))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!RequestHeader.ParseQuery(ref parameter)) return false;
                    var queryJson = RequestHeader.QueryJson;
                    if (queryJson.Length != 0
                        && !TmphJsonParser.Parse(queryJson, ref parameter))
                    {
                        return false;
                    }
                }
                return true;
            }

            /// <summary>
            ///     根据HTTP请求表单值获取保存文件全称
            /// </summary>
            /// <param name="value">HTTP请求表单值</param>
            /// <returns>文件全称</returns>
            public virtual string GetSaveFileName(TmphRequestForm.TmphValue value)
            {
                return null;
            }
        }

        /// <summary>
        ///     web调用
        /// </summary>
        /// <typeparam name="TCallType">web调用类型</typeparam>
        public abstract class TmphCall<TCallType> : TmphCall, IWebCall where TCallType : TmphCall<TCallType>
        {
            /// <summary>
            ///     是否使用对象池
            /// </summary>
            private bool _isPool;

            ///// <summary>
            ///// 是否已经加载HTTP请求头部
            ///// </summary>
            //private int isLoadHeader;
            /// <summary>
            ///     当前web调用
            /// </summary>
            private TCallType _thisCall;

            /// <summary>
            ///     WEB页面回收
            /// </summary>
            internal override void PushPool()
            {
                if (_isPool)
                {
                    _isPool = false;
                    Clear();
                    if (_thisCall == null) _thisCall = (TCallType)this;
                    TmphTypePool<TCallType>.Push(_thisCall);
                }
            }

            ///// <summary>
            ///// WEB页面回收
            ///// </summary>
            //protected static void pushPool(TCallType call)
            //{
            //    if (call.isLoadHeader != 0)
            //    {
            //        call.clear();
            //        if (Interlocked.CompareExchange(ref call.isLoadHeader, 0, 2) == 2) typePool<TCallType>.Push(call);
            //        return;
            //    }
            //    log.Default.Add("WEB页面回收", true, true);
            //}
            /// <summary>
            ///     HTTP请求头部处理
            /// </summary>
            /// <param name="socketIdentity">套接字操作编号</param>
            /// <param name="request">HTTP请求头部</param>
            /// <param name="isPool">是否使用WEB页面池</param>
            /// <returns>是否成功</returns>
            internal override bool LoadHeader(long socketIdentity, TmphRequestHeader request, bool isPool)
            {
                //if (Interlocked.CompareExchange(ref isLoadHeader, isPool ? 2 : 1, 0) == 0)
                //{
                SocketIdentity = socketIdentity;
                RequestHeader = request;
                ResponseEncoding = request.IsWebSocket ? Encoding.UTF8 : DomainServer.ResponseEncoding;
                _isPool = isPool;
                return true;
                //}
                //log.Default.Add(typeof(TCallType).fullName() + " 页面回收错误[" + isLoadHeader.toString() + "]", false, true);
                //return false;
            }
        }
    }
}