// <copyright file="Requirement.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;

namespace Allors.Database.Domain
{
    public partial class WorkRequirement
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.WorkRequirement, this.M.WorkRequirement.RequirementState),
        };
        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistRequirementState)
            {
                this.RequirementState = new RequirementStates(this.Strategy.Transaction).Created;
            }
        }

        public void AppsOnInit(ObjectOnInit method)
        {
            var internalOrganisations = new Organisations(this.Strategy.Transaction).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

            if (!this.ExistServicedBy && internalOrganisations.Length == 1)
            {
                this.ServicedBy = internalOrganisations[0];
            }
        }

        public void AppsDelete(DeletableDelete method)
        {
            foreach (var deletable in this.AllVersions)
            {
                deletable.Delete();
            }
        }

        public void AppsCancel(WorkRequirementCancel method)
        {
            this.RequirementState = new RequirementStates(this.Strategy.Transaction).Cancelled;
            method.StopPropagation = true;
        }

        public void AppsReopen(WorkRequirementReopen method)
        {
            this.RequirementState = new RequirementStates(this.Strategy.Transaction).Created;
            method.StopPropagation = true;
        }

        public void AppsClose(WorkRequirementClose method)
        {
            this.RequirementState = new RequirementStates(this.Strategy.Transaction).Finished;
            method.StopPropagation = true;
        }

        public void AppsCreateWorkTask(WorkRequirementCreateWorkTask method)
        {
            var transaction = this.Strategy.Transaction;

            var workTask = new WorkTaskBuilder(transaction)
                .WithName(this.Description)
                .WithDescription(this.Reason)
                .WithTakenBy(this.ServicedBy)
                .WithCustomer(this.Originator)
                .Build();

            new WorkEffortFixedAssetAssignmentBuilder(transaction).WithAssignment(workTask).WithFixedAsset(this.FixedAsset).Build();
            new WorkRequirementFulfillmentBuilder(transaction).WithFullfilledBy(this).WithFullfillmentOf(workTask).Build();

            method.StopPropagation = true;
        }
    }
}
