// <copyright file="GlBudgetAllocation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("084829bc-d347-489a-9557-9ff1ac7fb5a0")]
    #endregion
    public partial class GlBudgetAllocation : Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("b09babba-1379-44fe-9e5f-89ec75c65a9c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public GeneralLedgerAccount GeneralLedgerAccount { get; set; }

        #region Allors
        [Id("dddccd24-864c-48bb-b1ac-35b8a201cd65")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public BudgetItem BudgetItem { get; set; }

        #region Allors
        [Id("eb1e7e03-8b88-4a69-b1cc-46dc77b44a8b")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        public decimal AllocationPercentage { get; set; }

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
