// <copyright file="IOperator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Ranges
{
    using System;
    using System.Collections.Generic;

    public interface IRange : IEquatable<IRange>, IEnumerable<long>
    {
        bool IsEmpty { get; }

        bool Contains(long item);

        long[]? Save();
    }
}
