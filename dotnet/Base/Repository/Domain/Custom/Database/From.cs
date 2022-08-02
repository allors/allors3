// <copyright file="From.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("6217b428-4ad0-4f7f-ad4b-e334cf0b3ab1")]
    #endregion
    public partial class From : Object
    {
        #region inherited properties
        #endregion

        #region Allors
        [Id("d9a9896d-e175-410a-9916-9261d83aa229")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        #endregion
        public To[] Tos { get; set; }

        #region inherited methods

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion

    }
}
