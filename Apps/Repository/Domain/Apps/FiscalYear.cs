// <copyright file="AccountingPeriod.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;

    #region Allors
    [Id("dbf992ec-005e-430e-95f8-db05ce7f023c")]
    #endregion
    public partial class FiscalYear : Object
    {
        #region inherited properties

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("c682f15e-15b1-4a31-8b7d-bf29a6cf3a55")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Required]
        public AccountingPeriod[] AccountingPeriods { get; set; }

        #region Allors
        [Id("57eabaf2-e31f-47e1-b453-31d44cc74c93")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        public InternalOrganisation InternalOrganisation { get; set; }  

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
