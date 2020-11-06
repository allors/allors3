// <copyright file="PickList.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
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

        public void AppsDelete(PickListDelete method)
        {
            foreach (PickListItem pickListItem in this.PickListItems)
            {
                pickListItem.Delete();
            }
        }

        public void AppsCancel(PickListCancel method) => this.PickListState = new PickListStates(this.Strategy.Session).Cancelled;

        public void AppsHold(PickListHold method) => this.PickListState = new PickListStates(this.Strategy.Session).OnHold;

        public void AppsContinue(PickListContinue method) => this.PickListState = this.PreviousPickListState;

        public void AppsSetPicked(PickListSetPicked method)
        {
            foreach (PickListItem pickListItem in this.PickListItems)
            {
                if (pickListItem.QuantityPicked == 0)
                {
                    pickListItem.QuantityPicked = pickListItem.Quantity;
                }
            }

            this.PickListState = new PickListStates(this.Strategy.Session).Picked;
        }
    }
}
