// <copyright file="StatementOfWork.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;

namespace Allors.Database.Domain
{
    public partial class StatementOfWork
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.StatementOfWork, this.M.StatementOfWork.QuoteState),
        };

        public void AppsDelete(DeletableDelete method)
        {
            foreach (var deletable in this.AllVersions)
            {
                deletable.Strategy.Delete();
            }
        }

        public void AppsPrint(PrintablePrint method)
        {
            var singleton = this.Strategy.Transaction.GetSingleton();
            var logo = this.Issuer?.ExistLogoImage == true ?
                            this.Issuer.LogoImage.MediaContent.Data :
                            singleton.LogoImage.MediaContent.Data;

            var images = new Dictionary<string, byte[]>
                                {
                                    { "Logo1", logo },
                                    { "Logo2", logo },
                                };

            if (this.ExistQuoteNumber)
            {
                var transaction = this.Strategy.Transaction;
                var barcodeGenerator = transaction.Database.Services.Get<IBarcodeGenerator>();
                var barcode = barcodeGenerator.Generate(this.QuoteNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                images.Add("Barcode", barcode);
            }

            var printModel = new Print.StatementOfWorkModel.Model(this, images);
            this.RenderPrintDocument(this.Issuer?.StatementOfWorkTemplate, printModel, images);

            this.PrintDocument.Media.InFileName = $"{this.QuoteNumber}.odt";

            method.StopPropagation = true;
        }
    }
}
