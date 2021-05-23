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
    public static class UnitTags
    {
        /// <summary>
        /// The tag for the binary <see cref="IObjectType"/>.
        /// </summary>
        public const int Binary = 1;

        /// <summary>
        /// The tag for the boolean <see cref="IObjectType"/>.
        /// </summary>
        public const int Boolean = 2;

        /// <summary>
        /// The tag for the date time <see cref="IObjectType"/>.
        /// </summary>
        public const int DateTime = 3;

        /// <summary>
        /// The tag for the decimal <see cref="IObjectType"/>.
        /// </summary>
        public const int Decimal = 4;

        /// <summary>
        /// The tag for the float <see cref="IObjectType"/>.
        /// </summary>
        public const int Float = 5;

        /// <summary>
        /// The tag for the integer <see cref="IObjectType"/>.
        /// </summary>
        public const int Integer = 6;

        /// <summary>
        /// The tag for the string <see cref="IObjectType"/>.
        /// </summary>
        public const int String = 7;

        /// <summary>
        /// The tag for the unique <see cref="IObjectType"/>.
        /// </summary>
        public const int Unique = 8;
    }
}
