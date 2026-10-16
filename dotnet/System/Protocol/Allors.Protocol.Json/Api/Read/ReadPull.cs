// <copyright file="ReadPull.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Read
{
    using Allors.Protocol.Json.Api.Pull;

    public class ReadPull
    {
        /// <summary>
        /// Declarative selections.
        /// </summary>
        public Data.Pull[] l { get; set; }

        /// <summary>
        /// Configured dependencies; an empty array means none.
        /// </summary>
        public PullDependency[] d { get; set; }
    }
}
