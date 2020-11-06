// <copyright file="WorkEffortAssignmentRate.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using Resources;

    public partial class WorkEffortAssignmentRate
    {
        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistFrequency)
            {
                this.Frequency = new TimeFrequencies(this.Strategy.Session).Hour;
            }
        }

        public void AppsOnPreDerive(ObjectOnPreDerive method)
        {
            var (iteration, changeSet, derivedObjects) = method;

            if (iteration.IsMarked(this) || changeSet.IsCreated(this) || changeSet.HasChangedRoles(this))
            {
                foreach (ServiceEntry serviceEntry in this.WorkEffort.ServiceEntriesWhereWorkEffort)
                {
                    iteration.AddDependency(serviceEntry, this);
                    iteration.Mark(serviceEntry);
                }
            }
        }

        public void AppsDelegateAccess(DelegatedAccessControlledObjectDelegateAccess method)
        {
            if (method.SecurityTokens == null)
            {
                method.SecurityTokens = this.WorkEffort?.SecurityTokens.ToArray();
            }

            if (method.DeniedPermissions == null)
            {
                method.DeniedPermissions = this.WorkEffort?.DeniedPermissions.ToArray();
            }
        }
    }
}
