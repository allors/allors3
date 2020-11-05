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
            new ChangedPattern(m.ExternalAccountingTransaction.FromParty) { Steps = new IPropertyType[] { m.ExternalAccountingTransaction.FromParty }, OfType = m.Organisation.Class },
            new ChangedPattern(m.ExternalAccountingTransaction.ToParty) { Steps = new IPropertyType[] { m.ExternalAccountingTransaction.ToParty }, OfType = m.Organisation.Class },
            new ChangedPattern(m.Shipment.ShipFromParty) { Steps = new IPropertyType[] { m.Shipment.ShipFromParty }, OfType = m.Organisation.Class },
            new ChangedPattern(m.Shipment.ShipToParty) { Steps = new IPropertyType[] { m.Shipment.ShipToParty }, OfType = m.Organisation.Class },
            new ChangedPattern(m.Payment.Receiver) { Steps = new IPropertyType[] { m.Payment.Receiver }, OfType = m.Organisation.Class },
            new ChangedPattern(m.Payment.Sender) { Steps = new IPropertyType[] { m.Payment.Sender }, OfType = m.Organisation.Class },
            new ChangedPattern(m.Employment.Employer) { Steps = new IPropertyType[] { m.Employment.Employer }, OfType = m.Organisation.Class },
            new ChangedPattern(m.Engagement.BillToParty) { Steps = new IPropertyType[] { m.Engagement.BillToParty }, OfType = m.Organisation.Class },
            new ChangedPattern(m.Engagement.PlacingParty) { Steps = new IPropertyType[] { m.Engagement.PlacingParty }, OfType = m.Organisation.Class },
            new ChangedPattern(m.Part.ManufacturedBy) { Steps = new IPropertyType[] { m.Part.ManufacturedBy }, OfType = m.Organisation.Class },
            new ChangedPattern(m.Part.SuppliedBy) { Steps = new IPropertyType[] { m.Part.SuppliedBy }, OfType = m.Organisation.Class },
            new ChangedPattern(m.OrganisationGlAccount.InternalOrganisation) { Steps = new IPropertyType[] { m.OrganisationGlAccount.InternalOrganisation }, OfType = m.Organisation.Class },
            new ChangedPattern(m.OrganisationRollUp.Parent) { Steps = new IPropertyType[] { m.OrganisationRollUp.Parent }, OfType = m.Organisation.Class },
            new ChangedPattern(m.PartyFixedAssetAssignment.Party) { Steps = new IPropertyType[] { m.PartyFixedAssetAssignment.Party }, OfType = m.Organisation.Class },
            new ChangedPattern(m.PickList.ShipToParty) { Steps = new IPropertyType[] { m.PickList.ShipToParty }, OfType = m.Organisation.Class },
            new ChangedPattern(m.Proposal.Issuer) { Steps = new IPropertyType[] { m.Proposal.Issuer }, OfType = m.Organisation.Class },
            new ChangedPattern(m.Proposal.Receiver) { Steps = new IPropertyType[] { m.Proposal.Receiver }, OfType = m.Organisation.Class },
            new ChangedPattern(m.PurchaseInvoice.BilledTo) { Steps = new IPropertyType[] { m.PurchaseInvoice.BilledTo }, OfType = m.Organisation.Class },
            new ChangedPattern(m.PurchaseInvoice.BilledFrom) { Steps = new IPropertyType[] { m.PurchaseInvoice.BilledFrom }, OfType = m.Organisation.Class },
            new ChangedPattern(m.PurchaseInvoice.ShipToCustomer) { Steps = new IPropertyType[] { m.PurchaseInvoice.ShipToCustomer }, OfType = m.Organisation.Class },
            new ChangedPattern(m.PurchaseInvoice.ShipToEndCustomer) { Steps = new IPropertyType[] { m.PurchaseInvoice.ShipToEndCustomer }, OfType = m.Organisation.Class },
            new ChangedPattern(m.PurchaseInvoice.BillToEndCustomer) { Steps = new IPropertyType[] { m.PurchaseInvoice.BillToEndCustomer }, OfType = m.Organisation.Class },
            new ChangedPattern(m.PurchaseOrder.TakenViaSupplier) { Steps = new IPropertyType[] { m.PurchaseOrder.TakenViaSupplier }, OfType = m.Organisation.Class },
            new ChangedPattern(m.PurchaseOrder.TakenViaSubcontractor) { Steps = new IPropertyType[] { m.PurchaseOrder.TakenViaSubcontractor }, OfType = m.Organisation.Class },
            new ChangedPattern(m.Request.Originator) { Steps = new IPropertyType[] { m.Request.Originator }, OfType = m.Organisation.Class },
            new ChangedPattern(m.Request.Recipient) { Steps = new IPropertyType[] { m.Request.Recipient }, OfType = m.Organisation.Class },
            new ChangedPattern(m.Requirement.Authorizer) { Steps = new IPropertyType[] { m.Requirement.Authorizer }, OfType = m.Organisation.Class },
            new ChangedPattern(m.Requirement.NeededFor) { Steps = new IPropertyType[] { m.Requirement.NeededFor }, OfType = m.Organisation.Class },
            new ChangedPattern(m.Requirement.Originator) { Steps = new IPropertyType[] { m.Requirement.Originator }, OfType = m.Organisation.Class },
            new ChangedPattern(m.Requirement.ServicedBy) { Steps = new IPropertyType[] { m.Requirement.ServicedBy }, OfType = m.Organisation.Class },
            new ChangedPattern(m.SalesInvoice.BilledFrom) { Steps = new IPropertyType[] { m.SalesInvoice.BilledFrom }, OfType = m.Organisation.Class },
            new ChangedPattern(m.SalesInvoice.BillToCustomer) { Steps = new IPropertyType[] { m.SalesInvoice.BillToCustomer }, OfType = m.Organisation.Class },
            new ChangedPattern(m.SalesInvoice.BillToEndCustomer) { Steps = new IPropertyType[] { m.SalesInvoice.BillToEndCustomer }, OfType = m.Organisation.Class },
            new ChangedPattern(m.SalesInvoice.ShipToCustomer) { Steps = new IPropertyType[] { m.SalesInvoice.ShipToCustomer }, OfType = m.Organisation.Class },
            new ChangedPattern(m.SalesInvoice.ShipToEndCustomer) { Steps = new IPropertyType[] { m.SalesInvoice.ShipToEndCustomer }, OfType = m.Organisation.Class },
            new ChangedPattern(m.SalesOrder.BillToCustomer) { Steps = new IPropertyType[] { m.SalesOrder.BillToCustomer }, OfType = m.Organisation.Class },
            new ChangedPattern(m.SalesOrder.BillToEndCustomer) { Steps = new IPropertyType[] { m.SalesOrder.BillToEndCustomer }, OfType = m.Organisation.Class },
            new ChangedPattern(m.SalesOrder.ShipToCustomer) { Steps = new IPropertyType[] { m.SalesOrder.ShipToCustomer }, OfType = m.Organisation.Class },
            new ChangedPattern(m.SalesOrder.ShipToEndCustomer) { Steps = new IPropertyType[] { m.SalesOrder.ShipToEndCustomer }, OfType = m.Organisation.Class },
            new ChangedPattern(m.SalesOrder.PlacingCustomer) { Steps = new IPropertyType[] { m.SalesOrder.PlacingCustomer }, OfType = m.Organisation.Class },
            new ChangedPattern(m.SalesOrder.TakenBy) { Steps = new IPropertyType[] { m.SalesOrder.TakenBy }, OfType = m.Organisation.Class },
            new ChangedPattern(m.SalesOrderItem.AssignedShipToParty) { Steps = new IPropertyType[] { m.SalesOrderItem.AssignedShipToParty }, OfType = m.Organisation.Class },
            new ChangedPattern(m.SerialisedItem.SuppliedBy) { Steps = new IPropertyType[] { m.SerialisedItem.SuppliedBy }, OfType = m.Organisation.Class },
            new ChangedPattern(m.SerialisedItem.OwnedBy) { Steps = new IPropertyType[] { m.SerialisedItem.OwnedBy }, OfType = m.Organisation.Class },
            new ChangedPattern(m.SerialisedItem.RentedBy) { Steps = new IPropertyType[] { m.SerialisedItem.RentedBy }, OfType = m.Organisation.Class },
            new ChangedPattern(m.SerialisedItem.Buyer) { Steps = new IPropertyType[] { m.SerialisedItem.Buyer }, OfType = m.Organisation.Class },
            new ChangedPattern(m.SerialisedItem.Seller) { Steps = new IPropertyType[] { m.SerialisedItem.Seller }, OfType = m.Organisation.Class },
            new ChangedPattern(m.WorkEffort.Customer) { Steps = new IPropertyType[] { m.WorkEffort.Customer }, OfType = m.Organisation.Class },
            new ChangedPattern(m.WorkEffort.ExecutedBy) { Steps = new IPropertyType[] { m.WorkEffort.ExecutedBy }, OfType = m.Organisation.Class },
            new ChangedPattern(m.WorkEffortPartyAssignment.Party) { Steps = new IPropertyType[] { m.WorkEffortPartyAssignment.Party }, OfType = m.Organisation.Class },
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
