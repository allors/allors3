// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class OrganisationDeniedPermissionDerivation : DomainDerivation
    {
        public OrganisationDeniedPermissionDerivation(M m) : base(m, new Guid("c10dd444-3107-448e-a690-02f4d839ec0c")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(m.Organisation.IsInternalOrganisation),
            new ChangedPattern(m.ExternalAccountingTransaction.FromParty) { Steps = new IPropertyType[] { m.ExternalAccountingTransaction.FromParty } },
            new ChangedPattern(m.ExternalAccountingTransaction.ToParty) { Steps = new IPropertyType[] { m.ExternalAccountingTransaction.ToParty } },
            new ChangedPattern(m.Shipment.ShipFromParty) { Steps = new IPropertyType[] { m.Shipment.ShipFromParty } },
            new ChangedPattern(m.Shipment.ShipToParty) { Steps = new IPropertyType[] { m.Shipment.ShipToParty } },
            new ChangedPattern(m.Payment.Receiver) { Steps = new IPropertyType[] { m.Payment.Receiver } },
            new ChangedPattern(m.Payment.Sender) { Steps = new IPropertyType[] { m.Payment.Sender } },
            new ChangedPattern(m.Employment.Employer) { Steps = new IPropertyType[] { m.Employment.Employer } },
            new ChangedPattern(m.Engagement.BillToParty) { Steps = new IPropertyType[] { m.Engagement.BillToParty } },
            new ChangedPattern(m.Engagement.PlacingParty) { Steps = new IPropertyType[] { m.Engagement.PlacingParty } },
            new ChangedPattern(m.Part.ManufacturedBy) { Steps = new IPropertyType[] { m.Part.ManufacturedBy } },
            new ChangedPattern(m.Part.SuppliedBy) { Steps = new IPropertyType[] { m.Part.SuppliedBy } },
            new ChangedPattern(m.OrganisationGlAccount.InternalOrganisation) { Steps = new IPropertyType[] { m.OrganisationGlAccount.InternalOrganisation } },
            new ChangedPattern(m.OrganisationRollUp.Parent) { Steps = new IPropertyType[] { m.OrganisationRollUp.Parent } },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Organisation>())
            {
                var deletePermission = new Permissions(@this.Strategy.Session).Get(@this.Meta.ObjectType, @this.Meta.Delete);
                if (@this.IsDeletable)
                {
                    @this.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    @this.AddDeniedPermission(deletePermission);
                }
            }
        }
    }
}
