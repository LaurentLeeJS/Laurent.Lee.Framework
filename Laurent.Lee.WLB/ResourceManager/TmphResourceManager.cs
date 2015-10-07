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
using System.Collections.Generic;
using System.IO;

namespace Laurent.Lee.WLB
{
    /// <summary>
    /// ResourceManager
    /// </author>
    /// </summary>
    public class TmphResourceManager
    {
        private volatile static TmphResourceManager instance = null;
        private static object locker = new Object();

        public static TmphResourceManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locker)
                    {
                        if (instance == null)
                        {
                            instance = new TmphResourceManager();
                        }
                    }
                }
                return instance;
            }
        }

        private string FolderPath = string.Empty;
        public SortedList<String, TmphResources> LanguageResources = new SortedList<String, TmphResources>();

        public void Serialize(TmphResources resources, string filePath)
        {
            ResourcesSerializer.Serialize(filePath, resources);
        }

        public void Init(string filePath)
        {
            FolderPath = filePath;
            DirectoryInfo directoryInfo = new DirectoryInfo(filePath);
            LanguageResources.Clear();
            if (!directoryInfo.Exists)
            {
                return;
            }
            FileInfo[] FileInfo = directoryInfo.GetFiles();
            for (int i = 0; i < FileInfo.Length; i++)
            {
                TmphResources resources = ResourcesSerializer.DeSerialize(FileInfo[i].FullName);
                resources.createIndex();
                LanguageResources.Add(resources.language, resources);
            }
        }

        public Hashtable GetLanguages()
        {
            Hashtable hashtable = new Hashtable();
            IEnumerator<KeyValuePair<String, TmphResources>> iEnumerator = LanguageResources.GetEnumerator();
            while (iEnumerator.MoveNext())
            {
                hashtable.Add(iEnumerator.Current.Key, iEnumerator.Current.Value.displayName);
            }
            return hashtable;
        }

        public Hashtable GetLanguages(string path)
        {
            Hashtable hashtable = new Hashtable();
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists)
            {
                return hashtable;
            }
            FileInfo[] fileInfo = directoryInfo.GetFiles();
            for (int i = 0; i < fileInfo.Length; i++)
            {
                TmphResources resources = ResourcesSerializer.DeSerialize(fileInfo[i].FullName);
                hashtable.Add(resources.language, resources.displayName);
            }
            return hashtable;
        }

        public TmphResources GetResources(string filePath)
        {
            TmphResources resources = new TmphResources();
            if (File.Exists(filePath))
            {
                resources = ResourcesSerializer.DeSerialize(filePath);
                resources.createIndex();
            }
            return resources;
        }

        public string Get(string language, string key)
        {
            if (!LanguageResources.ContainsKey(language))
            {
                return string.Empty;
            }
            return LanguageResources[language].Get(key);
        }
    }
}