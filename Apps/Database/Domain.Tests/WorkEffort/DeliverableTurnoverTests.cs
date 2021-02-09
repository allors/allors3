// <copyright file="DeliverableTurnoverTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using Allors.Database.Derivations;
    using Xunit;

    public class DeliverableTurnoverTests : DomainTest, IClassFixture<Fixture>
    {
        public DeliverableTurnoverTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnCreatedThrowValidationError()
        {
            var basePrice = new DeliverableTurnoverBuilder(this.Session).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("DeliverableTurnover.WorkEffort, DeliverableTurnover.EngagementItem at least one"));
        }
    }
}
