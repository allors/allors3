// <copyright file="SurchargeComponentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Derivations.Errors;
    using Meta;
    using Xunit;

    public class SurchargeComponentTests : DomainTest, IClassFixture<Fixture>
    {
        public SurchargeComponentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnCreatedThrowValidationError()
        {
            var surchargeComponent = new SurchargeComponentBuilder(this.Transaction).Build();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("SurchargeComponent.Price, SurchargeComponent.Percentage at least one"));
        }

        [Fact]
        public void ChangedPriceThrowValidationError()
        {
            var surchargeComponent = new SurchargeComponentBuilder(this.Transaction).WithPrice(1).Build();
            this.Transaction.Derive(false);

            surchargeComponent.RemovePrice();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("SurchargeComponent.Price, SurchargeComponent.Percentage at least one"));
        }

        [Fact]
        public void ChangedPercentageThrowValidationError()
        {
            var surchargeComponent = new SurchargeComponentBuilder(this.Transaction).WithPercentage(1).Build();
            this.Transaction.Derive(false);

            surchargeComponent.RemovePercentage();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("SurchargeComponent.Price, SurchargeComponent.Percentage at least one"));
        }

        [Fact]
        public void ChangedPriceThrowValidationErrorAtmostOne()
        {
            var surchargeComponent = new SurchargeComponentBuilder(this.Transaction).WithPercentage(1).Build();
            this.Transaction.Derive(false);

            surchargeComponent.Price = 1;

            var errors = this.Transaction.Derive(false).Errors.Cast<DerivationErrorAtMostOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.SurchargeComponent.Price,
                this.M.SurchargeComponent.Percentage,
            }, errors.SelectMany(v => v.RoleTypes));
        }

        [Fact]
        public void ChangedPercentageThrowValidationErrorAtmostOne()
        {
            var surchargeComponent = new SurchargeComponentBuilder(this.Transaction).WithPrice(1).Build();
            this.Transaction.Derive(false);

            surchargeComponent.Percentage = 1;

            var errors = this.Transaction.Derive(false).Errors.Cast<DerivationErrorAtMostOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.SurchargeComponent.Price,
                this.M.SurchargeComponent.Percentage,
            }, errors.SelectMany(v => v.RoleTypes));
        }
    }
}
