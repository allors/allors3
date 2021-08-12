// <copyright file="Fixture.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Tests.Workspace
{
    using System;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Domain;
    using Allors.Database.Meta;

    public class Fixture : IDisposable
    {
        private static readonly MetaBuilder MetaBuilder = new MetaBuilder();

        public Fixture()
        {
            this.M = MetaBuilder.Build();
            var rules = Rules.Create(this.M);
            this.Engine = new Engine(rules);
        }

        public MetaPopulation M { get; private set; }

        public Engine Engine { get; }

        public void Dispose() => this.M = null;
    }
}
