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
using Laurent.Lee.CLB.Threading;
using System;

namespace Laurent.Lee.CLB.Code.CSharp
{
    /// <summary>
    ///     方法信息
    /// </summary>
    public abstract class TmphAsynchronousMethod
    {
        /// <summary>
        ///     返回值参数名称
        /// </summary>
        public const string ReturnParameterName = "Return";

        /// <summary>
        ///     异步返回值
        /// </summary>
        public struct TmphReturnValue
        {
            /// <summary>
            ///     是否调用成功
            /// </summary>
            public bool IsReturn;
        }

        /// <summary>
        ///     异步返回值
        /// </summary>
        /// <typeparam name="TValueType">返回值类型</typeparam>
        public struct TmphReturnValue<TValueType>
        {
            /// <summary>
            ///     异步调用失败
            /// </summary>
            private static readonly Exception ReturnException = new Exception("异步调用失败");

            /// <summary>
            ///     是否调用成功
            /// </summary>
            public bool IsReturn;

            /// <summary>
            ///     返回值
            /// </summary>
            public TValueType Value;

            /// <summary>
            ///     清空数据
            /// </summary>
            public void Null()
            {
                IsReturn = false;
                Value = default(TValueType);
            }

            /// <summary>
            ///     获取返回值
            /// </summary>
            /// <param name="value">异步返回值</param>
            /// <returns>返回值</returns>
            public static implicit operator TmphReturnValue<TValueType>(TValueType value)
            {
                return new TmphReturnValue<TValueType> { IsReturn = true, Value = value };
            }

            /// <summary>
            ///     获取返回值
            /// </summary>
            /// <param name="value">返回值</param>
            /// <returns>异步返回值</returns>
            public static implicit operator TValueType(TmphReturnValue<TValueType> value)
            {
                if (value.IsReturn) return value.Value;
                throw ReturnException;
            }
        }

        /// <summary>
        ///     返回参数
        /// </summary>
        /// <typeparam name="TValueType">返回参数类型</typeparam>
        public interface IReturnParameter<TValueType>
        {
            /// <summary>
            ///     返回值
            /// </summary>
            TValueType Return { get; set; }
        }

        /// <summary>
        ///     返回参数
        /// </summary>
        /// <typeparam name="TValueType">返回参数类型</typeparam>
        public class TmphReturnParameter<TValueType> : IReturnParameter<TValueType>
        {
            [TmphJsonSerialize.TmphMember(IsIgnoreCurrent = true)]
            [TmphJsonParse.TmphMember(IsIgnoreCurrent = true)]
            internal
                TValueType Ret;

            /// <summary>
            ///     返回值
            /// </summary>
            public TValueType Return
            {
                get { return Ret; }
                set { Ret = value; }
            }
        }

        /// <summary>
        ///     异步回调
        /// </summary>
        public sealed class TmphCallReturn
        {
            /// <summary>
            ///     回调委托
            /// </summary>
            private Action<TmphReturnValue> _callback;

            /// <summary>
            ///     异步回调返回值
            /// </summary>
            /// <param name="isReturn">调用使用成功</param>
            private void OnReturn(bool isReturn)
            {
                try
                {
                    _callback(new TmphReturnValue { IsReturn = isReturn });
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
            }

            /// <summary>
            ///     获取异步回调
            /// </summary>
            /// <param name="callback">回调委托</param>
            /// <returns>异步回调</returns>
            public static Action<bool> Get(Action<TmphReturnValue> callback)
            {
                if (callback == null) return null;
                try
                {
                    return new TmphCallReturn { _callback = callback }.OnReturn;
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                callback(new TmphReturnValue { IsReturn = false });
                return null;
            }
        }

        /// <summary>
        ///     异步回调
        /// </summary>
        /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
        public sealed class TmphCallReturn<TOutputParameterType>
        {
            /// <summary>
            ///     回调委托
            /// </summary>
            private Action<TmphReturnValue> _callback;

            /// <summary>
            ///     异步回调返回值
            /// </summary>
            /// <param name="outputParameter">输出参数</param>
            private void OnReturn(TmphReturnValue<TOutputParameterType> outputParameter)
            {
                try
                {
                    _callback(new TmphReturnValue { IsReturn = outputParameter.IsReturn });
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
            }

            /// <summary>
            ///     获取异步回调
            /// </summary>
            /// <param name="callback">回调委托</param>
            /// <returns>异步回调</returns>
            public static Action<TmphReturnValue<TOutputParameterType>> Get(Action<TmphReturnValue> callback)
            {
                if (callback == null) return null;
                try
                {
                    return new TmphCallReturn<TOutputParameterType> { _callback = callback }.OnReturn;
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                callback(new TmphReturnValue { IsReturn = false });
                return null;
            }
        }

        /// <summary>
        ///     异步回调
        /// </summary>
        /// <typeparam name="TReturnType">返回值类型</typeparam>
        /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
        public sealed class TmphCallReturn<TReturnType, TOutputParameterType>
            where TOutputParameterType : IReturnParameter<TReturnType>
        {
            /// <summary>
            ///     回调委托
            /// </summary>
            private Action<TmphReturnValue<TReturnType>> _callback;

            /// <summary>
            ///     异步回调返回值
            /// </summary>
            /// <param name="outputParameter">输出参数</param>
            private void OnReturn(TmphReturnValue<TOutputParameterType> outputParameter)
            {
                try
                {
                    _callback(outputParameter.IsReturn
                        ? new TmphReturnValue<TReturnType> { IsReturn = true, Value = outputParameter.Value.Return }
                        : new TmphReturnValue<TReturnType> { IsReturn = false });
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
            }

            /// <summary>
            ///     获取异步回调
            /// </summary>
            /// <param name="callback">回调委托</param>
            /// <returns>异步回调</returns>
            public static Action<TmphReturnValue<TOutputParameterType>> Get(Action<TmphReturnValue<TReturnType>> callback)
            {
                if (callback == null) return null;
                try
                {
                    return new TmphCallReturn<TReturnType, TOutputParameterType> { _callback = callback }.OnReturn;
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                callback(new TmphReturnValue<TReturnType> { IsReturn = false });
                return null;
            }
        }

        /// <summary>
        ///     异步回调泛型返回值
        /// </summary>
        /// <typeparam name="TReturnType">返回值类型</typeparam>
        /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
        public sealed class TmphCallReturnGeneric<TReturnType, TOutputParameterType>
            where TOutputParameterType : IReturnParameter<object>
        {
            /// <summary>
            ///     回调委托
            /// </summary>
            private Action<TmphReturnValue<TReturnType>> callback;

            /// <summary>
            ///     异步回调返回值
            /// </summary>
            /// <param name="outputParameter">输出参数</param>
            private void OnReturn(TmphReturnValue<TOutputParameterType> outputParameter)
            {
                try
                {
                    callback(outputParameter.IsReturn
                        ? new TmphReturnValue<TReturnType>
                        {
                            IsReturn = true,
                            Value = (TReturnType)outputParameter.Value.Return
                        }
                        : new TmphReturnValue<TReturnType> { IsReturn = false });
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
            }

            /// <summary>
            ///     异步回调泛型返回值
            /// </summary>
            /// <param name="callback">异步回调返回值</param>
            /// <returns>异步回调返回值</returns>
            public static Action<TmphReturnValue<TOutputParameterType>> Get(Action<TmphReturnValue<TReturnType>> callback)
            {
                if (callback == null) return null;
                try
                {
                    return new TmphCallReturnGeneric<TReturnType, TOutputParameterType> { callback = callback }.OnReturn;
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                callback(new TmphReturnValue<TReturnType> { IsReturn = false });
                return null;
            }
        }

        /// <summary>
        ///     同步等待调用
        /// </summary>
        public sealed class TmphWaitCall
        {
            /// <summary>
            ///     同步等待
            /// </summary>
            private readonly TmphAutoWaitHandle _waitHandle;

            /// <summary>
            ///     输出参数
            /// </summary>
            private bool _outputParameter;

            /// <summary>
            ///     回调处理
            /// </summary>
            public Action<bool> OnReturn;

            /// <summary>
            ///     同步等待调用
            /// </summary>
            private TmphWaitCall()
            {
                _waitHandle = new TmphAutoWaitHandle(false);
                OnReturn = OnReturnHandle;
            }

            /// <summary>
            ///     调用返回值（警告：每次调用只能使用一次）
            /// </summary>
            public TmphReturnValue Value
            {
                get
                {
                    _waitHandle.Wait();
                    var outputParameter = _outputParameter;
                    _outputParameter = false;
                    TmphTypePool<TmphWaitCall>.Push(this);
                    return new TmphReturnValue { IsReturn = outputParameter };
                }
            }

            /// <summary>
            ///     回调处理
            /// </summary>
            /// <param name="outputParameter">是否调用成功</param>
            private void OnReturnHandle(bool outputParameter)
            {
                _outputParameter = outputParameter;
                //if (!outputParameter) log.Default.Add("异步调用失败(bool)", true, false);
                _waitHandle.Set();
            }

            /// <summary>
            ///     获取同步等待调用
            /// </summary>
            /// <returns>同步等待调用</returns>
            public static TmphWaitCall Get()
            {
                var value = TmphTypePool<TmphWaitCall>.Pop();
                if (value == null)
                {
                    try
                    {
                        value = new TmphWaitCall();
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                        return null;
                    }
                }
                return value;
            }
        }

        /// <summary>
        ///     同步等待调用
        /// </summary>
        /// <typeparam name="TOutputParameterType">输出参数类型</typeparam>
        public sealed class TmphWaitCall<TOutputParameterType>
        {
            /// <summary>
            ///     同步等待
            /// </summary>
            private readonly TmphAutoWaitHandle _waitHandle;

            /// <summary>
            ///     输出参数
            /// </summary>
            private TmphReturnValue<TOutputParameterType> _outputParameter;

            /// <summary>
            ///     回调处理
            /// </summary>
            public Action<TmphReturnValue<TOutputParameterType>> OnReturn;

            /// <summary>
            ///     同步等待调用
            /// </summary>
            private TmphWaitCall()
            {
                _waitHandle = new TmphAutoWaitHandle(false);
                OnReturn = OnReturnHandle;
            }

            /// <summary>
            ///     调用返回值（警告：每次调用只能使用一次）
            /// </summary>
            public TmphReturnValue<TOutputParameterType> Value
            {
                get
                {
                    _waitHandle.Wait();
                    var outputParameter = _outputParameter;
                    _outputParameter.Null();
                    TmphTypePool<TmphWaitCall<TOutputParameterType>>.Push(this);
                    return outputParameter;
                }
            }

            /// <summary>
            ///     回调处理
            /// </summary>
            /// <param name="outputParameter">输出参数</param>
            private void OnReturnHandle(TmphReturnValue<TOutputParameterType> outputParameter)
            {
                _outputParameter = outputParameter;
                //if (!outputParameter.IsReturn) log.Default.Add("异步调用失败()", true, false);
                _waitHandle.Set();
            }

            /// <summary>
            ///     获取同步等待调用
            /// </summary>
            /// <returns>同步等待调用</returns>
            public static TmphWaitCall<TOutputParameterType> Get()
            {
                var value = TmphTypePool<TmphWaitCall<TOutputParameterType>>.Pop();
                if (value == null)
                {
                    try
                    {
                        value = new TmphWaitCall<TOutputParameterType>();
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                        return null;
                    }
                }
                return value;
            }
        }
    }
}