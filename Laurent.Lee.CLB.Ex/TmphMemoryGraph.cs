using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Laurent.Lee.CLB
{
    public static partial class TmphMemoryGraph
    {
        [Laurent.Lee.CLB.Emit.TmphDataSerialize(IsMemberMap = false)]
        public sealed class TmphType
        {
            public string FullName;
            public TmphField[] Fields;
            public int Size;
        }

        [Laurent.Lee.CLB.Emit.TmphDataSerialize(IsMemberMap = false)]
        public struct TmphField
        {
            public string Name;
            public TmphType Type;

            public void Set(string name, TmphType type)
            {
                Name = name;
                Type = type;
            }
        }

        [Laurent.Lee.CLB.Emit.TmphDataSerialize(IsMemberMap = false)]
        public sealed class TmphValue
        {
            public TmphType Type;
            public TmphValue[] Values;
        }

        [Laurent.Lee.CLB.Emit.TmphDataSerialize(IsMemberMap = false)]
        public struct TmphStaticValue
        {
            public string Name;
            public TmphValue Value;
        }

        [Laurent.Lee.CLB.Emit.TmphDataSerialize(IsMemberMap = false)]
        public struct TmphStaticType
        {
            public string TypeName;
            public TmphStaticValue[] Values;

            public void Set(Type type, TmphList<TmphStaticValue> values)
            {
                TypeName = type.FullName;
                Values = values.GetArray();
            }
        }

        private sealed class TmphGraphBuilder
        {
            private const int maxDepth = 256;

            private sealed class TmphTypeInfo
            {
                public TmphType Type;
                public TmphList<FieldInfo> Fields;

                public void Add(FieldInfo field)
                {
                    if (Fields == null) Fields = new TmphList<FieldInfo>();
                    Fields.Add(field);
                }
            }

            public TmphStaticType[] StaticTypes;
            private Dictionary<Type, TmphTypeInfo> types;
            private Dictionary<TmphObjectReference, TmphValue> values;
            private Dictionary<Type, Action> arrayBuilders;
            private TmphList<TmphStaticValue> staticValues;
            private Dictionary<TmphHashString, TmphTypeInfo> typeNames;
            private TmphSearcher searcher;

            public TmphGraphBuilder()
            {
                HashSet<Type> checkTypes = TmphHashSet.CreateOnly<Type>();
                foreach (Type type in Laurent.Lee.CLB.TmphCheckMemory.GetTypes().ToArray())
                {
                    if (checkTypes.Contains(type)) CLB.TmphLog.Error.Add("重复类型 " + type.FullName, false, false);
                    else checkTypes.Add(type);
                }
                int count = checkTypes.Count;
                if (count != 0)
                {
                    StaticTypes = new TmphStaticType[count];
                    types = TmphDictionary.CreateOnly<Type, TmphTypeInfo>();
                    values = TmphDictionary<TmphObjectReference>.Create<TmphValue>();
                    arrayBuilders = TmphDictionary.CreateOnly<Type, Action>();
                    staticValues = new TmphList<TmphStaticValue>();
                    foreach (Type type in checkTypes)
                    {
                        currentType = type;
                        buildStatic();
                        StaticTypes[checkTypes.Count - count].Set(type, staticValues);
                        if (--count == 0) break;
                        staticValues.Empty();
                    }
                    values = null;
                    arrayBuilders = null;
                    staticValues = null;
                    checkTypes = null;
                    typeNames = TmphDictionary.CreateHashString<TmphTypeInfo>();
                    foreach (TmphTypeInfo type in types.Values) typeNames[type.Type.FullName] = type;
                    types = null;
                    searcher = new TmphSearcher(StaticTypes);
                    searcher.OnType = searchTypeFields;
                    searcher.Start();
                }
            }

            private Type currentType;
            private TmphTypeInfo type;
            private object currentValue;
            private int depth;
            private TmphValue value;
            private Type fieldType;

            private void buildStatic()
            {
                foreach (FieldInfo field in currentType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (!(fieldType = field.FieldType).IsPrimitive && !fieldType.IsPointer && !fieldType.IsEnum
                        && (currentValue = field.GetValue(null)) != null)
                    {
                        currentType = currentValue.GetType();
                        if (currentType.IsClass || currentType.IsInterface)
                        {
                            value = null;
                            if (values.TryGetValue(new TmphObjectReference { Value = currentValue }, out value))
                            {
                                staticValues.Add(new TmphStaticValue { Name = field.Name, Value = value });
                            }
                            else
                            {
                                values.Add(new TmphObjectReference { Value = currentValue }, value = new TmphValue());
                                staticValues.Add(new TmphStaticValue { Name = field.Name, Value = value });
                                buildValue();
                            }
                        }
                        else
                        {
                            staticValues.Add(new TmphStaticValue { Name = field.Name, Value = new TmphValue() });
                            buildValue();
                        }
                    }
                }
            }

            private void buildValue()
            {
                ++depth;
                if (!types.TryGetValue(currentType, out type))
                {
                    types.Add(currentType, type = new TmphTypeInfo { Type = new TmphType() });
                    buildType();
                }
                this.value.Type = type.Type;
                if (type.Fields == null)
                {
                    if (currentType == typeof(string))
                    {
                        this.value.Type = new TmphType { FullName = type.Type.FullName, Size = ((string)this.currentValue).Length };
                    }
                    else if (currentType.IsArray)
                    {
                        TmphArrayType = currentType.GetElementType();
                        if (TmphArrayType.IsPrimitive || TmphArrayType.IsPointer || TmphArrayType.IsEnum)
                        {
                            this.value.Type = new TmphType { FullName = this.value.Type.FullName, Size = ((Array)this.currentValue).Length };
                        }
                        else
                        {
                            if (!arrayBuilders.TryGetValue(TmphArrayType, out arrayBuilder))
                            {
                                arrayBuilders.Add(TmphArrayType, arrayBuilder = (Action)Delegate.CreateDelegate(typeof(Action), this, buildArrayMethod.MakeGenericMethod(TmphArrayType)));
                            }
                            arrayBuilder();
                        }
                    }
                }
                else if (depth < maxDepth)
                {
                    object currentValue = this.currentValue;
                    TmphValue value = this.value;
                    int count = type.Fields.Count, index = count;
                    foreach (FieldInfo field in type.Fields.Unsafer.Array)
                    {
                        if ((this.currentValue = field.GetValue(currentValue)) != null)
                        {
                            if (value.Values == null) value.Values = new TmphValue[count];
                            currentType = this.currentValue.GetType();
                            if (currentType.IsClass || currentType.IsInterface)
                            {
                                if (!values.TryGetValue(new TmphObjectReference { Value = this.currentValue }, out value.Values[count - index]))
                                {
                                    values.Add(new TmphObjectReference { Value = this.currentValue }, value.Values[count - index] = this.value = new TmphValue());
                                    buildValue();
                                }
                            }
                            else
                            {
                                value.Values[count - index] = this.value = new TmphValue();
                                buildValue();
                            }
                        }
                        if (--index == 0) break;
                    }
                }
                --depth;
            }

            private Type baseType;
            private TmphTypeInfo TValueType;

            private void buildType()
            {
                type.Type.FullName = currentType.FullName;
                if (currentType.IsPrimitive || currentType.IsPointer || currentType.IsEnum || currentType.IsArray || currentType == typeof(string))
                {
                    type.Type.Size = getMemorySize(currentType);
                }
                else
                {
                    if (currentType.IsValueType)
                    {
                        baseType = currentType;
                        buildFields();
                    }
                    else if (!currentType.IsInterface)
                    {
                        for (baseType = currentType; baseType != typeof(object); baseType = baseType.BaseType) buildFields();
                    }
                }
            }

            private void buildFields()
            {
                foreach (FieldInfo field in baseType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if ((fieldType = field.FieldType).IsPrimitive || fieldType.IsPointer || fieldType.IsEnum)
                    {
                        type.Type.Size += getMemorySize(fieldType);
                    }
                    else if (fieldType.IsValueType)
                    {
                        TValueType = buildValueType(fieldType);
                        if (TValueType.Fields == null) type.Type.Size += TValueType.Type.Size;
                        else type.Add(field);
                    }
                    else
                    {
                        type.Add(field);
                        if (!fieldType.IsValueType) type.Type.Size = Laurent.Lee.CLB.TmphPub.MemoryBytes;
                    }
                }
            }

            private TmphTypeInfo buildValueType(Type type)
            {
                TmphTypeInfo TValueType;
                if (!types.TryGetValue(type, out TValueType))
                {
                    TValueType = new TmphTypeInfo { Type = new TmphType { FullName = type.FullName } };
                    foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if ((fieldType = field.FieldType).IsPrimitive || fieldType.IsPointer || fieldType.IsEnum)
                        {
                            TValueType.Type.Size += getMemorySize(fieldType);
                        }
                        else if (fieldType.IsValueType)
                        {
                            TValueType = buildValueType(fieldType);
                            if (TValueType.Fields == null) TValueType.Type.Size += TValueType.Type.Size;
                            else TValueType.Add(field);
                        }
                        else
                        {
                            TValueType.Add(field);
                            if (!fieldType.IsValueType) TValueType.Type.Size = Laurent.Lee.CLB.TmphPub.MemoryBytes;
                        }
                    }
                }
                return TValueType;
            }

            private Type TmphArrayType;
            private Action arrayBuilder;

            private void buildArray<TValueType>()
            {
                if (!TmphArrayType.IsInterface && !types.TryGetValue(TmphArrayType, out type))
                {
                    types.Add(currentType = TmphArrayType, type = new TmphTypeInfo { Type = new TmphType() });
                    buildType();
                }
                TmphValue[] values = this.value.Values = new TmphValue[((TValueType[])this.currentValue).Length];
                int index = 0;
                if (TmphArrayType == typeof(string))
                {
                    foreach (string value in (string[])this.currentValue)
                    {
                        if (value.Length != 0)
                        {
                            values[index] = new TmphValue { Type = new TmphType { FullName = type.Type.FullName, Size = value.Length } };
                        }
                        ++index;
                    }
                }
                else if (depth < maxDepth)
                {
                    if (TmphArrayType.IsClass || TmphArrayType.IsInterface)
                    {
                        foreach (TValueType value in (TValueType[])this.currentValue)
                        {
                            if (value != null)
                            {
                                currentType = value.GetType();
                                if (currentType.IsClass || currentType.IsInterface)
                                {
                                    if (!this.values.TryGetValue(new TmphObjectReference { Value = value }, out values[index]))
                                    {
                                        this.values.Add(new TmphObjectReference { Value = this.currentValue = value }, values[index] = this.value = new TmphValue());
                                        buildValue();
                                    }
                                }
                                else
                                {
                                    this.currentValue = value;
                                    values[index] = this.value = new TmphValue();
                                    buildValue();
                                }
                            }
                            ++index;
                        }
                    }
                    else
                    {
                        foreach (TValueType value in (TValueType[])this.currentValue)
                        {
                            this.currentValue = value;
                            currentType = typeof(TValueType);
                            values[index++] = this.value = new TmphValue();
                            buildValue();
                        }
                    }
                }
            }

            private void searchTypeFields()
            {
                TmphTypeInfo type;
                if (typeNames.TryGetValue(searcher.Value.Type.FullName, out type) && type.Fields != null)
                {
                    int count = type.Fields.Count;
                    TmphField[] fields = type.Type.Fields = new TmphField[count];
                    foreach (FieldInfo field in type.Fields.Unsafer.Array)
                    {
                        fields[type.Fields.Count - count].Set(field.Name, type.Type);
                        if (--count == 0) break;
                    }
                }
            }

            private static readonly MethodInfo buildArrayMethod = typeof(TmphGraphBuilder).GetMethod("buildArray", BindingFlags.Instance | BindingFlags.NonPublic, null, TmphNullValue<Type>.Array, null);
            private static readonly Dictionary<Type, int> memorySizes;

            private static int getMemorySize(Type type)
            {
                int size;
                return memorySizes.TryGetValue(type, out size) ? size : Laurent.Lee.CLB.TmphPub.MemoryBytes;
            }

            static unsafe TmphGraphBuilder()
            {
                memorySizes = TmphDictionary.CreateOnly<Type, int>();
                memorySizes.Add(typeof(bool), sizeof(bool));
                memorySizes.Add(typeof(byte), sizeof(byte));
                memorySizes.Add(typeof(sbyte), sizeof(sbyte));
                memorySizes.Add(typeof(short), sizeof(short));
                memorySizes.Add(typeof(ushort), sizeof(ushort));
                memorySizes.Add(typeof(int), sizeof(int));
                memorySizes.Add(typeof(uint), sizeof(uint));
                memorySizes.Add(typeof(long), sizeof(long));
                memorySizes.Add(typeof(ulong), sizeof(ulong));
                memorySizes.Add(typeof(char), sizeof(char));
                memorySizes.Add(typeof(DateTime), sizeof(long));
                memorySizes.Add(typeof(float), sizeof(float));
                memorySizes.Add(typeof(double), sizeof(double));
                memorySizes.Add(typeof(decimal), sizeof(decimal));
                memorySizes.Add(typeof(Guid), sizeof(Guid));
            }
        }

        public static TmphStaticType[] Get()
        {
            return new TmphGraphBuilder().StaticTypes;
        }

        public sealed class TmphSearcher
        {
            private TmphStaticType[] staticTypes;
            public TmphStaticType StaticType { get; private set; }
            public TmphStaticValue StaticValue { get; private set; }
            public TmphValue Value { get; private set; }
            private HashSet<TmphHashString> types;
            private HashSet<TmphValue> values;
            private TmphList<string> path;

            public IEnumerable<string> Path
            {
                get { return path; }
            }

            public Action OnValue;
            public Action OnNewValue;
            public Action OnType;
            public bool IsStop;

            public TmphSearcher(TmphStaticType[] staticTypes)
            {
                this.staticTypes = staticTypes ?? TmphNullValue<TmphStaticType>.Array;
            }

            public void Start()
            {
                IsStop = false;
                if (types == null) types = TmphHashSet.CreateHashString();
                else types.Clear();
                if (values == null) values = TmphHashSet.CreateOnly<TmphValue>();
                else values.Clear();
                if (path == null) path = new TmphList<string>();
                else path.Clear();
                foreach (TmphStaticType type in staticTypes)
                {
                    if (IsStop) return;
                    StaticType = type;
                    foreach (TmphStaticValue value in type.Values.notNull())
                    {
                        if (IsStop) return;
                        StaticValue = value;
                        if (value.Value != null)
                        {
                            Value = value.Value;
                            searchValue();
                        }
                    }
                }
            }

            private void searchValue()
            {
                if (OnValue != null)
                {
                    OnValue();
                    if (IsStop) return;
                }
                if (!values.Contains(Value))
                {
                    values.Add(Value);
                    if (OnNewValue != null)
                    {
                        OnNewValue();
                        if (IsStop) return;
                    }
                    if (!types.Contains(Value.Type.FullName))
                    {
                        types.Add(Value.Type.FullName);
                        if (OnType != null)
                        {
                            OnType();
                            if (IsStop) return;
                        }
                    }
                    if (Value.Values != null)
                    {
                        TmphField[] fields = Value.Type.Fields;
                        int index = 0;
                        if (fields == null)
                        {
                            foreach (TmphValue value in Value.Values)
                            {
                                if (IsStop) break;
                                if (value != null)
                                {
                                    path.Add(index.toString());
                                    Value = value;
                                    searchValue();
                                    path.Unsafer.Pop();
                                }
                                ++index;
                            }
                        }
                        else
                        {
                            foreach (TmphValue value in Value.Values)
                            {
                                if (IsStop) break;
                                if (value != null)
                                {
                                    path.Add(fields[index].Name);
                                    Value = value;
                                    searchValue();
                                    path.Unsafer.Pop();
                                }
                                ++index;
                            }
                        }
                    }
                }
            }
        }

        private static int saveFileLock;

        public static void SaveFile(string fileName)
        {
            if (Interlocked.CompareExchange(ref saveFileLock, 1, 0) == 0)
            {
                Laurent.Lee.CLB.Threading.TmphThreadPool.TinyPool.Start(saveFile, fileName);
            }
        }

        private static void saveFile(string fileName)
        {
            try
            {
                GC.Collect();
                using (FileStream fileStream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                using (TmphUnmanagedStreamProxy stream = new TmphUnmanagedStreamProxy(fileStream))
                {
                    TmphLog.Default.Add("开始生成内存对象关系数据", false, false);
                    TmphStaticType[] values = Get();
                    GC.Collect();
                    Laurent.Lee.CLB.Emit.TmphDataSerializer.Serialize(values, stream);
                    TmphLog.Default.Add("内存对象关系数据生成完毕", false, false);
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
            }
            finally
            {
                saveFileLock = 0;
                GC.Collect();
            }
        }

#if MONO
#else

        public struct TmphMemoryInfo
        {
            public ulong Total;
            public ulong Avail;
        }

        public static TmphMemoryInfo GetMemoryInfo()
        {
            Win32.TmphKernel32.TmphMemoryStatuExpand memory = new Win32.TmphKernel32.TmphMemoryStatuExpand();
            if (Win32.TmphKernel32.GlobalMemoryStatusEx(memory))
            {
                return new TmphMemoryInfo { Total = memory.TotalPhys, Avail = memory.AvailPhys };
            }
            return default(TmphMemoryInfo);
        }

#endif
    }
}