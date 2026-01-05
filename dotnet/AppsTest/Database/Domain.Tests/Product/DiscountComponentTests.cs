// <copyright file="DiscountComponentTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Xunit;

    public class DiscountComponentTests : DomainTest, IClassFixture<Fixture>
    {
        public DiscountComponentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnCreatedThrowValidationError()
        {
            var discountComponent = new DiscountComponentBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.StartsWith("DiscountComponent.Price, DiscountComponent.Percentage at least one"));
        }

        [Fact]
        public void ChangedPriceThrowValidationError()
        {
            var discountComponent = new DiscountComponentBuilder(this.Transaction).WithPrice(1).Build();
            this.Derive();

            discountComponent.RemovePrice();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.StartsWith("DiscountComponent.Price, DiscountComponent.Percentage at least one"));
        }

        [Fact]
        public void ChangedPercentageThrowValidationError()
        {
            var discountComponent = new DiscountComponentBuilder(this.Transaction).WithPercentage(1).Build();
            this.Derive();

            discountComponent.RemovePercentage();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.StartsWith("DiscountComponent.Price, DiscountComponent.Percentage at least one"));
        }

        [Fact]
        public void ChangedPriceThrowValidationErrorAtmostOne()
        {
            var discountComponent = new DiscountComponentBuilder(this.Transaction).WithPercentage(1).Build();
            this.Derive();

            discountComponent.Price = 1;

            var errors = this.Derive().Errors.OfType<IDerivationErrorAtMostOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.DiscountComponent.Price,
                this.M.DiscountComponent.Percentage,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedPercentageThrowValidationErrorAtmostOne()
        {
            var discountComponent = new DiscountComponentBuilder(this.Transaction).WithPrice(1).Build();
            this.Derive();

            discountComponent.Percentage = 1;

            var errors = this.Derive().Errors.OfType<IDerivationErrorAtMostOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.DiscountComponent.Price,
                this.M.DiscountComponent.Percentage,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }
    }
}
