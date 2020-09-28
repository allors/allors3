// <copyright file="ProductFeature.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("d3c5a482-e17a-4e37-84eb-55a035e80f2f")]
    #endregion
    public partial interface ProductFeature : UniquelyIdentifiable, Object
    {
        #region Allors
        [Id("4a8c511a-8146-4d6d-bc35-d8d6b8f1786d")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        EstimatedProductCost[] EstimatedProductCosts { get; set; }

        #region Allors
        [Id("8ac8ab84-f78f-4232-a4f7-390f55019663")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        PriceComponent[] BasePrices { get; set; }

        #region Allors
        [Id("b75855b8-c921-4d60-8ea0-650a0f574f7f")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Description { get; set; }

        #region Allors
        [Id("badde93b-4691-435e-9ba3-e52435e9f574")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        ProductFeature[] DependentFeatures { get; set; }

        #region Allors
        [Id("ce228118-f5b2-49bb-b0cd-7e0ca8e10315")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        ProductFeature[] IncompatibleFeatures { get; set; }

        #region Allors
        [Id("efe16e22-edfb-40b1-83c0-110f874c285a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        VatRate VatRate { get; set; }
    }
}
