// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class OrganisationCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdOrgaisations = changeSet.Created.Select(session.Instantiate).OfType<Organisation>();

                changeSet.AssociationsByRoleType.TryGetValue(M.Employment.Employer, out var changedEmployer);
                var employmentWhereEmployer = changedEmployer?.Select(session.Instantiate).OfType<Employment>();

                foreach (var organisation in createdOrgaisations)
                {
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

                    var now = organisation.Session().Now();

                    DeriveActiveEmployees(organisation, now);

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
                }

                if (employmentWhereEmployer?.Any() == true)
                {
                    foreach (var employment in employmentWhereEmployer)
                    {
                        var now = employment.Employer.Session().Now();

                        DeriveActiveEmployees((Organisation) employment.Employer, now);
                    }
                }

                static void DeriveActiveEmployees(Organisation organisation, DateTime now) => (organisation).ActiveEmployees = organisation.EmploymentsWhereEmployer
                                        .Where(v => v.FromDate <= now && (!v.ExistThroughDate || v.ThroughDate >= now))
                                        .Select(v => v.Employee)
                                        .ToArray();
            }


        }

        public static void OrganisationRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("B599AAA1-9B94-4298-9649-23D2B17F12A1")] = new OrganisationCreationDerivation();
        }
    }
}
