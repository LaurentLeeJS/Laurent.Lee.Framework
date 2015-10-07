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

namespace Laurent.Lee.CLB.OpenAPI.QQ
{
    /// <summary>
    /// 访问令牌
    /// </summary>
    public struct token
    {
        /// <summary>
        /// 访问令牌
        /// </summary>
        public string access_token;

        /// <summary>
        /// 有效期，单位为秒
        /// </summary>
        public int expires_in;

        /// <summary>
        /// 访问令牌是否有效
        /// </summary>
        public bool IsToken
        {
            get
            {
                return access_token != null && expires_in != 0;
            }
        }

        /// <summary>
        /// 获取用户身份的标识
        /// </summary>
        /// <returns>用户身份的标识</returns>
        public openId GetOpenId()
        {
            if (IsToken)
            {
                string json = config.Request.Request(@"https://graph.qq.com/oauth2.0/me?access_token=" + access_token);
                if (json != null)
                {
                    bool isError = false, isJson = false;
                    openId openId = new openId();
                    try
                    {
                        if (Laurent.Lee.CLB.Emit.TmphJsonParser.Parse(formatJson(json), ref openId)) isJson = true;
                    }
                    catch (Exception error)
                    {
                        isError = true;
                        TmphLog.Error.Add(error, json, false);
                    }
                    if (isJson && openId.openid != null) return openId;
                    if (!isError) TmphLog.Default.Add(json, false, false);
                }
            }
            return default(openId);
        }

        /// <summary>
        /// 格式化json，去掉函数调用
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private static TmphSubString formatJson(string json)
        {
            int functionIndex = json.IndexOf('(');
            if (functionIndex != -1)
            {
                int objectIndex = json.IndexOf('{');
                if (objectIndex == -1)
                {
                    int arrayIndex = json.IndexOf('[');
                    if (arrayIndex != -1 && functionIndex < arrayIndex)
                    {
                        return new TmphSubString(json, ++functionIndex, json.LastIndexOf(')') - functionIndex);
                    }
                }
                else if (functionIndex < objectIndex)
                {
                    int arrayIndex = json.IndexOf('[');
                    if (arrayIndex == -1 || functionIndex < arrayIndex)
                    {
                        return new TmphSubString(json, ++functionIndex, json.LastIndexOf(')') - functionIndex);
                    }
                }
            }
            return json;
        }
    }
}