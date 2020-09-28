// <copyright file="PartCategory.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("27A26380-7007-4A18-8054-D7A446604452")]
    #endregion
    public partial class PartCategory : UniquelyIdentifiable, Deletable, Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        #endregion

        #region Allors
        [Id("5982BEDE-D784-4356-8523-9D1DD2B774BC")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public PartCategory PrimaryParent { get; set; }

        #region Allors
        [Id("67645C78-0428-4B8C-946E-1E1F9AABEA2C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Derived]
        public PartCategory[] PrimaryAncestors { get; set; }

        #region Allors
        [Id("71F448B7-9486-4A12-8495-C61307C41924")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        public PartCategory[] SecondaryParents { get; set; }

        #region Allors
        [Id("F005F74D-E7E4-4488-82A0-55C0384FF255")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        public PartCategory[] Children { get; set; }

        #region Allors
        [Id("80890B4C-80D7-438F-ADC6-F9078F2A2882")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        public PartCategory[] Descendants { get; set; }

        #region Allors
        [Id("D129AEC4-5345-40AC-BFBF-1AF849B21544")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace(Default)]
        public string Name { get; set; }

        #region Allors
        [Id("CF6C83F2-6CDC-4E9B-A757-79F8A71C2BD7")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public LocalisedText[] LocalisedNames { get; set; }

        #region Allors
        [Id("9D3D6A2A-3CA8-4A1F-B736-60268EF8F73B")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        public string Description { get; set; }

        #region Allors
        [Id("7E3C4257-E2E7-4C42-A51C-26B939E9524F")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public LocalisedText[] LocalisedDescriptions { get; set; }

        #region Allors
        [Id("90BA0958-81FD-4249-A2F4-53A12DA2F33A")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Media CategoryImage { get; set; }

        #region Allors
        [Id("FAC3039F-B0BF-44B6-8DEF-6CDF81886148")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        public Part[] Parts { get; set; }

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
