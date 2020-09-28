// <copyright file="EngineeringChangeStatus.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("d149dd80-1cdc-4a29-bb0b-b88823d718bc")]
    #endregion
    public partial class EngineeringChangeStatus : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("0a6c34f7-b37b-4abc-b12e-05ef14a8d986")]
        #endregion
        [Required]

        public DateTime StartDateTime { get; set; }

        #region Allors
        [Id("6a7695dc-4343-4645-b4f1-78348d6873c3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Required]

        public EngineeringChangeObjectState EngineeringChangeObjectState { get; set; }

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
