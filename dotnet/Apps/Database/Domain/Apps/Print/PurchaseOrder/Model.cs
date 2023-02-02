// <copyright file="Model.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Print.PurchaseOrderModel
{
    using System.Collections.Generic;
    using System.Linq;
    using Bogus.DataSets;

    public class Model
    {
        public Model(PurchaseOrder order, Dictionary<string, byte[]> images)
        {
            this.Order = new OrderModel(order);

            this.OrderedBy = new OrderedByModel((Organisation)order.OrderedBy);
            this.TakenVia = new TakenViaModel(order);
            this.ShipTo = new ShipToModel(order);

            this.OrderItems = order.PurchaseOrderItems.Select(v => new OrderItemModel(v, images)).ToArray();
        }

        public OrderModel Order { get; }

        public OrderedByModel OrderedBy { get; }

        public TakenViaModel TakenVia { get; }

        public ShipToModel ShipTo { get; }

        public OrderItemModel[] OrderItems { get; }
    }
}
