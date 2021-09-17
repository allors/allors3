// <copyright file="WorkEffortAssignmentRate.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public partial class WorkEffortAssignmentRate
    {
        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistFrequency)
            {
                this.Frequency = new TimeFrequencies(this.Strategy.Transaction).Hour;
            }
        }

        public void AppsDelegateAccess(DelegatedAccessControlledObjectDelegateAccess method)
        {
            if (method.SecurityTokens == null)
            {
                method.SecurityTokens = this.WorkEffort?.SecurityTokens.ToArray();
            }

            if (method.Restrictions == null)
            {
                method.Restrictions = this.WorkEffort?.DeniedPermissions.ToArray();
            }
        }
    }
}
