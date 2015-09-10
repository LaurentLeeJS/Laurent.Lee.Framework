using System;

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     泛型任务信息
    /// </summary>
    /// <typeparam name="TParameterType">任务执行参数类型</typeparam>
    public sealed class TmphRun<TParameterType>
    {
        /// <summary>
        ///     执行任务
        /// </summary>
        private readonly Action action;

        /// <summary>
        ///     任务执行委托
        /// </summary>
        private Action<TParameterType> func;

        /// <summary>
        ///     任务执行参数
        /// </summary>
        private TParameterType parameter;

        /// <summary>
        ///     泛型任务信息
        /// </summary>
        private TmphRun()
        {
            action = call;
        }

        /// <summary>
        ///     执行任务
        /// </summary>
        private void call()
        {
            var func = this.func;
            var parameter = this.parameter;
            this.func = null;
            this.parameter = default(TParameterType);
            try
            {
                TmphTypePool<TmphRun<TParameterType>>.Push(this);
            }
            finally
            {
                func(parameter);
            }
        }

        /// <summary>
        ///     泛型任务信息
        /// </summary>
        /// <param name="action">任务执行委托</param>
        /// <param name="parameter">任务执行参数</param>
        /// <returns>泛型任务信息</returns>
        public static Action Create(Action<TParameterType> action, TParameterType parameter)
        {
            var run = TmphTypePool<TmphRun<TParameterType>>.Pop();
            if (run == null)
            {
                try
                {
                    run = new TmphRun<TParameterType>();
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                if (run == null) return null;
            }
            run.func = action;
            run.parameter = parameter;
            return run.action;
        }
    }

    /// <summary>
    ///     泛型任务信息
    /// </summary>
    /// <typeparam name="TParameterType">任务执行参数类型</typeparam>
    /// <typeparam name="TReturnType">返回值类型</typeparam>
    public sealed class TmphRun<TParameterType, TReturnType>
    {
        /// <summary>
        ///     执行任务
        /// </summary>
        private readonly Action action;

        /// <summary>
        ///     任务执行委托
        /// </summary>
        private Func<TParameterType, TReturnType> func;

        /// <summary>
        ///     返回值执行委托
        /// </summary>
        private Action<TReturnType> onReturn;

        /// <summary>
        ///     任务执行参数
        /// </summary>
        private TParameterType parameter;

        /// <summary>
        ///     泛型任务信息
        /// </summary>
        private TmphRun()
        {
            action = call;
        }

        /// <summary>
        ///     执行任务
        /// </summary>
        private void call()
        {
            var func = this.func;
            var onReturn = this.onReturn;
            var parameter = this.parameter;
            this.func = null;
            this.onReturn = null;
            this.parameter = default(TParameterType);
            try
            {
                TmphTypePool<TmphRun<TParameterType, TReturnType>>.Push(this);
            }
            finally
            {
                onReturn(func(parameter));
            }
        }

        /// <summary>
        ///     泛型任务信息
        /// </summary>
        /// <param name="func">任务执行委托</param>
        /// <param name="parameter">任务执行参数</param>
        /// <param name="onReturn">返回值执行委托</param>
        /// <returns>泛型任务信息</returns>
        public static Action Create(Func<TParameterType, TReturnType> func, TParameterType parameter,
            Action<TReturnType> onReturn)
        {
            var run = TmphTypePool<TmphRun<TParameterType, TReturnType>>.Pop();
            if (run == null)
            {
                try
                {
                    run = new TmphRun<TParameterType, TReturnType>();
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
                if (run == null) return null;
            }
            run.func = func;
            run.parameter = parameter;
            run.onReturn = onReturn;
            return run.action;
        }
    }
}