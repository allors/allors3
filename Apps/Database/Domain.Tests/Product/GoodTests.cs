
// <copyright file="GoodTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using TestPopulation;
    using Xunit;

    public class GoodTests : DomainTest, IClassFixture<Fixture>
    {
        public GoodTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnInitAddProductIdentification()
        {
            this.Session.GetSingleton().Settings.UseProductNumberCounter = true;

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Single(nonUnifiedGood.ProductIdentifications);
        }
    }
}
