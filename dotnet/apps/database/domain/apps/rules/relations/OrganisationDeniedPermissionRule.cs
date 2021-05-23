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
            m.Party.AssociationPattern(v => v.ExternalAccountingTransactionsWhereFromParty ,m.Organisation),
            m.Party.AssociationPattern(v => v.ExternalAccountingTransactionsWhereToParty,m.Organisation),
            m.Party.AssociationPattern(v => v.ShipmentsWhereShipFromParty,m.Organisation),
            m.Party.AssociationPattern(v => v.ShipmentsWhereShipToParty,m.Organisation),
            m.Party.AssociationPattern(v => v.PaymentsWhereReceiver,m.Organisation),
            m.Party.AssociationPattern(v => v.PaymentsWhereSender,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.EmploymentsWhereEmployer,m.Organisation),
            m.Party.AssociationPattern(v => v.EngagementsWhereBillToParty,m.Organisation),
            m.Party.AssociationPattern(v => v.EngagementsWherePlacingParty,m.Organisation),
            m.Party.AssociationPattern(v => v.PartsWhereManufacturedBy,m.Organisation),
            m.Party.AssociationPattern(v => v.PartsWhereSuppliedBy,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.OrganisationGlAccountsWhereInternalOrganisation,m.Organisation),
            m.Organisation.AssociationPattern(v => v.OrganisationRollUpsWhereParent,m.Organisation),
            m.Party.AssociationPattern(v => v.PartyFixedAssetAssignmentsWhereParty,m.Organisation),
            m.Party.AssociationPattern(v => v.PickListsWhereShipToParty,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.QuotesWhereIssuer,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.QuotesWhereReceiver,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.PurchaseInvoicesWhereBilledTo,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.PurchaseInvoicesWhereBilledFrom,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.PurchaseInvoicesWhereShipToCustomer,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.PurchaseInvoicesWhereShipToEndCustomer,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.PurchaseInvoicesWhereBillToEndCustomer,m.Organisation),
            m.Organisation.AssociationPattern(v => v.PurchaseOrdersWhereTakenViaSupplier,m.Organisation),
            m.Organisation.AssociationPattern(v => v.PurchaseOrdersWhereTakenViaSubcontractor,m.Organisation),
            m.Party.AssociationPattern(v => v.RequestsWhereOriginator,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.RequestsWhereRecipient,m.Organisation),
            m.Party.AssociationPattern(v => v.RequirementsWhereAuthorizer,m.Organisation),
            m.Party.AssociationPattern(v => v.RequirementsWhereNeededFor,m.Organisation),
            m.Party.AssociationPattern(v => v.RequirementsWhereOriginator,m.Organisation),
            m.Party.AssociationPattern(v => v.RequirementsWhereServicedBy,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.SalesInvoicesWhereBilledFrom,m.Organisation),
            m.Party.AssociationPattern(v => v.SalesInvoicesWhereBillToCustomer,m.Organisation),
            m.Party.AssociationPattern(v => v.SalesInvoicesWhereBillToEndCustomer,m.Organisation),
            m.Party.AssociationPattern(v => v.SalesInvoicesWhereShipToCustomer,m.Organisation),
            m.Party.AssociationPattern(v => v.SalesInvoicesWhereShipToEndCustomer,m.Organisation),
            m.Party.AssociationPattern(v => v.SalesOrdersWhereBillToCustomer,m.Organisation),
            m.Party.AssociationPattern(v => v.SalesOrdersWhereBillToEndCustomer,m.Organisation),
            m.Party.AssociationPattern(v => v.SalesOrdersWhereShipToCustomer,m.Organisation),
            m.Party.AssociationPattern(v => v.SalesOrdersWhereShipToEndCustomer,m.Organisation),
            m.Party.AssociationPattern(v => v.SalesOrdersWherePlacingCustomer,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.SalesOrdersWhereTakenBy,m.Organisation),
            m.Party.AssociationPattern(v => v.SalesOrderItemsWhereAssignedShipToParty,m.Organisation),
            m.Party.AssociationPattern(v => v.SerialisedItemsWhereSuppliedBy,m.Organisation),
            m.Party.AssociationPattern(v => v.SerialisedItemsWhereOwnedBy,m.Organisation),
            m.Party.AssociationPattern(v => v.SerialisedItemsWhereRentedBy,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.SerialisedItemsWhereBuyer,m.Organisation),
            m.InternalOrganisation.AssociationPattern(v => v.SerialisedItemsWhereSeller,m.Organisation),
            m.Party.AssociationPattern(v => v.WorkEffortsWhereCustomer,m.Organisation),
            m.Organisation.AssociationPattern(v => v.WorkEffortsWhereExecutedBy,m.Organisation),
            m.Party.AssociationPattern(v => v.WorkEffortPartyAssignmentsWhereParty,m.Organisation),
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
