// <copyright file="CacheTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Tracing
{
    public static class TransactionExtensions
    {
        public static Sink GetSink(this ITransaction @this) => (@this as Transaction)?.Database.Sink as Sink;

        public static SinkTree GetSinkTree(this ITransaction @this) => @this.GetSink().TreeByTransaction[@this];
    }
}
