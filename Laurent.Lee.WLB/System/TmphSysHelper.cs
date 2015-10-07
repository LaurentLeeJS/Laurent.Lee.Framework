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
using System;
using System.Diagnostics;
using System.Threading;
using System.Web;

namespace Laurent.Lee.WLB
{
    /// <summary>
    /// 系统操作相关的公共类
    /// </summary>
    public static class TmphSysHelper
    {
        #region 获取文件相对路径映射的物理路径

        /// <summary>
        /// 获取文件相对路径映射的物理路径
        /// </summary>
        /// <param name="virtualPath">文件的相对路径</param>
        public static string GetPath(string virtualPath)
        {
            return HttpContext.Current.Server.MapPath(virtualPath);
        }

        #endregion 获取文件相对路径映射的物理路径

        #region 获取指定调用层级的方法名

        /// <summary>
        /// 获取指定调用层级的方法名
        /// </summary>
        /// <param name="level">调用的层数</param>
        public static string GetMethodName(int level)
        {
            //创建一个堆栈跟踪
            StackTrace trace = new StackTrace();

            //获取指定调用层级的方法名
            return trace.GetFrame(level).GetMethod().Name;
        }

        #endregion 获取指定调用层级的方法名

        #region 获取GUID值

        /// <summary>
        /// 获取GUID值
        /// </summary>
        public static string NewGUID
        {
            get
            {
                return Guid.NewGuid().ToString();
            }
        }

        #endregion 获取GUID值

        #region 获取换行字符

        /// <summary>
        /// 获取换行字符
        /// </summary>
        public static string NewLine
        {
            get
            {
                return Environment.NewLine;
            }
        }

        #endregion 获取换行字符

        #region 获取当前应用程序域

        /// <summary>
        /// 获取当前应用程序域
        /// </summary>
        public static AppDomain CurrentAppDomain
        {
            get
            {
                return Thread.GetDomain();
            }
        }

        #endregion 获取当前应用程序域
    }
}