// <copyright file="Multiplicity.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    using System;

    [Flags]
    public enum Multiplicity
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// One to one.
        /// </summary>
        OneToOne = 1,

        /// <summary>
        /// One to many.
        /// </summary>
        OneToMany = 2,

        /// <summary>
        /// Many to one.
        /// </summary>
        ManyToOne = 4,

        /// <summary>
        /// Many to Many.
        /// </summary>
        ManyToMany = 8,
    }
}
