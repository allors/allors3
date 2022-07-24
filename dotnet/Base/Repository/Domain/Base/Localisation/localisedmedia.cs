// <copyright file="LocalisedMedia.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("2288E1F3-5DC5-458B-9F5E-076F133890C0")]
    #endregion
    public partial class LocalisedMedia : Localised, Deletable
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Locale Locale { get; set; }

        #endregion

        #region Allors
        [Id("B6AE19AE-76BF-4B84-9CBE-176217D94B9E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public Media Media { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Delete() { }
        #endregion
    }
}
