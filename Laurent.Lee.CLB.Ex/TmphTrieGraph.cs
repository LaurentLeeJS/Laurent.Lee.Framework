using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Laurent.Lee.CLB
{
    public abstract class TmphTrieGraph<TKeyType, TValueType> : IDisposable
        where TKeyType : struct, IEquatable<TKeyType>
        where TValueType : class
    {
        public struct TmphNode
        {
            public Dictionary<TKeyType, int> Nodes;

            public int NodeCount
            {
                get { return Nodes == null ? 0 : Nodes.Count; }
            }

            public int Link;
            public TValueType Value;

            public void Clear()
            {
                Link = 0;
                Value = null;
            }

            public void SetNodes()
            {
                if (Nodes == null) Nodes = TmphDictionary.Create<TKeyType, int>();
            }

            public void SetBoot()
            {
                Clear();
                SetNodes();
            }

            public void Free()
            {
                Value = null;
                if (Nodes != null)
                {
                    foreach (int node in Nodes.Values) freeNodeNoLock(node);
                    Nodes.Clear();
                }
            }

            public int Create(TKeyType letter)
            {
                int node;
                if (Nodes == null)
                {
                    Nodes = TmphDictionary.Create<TKeyType, int>();
                    Nodes[letter] = node = nextNodeNoLock();
                }
                else if (!Nodes.TryGetValue(letter, out node))
                {
                    Nodes[letter] = node = nextNodeNoLock();
                }
                return node;
            }

            public int GetNodeCount(int link)
            {
                Link = link;
                return Nodes == null ? 0 : Nodes.Count;
            }

            public int GetNodeCount(Dictionary<TKeyType, int> boot, TKeyType letter)
            {
                boot.TryGetValue(letter, out Link);
                return Nodes == null ? 0 : Nodes.Count;
            }

            public int GetLinkWhereNull(TKeyType letter, ref int node, ref int link)
            {
                if (Nodes == null || Nodes.Count == 0 || !Nodes.TryGetValue(letter, out node))
                {
                    return link = Link;
                }
                return 0;
            }

            public int GetNodeOrLink(TKeyType letter, ref int node, ref int link, out TValueType value)
            {
                value = Value;
                if (Nodes == null || Nodes.Count == 0 || !Nodes.TryGetValue(letter, out node))
                {
                    return link = Link;
                }
                return 0;
            }

            public TValueType GetLink(ref int link)
            {
                link = Link;
                return Value;
            }
        }

        public static TmphNode[] NodePool = new TmphNode[256];
        private static readonly TmphList<int> freeNodes = new TmphList<int>();
        private static int currentNode = 1;
        protected static int nodeLock;

        //private static int nextNode()
        //{
        //    int node;
        //    TmphInterlocked.CompareSetSleep0NoCheck(ref nodeLock);
        //    if (freeNodes.Count == 0)
        //    {
        //        if (currentNode == nodePool.Length)
        //        {
        //            try
        //            {
        //                newNode[] nodes = new newNode[currentNode << 1];
        //                node = currentNode;
        //                nodePool.CopyTo(nodes, 0);
        //                ++currentNode;
        //                nodePool = nodes;
        //            }
        //            finally { nodeLock = 0; }
        //        }
        //        else
        //        {
        //            node = currentNode++;
        //            nodeLock = 0;
        //        }
        //    }
        //    else
        //    {
        //        nodePool[node = freeNodes.Unsafer.Pop()].Clear();
        //        nodeLock = 0;
        //    }
        //    return node;
        //}
        private static int getBootNode()
        {
            int node;
            TmphInterlocked.NoCheckCompareSetSleep0(ref nodeLock);
            if (freeNodes.Count == 0)
            {
                if (currentNode == NodePool.Length)
                {
                    try
                    {
                        TmphNode[] newNodes = new TmphNode[currentNode << 1];
                        node = currentNode;
                        NodePool.CopyTo(newNodes, 0);
                        ++currentNode;
                        NodePool = newNodes;
                        newNodes[node].SetNodes();
                    }
                    finally { nodeLock = 0; }
                }
                else
                {
                    NodePool[node = currentNode].SetNodes();
                    ++currentNode;
                    nodeLock = 0;
                }
            }
            else
            {
                NodePool[node = freeNodes.Unsafer.Pop()].SetBoot();
                nodeLock = 0;
            }
            return node;
        }

        private static int nextNodeNoLock()
        {
            if (freeNodes.Count == 0)
            {
                if (currentNode == NodePool.Length)
                {
                    TmphNode[] newNodes = new TmphNode[currentNode << 1];
                    NodePool.CopyTo(newNodes, 0);
                    NodePool = newNodes;
                }
                return currentNode++;
            }
            int node = freeNodes.Unsafer.Pop();
            NodePool[node].Clear();
            return node;
        }

        private static void freeNode(int node)
        {
            TmphInterlocked.NoCheckCompareSetSleep0(ref nodeLock);
            try
            {
                freeNodeNoLock(node);
            }
            finally { nodeLock = 0; }
        }

        private static void freeNodeNoLock(int node)
        {
            NodePool[node].Free();
            freeNodes.Add(node);
        }

        private struct TmphGraphBuilder
        {
            public TmphNode[] pool;
            public Dictionary<TKeyType, int> Boot;
            public TmphList<int> Writer;
            private int[] reader;
            private int startIndex;
            private int endIndex;

            public void Set(TmphNode node)
            {
                Boot = node.Nodes;
                pool = NodePool;
                Writer = new TmphList<int>();
            }

            public void Set(int[] reader, int startIndex, int endIndex)
            {
                this.reader = reader;
                this.startIndex = startIndex;
                this.endIndex = endIndex;
            }

            public unsafe void Build()
            {
                Writer.Empty();
                fixed (int* readerFixed = reader)
                {
                    while (startIndex != endIndex)
                    {
                        TmphNode fatherNode = pool[readerFixed[startIndex++]];
                        if (fatherNode.Link == 0)
                        {
                            foreach (KeyValuePair<TKeyType, int> nextNode in fatherNode.Nodes)
                            {
                                if (pool[nextNode.Value].GetNodeCount(Boot, nextNode.Key) != 0) Writer.Add(nextNode.Value);
                            }
                        }
                        else
                        {
                            foreach (KeyValuePair<TKeyType, int> nextNode in fatherNode.Nodes)
                            {
                                int link = fatherNode.Link, linkNode = 0;
                                while (pool[link].GetLinkWhereNull(nextNode.Key, ref linkNode, ref link) != 0) ;
                                if (linkNode == 0) Boot.TryGetValue(nextNode.Key, out linkNode);
                                if (pool[nextNode.Value].GetNodeCount(linkNode) != 0) Writer.Add(nextNode.Value);
                            }
                        }
                    }
                }
            }
        }

        protected int boot = getBootNode();

        public void BuildGraph(int threadCount = 0)
        {
            if (threadCount > TmphPub.CpuCount) threadCount = TmphPub.CpuCount;
            if (threadCount > 1) buildGraph(threadCount);
            else buildGraph();
        }

        public void Dispose()
        {
            int boot = Interlocked.Exchange(ref this.boot, int.MinValue);
            if (boot > 0) freeNode(boot);
        }

        private unsafe void buildGraph()
        {
            TmphNode bootNode = NodePool[boot];
            TmphGraphBuilder TmphBuilder = new TmphGraphBuilder();
            TmphBuilder.Set(bootNode);
            TmphInterlocked.NoCheckCompareSetSleep0(ref nodeLock);
            try
            {
                TmphList<int> reader = getBuildGraphReader(bootNode);
                while (reader.Count != 0)
                {
                    TmphBuilder.Set(reader.Unsafer.Array, 0, reader.Count);
                    TmphBuilder.Build();
                    TmphList<int> values = reader;
                    reader = TmphBuilder.Writer;
                    TmphBuilder.Writer = values;
                }
            }
            finally { nodeLock = 0; }
        }

        private unsafe void buildGraph(int threadCount)
        {
            TmphNode bootNode = NodePool[boot];
            TmphInterlocked.NoCheckCompareSetSleep0(ref nodeLock);
            try
            {
                TmphList<int> reader = getBuildGraphReader(bootNode);
                if (reader.Count != 0)
                {
                    int taskCount = threadCount - 1;
                    TmphGraphBuilder[] builders = new TmphGraphBuilder[threadCount];
                    for (int builderIndex = 0; builderIndex != builders.Length; builders[builderIndex++].Set(bootNode)) ;
                    using (TmphTask task = new TmphTask(taskCount))
                    {
                        do
                        {
                            int[] readerArray = reader.Unsafer.Array;
                            int count = reader.Count / threadCount, index = 0;
                            for (int builderIndex = 0; builderIndex != taskCount; ++builderIndex)
                            {
                                builders[builderIndex].Set(readerArray, index, index += count);
                                task.Add(builders[builderIndex].Build);
                            }
                            builders[taskCount].Set(readerArray, index, reader.Count);
                            builders[taskCount].Build();
                            task.WaitFree();
                            reader.Empty();
                            for (int builderIndex = 0; builderIndex != builders.Length; ++builderIndex)
                            {
                                TmphList<int> TmphWriter = builders[builderIndex].Writer;
                                if (TmphWriter.Count != 0) reader.Add(TmphWriter.Unsafer.Array, 0, TmphWriter.Count);
                            }
                        }
                        while (reader.Count != 0);
                    }
                }
            }
            finally { nodeLock = 0; }
        }

        private static unsafe TmphList<int> getBuildGraphReader(TmphNode boot)
        {
            TmphNode[] pool = NodePool;
            TmphList<int> reader = new TmphList<int>(boot.Nodes.Count);
            fixed (int* readerFixed = reader.Unsafer.Array)
            {
                int* write = readerFixed;
                foreach (int node in boot.Nodes.Values)
                {
                    TmphNode nodeValue = pool[node];
                    if (nodeValue.Nodes != null && nodeValue.Nodes.Count != 0) *write++ = node;
                }
                reader.AddLength((int)(write - readerFixed));
            }
            return reader;
        }

        static TmphTrieGraph()
        {
            if (Config.TmphAppSetting.IsCheckMemory) TmphCheckMemory.Add(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }
    }
}