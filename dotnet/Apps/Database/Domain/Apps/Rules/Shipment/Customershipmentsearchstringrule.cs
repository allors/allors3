// <copyright file="InventoryItemSearchStringDerivation.cs" company="Allors bv">
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

    public class CustomerShipmentSearchStringRule : Rule
    {
        public CustomerShipmentSearchStringRule(MetaPopulation m) : base(m, new Guid("89ebf89e-8d36-45c1-8ff6-09385fb2df19")) =>
            this.Patterns = new Pattern[]
            {
                m.Shipment.RolePattern(v => v.ShipmentState, m.CustomerShipment),
                m.Shipment.RolePattern(v => v.ShipmentMethod, m.CustomerShipment),
                m.Shipment.RolePattern(v => v.ShipmentNumber, m.CustomerShipment),
                m.Shipment.RolePattern(v => v.ShipFromParty, m.CustomerShipment),
                m.Party.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipFromParty.ObjectType, m.CustomerShipment),
                m.Shipment.RolePattern(v => v.ShipFromAddress, m.CustomerShipment),
                m.PostalAddress.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipFromAddress.ObjectType, m.CustomerShipment),
                m.Shipment.RolePattern(v => v.ShipFromContactPerson, m.CustomerShipment),
                m.Person.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipFromContactPerson.ObjectType, m.CustomerShipment),
                m.Shipment.RolePattern(v => v.ShipFromFacility, m.CustomerShipment),
                m.Facility.RolePattern(v => v.Name, v => v.ShipmentsWhereShipFromFacility.ObjectType, m.CustomerShipment),
                m.Party.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipToParty.ObjectType, m.CustomerShipment),
                m.Shipment.RolePattern(v => v.ShipToAddress, m.CustomerShipment),
                m.PostalAddress.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipToAddress.ObjectType, m.CustomerShipment),
                m.Shipment.RolePattern(v => v.ShipToContactPerson, m.CustomerShipment),
                m.Person.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipToContactPerson.ObjectType, m.CustomerShipment),
                m.Shipment.RolePattern(v => v.ShipToFacility, m.CustomerShipment),
                m.Facility.RolePattern(v => v.Name, v => v.ShipmentsWhereShipToFacility.ObjectType, m.CustomerShipment),
                m.Shipment.RolePattern(v => v.Carrier, m.CustomerShipment),
                m.Carrier.RolePattern(v => v.Name, v => v.ShipmentsWhereCarrier.ObjectType, m.CustomerShipment),
                m.Shipment.RolePattern(v => v.HandlingInstruction, m.CustomerShipment),
                m.Shipment.RolePattern(v => v.Store, m.CustomerShipment),
                m.Store.RolePattern(v => v.Name, v => v.ShipmentsWhereStore.ObjectType, m.CustomerShipment),

                m.ShipmentItem.RolePattern(v => v.ShipmentItemState, v => v.ShipmentWhereShipmentItem.ObjectType, m.CustomerShipment),
                m.ShipmentItem.RolePattern(v => v.Part, v => v.ShipmentWhereShipmentItem.ObjectType, m.CustomerShipment),
                m.Part.RolePattern(v => v.DisplayName, v => v.ShipmentItemsWherePart.ObjectType.ShipmentWhereShipmentItem.ObjectType, m.CustomerShipment),
                m.ShipmentItem.RolePattern(v => v.Good, v => v.ShipmentWhereShipmentItem.ObjectType, m.CustomerShipment),
                m.Good.RolePattern(v => v.DisplayName, v => v.ShipmentItemsWhereGood.ObjectType.ShipmentWhereShipmentItem.ObjectType, m.CustomerShipment),
                m.ShipmentItem.RolePattern(v => v.ContentsDescription, v => v.ShipmentWhereShipmentItem.ObjectType, m.CustomerShipment),
                m.ShipmentItem.RolePattern(v => v.SerialisedItem, v => v.ShipmentWhereShipmentItem.ObjectType, m.CustomerShipment),
                m.SerialisedItem.RolePattern(v => v.DisplayName, v => v.ShipmentItemsWhereSerialisedItem.ObjectType.ShipmentWhereShipmentItem.ObjectType, m.CustomerShipment),
                m.ShipmentItem.RolePattern(v => v.StoredInFacility, v => v.ShipmentWhereShipmentItem.ObjectType, m.CustomerShipment),
                m.Facility.RolePattern(v => v.Name, v => v.ShipmentItemsWhereStoredInFacility.ObjectType.ShipmentWhereShipmentItem.ObjectType, m.CustomerShipment),

                m.Shipment.AssociationPattern(v => v.SalesInvoicesWhereShipment, m.CustomerShipment),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<CustomerShipment>())
            {
                @this.DeriveCustomerShipmentSearchString(validation);
            }
        }
    }

    public static class CustomerShipmentSearchStringRuleExtensions
    {
        public static void DeriveCustomerShipmentSearchString(this CustomerShipment @this, IValidation validation)
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
                    @this.ExistSalesInvoicesWhereShipment ? string.Join(" ", @this.SalesInvoicesWhereShipment?.Select(v => v.InvoiceNumber).ToArray()) : null,
                };

            if (array.Any(s => !string.IsNullOrEmpty(s)))
            {
                @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
            else
            {
                @this.RemoveSearchString();
            }
        }
    }
}
