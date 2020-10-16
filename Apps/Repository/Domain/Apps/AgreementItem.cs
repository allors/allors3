// <copyright file="AgreementItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;

    #region Allors
    [Id("8ba98e1b-1d4d-46b1-bf27-bb2bf53501fd")]
    #endregion
    public partial interface AgreementItem : DelegatedAccessControlledObject
    {
        #region Allors
        [Id("3ad6eaac-8cc3-4738-9a5b-617386e296c8")]
        #endregion
        [Size(-1)]
        string Text { get; set; }

        #region Allors
        [Id("49d6363c-6006-4850-8a96-d87b9336ae59")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        Addendum[] Addenda { get; set; }

        #region Allors
        [Id("4bd76d2c-383e-460f-a597-4283addcbef8")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        AgreementItem[] Children { get; set; }

        #region Allors
        [Id("9431dbfa-c620-445a-914f-8f12d4734b8b")]
        #endregion
        [Required]
        [Size(-1)]
        string Description { get; set; }

        #region Allors
        [Id("cfa9c54b-4e9f-4bd2-897d-baf8fb32fa9c")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        AgreementTerm[] AgreementTerms { get; set; }
    }
}
