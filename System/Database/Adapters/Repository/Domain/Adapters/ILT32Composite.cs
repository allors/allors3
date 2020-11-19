// <copyright file="ILT32Composite.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("4f53e1e7-e88a-4161-969c-1fed0b3a24a2")]
    #endregion
    public partial interface ILT32Composite : Object
    {
        #region Allors
        [Id("be3fc71d-66d8-411f-ab5f-4ed91e437852")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        ILT32Composite Self3 { get; set; }

        #region Allors
        [Id("c03a8b50-7fd1-4304-9d45-2c699fcbee80")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        ILT32Composite Self2 { get; set; }

        #region Allors
        [Id("d0eeeb45-97a6-465e-9a05-7e0fa970a969")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        ILT32Composite Self1 { get; set; }
    }
}
