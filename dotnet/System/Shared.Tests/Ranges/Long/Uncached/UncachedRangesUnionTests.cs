// <copyright file="UncachedRangesUnionTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Ranges.Long
{
    public class UncachedRangesUnionTests : RangesUnionTests
    {
        public override IRanges<long> Ranges { get; }

        public UncachedRangesUnionTests() => this.Ranges = new DefaultStructRanges<long>();
    }
}
