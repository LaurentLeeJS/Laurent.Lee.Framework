using System;
using System.Collections.Generic;

namespace Laurent.Lee.CLB
{
    public sealed class TmphSearchTree<TKeyType, TValueType> where TKeyType : IComparable<TKeyType>
    {
        public sealed class TmphNode
        {
            public TmphNode Left;
            public TmphNode Right;
            public int Count;
            public TKeyType Key;
            public TValueType Value;

            public TmphNode(TKeyType key, TValueType value)
            {
                Key = key;
                Value = value;
                Count = 1;
            }

            public TmphNode(int count)
            {
                Count = count;
            }

            public void Remove()
            {
                if (--Count != 0)
                {
                    if (Left != null)
                    {
                        if (Right != null)
                        {
                            if (Right.Count > Left.Count)
                            {
                                if (Right.Left != null)
                                {
                                    TmphNode father = Right, node = Right.Left;
                                    for (--Right.Count; node.Left != null; --father.Count) node = (father = node).Left;
                                    Key = node.Key;
                                    Value = node.Value;
                                    if (node.Count == 1) father.Left = null;
                                    else node.Remove();
                                }
                                else
                                {
                                    Key = Right.Key;
                                    Value = Right.Value;
                                    Right = Right.Right;
                                }
                            }
                            else
                            {
                                if (Left.Right != null)
                                {
                                    TmphNode father = Left, node = Left.Right;
                                    for (--Left.Count; node.Right != null; --father.Count) node = (father = node).Right;
                                    Key = node.Key;
                                    Value = node.Value;
                                    if (node.Count == 1) father.Right = null;
                                    else node.Remove();
                                }
                                else
                                {
                                    Key = Left.Key;
                                    Value = Left.Value;
                                    Left = Left.Left;
                                }
                            }
                        }
                        else copyForm(Left);
                    }
                    else copyForm(Right);
                }
            }

            private void copyForm(TmphNode node)
            {
                Left = node.Left;
                Right = node.Right;
                Key = node.Key;
                Value = node.Value;
            }

            private void changeKey(TmphNode node)
            {
                TKeyType key = Key;
                TValueType value = Value;
                Key = node.Key;
                Value = node.Value;
                node.Key = key;
                node.Value = value;
            }

            public void CheckChange()
            {
                if (Count >= 3)
                {
                    if (Left != null)
                    {
                        if (Right != null) checkChange();
                        else leftToNull();
                    }
                    else rightToNull();
                }
            }

            private void checkChange()
            {
                if (Left.Count > Right.Count)
                {
                    if (Left.Left == null || Left.Right == null) Left.CheckChange();
                    else if (Right.Count == 1) leftToRightNull();
                    else
                    {
                        TmphNode leftLeft = Left.Left, leftRight = Left.Right;
                        if (leftLeft.Count > leftRight.Count)
                        {
                            if (leftLeft.Count > Right.Count)
                            {
                                if (leftLeft.Left == null || leftLeft.Right == null) leftLeft.CheckChange();
                                leftLeftToRight();
                            }
                        }
                        else if (leftRight.Count > Right.Count)
                        {
                            if (leftRight.Left == null || leftRight.Right == null) leftRight.CheckChange();
                            leftRightToRight();
                        }
                    }
                }
                else
                {
                    if (Right.Right == null || Right.Left == null) Right.CheckChange();
                    else if (Left.Count == 1) rightToLeftNull();
                    else
                    {
                        TmphNode rightRight = Right.Right, rightLeft = Right.Left;
                        if (rightRight.Count > rightLeft.Count)
                        {
                            if (rightRight.Count > Left.Count)
                            {
                                if (rightRight.Right == null || rightRight.Left == null) rightRight.CheckChange();
                                rightRightToLeft();
                            }
                        }
                        else if (rightLeft.Count > Left.Count)
                        {
                            if (rightLeft.Right == null || rightLeft.Left == null) rightLeft.CheckChange();
                            rightLeftToLeft();
                        }
                    }
                }
            }

            private bool isCheckChange()
            {
                if (Count >= 3)
                {
                    if (Left != null)
                    {
                        if (Right != null) return isCheckChangeNotNull();
                        leftToNull();
                    }
                    else rightToNull();
                    return true;
                }
                return false;
            }

            private bool isCheckChangeNotNull()
            {
                if (Left.Count > Right.Count)
                {
                    if (Left.Left == null || Left.Right == null) return Left.isCheckChange();
                    if (Right.Count == 1)
                    {
                        leftToRightNull();
                        return true;
                    }
                    TmphNode leftLeft = Left.Left, leftRight = Left.Right;
                    if (leftLeft.Count > leftRight.Count)
                    {
                        if (leftLeft.Count > Right.Count)
                        {
                            if (leftLeft.Left == null || leftLeft.Right == null) return leftLeft.isCheckChange();
                            leftLeftToRight();
                            return true;
                        }
                    }
                    else if (leftRight.Count > Right.Count)
                    {
                        if (leftRight.Left == null || leftRight.Right == null) return leftRight.isCheckChange();
                        leftRightToRight();
                        return true;
                    }
                }
                else
                {
                    if (Right.Right == null || Right.Left == null) return Right.isCheckChange();
                    if (Left.Count == 1)
                    {
                        rightToLeftNull();
                        return true;
                    }
                    TmphNode rightRight = Right.Right, rightLeft = Right.Left;
                    if (rightRight.Count > rightLeft.Count)
                    {
                        if (rightRight.Count > Left.Count)
                        {
                            if (rightRight.Right == null || rightRight.Left == null) return rightRight.isCheckChange();
                            rightRightToLeft();
                            return true;
                        }
                    }
                    else if (rightLeft.Count > Left.Count)
                    {
                        if (rightLeft.Right == null || rightLeft.Left == null) return rightLeft.isCheckChange();
                        rightLeftToLeft();
                        return true;
                    }
                }
                return false;
            }

            private void leftToNull()
            {
                if (Left.Left == null)
                {
                    #region 左节点为null
                    TmphNode leftRight = Left.Right;
                    changeKey(leftRight);
                    Right = leftRight;
                    Left.Right = leftRight.Left;
                    if (leftRight.Left != null) leftRight.Count -= leftRight.Left.Count;
                    leftRight.Left = leftRight.Right;
                    Left.Count -= leftRight.Count;
                    leftRight.Right = null;
                    (Left.Count > leftRight.Count ? Left : leftRight).CheckChange();
                    #endregion 左节点为null
                }
                else if (Left.Right == null)
                {
                    #region 右节点为null
                    TmphNode leftLeft = Left.Left;
                    changeKey(Left);
                    Right = Left;
                    Left = leftLeft;
                    Right.Left = null;
                    Right.Count = 1;
                    checkChange();
                    #endregion 右节点为null
                }
                else
                {
                    #region 分裂左节点
                    TmphNode left = Left, leftLeft = Left.Left, leftRight = Left.Right;
                    changeKey(Left);
                    Right = leftRight;
                    Left = leftLeft;
                    left.Left = leftRight.Right;
                    left.Count = leftRight.Right == null ? 1 : (leftRight.Right.Count + 1);
                    left.Right = null;
                    ++leftRight.Count;
                    leftRight.Right = left;
                    left.CheckChange();
                    #endregion 分裂左节点
                }
            }

            private void rightToNull()
            {
                if (Right.Right == null)
                {
                    #region 右节点为null
                    TmphNode rightLeft = Right.Left;
                    changeKey(rightLeft);
                    Left = rightLeft;
                    Right.Left = rightLeft.Right;
                    if (rightLeft.Right != null) rightLeft.Count -= rightLeft.Right.Count;
                    rightLeft.Right = rightLeft.Left;
                    Right.Count -= rightLeft.Count;
                    rightLeft.Left = null;
                    (Right.Count > rightLeft.Count ? Right : rightLeft).CheckChange();
                    #endregion 右节点为null
                }
                else if (Right.Left == null)
                {
                    #region 左节点为null
                    TmphNode rightRight = Right.Right;
                    changeKey(Right);
                    Left = Right;
                    Right = rightRight;
                    Left.Right = null;
                    Left.Count = 1;
                    checkChange();
                    #endregion 左节点为null
                }
                else
                {
                    #region 分裂右节点
                    TmphNode right = Right, rightRight = Right.Right, rightLeft = Right.Left;
                    changeKey(Right);
                    Left = rightLeft;
                    Right = rightRight;
                    right.Right = rightLeft.Left;
                    right.Count = rightLeft.Left == null ? 1 : (rightLeft.Left.Count + 1);
                    right.Left = null;
                    ++rightLeft.Count;
                    rightLeft.Left = right;
                    right.CheckChange();
                    #endregion 分裂右节点
                }
            }

            private void leftToRightNull()
            {
                TmphNode leftLeft = Left.Left, leftRight = Left.Right;
                int leftRightRightCount = leftRight.Right != null ? leftRight.Right.Count : 0;
                changeKey(Left);
                int leftRightLeftCount = leftRight.Count - leftRightRightCount - 1;
                Left.Left = leftRight.Right;
                Left.Right = Right;
                Left.Count = leftRightRightCount + 2;
                leftRight.Right = Left;
                leftRight.Count += 2;
                Right = leftRight;
                Left = leftLeft;
                if (leftRightLeftCount > leftRightRightCount)
                {
                    if (!Right.Right.isCheckChange()) Right.checkChange();
                }
                else Right.Right.CheckChange();
            }

            private void rightToLeftNull()
            {
                TmphNode rightRight = Right.Right, rightLeft = Right.Left;
                int rightLeftLeftCount = rightLeft.Left != null ? rightLeft.Left.Count : 0;
                changeKey(Right);
                int rightLeftRightCount = rightLeft.Count - rightLeftLeftCount - 1;
                Right.Right = rightLeft.Left;
                Right.Left = Left;
                Right.Count = rightLeftLeftCount + 2;
                rightLeft.Left = Right;
                rightLeft.Count += 2;
                Left = rightLeft;
                Right = rightRight;
                if (rightLeftRightCount > rightLeftLeftCount)
                {
                    if (!Left.Left.isCheckChange()) Left.checkChange();
                }
                else Left.Left.CheckChange();
            }

            private void leftLeftToRight()
            {
                TmphNode leftRight = Left.Right, leftLeft = Left.Left;
                changeKey(leftRight);
                if (Right.Left != null && Right.Right != null)
                {
                    TmphNode rightLeft = Right.Left, rightRight = Right.Right, leftLeftRight = leftLeft.Right;
                    int leftRightLeftCount = leftRight.Left != null ? leftRight.Left.Count : 0;
                    if (leftLeft.Left.Count > leftLeftRight.Count)
                    {
                        if (rightLeft.Count < rightRight.Count)
                        {
                            #region
                            Left.Right = leftRight.Left;
                            int leftRightRightCount = leftRight.Count - leftRightLeftCount - 1;
                            leftLeft.Count += leftRightLeftCount + 1;
                            Right.Left = leftRight;
                            Right.Count += leftRightRightCount + 1;
                            Left.Left = leftLeftRight;
                            leftRight.Count += rightLeft.Count - leftRightLeftCount;
                            Left.Count = leftRightLeftCount + leftLeftRight.Count + 1;
                            leftRight.Left = leftRight.Right;
                            leftLeft.Right = Left;
                            leftRight.Right = rightLeft;
                            Left = leftLeft;
                            if (!leftLeft.isCheckChangeNotNull() && !Right.isCheckChangeNotNull()) leftRight.CheckChange();
                            #endregion
                        }
                        else
                        {
                            #region
                            int rightLeftLeftCount = rightLeft.Left != null ? rightLeft.Left.Count : 0;
                            Right.Left = rightLeft.Right;
                            Left.Right = leftRight.Left;
                            leftLeft.Count += leftRightLeftCount + 1;
                            Left.Left = leftLeftRight;
                            leftRight.Left = leftRight.Right;
                            Right.Count -= rightLeftLeftCount + 1;
                            rightLeft.Right = Right;
                            leftRight.Count += rightLeftLeftCount - leftRightLeftCount;
                            Left.Count = leftRightLeftCount + leftLeftRight.Count + 1;
                            leftRight.Right = rightLeft.Left;
                            rightLeft.Count = leftRight.Count + Right.Count + 1;
                            leftLeft.Right = Left;
                            rightLeft.Left = leftRight;
                            Right = rightLeft;
                            Left = leftLeft;
                            if (!rightLeft.isCheckChangeNotNull() && !Right.Right.isCheckChange() && !leftLeft.isCheckChangeNotNull())
                            {
                                leftRight.CheckChange();
                            }
                            #endregion
                        }
                    }
                    else
                    {
                        int leftLeftRightRightCount = leftLeftRight.Right != null ? leftLeftRight.Right.Count : 0;
                        int leftLeftRightCount = leftLeftRight.Count;
                        if (rightLeft.Count < rightRight.Count)
                        {
                            #region
                            Left.Count = leftLeftRightRightCount + leftRightLeftCount + 1;
                            Left.Left = leftLeftRight.Right;
                            leftRight.Count += rightLeft.Count - leftRightLeftCount;
                            Right.Left = leftRight;
                            leftLeft.Count -= leftLeftRightRightCount + 1;
                            leftLeft.Right = leftLeftRight.Left;
                            Right.Count = leftRight.Count + rightRight.Count + 1;
                            Left.Right = leftRight.Left;
                            leftLeftRight.Count = Left.Count + leftLeft.Count + 1;
                            leftLeftRight.Right = Left;
                            leftRight.Left = leftRight.Right;
                            leftLeftRight.Left = leftLeft;
                            leftRight.Right = rightLeft;
                            Left = leftLeftRight;
                            if (!leftRight.isCheckChange())
                            {
                                if (leftLeft.Left.Count - (leftLeftRightCount - leftLeftRightRightCount - 1) > leftRightLeftCount - leftLeftRightRightCount)
                                {
                                    if (!leftLeft.isCheckChange()) Left.Right.CheckChange();
                                }
                                else if (!Left.Right.isCheckChange()) leftLeft.CheckChange();
                            }
                            #endregion
                        }
                        else
                        {
                            #region
                            int rightLeftLeftCount = rightLeft.Left != null ? rightLeft.Left.Count : 0;
                            Right.Left = rightLeft.Right;
                            Left.Count = leftRightLeftCount + leftLeftRightRightCount + 1;
                            leftLeft.Right = leftLeftRight.Left;
                            Right.Count -= rightLeftLeftCount + 1;
                            rightLeft.Right = Right;
                            leftLeft.Count -= leftLeftRightRightCount + 1;
                            Left.Left = leftLeftRight.Right;
                            leftRight.Count += rightLeftLeftCount - leftRightLeftCount;
                            Left.Right = leftRight.Left;
                            leftLeftRight.Left = leftLeft;
                            leftLeftRight.Count = leftLeft.Count + Left.Count + 1;
                            leftRight.Left = leftRight.Right;
                            leftLeftRight.Right = Left;
                            rightLeft.Count = leftRight.Count + Right.Count + 1;
                            leftRight.Right = rightLeft.Left;
                            Right = rightLeft;
                            rightLeft.Left = leftRight;
                            Left = leftLeftRight;
                            if (!rightLeft.isCheckChangeNotNull() && !Right.Right.isCheckChange())
                            {
                                if (leftLeft.Left.Count - (leftLeftRightCount - leftLeftRightRightCount - 1) > leftRightLeftCount - leftLeftRightRightCount)
                                {
                                    if (!leftLeft.isCheckChange()) Left.Right.CheckChange();
                                }
                                else if (!Left.Right.isCheckChange()) leftLeft.CheckChange();
                            }
                            #endregion
                        }
                    }
                }
                else
                {
                    int leftRightLeftCount = leftRight.Left != null ? leftRight.Left.Count : 0, leftLeftRightCount = leftLeft.Right.Count;
                    if (Right.Left == null)
                    {
                        #region
                        Right.Left = leftRight;
                        leftRight.Count -= leftRightLeftCount;
                        Left.Right = leftRight.Left;
                        Right.Count += leftRight.Count;
                        leftRight.Left = leftRight.Right;
                        leftRight.Right = null;
                        Left.Count = leftRightLeftCount + leftLeftRightCount + 1;
                        leftLeft.Count += leftRightLeftCount + 1;
                        Left.Left = leftLeft.Right;
                        leftLeft.Right = Left;
                        Left = leftLeft;
                        if (!leftRight.isCheckChange() && !Right.isCheckChangeNotNull()) Left.checkChange();
                        #endregion
                    }
                    else
                    {
                        #region
                        TmphNode rightLeft = Right.Left;
                        int rightLeftLeftCount = rightLeft.Left != null ? rightLeft.Left.Count : 0;
                        Right.Left = rightLeft.Right;
                        Right.Count -= rightLeftLeftCount + 1;
                        Left.Right = leftRight.Left;
                        leftRight.Count += rightLeftLeftCount - leftRightLeftCount;
                        rightLeft.Right = Right;
                        rightLeft.Count = Right.Count + leftRight.Count + 1;
                        leftRight.Left = leftRight.Right;
                        leftLeft.Count += leftRightLeftCount + 1;
                        Left.Left = leftLeft.Right;
                        Left.Count = leftRightLeftCount + leftLeftRightCount + 1;
                        leftRight.Right = rightLeft.Left;
                        leftLeft.Right = Left;
                        rightLeft.Left = leftRight;
                        Right = rightLeft;
                        Left = leftLeft;
                        if (!Right.Right.isCheckChange() && !Right.isCheckChangeNotNull()) Left.checkChange();
                        #endregion
                    }
                }
            }

            private void rightRightToLeft()
            {
                TmphNode rightLeft = Right.Left, rightRight = Right.Right;
                changeKey(rightLeft);
                if (Left.Right != null && Left.Left != null)
                {
                    TmphNode leftRight = Left.Right, leftLeft = Left.Left, rightRightLeft = rightRight.Left;
                    int rightLeftRightCount = rightLeft.Right != null ? rightLeft.Right.Count : 0;
                    if (rightRight.Right.Count > rightRightLeft.Count)
                    {
                        if (leftRight.Count < leftLeft.Count)
                        {
                            #region
                            Right.Left = rightLeft.Right;
                            int rightLeftLeftCount = rightLeft.Count - rightLeftRightCount - 1;
                            rightRight.Count += rightLeftRightCount + 1;
                            Left.Right = rightLeft;
                            Left.Count += rightLeftLeftCount + 1;
                            Right.Right = rightRightLeft;
                            rightLeft.Count += leftRight.Count - rightLeftRightCount;
                            Right.Count = rightLeftRightCount + rightRightLeft.Count + 1;
                            rightLeft.Right = rightLeft.Left;
                            rightRight.Left = Right;
                            rightLeft.Left = leftRight;
                            Right = rightRight;
                            if (!rightRight.isCheckChangeNotNull() && !Left.isCheckChangeNotNull()) rightLeft.CheckChange();
                            #endregion
                        }
                        else
                        {
                            #region
                            int leftRightRightCount = leftRight.Right != null ? leftRight.Right.Count : 0;
                            Left.Right = leftRight.Left;
                            Right.Left = rightLeft.Right;
                            rightRight.Count += rightLeftRightCount + 1;
                            Right.Right = rightRightLeft;
                            rightLeft.Right = rightLeft.Left;
                            Left.Count -= leftRightRightCount + 1;
                            leftRight.Left = Left;
                            rightLeft.Count += leftRightRightCount - rightLeftRightCount;
                            Right.Count = rightLeftRightCount + rightRightLeft.Count + 1;
                            rightLeft.Left = leftRight.Right;
                            leftRight.Count = rightLeft.Count + Left.Count + 1;
                            rightRight.Left = Right;
                            leftRight.Right = rightLeft;
                            Left = leftRight;
                            Right = rightRight;
                            if (!leftRight.isCheckChangeNotNull() && !Left.Left.isCheckChange() && !rightRight.isCheckChangeNotNull())
                            {
                                rightLeft.CheckChange();
                            }
                            #endregion
                        }
                    }
                    else
                    {
                        int rightRightLeftLeftCount = rightRightLeft.Left != null ? rightRightLeft.Left.Count : 0;
                        int rightRightLeftCount = rightRightLeft.Count;
                        if (leftRight.Count < leftLeft.Count)
                        {
                            #region
                            Right.Count = rightRightLeftLeftCount + rightLeftRightCount + 1;
                            Right.Right = rightRightLeft.Left;
                            rightLeft.Count += leftRight.Count - rightLeftRightCount;
                            Left.Right = rightLeft;
                            rightRight.Count -= rightRightLeftLeftCount + 1;
                            rightRight.Left = rightRightLeft.Right;
                            Left.Count = rightLeft.Count + leftLeft.Count + 1;
                            Right.Left = rightLeft.Right;
                            rightRightLeft.Count = Right.Count + rightRight.Count + 1;
                            rightRightLeft.Left = Right;
                            rightLeft.Right = rightLeft.Left;
                            rightRightLeft.Right = rightRight;
                            rightLeft.Left = leftRight;
                            Right = rightRightLeft;
                            if (!rightLeft.isCheckChange())
                            {
                                if (rightRight.Right.Count - (rightRightLeftCount - rightRightLeftLeftCount - 1) > rightLeftRightCount - rightRightLeftLeftCount)
                                {
                                    if (!rightRight.isCheckChange()) Right.Left.CheckChange();
                                }
                                else if (!Right.Left.isCheckChange()) rightRight.CheckChange();
                            }
                            #endregion
                        }
                        else
                        {
                            #region
                            int leftRightRightCount = leftRight.Right != null ? leftRight.Right.Count : 0;
                            Left.Right = leftRight.Left;
                            Right.Count = rightLeftRightCount + rightRightLeftLeftCount + 1;
                            rightRight.Left = rightRightLeft.Right;
                            Left.Count -= leftRightRightCount + 1;
                            leftRight.Left = Left;
                            rightRight.Count -= rightRightLeftLeftCount + 1;
                            Right.Right = rightRightLeft.Left;
                            rightLeft.Count += leftRightRightCount - rightLeftRightCount;
                            Right.Left = rightLeft.Right;
                            rightRightLeft.Right = rightRight;
                            rightRightLeft.Count = rightRight.Count + Right.Count + 1;
                            rightLeft.Right = rightLeft.Left;
                            rightRightLeft.Left = Right;
                            leftRight.Count = rightLeft.Count + Left.Count + 1;
                            rightLeft.Left = leftRight.Right;
                            Left = leftRight;
                            leftRight.Right = rightLeft;
                            Right = rightRightLeft;
                            if (!leftRight.isCheckChangeNotNull() && !Left.Left.isCheckChange())
                            {
                                if (rightRight.Right.Count - (rightRightLeftCount - rightRightLeftLeftCount - 1) > rightLeftRightCount - rightRightLeftLeftCount)
                                {
                                    if (!rightRight.isCheckChange()) Right.Left.CheckChange();
                                }
                                else if (!Right.Left.isCheckChange()) rightRight.CheckChange();
                            }
                            #endregion
                        }
                    }
                }
                else
                {
                    int rightLeftRightCount = rightLeft.Right != null ? rightLeft.Right.Count : 0, rightRightLeftCount = rightRight.Left.Count;
                    if (Left.Right == null)
                    {
                        #region
                        Left.Right = rightLeft;
                        rightLeft.Count -= rightLeftRightCount;
                        Right.Left = rightLeft.Right;
                        Left.Count += rightLeft.Count;
                        rightLeft.Right = rightLeft.Left;
                        rightLeft.Left = null;
                        Right.Count = rightLeftRightCount + rightRightLeftCount + 1;
                        rightRight.Count += rightLeftRightCount + 1;
                        Right.Right = rightRight.Left;
                        rightRight.Left = Right;
                        Right = rightRight;
                        if (!rightLeft.isCheckChange() && !Left.isCheckChangeNotNull()) Right.checkChange();
                        #endregion
                    }
                    else
                    {
                        #region
                        TmphNode leftRight = Left.Right;
                        int leftRightRightCount = leftRight.Right != null ? leftRight.Right.Count : 0;
                        Left.Right = leftRight.Left;
                        Left.Count -= leftRightRightCount + 1;
                        Right.Left = rightLeft.Right;
                        rightLeft.Count += leftRightRightCount - rightLeftRightCount;
                        leftRight.Left = Left;
                        leftRight.Count = Left.Count + rightLeft.Count + 1;
                        rightLeft.Right = rightLeft.Left;
                        rightRight.Count += rightLeftRightCount + 1;
                        Right.Right = rightRight.Left;
                        Right.Count = rightLeftRightCount + rightRightLeftCount + 1;
                        rightLeft.Left = leftRight.Right;
                        rightRight.Left = Right;
                        leftRight.Right = rightLeft;
                        Left = leftRight;
                        Right = rightRight;
                        if (!Left.Left.isCheckChange() && !Left.isCheckChangeNotNull()) Right.checkChange();
                        #endregion
                    }
                }
            }

            private void leftRightToRight()
            {
                TmphNode leftRight = Left.Right, leftLeft = Left.Left, leftRightRight = leftRight.Right;
                int leftRightLeftCount = leftRight.Left.Count, leftRightRightCount = leftRightRight.Count;
                if (Right.Left != null && Right.Right != null)
                {
                    TmphNode rightLeft = Right.Left, rightRight = Right.Right;
                    if (leftRightLeftCount > leftRightRightCount)
                    {
                        changeKey(leftRight);
                        ++leftRightRightCount;
                        if (rightLeft.Count < rightRight.Count)
                        {
                            #region
                            Left.Right = leftRight.Left;
                            leftRight.Count += rightLeft.Count - leftRightLeftCount;
                            leftRight.Left = leftRightRight;
                            Left.Count -= leftRightRightCount;
                            Right.Left = leftRight;
                            Right.Count += leftRightRightCount;
                            leftRight.Right = rightLeft;
                            Left.checkChange();
                            #endregion
                        }
                        else
                        {
                            #region
                            int rightLeftLeftCount = rightLeft.Left != null ? rightLeft.Left.Count : 0;
                            Left.Right = leftRight.Left;
                            rightLeft.Count = Right.Count + leftRightRightCount;
                            Right.Left = rightLeft.Right;
                            Left.Count -= leftRightRightCount;
                            leftRight.Left = leftRightRight;
                            leftRight.Count += rightLeftLeftCount - leftRightLeftCount;
                            rightLeft.Right = Right;
                            Right.Count -= rightLeftLeftCount + 1;
                            leftRight.Right = rightLeft.Left;
                            Right = rightLeft;
                            rightLeft.Left = leftRight;
                            if (!Left.isCheckChangeNotNull()) Right.Right.CheckChange();
                            #endregion
                        }
                    }
                    else
                    {
                        if (leftRightRight.Left == null || leftRightRight.Right == null) leftRightRight.CheckChange();
                        else
                        {
                            int leftRightRightRightCount = leftRightRight.Right.Count + 1;
                            changeKey(leftRightRight);
                            if (rightLeft.Count < rightRight.Count)
                            {
                                #region
                                leftRightRight.Count -= leftRightRight.Left.Count;
                                leftRight.Right = leftRightRight.Left;
                                leftRight.Count -= leftRightRightRightCount;
                                Right.Count += leftRightRightRightCount;
                                leftRightRight.Left = leftRightRight.Right;
                                if (rightLeft.Left != null) leftRightRight.Count += rightLeft.Left.Count;
                                leftRightRight.Right = rightLeft.Left;
                                Left.Count -= leftRightRightRightCount;
                                rightLeft.Count += leftRightRightRightCount;
                                rightLeft.Left = leftRightRight;
                                if (!leftRight.isCheckChange() && !rightLeft.isCheckChange()) Right.checkChange();
                                #endregion
                            }
                            else
                            {
                                #region
                                int rightLeftLeftCount = rightLeft.Left != null ? rightLeft.Left.Count : 0;
                                leftRight.Right = leftRightRight.Left;
                                leftRight.Count -= leftRightRightRightCount;
                                leftRightRight.Count += rightLeftLeftCount;
                                Right.Left = rightLeft.Right;
                                if (leftRightRight.Left != null) leftRightRight.Count -= leftRightRight.Left.Count;
                                rightLeft.Count = Right.Count + leftRightRightRightCount;
                                leftRightRight.Left = leftRightRight.Right;
                                Left.Count -= leftRightRightRightCount;
                                rightLeft.Right = Right;
                                Right.Count -= rightLeftLeftCount + 1;
                                leftRightRight.Right = rightLeft.Left;
                                Right = rightLeft;
                                rightLeft.Left = leftRightRight;
                                if (!leftRight.isCheckChange() && !Right.isCheckChangeNotNull() && !Right.Right.isCheckChange()) leftRightRight.CheckChange();
                                #endregion
                            }
                        }
                    }
                }
                else
                {
                    if (leftRightLeftCount > leftRightRightCount)
                    {
                        changeKey(leftRight);
                        if (Right.Left == null)
                        {
                            #region
                            Left.Right = leftRight.Left;
                            leftRight.Count -= leftRightLeftCount;
                            leftRight.Left = leftRightRight;
                            Right.Count += leftRight.Count;
                            Right.Left = leftRight;
                            Left.Count = leftLeft.Count + leftRightLeftCount + 1;
                            leftRight.Right = null;
                            if (!leftRight.isCheckChange() && !Right.isCheckChangeNotNull()) Left.checkChange();
                            #endregion
                        }
                        else
                        {
                            #region
                            TmphNode rightLeft = Right.Left;
                            int rightLeftLeftCount = rightLeft.Left != null ? rightLeft.Left.Count : 0;
                            Left.Right = leftRight.Left;
                            leftRight.Count += rightLeftLeftCount - leftRightLeftCount;
                            Right.Left = rightLeft.Right;
                            Right.Count -= rightLeftLeftCount + 1;
                            leftRight.Left = leftRightRight;
                            Left.Count = leftLeft.Count + leftRightLeftCount + 1;
                            rightLeft.Right = Right;
                            rightLeft.Count = leftRight.Count + Right.Count + 1;
                            leftRight.Right = rightLeft.Left;
                            Right = rightLeft;
                            rightLeft.Left = leftRight;
                            if (!Right.Right.isCheckChange() && !Right.isCheckChangeNotNull()) Left.checkChange();
                            #endregion
                        }
                    }
                    else
                    {
                        if (leftRightRight.Left == null || leftRightRight.Right == null) leftRightRight.CheckChange();
                        else
                        {
                            changeKey(leftRightRight);
                            if (Right.Left == null)
                            {
                                #region
                                TmphNode rightRight = Right.Right;
                                leftRightRight.Count = leftRightRight.Right.Count + 1;
                                Right.Left = leftRightRight;
                                Right.Count = leftRightRight.Count + 1;
                                leftRight.Count -= leftRightRight.Count;
                                leftRight.Right = leftRightRight.Left;
                                rightRight.Count += Right.Count;
                                Left.Count -= leftRightRight.Count;
                                Right.Right = rightRight.Left;
                                if (rightRight.Left != null) Right.Count += rightRight.Left.Count;
                                leftRightRight.Left = leftRightRight.Right;
                                rightRight.Left = Right;
                                leftRightRight.Right = null;
                                Right = rightRight;
                                if (!leftRightRight.isCheckChange() && !Right.Left.isCheckChange()) Right.CheckChange();
                                #endregion
                            }
                            else
                            {
                                #region
                                TmphNode rightLeft = Right.Left;
                                int rightLeftLeftCount = rightLeft.Left != null ? rightLeft.Left.Count : 0;
                                leftRightRight.Count = leftRightRight.Right.Count + 1;
                                leftRight.Right = leftRightRight.Left;
                                leftRight.Count -= leftRightRight.Count;
                                Right.Left = rightLeft.Right;
                                Left.Count -= leftRightRight.Count;
                                leftRightRight.Left = leftRightRight.Right;
                                rightLeft.Count += leftRightRight.Count + 1;
                                rightLeft.Right = Right;
                                Right.Count -= rightLeftLeftCount + 1;
                                leftRightRight.Right = rightLeft.Left;
                                leftRightRight.Count += rightLeftLeftCount;
                                Right = rightLeft;
                                rightLeft.Left = leftRightRight;
                                if (!Right.Right.isCheckChange() && !leftRightRight.isCheckChange()) Right.checkChange();
                                #endregion
                            }
                        }
                    }
                }
            }

            private void rightLeftToLeft()
            {
                TmphNode rightLeft = Right.Left, rightRight = Right.Right, rightLeftLeft = rightLeft.Left;
                int rightLeftRightCount = rightLeft.Right.Count, rightLeftLeftCount = rightLeftLeft.Count;
                if (Left.Right != null && Left.Left != null)
                {
                    TmphNode leftRight = Left.Right, leftLeft = Left.Left;
                    if (rightLeftRightCount > rightLeftLeftCount)
                    {
                        changeKey(rightLeft);
                        ++rightLeftLeftCount;
                        if (leftRight.Count < leftLeft.Count)
                        {
                            #region
                            Right.Left = rightLeft.Right;
                            rightLeft.Count += leftRight.Count - rightLeftRightCount;
                            rightLeft.Right = rightLeftLeft;
                            Right.Count -= rightLeftLeftCount;
                            Left.Right = rightLeft;
                            Left.Count += rightLeftLeftCount;
                            rightLeft.Left = leftRight;
                            Right.checkChange();
                            #endregion
                        }
                        else
                        {
                            #region
                            int leftRightRightCount = leftRight.Right != null ? leftRight.Right.Count : 0;
                            Right.Left = rightLeft.Right;
                            leftRight.Count = Left.Count + rightLeftLeftCount;
                            Left.Right = leftRight.Left;
                            Right.Count -= rightLeftLeftCount;
                            rightLeft.Right = rightLeftLeft;
                            rightLeft.Count += leftRightRightCount - rightLeftRightCount;
                            leftRight.Left = Left;
                            Left.Count -= leftRightRightCount + 1;
                            rightLeft.Left = leftRight.Right;
                            Left = leftRight;
                            leftRight.Right = rightLeft;
                            if (!Right.isCheckChangeNotNull()) Left.Left.CheckChange();
                            #endregion
                        }
                    }
                    else
                    {
                        if (rightLeftLeft.Right == null || rightLeftLeft.Left == null) rightLeftLeft.CheckChange();
                        else
                        {
                            int rightLeftLeftLeftCount = rightLeftLeft.Left.Count + 1;
                            changeKey(rightLeftLeft);
                            if (leftRight.Count < leftLeft.Count)
                            {
                                #region
                                rightLeftLeft.Count -= rightLeftLeft.Right.Count;
                                rightLeft.Left = rightLeftLeft.Right;
                                rightLeft.Count -= rightLeftLeftLeftCount;
                                Left.Count += rightLeftLeftLeftCount;
                                rightLeftLeft.Right = rightLeftLeft.Left;
                                if (leftRight.Right != null) rightLeftLeft.Count += leftRight.Right.Count;
                                rightLeftLeft.Left = leftRight.Right;
                                Right.Count -= rightLeftLeftLeftCount;
                                leftRight.Count += rightLeftLeftLeftCount;
                                leftRight.Right = rightLeftLeft;
                                if (!rightLeft.isCheckChange() && !leftRight.isCheckChange()) Left.checkChange();
                                #endregion
                            }
                            else
                            {
                                #region
                                int leftRightRightCount = leftRight.Right != null ? leftRight.Right.Count : 0;
                                rightLeft.Left = rightLeftLeft.Right;
                                rightLeft.Count -= rightLeftLeftLeftCount;
                                rightLeftLeft.Count += leftRightRightCount;
                                Left.Right = leftRight.Left;
                                if (rightLeftLeft.Right != null) rightLeftLeft.Count -= rightLeftLeft.Right.Count;
                                leftRight.Count = Left.Count + rightLeftLeftLeftCount;
                                rightLeftLeft.Right = rightLeftLeft.Left;
                                Right.Count -= rightLeftLeftLeftCount;
                                leftRight.Left = Left;
                                Left.Count -= leftRightRightCount + 1;
                                rightLeftLeft.Left = leftRight.Right;
                                Left = leftRight;
                                leftRight.Right = rightLeftLeft;
                                if (!rightLeft.isCheckChange() && !Left.isCheckChangeNotNull() && !Left.Left.isCheckChange()) rightLeftLeft.CheckChange();
                                #endregion
                            }
                        }
                    }
                }
                else
                {
                    if (rightLeftRightCount > rightLeftLeftCount)
                    {
                        changeKey(rightLeft);
                        if (Left.Right == null)
                        {
                            #region
                            Right.Left = rightLeft.Right;
                            rightLeft.Count -= rightLeftRightCount;
                            rightLeft.Right = rightLeftLeft;
                            Left.Count += rightLeft.Count;
                            Left.Right = rightLeft;
                            Right.Count = rightRight.Count + rightLeftRightCount + 1;
                            rightLeft.Left = null;
                            if (!rightLeft.isCheckChange() && !Left.isCheckChangeNotNull()) Right.checkChange();
                            #endregion
                        }
                        else
                        {
                            #region
                            TmphNode leftRight = Left.Right;
                            int leftRightRightCount = leftRight.Right != null ? leftRight.Right.Count : 0;
                            Right.Left = rightLeft.Right;
                            rightLeft.Count += leftRightRightCount - rightLeftRightCount;
                            Left.Right = leftRight.Left;
                            Left.Count -= leftRightRightCount + 1;
                            rightLeft.Right = rightLeftLeft;
                            Right.Count = rightRight.Count + rightLeftRightCount + 1;
                            leftRight.Left = Left;
                            leftRight.Count = rightLeft.Count + Left.Count + 1;
                            rightLeft.Left = leftRight.Right;
                            Left = leftRight;
                            leftRight.Right = rightLeft;
                            if (!Left.Left.isCheckChange() && !Left.isCheckChangeNotNull()) Right.checkChange();
                            #endregion
                        }
                    }
                    else
                    {
                        if (rightLeftLeft.Right == null || rightLeftLeft.Left == null) rightLeftLeft.CheckChange();
                        else
                        {
                            changeKey(rightLeftLeft);
                            if (Left.Right == null)
                            {
                                #region
                                TmphNode leftLeft = Left.Left;
                                rightLeftLeft.Count = rightLeftLeft.Left.Count + 1;
                                Left.Right = rightLeftLeft;
                                Left.Count = rightLeftLeft.Count + 1;
                                rightLeft.Count -= rightLeftLeft.Count;
                                rightLeft.Left = rightLeftLeft.Right;
                                leftLeft.Count += Left.Count;
                                Right.Count -= rightLeftLeft.Count;
                                Left.Left = leftLeft.Right;
                                if (leftLeft.Right != null) Left.Count += leftLeft.Right.Count;
                                rightLeftLeft.Right = rightLeftLeft.Left;
                                leftLeft.Right = Left;
                                rightLeftLeft.Left = null;
                                Left = leftLeft;
                                if (!rightLeftLeft.isCheckChange() && !Left.Right.isCheckChange()) Left.CheckChange();
                                #endregion
                            }
                            else
                            {
                                #region
                                TmphNode leftRight = Left.Right;
                                int leftRightRightCount = leftRight.Right != null ? leftRight.Right.Count : 0;
                                rightLeftLeft.Count = rightLeftLeft.Left.Count + 1;
                                rightLeft.Left = rightLeftLeft.Right;
                                rightLeft.Count -= rightLeftLeft.Count;
                                Left.Right = leftRight.Left;
                                Right.Count -= rightLeftLeft.Count;
                                rightLeftLeft.Right = rightLeftLeft.Left;
                                leftRight.Count += rightLeftLeft.Count + 1;
                                leftRight.Left = Left;
                                Left.Count -= leftRightRightCount + 1;
                                rightLeftLeft.Left = leftRight.Right;
                                rightLeftLeft.Count += leftRightRightCount;
                                Left = leftRight;
                                leftRight.Right = rightLeftLeft;
                                if (!Left.Left.isCheckChange() && !rightLeftLeft.isCheckChange()) Left.checkChange();
                                #endregion
                            }
                        }
                    }
                }
            }
        }

        public unsafe struct TmphFinder
        {
            private TmphSearchTree<TKeyType, TValueType> tree;
            private TmphNode father;
            private TmphNode node;

            public TmphNode Node
            {
                get { return node; }
            }

            private ulong pathMap;
            private int depth;

            public TmphFinder(TmphSearchTree<TKeyType, TValueType> tree)
            {
                this.tree = tree;
                depth = 0;
                pathMap = 0;
                father = node = null;
            }

            public bool Find(TKeyType key)
            {
                TmphNode node = tree.boot;
                int depth = 0;
                do
                {
                    int cmp = node.Key.CompareTo(key);
                    if (cmp == 0)
                    {
                        if (depth >= 64) throw new StackOverflowException();
                        this.node = node;
                        this.depth = depth;
                        return true;
                    }
                    father = node;
                    if (cmp > 0)
                    {
                        pathMap |= 1UL << depth;
                        node = node.Left;
                    }
                    else node = node.Right;
                    ++depth;
                }
                while (node != null);
                if (depth >= 64) throw new StackOverflowException();
                this.depth = depth;
                return false;
            }

            public int IndexOf(TKeyType key)
            {
                TmphNode node = tree.boot;
                int index = 0;
                do
                {
                    int cmp = node.Key.CompareTo(key);
                    if (cmp == 0) return node.Left == null ? index : (index + node.Left.Count);
                    if (cmp > 0) node = node.Left;
                    else
                    {
                        if (node.Left != null) index += node.Left.Count;
                        node = node.Right;
                        ++index;
                    }
                }
                while (node != null);
                return -1;
            }

            public int CountLess(TKeyType key)
            {
                TmphNode node = tree.boot;
                int count = 0;
                do
                {
                    int cmp = node.Key.CompareTo(key);
                    if (cmp >= 0) node = node.Left;
                    else
                    {
                        if (node.Left != null) count += node.Left.Count;
                        node = node.Right;
                        ++count;
                    }
                }
                while (node != null);
                return count;
            }

            public int CountThan(TKeyType key)
            {
                TmphNode node = tree.boot;
                int count = 0;
                do
                {
                    int cmp = node.Key.CompareTo(key);
                    if (cmp <= 0) node = node.Right;
                    else
                    {
                        if (node.Right != null) count += node.Right.Count;
                        node = node.Left;
                        ++count;
                    }
                }
                while (node != null);
                return count;
            }

            public void Remove()
            {
                this.node.Remove();
                int depth = this.depth;
                ulong isLastLeft = pathMap & (1UL << (depth - 1));
                TmphNode father = null, node = tree.boot;
                while (depth != 0)
                {
                    father = node;
                    node = (pathMap & 1) == 0 ? node.Right : node.Left;
                    --father.Count;
                    pathMap >>= 1;
                    --depth;
                }
                if (this.node.Count == 0)
                {
                    if (father == null) tree.boot = null;
                    else if (isLastLeft == 0) father.Right = null;
                    else father.Left = null;
                }
            }

            public void Add(TKeyType key, TValueType value)
            {
                if ((pathMap & (1UL << (depth - 1))) == 0) this.father.Right = new TmphNode(key, value);
                else this.father.Left = new TmphNode(key, value);

                TmphNode father = null, changeNode = null;
                int brotherCount = int.MaxValue;
                node = tree.boot;
                while (depth != 0)
                {
                    if (changeNode == null)
                    {
                        if ((pathMap & 1) == 0)
                        {
                            int leftCount = 0;
                            if (node.Right.Count > brotherCount
                                || (node.Left != null && (leftCount = node.Left.Count) > brotherCount))
                                changeNode = father;
                            father = node;
                            brotherCount = leftCount;
                            node = node.Right;
                        }
                        else
                        {
                            int rightCount = 0;
                            if (node.Left.Count > brotherCount
                                || (node.Right != null && (rightCount = node.Right.Count) > brotherCount))
                                changeNode = father;
                            father = node;
                            brotherCount = rightCount;
                            node = node.Left;
                        }
                    }
                    else
                    {
                        father = node;
                        node = (pathMap & 1) == 0 ? node.Right : node.Left;
                    }
                    ++father.Count;
                    pathMap >>= 1;
                    --depth;
                }
                if (changeNode != null) changeNode.CheckChange();
            }
        }

        private struct TmphIndexFinder
        {
            private int skipCount;
            public TValueType Value;

            public TmphIndexFinder(int skipCount)
            {
                Value = default(TValueType);
                this.skipCount = skipCount;
            }

            public void Find(TmphNode node)
            {
                TmphNode left = node.Left;
                while (left != null && left.Count > skipCount)
                {
                    if (left.Count > skipCount)
                    {
                        node = left;
                        left = node.Left;
                    }
                    else
                    {
                        skipCount -= left.Count;
                        break;
                    }
                }
                if (skipCount == 0)
                {
                    Value = node.Value;
                    return;
                }
                --skipCount;
                if (node.Right != null) Find(node.Right);
            }
        }

        private struct TmphLoader
        {
            private TmphKeyValue<TKeyType, TValueType>[] values;
            private int index;

            public TmphLoader(TmphKeyValue<TKeyType, TValueType>[] values)
            {
                this.values = values;
                index = 0;
            }

            public TmphNode Load(int count)
            {
                TmphNode node = new TmphNode(count);
                int leftCount = count >> 1;
                if (leftCount != 0) node.Left = Load(leftCount);
                TmphKeyValue<TKeyType, TValueType> value = values[index++];
                count -= leftCount;
                node.Key = value.Key;
                node.Value = value.Value;
                if (--count != 0) node.Right = Load(count);
                return node;
            }
        }

        private struct TmphCopyer
        {
            public TValueType[] Array;
            private int index;
            private int skipCount;
            public int count;

            public TmphCopyer(TValueType[] array, int skipCount, int count)
            {
                Array = array;
                this.skipCount = skipCount;
                this.count = count;
                index = 0;
            }

            public void Copy(TmphNode node)
            {
                TmphNode left = node.Left;
                for (int count = skipCount + this.count; left != null && left.Count >= count; left = node.Left) node = left;
                if (left != null)
                {
                    if (skipCount == 0) copy(left);
                    else if (skipCount >= left.Count) skipCount -= left.Count;
                    else skip(left);
                }
                if (skipCount == 0) Array[index++] = node.Value;
                else --skipCount;
                if (node.Right != null)
                {
                    int count = this.count - index;
                    if (count != 0)
                    {
                        if (skipCount == 0)
                        {
                            if (count < node.Right.Count) take(node.Right);
                            else copy(node.Right);
                        }
                        else Copy(node.Right);
                    }
                }
            }

            private void copy(TmphNode node)
            {
                if (node.Left != null) copy(node.Left);
                Array[index++] = node.Value;
                if (node.Right != null) copy(node.Right);
            }

            private void skip(TmphNode node)
            {
                if (node.Left != null)
                {
                    if (skipCount >= node.Left.Count) skipCount -= node.Left.Count;
                    else skip(node.Left);
                }
                if (skipCount == 0) Array[index++] = node.Value;
                else --skipCount;
                if (node.Right != null)
                {
                    if (skipCount == 0) copy(node.Right);
                    else skip(node.Right);
                }
            }

            private void take(TmphNode node)
            {
                TmphNode left = node.Left;
                for (int count = this.count - index; left != null && left.Count >= count; left = node.Left) node = left;
                if (left != null) copy(left);
                Array[index++] = node.Value;
                if (node.Right != null)
                {
                    int count = this.count - index;
                    if (count != 0)
                    {
                        if (count < node.Right.Count) take(node.Right);
                        else copy(node.Right);
                    }
                }
            }
        }

        private struct TmphCopyer<TArrayType>
        {
            public TArrayType[] Array;
            private Func<TValueType, TArrayType> getValue;
            private int index;
            private int skipCount;
            public int count;

            public TmphCopyer(TArrayType[] array, Func<TValueType, TArrayType> getValue, int skipCount, int count)
            {
                Array = array;
                this.getValue = getValue;
                this.skipCount = skipCount;
                this.count = count;
                index = 0;
            }

            /// <summary>
            /// 复制数据
            /// </summary>
            /// <param name="node">二叉树节点</param>
            public void Copy(TmphNode node)
            {
                TmphNode left = node.Left;
                for (int count = skipCount + this.count; left != null && left.Count >= count; left = node.Left) node = left;
                if (left != null)
                {
                    if (skipCount == 0) copy(left);
                    else if (skipCount >= left.Count) skipCount -= left.Count;
                    else skip(left);
                }
                if (skipCount == 0) Array[index++] = getValue(node.Value);
                else --skipCount;
                if (node.Right != null)
                {
                    int count = this.count - index;
                    if (count != 0)
                    {
                        if (skipCount == 0)
                        {
                            if (count < node.Right.Count) take(node.Right);
                            else copy(node.Right);
                        }
                        else Copy(node.Right);
                    }
                }
            }

            /// <summary>
            /// 复制节点数据
            /// </summary>
            /// <param name="node">二叉树节点</param>
            private void copy(TmphNode node)
            {
                if (node.Left != null) copy(node.Left);
                Array[index++] = getValue(node.Value);
                if (node.Right != null) copy(node.Right);
            }

            /// <summary>
            /// 跳过记录复制数据
            /// </summary>
            /// <param name="node">二叉树节点</param>
            private void skip(TmphNode node)
            {
                if (node.Left != null)
                {
                    if (skipCount >= node.Left.Count) skipCount -= node.Left.Count;
                    else skip(node.Left);
                }
                if (skipCount == 0) Array[index++] = getValue(node.Value);
                else --skipCount;
                if (node.Right != null)
                {
                    if (skipCount == 0) copy(node.Right);
                    else skip(node.Right);
                }
            }

            /// <summary>
            /// 复制节点数据
            /// </summary>
            /// <param name="node">二叉树节点</param>
            private void take(TmphNode node)
            {
                TmphNode left = node.Left;
                for (int count = this.count - index; left != null && left.Count >= count; left = node.Left) node = left;
                if (left != null) copy(left);
                Array[index++] = getValue(node.Value);
                if (node.Right != null)
                {
                    int count = this.count - index;
                    if (count != 0)
                    {
                        if (count < node.Right.Count) take(node.Right);
                        else copy(node.Right);
                    }
                }
            }
        }

        private TmphNode boot;

        public int Count
        {
            get { return boot != null ? boot.Count : 0; }
        }

        public TValueType this[TKeyType key]
        {
            get
            {
                TmphNode node = get(key);
                if (node != null) return node.Value;
                throw new KeyNotFoundException(key.ToString());
            }
            set { set(key, value, false); }
        }

        public TmphSearchTree()
        {
        }

        public static TmphSearchTree<TKeyType, TValueType> Unsafe(TmphKeyValue<TKeyType, TValueType>[] values)
        {
            TmphSearchTree<TKeyType, TValueType> tree = new TmphSearchTree<TKeyType, TValueType>();
            tree.boot = new TmphLoader(values).Load(values.Length);
            return tree;
        }

        private TmphNode get(TKeyType key)
        {
            TmphNode node = boot;
            while (node != null)
            {
                int cmp = node.Key.CompareTo(key);
                if (cmp == 0) return node;
                node = cmp > 0 ? node.Left : node.Right;
            }
            return null;
        }

        private void set(TKeyType key, TValueType value, bool isCheck)
        {
            if (boot != null)
            {
                TmphFinder finder = new TmphFinder(this);
                if (finder.Find(key))
                {
                    if (isCheck) throw new ArgumentException("关键字 " + key.ToString() + " 已存在");
                    finder.Node.Value = value;
                }
                else finder.Add(key, value);
            }
            else boot = new TmphNode(key, value);
        }

        public void Add(TKeyType key, TValueType value)
        {
            set(key, value, true);
        }

        public bool Remove(TKeyType key)
        {
            if (boot != null)
            {
                TmphFinder finder = new TmphFinder(this);
                if (finder.Find(key))
                {
                    finder.Remove();
                    return true;
                }
            }
            return false;
        }

        public bool Remove(TKeyType key, out TValueType value)
        {
            if (boot != null)
            {
                TmphFinder finder = new TmphFinder(this);
                if (finder.Find(key))
                {
                    value = finder.Node.Value;
                    finder.Remove();
                    return true;
                }
            }
            value = default(TValueType);
            return false;
        }

        public bool ContainsKey(TKeyType key)
        {
            return get(key) != null;
        }

        public bool TryGetValue(TKeyType key, out TValueType value)
        {
            TmphNode node = get(key);
            if (node != null)
            {
                value = node.Value;
                return true;
            }
            value = default(TValueType);
            return false;
        }

        public TValueType[] GetArray()
        {
            if (boot != null)
            {
                TmphCopyer copyer = new TmphCopyer(new TValueType[boot.Count], 0, boot.Count);
                copyer.Copy(boot);
                return copyer.Array;
            }
            return TmphNullValue<TValueType>.Array;
        }

        public TArrayType[] GetArray<TArrayType>(Func<TValueType, TArrayType> getValue)
        {
            if (boot != null)
            {
                if (getValue == null) throw new NullReferenceException();
                TmphCopyer<TArrayType> copyer = new TmphCopyer<TArrayType>(new TArrayType[boot.Count], getValue, 0, boot.Count);
                copyer.Copy(boot);
                return copyer.Array;
            }
            return TmphNullValue<TArrayType>.Array;
        }

        public TValueType[] GetRange(int skipCount, int getCount)
        {
            if (boot != null)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(boot.Count, skipCount, getCount);
                if (range.GetCount != 0)
                {
                    TmphCopyer copyer = new TmphCopyer(new TValueType[range.GetCount], range.SkipCount, range.GetCount);
                    copyer.Copy(boot);
                    return copyer.Array;
                }
            }
            return TmphNullValue<TValueType>.Array;
        }

        public TArrayType[] GetRange<TArrayType>(int skipCount, int getCount, Func<TValueType, TArrayType> getValue)
        {
            if (boot != null)
            {
                TmphArray.TmphRange range = new TmphArray.TmphRange(boot.Count, skipCount, getCount);
                if (range.GetCount != 0)
                {
                    if (getValue == null) throw new NullReferenceException();
                    TmphCopyer<TArrayType> copyer = new TmphCopyer<TArrayType>(new TArrayType[range.GetCount], getValue, range.SkipCount, range.GetCount);
                    copyer.Copy(boot);
                    return copyer.Array;
                }
            }
            return TmphNullValue<TArrayType>.Array;
        }

        public int IndexOf(TKeyType key)
        {
            if (boot != null) return new TmphFinder(this).IndexOf(key);
            return -1;
        }

        public int CountLess(TKeyType key)
        {
            if (boot != null) return new TmphFinder(this).CountLess(key);
            return 0;
        }

        public int CountThan(TKeyType key)
        {
            if (boot != null) return new TmphFinder(this).CountThan(key);
            return 0;
        }

        public TValueType GetIndex(int index)
        {
            if (boot != null)
            {
                TmphIndexFinder indexer = new TmphIndexFinder(index);
                indexer.Find(boot);
                return indexer.Value;
            }
            return default(TValueType);
        }
    }
}