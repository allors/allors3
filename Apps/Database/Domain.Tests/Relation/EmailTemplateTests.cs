// <copyright file="EmailCommunicationTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Domain
{
    using Xunit;

    public class EmailTemplateTests : DomainTest, IClassFixture<Fixture>
    {
        public EmailTemplateTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void WhenEmailTemplateDescriptionIsNull()
        {
            var emailTemplate = new EmailTemplateBuilder(this.Session).Build();

            this.Session.Derive();

            Assert.Equal("Default", emailTemplate.Description);
        }
    }
}
