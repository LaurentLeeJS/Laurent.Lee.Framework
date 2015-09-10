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

namespace Laurent.Lee.CLB.Config
{
    /// <summary>
    ///     文件分块相关参数
    /// </summary>
    public sealed class TmphFileBlock
    {
        /// <summary>
        ///     默认文件分块相关参数
        /// </summary>
        public static readonly TmphFileBlock Default = new TmphFileBlock();

        /// <summary>
        ///     文件分块服务验证
        /// </summary>
        public string Verify;

        /// <summary>
        ///     文件分块相关参数
        /// </summary>
        private TmphFileBlock()
        {
            TmphPub.LoadConfig(this);
        }
    }
}