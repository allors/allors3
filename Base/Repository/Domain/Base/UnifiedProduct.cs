// <copyright file="UnifiedProduct.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;

    #region Allors
    [Id("823D3C34-3441-40CF-8ED8-C44694933DC6")]
    #endregion
    public partial interface UnifiedProduct : Commentable, UniquelyIdentifiable, Deletable, Auditable, Searchable
    {
        #region Allors
        [Id("1A5619BE-43D0-47CF-B906-0A15277B86A6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace]
        ProductIdentification[] ProductIdentifications { get; set; }

        #region Allors
        [Id("8cbbbd41-3eb9-4ce5-a5ce-72660a94e9f5")]
        #endregion
        [Derived]
        [Workspace]
        [Size(-1)]
        string ProductNumber { get; set; }

        #region Allors
        [Id("7423a3e3-3619-4afa-ab67-e605b2a62e02")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace]
        string Name { get; set; }

        #region Allors
        [Id("EBF60298-05C0-4885-81F9-64E0BE4ACE40")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace]
        LocalisedText[] LocalisedNames { get; set; }

        #region Allors
        [Id("0cbb9d37-20cf-4e0c-9099-07f1fcb88590")]
        #endregion
        [Size(-1)]
        [Workspace]
        [MediaType("text/markdown")]
        string Description { get; set; }

        #region Allors
        [Id("C8B9AF4A-9385-4225-A458-77EC3526A7B2")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace]
        LocalisedText[] LocalisedDescriptions { get; set; }

        #region Allors
        [Id("D88189C8-735E-4A5A-B46F-AEFF4F1F0501")]
        #endregion
        [Workspace]
        [Size(-1)]
        string InternalComment { get; set; }

        #region Allors
        [Id("7c41deee-b270-4810-abaa-6d00e6507b9b")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace]
        Document[] Documents { get; set; }

        #region Allors
        [Id("944F8DF7-1E19-46EA-8DFA-031093DD387E")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace]
        Media[] PublicElectronicDocuments { get; set; }

        #region Allors
        [Id("AFBF7196-1EDC-4F3A-845B-E583E6EFA6B8")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace]
        LocalisedMedia[] PublicLocalisedElectronicDocuments { get; set; }

        #region Allors
        [Id("99151780-bcf5-4c39-9264-5ed1126ca1c2")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace]
        Media[] PrivateElectronicDocuments { get; set; }

        #region Allors
        [Id("b7075146-8c4d-418a-a6b7-b2a242f98f53")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace]
        LocalisedMedia[] PrivateLocalisedElectronicDocuments { get; set; }

        #region Allors
        [Id("9b66342e-48ac-4761-b375-b9b60d94b005")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        UnitOfMeasure UnitOfMeasure { get; set; }

        #region Allors
        [Id("38010003-F586-49E5-8A6C-11D490738B9A")]
        #endregion
        [Workspace]
        [Size(-1)]
        string Keywords { get; set; }

        #region Allors
        [Id("990A3EFD-891E-4BB9-8FEE-8DE538E9EC04")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace]
        LocalisedText[] LocalisedKeywords { get; set; }

        #region Allors
        [Id("f52c0b7e-dbc4-4082-a2b9-9b1a05ce7179")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace]
        Media PrimaryPhoto { get; set; }

        #region Allors
        [Id("C7FB85EB-CF47-4FE1-BD67-E2832E5893B9")]
        [Indexed]
        #endregion
        [Workspace]
        [Multiplicity(Multiplicity.ManyToMany)]
        Media[] Photos { get; set; }
    }
}
