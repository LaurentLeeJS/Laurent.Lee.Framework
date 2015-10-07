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

using Laurent.Lee.CLB.Code.CSharp;
using Laurent.Lee.CLB.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Laurent.Lee.CLB.Code
{
    /// <summary>
    ///     树节点模板
    /// </summary>
    public abstract class TmphTemplate
    {
        /// <summary>
        ///     成员信息缓存集合
        /// </summary>
        private static readonly Dictionary<Type, Dictionary<TmphHashString, TmphMemberIndex>> _memberCache =
            TmphDictionary.CreateOnly<Type, Dictionary<TmphHashString, TmphMemberIndex>>();

        /// <summary>
        ///     当前代码字符串
        /// </summary>
        protected TmphStringBuilder CurrentCode = new TmphStringBuilder();

        /// <summary>
        ///     当前成员节点集合
        /// </summary>
        protected TmphList<TmphMemberNode> CurrentMembers = new TmphList<TmphMemberNode>();

        ///// <summary>
        ///// 属性成员映射集合
        ///// </summary>
        //protected Dictionary<string, keyValue<memberIndex, string>> propertyNames = dictionary.CreateOnly<string, keyValue<memberIndex, string>>();
        /// <summary>
        ///     临时逻辑变量名称
        /// </summary>
        protected string IfName = "_if_";

        /// <summary>
        ///     成员树
        /// </summary>
        protected Dictionary<TmphMemberNode, Dictionary<TmphHashString, TmphMemberNode>> MemberPaths =
            TmphDictionary.CreateOnly<TmphMemberNode, Dictionary<TmphHashString, TmphMemberNode>>();

        /// <summary>
        ///     错误处理委托
        /// </summary>
        protected Action<string> OnError;

        /// <summary>
        ///     信息处理委托
        /// </summary>
        protected Action<string> OnMessage;

        /// <summary>
        ///     子段程序代码集合
        /// </summary>
        protected Dictionary<TmphHashString, string> PartCodes = TmphDictionary.CreateHashString<string>();

        /// <summary>
        ///     当前代码字符串常量
        /// </summary>
        protected TmphStringBuilder PushCodes = new TmphStringBuilder();

        /// <summary>
        ///     模板数据视图类型
        /// </summary>
        protected Type ViewType;

        /// <summary>
        ///     当前代码字符串
        /// </summary>
        public string Code
        {
            get
            {
                PushCode(null);
                return CurrentCode.ToString();
            }
        }

        /// <summary>
        ///     集合是否支持length属性
        /// </summary>
        protected virtual bool IsCollectionLength
        {
            get { return false; }
        }

        /// <summary>
        ///     是否记录循环集合
        /// </summary>
        protected virtual bool IsLoopValue
        {
            get { return false; }
        }

        /// <summary>
        ///     获取临时变量名称
        /// </summary>
        /// <param name="index">临时变量层次</param>
        /// <returns>变量名称</returns>
        protected string Path(int index)
        {
            return "_value" + (index == 0 ? (CurrentMembers.Count - 1) : index) + "_";
        }

        /// <summary>
        ///     获取循环索引临时变量名称
        /// </summary>
        /// <param name="index">临时变量层次</param>
        /// <returns>循环索引变量名称</returns>
        protected string LoopIndex(int index)
        {
            return "_loopIndex" + (index == 0 ? (CurrentMembers.Count - 1) : index) + "_";
        }

        /// <summary>
        ///     获取循环数量临时变量名称
        /// </summary>
        /// <param name="index">临时变量层次</param>
        /// <returns>循环数量变量名称</returns>
        protected string LoopCount(int index)
        {
            return "_loopCount" + (index == 0 ? (CurrentMembers.Count - 1) : index) + "_";
        }

        /// <summary>
        ///     获取循环集合临时变量名称
        /// </summary>
        /// <param name="index">临时变量层次</param>
        /// <returns>循环集合变量名称</returns>
        protected string LoopValues(int index)
        {
            return "_loopValues" + (index == 0 ? (CurrentMembers.Count - 1) : index) + "_";
        }

        /// <summary>
        ///     获取循环内临时变量名称
        /// </summary>
        /// <param name="index">临时变量层次</param>
        /// <returns>循环内变量名称</returns>
        protected string LoopValue(int index)
        {
            return "_loopValue" + (index == 0 ? (CurrentMembers.Count - 1) : index) + "_";
        }

        /// <summary>
        ///     根据成员名称获取成员树节点
        /// </summary>
        /// <param name="memberName">成员名称</param>
        /// <param name="isDepth">是否深度搜索,false表示当前节点子节点</param>
        /// <returns>成员树节点</returns>
        protected TmphMemberNode GetMember(string memberName, out bool isDepth)
        {
            var memberIndex = 0;
            while (memberIndex != memberName.Length && memberName[memberIndex] == '.') ++memberIndex;
            memberName = memberName.Substring(memberIndex);
            memberIndex = CurrentMembers.Count - memberIndex - 1;
            if (memberIndex < 0) memberIndex = 0;
            var value = CurrentMembers[memberIndex];
            isDepth = false;
            if (memberName.Length != 0)
            {
                var names = memberName.Split('.');
                for (var lastIndex = names.Length - 1; memberIndex >= 0; --memberIndex)
                {
                    if ((value = CurrentMembers[memberIndex].Get(ref names[0], lastIndex == 0)) != null)
                    {
                        if (memberIndex == 0)
                        {
                            //keyValue<memberIndex, string> propertyIndex;
                            //if (!propertyNames.TryGetValue(names[0], out propertyIndex)) propertyIndex.Value = names[0];
                            //value.Path = propertyIndex.Value;
                            value.Path = names[0];
                        }
                        else value.Path = Path(memberIndex) + "." + names[0];
                        if (names.Length != 1) isDepth = true;
                        for (var nameIndex = 1; nameIndex != names.Length; ++nameIndex)
                        {
                            if ((value = value.Get(ref names[nameIndex], nameIndex == lastIndex)) == null) break;
                            value.Path = value.Parent.Path + "." + names[nameIndex];
                        }
                        if (value == null) break;
                        return value;
                    }
                }
                var message = ViewType.fullName() + " 未找到属性 " + CurrentMembers.lastOrDefault().FullPath + " . " +
                              memberName;
                if (CheckErrorMemberName(memberName))
                {
                    OnMessage(message);
                    return null;
                }
                OnError(message);
            }
            return value;
        }

        /// <summary>
        ///     检测错误成员名称
        /// </summary>
        /// <param name="memberName">成员名称</param>
        /// <returns>是否忽略错误</returns>
        protected virtual bool CheckErrorMemberName(string memberName)
        {
            return false;
        }

        /// <summary>
        ///     添加代码
        /// </summary>
        /// <param name="code">代码,null表示截断字符串</param>
        protected virtual void PushCode(string code)
        {
            if (code != null) PushCodes.Add(code);
            else
            {
                code = PushCodes.ToString();
                if (code.Length != 0)
                {
                    CurrentCode.Append(@"
            _code_.Add(@""", code.Replace(@"""", @""""""), @""");");
                }
                PushCodes.Empty();
            }
        }

        /// <summary>
        ///     添加当前成员节点
        /// </summary>
        /// <param name="member">成员节点</param>
        protected void PushMember(TmphMemberNode member)
        {
            CurrentMembers.Add(member);
        }

        /// <summary>
        ///     if开始代码段
        /// </summary>
        /// <param name="memberName">成员名称</param>
        /// <param name="isSkip">是否跳跃层次</param>
        protected void IfStart(string memberName, bool isSkip)
        {
            bool isDepth;
            var member = GetMember(memberName, out isDepth);
            PushMember(member);
            if (isSkip) PushMember(member);
            var name = Path(0);
            CurrentCode.Append(@"
                {
                    ", member.Type.FullName, " ", name, " = ", member.Path, ";");
            IfStart(member.Type, name, null);
        }

        /// <summary>
        ///     if开始代码段
        /// </summary>
        /// <param name="type">成员类型</param>
        /// <param name="name">成员路径名称</param>
        /// <param name="ifName">if临时变量名称</param>
        protected void IfStart(TmphMemberType type, string name, string ifName)
        {
            if (type.IsStruct || type.Type.IsEnum)
            {
                if (type.IsBool)
                {
                    CurrentCode.Append(@"
                    if (", name, ")");
                }
                else if (type.IsAjaxToString)
                {
                    CurrentCode.Append(@"
                    if (", name, " != 0)");
                }
            }
            else
            {
                CurrentCode.Append(@"
                    if (", name, " != null)");
            }
            CurrentCode.Append(@"
                    {");
            if (ifName != null)
            {
                CurrentCode.Append(@"
                        ", ifName, " = true;");
            }
        }

        /// <summary>
        ///     if结束代码段
        /// </summary>
        /// <param name="isMember">是否删除成员节点</param>
        protected void IfEnd(bool isMember)
        {
            if (isMember) CurrentMembers.Pop();
            CurrentCode.Append(@"
                    }
                }");
        }

        /// <summary>
        ///     if代码段处理
        /// </summary>
        /// <param name="member">成员节点</param>
        /// <param name="memberName">成员名称</param>
        /// <param name="isDepth">是否深度搜索</param>
        /// <param name="doMember">成员处理函数</param>
        protected void IfThen(TmphMemberNode member, string memberName, bool isDepth, Action<TmphMemberNode> doMember)
        {
            if (isDepth)
            {
                PushCode(null);
                var names = SplitMemberName(memberName);
                for (var index = 0; index != names.Length - 1; ++index) IfStart(names[index], false);
                doMember(GetMember(names[names.Length - 1], out isDepth));
                PushCode(null);
                for (var index = 0; index != names.Length - 1; ++index) IfEnd(true);
            }
            else doMember(member);
        }

        /// <summary>
        ///     输出绑定的数据
        /// </summary>
        /// <param name="member">成员节点</param>
        protected void At(TmphMemberNode member)
        {
            PushCode(null);
            if (member.Type.IsString)
            {
                CurrentCode.Append(@"
            _code_.Add(", member.Path, ");");
            }
            else if (member.Type.IsBool && member.Type.IsStruct)
            {
                CurrentCode.Append(@"
            _code_.Add(", member.Path, @" ? ""true"" : ""false"");");
            }
            else
            {
                CurrentCode.Append(@"
            _code_.Add(", member.Path, ".ToString());");
            }
        }

        /// <summary>
        ///     分解成员名称
        /// </summary>
        /// <param name="memberName">成员名称</param>
        /// <returns>成员名称集合</returns>
        protected static TmphSubString[] SplitMemberName(string memberName)
        {
            var memberIndex = 0;
            while (memberIndex != memberName.Length && memberName[memberIndex] == '.') ++memberIndex;
            var value = memberName.Substring(0, memberIndex);
            var names = TmphSubString.Unsafe(memberName, memberIndex).Split('.').ToArray();
            names[0] = value + names[0];
            return names;
        }

        ///// <summary>
        ///// 获取成员信息集合
        ///// </summary>
        ///// <param name="type">类型</param>
        ///// <returns>成员信息集合</returns>
        //protected static Dictionary<hashString, memberIndex> getMemberCache(Type type)
        //{
        //    Dictionary<hashString, memberIndex> values;
        //    if (!memberCache.TryGetValue(type, out values))
        //    {
        //        try
        //        {
        //            memberCache[type] = values = memberIndexGroup.Get((Type)type).Find(memberFilters.Instance)
        //                .getDictionary(value => (hashString)value.Member.Name);
        //        }
        //        catch (Exception error)
        //        {
        //            string output = string.Join(",", memberIndexGroup.Get((Type)type).Find(memberFilters.Instance)
        //                .groupCount(value => value.Member.Name)
        //                .getFind(value => value.Value != 1)
        //                .GetArray(value => value.Key));
        //            log.Error.ThrowReal(error, ((Type)type).FullName + " : " + output, true);
        //        }
        //    }
        //    return values;
        //}
        /// <summary>
        ///     成员树节点
        /// </summary>
        public class TmphMemberNode
        {
            /// <summary>
            ///     当前节点成员名称
            /// </summary>
            private readonly string _name;

            /// <summary>
            ///     树节点模板
            /// </summary>
            private readonly TmphTemplate _template;

            /// <summary>
            ///     父节点成员
            /// </summary>
            internal TmphMemberNode Parent;

            /// <summary>
            ///     成员树节点
            /// </summary>
            /// <param name="template">树节点模板</param>
            /// <param name="type">成员类型</param>
            /// <param name="name">当前节点成员名称</param>
            /// <param name="path">当前节点成员名称</param>
            internal TmphMemberNode(TmphTemplate template, TmphMemberType type, string name, string path)
            {
                _template = template;
                Type = type;
                _name = name;
                Path = path;
                foreach (var member in Members.Values)
                {
                    //if (member.Member.customAttribute<Laurent.Lee.CLB.Code.ignore>(true) == null)
                    if (!member.IsIgnore)
                    {
                        var outputAjax = member.GetAttribute<TmphWebView.TmphOutputAjax>(true, true);
                        if (outputAjax != null && outputAjax.IsSetup)
                        {
                            name = member.Member.Name;
                            Get(ref name, false);
                        }
                    }
                }
            }

            /// <summary>
            ///     成员类型
            /// </summary>
            public TmphMemberType Type { get; private set; }

            /// <summary>
            ///     成员名称+成员信息集合
            /// </summary>
            internal Dictionary<TmphHashString, TmphMemberIndex> Members
            {
                get
                {
                    Dictionary<TmphHashString, TmphMemberIndex> values;
                    var type = Type.Type;
                    if (!_memberCache.TryGetValue(type, out values))
                    {
                        try
                        {
                            _memberCache[type] = values = TmphMemberIndexGroup.Get(Type).Find(TmphMemberFilters.Instance)
                                .getDictionary(value => (TmphHashString)value.Member.Name);
                        }
                        catch (Exception error)
                        {
                            var output = string.Join(",", TmphMemberIndexGroup.Get(Type).Find(TmphMemberFilters.Instance)
                                .groupCount(value => value.Member.Name)
                                .GetFind(value => value.Value != 1)
                                .GetArray(value => value.Key));
                            TmphLog.Error.ThrowReal(error, Type.FullName + " : " + output);
                        }
                    }
                    return values;
                }
            }

            /// <summary>
            ///     节点路径
            /// </summary>
            public string Path { get; internal set; }

            ///// <summary>
            ///// 是否延时加载属性
            ///// </summary>
            //public bool IsLadyProperty;
            /// <summary>
            ///     节点路径全称
            /// </summary>
            internal string FullPath
            {
                get
                {
                    if (Parent != null)
                    {
                        var path = new TmphCollection<string>();
                        for (var member = this; member.Parent != null; member = member.Parent)
                            path.AddExpand("." + member._name);
                        return string.Concat(path.ToArray()).Substring(1);
                    }
                    return null;
                }
            }

            /// <summary>
            ///     节点路径上是否有下级路径
            /// </summary>
            public bool IsNextPath
            {
                get
                {
                    Dictionary<TmphHashString, TmphMemberNode> paths;
                    return _template.MemberPaths.TryGetValue(this, out paths) && paths.Count != 0;
                }
            }

            /// <summary>
            ///     根据成员名称获取子节点成员
            /// </summary>
            /// <param name="name">成员名称</param>
            /// <returns>子节点成员</returns>
            public TmphMemberNode Get(string name)
            {
                return Get(ref name, false);
            }

            /// <summary>
            ///     根据成员名称获取子节点成员
            /// </summary>
            /// <param name="name">成员名称</param>
            /// <param name="isLast">是否最后层级</param>
            /// <returns>子节点成员</returns>
            internal TmphMemberNode Get(ref string name, bool isLast)
            {
                Dictionary<TmphHashString, TmphMemberNode> paths;
                if (!_template.MemberPaths.TryGetValue(this, out paths))
                {
                    _template.MemberPaths[this] = paths = TmphDictionary.CreateHashString<TmphMemberNode>();
                }
                TmphMemberNode value;
                if (isLast && _template.IsCollectionLength && name == "length")
                {
                    if (Type.Type.IsArray) name = "Length";
                    else if (typeof(ICollection).IsAssignableFrom(Type.Type)) name = "Count";
                }
                if (paths.TryGetValue(name, out value)) return value;
                var isPath = true;
                if (name.Length != 0)
                {
                    TmphMemberIndex member;
                    if (Members.TryGetValue(name, out member))
                    {
                        //if (member.Member.customAttribute<Laurent.Lee.CLB.Code.ignore>(true) != null) isPath = false;
                        if (member.IsIgnore) isPath = false;
                        var outputAjax = member.GetAttribute<TmphWebView.TmphOutputAjax>(true, true);
                        if (outputAjax != null)
                        {
                            if (outputAjax.BindingName != null)
                            {
                                var outputName = outputAjax.BindingName;
                                value = Get(ref outputName, false);
                            }
                            if (!outputAjax.IsSetup) isPath = false;
                        }
                        value = new TmphMemberNode(_template, member.Type, name, null);
                        //keyValue<memberIndex, string> propertyIndex;
                        //if (Template.currentMembers.Unsafer.Array[0] == this
                        //    && !Template.propertyNames.TryGetValue(name, out propertyIndex))
                        //{
                        //    Template.propertyNames.Add(name, new keyValue<memberIndex, string>(member, "_p" + Template.propertyNames.Count.toString()));
                        //}
                        //else propertyIndex.Value = name;
                        //, IsLadyProperty = !member.IsField && member.Member.customAttribute<ladyProperty>(false, false) != null
                    }
                }
                else value = new TmphMemberNode(_template, Type.EnumerableArgumentType, null, null);
                if (value != null)
                {
                    value.Parent = this;
                    //value.template = template;
                    if (isPath) paths[name] = value;
                }
                return value;
            }
        }
    }

    /// <summary>
    ///     树节点模板
    /// </summary>
    /// <typeparam name="TNodeType">树节点类型</typeparam>
    public abstract class TmphTemplate<TNodeType> : TmphTemplate where TNodeType : TmphTemplate<TNodeType>.TmphINode
    {
        /// <summary>
        ///     模板代码节点接口
        /// </summary>
        public interface TmphINode
        {
            /// <summary>
            ///     模板命令
            /// </summary>
            string TemplateCommand { get; }

            /// <summary>
            ///     模板成员名称
            /// </summary>
            string TemplateMemberName { get; }

            /// <summary>
            ///     模板文本代码
            /// </summary>
            string TemplateCode { get; }

            /// <summary>
            ///     子节点数量
            /// </summary>
            int ChildCount { get; }

            /// <summary>
            ///     子节点集合
            /// </summary>
            IEnumerable<TNodeType> Childs { get; }
        }

        /// <summary>
        ///     模板command+解析器
        /// </summary>
        protected Dictionary<TmphHashString, Action<TNodeType>> Creators =
            TmphDictionary.CreateHashString<Action<TNodeType>>();

        /// <summary>
        ///     引用代码树节点
        /// </summary>
        protected Dictionary<TmphHashString, TNodeType> NameNodes = TmphDictionary.CreateHashString<TNodeType>();

        /// <summary>
        ///     树节点模板
        /// </summary>
        /// <param name="type">模板数据视图</param>
        /// <param name="onError">错误处理委托</param>
        /// <param name="onMessage">消息处理委托</param>
        protected TmphTemplate(Type type, Action<string> onError, Action<string> onMessage)
        {
            OnError = onError;
            OnMessage = onMessage;
            CurrentMembers.Add(new TmphMemberNode(this, ViewType = type ?? GetType(), null, "this"));
        }

        /// <summary>
        ///     检测成员名称
        /// </summary>
        /// <param name="memberName"></param>
        /// <returns></returns>
        protected virtual string CheckMemberName(string memberName)
        {
            return memberName;
        }

        /// <summary>
        ///     添加代码树节点
        /// </summary>
        /// <param name="node">代码树节点</param>
        protected void Skin(TNodeType node)
        {
            Action<TNodeType> creator;
            foreach (var son in node.Childs)
            {
                var command = son.TemplateCommand;
                if (command == null) PushCode(son.TemplateCode);
                else if (Creators.TryGetValue(command, out creator)) creator(son);
                else OnError(ViewType.fullName() + " 未找到命名处理函数 " + command + " : " + son.TemplateMemberName);
            }
        }

        /// <summary>
        ///     输出绑定的数据
        /// </summary>
        /// <param name="node">代码树节点</param>
        protected virtual void At(TNodeType node)
        {
            bool isDepth;
            var memberName = node.TemplateMemberName;
            var member = GetMember(memberName, out isDepth);
            if (member != null) IfThen(member, memberName, isDepth, value => At(value));
        }

        /// <summary>
        ///     注释处理
        /// </summary>
        /// <param name="node">代码树节点</param>
        protected void Note(TNodeType node)
        {
        }

        /// <summary>
        ///     循环处理
        /// </summary>
        /// <param name="member">成员节点</param>
        /// <param name="node">代码树节点</param>
        /// <param name="name">成员路径名称</param>
        /// <param name="popCount">删除成员节点数量</param>
        protected void Loop(TmphMemberNode member, TNodeType node, string name, int popCount)
        {
            CurrentCode.Append(@"
                    ", name, " = ", member.Path, ";");
            if (popCount != 0) CurrentMembers.Pop();
            while (popCount != 0)
            {
                IfEnd(true);
                --popCount;
            }
            if (member.Type.EnumerableType == null)
            {
                OnError(ViewType.fullName() + " 属性不可枚举 " + CurrentMembers.lastOrDefault().FullPath);
                return;
            }
            PushMember(member);
            var valueName = Path(CurrentMembers.Count);
            if (!member.Type.Type.IsValueType)
            {
                CurrentCode.Append(@"
                    if (", name, @" != null)");
            }
            CurrentCode.Append(@"
                    {
                        int ", LoopIndex(0), @" = _loopIndex_, ", LoopCount(0), @" = _loopCount_;");
            if (IsLoopValue)
            {
                CurrentCode.Append(@"
                        var ", LoopValues(0), @" = _loopValues_, ", LoopValue(0), @" = _loopValue_;
                        _loopValues_ = ", name, ";");
            }
            CurrentCode.Append(@"
                        _loopIndex_ = 0;
                        _loopCount_ = ", name, member.Type.Type.IsArray ? ".Length" : ".count()", @";
                        foreach (", member.Type.EnumerableArgumentType.FullName, " " + valueName + " in ", name, @")
                        {");
            if (IsLoopValue)
            {
                CurrentCode.Append(@"
                            _loopValue_ = ", valueName, ";");
            }
            var loopMember = member.Get(string.Empty);
            loopMember.Path = valueName;
            PushMember(loopMember);
            Skin(node);
            CurrentMembers.Pop();
            PushCode(null);
            CurrentCode.Append(@"
                            ++_loopIndex_;
                        }
                        _loopIndex_ = ", LoopIndex(0), @";
                        _loopCount_ = ", LoopCount(0), @";");
            if (IsLoopValue)
            {
                CurrentCode.Append(@"
                        _loopValue_ = ", LoopValue(0), @";
                        _loopValues_ = ", LoopValues(0), ";");
            }
            CurrentCode.Append(@"
                    }");
            CurrentMembers.Pop();
        }

        /// <summary>
        ///     循环处理
        /// </summary>
        /// <param name="node">代码树节点</param>
        protected void Loop(TNodeType node)
        {
            bool isDepth;
            var memberName = CheckMemberName(node.TemplateMemberName);
            var member = GetMember(memberName, out isDepth);
            if (member == null) return;
            PushCode(null);
            var name = Path(CurrentMembers.Count);
            if (isDepth)
            {
                CurrentCode.Append(@"
                {
                    ", member.Type.FullName, " ", name, " = default(", member.Type.FullName, ");");
                var names = SplitMemberName(memberName);
                IfStart(names[0], true);
                for (var index = 1; index != names.Length - 1; ++index) IfStart(names[index], false);
                Loop(GetMember(names[names.Length - 1], out isDepth), node, name, names.Length - 1);
            }
            else
            {
                CurrentCode.Append(@"
                {
                    ", member.Type.FullName, " ", name, ";");
                Loop(member, node, name, 0);
            }
            CurrentCode.Append(@"
                }");
        }

        /// <summary>
        ///     子代码段处理
        /// </summary>
        /// <param name="member">成员节点</param>
        /// <param name="node">代码树节点</param>
        /// <param name="name">成员路径名称</param>
        /// <param name="popCount">删除成员节点数量</param>
        protected void Push(TmphMemberNode member, TNodeType node, string name, int popCount)
        {
            CurrentCode.Append(@"
                    ", name, " = ", member.Path, ";");
            if (popCount != 0) CurrentMembers.Pop();
            while (popCount != 0)
            {
                IfEnd(true);
                --popCount;
            }
            PushMember(member);
            CurrentCode.Append(@"
            ", IfName, " = false;");
            IfThen(node, member.Type, name, IfName, false, 0);
            CurrentMembers.Pop();
        }

        /// <summary>
        ///     子代码段处理
        /// </summary>
        /// <param name="node">代码树节点</param>
        protected void Push(TNodeType node)
        {
            bool isDepth;
            var memberName = CheckMemberName(node.TemplateMemberName);
            var member = GetMember(memberName, out isDepth);
            if (member != null && node.ChildCount != 0)
            {
                PushCode(null);
                var name = Path(CurrentMembers.Count);
                CurrentCode.Append(@"
                {
                    ", member.Type.FullName, " ", name, " = default(", member.Type.FullName, ");");
                if (isDepth)
                {
                    var names = SplitMemberName(memberName);
                    IfStart(names[0], true);
                    for (var index = 1; index != names.Length - 1; ++index) IfStart(names[index], false);
                    Push(GetMember(names[names.Length - 1], out isDepth), node, name, names.Length - 1);
                }
                else Push(member, node, name, 0);
                CurrentCode.Append(@"
                }");
            }
        }

        /// <summary>
        ///     if代码段处理
        /// </summary>
        /// <param name="node">代码树节点</param>
        /// <param name="type">成员类型</param>
        /// <param name="name">成员路径名称</param>
        /// <param name="ifName">逻辑变量名称</param>
        /// <param name="isMember">是否删除当前成员节点</param>
        /// <param name="popCount">删除成员节点数量</param>
        protected void IfThen(TNodeType node, TmphMemberType type, string name, string ifName, bool isMember, int popCount)
        {
            IfStart(type, name, ifName);
            while (popCount != 0)
            {
                IfEnd(true);
                --popCount;
            }
            if (isMember) CurrentMembers.Pop();
            CurrentCode.Append(@"
                }
            if (", ifName, @")
            {");
            Skin(node);
            PushCode(null);
            CurrentCode.Append(@"
            }");
        }

        /// <summary>
        ///     if代码段处理
        /// </summary>
        /// <param name="node">代码树节点</param>
        /// <param name="type">成员类型</param>
        /// <param name="name">成员路径名称</param>
        /// <param name="value">匹配值</param>
        /// <param name="ifName">逻辑变量名称</param>
        /// <param name="popCount">删除成员节点数量</param>
        protected void IfThen(TNodeType node, TmphMemberType type, string name, string value, string ifName, int popCount)
        {
            if (type.IsStruct || type.Type.IsEnum)
            {
                CurrentCode.Append(@"
                if (", name, @".ToString() == @""", value.Replace(@"""", @""""""), @""")");
            }
            else
            {
                CurrentCode.Append(@"
                if (", name, @" != null && ", name, @".ToString() == @""", value.Replace(@"""", @""""""), @""")");
            }
            CurrentCode.Append(@"
                {
                    ", ifName, @" = true;
                }");
            while (popCount != 0)
            {
                IfEnd(true);
                --popCount;
            }
            CurrentCode.Append(@"
            if (", ifName, @")
            {");
            Skin(node);
            PushCode(null);
            CurrentCode.Append(@"
            }");
        }

        /// <summary>
        ///     if代码段处理
        /// </summary>
        /// <param name="member">成员节点</param>
        /// <param name="node">代码树节点</param>
        /// <param name="value">匹配值</param>
        /// <param name="ifName">逻辑变量名称</param>
        /// <param name="popCount">删除成员节点数量</param>
        protected void IfThen(TmphMemberNode member, TNodeType node, string value, string ifName, int popCount)
        {
            if (value == null) IfThen(node, member.Type, member.Path, ifName, false, popCount);
            else IfThen(node, member.Type, member.Path, value, ifName, popCount);
        }

        /// <summary>
        ///     绑定的数据为true非0非null时输出代码
        /// </summary>
        /// <param name="node">代码树节点</param>
        protected void IfThen(TNodeType node)
        {
            string value = null, memberName = node.TemplateMemberName;
            var valueIndex = memberName.IndexOf('=');
            if (valueIndex != -1)
            {
                value = memberName.Substring(valueIndex + 1);
                memberName = memberName.Substring(0, valueIndex);
            }
            bool isDepth;
            var member = GetMember(memberName = CheckMemberName(memberName), out isDepth);
            if (member == null) return;
            PushCode(null);
            CurrentCode.Append(@"
            ", IfName, " = false;");
            if (isDepth)
            {
                var names = SplitMemberName(memberName);
                for (var index = 0; index != names.Length - 1; ++index) IfStart(names[index], false);
                IfThen(GetMember(names[names.Length - 1], out isDepth), node, value, IfName, names.Length - 1);
            }
            else IfThen(member, node, value, IfName, 0);
        }

        /// <summary>
        ///     not代码段处理
        /// </summary>
        /// <param name="member">成员节点</param>
        /// <param name="node">代码树节点</param>
        /// <param name="value">匹配值</param>
        /// <param name="ifName">逻辑变量名称</param>
        /// <param name="popCount">删除成员节点数量</param>
        protected void Not(TmphMemberNode member, TNodeType node, string value, string ifName, int popCount)
        {
            if (member.Type.IsStruct || member.Type.Type.IsEnum)
            {
                if (value != null)
                {
                    CurrentCode.Append(@"
                if (", member.Path, @".ToString() != @""", value.Replace(@"""", @""""""), @""")");
                }
                else if (member.Type.IsBool)
                {
                    CurrentCode.Append(@"
                if (!(bool)", member.Path, ")");
                }
                else if (member.Type.IsAjaxToString)
                {
                    CurrentCode.Append(@"
                if (", member.Path, " == 0)");
                }
            }
            else if (value != null)
            {
                CurrentCode.Append(@"
                if (", member.Path, @" == null || ", member.Path, @".ToString() != @""", value.Replace(@"""", @""""""),
                    @""")");
            }
            else
            {
                CurrentCode.Append(@"
                if (", member.Path, " == null)");
            }
            CurrentCode.Append(@"
                {
                    ", ifName, @" = true;
                }");
            while (popCount != 0)
            {
                IfEnd(true);
                --popCount;
            }
            CurrentCode.Append(@"
            if (", ifName, @")
            {");
            Skin(node);
            PushCode(null);
            CurrentCode.Append(@"
            }");
        }

        /// <summary>
        ///     绑定的数据为false或者0或者null时输出代码
        /// </summary>
        /// <param name="node">代码树节点</param>
        protected void Not(TNodeType node)
        {
            string value = null, memberName = node.TemplateMemberName;
            var valueIndex = memberName.IndexOf('=');
            if (valueIndex != -1)
            {
                value = memberName.Substring(valueIndex + 1);
                memberName = memberName.Substring(0, valueIndex);
            }
            bool isDepth;
            var member = GetMember(memberName = CheckMemberName(memberName), out isDepth);
            if (member == null) return;
            PushCode(null);
            CurrentCode.Append(@"
            ", IfName, " = false;");
            if (isDepth)
            {
                var names = SplitMemberName(memberName);
                for (var index = 0; index != names.Length - 1; ++index) IfStart(names[index], false);
                Not(GetMember(names[names.Length - 1], out isDepth), node, value, IfName, names.Length - 1);
            }
            else Not(member, node, value, IfName, 0);
        }

        /// <summary>
        ///     根据类型名称获取子段模板
        /// </summary>
        /// <param name="typeName">类型名称</param>
        /// <param name="name">子段模板名称</param>
        /// <returns>子段模板</returns>
        protected virtual TNodeType FromNameNode(string typeName, string name)
        {
            return default(TNodeType);
        }

#if MONO
#else

        /// <summary>
        ///     子段模板处理
        /// </summary>
        /// <param name="node">代码树节点</param>
        protected void Name(TNodeType node)
        {
            TmphHashString nameKey = node.TemplateMemberName;
            if (NameNodes.ContainsKey(nameKey)) OnError(ViewType.fullName() + " NAME " + nameKey + " 重复定义");
            NameNodes[nameKey] = node;
            if (node.ChildCount != 0) Skin(node);
        }

        /// <summary>
        ///     引用子段模板处理
        /// </summary>
        /// <param name="node">代码树节点</param>
        protected void FromName(TNodeType node)
        {
            var memberName = node.TemplateMemberName;
            var typeIndex = memberName.IndexOf('.');
            if (typeIndex == -1)
            {
                if (!NameNodes.TryGetValue(memberName, out node))
                    OnError(ViewType.fullName() + " NAME " + memberName + " 未定义");
            }
            else
            {
                node = FromNameNode(memberName.Substring(0, typeIndex), memberName.Substring(++typeIndex));
            }
            if (node != null && node.ChildCount != 0) Skin(node);
        }

#endif

        /// <summary>
        ///     子段程序代码处理
        /// </summary>
        /// <param name="node">代码树节点</param>
        protected void Part(TNodeType node)
        {
            var memberName = node.TemplateMemberName;
            PushCode(null);
            CurrentCode.Add(@"
            stringBuilder _PART_" + memberName + @"_ = _code_;
            _code_ = new stringBuilder();");
            var historyCode = CurrentCode;
            CurrentCode = new TmphStringBuilder();
            Skin(node);
            PushCode(null);
            var partCode = CurrentCode.ToString();
            PartCodes[memberName] = partCode;
            CurrentCode = historyCode;
            CurrentCode.Add(partCode);
            CurrentCode.Add(@"
            _partCodes_[""" + memberName + @"""] = _code_.ToString();
            _code_ = _PART_" + memberName + @"_;
            _code_.Add(_partCodes_[""" + memberName + @"""]);");
        }
    }
}