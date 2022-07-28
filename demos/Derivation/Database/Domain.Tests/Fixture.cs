// <copyright file="DomainTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using Allors.Database.Meta;

    public class Fixture : IDisposable
    {
        private static readonly MetaBuilder MetaBuilder = new MetaBuilder();

        public Fixture() => this.MetaPopulation = MetaBuilder.Build();

        public MetaPopulation MetaPopulation { get; set; }

        public void Dispose() => this.MetaPopulation = null;
    }
}
