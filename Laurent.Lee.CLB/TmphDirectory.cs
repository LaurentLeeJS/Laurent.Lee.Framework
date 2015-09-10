using System;
using System.IO;

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     目录相关操作
    /// </summary>
    public static class TmphDirectory
    {
        /// <summary>
        ///     目录分隔符
        /// </summary>
        public static readonly string DirectorySeparator = Path.DirectorySeparatorChar.ToString();

        /// <summary>
        ///     取以\结尾的路径全名
        /// </summary>
        /// <param name="path">目录</param>
        /// <returns>\结尾的路径全名</returns>
        public static string fullName(this DirectoryInfo path)
        {
            return path != null ? path.FullName.pathSuffix() : null;
        }

        /// <summary>
        ///     路径补全结尾的\
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>路径</returns>
        public static string pathSuffix(this string path)
        {
            if (path.Length() != 0)
            {
                return Unsafe.TmphString.Last(path) == Path.DirectorySeparatorChar ? path : (path + DirectorySeparator);
            }
            return DirectorySeparator;
        }

        /// <summary>
        ///     创建目录
        /// </summary>
        /// <param name="path">目录</param>
        /// <returns>是否创建成功</returns>
        public static bool Create(string path)
        {
            if (path != null)
            {
                if (Directory.Exists(path)) return true;
                try
                {
                    Directory.CreateDirectory(path);
                    return true;
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, "目录创建失败 : " + path, false);
                }
            }
            return false;
        }
    }
}