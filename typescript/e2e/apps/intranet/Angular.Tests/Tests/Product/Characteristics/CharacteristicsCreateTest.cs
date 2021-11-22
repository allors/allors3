// <copyright file="CustomerShipmentCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.customershipment.create;
using libs.workspace.angular.apps.src.lib.objects.shipment.list;

namespace Tests.CatalogueTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using libs.workspace.angular.apps.src.lib.objects.serialiseditemcharacteristictype.list;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Product")]
    public class CharacteristicsCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly SerialisedItemCharacteristicListComponent serialisedItemCharacteristicList;

        public CharacteristicsCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.serialisedItemCharacteristicList = this.Sidenav.NavigateToCharacteristics();
        }

        [Fact]
        public void Create()
        {
            var before = new SerialisedItemCharacteristicTypes(this.Transaction).Extent().ToArray();

            var expected = new SerialisedItemCharacteristicTypeBuilder(this.Transaction).WithDefaults().Build();

            this.Transaction.Derive();

            var expectedName = expected.Name;
            var expectedUnitOfMeasure = expected.UnitOfMeasure;

            var serialItemCharacteristicListComponent = serialisedItemCharacteristicList.CreateSerialisedItemCharacteristicType();

            serialItemCharacteristicListComponent
                .Name.Set(expected.Name)
                .UnitOfMeasure.Select(expected.UnitOfMeasure)
                .IsActive.Set(expected.IsActive.Value)
                .IsPublic.Set(expected.IsPublic);

            this.Transaction.Rollback();
            serialItemCharacteristicListComponent.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SerialisedItemCharacteristicTypes(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedName, actual.Name);
            Assert.Equal(expectedUnitOfMeasure, actual.UnitOfMeasure);
        }
    }
}
