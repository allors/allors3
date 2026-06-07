// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuoteItemModel.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.Print.ProductQuoteModel
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Meta;
    using NonUnifiedGood = NonUnifiedGood;
    using QuoteItem = QuoteItem;
    using UnifiedGood = UnifiedGood;

    public class QuoteItemModel
    {
        public QuoteItemModel(QuoteItem item, Dictionary<string, byte[]> imageByImageName)
        {
            var transaction = item.Strategy.Transaction;
            var m = transaction.Database.Services.Get<MetaPopulation>();

            var product = item.Product;
            var serialisedItem = item.SerialisedItem;

            this.Reference = item.InvoiceItemType?.Name;
            this.Product = serialisedItem?.DisplayName?? product?.Name;
            this.Description = serialisedItem?.Description ?? product?.Description;
            this.Details = item.Details;
            this.Quantity = item.Quantity.ToString("0");
            // TODO: Where does the currency come from?
            var currency = "€";
            this.Price = Rounder.RoundDecimal(item.UnitPrice, 2).ToString("N2", new CultureInfo("nl-BE")) + " " + currency;
            this.UnitAmount = Rounder.RoundDecimal(item.UnitPrice, 2).ToString("N2", new CultureInfo("nl-BE")) + " " + currency;
            this.TotalAmount = Rounder.RoundDecimal(item.TotalExVat, 2).ToString("N2", new CultureInfo("nl-BE")) + " " + currency;

            this.Comment = item.Comment;

            if (product != null)
            {
                this.ProductCategory = string.Join(", ", product.ProductCategoriesWhereProduct.Select(v => v.Name));
            }

            var unifiedGood = product as UnifiedGood;
            var nonUnifiedGood = product as NonUnifiedGood;

            if (unifiedGood != null)
            {
                this.BrandName = unifiedGood.Brand?.Name;
                this.ModelName = unifiedGood.Model?.Name;
            }
            else if (nonUnifiedGood != null)
            {
                this.BrandName = nonUnifiedGood.Part?.Brand?.Name;
                this.ModelName = nonUnifiedGood.Part?.Model?.Name;
            }

            if (serialisedItem != null)
            {
                this.IdentificationNumber = serialisedItem.ItemNumber;
                this.Year = serialisedItem.ManufacturingYear.ToString();

                var hoursType = new SerialisedItemCharacteristicTypes(transaction).FindBy(m.SerialisedItemCharacteristicType.Name, "Hours");
                var hoursCharacteristic = serialisedItem.SerialisedItemCharacteristics.FirstOrDefault(v => v.SerialisedItemCharacteristicType.Equals(hoursType));
                if (hoursCharacteristic != null)
                {
                    this.Hours = $"{hoursCharacteristic.Value} {hoursType.UnitOfMeasure?.Abbreviation}";
                }

                if (serialisedItem.ExistPrimaryPhoto && serialisedItem.PrimaryPhoto.MediaContent?.Data is { } primaryPhoto)
                {
                    this.PrimaryPhotoName = $"{item.Id}_primaryPhoto";
                    imageByImageName.Add(this.PrimaryPhotoName, primaryPhoto);
                }

                if (serialisedItem.AdditionalPhotos.Any() && serialisedItem.AdditionalPhotos.ElementAt(0).MediaContent?.Data is { } secondaryPhoto1)
                {
                    this.SecondaryPhotoName1 = $"{item.Id}_secondaryPhoto1";
                    imageByImageName.Add(this.SecondaryPhotoName1, secondaryPhoto1);
                }

                if (serialisedItem.AdditionalPhotos.Count() > 1 && serialisedItem.AdditionalPhotos.ElementAt(1).MediaContent?.Data is { } secondaryPhoto2)
                {
                    this.SecondaryPhotoName2 = $"{item.Id}_secondaryPhoto2";
                    imageByImageName.Add(this.SecondaryPhotoName2, secondaryPhoto2);
                }
            }
            else if (product != null)
            {
                this.IdentificationNumber = product.ProductIdentifications.FirstOrDefault(v => v.ProductIdentificationType.Equals(new ProductIdentificationTypes(transaction).Good)).Identification;

                if (product.ExistPrimaryPhoto && product.PrimaryPhoto.MediaContent?.Data is { } primaryPhoto)
                {
                    this.PrimaryPhotoName = $"{item.Id}_primaryPhoto";
                    imageByImageName.Add(this.PrimaryPhotoName, primaryPhoto);
                }

                if (product.Photos.Any() && product.Photos.ElementAt(0).MediaContent?.Data is { } secondaryPhoto1)
                {
                    this.SecondaryPhotoName1 = $"{item.Id}_secondaryPhoto1";
                    imageByImageName.Add(this.SecondaryPhotoName1, secondaryPhoto1);
                }

                if (product.Photos.Count() > 1 && product.Photos.ElementAt(1).MediaContent?.Data is { } secondaryPhoto2)
                {
                    this.SecondaryPhotoName2 = $"{item.Id}_secondaryPhoto2";
                    imageByImageName.Add(this.SecondaryPhotoName2, secondaryPhoto2);
                }
            }
        }

        public string PrimaryPhotoName { get; set; }

        public string SecondaryPhotoName1 { get; set; }

        public string SecondaryPhotoName2 { get; set; }

        public string Reference { get; }

        public string Product { get; }

        public string Description { get; }

        public string Details { get; }

        public string Quantity { get; }

        public string Price { get; }

        public string UnitAmount { get; }

        public string TotalAmount { get; }

        public string Comment { get; }

        public string IdentificationNumber { get; }

        public string ProductCategory { get; }

        public string BrandName { get; }

        public string ModelName { get; }

        public string Year { get; }

        public string Hours { get; }
    }
}
