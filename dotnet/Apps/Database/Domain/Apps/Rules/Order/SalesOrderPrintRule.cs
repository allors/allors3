// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class SalesOrderPrintRule : Rule
    {
        public SalesOrderPrintRule(MetaPopulation m) : base(m, new Guid("a02c04dd-fb87-4aee-a127-5907de69691b")) =>
            this.Patterns = new Pattern[]
            {
                m.Currency.RolePattern(v => v.IsoCode, v => v.OrdersWhereDerivedCurrency.ObjectType.AsSalesOrder),
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
                m.SalesOrder.RolePattern(v => v.DerivedShipToAddress),
                m.SalesOrder.RolePattern(v => v.BillToContactPerson),
                m.SalesOrder.RolePattern(v => v.DerivedBillToContactMechanism),
                m.AgreementTerm.RolePattern(v => v.TermType, v => v.AsSalesTerm.OrderWhereSalesTerm.ObjectType.AsSalesOrder),
                m.SalesOrder.RolePattern(v => v.TakenBy),
                m.Organisation.RolePattern(v => v.DisplayName, v => v.SalesOrdersWhereTakenBy.ObjectType),
                m.EmailAddress.RolePattern(v => v.ElectronicAddressString, v => v.PartiesWhereGeneralEmail.ObjectType.AsOrganisation.SalesOrdersWhereTakenBy.ObjectType),
                m.EmailAddress.RolePattern(v => v.ElectronicAddressString, v => v.PartiesWhereInternetAddress.ObjectType.AsOrganisation.SalesOrdersWhereTakenBy.ObjectType),
                m.Organisation.RolePattern(v => v.TaxNumber, v => v.SalesOrdersWhereTakenBy.ObjectType),
                m.Organisation.RolePattern(v => v.BillingInquiriesPhone, v => v.SalesOrdersWhereTakenBy.ObjectType),
                m.Organisation.RolePattern(v => v.GeneralPhoneNumber, v => v.SalesOrdersWhereTakenBy.ObjectType),
                m.TelecommunicationsNumber.RolePattern(v => v.CountryCode, v => v.PartiesWhereBillingInquiriesPhone.ObjectType.AsOrganisation.SalesOrdersWhereTakenBy.ObjectType),
                m.TelecommunicationsNumber.RolePattern(v => v.CountryCode, v => v.PartiesWhereGeneralPhoneNumber.ObjectType.AsOrganisation.SalesOrdersWhereTakenBy.ObjectType),
                m.TelecommunicationsNumber.RolePattern(v => v.AreaCode, v => v.PartiesWhereBillingInquiriesPhone.ObjectType.AsOrganisation.SalesOrdersWhereTakenBy.ObjectType),
                m.TelecommunicationsNumber.RolePattern(v => v.AreaCode, v => v.PartiesWhereGeneralPhoneNumber.ObjectType.AsOrganisation.SalesOrdersWhereTakenBy.ObjectType),
                m.TelecommunicationsNumber.RolePattern(v => v.ContactNumber, v => v.PartiesWhereBillingInquiriesPhone.ObjectType.AsOrganisation.SalesOrdersWhereTakenBy.ObjectType),
                m.TelecommunicationsNumber.RolePattern(v => v.ContactNumber, v => v.PartiesWhereGeneralPhoneNumber.ObjectType.AsOrganisation.SalesOrdersWhereTakenBy.ObjectType),

                //Gestopt in TakenByModel

            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SalesOrder>())
            {
                @this.ResetPrintDocument();
            }
        }
    }
}
