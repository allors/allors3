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

    public class PurchaseShipmentSearchStringRule : Rule
    {
        public PurchaseShipmentSearchStringRule(MetaPopulation m) : base(m, new Guid("a9dd697a-d240-408c-85e2-b141318eb1fe")) =>
            this.Patterns = new Pattern[]
            {
                m.Shipment.RolePattern(v => v.ShipmentState, m.PurchaseShipment),
                m.Shipment.RolePattern(v => v.ShipmentMethod, m.PurchaseShipment),
                m.Shipment.RolePattern(v => v.ShipmentNumber, m.PurchaseShipment),
                m.Shipment.RolePattern(v => v.ShipFromParty, m.PurchaseShipment),
                m.Party.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipFromParty.ObjectType, m.PurchaseShipment),
                m.Shipment.RolePattern(v => v.ShipFromAddress, m.PurchaseShipment),
                m.PostalAddress.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipFromAddress.ObjectType, m.PurchaseShipment),
                m.Shipment.RolePattern(v => v.ShipFromContactPerson, m.PurchaseShipment),
                m.Person.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipFromContactPerson.ObjectType, m.PurchaseShipment),
                m.Shipment.RolePattern(v => v.ShipFromFacility, m.PurchaseShipment),
                m.Facility.RolePattern(v => v.Name, v => v.ShipmentsWhereShipFromFacility.ObjectType, m.PurchaseShipment),
                m.Party.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipToParty.ObjectType, m.PurchaseShipment),
                m.Shipment.RolePattern(v => v.ShipToAddress, m.PurchaseShipment),
                m.PostalAddress.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipToAddress.ObjectType, m.PurchaseShipment),
                m.Shipment.RolePattern(v => v.ShipToContactPerson, m.PurchaseShipment),
                m.Person.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipToContactPerson.ObjectType, m.PurchaseShipment),
                m.Shipment.RolePattern(v => v.ShipToFacility, m.PurchaseShipment),
                m.Facility.RolePattern(v => v.Name, v => v.ShipmentsWhereShipToFacility.ObjectType, m.PurchaseShipment),
                m.Shipment.RolePattern(v => v.Carrier, m.PurchaseShipment),
                m.Carrier.RolePattern(v => v.Name, v => v.ShipmentsWhereCarrier.ObjectType, m.PurchaseShipment),
                m.Shipment.RolePattern(v => v.HandlingInstruction, m.PurchaseShipment),
                m.Shipment.RolePattern(v => v.Store, m.PurchaseShipment),
                m.Store.RolePattern(v => v.Name, v => v.ShipmentsWhereStore.ObjectType, m.PurchaseShipment),

                m.ShipmentItem.RolePattern(v => v.ShipmentItemState, v => v.ShipmentWhereShipmentItem.ObjectType, m.PurchaseShipment),
                m.ShipmentItem.RolePattern(v => v.Part, v => v.ShipmentWhereShipmentItem.ObjectType, m.PurchaseShipment),
                m.Part.RolePattern(v => v.DisplayName, v => v.ShipmentItemsWherePart.ObjectType.ShipmentWhereShipmentItem.ObjectType, m.PurchaseShipment),
                m.ShipmentItem.RolePattern(v => v.Good, v => v.ShipmentWhereShipmentItem.ObjectType, m.PurchaseShipment),
                m.Good.RolePattern(v => v.DisplayName, v => v.ShipmentItemsWhereGood.ObjectType.ShipmentWhereShipmentItem.ObjectType, m.PurchaseShipment),
                m.ShipmentItem.RolePattern(v => v.ContentsDescription, v => v.ShipmentWhereShipmentItem.ObjectType, m.PurchaseShipment),
                m.ShipmentItem.RolePattern(v => v.SerialisedItem, v => v.ShipmentWhereShipmentItem.ObjectType, m.PurchaseShipment),
                m.SerialisedItem.RolePattern(v => v.DisplayName, v => v.ShipmentItemsWhereSerialisedItem.ObjectType.ShipmentWhereShipmentItem.ObjectType, m.PurchaseShipment),
                m.ShipmentItem.RolePattern(v => v.StoredInFacility, v => v.ShipmentWhereShipmentItem.ObjectType, m.PurchaseShipment),
                m.Facility.RolePattern(v => v.Name, v => v.ShipmentItemsWhereStoredInFacility.ObjectType.ShipmentWhereShipmentItem.ObjectType, m.PurchaseShipment),

                m.Shipment.AssociationPattern(v => v.SalesInvoicesWhereShipment, m.PurchaseShipment),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseShipment>())
            {
                @this.DerivePurchaseShipmentSearchString(validation);
            }
        }
    }

    public static class PurchaseShipmentSearchStringRuleExtensions
    {
        public static void DerivePurchaseShipmentSearchString(this PurchaseShipment @this, IValidation validation)
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
