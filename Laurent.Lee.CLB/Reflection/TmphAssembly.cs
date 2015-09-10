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