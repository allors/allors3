// <copyright file="PushReadChanges.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.PushRead
{
    using Allors.Protocol.Json.Api.Push;

    public class PushReadChanges
    {
        /// <summary>
        /// New objects with negative workspace IDs.
        /// </summary>
        public PushRequestNewObject[] n { get; set; }

        /// <summary>
        /// Submitted existing objects, including unchanged baselines.
        /// </summary>
        public PushRequestObject[] o { get; set; }
    }
}
