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

using Laurent.Lee.CLB.Threading;
using System;
using System.Reflection;

namespace Laurent.Lee.CLB.Reflection
{
    /// <summary>
    ///     程序集扩展操作
    /// </summary>
    public static class TmphAssembly
    {
        /// <summary>
        ///     程序集名称缓存
        /// </summary>
        private static TmphInterlocked.TmphDictionary<TmphHashString, Assembly> nameCache;

        static TmphAssembly()
        {
            nameCache = new TmphInterlocked.TmphDictionary<TmphHashString, Assembly>(TmphDictionary.CreateHashString<Assembly>());
            AppDomain.CurrentDomain.AssemblyLoad += loadAssembly;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                nameCache.Set(assembly.FullName, assembly);
        }

        /// <summary>
        ///     根据程序集名称获取程序集
        /// </summary>
        /// <param name="fullName">程序集名称</param>
        /// <returns>程序集,失败返回null</returns>
        public static Assembly Get(string fullName)
        {
            if (fullName != null)
            {
                Assembly value;
                if (nameCache.TryGetValue(fullName, out value)) return value;
            }
            return null;
        }

        /// <summary>
        ///     获取类型信息
        /// </summary>
        /// <param name="assembly">程序集信息</param>
        /// <param name="fullName">类型全名</param>
        /// <returns>类型信息</returns>
        public static Type getType(this Assembly assembly, string fullName)
        {
            return assembly != null ? assembly.GetType(fullName) : null;
        }

        /// <summary>
        ///     加载程序集
        /// </summary>
        /// <param name="assembly">程序集</param>
        private static void loadAssembly(Assembly assembly)
        {
            nameCache.Set(assembly.FullName, assembly);
        }

        /// <summary>
        ///     加载程序集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void loadAssembly(object sender, AssemblyLoadEventArgs args)
        {
            loadAssembly(args.LoadedAssembly);
        }
    }
}