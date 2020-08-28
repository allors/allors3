// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class QuoteItemCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdQuoteItem = changeSet.Created.Select(session.Instantiate).OfType<QuoteItem>();

                foreach (var quoteItem in createdQuoteItem)
                {
                    if (quoteItem.InvoiceItemType.IsPartItem
                || quoteItem.InvoiceItemType.IsProductFeatureItem
                || quoteItem.InvoiceItemType.IsProductItem)
                    {
                        validation.AssertAtLeastOne(quoteItem, M.QuoteItem.Product, M.QuoteItem.ProductFeature, M.QuoteItem.SerialisedItem, M.QuoteItem.Deliverable, M.QuoteItem.WorkEffort);
                        validation.AssertExistsAtMostOne(quoteItem, M.QuoteItem.Product, M.QuoteItem.ProductFeature, M.QuoteItem.Deliverable, M.QuoteItem.WorkEffort);
                        validation.AssertExistsAtMostOne(quoteItem, M.QuoteItem.SerialisedItem, M.QuoteItem.ProductFeature, M.QuoteItem.Deliverable, M.QuoteItem.WorkEffort);
                    }
                    else
                    {
                        quoteItem.Quantity = 1;
                    }

                    if (quoteItem.ExistSerialisedItem && quoteItem.Quantity != 1)
                    {
                        validation.AddError($"{quoteItem} {quoteItem.Meta.Quantity} {ErrorMessages.SerializedItemQuantity}");
                    }

                    if (changeSet.IsCreated(quoteItem) && !quoteItem.ExistDetails)
                    {
                        quoteItem.DeriveDetails();
                    }

                    if (quoteItem.ExistRequestItem)
                    {
                        quoteItem.RequiredByDate = quoteItem.RequestItem.RequiredByDate;
                    }

                    if (!quoteItem.ExistUnitOfMeasure)
                    {
                        quoteItem.UnitOfMeasure = new UnitsOfMeasure(quoteItem.Strategy.Session).Piece;
                    }

                    quoteItem.UnitVat = quoteItem.ExistVatRate ? quoteItem.UnitPrice * quoteItem.VatRate.Rate / 100 : 0;

                    // Calculate Totals
                    quoteItem.TotalBasePrice = quoteItem.UnitBasePrice * quoteItem.Quantity;
                    quoteItem.TotalDiscount = quoteItem.UnitDiscount * quoteItem.Quantity;
                    quoteItem.TotalSurcharge = quoteItem.UnitSurcharge * quoteItem.Quantity;

                    if (quoteItem.TotalBasePrice > 0)
                    {
                        quoteItem.TotalDiscountAsPercentage = Math.Round(quoteItem.TotalDiscount / quoteItem.TotalBasePrice * 100, 2);
                        quoteItem.TotalSurchargeAsPercentage = Math.Round(quoteItem.TotalSurcharge / quoteItem.TotalBasePrice * 100, 2);
                    }
                    else
                    {
                        quoteItem.TotalDiscountAsPercentage = 0;
                        quoteItem.TotalSurchargeAsPercentage = 0;
                    }

                    quoteItem.TotalExVat = quoteItem.UnitPrice * quoteItem.Quantity;
                    quoteItem.TotalVat = quoteItem.UnitVat * quoteItem.Quantity;
                    quoteItem.TotalIncVat = quoteItem.TotalExVat + quoteItem.TotalVat;

                    // CurrentVersion is Previous Version until PostDerive
                    var previousSerialisedItem = quoteItem.CurrentVersion?.SerialisedItem;
                    if (previousSerialisedItem != null && previousSerialisedItem != quoteItem.SerialisedItem)
                    {
                        previousSerialisedItem.DerivationTrigger = Guid.NewGuid();
                    }

                    if (!quoteItem.ExistUnitPrice)
                    {
                        validation.AddError($"{quoteItem} {quoteItem.Meta.UnitPrice} {ErrorMessages.UnitPriceRequired}");
                    }

                    var deletePermission = new Permissions(quoteItem.Strategy.Session).Get(quoteItem.Meta.ObjectType, quoteItem.Meta.Delete, Operations.Execute);
                    if (quoteItem.IsDeletable)
                    {
                        quoteItem.RemoveDeniedPermission(deletePermission);
                    }
                    else
                    {
                        quoteItem.AddDeniedPermission(deletePermission);
                    }
                }
            }
        }

        public static void QuoteItemRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("10757e42-342e-4734-b797-bc6a8745a0cd")] = new QuoteItemCreationDerivation();
        }
    }
}
