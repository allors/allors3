// <copyright file="IOperator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Ranges
{
    using System.Collections.Generic;

    public interface IRanges
    {
        Range From(IEnumerable<long>? sortedItems);

        Range From(params long[] sortedItems);

        Range From(long item);

        Range Unbox(object boxed);

        Range Import(IEnumerable<long>? unsortedItems);

        Range Import(params long[] unsortedItems);

        Range Add(Range range, long item);

        Range Remove(Range range, long item);

        Range Union(Range range, Range other);

        Range Except(Range range, Range other);
    }
}
