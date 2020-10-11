// <copyright file="PushRequestObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Database.Push
{
    /// <summary>
    ///  New objects require NI and T.
    ///  Existing objects require I and V.
    /// </summary>
    public class PushRequestObject
    {
        /// <summary>
        /// Gets or sets the (database) id.
        /// </summary>
        public string I { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        public string V { get; set; }

        public PushRequestRole[] Roles { get; set; }
    }
}
