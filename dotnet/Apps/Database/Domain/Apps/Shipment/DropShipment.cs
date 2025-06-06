// <copyright file="DropShipment.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public partial class DropShipment
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.DropShipment, this.M.DropShipment.ShipmentState),
        };

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistEstimatedShipDate)
            {
                this.EstimatedShipDate = this.Transaction().Now().Date;
            }
        }

        public void AppsOnInit(ObjectOnInit method)
        {
            if (!this.ExistShipmentState)
            {
                this.ShipmentState = new ShipmentStates(this.Strategy.Transaction).Created;

                if (!this.ExistShipFromParty)
                {
                    var internalOrganisations = new Organisations(this.Strategy.Transaction).InternalOrganisations();
                    if (internalOrganisations.Count() == 1)
                    {
                        this.ShipFromParty = internalOrganisations.First();
                    }
                }

                if (!this.ExistStore && this.ExistShipFromParty)
                {
                    var stores = new Stores(this.Strategy.Transaction).Extent();
                    stores.Filter.AddEquals(this.M.Store.InternalOrganisation, this.ShipFromParty);

                    if (stores.Any())
                    {
                        this.Store = stores.First;
                    }
                }

                if (!this.ExistCarrier && this.ExistStore)
                {
                    this.Carrier = this.Store.DefaultCarrier;
                }
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
