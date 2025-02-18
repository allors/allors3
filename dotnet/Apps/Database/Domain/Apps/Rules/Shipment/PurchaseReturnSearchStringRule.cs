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

    public class PurchaseReturnSearchStringRule : Rule
    {
        public PurchaseReturnSearchStringRule(MetaPopulation m) : base(m, new Guid("f8313a78-5d46-4e01-854c-af7d43fc878d")) =>
            this.Patterns = new Pattern[]
            {
                m.Shipment.RolePattern(v => v.ShipmentState, m.PurchaseReturn),
                m.Shipment.RolePattern(v => v.ShipmentMethod, m.PurchaseReturn),
                m.Shipment.RolePattern(v => v.ShipmentNumber, m.PurchaseReturn),
                m.Shipment.RolePattern(v => v.ShipFromParty, m.PurchaseReturn),
                m.Party.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipFromParty.ObjectType, m.PurchaseReturn),
                m.Shipment.RolePattern(v => v.ShipFromAddress, m.PurchaseReturn),
                m.PostalAddress.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipFromAddress.ObjectType, m.PurchaseReturn),
                m.Shipment.RolePattern(v => v.ShipFromContactPerson, m.PurchaseReturn),
                m.Person.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipFromContactPerson.ObjectType, m.PurchaseReturn),
                m.Shipment.RolePattern(v => v.ShipFromFacility, m.PurchaseReturn),
                m.Facility.RolePattern(v => v.Name, v => v.ShipmentsWhereShipFromFacility.ObjectType, m.PurchaseReturn),
                m.Party.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipToParty.ObjectType, m.PurchaseReturn),
                m.Shipment.RolePattern(v => v.ShipToAddress, m.PurchaseReturn),
                m.PostalAddress.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipToAddress.ObjectType, m.PurchaseReturn),
                m.Shipment.RolePattern(v => v.ShipToContactPerson, m.PurchaseReturn),
                m.Person.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipToContactPerson.ObjectType, m.PurchaseReturn),
                m.Shipment.RolePattern(v => v.ShipToFacility, m.PurchaseReturn),
                m.Facility.RolePattern(v => v.Name, v => v.ShipmentsWhereShipToFacility.ObjectType, m.PurchaseReturn),
                m.Shipment.RolePattern(v => v.Carrier, m.PurchaseReturn),
                m.Carrier.RolePattern(v => v.Name, v => v.ShipmentsWhereCarrier.ObjectType, m.PurchaseReturn),
                m.Shipment.RolePattern(v => v.HandlingInstruction, m.PurchaseReturn),
                m.Shipment.RolePattern(v => v.Store, m.PurchaseReturn),
                m.Store.RolePattern(v => v.Name, v => v.ShipmentsWhereStore.ObjectType, m.PurchaseReturn),

                m.ShipmentItem.RolePattern(v => v.ShipmentItemState, v => v.ShipmentWhereShipmentItem.ObjectType, m.PurchaseReturn),
                m.ShipmentItem.RolePattern(v => v.Part, v => v.ShipmentWhereShipmentItem.ObjectType, m.PurchaseReturn),
                m.Part.RolePattern(v => v.DisplayName, v => v.ShipmentItemsWherePart.ObjectType.ShipmentWhereShipmentItem.ObjectType, m.PurchaseReturn),
                m.ShipmentItem.RolePattern(v => v.Good, v => v.ShipmentWhereShipmentItem.ObjectType, m.PurchaseReturn),
                m.Good.RolePattern(v => v.DisplayName, v => v.ShipmentItemsWhereGood.ObjectType.ShipmentWhereShipmentItem.ObjectType, m.PurchaseReturn),
                m.ShipmentItem.RolePattern(v => v.ContentsDescription, v => v.ShipmentWhereShipmentItem.ObjectType, m.PurchaseReturn),
                m.ShipmentItem.RolePattern(v => v.SerialisedItem, v => v.ShipmentWhereShipmentItem.ObjectType, m.PurchaseReturn),
                m.SerialisedItem.RolePattern(v => v.DisplayName, v => v.ShipmentItemsWhereSerialisedItem.ObjectType.ShipmentWhereShipmentItem.ObjectType, m.PurchaseReturn),
                m.ShipmentItem.RolePattern(v => v.StoredInFacility, v => v.ShipmentWhereShipmentItem.ObjectType, m.PurchaseReturn),
                m.Facility.RolePattern(v => v.Name, v => v.ShipmentItemsWhereStoredInFacility.ObjectType.ShipmentWhereShipmentItem.ObjectType, m.PurchaseReturn),

                m.Shipment.AssociationPattern(v => v.SalesInvoicesWhereShipment, m.PurchaseReturn),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseReturn>())
            {
                @this.DerivePurchaseReturnSearchString(validation);
            }
        }
    }

    public static class PurchaseReturnSearchStringRuleExtensions
    {
        public static void DerivePurchaseReturnSearchString(this PurchaseReturn @this, IValidation validation)
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
            else
            {
                @this.RemoveSearchString();
            }
        }
    }
}
