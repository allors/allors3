// <copyright file="UnitTags.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    /// <summary>
    /// The tags for units.
    /// Do not use tags for long term persistence, UnitTypeIds should be used for that purpose.
    /// </summary>
    public enum UnitTags
    {
        /// <summary>
        /// The tag for the binary <see cref="IObjectType"/>.
        /// </summary>
        Binary = 0,

        /// <summary>
        /// The tag for the boolean <see cref="IObjectType"/>.
        /// </summary>
        Boolean = 1,

        /// <summary>
        /// The tag for the date time <see cref="IObjectType"/>.
        /// </summary>
        DateTime = 2,

        /// <summary>
        /// The tag for the decimal <see cref="IObjectType"/>.
        /// </summary>
        Decimal = 3,

        /// <summary>
        /// The tag for the float <see cref="IObjectType"/>.
        /// </summary>
        Float = 4,

        /// <summary>
        /// The tag for the integer <see cref="IObjectType"/>.
        /// </summary>
        Integer = 5,

        /// <summary>
        /// The tag for the string <see cref="IObjectType"/>.
        /// </summary>
        String = 6,

        /// <summary>
        /// The tag for the unique <see cref="IObjectType"/>.
        /// </summary>
        Unique = 7,
    }
}
