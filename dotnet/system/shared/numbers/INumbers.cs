// <copyright file="IOperator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Numbers
{
    using System.Collections.Generic;

    public interface INumbers
    {
        public object? From(IEnumerable<long>? values);

        public object? From(params long[] values);

        public object? From(long value);

        public IEnumerable<long> Enumerate(object? numbers);

        long[] ToArray(object? numbers);

        bool AreEqual(object? numbers, object? otherNumbers);

        bool Contains(object numbers, long value);

        public object? Add(object? numbers, long other);

        public object? Remove(object? numbers, long value);

        public object? Union(object? numbers, object? other);

        public object? Except(object? numbers, object? other);
    }
}
