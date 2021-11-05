// <copyright file="NonUnifiedGoodCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.good.list;

namespace Tests.NonUnifiedGoodTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using libs.workspace.angular.apps.src.lib.objects.part.list;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Product")]
    public class NonUnifiedPartCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly PartListComponent parts;

        public NonUnifiedPartCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.parts = this.Sidenav.NavigateToParts();
        }

        [Fact]
        public void Create()
        {
            var before = new NonUnifiedParts(this.Transaction).Extent().ToArray();

            var internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var expected = new NonUnifiedPartBuilder(this.Transaction).WithNonSerialisedDefaults(internalOrganisation).Build();

            var expectedPart = new NonUnifiedParts(this.Transaction).Extent().FirstOrDefault();

            this.Transaction.Derive();

            var expectedName = expected.Name;
            var expectedInventoryItemKind = expected.InventoryItemKind;
            var expectedDefaultFacility = expected.DefaultFacility;

            var nonUnifiedPartCreate = this.parts.CreateNonUnifiedPart();

            nonUnifiedPartCreate
                .Name.Set(expected.Name)
                .InventoryItemKind.Select(expected.InventoryItemKind)
                .DefaultFacility.Select(expected.DefaultFacility);

            this.Transaction.Rollback();
            nonUnifiedPartCreate.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new NonUnifiedParts(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var good = after.Except(before).First();

            Assert.Equal(expectedName, good.Name);
            Assert.Equal(expectedDefaultFacility, good.DefaultFacility);
            Assert.Equal(expectedInventoryItemKind, good.InventoryItemKind);
        }
    }
}
