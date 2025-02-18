// <copyright file="RepeatingPurchaseInvoiceTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Linq;
    using Configuration.Derivations.Default;
    using Database.Derivations;
    using Meta;
    using Resources;
    using Xunit;
    using DateTime = System.DateTime;

    public class RepeatingPurchaseInvoiceTests : DomainTest, IClassFixture<Fixture>
    {
        public RepeatingPurchaseInvoiceTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedFrequencyThrowValidationError()
        {
            var repeatingInvoice = new RepeatingPurchaseInvoiceBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Hour)
                .Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.FrequencyNotSupported));
        }

        [Fact]
        public void ChangedDayOfWeekThrowValidationErrorAssertExistsDayOfWeek()
        {
            var repeatingInvoice = new RepeatingPurchaseInvoiceBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Week)
                .WithDayOfWeek(new DaysOfWeek(this.Transaction).Monday)
                .Build();
            this.Derive();

            repeatingInvoice.RemoveDayOfWeek();

            var errors = this.Derive().Errors.OfType<IDerivationErrorRequired>();
            Assert.Contains(this.M.RepeatingPurchaseInvoice.DayOfWeek, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedDayOfWeekThrowValidationErrorAssertnotExistsDayOfWeek()
        {
            var repeatingInvoice = new RepeatingPurchaseInvoiceBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Month)
                .Build();
            this.Derive();

            repeatingInvoice.DayOfWeek = new DaysOfWeek(this.Transaction).Monday;

            var errors = this.Derive().Errors.OfType<DerivationErrorNotAllowed>();
            Assert.Equal(new IRoleType[]
            {
                this.M.RepeatingPurchaseInvoice.DayOfWeek,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedNextExecutionDateThrowValidationError()
        {
            var repeatingInvoice = new RepeatingPurchaseInvoiceBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Week)
                .WithDayOfWeek(new DaysOfWeek(this.Transaction).Monday)
                .Build();
            this.Derive();

            repeatingInvoice.NextExecutionDate = new DateTime(2021, 01, 06, 12, 0, 0, DateTimeKind.Utc);

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.DateDayOfWeek));
        }
    }
}
