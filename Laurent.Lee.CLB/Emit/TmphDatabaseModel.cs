/*
 *  Copyright 2015 Tony Lee

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 */

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    ///     数据库表格模型
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public abstract class TmphDatabaseModel<TValueType>
    {
        /// <summary>
        ///     获取自增字段获取器
        /// </summary>
        /// <param name="name"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        protected static Func<TValueType, int> getIdentityGetter32(string name, FieldInfo field)
        {
            var dynamicMethod = new DynamicMethod(name, typeof(int), new[] { typeof(TValueType) }, typeof(TValueType),
                true);
            var generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, field);
            if (field.FieldType != typeof(int) && field.FieldType != typeof(uint)) generator.Emit(OpCodes.Conv_I4);
            generator.Emit(OpCodes.Ret);
            return (Func<TValueType, int>)dynamicMethod.CreateDelegate(typeof(Func<TValueType, int>));
        }

        /// <summary>
        ///     获取自增字段设置器
        /// </summary>
        /// <param name="name"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        protected static Action<TValueType, int> getIdentitySetter32(string name, FieldInfo field)
        {
            var dynamicMethod = new DynamicMethod(name, null, new[] { typeof(TValueType), typeof(int) },
                typeof(TValueType), true);
            var generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            if (field.FieldType == typeof(long) || field.FieldType == typeof(ulong)) generator.Emit(OpCodes.Conv_I8);
            generator.Emit(OpCodes.Stfld, field);
            generator.Emit(OpCodes.Ret);
            return (Action<TValueType, int>)dynamicMethod.CreateDelegate(typeof(Action<TValueType, int>));
        }

        /// <summary>
        ///     获取关键字获取器
        /// </summary>
        /// <param name="name"></param>
        /// <param name="primaryKeys"></param>
        /// <returns></returns>
        internal static Func<TValueType, TKeyType> GetPrimaryKeyGetter<TKeyType>(string name, FieldInfo[] primaryKeys)
        {
            if (primaryKeys.Length == 0) return null;
            var dynamicMethod = new DynamicMethod(name, typeof(TKeyType), new[] { typeof(TValueType) }, typeof(TValueType),
                true);
            var generator = dynamicMethod.GetILGenerator();
            if (primaryKeys.Length == 1)
            {
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, primaryKeys[0]);
            }
            else
            {
                var key = generator.DeclareLocal(typeof(TKeyType));
                generator.Emit(OpCodes.Ldloca_S, key);
                generator.Emit(OpCodes.Initobj, typeof(TKeyType));
                foreach (var primaryKey in primaryKeys)
                {
                    generator.Emit(OpCodes.Ldloca_S, key);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldfld, primaryKey);
                    generator.Emit(OpCodes.Stfld,
                        typeof(TKeyType).GetField(primaryKey.Name, BindingFlags.Instance | BindingFlags.Public));
                }
                generator.Emit(OpCodes.Ldloc_0);
            }
            generator.Emit(OpCodes.Ret);
            return (Func<TValueType, TKeyType>)dynamicMethod.CreateDelegate(typeof(Func<TValueType, TKeyType>));
        }

        /// <summary>
        ///     获取关键字设置器
        /// </summary>
        /// <param name="name"></param>
        /// <param name="primaryKeys"></param>
        /// <returns></returns>
        internal static Action<TValueType, TKeyType> GetPrimaryKeySetter<TKeyType>(string name, FieldInfo[] primaryKeys)
        {
            if (primaryKeys.Length == 0) return null;
            var dynamicMethod = new DynamicMethod(name, null, new[] { typeof(TValueType), typeof(TKeyType) },
                typeof(TValueType), true);
            var generator = dynamicMethod.GetILGenerator();
            if (primaryKeys.Length == 1)
            {
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Stfld, primaryKeys[0]);
            }
            else
            {
                foreach (var primaryKey in primaryKeys)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarga_S, 1);
                    generator.Emit(OpCodes.Ldfld,
                        typeof(TKeyType).GetField(primaryKey.Name, BindingFlags.Instance | BindingFlags.Public));
                    generator.Emit(OpCodes.Stfld, primaryKey);
                }
            }
            generator.Emit(OpCodes.Ret);
            return (Action<TValueType, TKeyType>)dynamicMethod.CreateDelegate(typeof(Action<TValueType, TKeyType>));
        }
    }
}