// <copyright file="DeliverableTurnoverTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class DeliverableTurnoverTests : DomainTest, IClassFixture<Fixture>
    {
        public DeliverableTurnoverTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnCreatedThrowValidationError()
        {
            var basePrice = new DeliverableTurnoverBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.StartsWith("DeliverableTurnover.WorkEffort, DeliverableTurnover.EngagementItem at least one"));
        }
    }
}
