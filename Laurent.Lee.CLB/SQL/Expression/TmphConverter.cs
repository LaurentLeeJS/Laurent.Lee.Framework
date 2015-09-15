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