// <copyright file="MediaContentFactory.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public class MediaContentFactory : IMediaContentFactory
    {
        public MediaContentFactory(Func<ITransaction, MediaContent> builder) => this.Builder = builder;

        // The build strategy. Supplied at composition (DefaultDatabaseServices) for the most flexibility:
        // the lambda is the strategy, so embedded, external, or any future variant needs no new type.
        // Reassignable as a startup/test seam; Create reads it per call, so swapping is service-cache-safe.
        public Func<ITransaction, MediaContent> Builder { get; set; }

        public MediaContent Create(ITransaction transaction) => this.Builder(transaction);
    }
}
