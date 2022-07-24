// <copyright file="WorkTaskBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
using System;
using System.Linq;

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class WorkEffortInventoryAssignmentBuilderExtensions
    {
        public static WorkEffortInventoryAssignmentBuilder WithDefaults(this WorkEffortInventoryAssignmentBuilder @this, Organisation internalOrganisation, WorkEffort workEffort)
        {
            var faker = @this.Transaction.Faker();

            var items = new NonSerialisedInventoryItems(@this.Transaction).Extent()
                .Where(v => v.Facility.Owner.Id == internalOrganisation.Id && v.QuantityOnHand > 0);
            var item = faker.PickRandom(items);

            @this.WithQuantity(Math.Round(faker.Random.Decimal(1M, item.QuantityOnHand), 2))
                .WithInventoryItem(item)
                .WithAssignment(workEffort);

            return @this;
        }
    }
}
