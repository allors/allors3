// <copyright file="Locale.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("45033ae6-85b5-4ced-87ce-02518e6c27fd")]
    #endregion
    [Workspace(Default)]
    public partial class Locale : Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("2a2c6f77-e6a2-4eab-bfe3-5d35a8abd7f7")]
        [Size(256)]
        #endregion
        [Workspace(Default)]
        public string Name { get; set; }

        #region Allors
        [Id("d8cac34a-9bb2-4190-bd2a-ec0b87e04cf5")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public Language Language { get; set; }

        #region Allors
        [Id("ea778b77-2929-4ab4-ad99-bf2f970401a9")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public Country Country { get; set; }

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
