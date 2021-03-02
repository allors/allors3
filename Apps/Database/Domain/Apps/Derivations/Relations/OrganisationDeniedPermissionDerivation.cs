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

    public class OrganisationDeniedPermissionDerivation : DomainDerivation
    {
        public OrganisationDeniedPermissionDerivation(M m) : base(m, new Guid("c10dd444-3107-448e-a690-02f4d839ec0c")) =>
            this.Patterns = new Pattern[]
        {
            new AssociationPattern(m.Organisation.IsInternalOrganisation),
            new RolePattern(m.ExternalAccountingTransaction.FromParty) { OfType = m.Organisation.Class },
            new RolePattern(m.ExternalAccountingTransaction.ToParty) { OfType = m.Organisation.Class },
            new RolePattern(m.Shipment.ShipFromParty) { OfType = m.Organisation.Class },
            new RolePattern(m.Shipment.ShipToParty) { OfType = m.Organisation.Class },
            new RolePattern(m.Payment.Receiver) { OfType = m.Organisation.Class },
            new RolePattern(m.Payment.Sender) { OfType = m.Organisation.Class },
            new RolePattern(m.Employment.Employer) { OfType = m.Organisation.Class },
            new RolePattern(m.Engagement.BillToParty) { OfType = m.Organisation.Class },
            new RolePattern(m.Engagement.PlacingParty) { OfType = m.Organisation.Class },
            new RolePattern(m.Part.ManufacturedBy) { OfType = m.Organisation.Class },
            new RolePattern(m.Part.SuppliedBy) { OfType = m.Organisation.Class },
            new RolePattern(m.OrganisationGlAccount.InternalOrganisation) { OfType = m.Organisation.Class },
            new RolePattern(m.OrganisationRollUp.Parent) { OfType = m.Organisation.Class },
            new RolePattern(m.PartyFixedAssetAssignment.Party) { OfType = m.Organisation.Class },
            new RolePattern(m.PickList.ShipToParty) { OfType = m.Organisation.Class },
            new RolePattern(m.Quote.Issuer) { OfType = m.Organisation.Class },
            new RolePattern(m.Quote.Receiver) { OfType = m.Organisation.Class },
            new RolePattern(m.PurchaseInvoice.BilledTo) { OfType = m.Organisation.Class },
            new RolePattern(m.PurchaseInvoice.BilledFrom) { OfType = m.Organisation.Class },
            new RolePattern(m.PurchaseInvoice.ShipToCustomer) { OfType = m.Organisation.Class },
            new RolePattern(m.PurchaseInvoice.ShipToEndCustomer) { OfType = m.Organisation.Class },
            new RolePattern(m.PurchaseInvoice.BillToEndCustomer) { OfType = m.Organisation.Class },
            new RolePattern(m.PurchaseOrder.TakenViaSupplier) { OfType = m.Organisation.Class },
            new RolePattern(m.PurchaseOrder.TakenViaSubcontractor) { OfType = m.Organisation.Class },
            new RolePattern(m.Request.Originator) { OfType = m.Organisation.Class },
            new RolePattern(m.Request.Recipient) { OfType = m.Organisation.Class },
            new RolePattern(m.Requirement.Authorizer) { OfType = m.Organisation.Class },
            new RolePattern(m.Requirement.NeededFor) { OfType = m.Organisation.Class },
            new RolePattern(m.Requirement.Originator) { OfType = m.Organisation.Class },
            new RolePattern(m.Requirement.ServicedBy) { OfType = m.Organisation.Class },
            new RolePattern(m.SalesInvoice.BilledFrom) { OfType = m.Organisation.Class },
            new RolePattern(m.SalesInvoice.BillToCustomer) { OfType = m.Organisation.Class },
            new RolePattern(m.SalesInvoice.BillToEndCustomer) { OfType = m.Organisation.Class },
            new RolePattern(m.SalesInvoice.ShipToCustomer) { OfType = m.Organisation.Class },
            new RolePattern(m.SalesInvoice.ShipToEndCustomer) { OfType = m.Organisation.Class },
            new RolePattern(m.SalesOrder.BillToCustomer) { OfType = m.Organisation.Class },
            new RolePattern(m.SalesOrder.BillToEndCustomer) { OfType = m.Organisation.Class },
            new RolePattern(m.SalesOrder.ShipToCustomer) { OfType = m.Organisation.Class },
            new RolePattern(m.SalesOrder.ShipToEndCustomer) { OfType = m.Organisation.Class },
            new RolePattern(m.SalesOrder.PlacingCustomer) { OfType = m.Organisation.Class },
            new RolePattern(m.SalesOrder.TakenBy) { OfType = m.Organisation.Class },
            new RolePattern(m.SalesOrderItem.AssignedShipToParty) { OfType = m.Organisation.Class },
            new RolePattern(m.SerialisedItem.SuppliedBy) { OfType = m.Organisation.Class },
            new RolePattern(m.SerialisedItem.OwnedBy) { OfType = m.Organisation.Class },
            new RolePattern(m.SerialisedItem.RentedBy) { OfType = m.Organisation.Class },
            new RolePattern(m.SerialisedItem.Buyer) { OfType = m.Organisation.Class },
            new RolePattern(m.SerialisedItem.Seller) { OfType = m.Organisation.Class },
            new RolePattern(m.WorkEffort.Customer) { OfType = m.Organisation.Class },
            new RolePattern(m.WorkEffort.ExecutedBy) { OfType = m.Organisation.Class },
            new RolePattern(m.WorkEffortPartyAssignment.Party) { OfType = m.Organisation.Class },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Organisation>())
            {
                var deletePermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta.ObjectType, @this.Meta.Delete);
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
