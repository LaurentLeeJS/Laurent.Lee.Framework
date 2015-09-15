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
using System.IO;
using System.Text;

#if MONO
#else

using Laurent.Lee.CLB.Win32;

#endif

namespace Laurent.Lee.CLB.IO
{
    /// <summary>
    ///     文件操作
    /// </summary>
    public static class TmphFile
    {
        /// <summary>
        ///     完全限定文件名必须少于 260 个字符
        /// </summary>
        public const int MaxFullNameLength = 260;

        /// <summary>
        ///     文件编码BOM集合
        /// </summary>
        private static readonly TmphUniqueDictionary<TmphEncodingBom, TmphBom> boms;

        /// <summary>
        ///     默认簇字节大小
        /// </summary>
        public static readonly uint DefaultBytesPerCluster;

        static TmphFile()
        {
            var bomList = new TmphKeyValue<TmphEncodingBom, TmphBom>[4];
            var count = 0;
            bomList[count++] = new TmphKeyValue<TmphEncodingBom, TmphBom>(Encoding.Unicode, new TmphBom { Bom = 0xfeffU, Length = 2 });
            bomList[count++] = new TmphKeyValue<TmphEncodingBom, TmphBom>(Encoding.BigEndianUnicode,
                new TmphBom { Bom = 0xfffeU, Length = 2 });
            bomList[count++] = new TmphKeyValue<TmphEncodingBom, TmphBom>(Encoding.UTF8, new TmphBom { Bom = 0xbfbbefU, Length = 3 });
            bomList[count++] = new TmphKeyValue<TmphEncodingBom, TmphBom>(Encoding.UTF32, new TmphBom { Bom = 0xfeffU, Length = 4 });
            boms = new TmphUniqueDictionary<TmphEncodingBom, TmphBom>(bomList, 4);

            DefaultBytesPerCluster = bytesPerCluster(AppDomain.CurrentDomain.BaseDirectory);
            if (DefaultBytesPerCluster == 0) DefaultBytesPerCluster = 1 << 12;
        }

        /// <summary>
        ///     根据磁盘根目录获取簇字节大小
        /// </summary>
        /// <param name="bootPath">磁盘根目录，如@"C:\"</param>
        /// <returns>簇字节大小</returns>
        public static uint BytesPerCluster(string bootPath)
        {
#if MONO
#else
            if (bootPath.Length >= 3 && bootPath[1] == ':')
            {
                var value = bytesPerCluster(bootPath[bootPath.Length - 1] == '\\' ? bootPath : bootPath.Substring(0, 3));
                if (value != 0) return value;
            }
#endif
            return DefaultBytesPerCluster;
        }

        /// <summary>
        ///     根据磁盘根目录获取簇字节大小
        /// </summary>
        /// <param name="bootPath">磁盘根目录，如@"C:\"</param>
        /// <returns>簇字节大小</returns>
        private static uint bytesPerCluster(string bootPath)
        {
#if MONO
#else
            uint sectorsPerCluster, bytesPerSector, numberOfFreeClusters, totalNumberOfClusters;
            if (TmphKernel32.GetDiskFreeSpace(bootPath, out sectorsPerCluster, out bytesPerSector,
                out numberOfFreeClusters, out totalNumberOfClusters))
            {
                return sectorsPerCluster * bytesPerSector;
            }
#endif
            return 0;
        }

        /// <summary>
        ///     修改文件名成为默认备份文件 %XXX_fileName
        /// </summary>
        /// <param name="fileName">源文件名</param>
        /// <returns>备份文件名称,失败返回null</returns>
        public static string MoveBak(string fileName)
        {
            if (File.Exists(fileName))
            {
                var newFileName = MoveBakFileName(fileName);
                File.Move(fileName, newFileName);
                return newFileName;
            }
            return null;
        }

        /// <summary>
        ///     获取备份文件名称 %XXX_fileName
        /// </summary>
        /// <param name="fileName">源文件名</param>
        /// <returns>备份文件名称</returns>
        internal static string MoveBakFileName(string fileName)
        {
            var timeIndex = 0;
            string newFileName = null;
            var index = fileName.LastIndexOf(Path.DirectorySeparatorChar) + 1;
            do
            {
                var bakName = "%" + TmphDate.NowSecond.ToString("yyyyMMdd-HHmmss") +
                              (timeIndex == 0 ? null : ("_" + timeIndex.toString())) + "_";
                newFileName = index != 0 ? fileName.Insert(index, bakName) : (bakName + fileName);
                ++timeIndex;
            } while (File.Exists(newFileName));
            return newFileName;
        }

        /// <summary>
        ///     根据文件编码获取BOM
        /// </summary>
        /// <param name="encoding">文件编码</param>
        /// <returns>文件编码BOM</returns>
        public static TmphBom GetBom(Encoding encoding)
        {
            return boms.Get(encoding, default(TmphBom));
        }

        /// <summary>
        ///     文件编码BOM唯一哈希
        /// </summary>
        private struct TmphEncodingBom : IEquatable<TmphEncodingBom>
        {
            /// <summary>
            ///     文件编码
            /// </summary>
            public Encoding Encoding;

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="other">待匹配数据</param>
            /// <returns>是否相等</returns>
            public bool Equals(TmphEncodingBom other)
            {
                return Encoding == other.Encoding;
            }

            /// <summary>
            ///     隐式转换
            /// </summary>
            /// <param name="encoding">文件编码</param>
            /// <returns>文件编码BOM唯一哈希</returns>
            public static implicit operator TmphEncodingBom(Encoding encoding)
            {
                return new TmphEncodingBom { Encoding = encoding };
            }

            /// <summary>
            ///     获取哈希值
            /// </summary>
            /// <returns>哈希值</returns>
            public override int GetHashCode()
            {
                var codePage = Encoding.CodePage;
                return ((codePage >> 3) | codePage) & 3;
            }

            /// <summary>
            ///     判断是否相等
            /// </summary>
            /// <param name="obj">待匹配数据</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((TmphEncodingBom)obj);
            }
        }

        /// <summary>
        ///     文件编码BOM
        /// </summary>
        public struct TmphBom
        {
            /// <summary>
            ///     BOM
            /// </summary>
            public uint Bom;

            /// <summary>
            ///     BOM长度
            /// </summary>
            public int Length;
        }
    }
}