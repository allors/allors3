// <copyright file="DropShipment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;

namespace Allors.Database.Domain
{
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
                this.EstimatedShipDate = this.Session().Now().Date;
            }
        }

        public void AppsOnInit(ObjectOnInit method)
        {
            if (!this.ExistShipmentState)
            {
                this.ShipmentState = new ShipmentStates(this.Strategy.Session).Created;

                if (!this.ExistShipFromParty)
                {
                    var internalOrganisations = new Organisations(this.Strategy.Session).InternalOrganisations();
                    if (internalOrganisations.Count() == 1)
                    {
                        this.ShipFromParty = internalOrganisations.First();
                    }
                }

                if (!this.ExistStore && this.ExistShipFromParty)
                {
                    var stores = new Stores(this.Strategy.Session).Extent();
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
    }
}
