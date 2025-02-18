// <copyright file="TemplateTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using Domain;
    using Xunit;

    public class TemplateTests : DomainTest, IClassFixture<Fixture>
    {
        public TemplateTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void Render()
        {
            var media = new MediaBuilder(this.Transaction).WithInData(this.GetResourceBytes("Domain.Tests.Resources.EmbeddedTemplate.odt")).Build();
            var templateType = new TemplateTypes(this.Transaction).OpenDocumentType;
            var template = new TemplateBuilder(this.Transaction).WithMedia(media).WithTemplateType(templateType).WithArguments("logo, people").Build();

            this.Transaction.Derive();

            var people = new People(this.Transaction).Extent();
            var logo = this.GetResourceBytes("Domain.Tests.Resources.logo.png");

            var data = new Dictionary<string, object>
            {
                               { "people", people },
                           };

            var images = new Dictionary<string, byte[]>
            {
                                { "logo", logo },
                            };

            var result = template.Render(data, images);

            File.WriteAllBytes("Embedded.odt", result);

            Assert.NotNull(result);
            Assert.NotEmpty(result);

            result = template.Render(data, images);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }
}
