﻿using System;
using Laurent.Lee.CLB.Code;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    /// web查询字符串类型配置
    /// </summary>
    public sealed class TmphUrlQuery : TmphMemberFilter.TmphInstanceField
    {
        /// <summary>
        /// 默认web查询字符串类型配置
        /// </summary>
        public static readonly TmphUrlQuery AllMember = new TmphUrlQuery { IsAllMember = true };
        /// <summary>
        /// 是否序列化所有成员
        /// </summary>
        public bool IsAllMember;
        /// <summary>
        /// web查询字符串成员配置
        /// </summary>
        public sealed class TmphMember : TmphIgnoreMember
        {
        }
        /// <summary>
        /// 自定义类型函数标识配置
        /// </summary>
        public sealed class TmphCustom : Attribute
        {
        }
    }
}
