// <copyright file="IOperator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Ranges
{
    using System.Collections.Generic;

    public interface IRanges
    {
        IRange Import(IEnumerable<long>? unsortedItems);

        IRange Load(IEnumerable<long>? sortedItems);

        IRange Load(params long[] sortedItems);

        IRange Load(long item);

        IRange Ensure(object? nullable);

        IRange Add(IRange? range, long item);

        IRange Remove(IRange? range, long item);

        IRange Union(IRange? range, IRange? other);

        IRange Except(IRange? range, IRange? other);
    }
}
