using Laurent.Lee.CLB.Code;
using System;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    /// web表单类型配置
    /// </summary>
    public sealed class TmphForm : TmphMemberFilter.TmphInstanceField
    {
        /// <summary>
        /// 默认web表单类型配置
        /// </summary>
        public static readonly TmphForm AllMember = new TmphForm { IsAllMember = true };

        /// <summary>
        /// 是否序列化所有成员
        /// </summary>
        public bool IsAllMember;

        /// <summary>
        /// web表单成员配置
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