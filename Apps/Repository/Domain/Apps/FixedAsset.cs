// <copyright file="FixedAsset.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("4a3efb9c-1556-4e57-bb59-f09d297e607e")]
    #endregion
    public partial interface FixedAsset : Commentable, Searchable
    {
        #region Allors
        [Id("354107ce-4eb6-4b9a-83c8-5cfe5e3adb22")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace(Default)]
        string Name { get; set; }

        #region Allors
        [Id("54EC72A4-B9AC-44A0-A29F-10DAC2F58DEC")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        LocalisedText[] LocalisedNames { get; set; }

        #region Allors
        [Id("51133e4d-5135-4991-9f2f-8df9762fac78")]
        #endregion
        [Workspace(Default)]
        DateTime LastServiceDate { get; set; }

        #region Allors
        [Id("54cf9225-9204-43ee-9984-7fd8b2cbf8bc")]
        #endregion
        [Workspace(Default)]
        DateTime AcquiredDate { get; set; }

        #region Allors
        [Id("725c6b7d-68ed-4576-8b17-eac4e9f4db83")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [MediaType("text/markdown")]
        string Description { get; set; }

        #region Allors
        [Id("16B8D296-15ED-47F3-8278-C59ED863E0F8")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        LocalisedText[] LocalisedDescriptions { get; set; }

        #region Allors
        [Id("913cc338-f844-49ae-886a-2e32db190b78")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        decimal ProductionCapacity { get; set; }

        #region Allors
        [Id("ead0e86a-dfc7-45b0-9865-b973175c4567")]
        #endregion
        [Workspace(Default)]
        DateTime NextServiceDate { get; set; }

        #region Allors
        [Id("822F98D5-D9B2-44DA-B097-911244609066")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        string Keywords { get; set; }

        #region Allors
        [Id("EAC7725D-7B6A-48E1-95D1-3D9B17F5FDA8")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        LocalisedText[] LocalisedKeywords { get; set; }

        #region Allors
        [Id("31c78290-83ff-469b-bbc5-8b9c2c7cbdf3")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        Media[] PublicElectronicDocuments { get; set; }

        #region Allors
        [Id("681b22d3-5338-4971-89e1-7f812883b728")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        LocalisedMedia[] PublicLocalisedElectronicDocuments { get; set; }

        #region Allors
        [Id("6835b1ee-427b-4fb0-acc4-e5572be2a855")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        Media[] PrivateElectronicDocuments { get; set; }

        #region Allors
        [Id("cfd10526-8ba6-4194-ad9d-12bbaedcc1ce")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        LocalisedMedia[] PrivateLocalisedElectronicDocuments { get; set; }
    }
}
