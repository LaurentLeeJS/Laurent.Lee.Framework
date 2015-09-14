using System;
using System.IO;

namespace Laurent.Lee.CLB.IO
{
    /// <summary>
    /// 目录相关操作
    /// </summary>
    public static class TmphDirectory
    {
        /// <summary>
        /// 目录分隔符\替换
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>替换\后的路径</returns>
        public static string pathSeparator(this string path)
        {
            if (Path.DirectorySeparatorChar != '\\') path.Replace('\\', Path.DirectorySeparatorChar);
            return path;
        }
    }
}
