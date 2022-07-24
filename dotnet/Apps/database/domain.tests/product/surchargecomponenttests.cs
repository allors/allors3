// <copyright file="SurchargeComponentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Xunit;

    public class SurchargeComponentTests : DomainTest, IClassFixture<Fixture>
    {
        public SurchargeComponentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnCreatedThrowValidationError()
        {
            var surchargeComponent = new SurchargeComponentBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.StartsWith("SurchargeComponent.Price, SurchargeComponent.Percentage at least one"));
        }

        [Fact]
        public void ChangedPriceThrowValidationError()
        {
            var surchargeComponent = new SurchargeComponentBuilder(this.Transaction).WithPrice(1).Build();
            this.Derive();

            surchargeComponent.RemovePrice();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.StartsWith("SurchargeComponent.Price, SurchargeComponent.Percentage at least one"));
        }

        [Fact]
        public void ChangedPercentageThrowValidationError()
        {
            var surchargeComponent = new SurchargeComponentBuilder(this.Transaction).WithPercentage(1).Build();
            this.Derive();

            surchargeComponent.RemovePercentage();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.StartsWith("SurchargeComponent.Price, SurchargeComponent.Percentage at least one"));
        }

        [Fact]
        public void ChangedPriceThrowValidationErrorAtmostOne()
        {
            var surchargeComponent = new SurchargeComponentBuilder(this.Transaction).WithPercentage(1).Build();
            this.Derive();

            surchargeComponent.Price = 1;

            var errors = this.Derive().Errors.OfType<IDerivationErrorAtMostOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.SurchargeComponent.Price,
                this.M.SurchargeComponent.Percentage,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedPercentageThrowValidationErrorAtmostOne()
        {
            var surchargeComponent = new SurchargeComponentBuilder(this.Transaction).WithPrice(1).Build();
            this.Derive();

            surchargeComponent.Percentage = 1;

            var errors = this.Derive().Errors.OfType<IDerivationErrorAtMostOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.SurchargeComponent.Price,
                this.M.SurchargeComponent.Percentage,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }
    }
}
