// <copyright file="EmailCommunicationTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class EmailTemplateTests : DomainTest, IClassFixture<Fixture>
    {
        public EmailTemplateTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void WhenEmailTemplateDescriptionIsNull()
        {
            var emailTemplate = new EmailTemplateBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal("Default", emailTemplate.Description);
        }
    }
}
