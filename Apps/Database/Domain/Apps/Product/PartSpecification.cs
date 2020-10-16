// <copyright file="PartSpecification.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public partial class PartSpecification
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.PartSpecification, this.M.PartSpecification.PartSpecificationState),
        };

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistPartSpecificationState)
            {
                this.PartSpecificationState = new PartSpecificationStates(this.Strategy.Session).Created;
            }
        }

        public void AppsApprove(PartSpecificationApprove method) => this.PartSpecificationState = new PartSpecificationStates(this.Strategy.Session).Approved;
    }
}
