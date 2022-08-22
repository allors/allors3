// <copyright file="PersonTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    [Trait("Category", "Security")]
    public class ModelDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public ModelDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deleteRevocation = new Revocations(this.Transaction).ModelDeleteRevocation;

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Revocation deleteRevocation;

        [Fact]
        public void OnChangedModelDeriveDeletePermission()
        {
            var model = new ModelBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, model.Revocations);
        }

        [Fact]
        public void OnChangedBrandModelsDeriveDeletePermission()
        {
            var model = new ModelBuilder(this.Transaction).Build();
            this.Derive();

            var brand = new BrandBuilder(this.Transaction).Build();
            this.Derive();

            brand.AddModel(model);
            this.Derive();

            Assert.Contains(this.deleteRevocation, model.Revocations);
        }
    }
}
