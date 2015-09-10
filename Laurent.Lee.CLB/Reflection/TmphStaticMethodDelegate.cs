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

using System;
using System.Reflection;

namespace Laurent.Lee.CLB.Reflection
{
    /// <summary>
    ///     静态方法委托
    /// </summary>
    public static class TmphStaticMethodDelegate
    {
        /// <summary>
        ///     创建方法委托
        /// </summary>
        /// <typeparam name="TParameterType1">参数1类型</typeparam>
        /// <typeparam name="TParameterType2">参数2类型</typeparam>
        /// <typeparam name="TReturnType">返回值类型</typeparam>
        /// <param name="method">方法信息</param>
        /// <returns>方法委托</returns>
        public static Func<TParameterType1, TParameterType2, TReturnType> Create
            <TParameterType1, TParameterType2, TReturnType>(MethodInfo method)
        {
            if (method != null)
            {
                try
                {
                    return
                        (Func<TParameterType1, TParameterType2, TReturnType>)
                            Delegate.CreateDelegate(typeof(Func<TParameterType1, TParameterType2, TReturnType>), method);
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, method.fullName());
                }
            }
            return null;
        }

        /// <summary>
        ///     创建方法委托
        /// </summary>
        /// <typeparam name="TParameterType1">参数1类型</typeparam>
        /// <typeparam name="TParameterType2">参数2类型</typeparam>
        /// <param name="method">方法信息</param>
        /// <returns>方法委托</returns>
        public static Func<TParameterType1, TParameterType2, object> Create2<TParameterType1, TParameterType2>(
            MethodInfo method)
        {
            if (method != null && method.ReturnType != typeof(void))
            {
                var parameters = method.GetParameters();
                if (parameters.length() == 2)
                {
                    try
                    {
                        return
                            ((IStaticMethodDelegate<TParameterType1, TParameterType2>)
                                Activator.CreateInstance(
                                    typeof(TmphStaticMethodDelegate<,,>).MakeGenericType(parameters[0].ParameterType,
                                        parameters[1].ParameterType, method.ReturnType), method)).Invoke;
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, method.fullName());
                    }
                }
            }
            return null;
        }

        /// <summary>
        ///     创建方法委托
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <returns>方法委托</returns>
        public static Func<object, object> Create(MethodInfo method)
        {
            if (method != null && method.ReturnType != typeof(void))
            {
                var parameters = method.GetParameters();
                if (parameters.length() == 1)
                {
                    try
                    {
                        return
                            ((IStaticMethodDelegate)
                                Activator.CreateInstance(
                                    typeof(TmphStaticMethodDelegate<,>).MakeGenericType(parameters[0].ParameterType,
                                        method.ReturnType), method)).Invoke;
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, method.fullName());
                    }
                }
            }
            return null;
        }

        /// <summary>
        ///     创建方法委托
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <returns>方法委托</returns>
        public static Func<TParameterType, object> Create<TParameterType>(MethodInfo method)
        {
            if (method != null && method.ReturnType != typeof(void))
            {
                var parameters = method.GetParameters();
                if (parameters.length() == 1)
                {
                    try
                    {
                        return
                            ((IStaticMethodDelegate<TParameterType>)
                                Activator.CreateInstance(
                                    typeof(TmphStaticMethodDelegate<,>).MakeGenericType(parameters[0].ParameterType,
                                        method.ReturnType), method)).Invoke;
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, method.fullName());
                    }
                }
            }
            return null;
        }
    }

    /// <summary>
    ///     方法委托
    /// </summary>
    internal interface IStaticMethodDelegate
    {
        /// <summary>
        ///     执行方法
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <returns>返回值</returns>
        object Invoke(object parameter);
    }

    /// <summary>
    ///     方法委托
    /// </summary>
    internal interface IStaticMethodDelegate<in TParameterType> : IStaticMethodDelegate
    {
        /// <summary>
        ///     执行方法
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <returns>返回值</returns>
        object Invoke(TParameterType parameter);
    }

    /// <summary>
    ///     1个参数的方法委托
    /// </summary>
    /// <typeparam name="TParameterType">参数类型</typeparam>
    /// <typeparam name="TReturnType">返回值类型</typeparam>
    internal sealed class TmphStaticMethodDelegate<TParameterType, TReturnType> : IStaticMethodDelegate<TParameterType>
    {
        /// <summary>
        ///     方法委托
        /// </summary>
        private readonly Func<TParameterType, TReturnType> _func;

        /// <summary>
        ///     方法委托
        /// </summary>
        /// <param name="method">方法信息</param>
        public TmphStaticMethodDelegate(MethodInfo method)
        {
            _func =
                (Func<TParameterType, TReturnType>)
                    Delegate.CreateDelegate(typeof(Func<TParameterType, TReturnType>), method);
        }

        /// <summary>
        ///     执行方法
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <returns>返回值</returns>
        public object Invoke(TParameterType parameter)
        {
            return _func(parameter);
        }

        /// <summary>
        ///     执行方法
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <returns>返回值</returns>
        public object Invoke(object parameter)
        {
            return _func((TParameterType)parameter);
        }
    }

    /// <summary>
    ///     方法委托
    /// </summary>
    internal interface IStaticMethodDelegate<TParameterType1, TParameterType2>
    {
        /// <summary>
        ///     执行方法
        /// </summary>
        /// <param name="parameter1">参数1</param>
        /// <param name="parameter2">参数2</param>
        /// <returns>返回值</returns>
        object Invoke(TParameterType1 parameter1, TParameterType2 parameter2);
    }

    /// <summary>
    ///     2个参数的方法委托
    /// </summary>
    /// <typeparam name="TParameterType1">参数1类型</typeparam>
    /// <typeparam name="TParameterType2">参数2类型</typeparam>
    /// <typeparam name="TReturnType">返回值类型</typeparam>
    internal sealed class TmphStaticMethodDelegate<TParameterType1, TParameterType2, TReturnType> :
        IStaticMethodDelegate<TParameterType1, TParameterType2>
    {
        /// <summary>
        ///     方法委托
        /// </summary>
        private readonly Func<TParameterType1, TParameterType2, TReturnType> _func;

        /// <summary>
        ///     方法委托
        /// </summary>
        /// <param name="method">方法信息</param>
        public TmphStaticMethodDelegate(MethodInfo method)
        {
            _func =
                (Func<TParameterType1, TParameterType2, TReturnType>)
                    Delegate.CreateDelegate(typeof(Func<TParameterType1, TParameterType2, TReturnType>), method);
        }

        /// <summary>
        ///     执行方法
        /// </summary>
        /// <param name="parameter1">参数1</param>
        /// <param name="parameter2">参数2</param>
        /// <returns>返回值</returns>
        public object Invoke(TParameterType1 parameter1, TParameterType2 parameter2)
        {
            return _func(parameter1, parameter2);
        }
    }
}