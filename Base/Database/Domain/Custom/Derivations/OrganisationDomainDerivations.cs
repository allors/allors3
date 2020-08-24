// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;

    public static partial class DabaseExtensions
    {
        public class OrganisationCreationDerivation : IDomainDerivation
        {
            public void Derive(IDomainChangeSet changeSet, IDomainValidation validation)
            {
                var createdOrgaisations = changeSet.Created.OfType<Organisation>();

                foreach (var organisation in createdOrgaisations)
                {
                    var session = organisation.Strategy.Session;
                    //var singleton = session.GetSingleton();

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
                            organisation.InvoiceSequence = new InvoiceSequenceBuilder(session).Build();
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

                    var now = organisation.Session().Now();

                    ((OrganisationDerivedRoles)organisation).ActiveEmployees = organisation.EmploymentsWhereEmployer
                        .Where(v => v.FromDate <= now && (!v.ExistThroughDate || v.ThroughDate >= now))
                        .Select(v => v.Employee)
                        .ToArray();

                    ((OrganisationDerivedRoles)organisation).ActiveCustomers = organisation.CustomerRelationshipsWhereInternalOrganisation
                        .Where(v => v.FromDate <= now && (!v.ExistThroughDate || v.ThroughDate >= now))
                        .Select(v => v.Customer)
                        .ToArray();

                    // Contacts
                    if (!organisation.ExistContactsUserGroup)
                    {
                        var customerContactGroupName = $"Customer contacts at {organisation.Name} ({organisation.UniqueId})";
                        ((OrganisationDerivedRoles)organisation).ContactsUserGroup = new UserGroupBuilder(organisation.Strategy.Session).WithName(customerContactGroupName).Build();
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
                }
            }
        }

        public static void OrganisationRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("B599AAA1-9B94-4298-9649-23D2B17F12A1")] = new OrganisationCreationDerivation();
        }
    }
}
