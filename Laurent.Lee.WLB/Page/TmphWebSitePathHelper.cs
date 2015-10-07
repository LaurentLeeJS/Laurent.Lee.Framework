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
using System.Web;

namespace Laurent.Lee.WLB
{
    /// <summary>
    /// 网站路径操作类
    /// </summary>
    public static class TmphWebSitePathHelper
    {
        /// <summary>
        ///
        /// </summary>
        public enum TmphSortType
        {
            /// <summary>
            ///
            /// </summary>
            Photo = 1,

            /// <summary>
            ///
            /// </summary>
            Article = 5,

            /// <summary>
            ///
            /// </summary>
            Diary = 7,

            /// <summary>
            ///
            /// </summary>
            Pic = 2,

            /// <summary>
            ///
            /// </summary>
            Music = 6,

            /// <summary>
            ///
            /// </summary>
            AddressList = 4,

            /// <summary>
            ///
            /// </summary>
            Favorite = 3,
        }

        #region 根据给出的相对地址获取网站绝对地址

        /// <summary>
        /// 根据给出的相对地址获取网站绝对地址
        /// </summary>
        /// <param name="localPath">相对地址</param>
        /// <returns>绝对地址</returns>
        public static string GetWebPath(string localPath)
        {
            string path = HttpContext.Current.Request.ApplicationPath;
            string thisPath;
            string thisLocalPath;
            //如果不是根目录就加上"/" 根目录自己会加"/"
            if (path != "/")
            {
                thisPath = path + "/";
            }
            else
            {
                thisPath = path;
            }
            if (localPath.StartsWith("~/"))
            {
                thisLocalPath = localPath.Substring(2);
            }
            else
            {
                return localPath;
            }
            return thisPath + thisLocalPath;
        }

        #endregion 根据给出的相对地址获取网站绝对地址

        #region 获取网站绝对地址

        /// <summary>
        ///  获取网站绝对地址
        /// </summary>
        /// <returns></returns>
        public static string GetWebPath()
        {
            string path = System.Web.HttpContext.Current.Request.ApplicationPath;
            string thisPath;
            //如果不是根目录就加上"/" 根目录自己会加"/"
            if (path != "/")
            {
                thisPath = path + "/";
            }
            else
            {
                thisPath = path;
            }
            return thisPath;
        }

        #endregion 获取网站绝对地址

        #region 根据相对路径或绝对路径获取绝对路径

        /// <summary>
        /// 根据相对路径或绝对路径获取绝对路径
        /// </summary>
        /// <param name="localPath">相对路径或绝对路径</param>
        /// <returns>绝对路径</returns>
        public static string GetFilePath(string localPath)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(localPath, @"([A-Za-z]):\\([\S]*)"))
            {
                return localPath;
            }
            else
            {
                return System.Web.HttpContext.Current.Server.MapPath(localPath);
            }
        }

        #endregion 根据相对路径或绝对路径获取绝对路径
    }
}