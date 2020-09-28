// <copyright file="Settings.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;

    #region Allors
    [Id("DC94D0BF-E08D-4B01-A91F-723CED6F3C36")]
    #endregion
    [Plural("Settingses")]

    public partial class Settings : Object
    {
        #region Allors
        [Id("9dee4a94-26d5-410f-a3e3-3fcde21c5c89")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Currency PreferredCurrency { get; set; }

        #region Allors
        [Id("a0fdc553-8081-43fa-ae1a-b9f7767d2d3e")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace]
        public Media NoImageAvailableImage { get; set; }

        #region Allors
        [Id("C84C214C-B6CA-4017-912D-954BAC0946D6")]
        [Indexed]
        #endregion
        [Workspace]
        [Multiplicity(Multiplicity.OneToOne)]
        public Counter SkuCounter { get; set; }

        #region Allors
        [Id("D306383F-B605-4635-8D06-DD3E4AF06FEF")]
        #endregion
        [Workspace]
        public string SkuPrefix { get; set; }

        #region Allors
        [Id("01E190C7-B91E-4A48-A251-6F3E625CD6D3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace]
        public Counter SerialisedItemCounter { get; set; }

        #region Allors
        [Id("F0B93DF3-E9E7-408D-980B-FB0889707FBE")]
        #endregion
        [Workspace]
        public string SerialisedItemPrefix { get; set; }

        #region Allors
        [Id("8438F903-1BFF-419D-9E89-D7A3943821D3")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace]
        public Counter ProductNumberCounter { get; set; }

        #region Allors
        [Id("E14816F1-65DA-4042-91E3-6F0906611D10")]
        #endregion
        [Workspace]
        public string ProductNumberPrefix { get; set; }

        #region Allors
        [Id("C1FA075A-2607-476D-BC27-A13656C56684")]
        #endregion
        [Workspace]
        [Required]
        public bool UseProductNumberCounter { get; set; }

        #region Allors
        [Id("5F85CAE6-B43C-400E-A2C0-D86FD7A080FA")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace]
        public Counter PartNumberCounter { get; set; }

        #region Allors
        [Id("FDFFDB77-D1DC-4479-8326-69722639E03B")]
        #endregion
        [Size(256)]
        [Workspace]
        public string PartNumberPrefix { get; set; }

        #region Allors
        [Id("840F8939-7CB8-4977-9BAC-A3375E50B3E6")]
        [Required]
        #endregion
        [Workspace]
        public bool UsePartNumberCounter { get; set; }

        #region Allors
        [Id("CC72A8ED-FE10-4350-ACAE-F88DF20E5AF4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Facility DefaultFacility { get; set; }

        /// <summary>
        /// Gets or Sets the InventoryStrategy used by this InternalOrganisation.
        /// </summary>
        #region Allors
        [Id("78D1D6C1-1F79-4B8A-9C85-F60C1A5594E4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public InventoryStrategy InventoryStrategy { get; set; }

        #region Allors
        [Id("791989EE-DAB9-42C6-B64C-6B07E8400C90")]
        #endregion
        [Workspace]
        public bool UseGlobalProductNumber { get; set; }

        #region Allors
        [Id("066c3053-b002-40c7-ab55-5cbe0a2a1cc5")]
        #endregion
        [Required]
        [Workspace]
        public decimal InternalLabourSurchargePercentage { get; set; }

        #region Allors
        [Id("8EA90F9A-A0D6-4FAB-ABCC-80AF7234F95E")]
        #endregion
        [Required]
        [Workspace]
        public decimal InternalPartSurchargePercentage { get; set; }

        #region Allors
        [Id("1B38C6A4-532D-4494-A621-69F3126801D3")]
        #endregion
        [Required]
        [Workspace]
        public decimal PartSurchargePercentage { get; set; }

        #region Allors
        [Id("ac0e5602-b102-4bd9-bf60-fd7da0a02dab")]
        #endregion
        [Required]
        [Workspace]
        public decimal InternalSubletSurchargePercentage { get; set; }

        #region Allors
        [Id("846B4409-D586-4AAA-9755-763D8726A739")]
        #endregion
        [Required]
        [Workspace]
        public decimal SubletSurchargePercentage { get; set; }

        #region inherited methods

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        #endregion
    }
}
