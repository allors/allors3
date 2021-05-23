// <copyright file="SupplierRelationship.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public partial class SupplierRelationship
    {
        public void AppsOnInit(ObjectOnInit method)
        {
            var internalOrganisations = new Organisations(this.Strategy.Transaction).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

            if (!this.ExistInternalOrganisation && internalOrganisations.Count() == 1)
            {
                this.InternalOrganisation = internalOrganisations.First();
            }

            if (!this.ExistNeedsApproval)
            {
                this.NeedsApproval = this.InternalOrganisation.PurchaseOrderNeedsApproval;
            }

            if (!this.ApprovalThresholdLevel1.HasValue)
            {
                this.ApprovalThresholdLevel1 = this.InternalOrganisation.PurchaseOrderApprovalThresholdLevel1;
            }

            if (!this.ApprovalThresholdLevel2.HasValue)
            {
                this.ApprovalThresholdLevel2 = this.InternalOrganisation.PurchaseOrderApprovalThresholdLevel2;
            }
        }
    }
}
