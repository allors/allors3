// <copyright file="Prefetcher.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    internal sealed class UntraceablePrefetcher : Prefetcher
    {
        public UntraceablePrefetcher(Transaction transaction) : base(transaction)
        {
        }
    }
}
