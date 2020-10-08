// <copyright file="Origin.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    using System;

    [Flags]
    public enum Origin
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// Remote origin.
        /// </summary>
        Remote = 1,

        /// <summary>
        /// Local origin.
        /// </summary>
        Local = 2,

        /// <summary>
        /// Working origin.
        /// </summary>
        Working = 4,
    }
}
