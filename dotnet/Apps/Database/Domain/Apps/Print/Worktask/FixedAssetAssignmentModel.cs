// <copyright file="FixedAssetAssignmentModel.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Print.WorkTaskModel
{
    using System.Linq;
    using Meta;
    using SerialisedItem = SerialisedItem;
    using WorkEffortFixedAssetAssignment = WorkEffortFixedAssetAssignment;

    public class FixedAssetAssignmentModel
    {
        public FixedAssetAssignmentModel(WorkEffortFixedAssetAssignment assignment)
        {
            var transaction = assignment.Strategy.Transaction;
            var m = transaction.Database.Services.Get<MetaPopulation>();

            this.Name = assignment.FixedAsset?.DisplayName;
            this.Comment = assignment.Comment?.Split('\n');

            if (assignment.FixedAsset is SerialisedItem serialisedItem)
            {
                this.CustomerReferenceNumber = serialisedItem.CustomerReferenceNumber;
                this.ItemNumber = serialisedItem.ItemNumber;
                this.SerialNumber = serialisedItem.SerialNumber;
                this.Brand = serialisedItem.PartWhereSerialisedItem?.Brand?.Name;
                this.Model = serialisedItem.PartWhereSerialisedItem?.Model?.Name;

                var hoursType = new SerialisedItemCharacteristicTypes(transaction).FindBy(m.SerialisedItemCharacteristicType.Name, "Operating Hours");
                var hoursCharacteristic = serialisedItem.SerialisedItemCharacteristics.FirstOrDefault(v => v.SerialisedItemCharacteristicType.Equals(hoursType));
                this.Hours = $"{hoursCharacteristic?.Value} {hoursType?.UnitOfMeasure?.Abbreviation}";
            }
        }

        public string Name { get; }

        public string CustomerReferenceNumber { get; }

        public string SerialNumber { get; }

        public string ItemNumber { get; }

        public string Brand { get; }

        public string Model { get; }

        public string Hours { get; }

        public string[] Comment { get; }
    }
}
