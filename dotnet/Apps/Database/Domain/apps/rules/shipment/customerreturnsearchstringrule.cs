// <copyright file="InventoryItemSearchStringDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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

    public class CustomerReturnSearchStringRule : Rule
    {
        public CustomerReturnSearchStringRule(MetaPopulation m) : base(m, new Guid("df58600c-e662-4110-bfb5-139b8f1102ce")) =>
            this.Patterns = new Pattern[]
            {
                m.Shipment.RolePattern(v => v.ShipmentState, m.CustomerReturn),
                m.Shipment.RolePattern(v => v.ShipmentMethod, m.CustomerReturn),
                m.Shipment.RolePattern(v => v.ShipmentNumber, m.CustomerReturn),
                m.Shipment.RolePattern(v => v.ShipFromParty, m.CustomerReturn),
                m.Party.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipFromParty.Shipment, m.CustomerReturn),
                m.Shipment.RolePattern(v => v.ShipFromAddress, m.CustomerReturn),
                m.PostalAddress.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipFromAddress.Shipment, m.CustomerReturn),
                m.Shipment.RolePattern(v => v.ShipFromContactPerson, m.CustomerReturn),
                m.Person.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipFromContactPerson.Shipment, m.CustomerReturn),
                m.Shipment.RolePattern(v => v.ShipFromFacility, m.CustomerReturn),
                m.Facility.RolePattern(v => v.Name, v => v.ShipmentsWhereShipFromFacility.Shipment, m.CustomerReturn),
                m.Party.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipToParty.Shipment, m.CustomerReturn),
                m.Shipment.RolePattern(v => v.ShipToAddress, m.CustomerReturn),
                m.PostalAddress.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipToAddress.Shipment, m.CustomerReturn),
                m.Shipment.RolePattern(v => v.ShipToContactPerson, m.CustomerReturn),
                m.Person.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipToContactPerson.Shipment, m.CustomerReturn),
                m.Shipment.RolePattern(v => v.ShipToFacility, m.CustomerReturn),
                m.Facility.RolePattern(v => v.Name, v => v.ShipmentsWhereShipToFacility.Shipment, m.CustomerReturn),
                m.Shipment.RolePattern(v => v.Carrier, m.CustomerReturn),
                m.Carrier.RolePattern(v => v.Name, v => v.ShipmentsWhereCarrier.Shipment, m.CustomerReturn),
                m.Shipment.RolePattern(v => v.HandlingInstruction, m.CustomerReturn),
                m.Shipment.RolePattern(v => v.Store, m.CustomerReturn),
                m.Store.RolePattern(v => v.Name, v => v.ShipmentsWhereStore.Shipment, m.CustomerReturn),

                m.ShipmentItem.RolePattern(v => v.ShipmentItemState, v => v.ShipmentWhereShipmentItem.Shipment, m.CustomerReturn),
                m.ShipmentItem.RolePattern(v => v.Part, v => v.ShipmentWhereShipmentItem.Shipment, m.CustomerReturn),
                m.Part.RolePattern(v => v.DisplayName, v => v.ShipmentItemsWherePart.ShipmentItem.ShipmentWhereShipmentItem.Shipment, m.CustomerReturn),
                m.ShipmentItem.RolePattern(v => v.Good, v => v.ShipmentWhereShipmentItem.Shipment, m.CustomerReturn),
                m.Good.RolePattern(v => v.DisplayName, v => v.ShipmentItemsWhereGood.ShipmentItem.ShipmentWhereShipmentItem.Shipment, m.CustomerReturn),
                m.ShipmentItem.RolePattern(v => v.ContentsDescription, v => v.ShipmentWhereShipmentItem.Shipment, m.CustomerReturn),
                m.ShipmentItem.RolePattern(v => v.SerialisedItem, v => v.ShipmentWhereShipmentItem.Shipment, m.CustomerReturn),
                m.SerialisedItem.RolePattern(v => v.DisplayName, v => v.ShipmentItemsWhereSerialisedItem.ShipmentItem.ShipmentWhereShipmentItem.Shipment, m.CustomerReturn),
                m.ShipmentItem.RolePattern(v => v.StoredInFacility, v => v.ShipmentWhereShipmentItem.Shipment, m.CustomerReturn),
                m.Facility.RolePattern(v => v.Name, v => v.ShipmentItemsWhereStoredInFacility.ShipmentItem.ShipmentWhereShipmentItem.Shipment, m.CustomerReturn),

                m.Shipment.AssociationPattern(v => v.SalesInvoicesWhereShipment, m.CustomerReturn),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<CustomerReturn>())
            {
                @this.DeriveCustomerReturnSearchString(validation);
            }
        }
    }

    public static class CustomerReturnSearchStringRuleExtensions
    {
        public static void DeriveCustomerReturnSearchString(this CustomerReturn @this, IValidation validation)
        {
            var array = new string[] {
                    @this.ShipmentState?.Name,
                    @this.ShipmentMethod?.Name,
                    @this.ShipmentNumber,
                    @this.ShipFromParty?.DisplayName,
                    @this.ShipFromAddress?.DisplayName,
                    @this.ShipFromContactPerson?.DisplayName,
                    @this.ShipFromFacility?.Name,
                    @this.ShipToParty?.DisplayName,
                    @this.ShipToAddress?.DisplayName,
                    @this.ShipToContactPerson?.DisplayName,
                    @this.ShipToFacility?.Name,
                    @this.Carrier?.Name,
                    @this.HandlingInstruction,
                    @this.Store?.Name,
                    @this.ExistShipmentItems ? string.Join(" ", @this.ShipmentItems?.Select(v => v.ShipmentItemState?.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistShipmentItems ? string.Join(" ", @this.ShipmentItems?.Select(v => v.Part?.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistShipmentItems ? string.Join(" ", @this.ShipmentItems?.Select(v => v.Good?.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistShipmentItems ? string.Join(" ", @this.ShipmentItems?.Select(v => v.ContentsDescription ?? string.Empty).ToArray()) : null,
                    @this.ExistShipmentItems ? string.Join(" ", @this.ShipmentItems?.Select(v => v.SerialisedItem?.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistShipmentItems ? string.Join(" ", @this.ShipmentItems?.Select(v => v.StoredInFacility?.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistSalesInvoicesWhereShipment ? string.Join(" ", @this.SalesInvoicesWhereShipment?.Select(v => v.InvoiceNumber ?? string.Empty).ToArray()) : null,
                };

            if (array.Any(s => !string.IsNullOrEmpty(s)))
            {
                @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
        }
    }
}
