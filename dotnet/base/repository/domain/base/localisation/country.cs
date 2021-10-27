// <copyright file="Country.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("c22bf60e-6428-4d10-8194-94f7be396f28")]
    #endregion
    [Plural("Countries")]
    [Workspace(Default)]
    public partial class Country : Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("62009cef-7424-4ec0-8953-e92b3cd6639d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Currency Currency { get; set; }

        #region Allors
        [Id("f93acc4e-f89e-4610-ada9-e58f21c165bc")]
        #endregion
        [Required]
        [Size(2)]
        [Workspace(Default)]
        public string IsoCode { get; set; }

        #region Allors
        [Id("6b9c977f-b394-440e-9781-5d56733b60da")]
        #endregion
        [Indexed]
        [Size(256)]
        [Required]
        [Workspace(Default)]
        public string Name { get; set; }

        #region Allors
        [Id("8236a702-a76d-4bb5-9afd-acacb1508261")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public LocalisedText[] LocalisedNames { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion
    }
}
