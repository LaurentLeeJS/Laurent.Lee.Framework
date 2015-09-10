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
using Laurent.Lee.CLB.Reflection;
using System;

namespace Laurent.Lee.CLB.Code
{
    /// <summary>
    ///     远程类型
    /// </summary>
    [TmphDataSerialize(IsMemberMap = false)]
    public struct TmphRemoteType
    {
        /// <summary>
        ///     程序集名称
        /// </summary>
        private readonly string _assemblyName;

        /// <summary>
        ///     类型名称
        /// </summary>
        private readonly string _name;

        /// <summary>
        ///     远程类型
        /// </summary>
        /// <param name="type">类型</param>
        public TmphRemoteType(Type type)
        {
            _name = type.FullName;
            _assemblyName = type.Assembly.FullName;
        }

        /// <summary>
        ///     是否空类型
        /// </summary>
        public bool IsNull
        {
            get { return _assemblyName == null || _name == null; }
        }

        /// <summary>
        ///     类型
        /// </summary>
        public Type Type
        {
            get
            {
                Type type;
                if (TryGet(out type)) return type;
                TmphLog.Default.Throw(null, "未能加载类型 : " + _name + " in " + _assemblyName);
                return null;
            }
        }

        /// <summary>
        ///     类型隐式转换
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>远程类型</returns>
        public static implicit operator TmphRemoteType(Type type)
        {
            return new TmphRemoteType(type);
        }

        /// <summary>
        ///     尝试获取类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>是否成功</returns>
        public bool TryGet(out Type type)
        {
            var assembly = TmphAssembly.Get(_assemblyName);
            if (assembly != null)
            {
                if ((type = assembly.GetType(_name)) != null) return true;
            }
            else type = null;
            return false;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _assemblyName + " + " + _name;
        }
    }
}