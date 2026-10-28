// <copyright file="UnitConversionErrors.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json
{
    using System;

    /// <summary>
    /// Builds the exceptions that unit converters throw when a value cannot become the unit a tag names.
    /// </summary>
    public static class UnitConversionErrors
    {
        public const string IntegerRange = "a whole number from -2147483648 through 2147483647";

        public static string UnitName(string tag) =>
            tag switch
            {
                UnitTags.Binary => UnitNames.Binary,
                UnitTags.Boolean => UnitNames.Boolean,
                UnitTags.DateTime => UnitNames.DateTime,
                UnitTags.Decimal => UnitNames.Decimal,
                UnitTags.Float => UnitNames.Float,
                UnitTags.Integer => UnitNames.Integer,
                UnitTags.String => UnitNames.String,
                UnitTags.Unique => UnitNames.Unique,
                _ => "unknown unit",
            };

        /// <summary>
        /// The exception for a source that no rule converts to the unit of the tag.
        /// </summary>
        public static Exception Unsupported(string tag, string source) =>
            new Exception($"Cannot convert {source} to {UnitName(tag)} (tag '{tag}').");

        /// <summary>
        /// The exception for a number that is not a whole number within the Integer range.
        /// </summary>
        public static Exception NotAnInteger(string source, string number) =>
            new Exception($"Cannot convert {source} to {UnitNames.Integer} (tag '{UnitTags.Integer}'): {number} is not {IntegerRange}.");
    }
}
