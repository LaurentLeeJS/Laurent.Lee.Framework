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

namespace Laurent.Lee.CLB.Code
{
    /// <summary>
    ///     建树器
    /// </summary>
    /// <typeparam name="TNodeType">树节点类型</typeparam>
    /// <typeparam name="TTagType">树节点标识类型</typeparam>
    public sealed class TmphTreeBuilder<TNodeType, TTagType>
        where TNodeType : TmphTreeBuilder<TNodeType, TTagType>.TmphINode
        where TTagType : IEquatable<TTagType>
    {
        /// <summary>
        ///     当前节点集合
        /// </summary>
        private readonly TmphList<TmphKeyValue<TNodeType, bool>> _nodes = new TmphList<TmphKeyValue<TNodeType, bool>>();

        ///// <summary>
        ///// 树节点回合检测器,必须回合返回true
        ///// </summary>
        //private func<nodeType, bool> checkRound;
        ///// <summary>
        ///// 建树器
        ///// </summary>
        ///// <param name="checkRound">树节点回合检测器,必须回合返回true</param>
        //public treeBuilder(func<nodeType, bool> checkRound)
        //{
        //    this.checkRound = checkRound;
        //}
        /// <summary>
        ///     清除节点
        /// </summary>
        public void Empty()
        {
            _nodes.Empty();
        }

        /// <summary>
        ///     追加新节点
        /// </summary>
        /// <param name="node">新节点</param>
        public void Append(TNodeType node)
        {
            _nodes.Add(new TmphKeyValue<TNodeType, bool>(node, true));
        }

        /// <summary>
        ///     追加新节点
        /// </summary>
        /// <param name="node">新节点</param>
        /// <param name="isRound">是否需要判断回合</param>
        public void Append(TNodeType node, bool isRound)
        {
            _nodes.Add(new TmphKeyValue<TNodeType, bool>(node, isRound));
        }

        /// <summary>
        ///     节点回合
        /// </summary>
        /// <param name="tagName">树节点标识</param>
        /// <param name="isAny"></param>
        /// <returns>节点回合状态</returns>
        private TmphCheckType Round(TTagType tagName, bool isAny)
        {
            var array = _nodes.array;
            for (var index = _nodes.Count; index != 0;)
            {
                if (array[--index].Value)
                {
                    var node = array[index].Key;
                    if (tagName.Equals(node.Tag))
                    {
                        array[index].Set(node, false);
                        node.SetChilds(array.sub(++index, _nodes.Count - index).GetArray(value => value.Key));
                        _nodes.Unsafer.AddLength(index - _nodes.Count);
                        return TmphCheckType.Ok;
                    }
                    if (!isAny) return TmphCheckType.LessRound;
                    //else if (checkRound != null && checkRound(node)) return checkType.LessRound;
                }
            }
            return TmphCheckType.UnknownRound;
        }

        /// <summary>
        ///     节点回合
        /// </summary>
        /// <param name="tagName">树节点标识</param>
        public void Round(TTagType tagName)
        {
            var check = Round(tagName, false);
            switch (check)
            {
                case TmphCheckType.LessRound:
                    TmphLog.Error.Throw("缺少回合节点 : " + tagName + @"
" + _nodes.JoinString(@"
", value => value.Key.Tag.ToString()));
                    break;

                case TmphCheckType.UnknownRound:
                    TmphLog.Error.Throw("未知的回合节点 : " + tagName + @"
" + _nodes.JoinString(@"
", value => value.Key.Tag.ToString()));
                    break;
            }
        }

        /// <summary>
        ///     节点回合
        /// </summary>
        /// <param name="tagName">树节点标识</param>
        /// <param name="isAny">是否匹配任意索引位置,否则只能匹配最后一个索引位置</param>
        /// <returns>节点回合是否成功</returns>
        public bool IsRound(TTagType tagName, bool isAny)
        {
            return Round(tagName, isAny) == TmphCheckType.Ok;
        }

        /// <summary>
        ///     建树结束
        /// </summary>
        /// <returns>根节点集合</returns>
        public TNodeType[] End()
        {
            //if (checkRound != null)
            //{
            var array = _nodes.array;
            for (var index = _nodes.Count; index != 0;)
            {
                //if (array[--index].Value && checkRound(array[index].Key))
                if (array[--index].Value)
                {
                    TmphLog.Error.Throw("缺少回合节点 : " + _nodes.JoinString(@" \ ", value => value.Key.Tag.ToString()));
                }
            }
            //}
            return _nodes.GetArray(value => value.Key);
        }

        /// <summary>
        ///     节点接口
        /// </summary>
        public interface TmphINode
        {
            /// <summary>
            ///     树节点标识
            /// </summary>
            TTagType Tag { get; }

            /// <summary>
            ///     设置子节点集合
            /// </summary>
            /// <param name="childs">子节点集合</param>
            void SetChilds(TNodeType[] childs);
        }

        /// <summary>
        ///     检测节点回合状态
        /// </summary>
        private enum TmphCheckType
        {
            /// <summary>
            ///     节点回合成功
            /// </summary>
            Ok,

            /// <summary>
            ///     缺少回合节点
            /// </summary>
            LessRound,

            /// <summary>
            ///     未知的回合节点
            /// </summary>
            UnknownRound
        }
    }
}