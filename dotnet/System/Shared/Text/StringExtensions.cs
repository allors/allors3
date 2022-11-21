// <copyright file="Multiplicity.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    public static class StringExtensions
    {
        public static string Left(this string @this, int count) => @this.Length <= count ? @this : @this.Substring(0, count);

        public static string? Truncate(this string? @this, int maxLength, string suffix = "…") =>
            @this?.Length > maxLength - suffix.Length
                ? @this.Substring(0, maxLength - suffix.Length) + suffix
                : @this;
    }
}
