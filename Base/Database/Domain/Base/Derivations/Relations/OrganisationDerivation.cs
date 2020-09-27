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

    public class OrganisationDerivation : DomainDerivation
    {
        public OrganisationDerivation(M m) : base(m, new Guid("0379B923-210D-46DD-9D18-9D7BF5ED6FEA")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(M.Organisation.Class),
                new CreatedPattern(M.SupplierRelationship.Class){Steps = new IPropertyType[]{M.SupplierRelationship.InternalOrganisation}, OfType = M.Organisation.Class},
                new ChangedRolePattern(M.Employment.Employer){Steps = new IPropertyType[]{M.Employment.Employer}},
                new ChangedRolePattern(M.SupplierRelationship.FromDate) {Steps = new IPropertyType[]{M.SupplierRelationship.InternalOrganisation}, OfType = M.Organisation.Class},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;

            foreach (var organisation in matches.Cast<Organisation>())
            {
                //var singleton = session.GetSingleton();
                var now = session.Now();

                session.Prefetch(organisation.PrefetchPolicy);

                if (organisation.IsInternalOrganisation)
                {
                    if (!organisation.ExistRequestCounter)
                    {
                        organisation.RequestCounter = new CounterBuilder(session).Build();
                    }

                    if (!organisation.ExistQuoteCounter)
                    {
                        organisation.QuoteCounter = new CounterBuilder(session).Build();
                    }

                    if (!organisation.ExistPurchaseInvoiceCounter)
                    {
                        organisation.PurchaseInvoiceCounter = new CounterBuilder(session).Build();
                    }

                    if (!organisation.ExistPurchaseOrderCounter)
                    {
                        organisation.PurchaseOrderCounter = new CounterBuilder(session).Build();
                    }

                    if (!organisation.ExistSubAccountCounter)
                    {
                        organisation.SubAccountCounter = new CounterBuilder(session).Build();
                    }

                    if (!organisation.ExistIncomingShipmentCounter)
                    {
                        organisation.IncomingShipmentCounter = new CounterBuilder(session).Build();
                    }

                    if (!organisation.ExistWorkEffortCounter)
                    {
                        organisation.WorkEffortCounter = new CounterBuilder(session).Build();
                    }

                    if (!organisation.ExistInvoiceSequence)
                    {
                        organisation.InvoiceSequence = new InvoiceSequenceBuilder(session)
                            .WithName(organisation.Name)
                            .Build();
                    }

                    if (organisation.DoAccounting && !organisation.ExistFiscalYearStartMonth)
                    {
                        organisation.FiscalYearStartMonth = 1;
                    }

                    if (organisation.DoAccounting && !organisation.ExistFiscalYearStartDay)
                    {
                        organisation.FiscalYearStartDay = 1;
                    }
                }

                organisation.PartyName = organisation.Name;

                organisation.ActiveEmployees = organisation.EmploymentsWhereEmployer
                    .Where(v => v.FromDate <= now && (!v.ExistThroughDate || v.ThroughDate >= now))
                    .Select(v => v.Employee)
                    .ToArray();

                (organisation).ActiveCustomers = organisation.CustomerRelationshipsWhereInternalOrganisation
                    .Where(v => v.FromDate <= now && (!v.ExistThroughDate || v.ThroughDate >= now))
                    .Select(v => v.Customer)
                    .ToArray();

                // Contacts
                if (!organisation.ExistContactsUserGroup)
                {
                    var customerContactGroupName = $"Customer contacts at {organisation.Name} ({organisation.UniqueId})";
                    organisation.ContactsUserGroup = new UserGroupBuilder(organisation.Strategy.Session).WithName(customerContactGroupName).Build();
                }

                organisation.DeriveRelationships();

                organisation.ContactsUserGroup.Members = organisation.CurrentContacts.ToArray();

                var deletePermission = new Permissions(organisation.Strategy.Session).Get(organisation.Meta.ObjectType, organisation.Meta.Delete, Operations.Execute);
                if (organisation.IsDeletable)
                {
                    organisation.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    organisation.AddDeniedPermission(deletePermission);
                }

                organisation.ActiveEmployees = organisation.EmploymentsWhereEmployer
                    .Where(v => v.FromDate <= now && (!v.ExistThroughDate || v.ThroughDate >= now))
                    .Select(v => v.Employee)
                    .ToArray();

                organisation.DeriveRelationships();
            }
        }
    }
}
