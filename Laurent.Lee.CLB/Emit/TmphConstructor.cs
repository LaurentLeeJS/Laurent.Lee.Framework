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
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    ///     默认构造函数
    /// </summary>
    public sealed class TmphConstructor : Attribute
    {
    }

    /// <summary>
    ///     默认构造函数
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public static class TmphConstructor<TValueType>
    {
        /// <summary>
        ///     默认构造函数
        /// </summary>
        public static readonly Func<TValueType> New;

        static TmphConstructor()
        {
            var type = typeof(TValueType);
            if (type.IsValueType || type.IsArray || type == typeof(string))
            {
                New = Default;
                return;
            }
            if (TmphTypeAttribute.GetAttribute<TmphConstructor>(type, false, true) != null)
            {
                foreach (var methodInfo in TmphAttributeMethod.GetStatic(type))
                {
                    if (methodInfo.Method.ReflectedType == type && methodInfo.Method.GetParameters().Length == 0 &&
                        methodInfo.GetAttribute<TmphConstructor>(true) != null)
                    {
                        New = (Func<TValueType>)Delegate.CreateDelegate(typeof(Func<TValueType>), methodInfo.Method);
                        return;
                    }
                }
            }
            if (!type.IsInterface && !type.IsAbstract)
            {
                var constructorInfo =
                    type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                        TmphNullValue<Type>.Array, null);
                if (constructorInfo != null)
                {
                    var dynamicMethod = new DynamicMethod("constructor", type, TmphNullValue<Type>.Array, type, true);
                    dynamicMethod.InitLocals = true;
                    var generator = dynamicMethod.GetILGenerator();
                    generator.Emit(OpCodes.Newobj, constructorInfo);
                    generator.Emit(OpCodes.Ret);
                    New = (Func<TValueType>)dynamicMethod.CreateDelegate(typeof(Func<TValueType>));
                }
            }
        }

        /// <summary>
        ///     默认空值
        /// </summary>
        /// <returns>默认空值</returns>
        public static TValueType Default()
        {
            return default(TValueType);
        }
    }
}