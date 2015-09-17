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
using System.Runtime.InteropServices;
using System.Security;

namespace Laurent.Lee.WLB
{
    /// <summary>
    /// 执行需要调用 <b>Win32</b> API 的操作辅助类。
    /// </summary>
    [SuppressUnmanagedCodeSecurity()]
    public static partial class TmphWin32
    {
        /// <summary>
        /// 执行当前类在使用前的初始化操作。
        /// </summary>
        static TmphWin32()
        {
            currentOs = GetCurrentPlatform();
        }

        /// <summary>
        /// 获取当前用户物理磁盘的性能信息。
        /// </summary>
        /// <returns>一个 <see cref="TmphHDiskInfo"/> 结构，它保存了物理硬盘的性能数据。</returns>
        public static TmphHDiskInfo GetHddInformation()
        {
            switch (currentOs)
            {
                case (TmphPlatform.Windows95):
                case (TmphPlatform.Windows98):
                case (TmphPlatform.WindowsCE):
                case (TmphPlatform.WindowsNT351):
                case (TmphPlatform.WindowsNT40):
                default:
                    throw new PlatformNotSupportedException(string.Format(TmphResourcesApi.Win32_CurrentPlatformNotSupport, currentOs.ToString()));
                case (TmphPlatform.UnKnown):
                    throw new PlatformNotSupportedException(TmphResourcesApi.Win32_CurrentPlatformUnknown);
                case (TmphPlatform.Windows982ndEdition):
                case (TmphPlatform.WindowsME):
                    return GetHddInfo9X(0);

                case (TmphPlatform.Windows2000):
                case (TmphPlatform.WindowsXP):
                case (TmphPlatform.Windows2003):
                case (TmphPlatform.WindowsVista):
                    return GetHddInfoNT(0);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nXAmount"></param>
        /// <param name="nYAmount"></param>
        /// <param name="rectScrollRegion"></param>
        /// <param name="rectClip"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool ScrollWindow(HandleRef hWnd, int nXAmount, int nYAmount, ref TmphRECT rectScrollRegion, ref TmphRECT rectClip);

        /// <summary>
        ///
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nBar"></param>
        /// <param name="nPos"></param>
        /// <param name="bRedraw"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int SetScrollPos(HandleRef hWnd, int nBar, int nPos, bool bRedraw);

        /// <summary>
        ///
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="fnBar"></param>
        /// <param name="si"></param>
        /// <param name="redraw"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int SetScrollInfo(HandleRef hWnd, int fnBar, TmphSCROLLINFO si, bool redraw);

        /// <summary>
        ///
        /// </summary>
        /// <param name="hDC"></param>
        /// <param name="nIndex"></param>
        /// <returns></returns>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int GetDeviceCaps(HandleRef hDC, int nIndex);

        /// <summary>
        ///
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int LOWORD(int n)
        {
            return (n & 0xffff);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int LOWORD(IntPtr n)
        {
            return LOWORD((int)((long)n));
        }

        public static int HIWORD(int n)
        {
            return ((n >> 0x10) & 0xffff);
        }

        public static int HIWORD(IntPtr n)
        {
            return HIWORD((int)(long)n);
        }
    }
}