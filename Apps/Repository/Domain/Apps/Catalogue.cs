// <copyright file="Catalogue.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("974DCB55-4D12-460F-A45D-9EBCCA54DA0B")]
    #endregion
    public partial class Catalogue : UniquelyIdentifiable, Deletable, Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        #endregion

        #region Allors
        [Id("0547F14E-97D5-48FD-8C06-33994070021C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public InternalOrganisation InternalOrganisation { get; set; }

        #region Allors
        [Id("21F55EB3-4DC1-42C5-AB16-4C47DBCF0456")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace(Default)]
        public string Name { get; set; }

        #region Allors
        [Id("4B3D6E3A-29C9-463A-A733-7C2E71BA4AA6")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public LocalisedText[] LocalisedNames { get; set; }

        #region Allors
        [Id("72EFFC9B-3233-4E3A-AB7F-1E0FAA386DB9")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        [MediaType("text/markdown")]
        public string Description { get; set; }

        #region Allors
        [Id("C5D0293C-42E0-4754-A513-1DE6D886A392")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public LocalisedText[] LocalisedDescriptions { get; set; }

        #region Allors
        [Id("7D57369D-2B53-4F7F-863A-70C61D73903D")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Media CatalogueImage { get; set; }

        #region Allors
        [Id("902035D4-5333-448D-A161-3D2BC8B0F2F6")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public ProductCategory[] ProductCategories { get; set; }

        #region Allors
        [Id("6ED2A606-6937-4711-8750-7137D285FE35")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Required]
        public Scope CatScope { get; set; }
        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion

    }
}
