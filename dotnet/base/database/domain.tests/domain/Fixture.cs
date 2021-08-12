// <copyright file="DomainTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using Configuration.Derivations.Default;
    using Meta;

    public class Fixture : IDisposable
    {
        private static readonly MetaBuilder MetaBuilder = new MetaBuilder();

        public Fixture()
        {
            this.MetaPopulation = MetaBuilder.Build();
            var rules = Rules.Create(this.MetaPopulation);
            this.Engine = new Engine(rules);
        }

        public MetaPopulation MetaPopulation { get; set; }

        public Engine Engine { get; set; }

        public void Dispose() => this.MetaPopulation = null;
    }
}
