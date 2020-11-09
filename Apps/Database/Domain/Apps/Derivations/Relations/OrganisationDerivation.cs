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
                new ChangedPattern(this.M.Employment.Employer){Steps = new IPropertyType[]{ this.M.Employment.Employer}},
                new ChangedPattern(this.M.SupplierRelationship.FromDate) {Steps = new IPropertyType[]{ this.M.SupplierRelationship.InternalOrganisation}, OfType = this.M.Organisation.Class},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;

            foreach (var @this in matches.Cast<Organisation>())
            {
                //var singleton = session.GetSingleton();
                var now = session.Now();

                session.Prefetch(@this.PrefetchPolicy);

                if (@this.IsInternalOrganisation)
                {
                    if (!@this.ExistRequestCounter)
                    {
                        @this.RequestCounter = new CounterBuilder(session).Build();
                    }

                    if (!@this.ExistQuoteCounter)
                    {
                        @this.QuoteCounter = new CounterBuilder(session).Build();
                    }

                    if (!@this.ExistPurchaseInvoiceCounter)
                    {
                        @this.PurchaseInvoiceCounter = new CounterBuilder(session).Build();
                    }

                    if (!@this.ExistPurchaseOrderCounter)
                    {
                        @this.PurchaseOrderCounter = new CounterBuilder(session).Build();
                    }

                    if (!@this.ExistSubAccountCounter)
                    {
                        @this.SubAccountCounter = new CounterBuilder(session).Build();
                    }

                    if (!@this.ExistIncomingShipmentCounter)
                    {
                        @this.IncomingShipmentCounter = new CounterBuilder(session).Build();
                    }

                    if (!@this.ExistWorkEffortCounter)
                    {
                        @this.WorkEffortCounter = new CounterBuilder(session).Build();
                    }

                    if (!@this.ExistInvoiceSequence)
                    {
                        @this.InvoiceSequence = new InvoiceSequenceBuilder(session)
                            .WithName(@this.Name)
                            .Build();
                    }

                    if (@this.DoAccounting && !@this.ExistFiscalYearStartMonth)
                    {
                        @this.FiscalYearStartMonth = 1;
                    }

                    if (@this.DoAccounting && !@this.ExistFiscalYearStartDay)
                    {
                        @this.FiscalYearStartDay = 1;
                    }
                }

                @this.PartyName = @this.Name;

                @this.ActiveEmployees = @this.EmploymentsWhereEmployer
                    .Where(v => v.FromDate <= now && (!v.ExistThroughDate || v.ThroughDate >= now))
                    .Select(v => v.Employee)
                    .ToArray();

                (@this).ActiveCustomers = @this.CustomerRelationshipsWhereInternalOrganisation
                    .Where(v => v.FromDate <= now && (!v.ExistThroughDate || v.ThroughDate >= now))
                    .Select(v => v.Customer)
                    .ToArray();

                // Contacts
                if (!@this.ExistContactsUserGroup)
                {
                    var customerContactGroupName = $"Customer contacts at {@this.Name} ({@this.UniqueId})";
                    @this.ContactsUserGroup = new UserGroupBuilder(@this.Strategy.Session).WithName(customerContactGroupName).Build();
                }

                @this.DeriveRelationships();

                @this.ContactsUserGroup.Members = @this.CurrentContacts.ToArray();

                @this.ActiveEmployees = @this.EmploymentsWhereEmployer
                    .Where(v => v.FromDate <= now && (!v.ExistThroughDate || v.ThroughDate >= now))
                    .Select(v => v.Employee)
                    .ToArray();

                @this.DeriveRelationships();
            }
        }
    }
}
