// <copyright file="Bank.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;
    using static Workspaces;

    [Id("51d4dbfb-98ef-4f38-836a-5948701c4cce")]
    public partial class ExchangeRate : Object, Deletable, Searchable
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string SearchString { get; set; }

        #endregion

        #region Allors
        [Id("d7beea67-7239-4ad8-a31d-c0850ed00b00")]
        #endregion
        [Required]
        [Workspace(Default)]
        public DateTime ValidFrom { get; set; }

        #region Allors
        [Id("c73191fb-5814-4d14-8725-65be2ff90f77")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public Currency FromCurrency { get; set; }

        #region Allors
        [Id("5d5d8bca-891e-4630-ac4d-748bfa323319")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public Currency ToCurrency { get; set; }

        #region Allors
        [Id("dad55248-a724-4be0-891a-51aec803f2d8")]
        #endregion
        [Required]
        [Workspace(Default)]
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

        public void OnPostDerive()
        {
        }

        public void Delete()
        {
        }

        #endregion
    }
}
