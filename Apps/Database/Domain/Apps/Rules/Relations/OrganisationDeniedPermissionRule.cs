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
            m.Organisation.RolePattern(v => v.IsInternalOrganisation),
            m.Party.AssociationPattern(v => v.ExternalAccountingTransactionsWhereFromParty, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.ExternalAccountingTransactionsWhereToParty, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.ShipmentsWhereShipFromParty, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.ShipmentsWhereShipToParty, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.PaymentsWhereReceiver, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.PaymentsWhereSender, null ,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.EmploymentsWhereEmployer, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.EngagementsWhereBillToParty, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.EngagementsWherePlacingParty, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.PartsWhereManufacturedBy, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.PartsWhereSuppliedBy, null ,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.OrganisationGlAccountsWhereInternalOrganisation, null ,m.Organisation),
            m.Organisation.AssociationPattern(v => v.OrganisationRollUpsWhereParent, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.PartyFixedAssetAssignmentsWhereParty, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.PickListsWhereShipToParty, null ,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.QuotesWhereIssuer, null ,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.QuotesWhereReceiver, null ,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.PurchaseInvoicesWhereBilledTo, null ,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.PurchaseInvoicesWhereBilledFrom, null ,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.PurchaseInvoicesWhereShipToCustomer, null ,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.PurchaseInvoicesWhereShipToEndCustomer, null ,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.PurchaseInvoicesWhereBillToEndCustomer, null ,m.Organisation),
            m.Organisation.AssociationPattern(v => v.PurchaseOrdersWhereTakenViaSupplier, null ,m.Organisation),
            m.Organisation.AssociationPattern(v => v.PurchaseOrdersWhereTakenViaSubcontractor, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.RequestsWhereOriginator, null ,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.RequestsWhereRecipient, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.RequirementsWhereAuthorizer, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.RequirementsWhereNeededFor, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.RequirementsWhereOriginator, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.RequirementsWhereServicedBy, null ,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.SalesInvoicesWhereBilledFrom, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.SalesInvoicesWhereBillToCustomer, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.SalesInvoicesWhereBillToEndCustomer, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.SalesInvoicesWhereShipToCustomer, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.SalesInvoicesWhereShipToEndCustomer, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.SalesOrdersWhereBillToCustomer, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.SalesOrdersWhereBillToEndCustomer, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.SalesOrdersWhereShipToCustomer, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.SalesOrdersWhereShipToEndCustomer, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.SalesOrdersWherePlacingCustomer, null ,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.SalesOrdersWhereTakenBy, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.SalesOrderItemsWhereAssignedShipToParty, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.SerialisedItemsWhereSuppliedBy, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.SerialisedItemsWhereOwnedBy, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.SerialisedItemsWhereRentedBy, null ,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.SerialisedItemsWhereBuyer, null ,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.SerialisedItemsWhereSeller, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.WorkEffortsWhereCustomer, null ,m.Organisation),
            m.Organisation.AssociationPattern(v => v.WorkEffortsWhereExecutedBy, null ,m.Organisation),
            m.Party.AssociationPattern(v => v.WorkEffortPartyAssignmentsWhereParty, null ,m.Organisation),
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
