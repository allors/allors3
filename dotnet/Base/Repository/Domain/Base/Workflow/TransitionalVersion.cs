// <copyright file="TransitionalVersion.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("A13C9057-8786-40CA-8421-476E55787D73")]
    #endregion
    public partial interface TransitionalVersion : Object
    {
        #region Allors
        [Id("96685F17-ABE3-459C-BF9F-8C5F05788C04")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        ObjectState[] PreviousObjectStates { get; set; }

        #region Allors
        [Id("39C43EB4-AA16-4CF8-93A0-60066CB746E8")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        ObjectState[] LastObjectStates { get; set; }

        #region Allors
        [Id("F2472C1F-8D2A-4400-B372-34C2B03207B6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        ObjectState[] ObjectStates { get; set; }
    }
}
