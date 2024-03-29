// <copyright file="NamedConstantTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the ApplicationTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class EnumerationTest : DomainTest, IClassFixture<Fixture>
    {
        public EnumerationTest(Fixture fixture) : base(fixture) { }

        // TODO: Create tests

        // [Fact]
        // public void GivenNamedConstant_WhenRetrievingConstant_ThenLocalDescriptionIsOfCorrectLocale()
        // {
        //    var poor = new SkillRatings(this.DatabaseSession).Poor;

        // var dutch = new Locales(this.DatabaseSession).LocaleByName["nl"];

        // Assert.Equal("Poor", poor.GetName());
        //    Assert.Equal("Slecht", poor.GetNameWithLocale(dutch));
        // }

        // [Fact]
        // public void GivenContactMechanismType_WhenRetrievingConstant_ThenLocalDescriptionIsOfCorrectLocale()
        // {
        //    var billingInquiries = new ContactMechanismPurposes(this.DatabaseSession).BillingInquiriesPhone;

        // var dutch = new Locales(this.DatabaseSession).LocaleByName["nl"];

        // Assert.Equal("Billing Inquiries phone", billingInquiries.GetName());
        //    Assert.Equal("Facturatie vragen telefoon", billingInquiries.GetNameWithLocale(dutch));
        // }
    }
}
