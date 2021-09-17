// <copyright file="BudgetRevision.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("9b6bf786-1c6c-4c4e-b940-7314d9c4ba71")]
    #endregion
    public partial class BudgetRevision : Object
    {
        #region inherited properties
        public Restriction[] Restrictions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("5124634a-dc8b-477a-8ae2-d4ad577a13bb")]
        #endregion
        [Required]

        public DateTime RevisionDate { get; set; }

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
