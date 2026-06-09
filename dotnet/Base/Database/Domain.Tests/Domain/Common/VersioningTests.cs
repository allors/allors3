// <copyright file="VersioningTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the ApplicationTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Domain;
    using Xunit;

    public class VersioningTests : DomainTest, IClassFixture<Fixture>
    {
        public VersioningTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void InitialNothing()
        {
            var order = new OrderBuilder(this.Transaction).Build();

            this.Transaction.Derive();

            Assert.True(order.ExistCurrentVersion);
            Assert.True(order.ExistAllVersions);
            Assert.Single(order.AllVersions);

            var version = order.CurrentVersion;

            Assert.Equal(order.Amount, version.Amount);
        }

        [Fact]
        public void VersionedUnitRole()
        {
            var order = new OrderBuilder(this.Transaction)
                .WithAmount(10m)
                .Build();

            this.Transaction.Derive();

            Assert.True(order.ExistCurrentVersion);
            Assert.True(order.ExistAllVersions);
            Assert.Single(order.AllVersions);

            var version = order.CurrentVersion;

            Assert.Equal(10m, version.Amount);
            Assert.False(version.ExistOrderState);
            Assert.False(version.ExistOrderLines);
        }

        [Fact]
        public void NonVersionedUnitRole()
        {
            var order = new OrderBuilder(this.Transaction)
                .WithAmount(10m)
                .Build();

            this.Transaction.Derive();

            var currentVersion = order.CurrentVersion;

            order.NonVersionedAmount = 20m;

            this.Transaction.Derive();

            Assert.True(order.ExistAllVersions);
            Assert.Single(order.AllVersions);
            Assert.Equal(currentVersion, order.CurrentVersion);
        }

        [Fact]
        public void InitialCompositeRole()
        {
            var initialObjectState = new OrderStates(this.Transaction).Initial;

            var order = new OrderBuilder(this.Transaction)
                .WithOrderState(initialObjectState)
                .Build();

            this.Transaction.Derive();

            Assert.True(order.ExistCurrentVersion);
            Assert.True(order.ExistAllVersions);
            Assert.Single(order.AllVersions);

            var version = order.CurrentVersion;

            Assert.False(version.ExistAmount);
            Assert.Equal(initialObjectState, version.OrderState);
            Assert.False(version.ExistOrderLines);
        }

        [Fact]
        public void InitialCompositeRoles()
        {
            var orderLine = new OrderLineBuilder(this.Transaction).Build();

            var order = new OrderBuilder(this.Transaction)
                .WithOrderLine(orderLine)
                .Build();

            this.Transaction.Derive();

            Assert.True(order.ExistCurrentVersion);
            Assert.True(order.ExistAllVersions);
            Assert.Single(order.AllVersions);

            var version = order.CurrentVersion;

            Assert.False(version.ExistAmount);
            Assert.False(version.ExistOrderState);
            Assert.Single(version.OrderLines);
            Assert.Equal(orderLine, version.OrderLines.ElementAt(0));
        }

        [Fact]
        public void VersionedCompositesRoleUnchanged()
        {
            // Two lines so the equal-count comparison runs OrderBy over more than one element
            // (a single element never invokes the comparer).
            var orderLine1 = new OrderLineBuilder(this.Transaction).Build();
            var orderLine2 = new OrderLineBuilder(this.Transaction).Build();

            var order = new OrderBuilder(this.Transaction)
                .WithOrderLine(orderLine1)
                .WithOrderLine(orderLine2)
                .Build();

            this.Transaction.Derive();

            var currentVersion = order.CurrentVersion;
            Assert.Single(order.AllVersions);

            // Force a re-derivation without changing any versioned role.
            order.NonVersionedAmount = 20m;

            this.Transaction.Derive();

            Assert.Single(order.AllVersions);
            Assert.Equal(currentVersion, order.CurrentVersion);
        }

        [Fact]
        public void VersionedCompositesRoleChanged()
        {
            var orderLine = new OrderLineBuilder(this.Transaction).Build();

            var order = new OrderBuilder(this.Transaction)
                .WithOrderLine(orderLine)
                .Build();

            this.Transaction.Derive();

            var currentVersion = order.CurrentVersion;
            Assert.Single(order.AllVersions);

            var anotherOrderLine = new OrderLineBuilder(this.Transaction).Build();
            order.AddOrderLine(anotherOrderLine);

            this.Transaction.Derive();

            Assert.Equal(2, order.AllVersions.Count());
            Assert.NotEqual(currentVersion, order.CurrentVersion);
        }

        // --- versioned unit role (Amount) ---

        [Fact]
        public void ChangedVersionedUnitRoleCreatesNewVersion()
        {
            var order = new OrderBuilder(this.Transaction).WithAmount(10m).Build();
            this.Transaction.Derive();

            var currentVersion = order.CurrentVersion;
            Assert.Single(order.AllVersions);

            order.Amount = 20m;
            this.Transaction.Derive();

            Assert.Equal(2, order.AllVersions.Count());
            Assert.NotEqual(currentVersion, order.CurrentVersion);
            Assert.Equal(20m, order.CurrentVersion.Amount);
        }

        [Fact]
        public void UnchangedVersionedUnitRoleDoesNotCreateNewVersion()
        {
            var order = new OrderBuilder(this.Transaction).WithAmount(10m).Build();
            this.Transaction.Derive();

            var currentVersion = order.CurrentVersion;
            Assert.Single(order.AllVersions);

            // Re-derive without changing the versioned Amount.
            order.NonVersionedAmount = 99m;
            this.Transaction.Derive();

            Assert.Single(order.AllVersions);
            Assert.Equal(currentVersion, order.CurrentVersion);
        }

        // --- versioned single composite role (OrderState) ---

        [Fact]
        public void ChangedVersionedSingleCompositeRoleCreatesNewVersion()
        {
            var order = new OrderBuilder(this.Transaction).Build();
            this.Transaction.Derive();

            var currentVersion = order.CurrentVersion;
            Assert.Single(order.AllVersions);

            var initial = new OrderStates(this.Transaction).Initial;
            order.OrderState = initial;
            this.Transaction.Derive();

            Assert.Equal(2, order.AllVersions.Count());
            Assert.NotEqual(currentVersion, order.CurrentVersion);
            Assert.Equal(initial, order.CurrentVersion.OrderState);
        }

        [Fact]
        public void UnchangedVersionedSingleCompositeRoleDoesNotCreateNewVersion()
        {
            var order = new OrderBuilder(this.Transaction)
                .WithOrderState(new OrderStates(this.Transaction).Initial)
                .Build();
            this.Transaction.Derive();

            var currentVersion = order.CurrentVersion;
            Assert.Single(order.AllVersions);

            // Re-derive without changing the versioned OrderState (mutate a non-versioned composite role).
            order.NonVersionedCurrentObjectState = new OrderStates(this.Transaction).Initial;
            this.Transaction.Derive();

            Assert.Single(order.AllVersions);
            Assert.Equal(currentVersion, order.CurrentVersion);
        }

        // --- versioned many composite role (OrderLines) ---

        [Fact]
        public void AddedCompositesRoleElementCreatesNewVersion()
        {
            var orderLine1 = new OrderLineBuilder(this.Transaction).Build();
            var order = new OrderBuilder(this.Transaction).WithOrderLine(orderLine1).Build();
            this.Transaction.Derive();

            Assert.Single(order.AllVersions);

            var orderLine2 = new OrderLineBuilder(this.Transaction).Build();
            order.AddOrderLine(orderLine2);
            this.Transaction.Derive();

            Assert.Equal(2, order.AllVersions.Count());
            Assert.Equal(2, order.CurrentVersion.OrderLines.Count());
        }

        [Fact]
        public void RemovedCompositesRoleElementCreatesNewVersion()
        {
            var orderLine1 = new OrderLineBuilder(this.Transaction).Build();
            var orderLine2 = new OrderLineBuilder(this.Transaction).Build();
            var order = new OrderBuilder(this.Transaction)
                .WithOrderLine(orderLine1)
                .WithOrderLine(orderLine2)
                .Build();
            this.Transaction.Derive();

            Assert.Single(order.AllVersions);

            order.RemoveOrderLine(orderLine1);
            this.Transaction.Derive();

            Assert.Equal(2, order.AllVersions.Count());
            Assert.Single(order.CurrentVersion.OrderLines);
            Assert.Equal(orderLine2, order.CurrentVersion.OrderLines.ElementAt(0));
        }

        [Fact]
        public void ClearedCompositesRoleCreatesNewVersion()
        {
            var orderLine = new OrderLineBuilder(this.Transaction).Build();
            var order = new OrderBuilder(this.Transaction).WithOrderLine(orderLine).Build();
            this.Transaction.Derive();

            Assert.Single(order.AllVersions);

            order.RemoveOrderLines();
            this.Transaction.Derive();

            Assert.Equal(2, order.AllVersions.Count());
            Assert.False(order.CurrentVersion.ExistOrderLines);
        }

        [Fact]
        public void PopulatedEmptyCompositesRoleCreatesNewVersion()
        {
            var order = new OrderBuilder(this.Transaction).Build();
            this.Transaction.Derive();

            Assert.Single(order.AllVersions);

            var orderLine = new OrderLineBuilder(this.Transaction).Build();
            order.AddOrderLine(orderLine);
            this.Transaction.Derive();

            Assert.Equal(2, order.AllVersions.Count());
            Assert.Single(order.CurrentVersion.OrderLines);
        }

        [Fact]
        public void ReassignedSameCompositesRoleSetDoesNotCreateNewVersion()
        {
            var orderLine1 = new OrderLineBuilder(this.Transaction).Build();
            var orderLine2 = new OrderLineBuilder(this.Transaction).Build();
            var order = new OrderBuilder(this.Transaction)
                .WithOrderLine(orderLine1)
                .WithOrderLine(orderLine2)
                .Build();
            this.Transaction.Derive();

            var currentVersion = order.CurrentVersion;
            Assert.Single(order.AllVersions);

            // Re-assign the same set in a different order, and force a re-derive: the change detection
            // orders by Id, so an unchanged set must not bump the version.
            order.RemoveOrderLines();
            order.AddOrderLine(orderLine2);
            order.AddOrderLine(orderLine1);
            order.NonVersionedCurrentObjectState = new OrderStates(this.Transaction).Initial;
            this.Transaction.Derive();

            Assert.Single(order.AllVersions);
            Assert.Equal(currentVersion, order.CurrentVersion);
        }

        // --- history / snapshot content ---

        [Fact]
        public void VersionSnapshotCapturesValueAtEachDerivation()
        {
            var order = new OrderBuilder(this.Transaction).WithAmount(10m).Build();
            this.Transaction.Derive();

            var firstVersion = order.CurrentVersion;

            order.Amount = 20m;
            this.Transaction.Derive();

            var secondVersion = order.CurrentVersion;

            Assert.Equal(2, order.AllVersions.Count());
            Assert.NotEqual(firstVersion, secondVersion);
            Assert.Equal(10m, firstVersion.Amount);   // history: the first version keeps the old value
            Assert.Equal(20m, secondVersion.Amount);
        }

        [Fact]
        public void SequentialChangesCreateSequentialVersions()
        {
            var order = new OrderBuilder(this.Transaction).WithAmount(1m).Build();
            this.Transaction.Derive();
            Assert.Single(order.AllVersions);

            order.Amount = 2m;
            this.Transaction.Derive();
            Assert.Equal(2, order.AllVersions.Count());

            order.Amount = 3m;
            this.Transaction.Derive();
            Assert.Equal(3, order.AllVersions.Count());
            Assert.Equal(3m, order.CurrentVersion.Amount);
        }

        [Fact]
        public void ChangedCompositesRoleSnapshotReflectsEachVersion()
        {
            var orderLine1 = new OrderLineBuilder(this.Transaction).Build();
            var order = new OrderBuilder(this.Transaction).WithOrderLine(orderLine1).Build();
            this.Transaction.Derive();

            var firstVersion = order.CurrentVersion;

            var orderLine2 = new OrderLineBuilder(this.Transaction).Build();
            order.AddOrderLine(orderLine2);
            this.Transaction.Derive();

            var secondVersion = order.CurrentVersion;

            Assert.NotEqual(firstVersion, secondVersion);
            Assert.Single(firstVersion.OrderLines);   // history: the first version still snapshots only line1
            Assert.Equal(orderLine1, firstVersion.OrderLines.ElementAt(0));
            Assert.Equal(2, secondVersion.OrderLines.Count());
        }

        // --- multiple versioned roles changing together ---

        [Fact]
        public void MultipleVersionedRolesChangedCreateSingleNewVersion()
        {
            var orderLine1 = new OrderLineBuilder(this.Transaction).Build();
            var order = new OrderBuilder(this.Transaction)
                .WithAmount(10m)
                .WithOrderLine(orderLine1)
                .Build();
            this.Transaction.Derive();

            Assert.Single(order.AllVersions);

            // Change three versioned roles in one derivation cycle.
            var initial = new OrderStates(this.Transaction).Initial;
            order.Amount = 20m;
            order.OrderState = initial;
            var orderLine2 = new OrderLineBuilder(this.Transaction).Build();
            order.AddOrderLine(orderLine2);
            this.Transaction.Derive();

            // Exactly one new version, capturing all of the new values.
            Assert.Equal(2, order.AllVersions.Count());
            Assert.Equal(20m, order.CurrentVersion.Amount);
            Assert.Equal(initial, order.CurrentVersion.OrderState);
            Assert.Equal(2, order.CurrentVersion.OrderLines.Count());
        }

        // --- child object (OrderLine) is independently versioned ---

        [Fact]
        public void OrderLineIsIndependentlyVersioned()
        {
            var orderLine = new OrderLineBuilder(this.Transaction).WithAmount(5m).Build();
            var order = new OrderBuilder(this.Transaction).WithOrderLine(orderLine).Build();
            this.Transaction.Derive();

            Assert.Single(orderLine.AllVersions);

            orderLine.Amount = 10m;
            this.Transaction.Derive();

            Assert.Equal(2, orderLine.AllVersions.Count());
            Assert.Equal(10m, orderLine.CurrentVersion.Amount);
        }

        // --- non-versioned roles must not create versions ---

        [Fact]
        public void NonVersionedSingleCompositeRoleDoesNotCreateNewVersion()
        {
            var order = new OrderBuilder(this.Transaction).Build();
            this.Transaction.Derive();

            var currentVersion = order.CurrentVersion;
            Assert.Single(order.AllVersions);

            order.NonVersionedCurrentObjectState = new OrderStates(this.Transaction).Initial;
            this.Transaction.Derive();

            Assert.Single(order.AllVersions);
            Assert.Equal(currentVersion, order.CurrentVersion);
        }

        [Fact]
        public void NonVersionedManyRoleDoesNotCreateNewVersion()
        {
            var order = new OrderBuilder(this.Transaction).Build();
            this.Transaction.Derive();

            var currentVersion = order.CurrentVersion;
            Assert.Single(order.AllVersions);

            var orderLine = new OrderLineBuilder(this.Transaction).Build();
            order.AddNonVersionedOrderLine(orderLine);
            this.Transaction.Derive();

            Assert.Single(order.AllVersions);
            Assert.Equal(currentVersion, order.CurrentVersion);
        }
    }
}
