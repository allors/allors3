// <copyright file="PickLists.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class PickLists
    {
        public Extent<PickList> PendingPickLists
        {
            get
            {
                var pickLists = new PickLists(this.Transaction).Extent();
                pickLists.Filter.AddNot().AddExists(this.M.PickList.Picker);
                pickLists.Filter.AddEquals(this.M.PickList.PickListState, new PickListStates(this.Transaction).Created);

                return pickLists;
            }
        }

        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.PickListState);

        protected override void AppsSecure(Security config)
        {
            var created = new PickListStates(this.Transaction).Created;
            var onHold = new PickListStates(this.Transaction).OnHold;
            var picked = new PickListStates(this.Transaction).Picked;
            var cancelled = new PickListStates(this.Transaction).Cancelled;

            config.Deny(this.ObjectType, created, this.Meta.Continue);
            config.Deny(this.ObjectType, onHold, this.Meta.Hold);

            config.Deny(this.ObjectType, picked, Operations.Execute, Operations.Write);
            config.Deny(this.ObjectType, cancelled, Operations.Execute, Operations.Write);
        }
    }
}
