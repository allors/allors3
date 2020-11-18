// <copyright file="Fixture.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Meta;

    public class Fixture
    {
        private static readonly MetaBuilder MetaBuilder = new MetaBuilder();

        public Fixture() => this.MetaPopulation = MetaBuilder.Build();

        public MetaPopulation MetaPopulation { get; set; }

        public void Dispose() => this.MetaPopulation = null;
    }
}
