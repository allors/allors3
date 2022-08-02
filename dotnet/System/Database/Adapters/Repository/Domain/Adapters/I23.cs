// <copyright file="I23.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("29cb9717-2452-4da0-9a29-8bd5d815307a")]
    #endregion
    public partial interface I23 : Object
    {
        #region Allors
        [Id("0407c93e-f2ea-49e4-8779-44b42c554e60")]
        [Size(256)]
        #endregion
        string I23AllorsString { get; set; }
    }
}
