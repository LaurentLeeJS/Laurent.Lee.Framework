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
using System.Collections;

namespace Laurent.Lee.WLB
{
    /// <summary>
    /// ResourceManagerWrapper
    /// </author>
    /// </summary>
    public class TmphResourceManagerWrapper
    {
        private volatile static TmphResourceManagerWrapper instance = null;
        private static object locker = new Object();
        private static string CurrentLanguage = "en-us";

        public static TmphResourceManagerWrapper Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locker)
                    {
                        if (instance == null)
                        {
                            instance = new TmphResourceManagerWrapper();
                        }
                    }
                }
                return instance;
            }
        }

        private TmphResourceManager ResourceManager;

        public TmphResourceManagerWrapper()
        {
        }

        public void LoadResources(string path)
        {
            ResourceManager = TmphResourceManager.Instance;
            ResourceManager.Init(path);
        }

        public string Get(string key)
        {
            return ResourceManager.Get(CurrentLanguage, key);
        }

        public string Get(string lanauage, string key)
        {
            return ResourceManager.Get(lanauage, key);
        }

        public Hashtable GetLanguages()
        {
            return ResourceManager.GetLanguages();
        }

        public Hashtable GetLanguages(string path)
        {
            return ResourceManager.GetLanguages(path);
        }

        public void Serialize(string path, string language, string key, string value)
        {
            TmphResources Resources = this.GetResources(path, language);
            Resources.Set(key, value);
            string filePath = path + "\\" + language + ".xml";
            ResourceManager.Serialize(Resources, filePath);
        }

        public TmphResources GetResources(string path, string language)
        {
            string filePath = path + "\\" + language + ".xml";
            return ResourceManager.GetResources(filePath);
        }

        public TmphResources GetResources(string language)
        {
            return ResourceManager.LanguageResources[language];
        }
    }
}