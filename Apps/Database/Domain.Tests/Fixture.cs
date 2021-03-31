// <copyright file="Fixture.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Derivations.Default;
    using Meta;

    public class Fixture
    {
        private static readonly MetaBuilder MetaBuilder = new MetaBuilder();

        public Fixture()
        {
            this.MetaPopulation = MetaBuilder.Build();
            this.M = new M(this.MetaPopulation);
            var rules = Rules.Create(this.M);
            this.Engine = new Engine(rules);
        }

        public MetaPopulation MetaPopulation { get; set; }

        public M M { get; set; }

        public Engine Engine { get; set; }

        public void Dispose() => this.MetaPopulation = null;
    }
}
