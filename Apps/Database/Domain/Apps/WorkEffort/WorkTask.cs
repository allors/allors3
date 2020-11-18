// <copyright file="WorkTask.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using System.Linq;

    public partial class WorkTask
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.WorkTask, this.M.WorkTask.WorkEffortState),
        };

        public void AppsOnBuild(ObjectOnBuild method)
        {
            var internalOrganisations = new Organisations(this.Strategy.Session).Extent()
                .Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

            if (!this.ExistTakenBy && internalOrganisations.Count() == 1)
            {
                this.TakenBy = internalOrganisations.First();
            }
        }

        public void AppsPrint(PrintablePrint method)
        {
            if (!method.IsPrinted)
            {
                var singleton = this.Strategy.Session.GetSingleton();
                var logo = this.TakenBy?.ExistLogoImage == true ?
                               this.TakenBy.LogoImage.MediaContent.Data :
                               singleton.LogoImage.MediaContent.Data;

                var images = new Dictionary<string, byte[]>
                                 {
                                     { "Logo", logo },
                                 };

                if (this.ExistWorkEffortNumber)
                {
                    var session = this.Strategy.Session;
                    var barcodeService = session.Database.State().BarcodeGenerator;
                    var barcode = barcodeService.Generate(this.WorkEffortNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                    images["Barcode"] = barcode;
                }

                var model = new Print.WorkTaskModel.Model(this);
                this.RenderPrintDocument(this.TakenBy?.WorkTaskTemplate, model, images);

                this.PrintDocument.Media.InFileName = $"{this.WorkEffortNumber}.odt";
            }
        }
    }
}
