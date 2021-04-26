// <copyright file="Bank.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;

    [Id("51d4dbfb-98ef-4f38-836a-5948701c4cce")]
    [Synced]
    public partial class ExchangeRate : Object, Deletable
    {
        #region inherited properties

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("d7beea67-7239-4ad8-a31d-c0850ed00b00")]
        #endregion
        [Required]
        [Workspace]
        public DateTime ValidFrom { get; set; }

        #region Allors
        [Id("c73191fb-5814-4d14-8725-65be2ff90f77")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace]
        public Currency FromCurrency { get; set; }

        #region Allors
        [Id("5d5d8bca-891e-4630-ac4d-748bfa323319")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace]
        public Currency ToCurrency { get; set; }

        #region Allors
        [Id("dad55248-a724-4be0-891a-51aec803f2d8")]
        #endregion
        [Required]
        [Workspace]
        [Precision(28)]
        [Scale(10)]
        public decimal Rate { get; set; }

        #region inherited methods

        public void OnBuild()
        {
        }

        public void OnPostBuild()
        {
        }

        public void OnInit()
        {
        }

        public void OnPreDerive()
        {
        }

        public void OnDerive()
        {
        }

        public void OnPostDerive()
        {
        }

        public void Delete()
        {
        }

        #endregion
    }
}
