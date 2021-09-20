// <copyright file="PriceComponent.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("383589fb-f410-4d22-ade6-aa5126fdef18")]
    #endregion
    public partial interface PriceComponent : Period, Commentable, Deletable, Auditable
    {
        #region Allors
        [Id("B4C737AF-9305-4586-984C-5B69F13CF1E6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        Party PricedBy { get; set; }

        #region Allors
        [Id("18cda5a7-6720-4133-a71b-ce23e9ebc1bb")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        GeographicBoundary GeographicBoundary { get; set; }

        #region Allors
        [Id("1cddef96-0be9-487a-bdb3-df024656214a")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal Rate { get; set; }

        #region Allors
        [Id("3230c33b-42ac-4eb4-b0c9-9791cc5604d7")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        PartyClassification PartyClassification { get; set; }

        #region Allors
        [Id("50d6ddf3-47d9-4de1-954e-a4fae881edd0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        OrderQuantityBreak OrderQuantityBreak { get; set; }

        #region Allors
        [Id("55c43896-ba79-4752-8fd4-7fd8501d64b6")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Product Product { get; set; }

        #region Allors
        [Id("1712C7D7-A222-4D3F-BBD9-19F1A491E018")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Part Part { get; set; }

        #region Allors
        [Id("6c0744ee-b730-490d-bb0c-b6be95211371")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        ProductFeature ProductFeature { get; set; }

        #region Allors
        [Id("80379d5a-1831-4eed-abd3-a9574e3edd1d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        AgreementPricingProgram AgreementPricingProgram { get; set; }

        #region Allors
        [Id("8c32a2ca-a0c7-4c92-9b65-91d8b5ccee94")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Description { get; set; }

        #region Allors
        [Id("c2b2b046-9e62-4065-8f2d-10624f7565aa")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        Currency Currency { get; set; }

        #region Allors
        [Id("cb552b8b-f251-4c57-8cc7-8cc299631022")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        OrderKind OrderKind { get; set; }

        #region Allors
        [Id("dc1cf3af-2f22-43e6-863d-346e91aa2240")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        OrderValue OrderValue { get; set; }

        #region Allors
        [Id("dc5ad82b-c18d-4971-9689-e81475ed6a54")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal Price { get; set; }

        #region Allors
        [Id("de59dbb7-996a-45be-ae2a-a7b5a0ff3d94")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        ProductCategory ProductCategory { get; set; }

        #region Allors
        [Id("f8976686-bd76-4435-8ed8-f5e2490eeb94")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        SalesChannel SalesChannel { get; set; }
    }
}
