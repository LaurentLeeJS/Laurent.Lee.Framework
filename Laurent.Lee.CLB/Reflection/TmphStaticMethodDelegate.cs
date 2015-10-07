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