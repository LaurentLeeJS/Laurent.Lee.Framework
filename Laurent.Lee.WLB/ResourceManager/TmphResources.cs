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
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Laurent.Lee.WLB
{
    /// <summary>
    /// BUResourceManager
    /// 资源管理器
    /// </author>
    /// </summary>
    [XmlRoot("resources")]
    public class TmphResources
    {
        private SortedList<String, String> indexs = new SortedList<String, String>();

        [XmlElement("language")]
        public string language = string.Empty;

        [XmlElement("displayName")]
        public string displayName = string.Empty;

        [XmlElement("version")]
        public string version = string.Empty;

        [XmlElement("author")]
        public string author = string.Empty;

        [XmlElement("description")]
        public string description = string.Empty;

        [XmlElement("items", typeof(Items))]
        public Items items;

        public void createIndex()
        {
            indexs.Clear();
            if (items == null)
            {
                return;
            }
            indexs = new SortedList<String, String>(items.items.Length);
            for (int i = 0; i < items.items.Length; i++)
            {
#if DEBUG
                try
                {
                    indexs.Add(items.items[i].key, items.items[i].value);
                }
                catch
                {
                    throw (new Exception(items.items[i].key + items.items[i].value));
                }
#else
                    indexs.Add(items.items[i].key, items.items[i].value);
#endif
            }
        }

        public string Get(string key)
        {
            if (!indexs.ContainsKey(key))
            {
                return string.Empty;
            }
            return indexs[key];
        }

        /// <summary>
        /// JiRiGaLa 2007.05.02
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Set(string key, string value)
        {
            if (!indexs.ContainsKey(key))
            {
                return false;
            }
            indexs[key] = value;
            for (int i = 0; i < items.items.Length; i++)
            {
                if (items.items[i].key == key)
                {
                    items.items[i].value = value;
                    break;
                }
            }
            return true;
        }
    }

    public class Items
    {
        [XmlElement("item", typeof(Item))]
        public Item[] items;
    }

    public class Item
    {
        [XmlAttribute("key")]
        public string key = string.Empty;

        [XmlText]
        public string value = string.Empty;
    }

    internal class ResourcesSerializer
    {
        public static TmphResources DeSerialize(string filePath)
        {
            System.Xml.Serialization.XmlSerializer XmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(TmphResources));
            System.IO.FileStream FileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Open);
            TmphResources Resources = XmlSerializer.Deserialize(FileStream) as TmphResources;
            FileStream.Close();
            return Resources;
        }

        public static void Serialize(string filePath, TmphResources Resources)
        {
            System.Xml.Serialization.XmlSerializer XmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(TmphResources));
            System.IO.FileStream FileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create);
            XmlSerializer.Serialize(FileStream, Resources);
            FileStream.Close();
        }
    }
}