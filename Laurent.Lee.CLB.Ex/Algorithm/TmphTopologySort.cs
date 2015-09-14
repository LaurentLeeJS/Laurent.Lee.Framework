using System.Collections.Generic;

namespace Laurent.Lee.CLB.Algorithm
{
    /// <summary>
    ///     拓扑排序
    /// </summary>
    public static class TmphTopologySort
    {
        /// <summary>
        ///     拓扑排序
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="edges">边集合</param>
        /// <param name="points">无边点集合</param>
        /// <param name="isDesc">是否反向排序</param>
        /// <returns>排序结果</returns>
        public static TValueType[] Sort<TValueType>(ICollection<TmphKeyValue<TValueType, TValueType>> edges,
            HashSet<TValueType> points
            , bool isDesc = false)
        {
            var graph = TmphDictionary.CreateAny<TValueType, TmphList<TValueType>>();
            var edgePoints = default(TmphList<TValueType>.TmphUnsafer);
            if (edges != null)
            {
                edgePoints = new TmphList<TValueType>(edges.Count).Unsafer;
                TmphList<TValueType> values;
                foreach (var edge in edges)
                {
                    if (!graph.TryGetValue(edge.Key, out values))
                        graph.Add(edge.Key, values = new TmphList<TValueType>());
                    values.Add(edge.Value);
                    edgePoints.Add(edge.Value);
                }
                var pointArray = edgePoints.Array;
                var count = 0;
                if (points.Count() != 0)
                {
                    while (count != pointArray.Length)
                    {
                        var point = pointArray[count];
                        if (graph.ContainsKey(point) || points.Contains(point)) break;
                        ++count;
                    }
                    if (count != pointArray.Length)
                    {
                        for (var index = count; ++index != pointArray.Length;)
                        {
                            var point = pointArray[index];
                            if (!graph.ContainsKey(point) && !points.Contains(point)) pointArray[count++] = point;
                        }
                    }
                }
                else
                {
                    while (count != pointArray.Length && !graph.ContainsKey(pointArray[count])) ++count;
                    if (count != pointArray.Length)
                    {
                        for (var index = count; ++index != pointArray.Length;)
                        {
                            var point = pointArray[index];
                            if (!graph.ContainsKey(point)) pointArray[count++] = point;
                        }
                    }
                }
                edgePoints.AddLength(count - pointArray.Length);
            }
            var pointList = edgePoints.List;
            if (points.Count() != 0)
            {
                if (pointList == null) pointList = points.GetList();
                else
                {
                    foreach (var point in points)
                    {
                        if (!graph.ContainsKey(point)) pointList.Add(point);
                    }
                }
            }
            return new TmphSorter<TValueType>(graph, pointList, isDesc).Sort();
        }

        /// <summary>
        ///     拓扑排序器
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        private struct TmphSorter<TValueType>
        {
            /// <summary>
            ///     图
            /// </summary>
            private readonly Dictionary<TValueType, TmphList<TValueType>> _graph;

            /// <summary>
            ///     排序结果
            /// </summary>
            private readonly TValueType[] _values;

            /// <summary>
            ///     是否反向排序
            /// </summary>
            private readonly bool isDesc;

            /// <summary>
            ///     当前排序位置
            /// </summary>
            private int _index;

            /// <summary>
            ///     拓扑排序器
            /// </summary>
            /// <param name="graph">图</param>
            /// <param name="points">单点集合</param>
            /// <param name="isDesc">是否反向排序</param>
            public TmphSorter(Dictionary<TValueType, TmphList<TValueType>> graph, TmphList<TValueType> points, bool isDesc)
            {
                _graph = graph;
                this.isDesc = isDesc;
                _values = new TValueType[graph.Count + points.Count()];
                if (isDesc)
                {
                    _index = points.Count();
                    points.copyTo(_values, 0);
                }
                else
                {
                    points.copyTo(_values, _index = graph.Count);
                }
            }

            /// <summary>
            ///     拓扑排序
            /// </summary>
            /// <returns>排序结果</returns>
            public TValueType[] Sort()
            {
                TmphList<TValueType> points;
                if (isDesc)
                {
                    foreach (var point in _graph.GetArray(value => value.Key))
                    {
                        if (_graph.TryGetValue(point, out points))
                        {
                            _graph[point] = null;
                            foreach (var nextPoint in points) PopDesc(nextPoint);
                            _graph.Remove(point);
                            _values[_index++] = point;
                        }
                    }
                }
                else
                {
                    foreach (var point in _graph.GetArray(value => value.Key))
                    {
                        if (_graph.TryGetValue(point, out points))
                        {
                            _graph[point] = null;
                            foreach (var nextPoint in points) Pop(nextPoint);
                            _graph.Remove(point);
                            _values[--_index] = point;
                        }
                    }
                }
                return _values;
            }

            /// <summary>
            ///     排序子节点
            /// </summary>
            /// <param name="point">子节点</param>
            private void Pop(TValueType point)
            {
                TmphList<TValueType> points;
                if (_graph.TryGetValue(point, out points))
                {
                    if (points == null) TmphLog.Error.Throw("拓扑排序循环");
                    _graph[point] = null;
                    if (points != null) foreach (var nextPoint in points) Pop(nextPoint);
                    _graph.Remove(point);
                    _values[--_index] = point;
                }
            }

            /// <summary>
            ///     排序子节点
            /// </summary>
            /// <param name="point">子节点</param>
            private void PopDesc(TValueType point)
            {
                TmphList<TValueType> points;
                if (_graph.TryGetValue(point, out points))
                {
                    if (points == null) TmphLog.Error.Throw("拓扑排序循环");
                    _graph[point] = null;
                    if (points != null) foreach (var nextPoint in points) PopDesc(nextPoint);
                    _graph.Remove(point);
                    _values[_index++] = point;
                }
            }
        }
    }
}