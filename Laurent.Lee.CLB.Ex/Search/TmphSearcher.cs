using System;
using System.Collections.Generic;
using System.Globalization;
using Laurent.Lee.CLB.Threading;

namespace Laurent.Lee.CLB.Search
{
    /// <summary>
    /// 分词搜索器
    /// </summary>
    public abstract class TmphSearcher
    {
        /// <summary>
        /// 字符类型
        /// </summary>
        private enum TmphCharType
        {
            /// <summary>
            /// 未知
            /// </summary>
            Unknown,
            /// <summary>
            /// 其它字母
            /// </summary>
            OtherLetter,
            /// <summary>
            /// 字母
            /// </summary>
            Letter,
            /// <summary>
            /// 数字
            /// </summary>
            Number,
            /// <summary>
            /// 保留字符
            /// </summary>
            Keep,
        }
        /// <summary>
        /// 分词trie图
        /// </summary>
        protected TmphTrieGraph wordTrieGraph;
        /// <summary>
        /// 结果访问锁
        /// </summary>
        protected int resultLock;
        /// <summary>
        /// 最大搜索字符串长度
        /// </summary>
        protected int maxSearchSize;
        /// <summary>
        /// 总词频
        /// </summary>
        protected int wordCount = 1;
        /// <summary>
        /// 分词搜索器
        /// </summary>
        /// <param name="wordTrieGraph">分词trie图</param>
        /// <param name="maxSearchSize">最大搜索字符串长度</param>
        protected TmphSearcher(TmphTrieGraph wordTrieGraph, int maxSearchSize)
        {
            this.maxSearchSize = maxSearchSize < 1 ? 1 : maxSearchSize;
            this.wordTrieGraph = wordTrieGraph;
        }
        /// <summary>
        /// 文本分词
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="length">文本长度</param>
        /// <returns>分词结果</returns>
        private unsafe TmphList<TmphSubString> getWords(string text, int length)
        {
            fixed (char* textFixed = text)
            {
                TmphSimplified.Format(textFixed, length);
                int count = (length + 7) >> 3;
                byte* match = stackalloc byte[count];
                TmphFixedMap matchMap = new TmphFixedMap(match, count, 0);
                TmphList<TmphSubString> words = TmphTypePool<TmphList<TmphSubString>>.Pop();
                if (words == null) words = new TmphList<TmphSubString>();
                else if (words.Count != 0) words.Clear();
                TmphList<TmphKeyValue<int, int>> matchs = TmphTypePool<TmphList<TmphKeyValue<int, int>>>.Pop() ?? new TmphList<TmphKeyValue<int, int>>();
                byte* charTypes = charTypePointer.Byte;
                for (char* start = textFixed, end = textFixed + length; start != end; )
                {
                    if (*start == ' ')
                    {
                        *end = '?';
                        while (*++start == ' ') ;
                    }
                    else
                    {
                        *end = ' ';
                        char* segment = start;
                        if ((uint)(*start - 0x4E00) <= 0X9FA5 - 0x4E00)
                        {
                            while ((uint)(*++start - 0x4E00) <= 0X9FA5 - 0x4E00) ;
                            if ((length = (int)(start - segment)) == 1)
                            {
                                words.Add(TmphSubString.Unsafe(text, (int)(segment - textFixed), 1));
                            }
                            else
                            {
                                int startIndex = (int)(segment - textFixed);
                                matchs.Empty();
                                wordTrieGraph.LeftRightMatchs(TmphSubString.Unsafe(text, startIndex, length), matchs);
                                if ((count = matchs.Count) != 0)
                                {
                                    foreach (TmphKeyValue<int, int> value in matchs.Unsafer.Array)
                                    {
                                        words.Add(TmphSubString.Unsafe(text, value.Key, value.Value));
                                        matchMap.Set(value.Key, value.Value);
                                        if (--count == 0) break;
                                    }
                                }
                                int index = startIndex;
                                for (int endIndex = startIndex + length; index != endIndex; ++index)
                                {
                                    if (matchMap.Get(index))
                                    {
                                        if ((count = index - startIndex) != 1)
                                        {
                                            words.Add(TmphSubString.Unsafe(text, startIndex, count));
                                        }
                                        startIndex = index;
                                    }
                                    else words.Add(TmphSubString.Unsafe(text, index, 1));
                                }
                                if ((index -= startIndex) > 1) words.Add(TmphSubString.Unsafe(text, startIndex, index));
                            }
                        }
                        else
                        {
                            byte type = charTypes[*start];
                            if (type == (byte)TmphCharType.OtherLetter)
                            {
                                while (charTypes[*++start] == (byte)TmphCharType.OtherLetter) ;
                            }
                            else
                            {
                                char* word = start;
                                for (byte newType = charTypes[*++start]; newType >= (byte)TmphCharType.Letter; newType = charTypes[*++start])
                                {
                                    if (type != newType)
                                    {
                                        if (type != (byte)TmphCharType.Keep)
                                        {
                                            words.Add(TmphSubString.Unsafe(text, (int)(word - textFixed), (int)(start - word)));
                                        }
                                        type = newType;
                                        word = start;
                                    }
                                }
                            }
                            words.Add(TmphSubString.Unsafe(text, (int)(segment - textFixed), (int)(start - segment)));
                        }
                    }
                }
                TmphTypePool<TmphList<TmphKeyValue<int, int>>>.Push(ref matchs);
                if ((count = words.Count) == 0)
                {
                    TmphTypePool<TmphList<TmphSubString>>.Push(ref words);
                    return null;
                }
                return words;
            }
        }
        /// <summary>
        /// 搜索字符串分词
        /// </summary>
        /// <param name="text">搜索字符串</param>
        /// <returns>分词结果</returns>
        protected TmphList<TmphSubString> getWords(string text)
        {
            if (text != null)
            {
                int length = text.Length;
                if (length != 0)
                {
                    TmphList<TmphSubString> words = length <= maxSearchSize ? getWords(text + " ", length) : getWords(text, maxSearchSize);
                    if (words != null)
                    {
                        int index = words.Count;
                        if (index > 1)
                        {
                            TmphSubString[] wordArray = words.Unsafer.Array;
                            int count = 0;
                            if (words.Count <= 4)
                            {
                                foreach (TmphSubString word in wordArray)
                                {
                                    if (count == 0) count = 1;
                                    else
                                    {
                                        int nextIndex = count;
                                        foreach (TmphSubString cmpWord in wordArray)
                                        {
                                            if (cmpWord.Equals(word) || --nextIndex == 0) break;
                                        }
                                        if (nextIndex == 0) wordArray[count++] = word;
                                    }
                                    if (--index == 0) break;
                                }
                            }
                            else
                            {
                                HashSet<TmphHashString> wordHash = TmphTypePool<HashSet<TmphHashString>>.Pop();
                                if (wordHash == null) wordHash = TmphHashSet.CreateHashString();
                                else if (wordHash.Count != 0) wordHash.Clear();
                                foreach (TmphSubString word in wordArray)
                                {
                                    if (count == 0)
                                    {
                                        wordHash.Add(word);
                                        count = 1;
                                    }
                                    else
                                    {
                                        TmphHashString wordKey = word;
                                        if (!wordHash.Contains(wordKey))
                                        {
                                            wordArray[count++] = word;
                                            wordHash.Add(wordKey);
                                        }
                                    }
                                    if (--index == 0) break;
                                }
                                wordHash.Clear();
                                TmphTypePool<HashSet<TmphHashString>>.Push(ref wordHash);
                            }
                            words.Unsafer.AddLength(count - words.Count);
                        }
                    }
                    return words;
                }
            }
            return null;
        }
        /// <summary>
        /// 数据文本分词
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>分词结果</returns>
        protected unsafe Dictionary<TmphHashString, TmphList<int>> getAllWords(string text)
        {
            if (text != null)
            {
                int length = text.Length;
                if (length != 0)
                {
                    TmphList<TmphSubString> words = getWords(text + " ", length);
                    if (words != null)
                    {
                        Dictionary<TmphHashString, TmphList<int>> dictionary = TmphTypePool<Dictionary<TmphHashString, TmphList<int>>>.Pop();
                        if (dictionary == null) dictionary = Laurent.Lee.CLB.TmphDictionary.CreateHashString<TmphList<int>>();
                        else if (dictionary.Count != 0) dictionary.Clear();
                        TmphList<int> indexs;
                        int count = words.Count;
                        foreach (TmphSubString word in words.Unsafer.Array)
                        {
                            TmphHashString wordKey = word;
                            if (!dictionary.TryGetValue(wordKey, out indexs))
                            {
                                indexs = TmphTypePool<TmphList<int>>.Pop();
                                if (indexs == null) indexs = new TmphList<int>();
                                else indexs.Empty();
                                dictionary.Add(wordKey, indexs);
                            }
                            indexs.Add(word.StartIndex);
                            if (--count == 0) break;
                        }
                        words.Clear();
                        TmphTypePool<TmphList<TmphSubString>>.Push(ref words);
                        return dictionary;
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 释放分词结果
        /// </summary>
        /// <param name="values">分词结果</param>
        protected static void free(Dictionary<TmphHashString, TmphList<int>> values)
        {
            foreach (TmphList<int> indexs in values.Values) TmphTypePool<TmphList<int>>.Push(indexs);
            values.Clear();
            TmphTypePool<Dictionary<TmphHashString, TmphList<int>>>.Push(ref values);
        }
        /// <summary>
        /// 字符类型集合
        /// </summary>
        private static TmphPointer charTypePointer;
        unsafe static TmphSearcher()
        {
            charTypePointer = TmphUnmanaged.Get(65536, true);
            byte* start = charTypePointer.Byte, end = charTypePointer.Byte + 65536;
            for (char TmphCode = (char)0; start != end; ++start, ++TmphCode)
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(TmphCode);
                if (category == UnicodeCategory.LowercaseLetter || category == UnicodeCategory.UppercaseLetter
                        || category == UnicodeCategory.TitlecaseLetter || category == UnicodeCategory.ModifierLetter)
                {
                    *start = (byte)TmphCharType.Letter;
                }
                else if (category == UnicodeCategory.DecimalDigitNumber
                        || category == UnicodeCategory.LetterNumber || category == UnicodeCategory.OtherNumber)
                {
                    *start = (byte)TmphCharType.Number;
                }
                else if (category == UnicodeCategory.OtherLetter) *start = (byte)TmphCharType.OtherLetter;
                else if (TmphCode == '&' || TmphCode == '.' || TmphCode == '+' || TmphCode == '#') *start = (byte)TmphCharType.Keep;
            }
        }
    }
    /// <summary>
    /// 分词搜索器
    /// </summary>
    /// <typeparam name="TKeyType">数据标识类型</typeparam>
    public sealed class TmphSearcher<TKeyType> : TmphSearcher where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        /// 关键字词频与数据结果
        /// </summary>
        private struct TmphCounter
        {
            /// <summary>
            /// 词频
            /// </summary>
            public int Count;
            /// <summary>
            /// 数据结果
            /// </summary>
            public Dictionary<TKeyType, int[]> Values;
            /// <summary>
            /// 添加数据结果
            /// </summary>
            /// <param name="key">关键字</param>
            /// <param name="values">数据结果</param>
            public void Add(TKeyType key, int[] values)
            {
                Values.Add(key, values);
                Count += values.Length;
            }
            /// <summary>
            /// 删除关键字
            /// </summary>
            /// <param name="key">关键字</param>
            /// <returns>删除结果数量</returns>
            public int Remove(TKeyType key)
            {
                int[] indexs;
                if (Values.TryGetValue(key, out indexs))
                {
                    Values.Remove(key);
                    Count -= indexs.Length;
                    return indexs.Length;
                }
                return 0;
            }
        }
        /// <summary>
        /// 关键字数据结果池
        /// </summary>
        private static TmphCounter[] counterPool = new TmphCounter[256];
        /// <summary>
        /// 当前分配结果索引
        /// </summary>
        private static int counterIndex;
        /// <summary>
        /// 关键字数据结果池访问锁
        /// </summary>
        private static int counterLock;
        /// <summary>
        /// 获取结果索引
        /// </summary>
        /// <returns>结果索引</returns>
        private static int getCounterIndex()
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref counterLock);
            try
            {
                if (counterIndex == counterPool.Length)
                {
                    TmphCounter[] newPool = new TmphCounter[counterIndex << 1];
                    counterPool.CopyTo(newPool, 0);
                    counterPool = newPool;
                }
                counterPool[counterIndex].Values = TmphDictionary<TKeyType>.Create<int[]>();
                return counterIndex++;
            }
            finally { counterLock = 0; }
        }
        /// <summary>
        /// 关键字数据结果集合
        /// </summary>
        private Dictionary<TmphHashString, int> results = TmphDictionary.CreateHashString<int>();
        /// <summary>
        /// 分词搜索器
        /// </summary>
        /// <param name="wordTrieGraph">分词trie图</param>
        /// <param name="values">数据集合[关键字+数据对象]</param>
        /// <param name="maxSearchSize">最大搜索字符串长度</param>
        public TmphSearcher(TmphTrieGraph wordTrieGraph, TmphKeyValue<string, TKeyType>[] values, int maxSearchSize = 128)
            : base(wordTrieGraph, maxSearchSize)
        {
            foreach (TmphKeyValue<string, TKeyType> value in values) add(value.Key, value.Value);
        }
        /// <summary>
        /// 添加新的数据
        /// </summary>
        /// <param name="key">数据标识</param>
        /// <param name="values">分词结果</param>
        private void add(TKeyType key, Dictionary<TmphHashString, TmphList<int>> values)
        {
            int counterIndex;
            foreach (KeyValuePair<TmphHashString, TmphList<int>> result in values)
            {
                if (!results.TryGetValue(result.Key, out counterIndex)) results.Add(result.Key, counterIndex = getCounterIndex());
                counterPool[counterIndex].Add(key, result.Value.GetArray());
                wordCount += result.Value.Count;
            }
        }
        /// <summary>
        /// 添加新的数据
        /// </summary>
        /// <param name="text">数据文本</param>
        /// <param name="key">数据标识</param>
        private void add(string text, TKeyType key)
        {
            Dictionary<TmphHashString, TmphList<int>> values = getAllWords(text);
            if (values != null)
            {
                add(key, values);
                free(values);
            }
        }
        /// <summary>
        /// 添加新的数据
        /// </summary>
        /// <param name="text">数据文本</param>
        /// <param name="key">数据标识</param>
        public void Add(string text, TKeyType key)
        {
            Dictionary<TmphHashString, TmphList<int>> values = getAllWords(text);
            if (values != null)
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref resultLock);
                try
                {
                    add(key, values);
                }
                finally
                {
                    resultLock = 0;
                    free(values);
                }
            }
        }
        /// <summary>
        /// 删除新的数据
        /// </summary>
        /// <param name="key">数据标识</param>
        /// <param name="values">分词结果</param>
        private void remove(TKeyType key, Dictionary<TmphHashString, TmphList<int>> values)
        {
            TmphCounter[] pool = counterPool;
            int counterIndex;
            foreach (KeyValuePair<TmphHashString, TmphList<int>> result in values)
            {
                if (results.TryGetValue(result.Key, out counterIndex)) wordCount -= pool[counterIndex].Remove(key);
            }
        }
        /// <summary>
        /// 删除无效的数据
        /// </summary>
        /// <param name="text">数据文本</param>
        /// <param name="key">数据标识</param>
        public void Remove(string text, TKeyType key)
        {
            Dictionary<TmphHashString, TmphList<int>> values = getAllWords(text);
            if (values != null)
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref resultLock);
                try
                {
                    remove(key, values);
                }
                finally
                {
                    resultLock = 0;
                    free(values);
                }
            }
        }
        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="text">数据文本</param>
        /// <param name="oldText">旧数据文本</param>
        /// <param name="key">数据标识</param>
        public void Update(string text, string oldText, TKeyType key)
        {
            Dictionary<TmphHashString, TmphList<int>> values = getAllWords(text);
            Dictionary<TmphHashString, TmphList<int>> oldValues = getAllWords(oldText);
            TmphInterlocked.NoCheckCompareSetSleep0(ref resultLock);
            try
            {
                if (oldValues != null) remove(key, oldValues);
                if (values != null) add(key, values);
            }
            finally
            {
                resultLock = 0;
                if (oldValues != null) free(oldValues);
                if (values != null) free(values);
            }
        }
        ///// <summary>
        ///// 判断是否存在匹配项
        ///// </summary>
        ///// <param name="text">搜索文本</param>
        ///// <param name="isMatch">匹配委托</param>
        ///// <returns>是否存在匹配项</returns>
        //public bool IsMatch(string text, func<Dictionary<TKeyType, int[]>, bool> isMatch)
        //{
        //    TmphList<TmphSubString> words = getWords(text);
        //    if (words != null)
        //    {
        //        counter counter;
        //        int count = words.Count;
        //        TmphInterlocked.CompareSetSleep0NoCheck(ref resultLock);
        //        try
        //        {
        //            foreach (subString word in words.Unsafer.Array)
        //            {
        //                if (!results.TryGetValue(word, out counter) || !isMatch(counter.Values)) return false;
        //                if (--count == 0) break;
        //            }
        //        }
        //        finally
        //        {
        //            resultLock = 0;
        //            words.Clear();
        //            typePool<TmphList<TmphSubString>>.Push(words);
        //        }
        //        return true;
        //    }
        //    return false;
        //}
        /// <summary>
        /// 搜索匹配项
        /// </summary>
        /// <param name="text">搜索文本</param>
        /// <param name="list">匹配项集合</param>
        /// <param name="merge">匹配项集合归并处理</param>
        public void Search(string text, TmphList<Dictionary<TKeyType, int[]>> list, Action merge)
        {
            if (list != null)
            {
                TmphList<TmphSubString> words = getWords(text);
                if (words != null)
                {
                    TmphCounter[] pool = counterPool;
                    int count = words.Count, counterIndex;
                    TmphInterlocked.NoCheckCompareSetSleep0(ref resultLock);
                    try
                    {
                        foreach (TmphSubString word in words.Unsafer.Array)
                        {
                            if (results.TryGetValue(word, out counterIndex)) list.Add(pool[counterIndex].Values);
                            if (--count == 0) break;
                        }
                        if (list.Count != 0) merge();
                    }
                    finally
                    {
                        resultLock = 0;
                        words.Clear();
                        TmphTypePool<TmphList<TmphSubString>>.Push(ref words);
                    }
                }
            }
        }
        static TmphSearcher()
        {
            if (Laurent.Lee.CLB.Config.TmphAppSetting.IsCheckMemory) TmphCheckMemory.Add(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }
    }
}
