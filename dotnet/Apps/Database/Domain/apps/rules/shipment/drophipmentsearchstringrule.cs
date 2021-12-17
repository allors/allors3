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

    public class DropShipmentSearchStringRule : Rule
    {
        public DropShipmentSearchStringRule(MetaPopulation m) : base(m, new Guid("f8313a78-5d46-4e01-854c-af7d43fc878d")) =>
            this.Patterns = new Pattern[]
            {
                m.Shipment.RolePattern(v => v.ShipmentState, m.DropShipment),
                m.Shipment.RolePattern(v => v.ShipmentMethod, m.DropShipment),
                m.Shipment.RolePattern(v => v.ShipmentNumber, m.DropShipment),
                m.Shipment.RolePattern(v => v.ShipFromParty, m.DropShipment),
                m.Party.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipFromParty.Shipment, m.DropShipment),
                m.Shipment.RolePattern(v => v.ShipFromAddress, m.DropShipment),
                m.PostalAddress.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipFromAddress.Shipment, m.DropShipment),
                m.Shipment.RolePattern(v => v.ShipFromContactPerson, m.DropShipment),
                m.Person.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipFromContactPerson.Shipment, m.DropShipment),
                m.Shipment.RolePattern(v => v.ShipFromFacility, m.DropShipment),
                m.Facility.RolePattern(v => v.Name, v => v.ShipmentsWhereShipFromFacility.Shipment, m.DropShipment),
                m.Party.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipToParty.Shipment, m.DropShipment),
                m.Shipment.RolePattern(v => v.ShipToAddress, m.DropShipment),
                m.PostalAddress.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipToAddress.Shipment, m.DropShipment),
                m.Shipment.RolePattern(v => v.ShipToContactPerson, m.DropShipment),
                m.Person.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipToContactPerson.Shipment, m.DropShipment),
                m.Shipment.RolePattern(v => v.ShipToFacility, m.DropShipment),
                m.Facility.RolePattern(v => v.Name, v => v.ShipmentsWhereShipToFacility.Shipment, m.DropShipment),
                m.Shipment.RolePattern(v => v.Carrier, m.DropShipment),
                m.Carrier.RolePattern(v => v.Name, v => v.ShipmentsWhereCarrier.Shipment, m.DropShipment),
                m.Shipment.RolePattern(v => v.HandlingInstruction, m.DropShipment),
                m.Shipment.RolePattern(v => v.Store, m.DropShipment),
                m.Store.RolePattern(v => v.Name, v => v.ShipmentsWhereStore.Shipment, m.DropShipment),

                m.ShipmentItem.RolePattern(v => v.ShipmentItemState, v => v.ShipmentWhereShipmentItem.Shipment, m.DropShipment),
                m.ShipmentItem.RolePattern(v => v.Part, v => v.ShipmentWhereShipmentItem.Shipment, m.DropShipment),
                m.Part.RolePattern(v => v.DisplayName, v => v.ShipmentItemsWherePart.ShipmentItem.ShipmentWhereShipmentItem.Shipment, m.DropShipment),
                m.ShipmentItem.RolePattern(v => v.Good, v => v.ShipmentWhereShipmentItem.Shipment, m.DropShipment),
                m.Good.RolePattern(v => v.DisplayName, v => v.ShipmentItemsWhereGood.ShipmentItem.ShipmentWhereShipmentItem.Shipment, m.DropShipment),
                m.ShipmentItem.RolePattern(v => v.ContentsDescription, v => v.ShipmentWhereShipmentItem.Shipment, m.DropShipment),
                m.ShipmentItem.RolePattern(v => v.SerialisedItem, v => v.ShipmentWhereShipmentItem.Shipment, m.DropShipment),
                m.SerialisedItem.RolePattern(v => v.DisplayName, v => v.ShipmentItemsWhereSerialisedItem.ShipmentItem.ShipmentWhereShipmentItem.Shipment, m.DropShipment),
                m.ShipmentItem.RolePattern(v => v.StoredInFacility, v => v.ShipmentWhereShipmentItem.Shipment, m.DropShipment),
                m.Facility.RolePattern(v => v.Name, v => v.ShipmentItemsWhereStoredInFacility.ShipmentItem.ShipmentWhereShipmentItem.Shipment, m.DropShipment),

                m.Shipment.AssociationPattern(v => v.SalesInvoicesWhereShipment, m.DropShipment),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<DropShipment>())
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
                    @this.ExistShipmentItems ? string.Join(" ", @this.ShipmentItems?.Select(v => v.ShipmentItemState?.Name)) : null,
                    @this.ExistShipmentItems ? string.Join(" ", @this.ShipmentItems?.Select(v => v.Part?.DisplayName)) : null,
                    @this.ExistShipmentItems ? string.Join(" ", @this.ShipmentItems?.Select(v => v.Good?.DisplayName)) : null,
                    @this.ExistShipmentItems ? string.Join(" ", @this.ShipmentItems?.Select(v => v.ContentsDescription)) : null,
                    @this.ExistShipmentItems ? string.Join(" ", @this.ShipmentItems?.Select(v => v.SerialisedItem?.DisplayName)) : null,
                    @this.ExistShipmentItems ? string.Join(" ", @this.ShipmentItems?.Select(v => v.StoredInFacility?.Name)) : null,
                    @this.ExistSalesInvoicesWhereShipment ? string.Join(" ", @this.SalesInvoicesWhereShipment?.Select(v => v.InvoiceNumber)) : null,
                };

                if (array.Any(s => !string.IsNullOrEmpty(s)))
                {
                    @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
                }
            }
        }
    }
}
