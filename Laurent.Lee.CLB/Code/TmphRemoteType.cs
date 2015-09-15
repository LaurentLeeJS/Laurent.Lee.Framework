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