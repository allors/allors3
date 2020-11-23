// <copyright file="RespondingParty.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("4b1e9776-8851-4a2a-a402-1b40211d1f3b")]
    #endregion
    public partial class RespondingParty : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("13f84c6c-d44a-4cc2-8898-bc2cbaed04f4")]
        #endregion

        public DateTime SendingDate { get; set; }

        #region Allors
        [Id("1d220b47-44de-4ab9-9219-b3acf78bdaf2")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public ContactMechanism ContactMechanism { get; set; }

        #region Allors
        [Id("8e4080f7-40b7-437c-aff2-0fb6b809797a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public Party Party { get; set; }

        #region inherited methods

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
