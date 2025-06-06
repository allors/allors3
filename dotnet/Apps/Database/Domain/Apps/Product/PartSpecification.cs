// <copyright file="PartSpecification.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
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
                this.PartSpecificationState = new PartSpecificationStates(this.Strategy.Transaction).Created;
            }
        }

        public void AppsDelete(DeletableDelete method)
        {
            foreach (var deletable in this.AllVersions)
            {
                deletable.Strategy.Delete();
            }
        }

        public void AppsApprove(PartSpecificationApprove method)
        {
            this.PartSpecificationState = new PartSpecificationStates(this.Strategy.Transaction).Approved;
            method.StopPropagation = true;
        }
    }
}
