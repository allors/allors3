// <copyright file="Agreement.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("4deca253-7135-4ceb-b984-6adaf1515630")]
    #endregion
    public partial interface Agreement : UniquelyIdentifiable, Period, Deletable
    {
        #region Allors
        [Id("2ddce7b3-c763-45ea-8e1b-5ef8a0ea8e4a")]
        #endregion
        DateTime AgreementDate { get; set; }

        #region Allors
        [Id("34f0e272-7c56-4d92-a187-c40d9d907110")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        Addendum[] Addenda { get; set; }

        #region Allors
        [Id("6bdc1767-2bbf-40de-9c2c-a84d1b376a6e")]
        #endregion
        [Required]
        [Size(-1)]
        string Description { get; set; }

        #region Allors
        [Id("9d0e9ea7-31d7-4c01-96f2-97c3e17b3f18")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        AgreementTerm[] AgreementTerms { get; set; }

        #region Allors
        [Id("9f4db098-c486-4d88-9df9-cd7c79294575")]
        #endregion
        [Size(-1)]
        string Text { get; set; }

        #region Allors
        [Id("d5c90527-cae6-4a4f-9fd7-96f93dad59c7")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        AgreementItem[] AgreementItems { get; set; }

        #region Allors
        [Id("daff1ce2-4d60-426c-a45c-a82b63751657")]
        #endregion
        [Size(256)]
        string AgreementNumber { get; set; }
    }
}
