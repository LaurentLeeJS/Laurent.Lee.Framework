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

namespace Laurent.Lee.CLB.Sql.Expression
{
    /// <summary>
    ///     委托关联表达式转SQL表达式
    /// </summary>
    internal abstract class TmphConverter
    {
        /// <summary>
        ///     参数成员名称集合
        /// </summary>
        internal string FirstMemberName;

        /// <summary>
        ///     SQL流
        /// </summary>
        protected TmphCharStream stream;

        /// <summary>
        ///     SQL流
        /// </summary>
        internal TmphCharStream Stream
        {
            get { return stream; }
        }

        /// <summary>
        ///     创建SQL
        /// </summary>
        /// <param name="expression">委托关联表达式</param>
        /// <returns>SQL表达式</returns>
        internal unsafe string Create(TmphLambdaExpression expression)
        {
            var TmphBuffer = TmphClient.SqlBuffers.Get();
            try
            {
                using (stream = new TmphCharStream(TmphBuffer.Char, TmphClient.SqlBufferSize))
                {
                    create(expression, stream);
                    return stream.ToString();
                }
            }
            finally
            {
                TmphClient.SqlBuffers.Push(ref TmphBuffer);
            }
        }

        /// <summary>
        ///     创建SQL
        /// </summary>
        /// <param name="expression">委托关联表达式</param>
        /// <param name="sqlStream">SQL表达式流</param>
        protected abstract void create(TmphLambdaExpression expression, TmphCharStream stream);
    }
}