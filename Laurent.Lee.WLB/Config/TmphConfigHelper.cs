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
using System.Configuration;

namespace Laurent.Lee.WLB
{
    /// <summary>
    /// web.config操作类
    /// </summary>
    public sealed class TmphConfigHelper
    {
        /// <summary>
        /// 得到AppSettings中的配置字符串信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetConfigString(string key)
        {
            string CacheKey = "AppSettings-" + key;
            object objModel = TmphDataCache.GetCache(CacheKey);
            if (objModel == null)
            {
                try
                {
                    objModel = ConfigurationManager.AppSettings[key];
                    if (objModel != null)
                    {
                        TmphDataCache.SetCache(CacheKey, objModel, DateTime.Now.AddMinutes(180), TimeSpan.Zero);
                    }
                }
                catch
                { }
            }
            return objModel.ToString();
        }

        /// <summary>
        /// 得到AppSettings中的配置Bool信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool GetConfigBool(string key)
        {
            bool result = false;
            string cfgVal = GetConfigString(key);
            if (null != cfgVal && string.Empty != cfgVal)
            {
                try
                {
                    result = bool.Parse(cfgVal);
                }
                catch (FormatException)
                {
                    // Ignore format exceptions.
                }
            }
            return result;
        }

        /// <summary>
        /// 得到AppSettings中的配置Decimal信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static decimal GetConfigDecimal(string key)
        {
            decimal result = 0;
            string cfgVal = GetConfigString(key);
            if (null != cfgVal && string.Empty != cfgVal)
            {
                try
                {
                    result = decimal.Parse(cfgVal);
                }
                catch (FormatException)
                {
                    // Ignore format exceptions.
                }
            }

            return result;
        }

        /// <summary>
        /// 得到AppSettings中的配置int信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int GetConfigInt(string key)
        {
            int result = 0;
            string cfgVal = GetConfigString(key);
            if (null != cfgVal && string.Empty != cfgVal)
            {
                try
                {
                    result = int.Parse(cfgVal);
                }
                catch (FormatException)
                {
                    // Ignore format exceptions.
                }
            }

            return result;
        }
    }
}