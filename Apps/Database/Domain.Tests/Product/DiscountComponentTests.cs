// <copyright file="DiscountComponentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using Allors.Database.Derivations;
    using Resources;
    using Xunit;

    public class DiscountComponentTests : DomainTest, IClassFixture<Fixture>
    {
        public DiscountComponentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnCreatedThrowValidationError()
        {
            var discountComponent = new DiscountComponentBuilder(this.Session).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("DiscountComponent.Price, DiscountComponent.Percentage at least one"));
        }

        [Fact]
        public void ChangedPriceThrowValidationError()
        {
            var discountComponent = new DiscountComponentBuilder(this.Session).WithPrice(1).Build();
            this.Session.Derive(false);

            discountComponent.RemovePrice();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("DiscountComponent.Price, DiscountComponent.Percentage at least one"));
        }

        [Fact]
        public void ChangedPercentageThrowValidationError()
        {
            var discountComponent = new DiscountComponentBuilder(this.Session).WithPercentage(1).Build();
            this.Session.Derive(false);

            discountComponent.RemovePercentage();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("DiscountComponent.Price, DiscountComponent.Percentage at least one"));
        }

        [Fact]
        public void ChangedPriceThrowValidationErrorAtmostOne()
        {
            var discountComponent = new DiscountComponentBuilder(this.Session).WithPercentage(1).Build();
            this.Session.Derive(false);

            discountComponent.Price = 1;

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: DiscountComponent.Price\nDiscountComponent.Percentage"));
        }

        [Fact]
        public void ChangedPercentageThrowValidationErrorAtmostOne()
        {
            var discountComponent = new DiscountComponentBuilder(this.Session).WithPrice(1).Build();
            this.Session.Derive(false);

            discountComponent.Percentage = 1;

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: DiscountComponent.Price\nDiscountComponent.Percentage"));
        }
    }
}
