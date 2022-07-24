// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using Attributes;

    public partial interface Object
    {
        #region Allors
        [Id("B33F8EAE-17DC-4BF9-AFBB-E7FC38F42695")]
        #endregion
        void OnPreDerive();

        #region Allors
        [Id("C107F8B3-12DC-4FF9-8CBF-A7DEC046244F")]
        #endregion
        void OnDerive();
    }
}
