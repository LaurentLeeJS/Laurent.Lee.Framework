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

namespace Laurent.Lee.WLB
{
    /// <summary>
    /// BaseRandom
    /// 产生随机数
    ///
    /// 随机数管理，最大值、最小值可以自己进行设定。
    /// </summary>
    public class TmphBaseRandom
    {
        public static int Minimum = 100000;
        public static int Maximal = 999999;
        public static int RandomLength = 6;

        private static string RandomString = "0123456789ABCDEFGHIJKMLNOPQRSTUVWXYZ";
        private static Random Random = new Random(DateTime.Now.Second);

        #region public static string GetRandomString() 产生随机字符

        /// <summary>
        /// 产生随机字符
        /// </summary>
        /// <returns>字符串</returns>
        public static string GetRandomString()
        {
            string returnValue = string.Empty;
            for (int i = 0; i < RandomLength; i++)
            {
                int r = Random.Next(0, RandomString.Length - 1);
                returnValue += RandomString[r];
            }
            return returnValue;
        }

        #endregion public static string GetRandomString() 产生随机字符

        #region public static int GetRandom()

        /// <summary>
        /// 产生随机数
        /// </summary>
        /// <returns>随机数</returns>
        public static int GetRandom()
        {
            return Random.Next(Minimum, Maximal);
        }

        #endregion public static int GetRandom()

        #region public static int GetRandom(int minimum, int maximal)

        /// <summary>
        /// 产生随机数
        /// </summary>
        /// <param name="minimum">最小值</param>
        /// <param name="maximal">最大值</param>
        /// <returns>随机数</returns>
        public static int GetRandom(int minimum, int maximal)
        {
            return Random.Next(minimum, maximal);
        }

        #endregion public static int GetRandom(int minimum, int maximal)
    }
}