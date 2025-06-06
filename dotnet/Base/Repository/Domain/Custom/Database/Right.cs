﻿// <copyright file="Right.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region

    [Id("E4BC4E69-831C-4D9B-93D9-531D226819E1")]

    #endregion
    public partial class Right : DerivationCounted
    {
        #region inherited properties
        public int DerivationCount { get; set; }

        #endregion

        #region Allors
        [Id("658FE4F7-FC40-4B3A-ABB1-84723E66F20C")]
        #endregion
        [Required]
        public int Counter { get; set; }

        #region inherited methods

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

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

        #endregion
    }
}
