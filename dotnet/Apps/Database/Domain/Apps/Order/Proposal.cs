// <copyright file="Proposal.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;

    public partial class Proposal
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.Proposal, this.M.Proposal.QuoteState),
        };

        public void AppsDelete(DeletableDelete method)
        {
            foreach (var deletable in this.AllVersions)
            {
                deletable.Strategy.Delete();
            }
        }

        private bool AppsNeedsApproval => false;

        public void AppsSetReadyForProcessing(ProposalSetReadyForProcessing method)
        {
            this.QuoteState = this.AppsNeedsApproval
                ? new QuoteStates(this.Strategy.Transaction).AwaitingApproval : new QuoteStates(this.Strategy.Transaction).InProcess;

            method.StopPropagation = true;
        }

        // printModel not implemented
        //public void AppsPrint(PrintablePrint method)
        //{
        //    var singleton = this.Strategy.Transaction.GetSingleton();
        //    var logo = this.Issuer?.ExistLogoImage == true ?
        //                    this.Issuer.LogoImage.MediaContent.Data :
        //                    singleton.LogoImage.MediaContent.Data;

        //    var images = new Dictionary<string, byte[]>
        //                        {
        //                            { "Logo1", logo },
        //                            { "Logo2", logo },
        //                        };

        //    if (this.ExistQuoteNumber)
        //    {
        //        var transaction = this.Strategy.Transaction;
        //        var barcodeGenerator = transaction.Database.Services.Get<IBarcodeGenerator>();
        //        var barcode = barcodeGenerator.Generate(this.QuoteNumber, BarcodeType.CODE_128, 320, 80, pure: true);
        //        images.Add("Barcode", barcode);
        //    }

        //    var printModel = new Print.ProposalModel.Model(this, images);
        //    this.RenderPrintDocument(this.Issuer?.ProposalTemplate, printModel, images);

        //    this.PrintDocument.Media.InFileName = $"{this.QuoteNumber}.odt";

        //    method.StopPropagation = true;
        //}
    }
}
