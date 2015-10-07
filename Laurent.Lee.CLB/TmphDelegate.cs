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