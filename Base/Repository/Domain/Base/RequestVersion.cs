// <copyright file="RequestVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;

    #region Allors
    [Id("CB830374-2F89-4911-9A33-98CE902741A8")]
    #endregion
    public partial interface RequestVersion : Version
    {
        #region Allors
        [Id("649F4856-6B08-4AC1-B4CB-87A1CCAFAAF8")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace]
        RequestState RequestState { get; set; }

        #region Allors
        [Id("80F4AD18-6905-4916-AAF6-4016948F1451")]
        #endregion
        [Workspace]
        [Size(-1)]
        string InternalComment { get; set; }

        #region Allors
        [Id("B1A51DCA-AA9E-49F7-A257-C17D70A3FAEE")]
        #endregion
        [Size(-1)]
        [Workspace]
        string Description { get; set; }

        #region Allors
        [Id("3BF82A10-6C93-4EB3-A3DD-615D8189712B")]
        #endregion
        [Required]
        [Workspace]
        DateTime RequestDate { get; set; }

        #region Allors
        [Id("28B2A495-04AA-42E2-807C-3D455C80BEC6")]
        #endregion
        [Workspace]
        DateTime RequiredResponseDate { get; set; }

        #region Allors
        [Id("A85396E8-4B3A-4B2F-AE61-EA8D87418DAD")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace]
        RequestItem[] RequestItems { get; set; }

        #region Allors
        [Id("8CC34A50-82AB-44CE-BE9B-64691ACDDCE9")]
        #endregion
        [Size(256)]
        [Workspace]
        string RequestNumber { get; set; }

        #region Allors
        [Id("9ECA802A-F588-4241-BC4C-8083D56DFE1E")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace]
        RespondingParty[] RespondingParties { get; set; }

        #region Allors
        [Id("7A6D4128-55C4-4E48-A3A0-07EF25ACF85E")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        Party Originator { get; set; }

        #region Allors
        [Id("A15D9E5E-2300-4618-B3F1-F572B1729231")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        Currency Currency { get; set; }

        #region Allors
        [Id("6ED70134-E2A9-4F73-A300-78DFC79F0004")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        ContactMechanism FullfillContactMechanism { get; set; }

        #region Allors
        [Id("8FCA340E-777B-4260-A6A9-8B7515D5FA5F")]
        #endregion
        [Workspace]
        string EmailAddress { get; set; }

        #region Allors
        [Id("FEA30E94-84F3-4E2D-B894-EED0829BA7DD")]
        #endregion
        [Workspace]
        string TelephoneNumber { get; set; }

        #region Allors
        [Id("CFCF2525-D88A-49B2-997F-4516BCF4364A")]
        #endregion
        [Workspace]
        string TelephoneCountryCode { get; set; }
    }
}
