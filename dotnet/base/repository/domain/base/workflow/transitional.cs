// <copyright file="Transitional.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("ab2179ad-9eac-4b61-8d84-81cd777c4926")]
    #endregion
    public partial interface Transitional : Object
    {
        #region Allors
        [Id("D9D86241-5FC7-4EDB-9FAA-FF5CA291F16C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        ObjectState[] PreviousObjectStates { get; set; }

        #region Allors
        [Id("2BC8AFDF-92BE-4088-9E35-C1C942CFE74B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        ObjectState[] LastObjectStates { get; set; }

        #region Allors
        [Id("52962C45-8A3E-4136-A968-C333CBE12685")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        ObjectState[] ObjectStates { get; set; }

        #region Allors
        [Id("02cd3896-ea9a-498c-8633-42a5df9c0b17")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        Revocation[] TransitionalRevocations { get; set; }
    }
}
