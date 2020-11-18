// <copyright file="PartTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class PartTests : DomainTest, IClassFixture<Fixture>
    {
        public PartTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPart_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            var finishedGood = new NonUnifiedPartBuilder(this.Session)
                .WithProductIdentification(new PartNumberBuilder(this.Session)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Part).Build())
                .WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised)
                .Build();

            Assert.Equal(new InventoryItemKinds(this.Session).NonSerialised, finishedGood.InventoryItemKind);
        }

        [Fact]
        public void GivenNewPart_WhenDeriving_ThenInventoryItemIsCreated()
        {
            var finishedGood = new NonUnifiedPartBuilder(this.Session)
                .WithProductIdentification(new PartNumberBuilder(this.Session)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Part).Build())
                .WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised)
                .Build();

            this.Session.Derive();

            Assert.Single(finishedGood.InventoryItemsWherePart);
            Assert.Equal(new Facilities(this.Session).FindBy(this.M.Facility.FacilityType, new FacilityTypes(this.Session).Warehouse), finishedGood.InventoryItemsWherePart.First.Facility);
        }
    }

    [Trait("Category", "Security")]
    public class NonUnifiedPartSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public NonUnifiedPartSecurityTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Session).Get(this.M.NonUnifiedPart.ObjectType, this.M.NonUnifiedPart.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedNonUnifiedPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }


        [Fact]
        public void OnChangedNonUnifiedPartWithWorkEffortInventoryProducedDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            var workEffortInventoryProduceds = new WorkEffortInventoryProducedBuilder(this.Session).WithPart(nonUnifiedPart).Build();
            this.Session.Derive(false);
          
            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedPartWithWorkEffortPartStandardDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            var workEffortPartStandards = new WorkEffortPartStandardBuilder(this.Session).WithPart(nonUnifiedPart).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedPartWithPartBillOfMaterialDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            var engineeringBoms = new EngineeringBomBuilder(this.Session).WithPart(nonUnifiedPart).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }


        [Fact]
        public void OnChangedNonUnifiedPartWithPartBillOfMaterialWithComponentDeriveDeletePermission()
        {

            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            var engineeringBoms = new EngineeringBomBuilder(this.Session).WithComponentPart(nonUnifiedPart).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedPartWithInventoryItemTransactionDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            var inventoryItemTransactions = new InventoryItemTransactionBuilder(this.Session).WithReason(new InventoryTransactionReasonBuilder(this.Session).Build()).WithPart(nonUnifiedPart).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

    }
}
