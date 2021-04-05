// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class OrganisationDeniedPermissionRule : Rule
    {
        public OrganisationDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("c10dd444-3107-448e-a690-02f4d839ec0c")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.Organisation, m.Organisation.IsInternalOrganisation),
            new AssociationPattern(m.ExternalAccountingTransaction.FromParty) { OfType = m.Organisation },
            new AssociationPattern(m.ExternalAccountingTransaction.ToParty) { OfType = m.Organisation },
            new AssociationPattern(m.Shipment.ShipFromParty) { OfType = m.Organisation },
            new AssociationPattern(m.Shipment.ShipToParty) { OfType = m.Organisation },
            new AssociationPattern(m.Payment.Receiver) { OfType = m.Organisation },
            new AssociationPattern(m.Payment.Sender) { OfType = m.Organisation },
            new AssociationPattern(m.Employment.Employer) { OfType = m.Organisation },
            new AssociationPattern(m.Engagement.BillToParty) { OfType = m.Organisation },
            new AssociationPattern(m.Engagement.PlacingParty) { OfType = m.Organisation },
            new AssociationPattern(m.Part.ManufacturedBy) { OfType = m.Organisation },
            new AssociationPattern(m.Part.SuppliedBy) { OfType = m.Organisation },
            new AssociationPattern(m.OrganisationGlAccount.InternalOrganisation) { OfType = m.Organisation },
            new AssociationPattern(m.OrganisationRollUp.Parent) { OfType = m.Organisation },
            new AssociationPattern(m.PartyFixedAssetAssignment.Party) { OfType = m.Organisation },
            new AssociationPattern(m.PickList.ShipToParty) { OfType = m.Organisation },
            new AssociationPattern(m.Quote.Issuer) { OfType = m.Organisation },
            new AssociationPattern(m.Quote.Receiver) { OfType = m.Organisation },
            new AssociationPattern(m.PurchaseInvoice.BilledTo) { OfType = m.Organisation },
            new AssociationPattern(m.PurchaseInvoice.BilledFrom) { OfType = m.Organisation },
            new AssociationPattern(m.PurchaseInvoice.ShipToCustomer) { OfType = m.Organisation },
            new AssociationPattern(m.PurchaseInvoice.ShipToEndCustomer) { OfType = m.Organisation },
            new AssociationPattern(m.PurchaseInvoice.BillToEndCustomer) { OfType = m.Organisation },
            new AssociationPattern(m.PurchaseOrder.TakenViaSupplier) { OfType = m.Organisation },
            new AssociationPattern(m.PurchaseOrder.TakenViaSubcontractor) { OfType = m.Organisation },
            new AssociationPattern(m.Request.Originator) { OfType = m.Organisation },
            new AssociationPattern(m.Request.Recipient) { OfType = m.Organisation },
            new AssociationPattern(m.Requirement.Authorizer) { OfType = m.Organisation },
            new AssociationPattern(m.Requirement.NeededFor) { OfType = m.Organisation },
            new AssociationPattern(m.Requirement.Originator) { OfType = m.Organisation },
            new AssociationPattern(m.Requirement.ServicedBy) { OfType = m.Organisation },
            new AssociationPattern(m.SalesInvoice.BilledFrom) { OfType = m.Organisation },
            new AssociationPattern(m.SalesInvoice.BillToCustomer) { OfType = m.Organisation },
            new AssociationPattern(m.SalesInvoice.BillToEndCustomer) { OfType = m.Organisation },
            new AssociationPattern(m.SalesInvoice.ShipToCustomer) { OfType = m.Organisation },
            new AssociationPattern(m.SalesInvoice.ShipToEndCustomer) { OfType = m.Organisation },
            new AssociationPattern(m.SalesOrder.BillToCustomer) { OfType = m.Organisation },
            new AssociationPattern(m.SalesOrder.BillToEndCustomer) { OfType = m.Organisation },
            new AssociationPattern(m.SalesOrder.ShipToCustomer) { OfType = m.Organisation },
            new AssociationPattern(m.SalesOrder.ShipToEndCustomer) { OfType = m.Organisation },
            new AssociationPattern(m.SalesOrder.PlacingCustomer) { OfType = m.Organisation },
            new AssociationPattern(m.SalesOrder.TakenBy) { OfType = m.Organisation },
            new AssociationPattern(m.SalesOrderItem.AssignedShipToParty) { OfType = m.Organisation },
            new AssociationPattern(m.SerialisedItem.SuppliedBy) { OfType = m.Organisation },
            new AssociationPattern(m.SerialisedItem.OwnedBy) { OfType = m.Organisation },
            new AssociationPattern(m.SerialisedItem.RentedBy) { OfType = m.Organisation },
            new AssociationPattern(m.SerialisedItem.Buyer) { OfType = m.Organisation },
            new AssociationPattern(m.SerialisedItem.Seller) { OfType = m.Organisation },
            new AssociationPattern(m.WorkEffort.Customer) { OfType = m.Organisation },
            new AssociationPattern(m.WorkEffort.ExecutedBy) { OfType = m.Organisation },
            new AssociationPattern(m.WorkEffortPartyAssignment.Party) { OfType = m.Organisation },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Organisation>())
            {
                var deletePermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Delete);
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
