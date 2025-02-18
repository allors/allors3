// <copyright file="WorkEffortExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.TestPopulation
{
    using System;
    using System.Linq;
    using Domain;

    public static partial class WorkEffortExtensions
    {
        public static WorkEffortInventoryAssignment CreateInventoryAssignment(this WorkEffort @this, Part part, int quantity)
        {
            new InventoryItemTransactionBuilder(@this.Transaction())
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(@this.Transaction()).IncomingShipment)
                .WithQuantity(quantity)
                .Build();

            @this.Transaction().Derive();

            return new WorkEffortInventoryAssignmentBuilder(@this.Transaction())
                .WithAssignment(@this)
                .WithInventoryItem(part.InventoryItemsWherePart.FirstOrDefault())
                .WithQuantity(quantity)
                .Build();
        }

        public static TimeEntry CreateTimeEntry(this WorkEffort @this, DateTime fromDate, DateTime throughDate, TimeFrequency frequency, RateType rateType) =>
            new TimeEntryBuilder(@this.Transaction())
                .WithRateType(rateType)
                .WithFromDate(fromDate)
                .WithThroughDate(throughDate)
                .WithTimeFrequency(frequency)
                .WithWorkEffort(@this)
                .Build();
    }
}
