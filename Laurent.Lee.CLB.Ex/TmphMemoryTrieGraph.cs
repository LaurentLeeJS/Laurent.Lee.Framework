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

using Laurent.Lee.CLB.Threading;
using System.Collections.Generic;

namespace Laurent.Lee.CLB
{
    public abstract class TmphMemoryTrieGraph : TmphTrieGraph<byte, byte[]>
    {
        private unsafe struct TmphTreeBuilder
        {
            private byte* end;
            public int Node;

            public void Build(byte[] keys)
            {
                fixed (byte* start = keys)
                {
                    end = start + keys.Length;
                    build(start);
                }
                NodePool[Node].Value = keys;
            }

            private void build(byte* start)
            {
                Node = NodePool[Node].Create(*start);
                if (++start != end) build(start);
            }
        }

        public void BuildTree(IEnumerable<byte[]> keys)
        {
            if (keys != null)
            {
                TmphTreeBuilder treeBuilder = new TmphTreeBuilder();
                TmphInterlocked.NoCheckCompareSetSleep0(ref nodeLock);
                try
                {
                    foreach (byte[] key in keys)
                    {
                        if (key != null && key.Length != 0)
                        {
                            treeBuilder.Node = boot;
                            treeBuilder.Build(key);
                        }
                    }
                }
                finally { nodeLock = 0; }
            }
        }

        public unsafe bool IsMatchLess(byte[] values)
        {
            if (values != null && values.Length != 0)
            {
                fixed (byte* valueFixed = values) return isMatchLess(valueFixed, valueFixed + values.Length);
            }
            return false;
        }

        public unsafe bool IsMatchLess(TmphSubArray<byte> values)
        {
            if (values.Count != 0)
            {
                fixed (byte* valueFixed = values.Array)
                {
                    byte* start = valueFixed + values.StartIndex;
                    return isMatchLess(start, start + values.Count);
                }
            }
            return false;
        }

        private unsafe bool isMatchLess(byte* start, byte* end)
        {
            TmphNode[] pool = NodePool;
            Dictionary<byte, int> bootNode = pool[boot].Nodes;
            byte[] value;
            for (int node = boot, nextNode = 0; start != end; ++start)
            {
                byte letter = *start;
                if (pool[node].GetLinkWhereNull(letter, ref nextNode, ref node) != 0)
                {
                    while (node != 0)
                    {
                        int isGetValue = pool[node].GetNodeOrLink(letter, ref nextNode, ref node, out value);
                        if (value != null) return true;
                        if (isGetValue == 0) break;
                    }
                    if (node == 0 && !bootNode.TryGetValue(letter, out nextNode)) nextNode = boot;
                }
                if (pool[nextNode].Value != null) return true;
                node = nextNode;
            }
            return false;
        }
    }
}