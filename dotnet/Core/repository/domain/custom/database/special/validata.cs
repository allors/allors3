// <copyright file="Data.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("CC635728-B7AE-4A07-BBF1-E16AEEC07750")]
    #endregion
    [Workspace(Default)]
    public partial class ValiData : Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("C90E7744-9AFD-46A2-9F6F-3D76D681106A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Required]
        public Person RequiredPerson { get; set; }

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
