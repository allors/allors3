// <copyright file="WorkTaskBuilderExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
namespace Allors.Database.Domain.TestPopulation
{
    public static partial class WorkEffortFixedAssetAssignmentBuilderExtensions
    {
        public static WorkEffortFixedAssetAssignmentBuilder WithDefaults(this WorkEffortFixedAssetAssignmentBuilder @this, Party customer, WorkEffort workEffort)
        {
            var faker = @this.Transaction.Faker();

            var serialisedItems = customer.SerialisedItemsWhereOwnedBy;

            @this.WithFromDate(DateTimeFactory.CreateDateTime(2021, 11, 5, 12, 0, 0, 0))
                 .WithFixedAsset(faker.PickRandom(serialisedItems))
                 .WithComment(faker.Random.Words(10))
                 .WithAssignment(workEffort);

            return @this;
        }
    }
}
