// <copyright file="IOperator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Ranges
{
    using System.Collections.Generic;

    public interface IRanges
    {
        IRange From(IEnumerable<long>? sortedItems);

        IRange From(params long[] sortedItems);

        IRange From(long item);

        IRange Unbox(object boxed);

        IRange Import(IEnumerable<long>? unsortedItems);

        IRange Import(params long[] unsortedItems);

        IRange Add(IRange range, long item);

        IRange Remove(IRange range, long item);

        IRange Union(IRange range, IRange other);

        IRange Except(IRange range, IRange other);
    }
}
