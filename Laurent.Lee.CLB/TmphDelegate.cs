namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     字符串转换委托
    /// </summary>
    /// <typeparam name="TReturnType">目标类型</typeparam>
    /// <param name="stringValue">字符串</param>
    /// <param name="value">目标对象</param>
    /// <returns>是否转换成功</returns>
    public delegate bool TmphTryParse<TReturnType>(string stringValue, out TReturnType value);

    /// <summary>
    ///     入池函数调用委托
    /// </summary>
    /// <typeparam name="TParameterType">输入参数类型</typeparam>
    /// <param name="parameter">输入参数</param>
    public delegate void TmphPushPool<TParameterType>(ref TParameterType parameter);
}