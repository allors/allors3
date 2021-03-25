// <copyright file="Product.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("56b79619-d04a-4924-96e8-e3e7be9faa09")]
    #endregion
    public partial interface Product : UnifiedProduct
    {
        #region Allors
        [Id("05a2e95a-e5f1-45bc-a8ca-4ebfad3290b5")]
        #endregion
        [Workspace(Default)]
        DateTime SupportDiscontinuationDate { get; set; }

        #region Allors
        [Id("0b283eb9-2972-47ae-80d8-1a7aa8f77673")]
        #endregion
        [Workspace(Default)]
        DateTime SalesDiscontinuationDate { get; set; }

        #region Allors
        [Id("28f34f5d-c98c-45f8-9534-ce9191587ac8")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Derived]
        [Indexed]
        PriceComponent[] VirtualProductPriceComponents { get; set; }

        #region Allors
        [Id("345aaf52-424a-4573-b77b-64708665822f")]
        #endregion
        [Size(256)]
        string IntrastatCode { get; set; }

        #region Allors
        [Id("4632101d-09d6-4a89-8bba-e02ac791f9ad")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Product ProductComplement { get; set; }

        #region Allors
        [Id("60bd113a-d6b9-4de9-bbff-2b5094ec4803")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        Product[] Variants { get; set; }

        #region Allors
        [Id("74fc9be0-8677-463c-b3b6-f0e7bb7478ba")]
        #endregion
        [Workspace(Default)]
        DateTime IntroductionDate { get; set; }

        #region Allors
        [Id("c018edeb-54e0-43d5-9bbd-bf68df1364de")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        [Indexed]
        EstimatedProductCost[] EstimatedProductCosts { get; set; }

        #region Allors
        [Id("e6f084e9-e6fe-49b8-940e-cda85e1dc1e0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        Product[] ProductObsolescences { get; set; }

        #region Allors
        [Id("29d3d43b-6332-4a13-830b-44ab828c357b")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        VatRegime VatRegime { get; set; }

        #region Allors
        [Id("f2abc02c-67a1-42b7-83f5-195841e58a6a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        PriceComponent[] BasePrices { get; set; }
    }
}
