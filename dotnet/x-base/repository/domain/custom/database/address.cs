// <copyright file="Address.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("130aa2ff-4f14-4ad7-8a27-f80e8aebfa00")]
    #endregion
    [Plural("Addresses")]
    public partial interface Address : Object
    {
        #region Allors
        [Id("36e7d935-a9c7-484d-8551-9bdc5bdeab68")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        #endregion
        Place Place { get; set; }
    }
}
