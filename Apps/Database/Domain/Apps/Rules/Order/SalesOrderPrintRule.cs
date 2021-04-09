// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class SalesOrderPrintRule : Rule
    {
        public SalesOrderPrintRule(MetaPopulation m) : base(m, new Guid("a02c04dd-fb87-4aee-a127-5907de69691b")) =>
            this.Patterns = new Pattern[]
            {
                m.Currency.RolePattern(v => v.IsoCode, v => v.OrdersWhereDerivedCurrency.Order.AsSalesOrder),
                m.SalesOrder.RolePattern(v => v.Description),
                m.SalesOrder.RolePattern(v => v.OrderNumber),
                m.SalesOrder.RolePattern(v => v.OrderDate),
                m.SalesOrder.RolePattern(v => v.TotalBasePrice),
                m.SalesOrder.RolePattern(v => v.TotalExVat),
                m.SalesOrder.RolePattern(v => v.GrandTotal),
                m.SalesOrder.RolePattern(v => v.DerivedVatRate),
                m.SalesOrder.RolePattern(v => v.TotalVat),
                m.SalesOrder.RolePattern(v => v.TotalIrpf),
                m.SalesOrder.RolePattern(v => v.TotalIncVat),
                m.SalesOrder.RolePattern(v => v.DerivedIrpfRate),
                m.SalesOrder.RolePattern(v => v.SalesTerms),
                m.AgreementTerm.RolePattern(v => v.TermType, v => v.AsSalesTerm.OrderWhereSalesTerm.Order.AsSalesOrder),
                m.SalesOrder.RolePattern(v => v.TakenBy),
                m.Organisation.RolePattern(v => v.PartyName, v => v.SalesOrdersWhereTakenBy.SalesOrder),
                m.EmailAddress.RolePattern(v => v.ElectronicAddressString, v => v.PartiesWhereGeneralEmail.Party.AsOrganisation.SalesOrdersWhereTakenBy.SalesOrder),
                m.EmailAddress.RolePattern(v => v.ElectronicAddressString, v => v.PartiesWhereInternetAddress.Party.AsOrganisation.SalesOrdersWhereTakenBy.SalesOrder),
                m.Organisation.RolePattern(v => v.TaxNumber, v => v.SalesOrdersWhereTakenBy.SalesOrder),
                m.Organisation.RolePattern(v => v.BillingInquiriesPhone, v => v.SalesOrdersWhereTakenBy.SalesOrder),
                m.Organisation.RolePattern(v => v.GeneralPhoneNumber, v => v.SalesOrdersWhereTakenBy.SalesOrder),
                m.TelecommunicationsNumber.RolePattern(v => v.CountryCode, v => v.PartiesWhereBillingInquiriesPhone.Party.AsOrganisation.SalesOrdersWhereTakenBy.SalesOrder),
                m.TelecommunicationsNumber.RolePattern(v => v.CountryCode, v => v.PartiesWhereGeneralPhoneNumber.Party.AsOrganisation.SalesOrdersWhereTakenBy.SalesOrder),
                m.TelecommunicationsNumber.RolePattern(v => v.AreaCode, v => v.PartiesWhereBillingInquiriesPhone.Party.AsOrganisation.SalesOrdersWhereTakenBy.SalesOrder),
                m.TelecommunicationsNumber.RolePattern(v => v.AreaCode, v => v.PartiesWhereGeneralPhoneNumber.Party.AsOrganisation.SalesOrdersWhereTakenBy.SalesOrder),
                m.TelecommunicationsNumber.RolePattern(v => v.ContactNumber, v => v.PartiesWhereBillingInquiriesPhone.Party.AsOrganisation.SalesOrdersWhereTakenBy.SalesOrder),
                m.TelecommunicationsNumber.RolePattern(v => v.ContactNumber, v => v.PartiesWhereGeneralPhoneNumber.Party.AsOrganisation.SalesOrdersWhereTakenBy.SalesOrder),

                //Gestopt in TakenByModel

            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SalesOrder>())
            {
                @this.ResetPrintDocument();
            }
        }
    }
}
