// <copyright file="Transfer.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class Transfer
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.Transfer, this.M.Transfer.ShipmentState),
        };

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistShipmentState)
            {
                this.ShipmentState = new ShipmentStates(this.Strategy.Transaction).Created;
            }
        }

        public void AppsDelete(DeletableDelete method)
        {
            foreach (var deletable in this.AllVersions)
            {
                deletable.Strategy.Delete();
            }
        }
    }
}
