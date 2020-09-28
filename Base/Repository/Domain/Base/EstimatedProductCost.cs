// <copyright file="EstimatedProductCost.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("c8df7ac5-4e6f-4add-981f-f0d9a8c14e24")]
    #endregion
    public partial interface EstimatedProductCost : Period, Deletable, Object
    {
        #region Allors
        [Id("2a8f919f-19f0-4b33-b8b8-26937d49d298")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal Cost { get; set; }

        #region Allors
        [Id("78a7ee9c-4aeb-471d-ae17-5878737f1f67")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        Currency Currency { get; set; }

        #region Allors
        [Id("ce0f4392-cf76-49ba-a6bd-47b4e125ec61")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        Organisation Organisation { get; set; }

        #region Allors
        [Id("d5e63839-7009-4582-8d9a-ac9591aa10c9")]
        #endregion
        [Size(-1)]

        string Description { get; set; }

        #region Allors
        [Id("e7942246-0343-437e-9b92-fc2d5e6438fd")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        GeographicBoundary GeographicBoundary { get; set; }
    }
}
