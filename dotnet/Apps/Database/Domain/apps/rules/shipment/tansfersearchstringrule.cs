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

    public class TransferSearchStringRule : Rule
    {
        public TransferSearchStringRule(MetaPopulation m) : base(m, new Guid("464ab96c-e193-42d5-8244-aab5875f63f4")) =>
            this.Patterns = new Pattern[]
            {
                m.Shipment.RolePattern(v => v.ShipmentState, m.Transfer),
                m.Shipment.RolePattern(v => v.ShipmentMethod, m.Transfer),
                m.Shipment.RolePattern(v => v.ShipmentNumber, m.Transfer),
                m.Shipment.RolePattern(v => v.ShipFromParty, m.Transfer),
                m.Party.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipFromParty.Shipment, m.Transfer),
                m.Shipment.RolePattern(v => v.ShipFromAddress, m.Transfer),
                m.PostalAddress.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipFromAddress.Shipment, m.Transfer),
                m.Shipment.RolePattern(v => v.ShipFromContactPerson, m.Transfer),
                m.Person.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipFromContactPerson.Shipment, m.Transfer),
                m.Shipment.RolePattern(v => v.ShipFromFacility, m.Transfer),
                m.Facility.RolePattern(v => v.Name, v => v.ShipmentsWhereShipFromFacility.Shipment, m.Transfer),
                m.Party.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipToParty.Shipment, m.Transfer),
                m.Shipment.RolePattern(v => v.ShipToAddress, m.Transfer),
                m.PostalAddress.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipToAddress.Shipment, m.Transfer),
                m.Shipment.RolePattern(v => v.ShipToContactPerson, m.Transfer),
                m.Person.RolePattern(v => v.DisplayName, v => v.ShipmentsWhereShipToContactPerson.Shipment, m.Transfer),
                m.Shipment.RolePattern(v => v.ShipToFacility, m.Transfer),
                m.Facility.RolePattern(v => v.Name, v => v.ShipmentsWhereShipToFacility.Shipment, m.Transfer),
                m.Shipment.RolePattern(v => v.Carrier, m.Transfer),
                m.Carrier.RolePattern(v => v.Name, v => v.ShipmentsWhereCarrier.Shipment, m.Transfer),
                m.Shipment.RolePattern(v => v.HandlingInstruction, m.Transfer),
                m.Shipment.RolePattern(v => v.Store, m.Transfer),
                m.Store.RolePattern(v => v.Name, v => v.ShipmentsWhereStore.Shipment, m.Transfer),

                m.ShipmentItem.RolePattern(v => v.ShipmentItemState, v => v.ShipmentWhereShipmentItem.Shipment, m.Transfer),
                m.ShipmentItem.RolePattern(v => v.Part, v => v.ShipmentWhereShipmentItem.Shipment, m.Transfer),
                m.Part.RolePattern(v => v.DisplayName, v => v.ShipmentItemsWherePart.ShipmentItem.ShipmentWhereShipmentItem.Shipment, m.Transfer),
                m.ShipmentItem.RolePattern(v => v.Good, v => v.ShipmentWhereShipmentItem.Shipment, m.Transfer),
                m.Good.RolePattern(v => v.DisplayName, v => v.ShipmentItemsWhereGood.ShipmentItem.ShipmentWhereShipmentItem.Shipment, m.Transfer),
                m.ShipmentItem.RolePattern(v => v.ContentsDescription, v => v.ShipmentWhereShipmentItem.Shipment, m.Transfer),
                m.ShipmentItem.RolePattern(v => v.SerialisedItem, v => v.ShipmentWhereShipmentItem.Shipment, m.Transfer),
                m.SerialisedItem.RolePattern(v => v.DisplayName, v => v.ShipmentItemsWhereSerialisedItem.ShipmentItem.ShipmentWhereShipmentItem.Shipment, m.Transfer),
                m.ShipmentItem.RolePattern(v => v.StoredInFacility, v => v.ShipmentWhereShipmentItem.Shipment, m.Transfer),
                m.Facility.RolePattern(v => v.Name, v => v.ShipmentItemsWhereStoredInFacility.ShipmentItem.ShipmentWhereShipmentItem.Shipment, m.Transfer),

                m.Shipment.AssociationPattern(v => v.SalesInvoicesWhereShipment, m.Transfer),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Transfer>())
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
                    string.Join(" ", @this.ShipmentItems?.Select(v => v.ShipmentItemState?.Name)),
                    string.Join(" ", @this.ShipmentItems?.Select(v => v.Part?.DisplayName)),
                    string.Join(" ", @this.ShipmentItems?.Select(v => v.Good?.DisplayName)),
                    string.Join(" ", @this.ShipmentItems?.Select(v => v.ContentsDescription)),
                    string.Join(" ", @this.ShipmentItems?.Select(v => v.SerialisedItem?.DisplayName)),
                    string.Join(" ", @this.ShipmentItems?.Select(v => v.StoredInFacility?.Name)),
                    string.Join(" ", @this.SalesInvoicesWhereShipment?.Select(v => v.InvoiceNumber)),
                };

                @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
        }
    }
}
