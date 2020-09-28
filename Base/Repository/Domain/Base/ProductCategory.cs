// <copyright file="ProductCategory.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("ea83087e-05cc-458c-a6ba-3ce947644a0f")]
    #endregion
    public partial class ProductCategory : UniquelyIdentifiable, Deletable, Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        #endregion

        #region Allors
        [Id("1F5775EF-9440-405B-8A7D-2A6460D8BCAF")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public InternalOrganisation InternalOrganisation { get; set; }

        #region Allors
        [Id("22b7b6ef-7adf-424d-a675-d5338478ed44")]
        #endregion
        [Indexed]
        [Size(256)]
        [Workspace(Default)]
        public string Code { get; set; }

        #region Allors
        [Id("511A0C9B-46C0-4ED8-8C6E-280FF4634076")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public ProductCategory PrimaryParent { get; set; }

        #region Allors
        [Id("29B96BB3-D121-405C-AFD5-90171729002E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        [Derived]
        public ProductCategory[] PrimaryAncestors { get; set; }

        #region Allors
        [Id("062A7D3C-74C9-46C8-9332-C239C13B4200")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        public ProductCategory[] PreviousSecondaryParents { get; set; }

        #region Allors
        [Id("16F70B37-079A-47AE-B883-8F1E1A9E345F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        public ProductCategory[] SecondaryParents { get; set; }

        #region Allors
        [Id("6ad49c7d-8c4e-455b-8073-a5ef72e92725")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        public ProductCategory[] Children { get; set; }

        #region Allors
        [Id("6AB3E5EC-FC02-4F2D-B5EB-4EFC50E2B33B")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        public ProductCategory[] Descendants { get; set; }

        #region Allors
        [Id("8af8b1b1-a711-4e98-a6a0-2948f2d1f315")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace(Default)]
        public string Name { get; set; }

        #region Allors
        [Id("97CB34DD-4E6A-4DCF-90B4-50071752B2D8")]
        #endregion
        [Workspace(Default)]
        public string DisplayName { get; set; }

        #region Allors
        [Id("0FB2F768-8313-450C-94AE-5F9C52B758E8")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public LocalisedText[] LocalisedNames { get; set; }

        #region Allors
        [Id("743985f3-cfee-45b5-b971-30adf46b5297")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [MediaType("text/markdown")]
        public string Description { get; set; }

        #region Allors
        [Id("40C3BD4D-C947-49F6-A5FA-A01398DB9E8A")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public LocalisedText[] LocalisedDescriptions { get; set; }

        #region Allors
        [Id("9f50cbbc-d0af-46e6-8e04-2bfb0bf1facf")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Media CategoryImage { get; set; }

        #region Allors
        [Id("7B219D9E-0234-4F34-884D-D092573F6172")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Required]
        [Multiplicity(Multiplicity.ManyToOne)]
        public CatScope CatScope { get; set; }

        #region Allors
        [Id("2C9927CA-BA9C-4F4A-8BE5-19523D9FDFA2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        public Product[] Products { get; set; }

        #region Allors
        [Id("293A6FED-2EFD-464F-9FCB-5C24E74DCE80")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Workspace(Default)]
        public Product[] AllProducts { get; set; }

        #region Allors
        [Id("43875418-7375-41BD-B6B3-1D091F98AF98")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Workspace(Default)]
        public Part[] AllParts { get; set; }

        #region Allors
        [Id("5889B718-100F-4444-AA5D-3B56FD33AD91")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Workspace(Default)]
        public SerialisedItem[] AllSerialisedItemsForSale { get; set; }

        #region Allors
        [Id("A34B5082-8B4C-41D7-B1C2-DC42D8805BE7")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Workspace(Default)]
        public NonSerialisedInventoryItem[] AllNonSerialisedInventoryItemsForSale { get; set; }

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
