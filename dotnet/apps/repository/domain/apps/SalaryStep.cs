// <copyright file="SalaryStep.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("6ebf4c66-dd19-494f-8081-67d7a10a16fc")]
    #endregion
    public partial class SalaryStep : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("162b31b7-78fd-4ec5-95f7-3913be0662e2")]
        #endregion
        [Required]

        public DateTime ModifiedDate { get; set; }

        #region Allors
        [Id("7cb593b7-48ac-4049-b78c-1e84bdd2fa3a")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        public decimal Amount { get; set; }

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
