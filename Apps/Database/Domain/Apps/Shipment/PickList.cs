// <copyright file="PickList.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace Allors.Database.Domain
{
    public partial class PickList
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.PickList, this.M.PickList.PickListState),
        };

        public bool IsComplete
        {
            get
            {
                foreach (PickListItem pickListItem in this.PickListItems)
                {
                    if (pickListItem.Quantity != pickListItem.QuantityPicked)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistCreationDate)
            {
                this.CreationDate = this.Transaction().Now();
            }

            if (!this.ExistPickListState)
            {
                this.PickListState = new PickListStates(this.Strategy.Transaction).Created;
            }

            if (!this.ExistStore)
            {
                this.Store = this.Strategy.Transaction.Extent<Store>().First;
            }
        }

        public void AppsDelete(PickListDelete method)
        {
            foreach (PickListItem pickListItem in this.PickListItems)
            {
                pickListItem.Delete();
            }

            if (this.ExistShipToParty)
            {
                foreach (Shipment shipment in this.ShipToParty.ShipmentsWhereShipToParty)
                {
                    shipment.DerivationTrigger = Guid.NewGuid();
                }
            }
        }

        public void AppsCancel(PickListCancel method)
        {
            this.PickListState = new PickListStates(this.Strategy.Transaction).Cancelled;
            method.StopPropagation = true;
        }

        public void AppsHold(PickListHold method)
        {
            this.PickListState = new PickListStates(this.Strategy.Transaction).OnHold;
            method.StopPropagation = true;
        }

        public void AppsContinue(PickListContinue method)
        {
            this.PickListState = this.PreviousPickListState;
            method.StopPropagation = true;
        }

        public void AppsSetPicked(PickListSetPicked method)
        {
            this.PickListState = new PickListStates(this.Strategy.Transaction).Picked;
            method.StopPropagation = true;
        }
    }
}
